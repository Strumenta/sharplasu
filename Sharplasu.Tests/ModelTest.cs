using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Tests
{
    public class MyNode : Node, Named
    {
        public string Name { get; set; }
        public MyNode(string name)
        { 
            Name = name;
        }
    }

    [TestClass]
    public class ModelTest
    {
        [TestMethod]
        public void ReferenceByNameUnsolvedToString()
        {
            var refUnsolved = new ReferenceByName<MyNode>("foo", null);
            Assert.AreEqual("Ref(foo)[Unsolved]", refUnsolved.ToString());
        }

        [TestMethod]
        public void ReferenceByNameSolvedToString()
        {
            var refSolved = new ReferenceByName<MyNode>("foo", new MyNode("foo"));
            Assert.AreEqual("Ref(foo)[Solved]", refSolved.ToString());
        }

        [TestMethod]
        public void TryToResolvePositiveCaseSameCase()
        {
            var refTest = new ReferenceByName<MyNode>("foo", null);            
            Assert.AreEqual(true, refTest.TryToResolve(new List<MyNode>() { new MyNode("foo") }));
            Assert.AreEqual(true, refTest.Resolved);
        }

        [TestMethod]
        public void TryToResolveNegativeCaseSameCase()
        {
            var refTest = new ReferenceByName<MyNode>("foo", null);
            Assert.AreEqual(false, refTest.TryToResolve(new List<MyNode>() { new MyNode("foo2") }));
            Assert.AreEqual(false, refTest.Resolved);
        }

        [TestMethod]

        public void TryToResolvePositiveCaseDifferentCase()
        {
            var refTest = new ReferenceByName<MyNode>("foo", null);
            Assert.AreEqual(true, refTest.TryToResolve(new List<MyNode>() { new MyNode("foO") }, caseInsensitive: true));
            Assert.AreEqual(true, refTest.Resolved);
        }

        [TestMethod]
        public void TryToResolveNegativeCaseDifferentCase()
        {
            var refTest = new ReferenceByName<MyNode>("foo", null);
            Assert.AreEqual(false, refTest.TryToResolve(new List<MyNode>() { new MyNode("foO") }));
            Assert.AreEqual(false, refTest.Resolved);
        }

        [TestMethod]
        public void TryToCompareUnresolvedReferences()
        {           
            var refTest1 = new ReferenceByName<MyNode>("foo", null);
            var refTest2 = new ReferenceByName<MyNode>("foo", null);
            Assert.AreEqual(refTest1, refTest2);
        }

        [TestMethod]
        public void TryToCompareResolvedReferences()
        {
            var foo = new MyNode("foo");
            var refTest1 = new ReferenceByName<MyNode>("foo", foo);
            var refTest2 = new ReferenceByName<MyNode>("foo", foo);
            Assert.AreEqual(refTest1, refTest2);
        }
    }
}
