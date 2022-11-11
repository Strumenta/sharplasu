using Strumenta.Cslasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strumenta.Cslasu.Tests
{
    internal class TopNode : Node
    {
        public int GoodStuff { get; init; }
        public int BadStuff { get; init; }

        public SmallNode? Smaller { get; init; }

    }

    internal class SmallNode : Node
    {
        public string Description { get; init; }
    }
}
