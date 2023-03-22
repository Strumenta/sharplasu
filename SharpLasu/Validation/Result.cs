using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Strumenta.Sharplasu.Validation
{
    [Serializable]
    public class Result<T>
    {
        public List<Issue> Issues { get; set; }
        public T Root { get; set; }

        [JsonIgnore][XmlIgnore]
        public IEnumerable<Issue> LexicalErrors
        {
            get
            {
                return Issues.Where(x => x.IssueType == IssueType.LEXICAL);
            }
        }

        [JsonIgnore][XmlIgnore]
        public IEnumerable<Issue> SyntacticErrors
        {
            get
            {
                return Issues.Where(x => x.IssueType == IssueType.SYNTACTIC);
            }
        }

        [JsonIgnore][XmlIgnore]
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
            ) && (
                (Root != null && o.Root != null && Root.Equals(o.Root)) ||
                (Root == null && o.Root == null)
            );
        }
        
        public override int GetHashCode()
        {
            return Root.GetHashCode();
        }
    }
}
