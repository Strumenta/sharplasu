using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;

namespace Strumenta.Cslasu.Testing
{
    public class BasicLexerErrorListener : IAntlrErrorListener<int>
    {
        protected string origin;
        
        public BasicLexerErrorListener(string origin) : base()
        {
            this.origin = origin;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string message = $"{origin} Error: {offendingSymbol} at {line}, {charPositionInLine}. {msg}, {e}";

            if (e == null)
                throw new LexicalErrorException(message, line, charPositionInLine);
            else
                throw new LexicalErrorException(message, line, charPositionInLine, e);
        }
    }
}
