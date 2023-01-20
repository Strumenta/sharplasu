using Strumenta.Sharplasu.Tests.Models;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Tests
{

    [TestClass]
    public class ErrorsTest
    {
        [TestMethod]
        public void CheckParseTreeWithErrors() {
            var parser = new ExampleSharpLasuParser();
            var cu = parser.GetTreeForText("displar 12.3");

            Assert.IsFalse(cu.Correct);
            Assert.IsNotNull(cu.Root);
        }

        [TestMethod]
        public void CheckASTWithSemanticError() {
            var parser = new ExampleSharpLasuParser();
            var cu = parser.GetTreeForText(
@"set a = 2
display 12.3
set b = 0"
);

            Assert.IsFalse(cu.Correct);
            Assert.AreEqual(1, cu.Issues.Count);
            Assert.AreEqual("Display statement not supported", cu.Issues[0].Message);
            Assert.AreEqual(IssueType.SEMANTIC, cu.Issues[0].IssueType);

            Assert.IsInstanceOfType(cu.Root, typeof(CompilationUnit));
            Assert.AreEqual(2, cu.Root.Statements.Count);
        }
    }
}
