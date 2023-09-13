using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Transformation;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strumenta.Sharplasu.Mapping
{
    /**
     * Implements a transformation from an ANTLR parse tree (the output of the parser) 
     * to an AST (a higher-level representation of the source code).
     */
    public class ParseTreeToASTTransformer : ASTTransformer
    {
        public Source Source { get; private set; }

        public ParseTreeToASTTransformer(
            List<Issue> issues = null,
            bool allowGenericNode = true, 
            Source source = null
            )
            : base(issues, allowGenericNode)
        {
            Source = source;
        }

        /**
         * Performs the transformation of a node and, recursively, its descendants. 
         * In addition to the overridden method, it also assigns the parseTreeNode 
         * to the AST node so that it can keep track of its position. However, a node 
         * factory can override the parseTreeNode of the nodes it creates (but not the parent).
         */
        public override List<Node> TransformIntoNodes(object source, Node parent = null)
        {
            var transformed = base.TransformIntoNodes(source, parent);
            return transformed.SelectMany(node =>
            {
                if (source is ParserRuleContext)
                {
                    if(node.Origin == null)
                    {
                        node.WithParseTreeNode(source as ParserRuleContext, this.Source);
                    } else if(node.Position != null && node.Source == null) 
                    {
                        node.Position.Source = this.Source;
                    }
                }
                return new List<Node>() { node };
            }).ToList();
        }

        protected override object GetSource(Node node, object source)
        {
            var origin = node.Origin;
            if (origin is ParseTreeOrigin)
                return (origin as ParseTreeOrigin).ParseTree;
            else
                return source;            
        }

        protected override Origin AsOrigin(object source)
        {
            if (source is IParseTree)
                return new ParseTreeOrigin((IParseTree)source);
            else
                return null;
        }

        /**
         * Often in ANTLR grammar we have rules which wraps other rules and act as wrapper.
         * When there is only a ParserRuleContext child we can transform that child 
         * and return that result.
         */
        public NodeFactory RegisterNodeFactoryUnwrappingChild<P>(Type kclass)
            where P : ParserRuleContext
        {
            return RegisterNodeFactory<Node>(kclass, (source, transformer, _) => {
                var nodeChildren = (source as ParserRuleContext).children.OfType<ParserRuleContext>();
                Asserts.Require(nodeChildren.Count() == 1, () =>
                    $"Node {source} ({source.GetType().Name}) has ${nodeChildren.Count()} " +
                    $"node children: {nodeChildren}"
                );
                return transformer.Transform(nodeChildren.ElementAt(0)) as Node;
            });            
        }

        /**
         * Alternative to registerNodeFactoryUnwrappingChild(KClass) which is slightly more concise.
         */
        public NodeFactory RegisterNodeFactoryUnwrappingChild<P>()
            where P : ParserRuleContext
        {
            return RegisterNodeFactoryUnwrappingChild<P>(typeof(P));
        }
    }
}
