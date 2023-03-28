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
        public static IEnumerable<Node> Walk(this Node node) {
            var stack = new Stack<Node>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                next.Children.ForEach(child => stack.Push(child));
                yield return next;
            }
        }
    }
}