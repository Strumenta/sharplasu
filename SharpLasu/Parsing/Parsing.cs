using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Antlr4.Runtime;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Parsing
{
    public class CodeProcessingResult<D>
    {
        public List<Issue> Issues { get; }
        public D Data { get; }
        public string Code { get; } = null;

        public CodeProcessingResult(List<Issue> issues, D data, string code)
        {
            Issues = issues;
            Data = data;
            Code = code;
        }

        public bool Correct
        {
            get
            {
                return Issues.All(issue => issue.Severity == IssueSeverity.Info);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            var o = obj as CodeProcessingResult<D>;
            return Issues.SequenceEqual(o.Issues) &&
                   Data.Equals(o.Data) &&
                   Code.Equals(o.Code);
        }

        public override int GetHashCode()
        {
            var result = Issues.GetHashCode();
            if (Data != null)
                result = 31 * result + Data.GetHashCode();
            if (Code != null)
                result = 31 * result + Code.GetHashCode();
            return result;
        }
    }
    public class FirstStageParsingResult<C> : CodeProcessingResult<C> where C : ParserRuleContext
    {
        public C Root { get; }
        public string Code { get; }
        public Node IncompleteNode { get; }

        public FirstStageParsingResult(
            List<Issue> issues,
            C root,
            string code,
            Node incompleteNode) : base(issues, root, code)
        {
            Root = root;
            Code = code;
            IncompleteNode = incompleteNode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var o = (FirstStageParsingResult<C>) obj;
            return base.Equals(o) &&
                   ((Root == null && o.Root == null) || (Root != null && Root.Equals(o.Root))) &&
                   ((IncompleteNode == null && o.IncompleteNode == null) || (IncompleteNode != null && IncompleteNode.Equals(o.IncompleteNode)));
        }
    }
}