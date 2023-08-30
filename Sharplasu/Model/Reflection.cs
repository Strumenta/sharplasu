using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Model
{
    public class PropertyTypeDescription
    {
        public string Name { get; set; }
        public bool ProvideNodes { get; set; }
        public bool Multiple { get; set; }
        public Type ValueType { get; set; }

        public PropertyTypeDescription(string name, bool provideNodes, bool multiple, Type valueType)
        {
            Name = name;
            ProvideNodes = provideNodes;
            Multiple = multiple;
            ValueType = valueType;
        }
        
        public static PropertyTypeDescription BuildFor(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var classifier = property.PropertyType;
            var multiple = false;
            if (property.PropertyType.IsACollection())
                multiple = true;
            Type valueType;
            bool provideNodes;
            if (multiple)
            {
                valueType = propertyType.GenericTypeArguments[0].GetType();
                provideNodes = Reflection.ProvidesNodes(propertyType.GenericTypeArguments[0]);
            }
            else
            {
                valueType = propertyType;
                provideNodes = Reflection.ProvidesNodes(classifier);
            }
            return new PropertyTypeDescription(
                property.Name,
                provideNodes,
                multiple,
                valueType
            );
        }
    }

    public static class Reflection
    {
        public static void ProcessProperties(
            this object obj,
            ISet<string> propertiesToIgnore,
            Action<PropertyTypeDescription> propertyTypeOperation
        )
        {
            obj.NodeProperties().ForEach(p =>
            {
                if (!propertiesToIgnore.Contains(p.Name))
                {
                    propertyTypeOperation(PropertyTypeDescription.BuildFor(p));
                }
            }
            );
        }

        internal static bool ProvidesNodes(Type kType)
        {
            return kType.IsANode();
        }

        public static bool IsANode(this Type type)
        {
            return type.IsSubclassOf(typeof(Node)) || type.IsMarkedAsNodeType();
        }

        public static bool IsMarkedAsNodeType(this Type type)
        {
            return type.CustomAttributes.Any(it => it.AttributeType.Name == "NodeTypeAttribute");
        }
        
        public static bool IsAList(this Object obj)
        {
            return obj.GetType().GetInterfaces().Any(
                        i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IList<>)) &&
                        obj.GetType().IsGenericType;
        }

        public static bool IsReferenceByName(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(ReferenceByName<>);
        }

        public static bool IsACollection(this Object obj)
        {
            return obj.GetType().GetInterfaces().Any(
                        i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) &&
                        obj.GetType().IsGenericType;
        }

        public static bool IsACollection(this Type type)
        {
            return type.GetInterfaces().Any(
                        i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) &&
                        type.IsGenericType;
        }
    }
}
