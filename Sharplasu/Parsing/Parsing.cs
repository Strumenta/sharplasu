using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Parsing
{
    [Serializable]
    public class CodeProcessingResult<D>
    {
        public List<Issue> Issues { get; set; }
        public D Data { get; }
        public string Code { get; } = null;

        public CodeProcessingResult(List<Issue> issues, D data, string code)
        {
            Issues = issues;
            Data = data;
            Code = code;
        }

        public bool Correct
        {
            get
            {
                return Issues.All(issue => issue.IssueSeverity == IssueSeverity.Info);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            var o = obj as CodeProcessingResult<D>;
            return Issues.SequenceEqual(o.Issues) &&
                   Data.Equals(o.Data) &&
                   Code.Equals(o.Code);
        }

        public override int GetHashCode()
        {
            var result = Issues.GetHashCode();
            if (Data != null)
                result = 31 * result + Data.GetHashCode();
            if (Code != null)
                result = 31 * result + Code.GetHashCode();
            return result;
        }
    }

    public class TokenCategory
    {
        public static TokenCategory COMMENT = new TokenCategory("Comment");
        public static TokenCategory KEYWORD = new TokenCategory("Keyword");
        public static TokenCategory NUMERIC_LITERAL = new TokenCategory("Numeric literal");
        public static TokenCategory STRING_LITERAL = new TokenCategory("String literal");
        public static TokenCategory PLAIN_TEXT = new TokenCategory("Plain text");

        public string Type { get; private set; }

        public TokenCategory(string type)
        {
            Type = type;
        }
    }

    /**
     * A token is a portion of text that has been assigned a category.
     */
    [Serializable]
    public class KolasuToken
    {
        public TokenCategory Category { get; protected set; }
        public Position Position { get; protected set; }
        public string Text { get; protected set; }

        public KolasuToken(TokenCategory category, Position position, string text)
        {
            Category = category;
            Position = position;
            Text = text;
        }
    }

    /**
     * A [KolasuToken] generated from a [Token]. The [token] contains additional information that is specific to ANTLR,
     * such as type and channel.
     */
    public class SharplasuANTLRToken : KolasuToken
    {
        public IToken Token { get; private set; }

        public SharplasuANTLRToken(TokenCategory category, IToken token)
            : base(category, token.Position(), token.Text) { }
    }

    /**
     * The result of lexing (tokenizing) a stream.
     */
    public class LexingResult<T> : CodeProcessingResult<List<T>>
        where T : KolasuToken
    {
        public List<T> Tokens { get; private set; }
        public long? Time { get; private set; }
        public LexingResult(List<Issue> issues, List<T> tokens, string code = null, long? time = null) : base(issues, tokens, code)
        {
            Tokens = tokens;
            Time = time;
        }

        public override bool Equals(object obj)
        {
            return obj is LexingResult<T> result &&
                   base.Equals(obj) &&
                   EqualityComparer<List<T>>.Default.Equals(Tokens, result.Tokens);
        }

        public override int GetHashCode()
        {
            int hashCode = 1748046089;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<T>>.Default.GetHashCode(Tokens);
            return hashCode;
        }
    }

    /**
     * The result of first-stage parsing: from source code to a parse tree.
     */
    public class FirstStageParsingResult<C> : CodeProcessingResult<C> where C : ParserRuleContext
    {
        public C Root { get; }        
        public Node IncompleteNode { get; }
        public long? Time { get; private set; }
        public long? LexingTime { get; private set; }

        public FirstStageParsingResult(
            List<Issue> issues,
            C root,
            string code = null,
            Node incompleteNode = null,
            long? time = null,
            long? lexingTime = null
            ) : base(issues, root, code)
        {
            Root = root;
            IncompleteNode = incompleteNode;
            Time = time;
            LexingTime = lexingTime;            
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var o = (FirstStageParsingResult<C>) obj;
            return base.Equals(o) &&
                   ((Root == null && o.Root == null) || (Root != null && Root.Equals(o.Root))) &&
                   ((IncompleteNode == null && o.IncompleteNode == null) || (IncompleteNode != null && IncompleteNode.Equals(o.IncompleteNode)));
        }

        public override int GetHashCode()
        {
            int hashCode = -779149088;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<C>.Default.GetHashCode(Root);
            hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode(IncompleteNode);
            return hashCode;
        }
    }

    /**
     * The complete result of parsing a piece of source code into an AST.     
     * <param name="RootNode">RootNode - the type of the root AST node.</param>
     * <param name="Issues">issues - a list of issues encountered while processing the code.</param>
     * <param name="Root">root - the resulting AST.</param>
     * <param name="Code">code - the processed source code.</param>
     * <param name="FirstStage">firstStage - the result of the first parsing stage (from source code to parse tree).</param>
     * <param name="Time">time - the time spent in the entire parsing process.</param>
    */
    [Serializable]
    public class ParsingResult<RootNode, C> : CodeProcessingResult<RootNode>
        where RootNode : Node
        where C : ParserRuleContext
    {
        public RootNode Root { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public FirstStageParsingResult<ParserRuleContext> FirstStage { get; private set; }
        [XmlIgnore]
        [JsonIgnore]
        public Node IncompleteNode { get; private set; }
        [XmlIgnore]
        [JsonIgnore]
        public long? Time { get; private set; }       

        public ParsingResult() : base(new List<Issue>(), null, null) { }

        public ParsingResult(
            List<Issue> issues,
            RootNode root,
            string code = null,
            Node incompleteNode = null,
            FirstStageParsingResult<C> firstStage = null,
            long? Time = null
            )
            : base(issues, root, code)
        {
            Root = root;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (!base.Equals(obj))
                return false;

            ParsingResult<RootNode, C> o = obj as ParsingResult<RootNode, C>;
            return (
                (IncompleteNode != null && o.IncompleteNode != null && IncompleteNode.Equals(o.IncompleteNode)) ||
                (IncompleteNode == null && o.IncompleteNode == null)
            ) && (
                (Root != null && o.Root != null && Root.Equals(o.Root)) ||
                (Root == null && o.Root == null)
            );
        }

        public override int GetHashCode()
        {
            int hashCode = -779149088;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<RootNode>.Default.GetHashCode(Root);
            hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode(IncompleteNode);
            return hashCode;
        }

        public Result<RootNode> ToResult()
            => new Result<RootNode>(Issues, Root);
    }

    // in the original Kolasu library this is an interface
    // here is an abstract class because we cannot have default implementations
    // for interfaces on .NET Framework
    [Serializable]
    public abstract class KolasuLexer<T>
        where T : KolasuToken
    {
        /*
         * Performs "lexing" on the given code string, i.e., it breaks it into tokens.
         */
    public LexingResult<T> Lex(string code, bool onlyFromDefaultChannel = true)
        => Lex(new MemoryStream(Encoding.UTF8.GetBytes(code)), Encoding.UTF8, onlyFromDefaultChannel);

        /*
         * Performs "lexing" on the given code string, i.e., it breaks it into tokens.
         */
        public LexingResult<T> Lex(string code) => Lex(code, true);

        /*
         * Performs "lexing" on the given code string, i.e., it breaks it into tokens.
         */
        abstract public LexingResult<T> Lex(Stream inputStream, Encoding encoding, bool onlyFromDefaultChannel = true);

        /*
         * Performs "lexing" on the given code string, i.e., it breaks it into tokens.
         */
        public LexingResult<T> Lex(Stream inputStream) => Lex(inputStream, Encoding.UTF8, true);

        /*
         * Performs "lexing" on the given code string, i.e., it breaks it into tokens.
         */
        public LexingResult<T> Lex(FileInfo file)
        {            
            using (var buffered = new BufferedStream(file.OpenRead()))
            {
                return Lex(buffered);
            }
        }
    }

    internal class AnonymousLexerErrorListener : IAntlrErrorListener<int>
    { 
        private List<Issue> Issues;

        public AnonymousLexerErrorListener(List<Issue> issues)
        {
            Issues = issues;
        }
        
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Issues.Add(
                new Issue(
                    IssueType.Lexical,
                    !String.IsNullOrEmpty(msg) ? msg : "unspecified",
                    new Point(line, charPositionInLine).AsPosition
                    )
            );
        }
    }

    internal class AnonymousParserErrorListener : IAntlrErrorListener<IToken>
    {
        private List<Issue> Issues;

        public AnonymousParserErrorListener(List<Issue> issues)
        {
            Issues = issues;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            Issues.Add(
                new Issue(
                    IssueType.Syntatic,
                    !String.IsNullOrEmpty(msg) ? msg : "unspecified",
                    new Point(line, charPositionInLine).AsPosition
                    )
            );
        }        
    }

    public static class ParsingExtensions
    {
        public static void InjectErrorCollectorInLexer(this Lexer lexer, List<Issue> issues)
        {
            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new AnonymousLexerErrorListener(issues));
        }

        public static void InjectErrorCollectorInParser(this Parser parser, List<Issue> issues)
        {
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new AnonymousParserErrorListener(issues));
        }

        public static Stream ToStream(this string text, Encoding encoding)
            => new MemoryStream(encoding.GetBytes(text));
    }
}