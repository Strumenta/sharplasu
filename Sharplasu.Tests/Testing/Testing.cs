using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Tests.Models.SimpleLang;
using Strumenta.Sharplasu.Validation;
using static Strumenta.Sharplasu.Testing.Asserts;

namespace Strumenta.Sharplasu.Tests;

[TestClass]
public class Testing
{
    [TestMethod]
    public void TestParsingResultsIdentity()
    {
        var result1 = new ParsingResult<CompilationUnit, ParserRuleContext>
        {
            Issues = new List<Issue>()
            {
                new (IssueType.Semantic, "foo issue", new Position(new Point(1, 2), new Point(1, 4))),
                new (IssueType.Syntatic, "bar issue", new Position(new Point(2, 3), new Point(3, 5)))
            },
            Root = new CompilationUnit
            {
                Statements = new List<Statement>
                {
                    new DisplayStatement
                    {
                        Expression = new StringLiteral
                        {
                            Value = "foo string"
                        }
                    }
                }
            }
        };

        AssertParsingResultsAreEqual(result1, result1);
    }
    
    [TestMethod]
    public void TestDifferentParsingResults()
    {
        var result1 = new ParsingResult<CompilationUnit, ParserRuleContext>
        {
            Issues = new List<Issue>()
            {
                new (IssueType.Semantic, "foo issue", new Position(new Point(1, 2), new Point(1, 4))),
                new (IssueType.Syntatic, "bar issue", new Position(new Point(2, 3), new Point(3, 5)))
            },
            Root = new CompilationUnit
            {
                Statements = new List<Statement>
                {
                    new DisplayStatement
                    {
                        Expression = new StringLiteral
                        {
                            Value = "foo string"
                        }
                    }
                }
            }
        };
        var result2 = new ParsingResult<CompilationUnit, ParserRuleContext>
        {
            Issues = new List<Issue>()
            {
                new (IssueType.Semantic, "another issue", new Position(new Point(1, 2), new Point(1, 4))),
                new (IssueType.Syntatic, "different issue", new Position(new Point(2, 3), new Point(3, 5)))
            },
            Root = new CompilationUnit
            {
                Statements = new List<Statement>
                {
                    new DisplayStatement
                    {
                        Expression = new StringLiteral
                        {
                            Value = "foo string"
                        }
                    }
                }
            }
        };

        Assert.ThrowsException<ASTDifferenceException>(() => AssertParsingResultsAreEqual(result1, result2));
    }
    
    [TestMethod]
    public void TestASTsIdentity()
    {
        var ast1 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                },
                new SetStatement
                {
                    Expression = new AdditionExpression()
                    {
                        Left = new IntLiteral()
                        {
                            Value = "4"
                        },
                        Right = new IntLiteral()
                        {
                            Value = "1"
                        }
                    }
                }
            }
        };

        AssertASTsAreEqual(ast1, ast1);
    }
    
    [TestMethod]
    public void TestEqualASTs()
    {
        var ast1 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                },
                new SetStatement
                {
                    Expression = new AdditionExpression()
                    {
                        Left = new IntLiteral()
                        {
                            Value = "4"
                        },
                        Right = new IntLiteral()
                        {
                            Value = "1"
                        }
                    }
                }
            }
        };
        var ast2 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                },
                new SetStatement
                {
                    Expression = new AdditionExpression()
                    {
                        Left = new IntLiteral()
                        {
                            Value = "4"
                        },
                        Right = new IntLiteral()
                        {
                            Value = "1"
                        }
                    }
                }
            }
        };

        AssertASTsAreEqual(ast1, ast2);
    }

    [TestMethod]
    public void TestAssertASTsWithDifferentNodes()
    {
        var ast1 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                }
            }
        };
        var ast2 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new SetStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                }
            }
        };

        Assert.ThrowsException<ASTDifferenceException>(() => AssertASTsAreEqual(ast1, ast2));
    }

    [TestMethod]
    public void TestAssertASTsWithSameNodeTypesButDifferentNodeValues()
    {
        var ast1 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                },
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "bar"
                    }
                }
            }
        };
        var ast2 = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new StringLiteral
                    {
                        Value = "foo"
                    }
                },
                new SetStatement
                {
                    Expression = new AdditionExpression()
                    {
                        Left = new IntLiteral()
                        {
                            Value = "2"
                        },
                        Right = new IntLiteral()
                        {
                            Value = "1"
                        }
                    }
                }
            }
        };

        Assert.ThrowsException<ASTDifferenceException>(() => AssertASTsAreEqual(ast1, ast2));
    }

    [TestMethod]
    public void TestTwoDifferentNodesWithIgnoreChildren()
    {
        AssertASTsAreEqual(
            new CompilationUnit
            {
                Statements = new IgnoreChildren<Statement>()
            },
            new CompilationUnit
            {
                Statements = new List<Statement>
                {
                    new DisplayStatement
                    {
                        Expression = new StringLiteral { Value = "foo" }
                    },
                    new SetStatement
                    {
                        Expression = new StringLiteral { Value = "bar" }
                    }
                }
            });
    }

    [TestMethod]
    public void CheckRequire()
    {
        Assert.ThrowsException<InvalidOperationException>(() => Require(false));
        Assert.ThrowsException<InvalidOperationException>(() => Require(false, () => "Hello. I'm an error"));
    }
}