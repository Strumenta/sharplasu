using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Traversing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strumenta.Sharplasu.Transformation
{
    public class GenericNode : Node
    {        
        public GenericNode(Node parent = null)
        { 
            this.Parent = parent;
        }
    }
    

    public static class GenericNodes
    {
        public static GenericNode FindGenericNode(this Node node)
        {
            if (node is GenericNode)
                return node as GenericNode;
            else
                return node.Children().Find(x => x.FindGenericNode() != null)?.FindGenericNode();
        }
    }
}
