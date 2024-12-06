using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Tests.CodeGeneration;

// This file contains the AST definition for the Kotlin language
// It is partially based on the specifications https://kotlinlang.org/docs/reference/grammar.html
// This could be potentially moved to a separate project in the future

// AST definition for the Kotlin language

public class KCompilationUnit : Node
{
    public KCompilationUnit(KPackageDecl packageDecl, 
        List<KImport> imports, 
        List<KTopLevelDeclaration> elements)
    {
        PackageDecl = packageDecl;
        Imports = imports;
        Elements = elements;
    }

    public KPackageDecl? PackageDecl { get; set; }
    public List<KImport> Imports { get; } = new();
    public List<KTopLevelDeclaration> Elements { get; } = new();
}

public class KImport : Node
{
    public string Imported { get; }

    public KImport(string imported)
    {
        Imported = imported;
    }
}

public class KPackageDecl : Node
{
    public string Name { get; set; }

    public KPackageDecl(string name)
    {
        Name = name;
    }
}

public abstract class KTopLevelDeclaration : Node { }

public class KClassDeclaration : KTopLevelDeclaration, Named
{
    public string Name { get; set; }
    public bool DataClass { get; set; }
    public bool IsSealed { get; set; }
    public bool IsAbstract { get; set; }
    public KPrimaryConstructor PrimaryConstructor { get; set; } = new();
    public List<KSuperTypeInvocation> SuperTypes { get; } = new();

    public KClassDeclaration(string name)
    {
        Name = name;
    }
}

public class KTopLevelFunction : KTopLevelDeclaration, Named
{
    public string Name { get; set; }
    public List<KParameterDeclaration> Params { get; } = new();
    public KType? ReturnType { get; set; }
    public List<KStatement> Body { get; } = new();

    public KTopLevelFunction(string name)
    {
        Name = name;
    }
}

public class KExtensionMethod : KTopLevelDeclaration, Named
{
    public KName ExtendedClass { get; }
    public string Name { get; set; }
    public List<KParameterDeclaration> Params { get; } = new();
    public KType? ReturnType { get; set; }
    public List<KStatement> Body { get; } = new();

    public KExtensionMethod(KName extendedClass, string name)
    {
        ExtendedClass = extendedClass;
        Name = name;
    }
}

public abstract class KStatement : Node { }

public class KExpressionStatement : KStatement
{
    public KExpression Expression { get; }

    public KExpressionStatement(KExpression expression)
    {
        Expression = expression;
    }
}

public class KReturnStatement : KStatement
{
    public KExpression? Value { get; }

    public KReturnStatement(KExpression? value = null)
    {
        Value = value;
    }
}

public class KWhenStatement : KExpression
{
    public KExpression? Subject { get; set; }
    public List<KWhenClause> WhenClauses { get; } = new();
    public KElseClause? ElseClause { get; set; }
}

public class KWhenClause : Node
{
    public KExpression Condition { get; }
    public KStatement Body { get; }

    public KWhenClause(KExpression condition, KStatement body)
    {
        Condition = condition;
        Body = body;
    }
}

public class KElseClause : Node
{
    public KStatement Body { get; }

    public KElseClause(KStatement body)
    {
        Body = body;
    }
}

public class KThrowStatement : KStatement
{
    public KExpression Exception { get; }

    public KThrowStatement(KExpression exception)
    {
        Exception = exception;
    }
}

public abstract class KExpression : Node { }

public class KThisExpression : KExpression { }

public class KReferenceExpr : KExpression
{
    public string Symbol { get; }

    public KReferenceExpr(string symbol)
    {
        Symbol = symbol;
    }
}

public class KStringLiteral : KExpression
{
    public string Value { get; }

    public KStringLiteral(string value)
    {
        Value = value;
    }
}

public class KIntLiteral : KExpression
{
    public int Value { get; }

    public KIntLiteral(int value)
    {
        Value = value;
    }
}

public class KPlaceholderExpr : KExpression
{
    public string? Name { get; set; }

    public KPlaceholderExpr(string? name = null)
    {
        Name = name;
    }
}

public class KUniIsExpression : KExpression
{
    public KType KType { get; }

    public KUniIsExpression(KType ktype)
    {
        KType = ktype;
    }
}

