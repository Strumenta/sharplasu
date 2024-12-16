using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Strumenta.Sharplasu.Model
{
    [Serializable]
    public class Node : Origin
    {
        [field: NonSerialized][JsonIgnore][XmlIgnore]
        [Internal]
        public Node Parent { get; set; } = null;
        
        [JsonIgnore]
        [XmlIgnore]
        [Internal]
        public Origin Origin { get; set; } = null;
        
        [JsonIgnore]
        [XmlIgnore]
        [Internal]
        public IDestination Destination { get; set; } = null;

        private IEnumerable<string> ignore = typeof(Node).GetProperties()
                .Where(it => it.GetCustomAttribute(typeof(InternalAttribute)) != null
                          || it.GetCustomAttribute(typeof(DerivedAttribute)) != null
                          || it.GetCustomAttribute(typeof(LinkAttribute)) != null)
                .Select(x => x.Name)
                .ToList();

        [JsonIgnore][XmlIgnore]
        [Internal]
        public IEnumerable<PropertyInfo> DerivedProperties => GetType().GetProperties().Where(p => ignore.Contains(p.Name));

        [JsonIgnore]
        [XmlIgnore]
        [Internal]
        public IEnumerable<PropertyInfo> NotDerivedProperties =>
            GetType().GetProperties().Where(p => !ignore.Contains(p.Name));    

        protected Position PositionOverride = null;

        [Internal]
        public Position Position
        {
            get
            {
                return PositionOverride ?? Origin?.Position;
            }
            set 
            {
                PositionOverride = value;
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

        public Node() {}

        public Node(Origin origin)
        {
            if (origin != null)
            {
                this.Origin = origin;
            }
        }

        public Node(Position position)
        {
            Position = position;
        }

        [Internal]
        public string NodeType
        {
            get => this.GetType().FullName;            
        }

        [Internal]
        public string SimpleNodeType
        {
            get => NodeType.Split('.').Last();
        }

        [Internal]
        [JsonIgnore]
        [XmlIgnore]
        public List<PropertyDescription> Properties
        {
            get
            {
                try
                { 
                    return this.NodeProperties().Select(it => PropertyDescription.BuildFor(it, this)).ToList();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Issue while getting properties of node {this.GetType().FullName}", ex);
                }
            }
        }        

        public Node WithPosition(Position position)
        {
            Position = position;

            return this;
        }
    }

    public class EmptyNode : Node { }  
}
