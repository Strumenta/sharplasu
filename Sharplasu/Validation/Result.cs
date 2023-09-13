using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Strumenta.Sharplasu.Validation
{
    public class Result<C>
        where C : class
    {
        public List<Issue> Issues { get; set; }
        public C Root { get; set; }

        public Result(List<Issue> issues, C root)
        {
            Issues = issues;
            Root = root;
        }

        public Result()
        {
            Issues = new List<Issue>();
            Root = null;
        }

        public IEnumerable<Issue> LexicalIssues => Issues.Where(x => x.IssueType == IssueType.Lexical);
        public IEnumerable<Issue> SyntacicIssues => Issues.Where(x => x.IssueType == IssueType.Syntatic);
        public IEnumerable<Issue> SemanticIssues => Issues.Where(x => x.IssueType == IssueType.Semantic);
        public IEnumerable<Issue> TranslationIssues => Issues.Where(x => x.IssueType == IssueType.Translation);
        public IEnumerable<Issue> Errors => Issues.Where(x => x.IssueSeverity == IssueSeverity.Error);
        public IEnumerable<Issue> Warnings => Issues.Where(x => x.IssueSeverity == IssueSeverity.Warning);
        public bool Correct => Issues.Count == 0;
        
        public static Result<C> Exception(IssueType errorType, Exception e)
        {
            var errors = new List<Issue>()
            {
                new Issue(
                        errorType,
                        e.Message ?? e.GetType().Name,
                        null
                    )
            };
            return new Result<C>(errors, null);
        }
    }
}
