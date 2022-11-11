using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Strumenta.Sharplasu.Validation
{
    public enum IssueType
    {
        LEXICAL,
        SYNTACTIC,
        SEMANTIC
    }

    public class Issue
    {
        public IssueType IssueType { get; private set; }
        public string Message { get; private set; }
        public Position Position { get; private set; }

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
    }
}
