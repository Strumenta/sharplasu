using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Traversing;

namespace Strumenta.Sharplasu.Tests.CodeGeneration
{
    internal static class TestUtils
    {
        internal static void CheckDestinationIsNull(Node node)
        {
            foreach (var item in node.Walk())
            {
                Assert.IsNull(item.Destination);
            }
        }

        internal static void CheckDestinationIsNotNull(Node node)
        {
            foreach (var item in node.Walk())
            {
                Assert.IsNotNull(item.Destination);
            }
        }
    }
}
