using ExtensionMethods;
using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strumenta.Sharplasu.Model
{
    /**
     * An AST node that marks the presence of an error, for example a syntactic 
     * or semantic error in the original tree.
     */
    public interface ErrorNode
    {
        string Message { get; }
        Position Position { get; }
    }

    public class GenericErrorNode : Node, ErrorNode
    {
        public GenericErrorNode(Exception error = null, string message = null)
        {
            if (message != null)
            {
                this.Message = message;
            }
            else if (error != null)
            {
                var msg = "";
                if (error.Message != null)
                {
                    msg = ": " + error.Message;
                    this.Message = $"Exception {error.GetType().FullName}{msg}";
                }
                else
                {
                    this.Message = "Unspecified error node";
                }
            }
        }

        public string Message { get; private set; }
    }

    public static class ErrorsClass
    { 
        public static IEnumerable<ErrorNode> Errors(this Node node)
        {
            return node.WalkDescendants<ErrorNode>() as IEnumerable<ErrorNode>;
        }

        public static ErrorNode FindError(this Node node)
        {
            return node.Errors().FirstOrDefault();
        }
    }
}
