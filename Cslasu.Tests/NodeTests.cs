using Strumenta.Cslasu.Model;
using Strumenta.Cslasu.Testing;
using Strumenta.Cslasu.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strumenta.Cslasu.Tests
{   

    [TestClass]
    public class NodeTests
    {
        [TestMethod]
        public void CheckingChildrenAreFound()
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
            List<Issue> issues = new List<Issue>();            

            Assert.AreEqual(1, one.Children.Count);
        }

        [TestMethod]
        public void CheckingNodeArePrintCorrectly()
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
            List<Issue> issues = new List<Issue>();            

            Assert.AreEqual(
@"TopNode
  GoodStuff 1
  BadStuff 2
  SmallNode
    Description I stand here
", one.MultiLineString());
        }
    }
}
