using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Strumenta.Sharplasu.Mapping;
using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Strumenta.Sharplasu.Transformation
{    
    public static class TrivialFactoryOfParseTreeToASTNodeFactory
    {
        public static object ConvertString(string text, ASTTransformer astTransformer, Type expectedType)
        {
            if (expectedType == typeof(ReferenceByName<Named>))
            { 
                return new ReferenceByName<Named>(text);
            } 
            else if (expectedType == typeof(string))
            {
                return text;
            }
            else if (expectedType == typeof(int))
            {
                return int.Parse(text);
            }
            else
                return null;
        }

        public static object Convert(object value, ASTTransformer astTransformer, Type expectedType)
        {
            switch(value)
            {
                case IToken token:
                    return ConvertString(token.Text, astTransformer, expectedType);
                case IList list:
                    List<object> list1 = new List<object>();                    
                    foreach (var item in list)
                    {
                        list1.Add(Convert(item, astTransformer, expectedType.GetGenericArguments()[0]));
                    }
                    return list1;
                case ParserRuleContext context:
                    if (expectedType == typeof(string))
                        return context.GetText();
                    else
                        return astTransformer.Transform(value);
                case ITerminalNode node:
                    return ConvertString(node.GetText(), astTransformer, expectedType);
                default: 
                    return null;
            }
        }

        public static Func<object, ASTTransformer, T> TrivialFactory<S, T>(params Pair<string, string>[] nameConversions)
            where S : RuleContext
            where T : Node
        {
            return (object parseTreeNode, ASTTransformer astTransformer) =>
            {
                var constructor = typeof(T).PreferredConstructor();
                var args = constructor.GetParameters().ToList().Select(it =>
                {
                    var parameterName = it.Name;
                    var searchedName = nameConversions.FirstOrDefault(nc => nc.b == parameterName)?.a ?? parameterName;
                    var parseTreeMember = parseTreeNode.GetType().GetProperties().FirstOrDefault(p => p.Name == searchedName);
                    if (parseTreeMember == null) 
                    {
                        var method = parseTreeNode.GetType().GetMethods().FirstOrDefault(
                        //m => m.Name == searchedName && m.GetParameters().Length == 1
                        m => m.Name == searchedName && m.GetParameters().Length == 0
                        );
                        if (method == null)
                        {
                            throw new InvalidOperationException($"Unable to convert {parameterName}" +
                                $"(looking for {searchedName} in ${parseTreeNode.GetType()})");
                        } 
                        else
                        {
                            var value = method.Invoke(parseTreeNode, null);
                            return Convert(value, astTransformer, it.GetType());
                        }
                    }
                    else
                    {
                        var value = parseTreeMember.GetValue(parseTreeNode);
                        return Convert(value, astTransformer, it.GetType());
                    }
                }).ToArray();
                try
                {
                    T instance = (T) constructor.Invoke(args);
                    instance.Children.ForEach(it => { it.Parent = instance; });
                    return instance;
                }
                catch (ArgumentException e)
                { 
                    throw new InvalidOperationException($"Failure while invoking constructor " +
                        $"{constructor} with args: {ParsingExtensions.ArrayToString(args)}",
                    e);
                }
            };
        }

        public static ASTTransformer RegisterTrivialPTtoASTConversion<S, T>(this ASTTransformer transformer, params Pair<string, string>[] nameConversions)
            where S : RuleContext
            where T : Node
        {
            transformer.RegisterNodeFactory<T>(typeof(S), TrivialFactoryOfParseTreeToASTNodeFactory.TrivialFactory<S, T>(nameConversions));
            return transformer;
        }

        public static ASTTransformer RegisterTrivialPTtoASTConversion<S, T>(this ParseTreeToASTTransformer transformer, params Pair<MethodInfo, MethodInfo>[] nameConversions)
            where S : RuleContext
            where T : Node
        {
            List<Pair<string, string>> names = new List<Pair<string, string>>();
            foreach (var item in nameConversions)
            {
                names.Add(new Pair<string, string>(item.a.Name, item.b.Name));
            }
            
            return transformer.RegisterTrivialPTtoASTConversion<S, T>(names.ToArray());
        }

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
