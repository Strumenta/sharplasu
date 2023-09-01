using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Parsing
{
    public class IssueErrorListener : BaseErrorListener, IAntlrErrorListener<int>
    {
        private List<Issue> issues;

        public IssueErrorListener(List<Issue> errors)
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
                    IssueType.Lexical,
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
                    IssueType.Syntatic,
                    msg ?? "Unspecified",
                    new Point(line, charPositionInLine).AsPosition
                )
            );
        }
    }
}
