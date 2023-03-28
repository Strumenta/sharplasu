using ExtensionMethods;
using Strumenta.SharpLasu.Tests.Models.SimpleLang;
using Type = System.Type;

namespace Strumenta.Sharplasu.Tests
{
    [TestClass]
    public class TraversingTest
    {
        [TestMethod]
        public void TestWalk()
        {
            var cu = Strumenta.SharpLasu.Tests.Models.SimpleLang.Models.GetCompilationUnit();

            var expectedNodeTypesPath = new List<System.Type>
            {
                typeof(CompilationUnit),
                typeof(DisplayStatement),
                typeof(BooleanLiteral),
                typeof(SetStatement),
                typeof(Identifier),
                typeof(StringLiteral)
            };

            var expectedNodeTypesPath2 = new List<System.Type>
            {
                typeof(CompilationUnit),
                typeof(DisplayStatement),
                typeof(BooleanLiteral),
                typeof(SetStatement),
                typeof(StringLiteral),
                typeof(Identifier)
            };

            var expectedNodeTypesPath3 = new List<Type>
            {
                typeof(CompilationUnit),
                typeof(SetStatement),
                typeof(Identifier),
                typeof(StringLiteral),
                typeof(DisplayStatement),
                typeof(BooleanLiteral)
            };

            var expectedNodeTypesPath4 = new List<Type>
            {
                typeof(CompilationUnit),
                typeof(SetStatement),
                typeof(StringLiteral),
                typeof(Identifier),
                typeof(DisplayStatement),
                typeof(BooleanLiteral)
            };

            var actualNodeTypesPath = cu.Walk().Select(node => node.GetType()).ToList();

            Assert.IsTrue(actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath) ||
                          actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath2) ||
                          actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath3) ||
                          actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath4));
        }

        [TestMethod]
        public void TestWalkLeavesFirst()
        {
            var cu = Strumenta.SharpLasu.Tests.Models.SimpleLang.Models.GetCompilationUnit();

            var expectedNodeTypesPath = new List<System.Type>
            {
                typeof(BooleanLiteral),
                typeof(DisplayStatement),
                typeof(Identifier),
                typeof(StringLiteral),
                typeof(SetStatement),
                typeof(CompilationUnit)
            };

            var expectedNodeTypesPath2 = new List<System.Type>
            {
                typeof(Identifier),
                typeof(StringLiteral),
                typeof(SetStatement),
                typeof(BooleanLiteral),
                typeof(DisplayStatement),
                typeof(CompilationUnit)
            };

            var expectedNodeTypesPath3 = new List<Type>
            {
                typeof(StringLiteral),
                typeof(Identifier),
                typeof(SetStatement),
                typeof(BooleanLiteral),
                typeof(DisplayStatement),
                typeof(CompilationUnit)
            };

            var actualNodeTypesPath = cu.WalkLeavesFirst().Select(node => node.GetType()).ToList();

            Assert.IsTrue(actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath) ||
                          actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath2) ||
                          actualNodeTypesPath.SequenceEqual(expectedNodeTypesPath3));
        }
    }
}
