using Strumenta.Sharplasu.CodeGeneration;

namespace Strumenta.Sharplasu.Tests.CodeGeneration;

using System.Collections.Generic;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Transformation;

[TestClass]
public class ASTCodeGeneratorTest
{
    [TestMethod]
    public void PrintSimpleKotlinExpression()
    {
        var ex = new KMethodCallExpression(
            new KThisExpression(),
            new ReferenceByName<KMethodSymbol>("myMethod"),
            new List<KExpression>
            {
                new KStringLiteral("abc"),
                new KIntLiteral(123),
                new KStringLiteral("qer")
            }
        );

        var code = new KotlinPrinter().PrintToString(ex);
        Assert.AreEqual(@"this.myMethod(""abc"", 123, ""qer"")", code);
    }

    [TestMethod]
    public void PrintSimpleFile()
    {
        var cu = new KCompilationUnit(
            new KPackageDecl("my.splendid.packag"),
            new List<KImport> { new KImport("my.imported.stuff") },
            new List<KTopLevelDeclaration> { new KFunctionDeclaration("foo") }
        );

        var code = new KotlinPrinter().PrintToString(cu);
        Assert.AreEqual(
            @"package my.splendid.packag
|
import my.imported.stuff
|
|
fun foo() {
}
".Replace("|", ""), code);
    }

    [TestMethod]
    public void PrintUsingNodePrinterOverrider()
    {
        var ex = new KMethodCallExpression(
            new KThisExpression(),
            new ReferenceByName<KMethodSymbol>("myMethod"),
            new List<KExpression>
            {
                new KStringLiteral("abc"),
                new KIntLiteral(123),
                new KStringLiteral("qer")
            }
        );

        var code = new KotlinPrinter().PrintToString(ex);
        Assert.AreEqual(@"this.myMethod(""abc"", 123, ""qer"")", code);

        var printer = new KotlinPrinter();
        printer.NodePrinterOverrider = node =>
        {
            return node switch
            {
                KStringLiteral => new ActionNodePrinter<KStringLiteral>((output, ast) => output.Print("YYY")),
                KIntLiteral => new ActionNodePrinter<KIntLiteral>((output, ast) => output.Print("XXX")),
                _ => null
            };
        };

        var codeWithNodePrinterOverrider = printer.PrintToString(ex);
        Assert.AreEqual(@"this.myMethod(YYY, XXX, YYY)", codeWithNodePrinterOverrider);
    }

    [TestMethod]
    public void PrintUntranslatedNodes()
    {
        var failedNode = new KImport("my.imported.stuff");
        failedNode.Origin = new MissingASTTransformation(failedNode);

        var cu = new KCompilationUnit(
            new KPackageDecl("my.splendid.packag"),
            new List<KImport> { failedNode },
            new List<KTopLevelDeclaration> { new KFunctionDeclaration("foo") }
        );

        var code = new KotlinPrinter().PrintToString(cu);
        Assert.AreEqual(
            @"package my.splendid.packag
|
/* Translation of a node is not yet implemented: KImport */
|
fun foo() {
}
".Replace("|", ""), code);
    }

    [TestMethod]
    public void PrintTransformationFailure()
    {
        var failedNode = new KImport("my.imported.stuff")
        {
            Origin = new FailingASTTransformation(null, "Something made BOOM!")
        };

        var cu = new KCompilationUnit(
            new KPackageDecl("my.splendid.packag"),
            new List<KImport> { failedNode },
            new List<KTopLevelDeclaration> { new KFunctionDeclaration("foo") }
        );

        var code = new KotlinPrinter().PrintToString(cu);
        Assert.AreEqual(
            @"package my.splendid.packag
|
/* Something made BOOM! */
|
fun foo() {
}
".Replace("|", ""), code);
    }
}