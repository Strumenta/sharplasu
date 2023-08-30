using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Strumenta.Sharplasu.Tests.Model
{
    [TestClass]
    public class ModelTest
    {
        private class MyNode : Node, Named
        {
            public string Name { get; set; }

            public MyNode(string name)
            {
                Name = name;
            }
        }

        [TestMethod]
        public void ReferenceByNameUnsolvedToString()
        {
            var refUnsolved = new ReferenceByName<MyNode>("foo");
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
            var reference = new ReferenceByName<MyNode>("foo");
            Assert.AreEqual(true, reference.TryToResolve(new List<MyNode>() { new MyNode("foo") }));
            Assert.AreEqual(true, reference.Resolved);
        }

        [TestMethod]
        public void TryToResolveNegativeCaseSameCase()
        {
            var reference = new ReferenceByName<MyNode>("foo");
            Assert.AreEqual(false, reference.TryToResolve(new List<MyNode>() { new MyNode("foo2") }));
            Assert.AreEqual(false, reference.Resolved);
        }

        [TestMethod]
        public void TryToResolvePositiveCaseDifference()
        {
            var reference = new ReferenceByName<MyNode>("foo");
            Assert.AreEqual(true, reference.TryToResolve(new List<MyNode>() { new MyNode("fOo") }, caseInsensitive: true));
            Assert.AreEqual(true, reference.Resolved);
        }

        [TestMethod]
        public void TryToResolveNegativeCaseDifference()
        {
            var reference = new ReferenceByName<MyNode>("foo");
            Assert.AreEqual(false, reference.TryToResolve(new List<MyNode>() { new MyNode("fOo") }));
            Assert.AreEqual(false, reference.Resolved);
        }
    }
}
