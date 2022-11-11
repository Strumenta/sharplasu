using Antlr4.Runtime;
using Strumenta.Cslasu.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Strumenta.Cslasu.Model
{
    [Serializable]
    public class Node
    {
        [field: NonSerialized]
        public ParserRuleContext ParseTreeNode { get; private set; } = null;

        public Node Parent { get; set; } = null;

        private IEnumerable<string> ignore = new string[] { "Parent", "ParseTreeNode", "Children", "Descendants", "Ancestors" };

        public List<Node> Children
        {
            get
            {                

                List<Node> properties = GetType().GetProperties().Where(x => typeof(Node).IsAssignableFrom(x.PropertyType) && !ignore.Contains(x.Name) && x.GetValue(this) != null).Select(x => x.GetValue(this) as Node).ToList();

                GetType().GetProperties().Where(x => typeof(IEnumerable<Node>).IsAssignableFrom(x.PropertyType) && !ignore.Contains(x.Name) && x.GetValue(this) != null).Select(x => x.GetValue(this) as IEnumerable<Node>).ToList().ForEach(p => properties.AddRange(p));

                return properties ?? new List<Node>();
            }
        }

        public List<Node> Descendants
        {
            get
            {
                List<Node> descendants = new List<Node>();

                Children.ForEach(x =>
                {
                    descendants.Add(x);
                    descendants.AddRange(x.Descendants);
                });

                return descendants;
            }
        }

        public List<T> DescendantsByType<T>() where T : Node
        {
            return Descendants.Where(x => typeof(T).IsAssignableFrom(x.GetType())).Select(x => x as T).ToList();
        }

        public List<Node> Ancestors
        {
            get
            {
                List<Node> ancestors = new List<Node>();

                var p = Parent;

                while (p != null)
                {
                    ancestors.Add(p);
                    p = p.Parent;
                }

                return ancestors;
            }
        }

        public List<T> AncestorsByType<T>() where T : Node
        {
            return Ancestors.Where(x => typeof(T).IsAssignableFrom(x.GetType())).Select(x => x as T).ToList();
        }

        protected Position specifiedPosition = null;

        public Position SpecifiedPosition
        {
            get
            {
                return specifiedPosition ?? ParseTreeNode?.Position();
            }
        }

        public Node(Position specifiedPosition = null, Node parent = null, ParserRuleContext ruleContext = null)
        {
            ParseTreeNode = ruleContext;
            Parent = parent;
            this.specifiedPosition = specifiedPosition;
        }

        public string MultiLineString(string indentation = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{indentation}{GetType().Name}");

            var properties = this.GetType().GetProperties()
                .Where(x => !ignore.Contains(x.Name)
                            && x.GetValue(this) != null
                            && !typeof(Node).IsAssignableFrom(x.PropertyType)
                            && !typeof(IEnumerable<Node>).IsAssignableFrom(x.PropertyType));
            foreach (PropertyInfo prp in properties)
            {
                object value = prp.GetValue(this);

                sb.AppendLine($"{indentation + "  "}{prp.Name} {prp.GetValue(this)}");
            }

            if (Children.Count > 0)
            {
                Children.ForEach(c => sb.Append(c.MultiLineString(indentation + "  ")));
            }

            return sb.ToString();
        }
    }
}
