using Strumenta.Sharplasu.CodeGeneration;

namespace Strumenta.Sharplasu.Tests.CodeGeneration;

using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Transformation;

public class KotlinPrinter : ASTCodeGenerator<KCompilationUnit>
{
    protected override INodePrinter PlaceholderNodePrinter =>
        new ActionNodePrinter<Node>((output, ast) =>
        {
            var placeholder = (PlaceholderAstTransformation)ast.Origin!;
            output.Print($"/* {placeholder.Message} */");
        });

    protected override void RegisterRecordPrinters()
    {
        RecordPrinter<KCompilationUnit>((output, ast) =>
        {
            output.Print(ast.PackageDecl);
            output.PrintList("", ast.Imports, "\n", false, "");
            if (ast.Imports.Count > 0)
            {
                output.Println();
            }

            foreach (var element in ast.Elements)
            {
                output.Print(element);
            }
        });

        RecordPrinter<KPackageDecl>((output, ast) =>
        {
            output.Print("package ");
            output.Print(ast.Name);
            output.Println();
            output.Println();
        });

        RecordPrinter<KImport>((output, ast) =>
        {
            output.Print("import ");
            output.Print(ast.Imported);
            output.Println();
        });

        RecordPrinter<KClassDeclaration>((output, ast) =>
        {
            output.PrintFlag(ast.DataClass, "data ");
            output.PrintFlag(ast.IsAbstract, "abstract ");
            output.PrintFlag(ast.IsSealed, "sealed ");
            output.Print($"class {ast.Name}");
            output.Print(ast.PrimaryConstructor);
            if (ast.SuperTypes.Count > 0)
            {
                output.Print(": ");
                output.PrintList(ast.SuperTypes);
                output.Print(" ");
            }

            output.Println(" {");
            output.Println("}");
            output.Println();
        });

        RecordPrinter<KPrimaryConstructor>((output, ast) => { output.PrintList("(", ast.Params, ")"); });

        RecordPrinter<KSuperTypeInvocation>((output, ast) =>
        {
            output.Print(ast.Name);
            output.Print("()");
        });

        RecordPrinter<KParameterDeclaration>((output, ast) =>
        {
            switch (ast.Persistence)
            {
                case KPersistence.VAR:
                    output.Print("var ");
                    break;
                case KPersistence.VAL:
                    output.Print("val ");
                    break;
            }

            output.Print(ast.Name);
            output.Print(": ");
            output.Print(ast.Type);
        });

        RecordPrinter<KRefType>((output, ast) =>
        {
            output.Print(ast.Name);
            output.PrintList("<", ast.Args, ">");
        });

        RecordPrinter<KOptionalType>((output, ast) =>
        {
            output.Print(ast.Base);
            output.Print("?");
        });

        RecordPrinter<KExtensionMethod>((output, ast) =>
        {
            output.Print("fun ");
            output.Print(ast.ExtendedClass);
            output.Print(".");
            output.Print(ast.Name);
            output.PrintList("(", ast.Params, ")", printEvenIfEmpty: true);
            if (ast.ReturnType != null)
            {
                output.Print(": ");
                output.Print(ast.ReturnType);
            }

            output.Println(" {");
            output.Indent();
            output.PrintList(ast.Body, "\n");
            output.Dedent();
            output.Println("}");
            output.Println();
        });

        RecordPrinter<KQualifiedName>((output, ast) =>
        {
            output.Print(ast.Container);
            output.Print(".");
            output.Print(ast.Name);
        });

        RecordPrinter<KSimpleName>((output, ast) => { output.Print(ast.Name); });

        RecordPrinter<KExpressionStatement>((output, ast) =>
        {
            output.Print(ast.Expression);
            output.Println();
        });

        RecordPrinter<KFunctionCall>((output, ast) =>
        {
            output.Print(ast.Function.Name);
            output.PrintList("(", ast.Args, ")", printEvenIfEmpty: true);
        });

        RecordPrinter<KReturnStatement>((output, ast) =>
        {
            output.Print("return");
            output.Print(ast.Value, prefix: " ");
            output.Println();
        });

        RecordPrinter<KWhenStatement>((output, ast) =>
        {
            output.Print("when ");
            output.Print(ast.Subject, "(", ") ");
            output.Println("{");
            output.Indent();
            output.PrintList(ast.WhenClauses, "");
            output.Print(ast.ElseClause);
            output.Dedent();
            output.Println("}");
        });

        RecordPrinter<KWhenClause>((output, ast) =>
        {
            output.Print(ast.Condition);
            output.Print(" -> ");
            output.Print(ast.Body);
        });

        RecordPrinter<KElseClause>((output, ast) =>
        {
            output.Print("else -> ");
            output.Print(ast.Body);
        });

        RecordPrinter<KThisExpression>((output, ast) => { output.Print("this"); });

        RecordPrinter<KUniIsExpression>((output, ast) =>
        {
            output.Print("is ");
            output.Print(ast.KType);
        });

        RecordPrinter<KMethodCallExpression>((output, ast) =>
        {
            output.Print(ast.Qualifier);
            output.Print(".");
            output.Print(ast.Method.Name);
            output.Print("(");
            output.PrintList(ast.Args);
            output.Print(")");
            output.Print(ast.Lambda, " ");
        });

        RecordPrinter<KInstantiationExpression>((output, ast) =>
        {
            output.Print(ast.Type);
            output.Print("(");
            output.PrintList(ast.Args);
            output.Print(")");
        });

        RecordPrinter<KThrowStatement>((output, ast) =>
        {
            output.Print("throw ");
            output.Println(ast.Exception);
        });

        RecordPrinter<KFieldAccessExpr>((output, ast) =>
        {
            output.Print(ast.Qualifier);
            output.Print(".");
            output.Print(ast.Field);
        });

        RecordPrinter<KParameterValue>((output, ast) =>
        {
            output.Print(ast.Name, "", "=");
            output.Print(ast.Value);
        });

        RecordPrinter<KLambda>((output, ast) =>
        {
            output.Print("{");
            output.Indent();
            output.PrintList(ast.Body, separator: "");
            output.Dedent();
            output.Println("}");
        });

        RecordPrinter<KReferenceExpr>((output, ast) => { output.Print(ast.Symbol); });

        RecordPrinter<KStringLiteral>((output, ast) =>
        {
            output.Print("\"");
            output.Print(ast.Value);
            output.Print("\"");
        });

        RecordPrinter<KIntLiteral>((output, ast) => { output.Print(ast.Value); });

        RecordPrinter<KFunctionDeclaration>((output, ast) =>
        {
            output.Print("fun ");
            output.Print(ast.Name);
            output.Print("(");
            // TODO print parameters
            output.Println(") {");
            output.Indent();
            // TODO print body
            output.Dedent();
            output.Println("}");
        });
    }

    public string PrintToString(KExpression expression)
    {
        var output = new PrinterOutput(NodePrinters, NodePrinterOverrider);
        output.Print(expression);
        return output.Text();
    }
}