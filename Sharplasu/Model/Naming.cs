﻿using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Strumenta.Sharplasu.Model
{
    /// <summary>
    /// An entity which has a name. 
    /// </summary>
    public interface Named
    {
        /// <summary>
        /// The mandatory name of the entity. 
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// <para>A reference associated by using a name.
    /// It can be used only to refer to Nodes and not to other values.</para>
    /// 
    /// <para>This is not statically enforced as we may want to use some interface, which cannot extend Node.
    /// However, this is enforced dynamically.</para>
    /// </summary>
    [Serializable]
    public class ReferenceByName<N> where N : class, Named
    {
        public string Name { get; private set; }
        private N referred;
        
        public N Referred
        {
            get
            {
                return referred;
            }
            
            set
            {
                if (!(value is Node) && value != null)
                {
                    throw new InvalidOperationException($"We cannot enforce it statically but only Node" +
                        $"should be referred to. Instead {value} was assigned class {value.GetType()}");
                }
                referred = value;
            }
        }

        public ReferenceByName(string name, N initialReferred = null)
        {
            Name = name;
            Referred = initialReferred;
        }

        public bool Resolved
        {
            get
            {
                return referred != null;
            }
        }

        public override string ToString()
        {
            if (Resolved)
            {
                return $"Ref({Name})[Solved]";
            }
            else
            {
                return $"Ref({Name})[Unsolved]";
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ReferenceByName<N> name &&
                   Name == name.Name &&
                   EqualityComparer<N>.Default.Equals(Referred, name.Referred);
        }

        public override int GetHashCode()
        {
            int hashCode = -1556776030;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<N>.Default.GetHashCode(Referred);
            return hashCode;
        }
    }

    public static class Naming
    {
        /// <summary>
        /// Try to resolve the reference by finding a named element with a matching name. 
        /// The name match is performed in a case sensitive or insensitive way depending on the value of <paramref name="caseInsensitive"/>.
        /// </summary>
        /// <param name="reference">The reference that should be solved</param>
        /// <param name="candidates">The candidates to which the reference could be resolved</param>
        /// <param name="caseInsensitive">Whether the resolution should be case-insensitive</param>        
        public static bool TryToResolve<N>(this ReferenceByName<N> reference, IEnumerable<N> candidates, bool caseInsensitive = false)
            where N : class, Named
        {
            N res = candidates.FirstOrDefault(it => it.Name == null ? false : String.Equals(it.Name, reference.Name, caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
            reference.Referred = res;

            return res != null;
        }

        /// <summary>
        /// Try to resolve the reference by assigning <paramref name="possibleValue"/>. The assignment is not performed if
        /// <paramref name="possibleValue"/> is null.
        /// </summary>
        /// <returns>True if the assignment has been performed</returns>
        public static bool TryToResolve<N>(this ReferenceByName<N> reference, N possibleValue)
                where N : class, Named
        {
            if (possibleValue == null)
            {
                return false;
            }                
            else
            {
                reference.Referred = possibleValue;
                return true;
            }
        }        
    }
}
