using Newtonsoft.Json.Linq;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Validation;

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

        private class BazRoot : Node
        {
            public List<BazStmt> Stmts { get; set; } = new List<BazStmt>();

            public BazRoot(List<BazStmt> stmts = null)
            { 
                Stmts = stmts ?? new List<BazStmt>();
            }
        }

        
        private class BazStmt : Node
        {
            public string Desc { get; set; }
            
            public BazStmt(string desc)
            { 
                this.Desc = desc; 
            }
        }


        private class BarRoot : Node
        {
            public List<BarStmt> Stmts { get; set; } = new List<BarStmt>();

            public BarRoot(List<BarStmt> stmts = null)
            {
                Stmts = stmts;
            }
        }


        private class BarStmt : Node
        {
            public string Desc { get; set; }

            public BarStmt(string desc)
            {
                this.Desc = desc;
            }
        }

        private enum Type
        {
            INT, STR
        }

        private abstract class TypedExpression : Node
        {
            public Type? Type { get; set; }
            
            public TypedExpression(Type? type = null) : base()
            {
                this.Type = type;
            }
        }

        private class TypedLiteral : TypedExpression
        {
            public string Value { get; set; }

            public TypedLiteral(string value, Type type) : base(type)
            {
                Value = value;
                Type = type;
            }
        }

        private class TypedSum : TypedExpression
        {
            public TypedExpression Left { get; set; }
            public TypedExpression Right { get; set; }

            public TypedSum(TypedExpression left, TypedExpression right, Type? type = null) : base(type)
            {
                Left = left;
                Right = right;
                Type = type;
            }
        }

        private class TypedConcat : TypedExpression
        {
            public TypedExpression Left { get; set; }
            public TypedExpression Right { get; set; }

            public TypedConcat(TypedExpression left, TypedExpression right, Type? type = null) : base(type)
            {
                Left = left;
                Right = right;
                Type = type;
            }
        }

        [TestMethod]
        public void TestIdentitiyTransformer()
        {
            var transformer = new ASTTransformer();
            transformer.RegisterNodeFactory<CU>(typeof(CU), typeof(CU))
                .WithChild<Node>(typeof(CU).GetProperty("Statements"), new PropertyAccessor(typeof(CU), "Statements"));
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
                .WithChild<Node>(typeof(CU).GetProperty("Statements"), new PropertyAccessor(typeof(CU), "Statements"));
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

        [TestMethod]
        public void TestNestedOrigin()
        {
            var transformer = new ASTTransformer();
            transformer.RegisterNodeFactory<CU>(typeof(CU), typeof(CU))
                .WithChild<Node>(typeof(CU).GetProperty("Statements"), new PropertyAccessor(typeof(CU), "Statements"));
            transformer.RegisterNodeFactory<DisplayIntStatement>(typeof(DisplayIntStatement), (source) =>
            {
                return (source as DisplayIntStatement).WithOrigin(new GenericNode());
            });
            transformer.RegisterIdentityTransformation<SetStatement>(typeof(SetStatement));

            var cu = new CU(
                new List<Node>()
                {
                    new DisplayIntStatement(value: 456)                    
                }

            );
            CU transformedCU = transformer.Transform(cu) as CU;
            Assert.IsTrue(transformedCU.HasValidParents());
            Assert.AreEqual(transformedCU.Origin, cu);
            Assert.IsTrue(transformedCU.Statements[0].Origin is GenericNode);            
        }

        [TestMethod]
        public void TestTransformingOneNodeToMany()
        {
            var transformer = new ASTTransformer();
            transformer.RegisterNodeFactory<BazRoot>(typeof(BarRoot), typeof(BazRoot))
                .WithChild<BazStmt>(typeof(BazRoot).GetProperty("Stmts"), new PropertyAccessor(typeof(BarRoot), "Stmts"));
            transformer.RegisterMultipleNodeFactory<BarStmt>(typeof(BarStmt), (source) =>
            {
                return new List<BazStmt>() { new BazStmt($"{(source as BarStmt).Desc}-1"), new BazStmt($"{(source as BarStmt).Desc}-2") }.ToList<Node>();
            });            

            var original = new BarRoot(
                new List<BarStmt>()
                {
                    new BarStmt("a"),
                    new BarStmt("b"),
                }

            );
            var transformed = transformer.Transform(original);
            Assert.IsTrue(transformed.HasValidParents());
            Assert.AreEqual(transformed.Origin, original);            
            Asserts.AssertASTsAreEqual(
                new BazRoot(
                    new List<BazStmt>() 
                    { 
                        new BazStmt("a-1"),
                        new BazStmt("a-2"),
                        new BazStmt("b-1"),
                        new BazStmt("b-2"),
                    }
                ),
                transformed
            );
        }

        /**
         * Example of transformation to perform a simple type calculation.
         */
        [TestMethod]
        public void TestFinalizerComputingTypes()
        {
            var transformer = new ASTTransformer(allowGenericNode: false);
            transformer.RegisterIdentityTransformation<TypedSum>(typeof(TypedSum)).WithFinalizer<TypedSum>((it) =>
            {
                if (it.Left.Type == Type.INT && it.Right.Type == Type.INT)
                    it.Type = Type.INT;
                else
                {
                    transformer.AddIssue("Illegal types for sum operation. Only integer values are allowed. " +
                            $"Found: ({it.Left.Type ?? null}, {it.Right.Type?? null})",
                            IssueSeverity.Error, it.Position
                    );
                }

            });
            transformer.RegisterIdentityTransformation<TypedConcat>(typeof(TypedConcat)).WithFinalizer<TypedConcat>((it) =>
            {
                if (it.Left.Type == Type.STR && it.Right.Type == Type.STR)
                    it.Type = Type.STR;
                else
                {
                    transformer.AddIssue("Illegal types for concat operation. Only string values are allowed. " +
                            $"Found: ({it.Left.Type ?? null}, {it.Right.Type ?? null})",
                            IssueSeverity.Error, it.Position
                    );
                }
            });
            transformer.RegisterIdentityTransformation<TypedLiteral>(typeof(TypedLiteral));
            // sum - legal
            Asserts.AssertASTsAreEqual(
                new TypedSum(
                    new TypedLiteral("1", Type.INT),
                    new TypedLiteral("1", Type.INT),
                    Type.INT
                ),
                transformer.Transform(
                    new TypedSum(
                        new TypedLiteral("1", Type.INT),
                        new TypedLiteral("1", Type.INT)                 
                    )
                )
            );
            // concat - legal
            Assert.AreEqual(0, transformer.Issues.Count);
            Asserts.AssertASTsAreEqual(
                new TypedConcat(
                    new TypedLiteral("test", Type.STR),
                    new TypedLiteral("test", Type.STR),
                    Type.STR
                ),
                transformer.Transform(
                    new TypedConcat(
                        new TypedLiteral("test", Type.STR),
                        new TypedLiteral("test", Type.STR)                      
                    )
                )
            );
            Assert.AreEqual(0, transformer.Issues.Count);
            // sum - error
            Asserts.AssertASTsAreEqual(
                new TypedSum(
                    new TypedLiteral("1", Type.INT),
                    new TypedLiteral("test", Type.STR),
                    null
                ),
                transformer.Transform(
                    new TypedSum(
                        new TypedLiteral("1", Type.INT),
                        new TypedLiteral("test", Type.STR)                        
                    )
                )
            );
            Assert.AreEqual(1, transformer.Issues.Count);
            Assert.AreEqual(
                Issue.Semantic(
                    "Illegal types for sum operation. Only integer values are allowed. Found: (INT, STR)",
                    null,
                    IssueSeverity.Error
                ),
                transformer.Issues[0]
            );
            // concat - error
            Asserts.AssertASTsAreEqual(
                new TypedConcat(
                    new TypedLiteral("1", Type.INT),
                    new TypedLiteral("test", Type.STR),
                    null
                ),
                transformer.Transform(
                    new TypedConcat(
                        new TypedLiteral("1", Type.INT),
                        new TypedLiteral("test", Type.STR)
                    )
                )
            );
            Assert.AreEqual(2, transformer.Issues.Count);
            Assert.AreEqual(
                Issue.Semantic(
                    "Illegal types for concat operation. Only string values are allowed. Found: (INT, STR)",
                    null,
                    IssueSeverity.Error
                ),
                transformer.Issues[1]
            );
        }
    }
}
