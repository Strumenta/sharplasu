using Strumenta.Sharplasu.Tests.Models;

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
    }
}
