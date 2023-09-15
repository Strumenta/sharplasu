using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SymbolTable = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Strumenta.Sharplasu.Model.Named>>;
using ClassScopeDefinitions = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<Strumenta.Sharplasu.SymbolResolution.ScopeDefinition>>;
using ReferenceByNameProperty = System.Reflection.PropertyInfo;
using PropertyScopeDefinitions = System.Collections.Generic.Dictionary<System.Reflection.PropertyInfo, System.Collections.Generic.List<Strumenta.Sharplasu.SymbolResolution.ScopeDefinition>>;
using Strumenta.Sharplasu.Traversing;
using System.Runtime.InteropServices;

namespace Strumenta.Sharplasu.SymbolResolution
{
    public class DeclarativeLocalSymbolResolver : LocalSymbolResolver
    {
        public List<Issue> Issues { get; private set; }
        public ClassScopeDefinitions ClassScopeDefinitions { get; private set; } = new ClassScopeDefinitions(); 
        public PropertyScopeDefinitions PropertyScopeDefinitions { get; private set; } = new PropertyScopeDefinitions();

        public DeclarativeLocalSymbolResolver(List<Issue> issues = null)
        {
            Issues = issues ?? new List<Issue>();
        }


        public override List<Issue> ResolveSymbols(Node root)
        {
            ResolveNode(root, true);
            return Issues;
        }

        public void ResolveNode(Node node, bool children = false)
        {
            node.ReferenceByNameProperties().ToList().ForEach(it => ResolveProperty(it, node));
            if (children)
            {                
                node.WalkChildren().ToList().ForEach(it => ResolveNode(it, true));
            }
        }

        public void ResolveProperty(ReferenceByNameProperty property, Node context)
        {
            var p = context.Properties.FirstOrDefault(it => it.Name == property.Name).Value;            
            if (p != null)
            {
                p.GetType().GetProperty("Referred").SetMethod.Invoke(p, new object[] { GetScope(property, context)?.Resolve(p.GetType().GetProperty("Name").GetValue(p) as string, property.GetReferredType()) });                
            }
        }

        public Scope GetScope(ReferenceByNameProperty property, Node context)
        {        
            return TryGetScopeForProperty(property, context) ?? TryGetScopeForPropertyType(property, context);
        }

        private Scope TryGetScopeForProperty(ReferenceByNameProperty reference, Node context)
        {
            Scope scope = null;
            if (PropertyScopeDefinitions.ContainsKey(reference))
                scope = TryGetScope(PropertyScopeDefinitions[reference], context);            
            if (scope == null)
            {
                if (context.Parent == null)
                    return null;
                else
                    return TryGetScopeForProperty(reference, context.Parent);
            }

            return scope;
        }

        private Scope TryGetScopeForPropertyType(ReferenceByNameProperty reference, Node context)
        {
            var referenceType = reference.PropertyType.GetGenericArguments()[0];
            Scope scope = null;
            if(ClassScopeDefinitions.ContainsKey(referenceType))
                scope = TryGetScope(ClassScopeDefinitions[referenceType], context);
            if (scope == null)
            {
                if (context.Parent == null)
                    return null;
                else
                    return TryGetScopeForPropertyType(reference, context.Parent);
            }

            return scope;
        }

        private class ScopeComparer : IComparer<ScopeDefinition>
        {
            public int Compare(ScopeDefinition left, ScopeDefinition right)
            {
                if (left.ContextType.IsSuperclassOf(right.ContextType))
                    return 1;
                else if (right.ContextType.IsSuperclassOf(left.ContextType))
                    return -1;
                else
                    return 0;
            }
        }

        private Scope TryGetScope(List<ScopeDefinition> scopeDefinitions, Node context)
        {
            return scopeDefinitions?.Where(scopeDefinition => scopeDefinition.ContextType.IsSuperclassOf(context.GetType()))
                                   ?.OrderBy(score => score, new ScopeComparer())?.FirstOrDefault()?.ScopeFunction?.Invoke(context);
        }

        public void ScopeFor<ContextType>(Type nodeType, Func<ContextType, Scope> scopeFunction)
            where ContextType : Node
        {
            List<ScopeDefinition> scopeDefinitions = null;
            if (!ClassScopeDefinitions.ContainsKey(nodeType) ||
                (ClassScopeDefinitions.ContainsKey(nodeType) && ClassScopeDefinitions[nodeType] == null))
            {
                scopeDefinitions = new List<ScopeDefinition>();
            }
            if (!ClassScopeDefinitions.ContainsKey(nodeType))
            {
                ClassScopeDefinitions.Add(nodeType, scopeDefinitions);
            }
            else if (!(ClassScopeDefinitions.ContainsKey(nodeType) && ClassScopeDefinitions[nodeType] == null))
            {
                scopeDefinitions = ClassScopeDefinitions[nodeType];
            }

            scopeDefinitions.Add(
                    new ScopeDefinition(
                            typeof(ContextType),
                            (Node context) =>
                            {
                                if (context is ContextType)
                                    return scopeFunction(context as ContextType);
                                else
                                    return null;
                            }
                        )
                );

            ClassScopeDefinitions[nodeType] = scopeDefinitions;                   
        }

        public void ScopeFor<ContextType>(ReferenceByNameProperty reference, Func<ContextType, Scope> scopeDefinition)
            where ContextType : Node
        {
            List<ScopeDefinition> scopeDefinitions = null;            
            if (!PropertyScopeDefinitions.ContainsKey(reference) || 
                (PropertyScopeDefinitions.ContainsKey(reference) && PropertyScopeDefinitions[reference] == null))
            {
                scopeDefinitions = new List<ScopeDefinition>();                
            }
            if (!PropertyScopeDefinitions.ContainsKey(reference))
            {
                PropertyScopeDefinitions.Add(reference, scopeDefinitions);
            }
            else if (!(PropertyScopeDefinitions.ContainsKey(reference) && PropertyScopeDefinitions[reference] == null))
            {
                scopeDefinitions = PropertyScopeDefinitions[reference];
            }

            scopeDefinitions.Add(                
                    new ScopeDefinition(
                            typeof(ContextType),
                            (Node context) =>
                            {
                                if (context is ContextType)
                                    return scopeDefinition(context as ContextType);
                                else
                                    return null;
                            }
                        )
                );

            PropertyScopeDefinitions[reference] = scopeDefinitions;
        }

        public static DeclarativeLocalSymbolResolver SymbolResolver(Action<DeclarativeLocalSymbolResolver> init, List<Issue> issues = null)
        {
            var decl = new DeclarativeLocalSymbolResolver(issues);
            init(decl);
            return decl;
        }
    }   
}
