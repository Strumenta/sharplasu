using System;
using Strumenta.Sharplasu.Model;

namespace ExtensionMethods
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
    }
}