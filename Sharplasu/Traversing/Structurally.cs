using Strumenta.Sharplasu.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Strumenta.Sharplasu.Traversing
{
    public static class StructurallyExtensions
    {
        /// <summary>
        /// Traverse the entire tree, deep first, starting from this Node
        /// </summary>
        public static IEnumerable<Node> Walk(this Node node)
        {
            var stack = new Stack<Node>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                stack.PushAll(next.Children());
                yield return next;
            }
        }
        
        /// <returns>all direct children of this node.</returns>
        public static List<Node> Children(this Node node)
        {
            return node.WalkChildren().ToList();
        }

        /// <returns>all direct children of this node.</returns>
        public static IEnumerable<Node> WalkChildren(this Node node)
        {
            List<Node> children = new List<Node>();
            node.Properties.ForEach(property =>
            {
                var value = property.Value;
                if (value is Node)
                    children.Add(value as Node);
                else if (value?.IsACollection() == true)
                {
                    foreach (var item in value as IList)
                    {
                        if (item is Node)
                            children.Add(item as Node);
                    }
                }
                                    
            });
            return children.AsEnumerable<Node>();
        }

        /// <summary>
        /// Performs a post-order (or leaves-first) node traversal starting with a given node.
        /// </summary>
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

            void FillStackToLeaf(Node n)
            {
                var currentNode = n;
                while (true)
                {
                    var childNodes = currentNode.Children();
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

        /// <returns>
        /// the sequence of nodes from this.parent all the way up to the root node.
        /// For this to work, <c>assignParents()</c> must have been called.
        /// </returns>
        public static IEnumerable<Node> WalkAncestors(this Node node)
        {
            var currentNode = node;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                yield return currentNode;
            }
        }

        /// <param name="walker">a function that generates a sequence of nodes. By default this is the depth-first <c>walk</c> method.
        /// For post-order traversal, take <c>walkLeavesFirst</c></param>
        /// <returns>walks the whole AST starting from the childnodes of this node.</returns>
        public static IEnumerable<Node> WalkDescendants(this Node node, Func<Node, IEnumerable<Node>> walker)
        {
            return walker.Invoke(node).Where(n => n != node);
        }

        public static IEnumerable<Node> WalkDescendants(this Node node)
        {
            return WalkDescendants(node, Walk);
        }

        public static IEnumerable<Node> WalkDescendants<T>(this Node node, Func<Node, IEnumerable<Node>> walker)
        {
            return WalkDescendants(node, walker).Where(typeof(T).IsInstanceOfType);
        }

        public static IEnumerable<Node> WalkDescendants<T>(this Node node)
        {
            return WalkDescendants<T>(node, Walk);
        }

        /// <summary>
        /// <para>Note that type T is not strictly forced to be a Node. This is intended to support
        /// interfaces like <c>Statement</c> or <c>Expression</c>. However, being an ancestor the returned
        /// value is guaranteed to be a Node, as only Node instances can be part of the hierarchy.</para>        
        /// </summary>
        /// <returns>the nearest ancestor of this node that is an instance of klass.</returns>
        public static T FindAncestorOfType<T>(this Node node) where T : Node
        {
            return (T)node.WalkAncestors().FirstOrDefault(n => n is T);
        }

        public static IEnumerable<T> SearchByType<T>(this Node node, Func<Node, IEnumerable<Node>> walker)
        {
            return walker.Invoke(node).Where(typeof(T).IsInstanceOfType).Cast<T>();
        }

        public static IEnumerable<T> SearchByType<T>(this Node node)
        {
            return node.SearchByType<T>(Walk);
        }    

        public static List<T> CollectByType<T>(this Node node, Func<Node, IEnumerable<Node>> walker)
        {
            return walker.Invoke(node).Where(typeof(T).IsInstanceOfType).Cast<T>().ToList();
        }

        public static List<T> CollectByType<T>(this Node node)
        {
            return node.CollectByType<T>(Walk);
        }           

        /*public static List<T> DescendantsByType<T>() where T : Node
        {
            return Descendants.Where(x => typeof(T).IsAssignableFrom(x.GetType())).Select(x => x as T).ToList();
        }

        public static List<T> AncestorsByType<T>() where T : Node
        {
            return Ancestors.Where(x => typeof(T).IsAssignableFrom(x.GetType())).Select(x => x as T).ToList();
        }*/
    }
}
