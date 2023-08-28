using ExtensionMethods;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Tests.Models.SimpleLang;
using Type = System.Type;

namespace Strumenta.Sharplasu.Tests
{
    [TestClass]
    public class TraversingTest
    {
        [TestMethod]
        public void TestWalk()
        {
            var cu = Models.SimpleLang.Models.GetCompilationUnit();
            TestSequences(
                MapNodesToTypes(cu.Walk()),
                new List<Type>
                {
                    typeof(CompilationUnit),
                    typeof(DisplayStatement),
                    typeof(BooleanLiteral),
                    typeof(SetStatement),
                    typeof(Identifier),
                    typeof(StringLiteral)
                });
        }

        [TestMethod]
        public void TestWalkLeavesFirst()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            TestSequences(
                MapNodesToTypes(cu.WalkLeavesFirst()),
                    new List<System.Type>
                {
                    typeof(BooleanLiteral),
                    typeof(DisplayStatement),
                    typeof(Identifier),
                    typeof(StringLiteral),
                    typeof(SetStatement),
                    typeof(CompilationUnit)
                },
                new List<System.Type>
                {
                    typeof(Identifier),
                    typeof(StringLiteral),
                    typeof(SetStatement),
                    typeof(BooleanLiteral),
                    typeof(DisplayStatement),
                    typeof(CompilationUnit)
                },
                new List<Type>
                {
                    typeof(StringLiteral),
                    typeof(Identifier),
                    typeof(SetStatement),
                    typeof(BooleanLiteral),
                    typeof(DisplayStatement),
                    typeof(CompilationUnit)
                });
        }
        
        [TestMethod]
        public void TestWalkAncestors()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            var identifier = cu.Descendants.First(n => n.GetType() == typeof(Identifier));
            TestSequences(
                MapNodesToTypes(identifier.WalkAncestors()),
                new List<System.Type>
                {
                    typeof(SetStatement),
                    typeof(CompilationUnit)
                });
        }
        
        [TestMethod]
        public void TestWalkChildren()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            TestSequences(
                MapNodesToTypes(cu.WalkChildren()),
                new List<Type>
                {
                    typeof(DisplayStatement),
                    typeof(SetStatement),
                },
                new List<Type>
                {
                    typeof(SetStatement),
                    typeof(DisplayStatement),
                });
        }

        [TestMethod]
        public void TestWalkDescendants()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            TestSequences(
                MapNodesToTypes(cu.WalkDescendants()),
                new List<System.Type>
                {
                    typeof(DisplayStatement),
                    typeof(BooleanLiteral),
                    typeof(SetStatement),
                    typeof(Identifier),
                    typeof(StringLiteral)
                },
                new List<System.Type>
                {
                    typeof(DisplayStatement),
                    typeof(BooleanLiteral),
                    typeof(SetStatement),
                    typeof(StringLiteral),
                    typeof(Identifier)
                },
                new List<Type>
                {
                    typeof(SetStatement),
                    typeof(Identifier),
                    typeof(StringLiteral),
                    typeof(DisplayStatement),
                    typeof(BooleanLiteral)
                },
                new List<Type>
                {
                    typeof(SetStatement),
                    typeof(StringLiteral),
                    typeof(Identifier),
                    typeof(DisplayStatement),
                    typeof(BooleanLiteral)
                });
            TestSequences(
                MapNodesToTypes(cu.WalkDescendants<Expression>()),
                new List<Type>
                {
                    typeof(BooleanLiteral),
                    typeof(StringLiteral)
                },
                new List<Type>
                {
                    typeof(StringLiteral),
                    typeof(BooleanLiteral)
                });
        }

        [TestMethod]
        public void TestFindAncestorOfType()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            var identifier = cu.Descendants.First(n => n.GetType() == typeof(Identifier));
            Assert.IsInstanceOfType(identifier.FindAncestorOfType<CompilationUnit>(), typeof(CompilationUnit));
            Assert.IsInstanceOfType(identifier.FindAncestorOfType<SetStatement>(), typeof(SetStatement));
            Assert.IsNull(identifier.FindAncestorOfType<DisplayStatement>());
        }

        private List<Type> MapNodesToTypes(IEnumerable<Node> nodeSequence)
        {
            return nodeSequence.Select(node => node.GetType()).ToList();
        }

        [TestMethod]
        public void TestSearchByType()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            TestSequences(
                MapNodesToTypes(cu.SearchByType<Expression>()),
                new List<Type>
                {
                    typeof(BooleanLiteral),
                    typeof(StringLiteral)
                },
                new List<Type>
                {
                    typeof(StringLiteral),
                    typeof(BooleanLiteral)
                });
            TestSequences(
                MapNodesToTypes(cu.SearchByType<CompilationUnit>()),
                new List<Type>
                {
                    typeof(CompilationUnit)
                });
        }

        [TestMethod]
        public void TestCollectByType()
        {
            var cu = Tests.Models.SimpleLang.Models.GetCompilationUnit();
            TestSequences(
                MapNodesToTypes(cu.CollectByType<Expression>()),
                new List<Type>
                {
                    typeof(BooleanLiteral),
                    typeof(StringLiteral)
                },
                new List<Type>
                {
                    typeof(StringLiteral),
                    typeof(BooleanLiteral)
                });
            TestSequences(
                MapNodesToTypes(cu.CollectByType<CompilationUnit>()),
                new List<Type>
                {
                    typeof(CompilationUnit)
                });
        }

        private void TestSequences<T>(IEnumerable<T> actualSequence, params IEnumerable<T>[] expectedSequences)
        {
            Assert.IsTrue(
                expectedSequences
                    .Select(s => s.SequenceEqual(actualSequence))
                    .Aggregate(false, (sum, cur) => cur || sum));
        }
    }
}
