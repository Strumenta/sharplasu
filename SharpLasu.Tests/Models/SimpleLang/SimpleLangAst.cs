using System.Reflection.Metadata;
using Strumenta.Sharplasu.Model;

namespace Strumenta.SharpLasu.Tests.Models.SimpleLang;

public class CompilationUnit : Node
{
    public List<Statement> Statements { get; init; }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var o = (obj as CompilationUnit)!;
        return Statements.SequenceEqual(o.Statements);
    }
        
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (var str in Statements)
            {
                hash = hash * 23 + (str?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }
}

public abstract class Statement : Node {}

public class DisplayStatement : Statement
{
    public Expression Expression { get; init; }
}

public class SetStatement : Statement
{
    public Identifier Id { get; init; }
    public Expression Expression { get; init; }
}

public class InputStatement : Statement
{
    public Identifier Id { get; init; }
    public Type Type { get; init; }
}

public class Identifier : Node
{
    public string Name { get; init; }
}

public abstract class Expression : Node {}
public abstract class LiteralExpression : Expression
{
    public string Value { get; init; }
}
public class IntLiteral : LiteralExpression {}
public class DecLiteral : LiteralExpression {}
public class StringLiteral : LiteralExpression {}
public class BooleanLiteral: LiteralExpression {}

public abstract class BinaryExpression: Expression
{
    public Expression Left { get; init; }
    public Expression Right { get; init; }
}
public class AdditionExpression : BinaryExpression {}
public class SubtractionExpression : BinaryExpression {}
public class MultiplicationExpression : BinaryExpression {}
public class DivisionExpression : BinaryExpression {}

public abstract class Type : Node {}
public class IntType : Type {}
public class DecType : Type {}
public class StringType : Type {}
public class BooleanType : Type {}