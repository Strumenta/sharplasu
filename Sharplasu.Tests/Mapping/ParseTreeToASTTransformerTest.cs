using Antlr4.Runtime;
using Newtonsoft.Json.Linq;
using Strumenta.Sharplasu.Mapping;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Tests.Models.SimpleLang;
using Strumenta.Sharplasu.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Strumenta.Sharplasu.Tests.Mapping
{
    [TestClass]
    public class ParseTreeToASTTransformerTest
    {
        private class CU : Node
        {
            public List<Node> Statements { get; set; }

            public CU(List<Node> statements)
            {
                Statements = statements ?? new List<Node>();
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
                Value = value;
            }
        }

        private class SetStatement : Statement
        {
            public string Variable { get; set; } = "";
            public int Value { get; set; } = 0;

            public SetStatement(string variable, int value)
            {
                Variable = variable;
                Value = value;
            }
        }

        private void Configure (ASTTransformer transformer)
        {
            transformer.RegisterNodeFactory<CU>(typeof(SimpleLangParser.CompilationUnitContext), typeof(CU))
                .WithChild<Node>(typeof(CU).GetProperty("Statements"), new PropertyAccessor(typeof(SimpleLangParser.CompilationUnitContext), "statement"));
            transformer.RegisterNodeFactory<DisplayIntStatement>(typeof(SimpleLangParser.DisplayStmtContext), (it) =>
            {
                var ctx = it as SimpleLangParser.DisplayStmtContext;
                if(ctx.exception != null || ctx.expression().exception != null)
                {
                    // We throw a custom error so that we can check that it's recorded in the AST
                    throw new InvalidOperationException("Parse error");
                }
                return new DisplayIntStatement(int.Parse(ctx.expression().INT_LIT().GetText()));
            });
            transformer.RegisterNodeFactory<SetStatement>(typeof(SimpleLangParser.SetStmtContext), (it) =>
            {
                var ctx = it as SimpleLangParser.SetStmtContext;
                if (ctx.exception != null || ctx.expression().exception != null)
                {
                    // We throw a custom error so that we can check that it's recorded in the AST
                    throw new InvalidOperationException("Parse error");
                }
                return new SetStatement(ctx.ID().GetText(), int.Parse(ctx.expression().INT_LIT().GetText()));
            });
        }
        
        [TestMethod]
        public void TestGenericNode()
        {
            var code = "set foo = 123\ndisplay 456";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var pt = parser.compilationUnit();

            var transformer = new ParseTreeToASTTransformer();
            Asserts.AssertASTsAreEqual(new GenericNode(), transformer.Transform(pt));
        }

        [TestMethod]
        public void TestParseTreeTransformer()
        {
            var code = "set foo = 123\ndisplay 456";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var pt = parser.compilationUnit();

            var transformer = new ParseTreeToASTTransformer();
            Configure(transformer);

            var cu = new CU(
                new List<Node>()
                {
                    new SetStatement("foo", 123).WithParseTreeNode(pt.statement(0)),
                    new DisplayIntStatement(456).WithParseTreeNode(pt.statement(1)),
                }
            ).WithParseTreeNode(pt);
            var transformedCU = transformer.Transform(pt);
            Asserts.AssertASTsAreEqual(cu, transformedCU, considerPosition: true);
            Assert.IsTrue(transformedCU.HasValidParents());
            Assert.IsNull(transformedCU.InvalidPositions().FirstOrDefault());
        }

        [TestMethod]
        public void TestGenericASTTransformer()
        {
            var code = "set foo = 123\ndisplay 456";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var pt = parser.compilationUnit();

            var transformer = new ASTTransformer();
            Configure(transformer);

            // Compared to ParseTreeToASTTransformer, the base class ASTTransformer
            // does not assign a parse tree node to each AST node
            var cu = new CU(
                new List<Node>()
                {
                    new SetStatement("foo", 123),
                    new DisplayIntStatement(456),
                }
            );
            var transformedCU = transformer.Transform(pt);
            Asserts.AssertASTsAreEqual(cu, transformedCU, considerPosition: true);
            Assert.IsTrue(transformedCU.HasValidParents());            
        }

        [TestMethod]
        public void TestTransformationWithErrors()
        {
            var code = "set foo = \ndisplay @@@";
            var lexer = new SimpleLangLexer(CharStreams.fromString(code));
            var parser = new SimpleLangParser(new CommonTokenStream(lexer));
            var pt = parser.compilationUnit();
            Assert.AreEqual(2, parser.NumberOfSyntaxErrors);

            var transformer = new ParseTreeToASTTransformer();
            Configure(transformer);

            var cu = new CU(
                new List<Node>()
                {
                    new GenericErrorNode(new InvalidOperationException("Parse error")).WithParseTreeNode(pt.statement(0)),
                    new GenericErrorNode(new InvalidOperationException("Parse error")).WithParseTreeNode(pt.statement(1))
                }
            ).WithParseTreeNode(pt);
            var transformedCU = transformer.Transform(pt);
            Asserts.AssertASTsAreEqual(cu, transformedCU, considerPosition: true);
            Assert.IsTrue(transformedCU.HasValidParents());
            Assert.IsNull(transformedCU.InvalidPositions().FirstOrDefault());
        }
    }
}
