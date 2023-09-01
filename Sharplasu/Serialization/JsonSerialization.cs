using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Parsing;

namespace Strumenta.Sharplasu.Serialization.Json
{
    public abstract class JsonParseResultSerialization {
        protected JsonSerializerOptions Options { get; }
        protected JsonParseResultSerialization(bool prettyPrint = true) {
            Options = new JsonSerializerOptions {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = prettyPrint
            };
        }
    }

    public class JsonGenerator : JsonParseResultSerialization, ParseResultSerializer
    {
        public JsonGenerator(bool prettyPrint = true) : base(prettyPrint) {}

        public virtual string generateString<T, C>(ParsingResult<T, C> parseResult) 
            where T : Node
            where C : ParserRuleContext
        {            
            return JsonSerializer.Serialize(parseResult, Options);
        }

        public string generateString<C>(Result<C> parseResult) where C : class
        {            
            return JsonSerializer.Serialize(parseResult, Options);
        }

        public virtual string generateString<T>(T tree) where T : Node
        {
            return JsonSerializer.Serialize(tree, Options);
        }
    }

    public class JsonDeserializer : JsonParseResultSerialization, ParseResultDeserializer
    {
        public JsonDeserializer(bool prettyPrint = true) : base(prettyPrint) {}

        public virtual ParsingResult<T, C> deserializeParsingResult<T, C>(string serializedParseResult)
            where T : Node
            where C : ParserRuleContext
        {
            return JsonSerializer.Deserialize<ParsingResult<T, C>>(serializedParseResult, Options);
        }

        public virtual Result<C> deserializeResult<C>(string serializedResult)
            where C : class
        {
            return JsonSerializer.Deserialize<Result<C>>(serializedResult, Options);
        }

        public virtual T deserializeTree<T>(string serializedTree) where T : Node
        {
            return JsonSerializer.Deserialize<T>(serializedTree, Options);
        }
    }
}
