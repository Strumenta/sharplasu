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

        private abstract class ALangExpression : Node { }
        private class ALangIntLiteral : ALangExpression
        {
            public int Value { get; set; } = 0;

            public ALangIntLiteral(int value)
            {
                Value = value;
            }
        }
        private class ALangSum : ALangExpression
        {
            public ALangExpression Left { get; set; }
            public ALangExpression Right { get; set; }

            public ALangSum(ALangExpression left, ALangExpression right)
            {
                Left = left;
                Right = right;
            }
        }
        private class ALangMult : ALangExpression
        {
            public ALangExpression Left { get; set; }
            public ALangExpression Right { get; set; }

            public ALangMult(ALangExpression left, ALangExpression right)
            {
                Left = left;
                Right = right;
            }
        }

        private abstract class BLangExpression : Node { }
        private class BLangIntLiteral : BLangExpression
        {
            public int Value { get; set; } = 0;

            public BLangIntLiteral(int value)
            {
                Value = value;
            }
        }
        private class BLangSum : BLangExpression
        {
            public BLangExpression Left { get; set; }
            public BLangExpression Right { get; set; }

            public BLangSum(BLangExpression left, BLangExpression right)
            {
                Left = left;
                Right = right;
            }
        }
        private class BLangMult : BLangExpression
        {
            public BLangExpression Left { get; set; }
            public BLangExpression Right { get; set; }

            public BLangMult(BLangExpression left, BLangExpression right)
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

        /**
         * Example of transformation to perform a refactoring within the same language.
         */
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
            Asserts.AssertASTsAreEqual(
                new Mult(new IntLiteral(7), new IntLiteral(8)),
                transformer.Transform(new GenericBinaryExpression(Operator.MULT, new IntLiteral(7), new IntLiteral(8)))
            );
            Asserts.AssertASTsAreEqual(
                new Sum(new IntLiteral(7), new IntLiteral(8)),
                transformer.Transform(new GenericBinaryExpression(Operator.PLUS, new IntLiteral(7), new IntLiteral(8)))
            );
        }

        /**
         * Example of transformation to perform a translation to another language.
         */
        [TestMethod]
        public void TranslateAcrossLanguages()
        {
            var transformer = new ASTTransformer(allowGenericNode: false);
            transformer.RegisterNodeFactory(typeof(ALangIntLiteral), (source, ast) =>
            {
                var al = source as ALangIntLiteral;
                return new BLangIntLiteral(al.Value);
            });
            transformer.RegisterNodeFactory(typeof(ALangSum), (source, ast) =>
            {
                var al = source as ALangSum;
                return new BLangSum(
                    ast.Transform(al.Left) as BLangExpression,
                    ast.Transform(al.Right) as BLangExpression
                );
            });
            transformer.RegisterNodeFactory(typeof(ALangMult), (source, ast) =>
            {
                var al = source as ALangMult;
                return new BLangMult(
                    ast.Transform(al.Left) as BLangExpression,
                    ast.Transform(al.Right) as BLangExpression
                );
            });            
            Asserts.AssertASTsAreEqual(
                new BLangMult(
                    new BLangSum(
                        new BLangIntLiteral(1),
                        new BLangMult(new BLangIntLiteral(2), new BLangIntLiteral(3))
                    ), new BLangIntLiteral(4)),
                transformer.Transform(
                    new ALangMult(
                    new ALangSum(
                        new ALangIntLiteral(1),
                        new ALangMult(new ALangIntLiteral(2), new ALangIntLiteral(3))
                    ), new ALangIntLiteral(4))
                )
            );            
        }

        [TestMethod]
        public void TestDroppingNodes()
        {
            var transformer = new ASTTransformer();
            transformer.RegisterNodeFactory<CU>(typeof(CU), typeof(CU))
                .WithChild(typeof(CU).GetProperty("Statements"), new PropertyAccessor(typeof(CU), "Statements"));
            transformer.RegisterNodeFactory<DisplayIntStatement>(typeof(DisplayIntStatement), (source, ast) =>
            {
                return null;
            });
            transformer.RegisterIdentityTransformation<SetStatement>(typeof(SetStatement));

            var cu = new CU(
                new List<Node>()
                {
                    new DisplayIntStatement(value: 456),
                    new SetStatement(variable: "foo", value: 123)
                }

            );
            CU transformedCU = transformer.Transform(cu) as CU;
            Assert.IsTrue(transformedCU.HasValidParents());
            Assert.AreEqual(transformedCU.Origin, cu);
            Assert.AreEqual(1, transformedCU.Statements.Count);
            Asserts.AssertASTsAreEqual(cu.Statements[1], transformedCU.Statements[0]);
        }
    }
}
