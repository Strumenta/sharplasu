using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Strumenta.Sharplasu.Transformation
{    
    public static class TrivialFactoryOfParseTreeToASTNodeFactory
    {
        public static ConstructorInfo PreferredConstructor(this Type type)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length != 1) 
            {
                if (type.GetConstructor(BindingFlags.Default, null, Type.EmptyTypes, new ParameterModifier[] { }) != null)
                    return type.GetConstructor(BindingFlags.Default, null, Type.EmptyTypes, new ParameterModifier[] { });
                else
                    throw new NotSupportedException("Node Factories support only classes with exactly one constructor or a " +
                        $"primary constructor. Class {type.FullName} has ${constructors.Length}");
            }
            else
            {
                return constructors.First();
            }
        }
    }
}
