using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Transformation;

namespace Strumenta.Sharplasu.CodeGeneration
{

    public class PrinterOutput
    {
        private readonly Dictionary<Type, INodePrinter> nodePrinters;
        private readonly Func<Node, INodePrinter> nodePrinterOverrider;
        private readonly INodePrinter placeholderNodePrinter;
        private readonly StringBuilder sb = new StringBuilder();
        private Point currentPoint = Point.START_POINT;
        private int indentationLevel = 0;
        private bool onNewLine = true;
        private string indentationBlock = "    ";
        private string newLineStr = "\n";

        public PrinterOutput(
            Dictionary<Type, INodePrinter> nodePrinters,
            Func<Node, INodePrinter> nodePrinterOverrider = null,
            INodePrinter placeholderNodePrinter = null)
        {
            this.nodePrinters = nodePrinters;
            this.nodePrinterOverrider = nodePrinterOverrider ?? (_ => null);
            this.placeholderNodePrinter = placeholderNodePrinter;
        }

        public string Text() => sb.ToString();

        public void Println() => Println("");

        public void Println(string text)
        {
            Print(text);
            sb.Append(newLineStr);
            currentPoint += newLineStr.Length;
            onNewLine = true;
        }

        public void PrintFlag(bool flag, string text)
        {
            if (flag)
            {
                Print(text);
            }
        }

        public void Print(string text)
        {
            Print(text, false);
        }

        public void Print(string text, bool allowMultiLine)
        {
            if (string.IsNullOrEmpty(text)) return;

            var adaptedText = text;
            var needPrintln = adaptedText.EndsWith("\n");
            if (needPrintln)
            {
                adaptedText = adaptedText.TrimEnd('\n');
            }

            ConsiderIndentation();
            if (adaptedText.Split('\n').Length > 1 && !allowMultiLine)
            {
                throw new ArgumentException($"Given text spans multiple lines: {adaptedText}");
            }

            sb.Append(adaptedText);
            currentPoint += adaptedText.Length;

            if (needPrintln)
            {
                Println();
            }
        }

        public void Print(char text) => Print(text.ToString());

        public void Print(int value) => Print(value.ToString());

        private void ConsiderIndentation()
        {
            if (onNewLine)
            {
                onNewLine = false;
                sb.Append(new string(' ', indentationLevel * indentationBlock.Length));
            }
        }

        public void Print(string text, string prefix = "", string postfix = "")
        {
            if (text == null) return;
            Print(prefix);
            Print(text);
            Print(postfix);
        }

        private INodePrinter FindPrinter(Node ast, Type type)
        {
            var overrider = nodePrinterOverrider(ast);
            if (overrider != null) return overrider;

            if (ast.Origin is PlaceholderASTTransformation && placeholderNodePrinter != null)
            {
                return placeholderNodePrinter;
            }

            if (nodePrinters.TryGetValue(type, out var printer))
            {
                return printer;
            }

            var baseType = type.BaseType;
            return baseType != null ? FindPrinter(ast, baseType) : null;
        }

        private INodePrinter GetPrinter(Node ast, Type type = null)
        {
            type = ast.GetType();
            var printer = FindPrinter(ast, type);
            return printer ?? throw new ArgumentException($"Unable to print {ast}");
        }

        public void Print(Node ast, string prefix = "", string postfix = "")
        {
            if (ast == null) return;

            Print(prefix);
            var printer = GetPrinter(ast);
            Associate(ast, () => printer.Print(this, ast));
            Print(postfix);
        }

        public void Println(Node ast, string prefix = "", string postfix = "")
        {
            Print(ast, prefix, postfix + "\n");
        }

        public void PrintEmptyLine()
        {
            Println();
            Println();
        }

        public void Indent() => indentationLevel++;

        public void Dedent() => indentationLevel--;

        public void Associate(Node ast, Action generation)
        {
            var startPoint = currentPoint;
            generation();
            var endPoint = currentPoint;
            ast.Destination = new TextFileDestination(new Position(startPoint, endPoint));
        }

        public void PrintList<T>(IList<T> elements, string separator = ", ", Action<T> elementPrinter = null) where T : Node
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (i > 0) Print(separator);
                elementPrinter(elements[i]);
            }
        }

        public void PrintList<T>(string prefix, IList<T> elements, string postfix, bool printEvenIfEmpty = false, string separator = ", ", Action<T> elementPrinter = null) where T : Node
        {
            if (elements.Count > 0 || printEvenIfEmpty)
            {
                Print(prefix);
                PrintList(elements, separator, elementPrinter);
                Print(postfix);
            }
        }

        public void PrintOneOf(params Node[] alternatives)
        {
            var notNull = alternatives.Where(x => x != null).ToList();
            if (notNull.Count != 1)
            {
                throw new InvalidOperationException($"Expected exactly one alternative to be not null. Not null alternatives: {string.Join(", ", notNull)}");
            }
            Print(notNull.First());
        }
    }
}