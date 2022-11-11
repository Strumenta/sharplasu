using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Strumenta.Cslasu.Model;
using Strumenta.Cslasu.Validation;
using Strumenta.Cslasu.Parsing;

namespace Strumenta.Cslasu.Testing
{
    public static class TestParserFacade
    {        
        public static void VerifyParseTree(Parser parser, ref List<Issue> errors, ParserRuleContext root)
        {
            var commonTokenStream = parser.TokenStream as CommonTokenStream;
            var lastToken = commonTokenStream.Get(commonTokenStream.Index);

            if (lastToken.Type != Parser.Eof)
            {
                errors.Add(new Issue(IssueType.SYNTACTIC, "Not whole input consumed", lastToken.EndPoint()?.AsPosition));
            }

            List<Issue> issues = errors;

            root?.ProcessDescendants((ParserRuleContext operation) =>
            {
                if (operation.exception != null)
                {
                    var ruleName = parser.RuleNames[operation.RuleIndex];
                    issues.Add(new Issue(IssueType.SYNTACTIC, $"Recognition exception: ${operation.exception?.Message} on rule $ruleName", operation.Start?.StartPoint()?.AsPosition));
                }
                if (operation is IErrorNode)
                {
                    issues.Add(new Issue(IssueType.SYNTACTIC, "Error node found", operation.ToPosition()));
                }
            });
        }

        public static void VerifyASTTree(Node root, ref List<Issue> errors)
        {
            List<Issue> issues = errors;

            root?.ProcessDescendants((Node node) => {
                if(node?.Parent == null)
                    issues.Add(new Issue(IssueType.SEMANTIC, "Node has no parent", node?.SpecifiedPosition));
            }, false);
        }
    }
}
