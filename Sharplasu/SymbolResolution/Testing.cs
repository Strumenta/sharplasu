using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ReferenceByNameProperty = System.Reflection.PropertyInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Strumenta.Sharplasu.Traversing;

namespace Strumenta.Sharplasu.SymbolResolution
{
    public static class Testing
    {
        public static void AssertAllReferencesResolved(this Node node, Type withReturnType = null)
        {
            var refs = node.GetReferenceResolvedValues(withReturnType);
            Assert.IsTrue(node.GetReferenceResolvedValues(withReturnType).All(it => it));
        }

        public static void AssertAllReferencesResolved(this Node node, ReferenceByNameProperty forProperty)
        {
            Assert.IsTrue(node.GetReferenceResolvedValues(forProperty).All(it => it));
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
            Assert.IsTrue(references.Any(it => !it));
        }

        public static void AssertNotAllReferencesResolved(this Node node, ReferenceByNameProperty forProperty)
        {
            IEnumerable<bool> references = new List<bool>() { false };
            if (node.GetReferenceResolvedValues(forProperty).Count() > 0)
            {
                references = node.GetReferenceResolvedValues(forProperty);
            }            
            Assert.IsTrue(references.Any(it => !it));
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
