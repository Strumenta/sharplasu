using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Strumenta.Sharplasu.Model
{
    [Serializable]
    public class Node : Origin
    {
        [field: NonSerialized][JsonIgnore][XmlIgnore]
        public ParserRuleContext ParseTreeNode { get; private set; } = null;

        [field: NonSerialized][JsonIgnore][XmlIgnore]
        public Node Parent { get; set; } = null;
        [JsonIgnore][XmlIgnore]
        public Origin Origin { get; set; } = null;

        private IEnumerable<string> ignore = new string[] { "Parent", "ParseTreeNode", "Children", "Descendants", "Ancestors",
            "DerivedProperties", "NotDerivedProperties" };

        [JsonIgnore][XmlIgnore]
        public IEnumerable<PropertyInfo> DerivedProperties => GetType().GetProperties().Where(p => ignore.Contains(p.Name));

        [JsonIgnore]
        [XmlIgnore]
        public IEnumerable<PropertyInfo> NotDerivedProperties =>
            GetType().GetProperties().Where(p => !ignore.Contains(p.Name));

        public List<Node> Children
        {
            get
            {

                List<Node> properties = GetType().GetProperties().Where(x => typeof(Node).IsAssignableFrom(x.PropertyType) && !ignore.Contains(x.Name) && x.GetValue(this) != null).Select(x => x.GetValue(this) as Node).ToList();

                GetType().GetProperties().Where(x => typeof(IEnumerable<Node>).IsAssignableFrom(x.PropertyType) && !ignore.Contains(x.Name) && x.GetValue(this) != null).Select(x => x.GetValue(this) as IEnumerable<Node>).ToList().ForEach(p => properties.AddRange(p));

                return properties ?? new List<Node>();
            }
        }

        [JsonIgnore][XmlIgnore]
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

        [JsonIgnore][XmlIgnore]
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

        [Internal]
        public Position SpecifiedPosition
        {
            get
            {
                return specifiedPosition ?? ParseTreeNode?.Position();
            }
        }

        [Internal]
        public Position Position
        {
            get
            {
                return SpecifiedPosition ?? Origin?.Position;
            }
            set 
            {
                specifiedPosition = value;
            }
        }

        [Internal]
        public string SourceText 
        { 
            get
            {
                return Origin?.SourceText;
            }            
        }

        [Internal]
        public Source Source => Origin?.Source;

        public Node() : this(null, null, null) {}

        public Node(Origin origin) : this()
        {
            if (origin != null)
            {
                this.Origin = origin;
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

    public class EmptyNode : Node { }  
}
