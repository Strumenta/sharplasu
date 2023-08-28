using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;

namespace Strumenta.Sharplasu.Parsing
{
    public static class MappingExtensions
    {
        public static Position Position(this ParserRuleContext rule)
        {
            return new Position(rule.Start.StartPoint(), rule.Stop.EndPoint());
        }

        public static Position ToPosition(this ParserRuleContext rule, bool considerPosition = true)
        {
            if (considerPosition && rule.Start != null && rule.Stop != null)
                return rule.Position();
            else
                return null;
        }
    }
}
