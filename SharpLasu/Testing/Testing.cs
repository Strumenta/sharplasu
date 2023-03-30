using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;

namespace Strumenta.Sharplasu.Testing
{
    public static class Testing
    {
        public static void AssertASTsAreEqual<TNode>(
            Node expected,
            ParsingResult<TNode> actual,
            string context = "<root>",
            bool considerPosition = false) where TNode : Node
        {
            Contract.Assert(0 == actual.Issues.Count, actual.Issues.ToString());
            AssertASTsAreEqual(expected, actual.Root, context, considerPosition);
        }
        
        public static void AssertASTsAreEqual(
            Node expected,
            Node actual,
            string context = "<root>",
            bool considerPosition = false)
        {
            Contract.Assert(expected.GetType() == actual.GetType(),
                $"{context}: expected node of type {expected.GetType().FullName}," +
                $"but found {actual.GetType().FullName}");
            //
            // if (expected.GetType() != actual.GetType())
            //     Assert.Fail($"{context}: expected node of type {expected.GetType().FullName}," +
            //                 $"but found {actual.GetType().FullName}");
            //
            // if (considerPosition)
            //     Assert.AreEqual(expected.SpecifiedPosition, actual.SpecifiedPosition, $"{context}.position");

            foreach (var propertyInfo in expected.GetType().GetProperties())
            {
                // TODO
            }
            
            // TODO ...
        }
    }
}