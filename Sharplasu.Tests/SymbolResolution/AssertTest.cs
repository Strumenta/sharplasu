using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.SymbolResolution;

namespace Strumenta.Sharplasu.Tests.SymbolResolution
{
    [TestClass]
    public class AssertTest
    {
        [TestMethod]
        public void TestAssertion()
        {
            var cu = DeclarativeLocalSymbolResolverTest.GetCompilationUnit();
            Assert.ThrowsException<SymbolResolutionException>(() => cu.AssertAllReferencesResolved());
            DeclarativeLocalSymbolResolverTest.GetFullSymbolResolver().ResolveSymbols(cu);
            Assert.ThrowsException<SymbolResolutionException>(() => cu.AssertNotAllReferencesResolved());
        }

        [TestMethod]
        public void CheckMessage()
        {
            var cu = DeclarativeLocalSymbolResolverTest.GetCompilationUnit();
            try
            {
                cu.AssertAllReferencesResolved();
            }
            catch (SymbolResolutionException ex)
            {
                Assert.AreEqual("Not all references in Strumenta.Sharplasu.Tests.SymbolResolution." +
                    "DeclarativeLocalSymbolResolverTest+CompilationUnit  are solved", ex.Message);
            }            
        }
    }
}
