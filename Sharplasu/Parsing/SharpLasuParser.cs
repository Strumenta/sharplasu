using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ExtensionMethods;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace Strumenta.Sharplasu.Parsing
{
 
    // in the original Kolasu library this is an interface
    // here is an abstract class because we cannot have default implementations
    // for interfaces on .NET Framework
    public abstract class TokenFactory<T>
        where T : KolasuToken
    {
        public TokenCategory CategoryOf(IToken t) => TokenCategory.PLAIN_TEXT;
        public abstract T ConvertToken(IToken t);
        private T ConvertToken(ITerminalNode terminalNode) => ConvertToken(terminalNode.Symbol);

        public LexingResult<T> ExtractTokens(ParsingResult<Node, ParserRuleContext> result)
        {
            var antlrTerminals = new List<ITerminalNode>();

            void extractTokensFromParseTree(IParseTree pt)
            {
                if(pt is ITerminalNode)
                {
                    antlrTerminals.Add(pt as ITerminalNode);
                }
                else if(pt != null)
                {
                    for(var i = 0; i <= pt.ChildCount; i++)
                    {
                        extractTokensFromParseTree(pt.GetChild(i));
                    }
                }
            }

            var ptRoot = result.FirstStage.Root;

            if(ptRoot != null)
            {
                extractTokensFromParseTree(ptRoot);
                antlrTerminals.Sort((ITerminalNode it, ITerminalNode comparison) => it.Symbol.TokenIndex - comparison.Symbol.TokenIndex);
                var tokens = antlrTerminals.Select(it => ConvertToken(it)).ToList();
                return new LexingResult<T>(result.Issues, tokens, result.Code, result.FirstStage.LexingTime);
            }
            else
                return null;
        }
    }

    public class ANTLRTokenFactory : TokenFactory<SharplasuANTLRToken>
    {
        public override SharplasuANTLRToken ConvertToken(IToken t) => new SharplasuANTLRToken(CategoryOf(t), t);        
    }


    public abstract class KolasuANTLRLexer<T> : KolasuLexer<T>
        where T : KolasuToken
    {
        public TokenFactory<T> TokenFactory { get; private set; }

        public KolasuANTLRLexer(TokenFactory<T> tokenFactory)
        {
            this.TokenFactory = tokenFactory;
        }

        /**
         * Creates the Lexer
         */
        protected Lexer CreateANTLRLexer(Stream inputStream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return CreateANTLRLexer(CharStreams.fromStream(inputStream, encoding));
        }

        /**
         * Creates the Lexer
         */
        protected abstract Lexer CreateANTLRLexer(ICharStream charStream);

        public override LexingResult<T> Lex(Stream inputStream, Encoding encoding, bool onlyFromDefaultChannel = true)
        {
            var issues = new List<Issue>();
            var tokens = new List<T>();
            IToken last = null;
            long time = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var lexer = CreateANTLRLexer(inputStream, encoding);
            AttachListeners(lexer, issues);
            
            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;

            return new LexingResult<T>(issues, tokens, null, time);
        }

        protected void AttachListeners(Lexer lexer, List<Issue> issues)
        {
            lexer.InjectErrorCollectorInLexer(issues);
        }
    }

    public abstract class SharpLasuParser<R, P, C, T> : KolasuANTLRLexer<T>, ASTParser<R, C>
        where R : Node
        where P : Parser
        where C : ParserRuleContext
        where T : KolasuToken
    {
        public SharpLasuParser(TokenFactory<T> tokenFactory) : base(tokenFactory) { }


        /**
         * Creates the first-stage parser.
         */
        protected abstract P CreateANTLRParser(ITokenStream tokenStream);

        /**
         * Invokes the parser's root rule, i.e., the method which is responsible of parsing the entire input.
         * Usually this is the topmost rule, the one with index 0 (as also assumed by other libraries such as antlr4-c3),
         * so this method invokes that rule. If your grammar/parser is structured differently, or if you're using this to
         * parse only a portion of the input or a subset of the language, you have to override this method to invoke the
         * correct entry point.
        */
        protected virtual C InvokeRootRule(P parser)
        {
            var entryPoint = parser.GetType().GetMethod(parser.RuleNames[0]);
            return entryPoint?.Invoke(parser, null) as C;
        }

        /**
        * Transforms a parse tree into an AST (second parsing stage).
        */
        protected abstract R ParseTreeToAst(C parseTreeRoot, bool considerPosition = true, List<Issue> issues = null, Source source = null);

        protected virtual void AttachListeners(P parser, List<Issue> issues)
        {
            parser.InjectErrorCollectorInParser(issues);
        }

        /**
         * Creates the first-stage parser.
         */
        private P CreateParser(ICharStream charStream, List<Issue> issues)
        {
            var lexer = CreateANTLRLexer(charStream);
            AttachListeners(lexer, issues);
            var commonTokenStream = CreateTokenStream(lexer);
            var parser = CreateANTLRParser(commonTokenStream);
            AttachListeners(parser, issues);
            return parser;
        }

        protected virtual CommonTokenStream CreateTokenStream(Lexer lexer) => new CommonTokenStream(lexer);

        /**
         * Checks the parse tree for correctness. If you're concerned about performance, you may want to 
         * override this to do nothing.
         */
        private void VerifyParseTree(P parser, List<Issue> issues, ParserRuleContext root)
        {
            var commonTokenStream = parser.TokenStream as CommonTokenStream;
            var lastToken = commonTokenStream.Get(commonTokenStream.Index);

            if (lastToken.Type != Parser.Eof)
            {
                issues.Add(new Issue(IssueType.Syntatic, "Not whole input consumed", lastToken.EndPoint()?.AsPosition));
            }

            root.ProcessDescendantsAndErrors((ParserRuleContext it) =>
            {
                if (it.exception != null)
                {
                    var ruleName = parser.RuleNames[it.RuleIndex];
                    issues.Add(new Issue(IssueType.Syntatic, $"Recognition exception: {it.exception?.Message} on rule {ruleName}", it.ToPosition()));
                }
            },
            (IErrorNode it) =>
            {
                issues.Add(new Issue(IssueType.Syntatic, $"Error node found (token: {it.Symbol.Text})", it.ToPosition()));
            });
        }

        public FirstStageParsingResult<C> ParseFirstStage(string code, bool measureLexingTime = false)
        {
            return ParseFirstStage(CharStreams.fromString(code), measureLexingTime);
        }

        public FirstStageParsingResult<C> ParseFirstStage(Stream inputStream, Encoding encoding = null, bool measureLexingTime = false)
        {
            return ParseFirstStage(CharStreams.fromStream(inputStream, encoding ?? Encoding.UTF8), measureLexingTime);
        }

        /**
         * Executes only the first stage of the parser, i.e., the production of a parse tree. 
         * Usually, you'll want to use the parse method, that returns an AST which is simpler 
         * to use and query.
         */
        public FirstStageParsingResult<C> ParseFirstStage(ICharStream inputStream, bool measureLexingTime = false)
        {
            var issues = new List<Issue>();
            C root;
            long? lexingTime = null;
            long time = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var parser = CreateParser(inputStream, issues);

            if (measureLexingTime)
            {
                Stopwatch lexingWatch = new Stopwatch();
                var tokenStream = parser.InputStream;
                if (tokenStream is CommonTokenStream)
                {
                    lexingWatch.Start();
                    (tokenStream as CommonTokenStream).Fill();
                    tokenStream.Seek(0);
                    lexingWatch.Stop();
                    lexingTime = lexingWatch.ElapsedMilliseconds;
                }
            }
            root = InvokeRootRule(parser);
            if (root != null)
                VerifyParseTree(parser, issues, root);
            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;

            return new FirstStageParsingResult<C>(
                issues,
                root,
                null,
                null,
                time,
                lexingTime
                );
        }

        public FirstStageParsingResult<C> ParseFirstStage(FileInfo file, Encoding encoding = null, bool measureLexingTime = false)
        {
            return ParseFirstStage(file.OpenRead(), encoding ?? Encoding.UTF8, measureLexingTime);
        }

        protected virtual R PostProcessAst(R ast, List<Issue> issues)
        {
            return ast;
        }

        public ParsingResult<R, C> Parse(string code, bool considerPosition = true, bool measureLexingTime = false, Source source = null)
            => Parse(CharStreams.fromString(code), considerPosition, measureLexingTime, source ?? new StringSource(code));

        private ParsingResult<R, C> Parse(ICharStream inputStream, bool considerPosition = true, bool measureLexingTime = false, Source source = null)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var firstStage = ParseFirstStage(inputStream, measureLexingTime);
            var myIssues = firstStage.Issues;
            R ast = ParseTreeToAst(firstStage.Root, considerPosition, myIssues, source);
            AssignParents(ast);
            if (ast != null)
                ast = PostProcessAst(ast, myIssues);
            if (ast != null && !considerPosition)
            {
                // Remove parseTreeNodes because they cause the position to be computed
                ast.Walk().ToList().ForEach(it => it.Origin = null);
            }

            stopwatch.Stop();
            var end = stopwatch.ElapsedMilliseconds;

            return new ParsingResult<R, C>(
                myIssues, ast, inputStream.GetText(new Antlr4.Runtime.Misc.Interval(0, inputStream.Index + 1)),
                null, firstStage, end
                );
        }

        public ParsingResult<R, C> Parse(Stream inputStream, Encoding encoding = null, bool considerPosition = true, bool measureLexingTime = false, Source source = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return Parse(ASTParserExtensions.InputStreamToString(inputStream, encoding), considerPosition, measureLexingTime, source);
        }

        public ParsingResult<R, C> Parse(FileInfo file, Encoding encoding = null, bool considerPosition = true, bool measureLexingTime = false)
            => Parse(file.OpenRead(), encoding, considerPosition, measureLexingTime, new FileSource(file));

        public void ProcessProperties(
            Node node,
            Action<PropertyDescription> propertyOperation,
            HashSet<string> propertiesToIgnore
            ) => node.ProcessProperties(propertiesToIgnore, propertyOperation);

        /**
         * Traverses the AST to ensure that parent nodes are correctly assigned.
         *
         * If you're already assigning the parents correctly when you build the AST, or you're not interested in tracking
         * child-parent relationships, you can override this method to do nothing to improve performance.
         */
        protected virtual void AssignParents(R ast)
        {
            ast.AssignParents();
        }             

        public static void VerifyASTTree(R root, List<Issue> errors)
        {
            List<Issue> issues = errors;

            root?.ProcessDescendants((Node node) => {
                if (node?.Parent == null)
                    issues.Add(new Issue(IssueType.Semantic, "Node has no parent", node?.Position));
            }, false);
        }        
    }
}
