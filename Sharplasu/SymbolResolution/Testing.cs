using ExtensionMethods;
using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ReferenceByNameProperty = System.Reflection.PropertyInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Strumenta.Sharplasu.SymbolResolution
{
    public static class Testing
    {
        public static void AssertAllReferencesResolved(this Node node, Type withReturnType = null)
        {           
            Assert.IsTrue(node.GetReferenceResolvedValues(withReturnType).All(it => it));
        }

        public static void AssertAllReferencesResolved(this Node node, ReferenceByNameProperty forProperty)
        {            
            Assert.IsTrue(node.GetReferenceResolvedValues(forProperty).All(it => it));
        }

        public static void AssertNotAllReferencesResolved(this Node node, Type withReturnType = null)
        {
            IEnumerable<bool> references = new List<bool>() { false };
            var s = node.GetReferenceResolvedValues(withReturnType);
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

        private static Type ReferenceByName()
        {
            return typeof(ReferenceByName<>);
        }

        private static IEnumerable<bool> GetReferenceResolvedValues(this Node node, Type withReturnType = null)            
        {
            return node.Walk().SelectMany(it => 
                    it.NodeProperties().Where(property => property.PropertyType.GetGenericTypeDefinition() == ReferenceByName())
                    .Select(property => property.GetValue(node)).ToList()
                    .Select(value => (bool) (value as dynamic).Resolved)
                );
        }

        private static IEnumerable<bool> GetReferenceResolvedValues(this Node node, ReferenceByNameProperty forProperty)            
        {
            return node.Walk().SelectMany(it =>
                    it.NodeProperties().Where(property => property == forProperty)
                    .Select(property => property.GetValue(node)).ToList()
                    .Select(value => (bool)(value as dynamic).Resolved)
                );
        }
    }
}
