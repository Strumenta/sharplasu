using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Strumenta.Sharplasu.SymbolResolution
{
    using SymbolTable = Dictionary<string, List<Named>>;
    using ClassScopeDefinitions = Dictionary<Type, List<ScopeDefinition>>;
    using ReferenceByNameProperty = PropertyInfo;
    using PropertyScopeDefinitions = Dictionary<PropertyInfo, List<ScopeDefinition>>;

    public class Scope
    {
        public Scope Parent { get; set; }
        public SymbolTable SymbolTable { get; set; }
        public bool IgnoreCase { get; set; }
        public Scope(Scope parent = null, SymbolTable symbolTable = null, bool ignoreCase = false) 
        { 
            Parent = parent ?? null;
            SymbolTable = symbolTable ?? new SymbolTable();
            IgnoreCase = ignoreCase;
        }

        public void Define(Named symbol)
        {
            List<Named> symbols = null;
            if (!SymbolTable.ContainsKey(ToSymbolTableKey(symbol.Name)) || 
                (SymbolTable.ContainsKey(ToSymbolTableKey(symbol.Name)) && SymbolTable[ToSymbolTableKey(symbol.Name)] == null))
            {
                symbols = new List<Named>();
            }

            if (!SymbolTable.ContainsKey(ToSymbolTableKey(symbol.Name)))
            {
                SymbolTable.Add(ToSymbolTableKey(symbol.Name), symbols);
            }
            else if(!(SymbolTable.ContainsKey(ToSymbolTableKey(symbol.Name)) && SymbolTable[ToSymbolTableKey(symbol.Name)] == null))
            {
                symbols = SymbolTable[ToSymbolTableKey(symbol.Name)];
            }

            symbols.Add(symbol);

            SymbolTable[ToSymbolTableKey(symbol.Name)] = symbols;
        }

        public Named Resolve(string name, Type type = null)
        {
            type = type ?? typeof(Named);
            var key = ToSymbolTableKey(name);
            List<Named> named;
            SymbolTable.TryGetValue(key, out named);
            return named?.FirstOrDefault(it => type.IsAssignableFrom(it.GetType()))
                    ?? Parent?.Resolve(key, type);
        }

        private string ToSymbolTableKey(string Text)
        {
            switch (Text)
            {
                case string t when t != null && this.IgnoreCase:
                    return t.ToLower();
                case string t when t != null:
                    return t;
                default:
                    throw new ArgumentException("The given symbol must have a name");
            }
        }
    }

    public class ScopeDefinition
    {
        public Type ContextType { get; private set; }
        public Func<Node, Scope> ScopeFunction { get; private set; }

        public ScopeDefinition(Type contextType, Func<Node, Scope> scopeFunction)
        {
            ContextType = contextType;
            ScopeFunction = scopeFunction;
        }
    }

    public static class ScopingExtensions
    {
        public static Type GetReferredType(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.GenericTypeArguments[0];
        }

        public static IEnumerable<ReferenceByNameProperty> ReferenceByNameProperties(this Node node)
        {
            return node.NodeProperties().Where(it => it.PropertyType.IsReferenceByName()).Select(it => it as ReferenceByNameProperty).ToList();
        }
    }
}
