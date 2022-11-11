using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;

namespace Strumenta.Sharplasu.Testing
{
    public class BasicParserErrorListener : BaseErrorListener
    {        
        protected string origin;

        public BasicParserErrorListener(string origin) : base()
        {
            this.origin = origin;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            string message = $"{origin} Error: {offendingSymbol} at {line}, {charPositionInLine}. {msg}, {e}";

            if (e == null)
            {
                if (recognizer.GetType() == typeof(Parser))
                {
                    throw new SyntaxErrorException(message, line, charPositionInLine, (recognizer as Parser).GetRuleInvocationStackAsString());                    
                }                  
                else
                    throw new SyntaxErrorException(message, line, charPositionInLine);
            }
            else
                throw new SyntaxErrorException(message, line, charPositionInLine, e);
        }
    }
}