public class KMethodCallExpression : KExpression
{
    public KExpression Qualifier { get; }
    public ReferenceByName<KMethodSymbol> Method { get; }
    public List<KExpression> Args { get; } = new();
    public KLambda? Lambda { get; set; }

    public KMethodCallExpression(KExpression qualifier, ReferenceByName<KMethodSymbol> method, 
        List<KExpression> args)
    {
        Qualifier = qualifier;
        Method = method;
        Args = args;
    }
    
    public KMethodCallExpression(KExpression qualifier, ReferenceByName<KMethodSymbol> method)
    {
        Qualifier = qualifier;
        Method = method;
        Args = new List<KExpression>();
    }
    
}

public class KFieldAccessExpr : KExpression
{
    public KExpression Qualifier { get; }
    public string Field { get; }

    public KFieldAccessExpr(KExpression qualifier, string field)
    {
        Qualifier = qualifier;
        Field = field;
    }
}

public class KLambda : KExpression
{
    public List<KLambdaParamDecl> Params { get; } = new();
    public List<KStatement> Body { get; } = new();
}

public class KLambdaParamDecl : Node, Named
{
    public string Name { get; set; }

    public KLambdaParamDecl(string name)
    {
        Name = name;
    }
}

public class KInstantiationExpression : KExpression
{
    public KType Type { get; }
    public List<KParameterValue> Args { get; } = new();

    public KInstantiationExpression(KType type)
    {
        Type = type;
    }
}

public interface KFunctionSymbol : Named { }
public interface KMethodSymbol : Named { }

public class KFunctionCall : KExpression
{
    public ReferenceByName<KFunctionSymbol> Function { get; }
    public List<KParameterValue> Args { get; } = new();

    public KFunctionCall(ReferenceByName<KFunctionSymbol> function)
    {
        Function = function;
    }
}

public abstract class KName : Node
{
    public static KName FromParts(string firstPart, params string[] otherParts)
    {
        KName Helper(List<string> parts)
        {
            return parts.Count switch
            {
                1 => new KSimpleName(parts[0]),
                _ => new KQualifiedName(Helper(parts.GetRange(0, parts.Count - 1)), parts[^1])
            };
        }

        var allParts = new List<string> { firstPart };
        allParts.AddRange(otherParts);
        return Helper(allParts);
    }
}

public class KSimpleName : KName
{
    public string Name { get; set; }

    public KSimpleName(string name)
    {
        Name = name;
    }
}

public class KQualifiedName : KName
{
    public KName Container { get; }
    public string Name { get; set; }

    public KQualifiedName(KName container, string name)
    {
        Container = container;
        Name = name;
    }
}

public class KPrimaryConstructor : Node
{
    public List<KParameterDeclaration> Params { get; } = new();

    public KPrimaryConstructor() { }
}

public enum KPersistence
{
    VAL,
    VAR,
    NONE
}

public class KParameterDeclaration : Node, Named, KTyped
{
    public string Name { get; set; }
    public KType Type { get; }
    public KPersistence Persistence { get; }

    public KParameterDeclaration(string name, KType type, KPersistence persistence = KPersistence.NONE)
    {
        Name = name;
        Type = type;
        Persistence = persistence;
    }
}

public interface KTyped
{
    KType Type { get; }
}

public abstract class KType : Node { }

public class KRefType : KType
{
    public string Name { get; set; }
    public List<KType> Args { get; } = new();

    public KRefType(string name)
    {
        Name = name;
    }
}

public class KOptionalType : KType
{
    public KType Base { get; }

    public KOptionalType(KType baseType)
    {
        Base = baseType;
    }
}

public class KSuperTypeInvocation : Node
{
    public string Name { get; set; }

    public KSuperTypeInvocation(string name)
    {
        Name = name;
    }
}

public class KObjectDeclaration : KTopLevelDeclaration, Named
{
    public string Name { get; set; }

    public KObjectDeclaration(string name)
    {
        Name = name;
    }
}

public class KFunctionDeclaration : KTopLevelDeclaration, Named
{
    public string Name { get; set; }

    public KFunctionDeclaration(string name)
    {
        Name = name;
    }
}

public class KParameterValue : Node
{
    public KExpression Value { get; }
    public string Name { get; }

    public KParameterValue(KExpression value, string name = null)
    {
        Value = value;
        Name = name;
    }
}
