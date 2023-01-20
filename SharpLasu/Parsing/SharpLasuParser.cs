using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.Sharplasu.Parsing
{
    public abstract class SharpLasuParser<R, P, C>
        where R : Node
        where P : Parser
        where C : ParserRuleContext
    {
        public abstract Lexer InstantiateLexer(ICharStream charStream);

        public abstract P InstantiateParser(ITokenStream tokenStream);

        public string GetParseTreeForText(string text, bool considerPosition = true)
        {
            List<Issue> errors = new List<Issue>();
            var parser = CreateParser(CharStreams.fromString(text), errors);            

            var root = InvokeRootRule(parser);

            return root.ToParseTree(parser.Vocabulary).MultiLineString();
        }

        public Result<R> GetTreeForText(string text, bool considerPosition = true)
        {
            return GetTreeForCharStream(CharStreams.fromString(text), considerPosition);
        }

        public Result<R> GetTreeForCharStream(ICharStream charStream, bool considerPosition = true)
        {
            var parsingResult = ParseStartRule(charStream);

            var ast = ParseTreeToAst(parsingResult.Root, considerPosition, parsingResult.Issues);
            VerifyASTTree(ast, parsingResult.Issues);

            return new Result<R>(parsingResult.Issues, ast);
        }

        private Result<C> ParseStartRule(ICharStream charStream)
        {
            List<Issue> errors = new List<Issue>();
            C root = Parse(charStream, errors);

            return new Result<C>(errors, root);
        }

        private C Parse(ICharStream charStream, List<Issue> errors, bool considerPosition = true)
        {
            var parser = CreateParser(charStream, errors);

            var root = InvokeRootRule(parser);

            VerifyParseTree(parser, errors, root);

            return root;
        }

        private P CreateParser(ICharStream charStream, List<Issue> errors)
        {
            var lexer = InstantiateLexer(charStream);
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new IssueErrorListener(errors));

            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = InstantiateParser(commonTokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new IssueErrorListener(errors));

            return parser;
        }

        private void VerifyParseTree(P parser, List<Issue> errors, ParserRuleContext root)
        {
            var commonTokenStream = parser.TokenStream as CommonTokenStream;
            var lastToken = commonTokenStream.Get(commonTokenStream.Index);

            if (lastToken.Type != Parser.Eof)
            {
                errors.Add(new Issue(IssueType.SYNTACTIC, "Not whole input consumed", lastToken.EndPoint()?.AsPosition));
            }

            List<Issue> issues = errors;

            root.ProcessDescendants((ParserRuleContext operation) =>
            {
                if (operation.exception != null)
                {
                    var ruleName = parser.RuleNames[operation.RuleIndex];
                    issues.Add(new Issue(IssueType.SYNTACTIC, $"Recognition exception: {operation.exception?.Message} on rule {ruleName}", operation.Start?.StartPoint()?.AsPosition));
                }
                if (operation is IErrorNode)
                {
                    issues.Add(new Issue(IssueType.SYNTACTIC, "Error node found", operation.ToPosition()));
                }
            });
        }

        public static void VerifyASTTree(R root, List<Issue> errors)
        {
            List<Issue> issues = errors;

            root?.ProcessDescendants((Node node) => {
                if (node?.Parent == null)
                    issues.Add(new Issue(IssueType.SEMANTIC, "Node has no parent", node?.SpecifiedPosition));
            }, false);
        }

        /**
         * Invokes the parser's root rule, i.e., the method which is responsible of parsing the entire input.
         * Usually this is the topmost rule, the one with index 0 (as also assumed by other libraries such as antlr4-c3),
         * so this method invokes that rule. If your grammar/parser is structured differently, or if you're using this to
         * parse only a portion of the input or a subset of the language, you have to override this method to invoke the
         * correct entry point.
        */
        protected C InvokeRootRule(P parser)
        {
            var entryPoint = parser.GetType().GetMethod(parser.RuleNames[0]);
            return entryPoint?.Invoke(parser, null) as C;
        }


        /**
        * Transforms a parse tree into an AST (second parsing stage).
        */
        protected abstract R ParseTreeToAst(C parseTreeRoot, bool considerPosition = true, List<Issue> issues = null);        
    }
}
