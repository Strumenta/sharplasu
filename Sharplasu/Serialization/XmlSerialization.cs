using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Model;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Parsing;

namespace Strumenta.Sharplasu.Serialization.Xml
{
    public class XmlGenerator : ParseResultSerializer
    {
        public virtual string generateString<T, C>(ParsingResult<T, C> parseResult)
            where T : Node
            where C : ParserRuleContext
        {
            var xmlSerializer = new XmlSerializer(typeof(ParsingResult<T, C>), new XmlRootAttribute("Result"));
            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder))
            {
                xmlSerializer.Serialize(writer, new ParsingResult<T, C>(parseResult.Issues, parseResult.Root), new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
            return stringBuilder.ToString();
        }

        public virtual string generateString<C>(Result<C> result) where C : class
        {
            var xmlSerializer = new XmlSerializer(typeof(Result<C>), new XmlRootAttribute("Result"));
            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder))
            {
                xmlSerializer.Serialize(writer, new Result<C>(result.Issues, result.Root), new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
            }
            return stringBuilder.ToString();
        }

        public virtual string generateString<T>(T tree) where T : Node
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

    public class XmlDeserializer : ParseResultDeserializer
    {        
        public virtual ParsingResult<T, C> deserializeParsingResult<T, C>(string serializedParseResult)
            where T : Node
            where C : ParserRuleContext
        {
            var xmlSerializer = new XmlSerializer(typeof(ParsingResult<T, C>), new XmlRootAttribute("Result"));
            using (var reader = new StringReader(serializedParseResult))
            {
                return (ParsingResult<T, C>)xmlSerializer.Deserialize(reader);
            }
        }

        public virtual Result<C> deserializeResult<C>(string serializedResult) where C : class
        {
            var xmlSerializer = new XmlSerializer(typeof(Result<C>), new XmlRootAttribute("Result"));
            using (var reader = new StringReader(serializedResult))
            {
                return (Result<C>)xmlSerializer.Deserialize(reader);
            }
        }

        public virtual T deserializeTree<T>(string serializedTree) where T : Node
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(serializedTree))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }
    }
}
