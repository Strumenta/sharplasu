using System.Collections.Generic;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Traversing
{
}

namespace ExtensionMethods
{
    public static class TraversingExtensions
    {
        /**
         * Traverse the entire tree, deep first, starting from this Node
         */
        public static IEnumerable<Node> walk(this Node node) {
            Stack<Node> stack = new Stack<Node>();
            stack.Push(node);

            if (stack.Count == 0) {
                yield return null;
            }
            else {
                var next = stack.Pop();
                next.Children.ForEach(child => stack.Push(child));
                yield return next;
            }
        }
    }
}