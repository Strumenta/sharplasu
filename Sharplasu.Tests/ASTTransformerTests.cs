using Newtonsoft.Json.Linq;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Testing;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Strumenta.Sharplasu.Tests
{   
    [TestClass]
    public class ASTTransformerTests
    {
        private class CU : Node
        {
            public List<Node> Statements { get; set; }

            public CU(List<Node> statements)
            {
                this.Statements = statements;
            }
        }

        private class Statement : Node { }
        private class DisplayIntStatement : Statement
        {
            public int Value { get; set; }

            public DisplayIntStatement(int value)
            {
                this.Value = value;
            }
        }

        private class SetStatement : Statement
        {
            public string Variable { get; set; } = "";
            public int Value { get; set; } = 0;

            public SetStatement(string variable, int value)
            {
                this.Variable = variable;
                this.Value = value;
            }
        }

        [TestMethod]
        public void TestIdentitiyTransformer()
        {
            var transformer = new ASTTransformer();
            transformer.RegisterNodeFactory<CU>(typeof(CU), typeof(CU))
                .WithChild(typeof(CU).GetProperty("Statements"), new PropertyAccessor(typeof(CU), "Statements"));
            transformer.RegisterIdentityTransformation<DisplayIntStatement>(typeof(DisplayIntStatement));
            transformer.RegisterIdentityTransformation<SetStatement>(typeof(SetStatement));

            var cu = new CU(
                new List<Node>()
                {
                    new SetStatement(variable: "foo", value: 123),
                    new DisplayIntStatement(value: 456)
                }

            );
            var transformedCU = transformer.Transform(cu);
            Asserts.AssertASTsAreEqual(cu, transformedCU, considerPosition: true);
            Assert.IsTrue(transformedCU.HasValidParents());
            Assert.AreEqual(transformedCU.Origin, cu);
        }
    }
}
