using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Serialization.Xml
{
    public class XmlParseResultSerializer : ParseResultSerializer
    {
        public string serializeResult<T>(Result<T> parseResult) where T : Node
        {
            var xmlSerializer = new XmlSerializer(typeof(Result<T>), new XmlRootAttribute("Result"));
            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder))
            {
                xmlSerializer.Serialize(writer, parseResult, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
            return stringBuilder.ToString();
        }

        public string serializeTree<T>(T tree) where T : Node
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder))
            {
                xmlSerializer.Serialize(writer, tree, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
            return stringBuilder.ToString();
        }
    }

    public class XmlParseResultDeserializer : ParseResultDeserializer
    {
        public Result<T> deserializeResult<T>(string serializedParseResult) where T : Node
        {
            var xmlSerializer = new XmlSerializer(typeof(Result<T>), new XmlRootAttribute("Result"));
            using (var reader = new StringReader(serializedParseResult))
            {
                return (Result<T>)xmlSerializer.Deserialize(reader);
            }
        }

        public T deserializeTree<T>(string serializedTree) where T : Node
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(serializedTree))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }
    }
}
