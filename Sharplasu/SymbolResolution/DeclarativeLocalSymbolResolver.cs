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
using ExtensionMethods;

namespace Strumenta.Sharplasu.SymbolResolution
{

    public class DeclarativeLocalSymbolResolver : LocalSymbolResolver
    {
        public List<Issue> Issues { get; private set; }
        public ClassScopeDefinitions ClassScopeDefinitions { get; private set; }
        public PropertyScopeDefinitions PropertyScopeDefinitions { get; private set; }

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
            (context.Properties.)
        }
    }
}
