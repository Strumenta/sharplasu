using System;
using System.Collections.Generic;
using System.IO;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.CodeGeneration
{
/// <summary>
/// Transforms an AST into code. This can be done on an AST obtained from parsing, or built programmatically.
/// It also works on ASTs obtained from parsing and then modified. It does not perform lexical preservation: comments
/// are lost, and whitespace is reorganized, effectively equivalent to auto-formatting.
/// 
/// The logic for printing the different elements of the language must be defined in subclasses. This logic could
/// potentially be expressed in a DSL, with multi-platform generators, permitting code generators usable across all
/// StarLasu platforms.
/// </summary>
public abstract class ASTCodeGenerator<R> where R : Node
{
    protected readonly Dictionary<Type, INodePrinter> NodePrinters = new Dictionary<Type, INodePrinter>();

    protected virtual INodePrinter PlaceholderNodePrinter => null;

    public Func<Node, INodePrinter> NodePrinterOverrider { get; set; } = _ => null;

    protected ASTCodeGenerator()
    {
        RegisterRecordPrinters();
    }

    /// <summary>
    /// Subclasses should define record printers by overriding this method.
    /// </summary>
    protected abstract void RegisterRecordPrinters();

    /// <summary>
    /// Registers a printer for a specific node type.
    /// </summary>
    /// <typeparam name="N1">The type of the node.</typeparam>
    /// <param name="generation">The generation logic for this node type.</param>
    protected void RecordPrinter<N1>(Action<PrinterOutput, N1> generation) where N1 : Node
    {
        NodePrinters[typeof(N1)] = new ActionNodePrinter<N1>(generation);
    }

    /// <summary>
    /// Prints the AST to a string.
    /// </summary>
    /// <param name="ast">The root node of the AST.</param>
    /// <returns>A string representing the generated code.</returns>
    public string PrintToString(R ast)
    {
        var printerOutput = new PrinterOutput(NodePrinters, NodePrinterOverrider, PlaceholderNodePrinter);
        ConfigurePrinter(printerOutput);
        printerOutput.Print(ast, Prefix, Postfix);
        return printerOutput.Text();
    }

    /// <summary>
    /// Prints the AST to a file.
    /// </summary>
    /// <param name="root">The root node of the AST.</param>
    /// <param name="file">The file to write the generated code to.</param>
    public void PrintToFile(R root, FileInfo file)
    {
        File.WriteAllText(file.FullName, PrintToString(root));
    }

    /// <summary>
    /// Subclasses can override this method to configure the printer.
    /// </summary>
    /// <param name="printerOutput">The printer output to configure.</param>
    protected virtual void ConfigurePrinter(PrinterOutput printerOutput) { }

    /// <summary>
    /// A prefix to add before printing the AST.
    /// </summary>
    protected virtual string Prefix => "";

    /// <summary>
    /// A postfix to add after printing the AST.
    /// </summary>
    protected virtual string Postfix => "";
}

/// <summary>
/// A helper class to implement a node printer using a lambda.
/// </summary>
/// <typeparam name="T">The type of the node.</typeparam>
public class ActionNodePrinter<T> : INodePrinter where T : Node
{
    private readonly Action<PrinterOutput, T> _action;

    public ActionNodePrinter(Action<PrinterOutput, T> action)
    {
        _action = action;
    }

    public void Print(PrinterOutput output, Node node)
    {
        _action(output, (T)node);
    }
}
}