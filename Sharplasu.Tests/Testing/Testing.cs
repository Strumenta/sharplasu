using Antlr4.Runtime;
using LionWeb.Core;
using LionWeb.Core.M2;
using LionWeb.Core.M3;
using LionWeb.Core.Serialization;
using LionWeb.Core.VersionSpecific.V2023_1;
using LionWeb.Generator;
using LionWeb.Generator.GeneratorExtensions;
using LionWeb.Generator.Names;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Tests.Models.SimpleLang;
using Strumenta.Sharplasu.Validation;
using System.Reflection;
using static Strumenta.Sharplasu.Testing.Asserts;
using Statement = Strumenta.Sharplasu.Tests.Models.SimpleLang.Statement;

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

        Assert.ThrowsExactly<ASTDifferenceException>(() => AssertParsingResultsAreEqual(result1, result2));
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

        Assert.ThrowsExactly<ASTDifferenceException>(() => AssertASTsAreEqual(ast1, ast2));
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

        Assert.ThrowsExactly<ASTDifferenceException>(() => AssertASTsAreEqual(ast1, ast2));
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
    public void TestTwoDifferentNodesWithOneNullProperty()
    {
        var ast1 = new CompilationUnit
        {
            Statements = new List<Statement>
                {
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
                                Value = "5"
                            }
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
                        Expression = new AdditionExpression()
                        {
                            Left = new IntLiteral()
                            {
                                Value = "4"
                            },
                            Right = null
                        }
                    }
                }
        };
        Assert.ThrowsExactly<ASTDifferenceException>(() => AssertASTsAreEqual(ast1, ast2));
    }

    [TestMethod]
    public void CheckRequire()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Require(false));
        Assert.ThrowsExactly<InvalidOperationException>(() => Require(false, () => "Hello. I'm an error"));
    }

    DynamicLanguage[] DeserializeExternalLanguage(string location,
    params Language[] dependentLanguages)
    {
        SerializationChunk serializationChunk =
            JsonUtils.ReadJsonFromString<SerializationChunk>(
                File.ReadAllText(location));
        return new LanguageDeserializerBuilder()
            .WithLionWebVersion(LionWebVersions.v2023_1)
            .WithCompressedIds(new(KeepOriginal: true))
            .Build()
            .Deserialize(serializationChunk, dependentLanguages).ToArray();
    }

    [TestMethod]
    public void CheckGeneration()
    {
        var language = DeserializeExternalLanguage("../../../ast.language.v2.json", 
            LionWebVersions.v2023_1.BuiltIns).First();
        
        Names ourName = new Names(language, "ns")
        {
            //{ aLang.FindByKey<PrimitiveType>("key-AType"), typeof(CustomType)},
            PrimitiveTypeMappings = {
                {   LionWebVersions.v2023_1.BuiltIns.FindByKey<PrimitiveType>("LionCore-builtins-String"), typeof(String) }
            }
        };

        var generator = new GeneratorFacade { Names = ourName, LionWebVersion = LionWebVersions.v2023_1 };

        using (StreamWriter writer = File.CreateText("../../../ast.cs"))
        {
            generator.Generate().WriteTo(writer);
        }
        generator.Persist("../../../ast2.cs");
        /*using (var stream = File.Open("../../../ast.language.v2.json", FileMode.Open))
        {
            var language = JsonUtils.ReadNodesFromStream(stream,
            new LanguageDeserializerBuilder().WithLionWebVersion(LionWebVersions.v2023_1).Build())
            .Cast<DynamicLanguage>().First();
            language.AddDependsOn(new List<Language>() { BuiltInsLanguage_2023_1.Instance.GetLanguage() });
            var settings = new GeneratorConfig() { UnresolvedReferenceHandling = UnresolvedReferenceHandling.ReturnAsNull };
           
            // Map the LionCore universal types to C# syntax            
            var generator = new GeneratorFacade { Names = new Names(language, "ns"), Config = settings };
           
            using (StreamWriter writer = File.CreateText("../../../ast.cs"))
            {
                generator.Generate();
            }            
        }        */
    }
}