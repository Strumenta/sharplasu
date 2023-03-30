using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        /**
         * Performs a post-order (or leaves-first) node traversal starting with a given node.
         */
        public static IEnumerable<Node> WalkLeavesFirst(this Node node)
        {
            var nodesStack = new Stack<List<Node>>();
            var cursorStack = new Stack<int>();
            var done = false;

            Node NextFromLevel()
            {
                var nodes = nodesStack.Peek();
                var cursor = cursorStack.Pop();
                cursorStack.Push(cursor + 1);
                return nodes[cursor];
            }

            void FillStackToLeaf(Node node)
            {
                var currentNode = node;
                while (true)
                {
                    var childNodes = currentNode.Children;
                    if (childNodes.Count == 0)
                        break;
                    nodesStack.Push(childNodes);
                    cursorStack.Push(0);
                    currentNode = childNodes[0];
                }
            }
            
            FillStackToLeaf(node);

            while (!done)
            {
                var nodes = nodesStack.Peek();
                var cursor = cursorStack.Peek();
                var levelHasNext = cursor < nodes.Count;
                if (levelHasNext)
                {
                    var n = nodes[cursor];
                    FillStackToLeaf(n);
                    yield return NextFromLevel();
                }
                else
                {
                    nodesStack.Pop();
                    cursorStack.Pop();
                    var hasNext = nodesStack.Count > 0;
                    if (hasNext)
                    {
                        yield return NextFromLevel();
                    }
                    else
                    {
                        done = true;
                        yield return node;
                    }
                }
            }
        }

        /**
         * @return the sequence of nodes from this.parent all the way up to the root node.
         * For this to work, assignParents() must have been called.
         */
        public static IEnumerable<Node> WalkAncestors(this Node node)
        {
            var currentNode = node;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                yield return currentNode;
            }
        }

        /**
         * @return all direct children of this node.
         */
        public static IEnumerable<Node> WalkChildren(this Node node)
        {
            // TODO: check this implementation against the one in Kolasu
            foreach (var child in node.Children)
            {
                yield return child;
            }
        }

        /**
         * @param walker a function that generates a sequence of nodes. By default this is the depth-first "walk" method.
         * For post-order traversal, take "walkLeavesFirst"
         * @return walks the whole AST starting from the childnodes of this node.
         */
        public static IEnumerable<Node> WalkDescendants(this Node node, Func<Node, IEnumerable<Node>> walker)
        {
            return walker.Invoke(node).Where(n => n != node);
        }

        public static IEnumerable<Node> WalkDescendants(this Node node)
        {
            return WalkDescendants(node, Walk);
        }

        public static IEnumerable<Node> WalkDescendants(this Node node, Type type, Func<Node, IEnumerable<Node>> walker)
        {
            return WalkDescendants(node, walker).Where(type.IsInstanceOfType);
        }

        public static IEnumerable<Node> WalkDescendants(this Node node, Type type)
        {
            return WalkDescendants(node, type, Walk);
        }

        /**
         * Note that type T is not strictly forced to be a Node. This is intended to support
         * interfaces like `Statement` or `Expression`. However, being an ancestor the returned
         * value is guaranteed to be a Node, as only Node instances can be part of the hierarchy.
         *
         * @return the nearest ancestor of this node that is an instance of klass.
         */
        public static T FindAncestorOfType<T>(this Node node) where T : Node
        {
            return (T) node.WalkAncestors().FirstOrDefault(n => n is T);
        }
    }
}