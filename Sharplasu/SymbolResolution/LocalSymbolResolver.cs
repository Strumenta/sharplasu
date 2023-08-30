using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.Sharplasu.SymbolResolution
{
    [Serializable]
    public abstract class LocalSymbolResolver
    {
        public abstract List<Issue> ResolveSimbols(Node root);
    }
}
