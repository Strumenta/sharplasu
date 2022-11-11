using Strumenta.Cslasu.Model;
using Strumenta.Cslasu.Testing;
using Strumenta.Cslasu.Validation;

namespace Strumenta.Cslasu.Tests
{    
    [TestClass]
    public class TreeTests
    {
        [TestMethod]
        public void CheckingCompleteASTTreeIsValid()
        {
            var one = new TopNode
            {
                GoodStuff = 1,
                BadStuff = 2,
                Smaller = new SmallNode
                {
                    Description = "I stand here"
                }
            };
            one.Smaller.Parent = one;
            List<Issue> issues = new List<Issue>();
            TestParserFacade.VerifyASTTree(one, ref issues);

            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod]
        public void CheckingASTTreeWithNullNodesIsValid()
        {
            var one = new TopNode
            {
                GoodStuff = 1,
                BadStuff = 2,
                Smaller = null
            };
            List<Issue> issues = new List<Issue>();
            TestParserFacade.VerifyASTTree(one, ref issues);

            Assert.AreEqual(0, issues.Count);
        }

        [TestMethod]
        public void CheckingNodeWithoutParentIsInvalid()
        {
            var small = new SmallNode
            {
                Description = "I stand here"
            };            
            var one = new TopNode
            {
                GoodStuff = 1,
                BadStuff = 2,
                Smaller = small
            };
            small.Parent = null;
            List<Issue> issues = new List<Issue>();
            TestParserFacade.VerifyASTTree(one, ref issues);

            Assert.AreEqual(1, issues.Count);
        }
    }
}