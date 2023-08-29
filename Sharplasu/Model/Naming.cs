using Strumenta.Sharplasu.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using static Antlr4.Runtime.Atn.SemanticContext;

namespace Strumenta.Sharplasu.Model
{
    /**
     * An entity which has a name.
     */
    public interface Named
    {
        /**
         * The mandatory name of the entity.
         */
        string Name { get; set; }
    }

    /**
    * A reference associated by using a name.
    * It can be used only to refer to Nodes and not to other values.
    *
    * This is not statically enforced as we may want to use some interface, which cannot extend Node.
    * However, this is enforced dynamically.
    */
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
        /**
        * Try to resolve the reference by finding a named element with a matching name.
        * The name match is performed in a case sensitive or insensitive way depending on the value of @param[caseInsensitive].
        */
        public static bool TryToResolve<N>(this ReferenceByName<N> reference, IEnumerable<N> candidates, bool caseInsensitive = false)
            where N : class, Named
        {
            N res = candidates.FirstOrDefault(it => it.Name == null ? false : String.Equals(it.Name, reference.Name, caseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
            reference.Referred = res;

            return res != null;
        }

        /**
         * Try to resolve the reference by assigning @param[possibleValue]. The assignment is not performed if
         * @param[possibleValue] is null.
         *
         * @return true if the assignment has been performed
         */
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
