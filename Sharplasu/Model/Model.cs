using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Traversing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Strumenta.Sharplasu.Model
{
    /**
     * Use this to mark properties that are internal, i.e., they are used for bookkeeping and are not part of the model,
     * so that they will not be considered branches of the AST.
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class InternalAttribute : System.Attribute
    {
        public InternalAttribute() { }
    }

    /**
     * Use this to mark all relations which are secondary, i.e., they are calculated from other relations,
     * so that they will not be considered branches of the AST.
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class DerivedAttribute : System.Attribute
    {
        public DerivedAttribute() { }
    }

    /**
     * Use this to mark all the properties that return a Node or a list of Nodes which are not
     * contained by the Node having the properties. In other words: they are just references.
     * This will prevent them from being considered branches of the AST.
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkAttribute : System.Attribute
    {
        public LinkAttribute() { }
    }

    /**
     * Use this to mark something that does not inherit from Node as a node, so it will be included in the AST.
     */
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class NodeTypeAttribute : System.Attribute
    {
        public NodeTypeAttribute() { }
    }

    public interface Origin
    {
        Position Position { get; set; }
        String SourceText { get; }
        Source Source { get; }           
    }

    [Serializable]
    public class SimpleOrigin : Origin
    {
        public Position Position { get; set; }
        public string SourceText { get; }

        public Source Source => Position?.Source;
    }

    public static class ModelExtensions
    {
        public static N WithOrigin<N> (this N node, Origin origin) where N : Node 
        {
            if (origin == node) 
            {
                node.Origin = null;
            } 
            else 
            {
                node.Origin = origin;
            }            
            return node;
        }

        public static List<PropertyInfo> NodeProperties(this Object obj)
        {
            // GetProperties just returns the public properties           
            return obj.GetType().GetProperties()
                .Where(it => it.GetCustomAttribute(typeof(DerivedAttribute)) == null)
                .Where(it => it.GetCustomAttribute(typeof(InternalAttribute)) == null)
                .Where(it => it.GetCustomAttribute(typeof(LinkAttribute)) == null)
                .Select(x => x)
                .ToList();
        }

        public static List<Type> Superclasses(this Type type)
        {
            // We do not merge these statements into one because AddRange returns void
            // so we would have to do it by initializing superclasses with the Interfaces
            // but that would result in the base types having the wrong order
            var superclasses = new List<Type>() { type.BaseType };
            superclasses.AddRange(type.GetInterfaces());
            return superclasses;
        }

        public static string MultiLineString(this Node node, string indentation = "")
        {
            IEnumerable<string> ignore = typeof(Node).GetProperties()
                .Where(it => it.GetCustomAttribute(typeof(InternalAttribute)) != null
                          || it.GetCustomAttribute(typeof(DerivedAttribute)) != null
                          || it.GetCustomAttribute(typeof(LinkAttribute)) != null)
                .Select(x => x.Name)
                .ToList();
            var sb = new StringBuilder();
            sb.AppendLine($"{indentation}{node.GetType().Name}");

            var properties = node.GetType().GetProperties()
                .Where(x => !ignore.Contains(x.Name)
                            && x.GetValue(node) != null
                            && !typeof(Node).IsAssignableFrom(x.PropertyType)
                            && !typeof(IEnumerable<Node>).IsAssignableFrom(x.PropertyType));
            foreach (PropertyInfo prp in properties)
            {
                object value = prp.GetValue(node);

                sb.AppendLine($"{indentation + "  "}{prp.Name} {prp.GetValue(node)}");
            }

            if (node.Children().Count > 0)
            {
                node.Children().ForEach(c => sb.Append(c.MultiLineString(indentation + "  ")));
            }

            return sb.ToString();
        }
    }
}
