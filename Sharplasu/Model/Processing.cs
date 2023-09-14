using System;
using System.Collections.Generic;
using System.Linq;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Traversing;
using static Antlr4.Runtime.Atn.SemanticContext;

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

            return node.Parent == realParent && node.Children().All(it => it.HasValidParents(node));
        }

        public static IEnumerable<Node> InvalidPositions(this Node node)
        {
            bool checkPosition(Node it)
            {
                var parentPos = it.Parent?.Position;
                var s = parentPos?.Contains(it.Position.Start);
                var e = parentPos?.Contains(it.Position.End);
                // If the parent position is null, we can't say anything about this node's position
                return (parentPos != null && !(parentPos.Contains(it.Position.Start) && parentPos.Contains(it.Position.End)));
            }
            
            return node.Walk().Where(it => it.Position == null || checkPosition(it));
        }
    }
}