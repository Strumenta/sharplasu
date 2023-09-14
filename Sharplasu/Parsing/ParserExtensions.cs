using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Traversing;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Strumenta.Sharplasu.Parsing
{
    public static class ParserExtensions
    {
        public static void ProcessDescendants(this ParserRuleContext rule, Action<ParserRuleContext> operation, bool includingMe = true)
        {
            if (includingMe)
            {
                operation(rule);
            }
            if (rule.children != null)
            {
                var children = (from child in rule.children
                                where child is ParserRuleContext
                                select child as ParserRuleContext).ToList();

                children.ForEach(x => x.ProcessDescendants(operation));
            }
        }

        public static void ProcessDescendants(this Node node, Action<Node> operation, bool includingMe = true)
        {
            if (includingMe)
            {
                operation(node);
            }

            foreach (var c in node.Children())
                c.ProcessDescendants(operation);
        }

        public static ParseTreeNode ToParseTree(this ParserRuleContext node, IVocabulary vocabulary)
        {
            var res = new ParseTreeNode(node.GetType().Name.Replace("Context", ""));

            (node.children as List<IParseTree>)?.ForEach(c =>
            {
                switch (c)
                {
                    case ParserRuleContext p:
                        res.Child(p.ToParseTree(vocabulary));
                        break;
                    case ITerminalNode t:
                        res.Child(new ParseTreeLeaf(vocabulary.GetSymbolicName(t.Symbol.Type), t.GetText()));
                        break;
                }
            });
            return res;
        }        
    }
}
