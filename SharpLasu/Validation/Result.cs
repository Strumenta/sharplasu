using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Strumenta.Sharplasu.Validation
{
    [Serializable]
    public class Result<T>
    {
        public List<Issue> Issues { get; set; }
        public T Root { get; protected set; }

        [JsonIgnore]
        public IEnumerable<Issue> LexicalErrors
        {
            get
            {
                return Issues.Where(x => x.IssueType == IssueType.LEXICAL);
            }
        }

        [JsonIgnore]
        public IEnumerable<Issue> SyntacticErrors
        {
            get
            {
                return Issues.Where(x => x.IssueType == IssueType.SYNTACTIC);
            }
        }

        [JsonIgnore]
        public bool Correct
        {
            get
            {
                return Issues.Count == 0;
            }
        }

        public Result() {}

        public Result(List<Issue> errors, T root)
        {
            Issues = errors;
            Root = root;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            Result<T> o = obj as Result<T>;
            return (
                (Issues != null && o.Issues != null && Enumerable.SequenceEqual(Issues, o.Issues)) ||
                (Issues == null && o.Issues == null)
            );
        }
        
        public override int GetHashCode()
        {
            return Root.GetHashCode();
        }
    }
}
