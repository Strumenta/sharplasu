using System;
using System.Collections;
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

    public enum Multiplicity
    {
        Optional,
        Singular,
        Many
    }

    public enum PropertyType
    {
        Attribute,
        Containment,
        Reference
    }

    public class PropertyDescription
    {
        public string Name { get; private set; }
        public bool CanProvideNodes { get; private set; }
        public Multiplicity WhichMultiplicity { get; private set; }
        public object Value { get; private set; }
        public PropertyType PropertyType { get; private set; }

        public PropertyDescription(
            string name, bool provideNodes, Multiplicity multiplicity, object value, PropertyType propertyType
            )
        {
            Name = name;
            CanProvideNodes = provideNodes;
            WhichMultiplicity = multiplicity;
            Value = value;
            PropertyType = propertyType;
        }

        public string ValueToString()
        {
            if (Value == null)
            {
                return "null";
            }
            if (CanProvideNodes)
            {
                if (WhichMultiplicity == Model.Multiplicity.Many)
                {
                    return $"[{(Value as IEnumerable<Node>).Select(it => it.NodeType).Aggregate((acc, val) => acc += "," + val)}]";
                }
                else
                {
                    return $"{(Value as Node).NodeType}(...)";
                }
            }
            else
            {
                if (WhichMultiplicity == Model.Multiplicity.Many)
                {
                    string result = "";
                    foreach(var item in Value as IEnumerable)
                    {
                        result += item.ToString() + ",";
                    }
                    return $"[{result}]";
                }
                else
                {
                    return Value.ToString();
                }
            }
        }

        public bool IsMultiple 
        {
            get => WhichMultiplicity == Model.Multiplicity.Many;        
        }

        public static bool Multiple(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var classifier = property.PropertyType;
            var multiple = false;
            if (property.PropertyType.IsACollection())
                multiple = true;
            return multiple;
        }

        public static bool Optional(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            // all classes can be null in C#
            return !Multiple(property) && propertyType.IsClass;
        }

        public static Multiplicity Multiplicity(PropertyInfo property)
        {
            if (Multiple(property))
                return Model.Multiplicity.Many;
            else if (Optional(property))
                return Model.Multiplicity.Optional;
            else
                return Model.Multiplicity.Singular;
        }

        public static bool ProvideNodes(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            var classifier = property.PropertyType;
            if (Multiple(property))
                return Reflection.ProvidesNodes(propertyType.GenericTypeArguments[0]);
            else
                return Reflection.ProvidesNodes(classifier);            
        }

        public static PropertyDescription BuildFor(PropertyInfo property, Node node)
        {
            var multiplicity = Multiplicity(property);
            var provideNodes = ProvideNodes(property);
            var propertyType = PropertyType.Attribute;
            if (property.IsReference())
                propertyType = PropertyType.Reference;
            else if (provideNodes)
                propertyType = PropertyType.Containment;
            return new PropertyDescription(
                property.Name,
                provideNodes,
                multiplicity,
                property.GetValue(node),
                propertyType
            );
        }
    }

    public static class Reflection
    {
        /**
         * Executes an operation on the properties definitions of a node class.
         * <param name="propertiesToIgnore">which properties to ignore</param>
         * <param name="propertyTypeOperation"> the operation to perform on each property.</param>
         */
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

        /**
         * Executes an operation on the properties of a node.
         * <param name="propertiesToIgnore">which properties to ignore</param>
         * <param name="propertyOperation"> the operation to perform on each property.</param>
         */
        public static void ProcessProperties(
            this Node node,
            ISet<string> propertiesToIgnore,
            Action<PropertyDescription> propertyOperation
        )
        {
            node.Properties.ForEach(it =>
            {
                try
                {
                    if (!propertiesToIgnore.Contains(it.Name))
                    {
                        propertyOperation(it);
                    }
                }
                catch (Exception t) 
                {
                    throw new Exception($"Issue processing property {it} in {node}", t);
                }
            });
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

        public static bool IsReference(this PropertyInfo propertyInfo)
        {
            // Check that this works
            return propertyInfo.PropertyType.Name.Equals("ReferenceByName`1");
        }
    }
}
