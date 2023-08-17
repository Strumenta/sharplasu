using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.SharpLasu.Model
{
    public interface Origin
    {
        Position Position { get; set; }
        String SourceText { get; set; }
        Source Source { get; }           
    }

    [Serializable]
    public class SimpleOrigin : Origin
    {
        public Position Position { get; set; }
        public string SourceText { get; set; }

        public Source Source => Position?.Source;
    }

    public static class NodeExtensions
    {
        public static N WithOrigin<N> (this N node, Origin origin) where N : Node 
        {
            if (origin == node) 
            {
                node.Origin = null;
            } 
            else 
            {
                node.Origin = origin;
            }
            return node;
        }
}
}
