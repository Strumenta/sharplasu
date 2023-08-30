using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Strumenta.Sharplasu.Parsing
{
    public static class ANTLRParseTreeUtils
    {
        /**
         * Navigate the parse tree performing the specified operations on the nodes, either real nodes or nodes
         * representing errors.
         */
        public static void ProcessDescendantsAndErrors(
            this ParserRuleContext parserRule,
            Action<ParserRuleContext> operationOnParserRuleContext,
            Action<ErrorNode> operationOnError,
            bool includingMe = false
        )
        {
            if (includingMe)
            {
                operationOnParserRuleContext(parserRule);
            }
            if(parserRule.children != null)
            {
                parserRule.children.OfType<ParserRuleContext>().ToList().ForEach(it =>
                {
                    it.ProcessDescendantsAndErrors(operationOnParserRuleContext, operationOnError, includingMe = true);
                });
                parserRule.children.OfType<ErrorNode>().ToList().ForEach(it =>
                {
                    operationOnError(it);
                });
            }
        }

        /**
         * Get the original text associated to this non-terminal by querying the inputstream.
         */
        public static string GetOriginalText(this ParserRuleContext parserRule)
        {
            var a = parserRule.Start.StartIndex;
            var b = parserRule.Stop.StopIndex;

            if(a > b)
            {
                throw new InvalidOperationException($"Start index should be less than or equal to the stop index. Start: {a}, Stop: {b}");
            }
            var interval = new Interval(a, b);
            return parserRule.Start.InputStream.GetText(interval);
        }

        /**
         * Get the original text associated to this terminal by querying the inputstream.
        */
        public static string GetOriginalText(this ITerminalNode terminalNode)
        {
            return terminalNode.Symbol.GetOriginalText();
        }

        /**
         * Get the original text associated to this token by querying the inputstream.
         */
        public static string GetOriginalText(this IToken token)
        {
            var a = token.StartIndex;
            var b = token.StopIndex;

            if (a > b)
            {
                throw new InvalidOperationException($"Start index should be less than or equal to the stop index. Start: {a}, Stop: {b}");
            }
            var interval = new Interval(a, b);
            return token.InputStream.GetText(interval);
        }

        /**
         * Given the entire code, this returns the slice covered by this Node.
         */
        public static string GetText(this Node node, string code)
        {           
            return node.Position?.Text(code);
        }

        /**
         * Set the origin of the AST node as a ParseTreeOrigin, providing the parseTree is not null.
         * If the parseTree is null, no operation is performed.
         */
        public static Node WithParseTreeNode(this Node node, ParserRuleContext parseTree, Source source = null)
        {
            if (parseTree != null)
                node.Origin = new ParseTreeOrigin(parseTree, source);

            return node;
        }

        public static bool HasChildren(this RuleContext rule)
        {
            return rule.ChildCount > 0;
        }

        public static IParseTree FirstChild(this RuleContext rule)
        {
            if (rule.HasChildren())
                return rule.GetChild(0);
            else
                return null;
        }

        public static IParseTree LastChild(this RuleContext rule)
        {
            if (rule.HasChildren())
                return rule.GetChild(rule.ChildCount - 1);
            else
                return null;
        }

        public static int Length(this IToken token)
        {
            if (token.Type == TokenConstants.EOF)
                return 0;
            else
                return token.Text.Length;                
        }

        public static Point StartPoint(this IToken token)
        {
            return new Point(token.Line, token.Column);
        }

        public static Point EndPoint(this IToken token)
        {
            return token.StartPoint() + token.Text;
        }        

        public static Position Position(this IToken token)
        {
            return new Position(token.StartPoint(), token.EndPoint());
        }

        /**
         * Returns the position of the receiver parser rule context.
         */
        public static Position Position(this ParserRuleContext rule)
        {
            return new Position(rule.Start.StartPoint(), rule.Stop.EndPoint());
        }

        /**
         * Returns the position of the receiver parser rule context.
         * <param name="considerPosition">if it's false, this method returns null.</param>
         */
        public static Position ToPosition(this ParserRuleContext rule, bool considerPosition = true, Source source = null)
        {
            if(considerPosition && rule.Start != null && rule.Stop != null)
            {
                var position = rule.Position();
                if (source == null)
                    return position;
                else
                    return new Position(position.Start, position.End, source);
            }
            return null;
        }

        public static Position ToPosition(this ITerminalNode node, bool considerPosition = true, Source source = null)
        {
            return node.Symbol.ToPosition(considerPosition, source);
        }

        public static Position ToPosition(this IToken token, bool considerPosition = true, Source source = null)
        {
            return new Position(token.StartPoint(), token.EndPoint());
        }

        public static Position ToPosition(this IParseTree parseTree, bool considerPosition = true, Source source = null)
        {
            switch(parseTree)
            {
                case ITerminalNode terminalNode:
                    return terminalNode.ToPosition(considerPosition, source);
                case ParserRuleContext rule:
                    return rule.ToPosition(considerPosition, source);
                default:
                    return null;
            }
        }

        /**
         * Find the ancestor of the given element with the given class.
         */
        public static T Ancestor<T>(this RuleContext rule)
            where T : RuleContext
        {
            return rule.Ancestor<T>(typeof(T));
        }

        /**
         * Find the ancestor of the given element with the given class.
         */
        public static T Ancestor<T>(this RuleContext rule, Type kclass)
            where T : RuleContext 
        {
            if (rule.Parent == null)
                throw new InvalidOperationException($"Cannot find ancestor of type {kclass}");
            else if (kclass.IsAssignableFrom(rule.Parent.GetType()))
                return rule.Parent as T;
            else
                return rule.Parent.Ancestor<T>(kclass);
        }
    }

    /**
     * An Origin corresponding to a ParseTreeNode. This is used to indicate that an AST Node has been obtained
     * by mapping an original ParseTreeNode.
     *
     * Note that this is NOT serializable as ParseTree elements are not Serializable.
     */
    public class ParseTreeOrigin : Origin
    {
        public IParseTree ParseTree { get; private set; }
        public Source Source { get; set; }
        
        public ParseTreeOrigin(IParseTree parseTree, Source source = null)
        {
            ParseTree = parseTree;
            Source = source;
        }
        
        public Position Position 
        {
            get => ParseTree.ToPosition(source: Source);
            set => throw new NotImplementedException(); 
        }

        public string SourceText
        {
            get
            {
                switch(ParseTree)
                {
                    case ParserRuleContext rule:
                        return rule.GetOriginalText();
                    case ITerminalNode node:
                        return node.GetText();
                    default: 
                        return null;
                }
            }
        }
    }
}
