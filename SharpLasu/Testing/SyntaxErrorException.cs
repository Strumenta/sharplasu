using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;

namespace Strumenta.Sharplasu.Testing
{
    public class SyntaxErrorException : Exception
    {
        int line;
        int charPositionInLine;
        string rules;

        public RecognitionException Cause { get; } = null;

        public SyntaxErrorException(string message, int line, int charPositionInLine, string rules = null) : base(message)
        {
            this.line = line;
            this.charPositionInLine = charPositionInLine;
            this.rules = rules;
        }

        public SyntaxErrorException(string message, int line, int charPositionInLine, RecognitionException innerException, string rules = null) : base(message, innerException)
        {
            this.line = line;
            this.charPositionInLine = charPositionInLine;
            this.Cause = innerException;
            this.rules = rules;
        }
    }
}
