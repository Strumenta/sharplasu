using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.Sharplasu.Parsing
{
    public abstract class ParseTreeElement
    {
        public abstract string MultiLineString(string indentation = "");
    }

    public class ParseTreeLeaf : ParseTreeElement
    {
        private string type;
        private string text;

        public ParseTreeLeaf(string type, string text)
        {
            this.type = type;
            this.text = text;
        }

        public override string ToString()
        {
            return $"T:{type}[{text}]";
        }

        public override string MultiLineString(string indentation = "") => $"{indentation}T:{type}[{text}]\n";
    }

    public class ParseTreeNode : ParseTreeElement
    {
        private string name;
        private List<ParseTreeElement> children = new List<ParseTreeElement>();

        public ParseTreeNode(string name)
        {
            this.name = name;
        }

        public ParseTreeNode Child(ParseTreeElement c)
        {
            children.Add(c);
            return this;
        }

        public override string ToString()
        {
            return $"Node({name}) {children}";
        }

        public override string MultiLineString(string indentation = "")
        {
            var sb = new StringBuilder();
            sb.Append($"{indentation}{name}\n");

            children.ForEach(c => sb.Append(c.MultiLineString(indentation + "  ")));

            return sb.ToString();
        }
    }
}
