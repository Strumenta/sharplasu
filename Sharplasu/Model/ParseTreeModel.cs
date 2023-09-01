using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.Sharplasu.Model
{
    public abstract class ParseTreeElement
    {
        public abstract string MultiLineString(string indentation = "");
    }

    /**
     * Representation of the information contained in a Parse Tree terminal or leaf.
     */
    public class ParseTreeLeaf : ParseTreeElement
    {
        public string Type { get; private set;}
        public string Text { get; private set;}

        public ParseTreeLeaf(string type, string text)
        {
            Type = type;
            Text = text;
        }

        public override string ToString()
        {
            return $"T:{Type}[{Text}]";
        }

        public override string MultiLineString(string indentation)
            => $"{indentation}T:{Type}[{Text}]\n";
    }

    /**
     * Representation of the information contained in a Parse Tree terminal or leaf.
     */
    public class ParseTreeNode : ParseTreeElement
    {
        public string Name { get; private set; }
        public List<ParseTreeElement> Children { get; private set; } = new List<ParseTreeElement>();
        
        public ParseTreeNode(string name)
        {
            Name = name;
        }

        public ParseTreeNode Child(ParseTreeElement c)
        {
            Children.Add(c);
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            Children.ForEach(c =>
            {
                sb.Append(c.ToString());
            });
            sb.Append("]");
            return $"Node({Name} {sb.ToString()})";
        }

        public override string MultiLineString(string indentation)
        {
            var sb = new StringBuilder("[");
            sb.Append($"{indentation}{Name}\n");
            Children.ForEach(c =>
            {
                sb.Append(c.MultiLineString($"{indentation} "));
            });
            return sb.ToString();
        }
    }

    /**
    * Given an actual parse-tree produced by ANTLR, it creates a Parse Tree model.
    */
    public static class ParseTreeModelExtensions
    {
        internal static string RemoveSuffix(this string text, string suffix)
        {
            if (text.EndsWith(suffix, StringComparison.Ordinal))
                return text.Remove(text.Length - suffix.Length);
            else
                return text;
        }

        public static ParseTreeNode ToParseTreeModel(ParserRuleContext node, Vocabulary vocabulary)
        {
            var res = new ParseTreeNode(node.GetType().Name.RemoveSuffix("Context"));
            foreach (var c in node.children)
            {
                switch(c)
                {
                    case ParserRuleContext ruleContext:
                        res.Child(ToParseTreeModel(ruleContext, vocabulary)); 
                        break;
                    case ITerminalNode terminalNode:
                        res.Child(new ParseTreeLeaf(vocabulary.GetSymbolicName(terminalNode.Symbol.Type), terminalNode.GetText()));
                        break;
                }
            }
            return res;
        }
    }
}
