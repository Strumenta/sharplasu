using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Antlr4.Runtime;

namespace Strumenta.Sharplasu.Serialization
{
    public interface ParseResultSerializer {
        string generateString<T, C>(ParsingResult<T, C> parseResult) 
            where T : Node            
            where C : ParserRuleContext;

        string generateString<C>(Result<C> parseResult)            
            where C : class;
        string generateString<T>(T tree) where T : Node;
    }

    public interface ParseResultDeserializer {
        ParsingResult<T, C> deserializeParsingResult<T, C>(string serializedParseResult)
            where T : Node
            where C : ParserRuleContext;

        Result<C> deserializeResult<C>(string serializedResult)
            where C : class;
        T deserializeTree<T>(string serializedTree) where T : Node;
    }
}
