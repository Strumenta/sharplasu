using System;
using System.Linq;
using ExtensionMethods;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Model
{
    public static class ProcessingExtensions
    {
        public static void AssignParents(this Node node)
        {
            foreach (var child in node.WalkChildren())
            {
                if (child == node)
                    throw new InvalidOperationException($"A node cannot be parent of itself: {node}");
                child.Parent = node;
                child.AssignParents();
            }
        }

        public static bool HasValidParents(this Node node, Node parent = null)
        {
            Node realParent = parent;
            if (realParent == null)
                realParent = node.Parent;

            return node.Parent == realParent && node.Children.All(it => it.HasValidParents(node));
        }
    }
}