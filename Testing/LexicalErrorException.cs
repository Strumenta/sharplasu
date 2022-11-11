using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;

namespace Strumenta.Cslasu.Testing
{
    public class LexicalErrorException : Exception
    {
        int line;
        int charPositionInLine;
        RecognitionException recognitionException = null;               

        public LexicalErrorException(string message, int line, int charPositionInLine) : base(message)
        {
            this.line = line;
            this.charPositionInLine = charPositionInLine;
        }

        public LexicalErrorException(string message, int line, int charPositionInLine, RecognitionException innerException) : base(message, innerException)
        {
            this.line = line;
            this.charPositionInLine = charPositionInLine;
            this.recognitionException = innerException;
        }
    }
}
