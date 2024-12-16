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
        private readonly Dictionary<Type, INodePrinter> _nodePrinters;
        private readonly Func<Node, INodePrinter> _nodePrinterOverrider;
        private readonly INodePrinter _placeholderNodePrinter;
        private readonly StringBuilder _sb = new StringBuilder();
        private Point _currentPoint = Point.START_POINT;
        private int _indentationLevel = 0;
        private bool _onNewLine = true;
        private const string _indentationBlock = "    ";
        private const string _newLineStr = "\n";
        private const char _newLineChar = '\n';

        public PrinterOutput(
            Dictionary<Type, INodePrinter> nodePrinters,
            Func<Node, INodePrinter> nodePrinterOverrider = null,
            INodePrinter placeholderNodePrinter = null)
        {
            _nodePrinters = nodePrinters;
            _nodePrinterOverrider = nodePrinterOverrider ?? (_ => null);
            _placeholderNodePrinter = placeholderNodePrinter;
        }

        public string Text() => _sb.ToString();

        public void Println() => Println("");

        public void Println(string text)
        {
            Print(text);
            _sb.Append(_newLineStr);
            _currentPoint += _newLineStr.Length;
            _onNewLine = true;
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
            var needPrintln = adaptedText.EndsWith(_newLineStr);
            if (needPrintln)
            {
                adaptedText = adaptedText.TrimEnd(_newLineChar);
            }

            ConsiderIndentation();
            if (adaptedText.Split(_newLineChar).Length > 1 && !allowMultiLine)
            {
                throw new ArgumentException($"Given text spans multiple lines: {adaptedText}");
            }

            _sb.Append(adaptedText);
            _currentPoint += adaptedText.Length;

            if (needPrintln)
            {
                Println();
            }
        }

        public void Print(char text) => Print(text.ToString());

        public void Print(int value) => Print(value.ToString());

        private void ConsiderIndentation()
        {
            if (!_onNewLine) return;
            _onNewLine = false;
            _sb.Append(new string(' ', _indentationLevel * _indentationBlock.Length));
        }

        public void Print(string text, string prefix = "", string postfix = "")
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (postfix == null) throw new ArgumentNullException(nameof(postfix));
            if (text == null) return;
            Print(prefix);
            Print(text);
            Print(postfix);
        }

        private INodePrinter FindPrinter(Node ast, Type type)
        {
            var overrider = _nodePrinterOverrider(ast);
            if (overrider != null) return overrider;

            if (ast.Origin is PlaceholderAstTransformation && _placeholderNodePrinter != null)
            {
                return _placeholderNodePrinter;
            }

            if (_nodePrinters.TryGetValue(type, out var printer))
            {
                return printer;
            }

            var baseType = type.BaseType;
            return baseType != null ? FindPrinter(ast, baseType) : null;
        }

        private INodePrinter GetPrinter(Node ast, Type type = null)
        {
            type = type ?? ast.GetType();
            var printer = FindPrinter(ast, type);
            return printer ?? throw new ArgumentException($"Unable to print {ast}");
        }

        public void Print(Node ast, string prefix = "", string postfix = "")
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (postfix == null) throw new ArgumentNullException(nameof(postfix));
            if (ast == null) return;

            Print(prefix);
            var printer = GetPrinter(ast);
            Associate(ast, () => printer.Print(this, ast));
            Print(postfix);
        }

        public void Println(Node ast, string prefix = "", string postfix = "")
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (postfix == null) throw new ArgumentNullException(nameof(postfix));

            Print(ast, prefix, postfix + _newLineStr);
        }

        public void PrintEmptyLine()
        {
            Println();
            Println();
        }

        public void Indent() => _indentationLevel++;

        public void Dedent() => _indentationLevel--;

        public void Associate(Node ast, Action generation)
        {
            if (ast == null) throw new ArgumentNullException(nameof(ast));
            if (generation == null) throw new ArgumentNullException(nameof(generation));

            var startPoint = _currentPoint;
            generation();
            var endPoint = _currentPoint;
            ast.Destination = new TextFileDestination(new Position(startPoint, endPoint));
        }

        public void PrintList<T>(IList<T> elements, string separator = ", ") where T : Node
        {
            Action<T> elementPrinter = (node) => Print(node);
            PrintList(elements, elementPrinter, separator: separator);
        }

        public void PrintList<T>(IList<T> elements, Action<T> elementPrinter, string separator = ", ") where T : Node
        {
            for (var i = 0; i < elements.Count; i++)
            {
                if (i > 0) Print(separator);
                elementPrinter(elements[i]);
            }
        }

        public void PrintList<T>(string prefix, IList<T> elements, string postfix, bool printEvenIfEmpty = false,
            string separator = ", ") where T : Node
        {
            Action<T> elementPrinter = (node) => Print(node);
            PrintList(prefix, elements, postfix, elementPrinter, printEvenIfEmpty, separator);
        }

        public void PrintList<T>(string prefix, IList<T> elements, string postfix, Action<T> elementPrinter,
            bool printEvenIfEmpty = false, string separator = ", ") where T : Node
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (postfix == null) throw new ArgumentNullException(nameof(postfix));
            if (separator == null) throw new ArgumentNullException(nameof(separator));

            if (elements.Count <= 0 && !printEvenIfEmpty) return;

            Print(prefix);
            PrintList(elements, elementPrinter, separator);
            Print(postfix);
        }

        public void PrintOneOf(params Node[] alternatives)
        {
            var notNull = alternatives.Where(x => x != null).ToList();
            if (notNull.Count != 1)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one alternative to be not null. Not null alternatives: {string.Join(", ", notNull)}");
            }

            Print(notNull.First());
        }
    }
}