using Newtonsoft.Json.Linq;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Testing;

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
                this.Statements = statements ?? new List<Node>(); 
            }

            public CU()
            { 
                Statements = new List<Node>();
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

        private abstract class Expression : Node { }
        private class IntLiteral : Expression
        {
            public int Value { get; set;} = 0;

            public IntLiteral(int value)
            {
                Value = value;
            }
        }

        private enum Operator
        {
            PLUS, MULT
        }

        private class GenericBinaryExpression : Node
        {
            public Operator Operator { get; set; }
            public Expression Left { get; set; }
            public Expression Right { get; set; }

            public GenericBinaryExpression(Operator op, Expression left, Expression right)
            {
                Operator = op;
                Left = left;
                Right = right;
            }
        }

        private class Mult : Node
        {
            public Expression Left { get; set; }
            public Expression Right { get; set; }

            public Mult(Expression left, Expression right)
            {                
                Left = left;
                Right = right;
            }
        }

        private class Sum : Node
        {
            public Expression Left { get; set; }
            public Expression Right { get; set; }

            public Sum(Expression left, Expression right)
            {
                Left = left;
                Right = right;
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

        [TestMethod]
        public void TranslateBinaryExpression()
        {
            var transformer = new ASTTransformer(allowGenericNode: false);
            transformer.RegisterNodeFactory<Node>(typeof(GenericBinaryExpression), (source, ast) =>
            {
                var sb = source as GenericBinaryExpression;         

                switch (sb.Operator)
                {
                    case Operator.MULT:
                        return new Mult(
                            ast.Transform(sb.Left) as Expression,
                            ast.Transform(sb.Right) as Expression
                        );
                    case Operator.PLUS:
                        return new Sum(
                            ast.Transform(sb.Left) as Expression,
                            ast.Transform(sb.Right) as Expression
                         );
                    default:
                        return null;
                }
            });
            transformer.RegisterIdentityTransformation<IntLiteral>(typeof(IntLiteral));
            var m = transformer.Transform(new GenericBinaryExpression(Operator.MULT, new IntLiteral(7), new IntLiteral(8)));
            Asserts.AssertASTsAreEqual(
                new Mult(new IntLiteral(7), new IntLiteral(8)),
                transformer.Transform(new GenericBinaryExpression(Operator.MULT, new IntLiteral(7), new IntLiteral(8)))
            );
            Asserts.AssertASTsAreEqual(
                new Sum(new IntLiteral(7), new IntLiteral(8)),
                transformer.Transform(new GenericBinaryExpression(Operator.PLUS, new IntLiteral(7), new IntLiteral(8)))
            );
        }
    }
}
