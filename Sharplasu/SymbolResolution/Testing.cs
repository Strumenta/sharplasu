using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ReferenceByNameProperty = System.Reflection.PropertyInfo;
using Strumenta.Sharplasu.Traversing;

namespace Strumenta.Sharplasu.SymbolResolution
{
    public class SymbolResolutionException : Exception
    {
        public Type WithType { get; }
        
        public SymbolResolutionException(string message, Type withType = null)
            : base(message)            
        {
            WithType = withType;
        }
    }

    public static class Testing
    {
        public static void AssertAllReferencesResolved(this Node node, Type withReturnType = null)
        {
            var refs = node.GetReferenceResolvedValues(withReturnType);

            if (!node.GetReferenceResolvedValues(withReturnType).All(it => it))
                throw new SymbolResolutionException($"Not all references in ${node} ${(withReturnType != null ? "with type:" + withReturnType.ToString() : "")} are solved");
        }

        public static void AssertAllReferencesResolved(this Node node, ReferenceByNameProperty forProperty)
        {
            if (!node.GetReferenceResolvedValues(forProperty).All(it => it))
                throw new SymbolResolutionException($"Not all references in ${node} are solved");
        }

        public static void AssertNotAllReferencesResolved(this Node node, Type withReturnType = null)
        {
            IEnumerable<bool> references = new List<bool>() { false };
            if (withReturnType == null)
                withReturnType = typeof(Named);
            if (node.GetReferenceResolvedValues(withReturnType).Count() > 0)
            {
                references = node.GetReferenceResolvedValues(withReturnType);
            }
            if (!references.Any(it => !it))
                throw new SymbolResolutionException($"All references in ${node} ${(withReturnType != null ? "with type:" + withReturnType.ToString() : "")} are solved");
        }

        public static void AssertNotAllReferencesResolved(this Node node, ReferenceByNameProperty forProperty)
        {
            IEnumerable<bool> references = new List<bool>() { false };
            if (node.GetReferenceResolvedValues(forProperty).Count() > 0)
            {
                references = node.GetReferenceResolvedValues(forProperty);
            }
            if (!references.Any(it => !it))
                throw new SymbolResolutionException($"All references in ${node} are solved");
        }

        private static IEnumerable<bool> GetReferenceResolvedValues(this Node node, Type withReturnType = null)            
        {                      
            return node.Walk().SelectMany(it =>
                    it.NodeProperties().Where(property => property.IsReference(withReturnType))
                    .Select(property => property.GetValue(it))
                    .Where(property => property != null)
                    .Select(value => (bool)value.GetType().GetProperty("Resolved").GetValue(value))
                    );
        }

        private static IEnumerable<bool> GetReferenceResolvedValues(this Node node, ReferenceByNameProperty forProperty)            
        {
            return node.Walk().SelectMany(it =>
                    it.NodeProperties().Where(property => property == forProperty)
                    .Select(property => property.GetValue(it)).ToList()
                    .Where(property => property != null)
                    .Select(value => (bool)value.GetType().GetProperty("Resolved").GetValue(value))
                );
        }
    }
}
