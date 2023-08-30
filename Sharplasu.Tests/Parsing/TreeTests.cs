using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Validation;
using Strumenta.Sharplasu.Tests.Models;

namespace Strumenta.Sharplasu.Tests
{
    
    [TestClass]
    public class TreeTests
    {
        [TestMethod]
        public void CheckingCompleteASTTreeIsValid()
        {
            var example = new ExampleSharpLasuParser();
            var tree = example.GetTreeForText("set foo = 123 set bar = 1.23");
            List<Issue> issues = new List<Issue>();
            ExampleSharpLasuParser.VerifyASTTree(tree.Root, issues);

            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod]
        public void CheckingASTIssuesAreRaised()
        {
            var example = new ExampleSharpLasuParser();
            var tree = example.GetTreeForText("display 12.3");      

            Assert.AreEqual(1, tree.Issues.Count);
        }

        [TestMethod]
        public void CheckingParsingIssuesAreRaised()
        {
            var example = new ExampleSharpLasuParser();
            var tree = example.GetTreeForText("displat 12.3");

            Assert.AreEqual(2, tree.Issues.Count);
        }
    }
}