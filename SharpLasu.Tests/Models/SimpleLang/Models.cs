namespace Strumenta.SharpLasu.Tests.Models.SimpleLang;

public static class Models
{
    public static CompilationUnit GetCompilationUnit()
    {
        return new CompilationUnit
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
    }
}