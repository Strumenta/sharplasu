namespace Strumenta.Sharplasu.Tests.Models.SimpleLang;
using Strumenta.Sharplasu.Model;

public static class Models
{
    public static CompilationUnit GetCompilationUnit()
    {
        var cu = new CompilationUnit
        {
            Statements = new List<Statement>
            {
                new DisplayStatement
                {
                    Expression = new BooleanLiteral { Value = "true" }
                },
                new SetStatement
                {
                    Id = new Identifier { Name = "foo" },
                    Expression = new StringLiteral
                    {
                        Value = "bar"
                    }
                }
            }
        };
        cu.AssignParents();
        return cu;
    }
}