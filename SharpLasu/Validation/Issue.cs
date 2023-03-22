using Strumenta.Sharplasu.Model;
using System;
using System.Text.Json.Serialization;

namespace Strumenta.Sharplasu.Validation
{
    public enum IssueType
    {
        LEXICAL,
        SYNTACTIC,
        SEMANTIC
    }

    [Serializable]
    public class Issue
    {
        public IssueType IssueType { get; set; }
        public string Message { get; set; }
        public Position Position { get; set; }

        private Issue() {}

        public Issue(IssueType issueType, string message, Position position)
        {
            IssueType = issueType;
            Message = message;
            Position = position;
        }

        public static Issue Lexical(string message, Position position)
        {
            return new Issue(IssueType.LEXICAL, message, position);
        }

        public static Issue Syntactic(string message, Position position)
        {
            return new Issue(IssueType.SYNTACTIC, message, position);
        }

        public static Issue Semantic(string message, Position position)
        {
            return new Issue(IssueType.SEMANTIC, message, position);
        }

        public override string ToString()
        {
            return $"{IssueType} Issue: {Message} {Position} ";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            Issue o = obj as Issue;
            return
                IssueType.Equals(o.IssueType) &&
                ((Message == null && o.Message == null) || Message.Equals(o.Message)) &&
                ((Position == null && o.Position == null) || Position.Equals(o.Position));
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + IssueType.GetHashCode();
                hash = hash * 23 + (Message == null ? 0 : Message.GetHashCode());
                hash = hash * 23 + (Position == null ? 0 : Position.GetHashCode());
                return hash;
            }
        }

    }
}
