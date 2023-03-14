using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public virtual string generateString<T>(Result<T> parseResult) where T : Node
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

        public virtual Result<T> deserializeResult<T>(string serializedParseResult) where T : Node
        {
            return JsonSerializer.Deserialize<Result<T>>(serializedParseResult, Options)!;
        }

        public virtual T deserializeTree<T>(string serializedTree) where T : Node
        {
            return JsonSerializer.Deserialize<T>(serializedTree, Options);
        }
    }
}
