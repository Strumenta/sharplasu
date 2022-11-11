using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Strumenta.Cslasu.Model;
using Strumenta.Cslasu.Validation;

namespace Strumenta.Cslasu.Parsing
{
    public class IssueErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        private List<Issue> issues;

        public IssueErrorListener(ref List<Issue> errors)
        {
            issues = errors;
        }

        // IAntlrErrorListener is an interface to catch errors from the Lexer
        // This SyntaxError function implements that interface
        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var mode = (recognizer as Lexer).ModeNames[(recognizer as Lexer).CurrentMode];

            issues.Add(
                new Issue(
                    IssueType.LEXICAL,
                    $"[mode {mode}] {msg}" ?? "Unspecified",
                    new Point(line, charPositionInLine).AsPosition
                )
            );
        }

        // BaseErrorListener is a base class to catch errors from the Parser and the Lexer
        // This SyntaxError function override the base class function
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            issues.Add(
                new Issue(
                    IssueType.SYNTACTIC,
                    msg ?? "Unspecified",
                    new Point(line, charPositionInLine).AsPosition
                )
            );
        }
    }
}
