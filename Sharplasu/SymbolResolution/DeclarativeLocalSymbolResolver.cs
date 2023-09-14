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


        public override List<Issue> ResolveSimbols(Node root)
        {
            ResolveNode(root, true);
            return Issues;
        }

        public void ResolveNode(Node node, bool children = false)
        {
            node.ReferenceByNameProperties().ToList().ForEach(it => ResolveProperty(it, node));
            if (children)
            {
                node.WalkChildren().ToList().ForEach(it => ResolveNode(node, true));
            }
        }

        public void ResolveProperty(ReferenceByNameProperty property, Node context)
        {
            var p = context.Properties.FirstOrDefault(it => it.Name == property.Name).Value as ReferenceByName<Named>;
            if (p != null)
            {
                p.Referred = GetScope(property, context)?.Resolve(p.Name, property.GetReferredType());
            }
        }

        public Scope GetScope(ReferenceByNameProperty property, Node context)
        {
            return TryGetScopeForProperty(property, context) ?? TryGetScopeForPropertyType(property, context);
        }

        private Scope TryGetScopeForProperty(ReferenceByNameProperty reference, Node context)
        {
            var scope = TryGetScope(PropertyScopeDefinitions[reference], context);
            if (scope != null)
            {
                if (context.Parent == null)
                    return null;
                else
                    return TryGetScopeForProperty(reference, context.Parent);
            }

            return null;
        }

        private Scope TryGetScopeForPropertyType(ReferenceByNameProperty reference, Node context)
        {
            var referenceType = reference.PropertyType.GetGenericArguments()[0];
            var scope = TryGetScope(ClassScopeDefinitions[referenceType], context);
            if (scope != null)
            {
                if (context.Parent == null)
                    return null;
                else
                    return TryGetScopeForPropertyType(reference, context.Parent);
            }

            return null;
        }

        private class ScopeComparer : IComparer<ScopeDefinition>
        {
            public int Compare(ScopeDefinition left, ScopeDefinition right)
            {
                if (left.ContextType.IsAssignableFrom(right.ContextType))
                    return 1;
                else if (right.ContextType.IsAssignableFrom(left.ContextType))
                    return -1;
                else
                    return 0;
            }
        }

        private Scope TryGetScope(List<ScopeDefinition> scopeDefinitions, Node context)
        {
            return scopeDefinitions?.Where(scopeDefinition => scopeDefinition.ContextType.IsAssignableFrom(context.GetType()))
                                   ?.OrderBy(score => score, new ScopeComparer())?.FirstOrDefault()?.ScopeFunction?.Invoke(context);
        }

        public void ScopeFor<ContextType>(Type nodeType, Func<ContextType, Scope> scopeFunction)
            where ContextType : Node
        {
            if (!ClassScopeDefinitions.ContainsKey(nodeType) || 
                (ClassScopeDefinitions.ContainsKey(nodeType) && ClassScopeDefinitions[nodeType] == null))
            {
                ClassScopeDefinitions.Add(nodeType, new List<ScopeDefinition>()
                {
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
                });
            }                
        }

        public void ScopeFor<ContextType>(ReferenceByNameProperty reference, Func<ContextType, Scope> scopeDefinition)
            where ContextType : Node
        {
            if (!PropertyScopeDefinitions.ContainsKey(reference) || 
                (PropertyScopeDefinitions.ContainsKey(reference) && PropertyScopeDefinitions[reference] == null))
            {
                PropertyScopeDefinitions.Add(reference, new List<ScopeDefinition>()
                {
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
                });
            }
        }

        public static DeclarativeLocalSymbolResolver SymbolResolver(Action<DeclarativeLocalSymbolResolver> init, List<Issue> issues = null)
        {
            var decl = new DeclarativeLocalSymbolResolver(issues);
            init(decl);
            return decl;
        }
    }   
}
