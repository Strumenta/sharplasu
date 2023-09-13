using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Tests.Models
{
    public class TopNode : Node
    {
        public int GoodStuff { get; init; }
        public int BadStuff { get; init; }

        public SmallNode? Smaller { get; init; }

    }

    public class SmallNode : Node
    {
        public string Description { get; init; }
    }

    [Serializable]
    public class CompilationUnit : Node
    {
        public List<string> Statements { get; set; } = new List<string>();

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            CompilationUnit o = (obj as CompilationUnit)!;
            return Statements.SequenceEqual(o.Statements);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var str in Statements)
                {
                    hash = hash * 23 + (str?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }

    public class ExampleSharpLasuParser : SharpLasuParser<CompilationUnit, SimpleLangParser, SimpleLangParser.CompilationUnitContext, SharplasuANTLRToken>
    {
        public ExampleSharpLasuParser(TokenFactory<SharplasuANTLRToken> tokenFactory) : base(tokenFactory) { }

        public ExampleSharpLasuParser() : base(new ANTLRTokenFactory()) { }

        protected override Lexer CreateANTLRLexer(ICharStream charStream)
        {
            return new SimpleLangLexer(charStream);
        }

        protected override SimpleLangParser CreateANTLRParser(ITokenStream tokenStream)
        {
            return new SimpleLangParser(tokenStream);
        }

        protected override CompilationUnit ParseTreeToAst(SimpleLangParser.CompilationUnitContext parseTreeRoot, bool considerPosition = true, List<Issue> issues = null, Source source = null)
        {
            CompilationUnit cu = new CompilationUnit();            
            foreach(var s in parseTreeRoot.statement())
            {
                if (s.GetText().ToLower().Contains("display"))
                    issues.Add(new Issue(IssueType.Semantic, "Display statement not supported", s.ToPosition()));
                else
                    cu.Statements.Add(s.GetText());
            }

            return cu;
        }
    }
}
