using ExtensionMethods;
using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strumenta.Sharplasu.Model
{
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

        public string Message
        {
            get { return Message; }
            private set { Message = value; }
        }
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
