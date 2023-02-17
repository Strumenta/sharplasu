using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Model;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Strumenta.Sharplasu.Serialization
{
    public interface ParseResultSerializer {
        string serialize<T>(Result<T> parseResult) where T : Node;
    }

    public interface ParseResultDeserializer {
        Result<T> deserialize<T>(string serializedParseResult) where T : Node;
    }

    public class JsonParseResultSerialization {
        protected JsonSerializerOptions Options { get; }
        protected JsonParseResultSerialization(bool prettyPrint = true) {
            Options = new JsonSerializerOptions {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = prettyPrint
            };
        }
    }

    public class JsonParseResultSerializer : JsonParseResultSerialization, ParseResultSerializer
    {
        public JsonParseResultSerializer(bool prettyPrint = true) : base(prettyPrint) {}

        public string serialize<T>(Result<T> parseResult) where T : Node
        {
            return JsonSerializer.Serialize(parseResult, Options);
        }
    }

    public class JsonParseResultDeserializer : JsonParseResultSerialization, ParseResultDeserializer
    {
        public JsonParseResultDeserializer(bool prettyPrint = true) : base(prettyPrint) {}

        public Result<T> deserialize<T>(string serializedParseResult) where T : Node
        {
            return JsonSerializer.Deserialize<Result<T>>(serializedParseResult, Options)!;
        }
    }
}
