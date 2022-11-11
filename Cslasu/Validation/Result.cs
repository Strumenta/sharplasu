using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Strumenta.Cslasu.Validation
{
    public class Result<T>
    {
        public List<Issue> Issues { get; protected set; }
        public T Root { get; protected set; }

        public IEnumerable<Issue> LexicalErrors
        {
            get
            {
                return Issues.Where(x => x.IssueType == IssueType.LEXICAL);
            }
        }

        public IEnumerable<Issue> SyntacticErrors
        {
            get
            {
                return Issues.Where(x => x.IssueType == IssueType.SYNTACTIC);
            }
        }

        public bool Correct
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        public Result(List<Issue> errors, T root)
        {
            Issues = errors;
            Root = root;
        }
    }
}
