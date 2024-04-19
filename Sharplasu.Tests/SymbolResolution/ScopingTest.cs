using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.SymbolResolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strumenta.Sharplasu.Tests.SymbolResolution
{    
    [TestClass]
    public class ScopingTest
    {
        private class TestNode : Node, Named
        {
            public string Name { get; set; }

            public TestNode(string name)
            {
                Name = name;
            }
        }

        private static bool GetReferenceType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(ReferenceByName<>);
        }

        [TestMethod]
        public void TestScopeWithIgnoreCase()
        {
            var node = new TestNode("TestNode");
            var scope = new Scope(ignoreCase: true);
            scope.Define(node);
            Assert.AreEqual(node, scope.Resolve("TestNode"));
            Assert.AreEqual(node, scope.Resolve("testnode"));
            Assert.AreEqual(node, scope.Resolve("testNode"));
            Assert.AreEqual(node, scope.Resolve("Testnode"));
        }

        [TestMethod]
        public void TestScopeWithoutIgnoreCase()
        {
            var node = new TestNode("TestNode");
            var scope = new Scope(ignoreCase: false);
            scope.Define(node);
            Assert.AreEqual(node, scope.Resolve("TestNode"));
            Assert.IsNull(scope.Resolve("testnode"));
            Assert.IsNull(scope.Resolve("testNode"));
            Assert.IsNull(scope.Resolve("Testnode"));
        }
    }
}
