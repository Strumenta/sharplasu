using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Strumenta.Sharplasu.Parsing
{        
    internal static class ASTParserExtensions
    {
        internal static string InputStreamToString(Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            using (var buffered = new StreamReader(stream, encoding))
            {
                return buffered.ReadToEnd();
            }
        }
    }
    
    public interface ASTParser<R, C> 
        where R : Node
        where C : ParserRuleContext
    {
        /**
         * Parses source code, returning a result that includes an AST and a collection of parse issues (errors, warnings).
         * The parsing is done in accordance to the StarLasu methodology i.e. a first-stage parser builds a parse tree which
         * is then mapped onto a higher-level tree called the AST.
         * @param inputStream the source code.
         * @param charset the character set in which the input is encoded.
         * @param considerPosition if true (the default), parsed AST nodes record their position in the input text.
         * @param measureLexingTime if true, the result will include a measurement of the time spent in lexing i.e. breaking
         * the input stream into tokens.
         */

        ParsingResult<R, C> Parse(Stream inputStream,
                Encoding encoding = null,
                bool considerPosition = true,
                bool measureLexingTime = false,
                Source source = null
            );
        //{
        //    encoding ??= Encoding.UTF8;
        //    return Parse(ASTParserExtensions.InputStreamToString(inputStream, encoding), considerPosition, measureLexingTime, source);
        //}

        ParsingResult<R, C> Parse(string code,
                bool considerPosition = true,
                bool measureLexingTime = false,
                Source source = null
            );

        //ParsingResult<R, C> Parse(string code,
        //        bool considerPosition = true,
        //        bool measureLexingTime = false                
        //    ) //=> Parse(code, considerPosition, measureLexingTime, new StringSource(code))
        //    ;

        ParsingResult<R, C> Parse(FileInfo file,
                Encoding encoding = null,
                bool considerPosition = true,
                bool measureLexingTime = false                
            );
    }
}
