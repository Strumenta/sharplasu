using System.Text.Json;
using System.Text.Json.Serialization;
using Strumenta.Sharplasu.Serialization.Json;
using Strumenta.Sharplasu.Serialization.Xml;
using Strumenta.Sharplasu.Tests.Models;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Tests {

    [TestClass]
    public class FirstStageParsingTest
    {
        [TestMethod]
        public void TestString() {
            var parser = new ExampleSharpLasuParser();
            var parsingResult = parser.ParseFirstStage("set foo = 123 set bar = 1.23");
            Assert.IsNotNull(parsingResult);
            Assert.IsTrue(parsingResult.Correct);
        }

        [TestMethod]
        public void TestStringWithError()
        {
            var parser = new ExampleSharpLasuParser();
            var parsingResult = parser.ParseFirstStage("ste foo = 123");
            Assert.IsNotNull(parsingResult);
            Assert.IsFalse(parsingResult.Correct);
            Assert.AreEqual(5, parsingResult.Issues.Count);
            Assert.AreEqual(IssueType.Syntatic, parsingResult.Issues[0].IssueType);
            Assert.AreEqual(IssueSeverity.Error, parsingResult.Issues[0].IssueSeverity);
            Assert.AreEqual(IssueType.Syntatic, parsingResult.Issues[1].IssueType);
            Assert.AreEqual(IssueSeverity.Error, parsingResult.Issues[1].IssueSeverity);
        }
    }
}
