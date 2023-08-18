﻿using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.Validation;
using Strumenta.SharpLasu.Model;
using Strumenta.SharpLasu.Transformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Antlr4.Runtime.Tree;

namespace Strumenta.SharpLasu.Transformation
{
    public class PropertyAccessor
    {
        private PropertyInfo PropertyInfo { get; set; }

        public PropertyAccessor(Type typeInfo, string property)
        {
            PropertyInfo = typeInfo.GetProperty(property);
        }

        public Object Accessor(Object obj)
        {
            return PropertyInfo.GetGetMethod().Invoke(obj, null);
        }
    }

    public delegate void Setter(Object obj, Object value);

    /**
     * A child of an AST node that is automatically populated from a source tree.
     */
    [AttributeUsage(AttributeTargets.Class)]
    public class MappedAttribute : System.Attribute
    {
        public string Path { get; set; }
        
        public MappedAttribute(string path = "")
        {
            Path = path;
        }
    }

    internal abstract class ParameterValue { };
    internal class PresentParameterValue : ParameterValue
    {
        internal Object Value { get; set; }

        internal PresentParameterValue(Object value)
        {
            this.Value = value;
        }   
    }

    internal class AbsentParameterValue : ParameterValue { }

    public static class Transformation
    {
        /**
        * Sentinel value used to represent the information that a given property is not a child node.
        */
        internal static ChildNodeFactory<Object, Object, Object> NO_CHILD_NODE = new ChildNodeFactory<Object, Object, Object>("", (x) => x, (y, z) => { });

        internal static AbsentParameterValue AbsentParameterValue = new AbsentParameterValue();

        internal static ChildNodeFactory<Source, Target, Child> GetChildNodeFactory<Source, Target, Child, A, B>(
            this NodeFactory<A, B> nodeFactory,
            Type nodeClass,
            string parameterName
        ) where B: Node
        {
            var childKey = nodeClass.FullName + "#" + parameterName;
            var childNodeFactory = nodeFactory.Children[childKey];
            if ( childNodeFactory == null )
            {
                childNodeFactory = nodeFactory.Children[parameterName];
            }
            return childNodeFactory as ChildNodeFactory<Source, Target, Child>;
        }
    }

    /**
     * Factory that, given a tree node, will instantiate the corresponding transformed node.
     */
    public class NodeFactory<Source, Output> where Output : Node
    {
        public Func<Source, ASTTransformer, NodeFactory<Source, Output>, List<Output>> Constructor { get; set; }
        public Dictionary<string, ChildNodeFactory<Source, Object, Object>> Children { get; set; } = new Dictionary<string, ChildNodeFactory<Source, object, object>>();
        public Action<Output> Finalizer { get; set; } = (x) => { };
        public bool SkipChildren = false;
        public bool ChildrenSetAtConstruction = false;
        public NodeFactory(
            Func<Source, ASTTransformer, NodeFactory<Source, Output>, List<Output>> constructor,
            Dictionary<string, ChildNodeFactory<Source, Object, Object>> children,
            Action<Output> finalizer,
            bool skipChildren = false,
            bool childrenSetAtConstruction = false
        )
        {
            Constructor = constructor ?? Constructor;
            Children = children ?? Children;
            Finalizer = finalizer ?? Finalizer;
            SkipChildren = skipChildren;
            ChildrenSetAtConstruction = childrenSetAtConstruction;
        }

        public static NodeFactory<Source, Output> 
            Single(
                Func<Source, ASTTransformer, NodeFactory<Source, Output>, Output> singleConstructor,
                Dictionary<string, ChildNodeFactory<Source, Object, Object>> children,
                Action<Output> finalizer,
                bool skipChildren = false,
                bool childrenSetAtConstruction = false
            )
        {
            return new NodeFactory<Source, Output>( (source, at, nf) => { 
                    var result = singleConstructor( source, at, nf );
                    if (result == null) 
                        return new List<Output>();
                    else
                        return new List<Output>() { result };
            }, children, finalizer, skipChildren, childrenSetAtConstruction);
                
        }

        /**
         * Specify how to convert a child. The value obtained from the conversion could either be used
         * as a constructor parameter when instantiating the parent, or be used to set the value after
         * the parent has been instantiated.
         *
         * Example using the scopedToType parameter:
         * ```
         *     on.registerNodeFactory(SASParser.DatasetOptionContext::class) { ctx ->
         *         when {
         *             ...
         *         }
         *     }
         *         .withChild(SASParser.DatasetOptionContext::macroStatementStrict, ComputedDatasetOption::computedWith, ComputedDatasetOption::class)
         *         .withChild(SASParser.DatasetOptionContext::variableList, DropDatasetOption::variables, DropDatasetOption::class)
         *         .withChild(SASParser.DatasetOptionContext::variableList, KeepDatasetOption::variables, KeepDatasetOption::class)
         *         .withChild(SASParser.DatasetOptionContext::variableList, InDatasetOption::variables, InDatasetOption::class)
         *         .withChild("indexDatasetOption.variables", IndexDatasetOption::variables, IndexDatasetOption::class)
         *  ```
         *
         *  Please note that we cannot merge this method with the variant without the type (making the type optional),
         *  as it would not permit to specify the lambda outside the list of method parameters.
         *  
         *  This corresponds to 2 methods in Kolasu, because C# does not have different PropertyInfo(s)
         *  for mutable and immutable properties
         */
        public NodeFactory<Source, Output> WithChild(            
            PropertyInfo targetProperty,
            PropertyAccessor sourceAccessor,
            TypeInfo scopedToType = null
        )
        {
            return WithChild<Source, Output>(
                     get: (source) => sourceAccessor.Accessor(source),
                     set: (Source obj, Output value) => {
                         targetProperty.GetSetMethod().Invoke(obj, new object[] { value });
                     },
                     targetProperty.Name,
                     scopedToType
            );
        }

        /**
        * Specify how to convert a child. The value obtained from the conversion could either be used
        * as a constructor parameter when instantiating the parent, or be used to set the value after
        * the parent has been instantiated.
        * 
        * This corresponds to 2 methods in Kolasu, because C# does not have different PropertyInfo(s)
        * for mutable and immutable properties
        */
        public NodeFactory<Source, Output> WithChild(
            PropertyInfo targetProperty,
            PropertyAccessor sourceAccessor            
        )
        {
            return WithChild(
                get: (source) => sourceAccessor.Accessor(source),
                set: (Source obj, Output value) => {
                        targetProperty.GetSetMethod().Invoke(obj, new object[] { value });
                     },
                name: targetProperty.Name,
                scopedToType: null
            );
        }
        

        /**
        * Specify how to convert a child. The value obtained from the conversion could either be used
        * as a constructor parameter when instantiating the parent, or be used to set the value after
        * the parent has been instantiated.
        */
        public NodeFactory<Source, Output> WithChild<Target, Child>
        (
           Func<Source, object> get,
           Action<Target, Child> set,
           string name,
           TypeInfo scopedToType = null
        )
        //  in C# you cannot use Object, the equivalent of Any in Kotlin, as a type constraint
        //where Target : Object
        //where Child : Object
        {
            string prefix = "";
            if (scopedToType != null)
                prefix = scopedToType.FullName + "#";
            else
                prefix = "";

            if (set == null)
            {
                // given we have no setter we MUST set the children at construction
                ChildrenSetAtConstruction = true;
            }

            Children[prefix + name] = new ChildNodeFactory<Source, Target, Child>(prefix + name, get, set) as ChildNodeFactory<Source, object, object>;
            return this;
        }

        public NodeFactory<Source, Output> WithFinalizer(Action<Output> finalizer)
        {
            this.Finalizer = finalizer;
            return this;
        }

        /**
        * Tells the transformer whether this factory already takes care of the node's children and no further computation
        * is desired on that subtree. E.g., when we're mapping an ANTLR parse tree, and we have a context that is only a
        * wrapper over several alternatives, and for some reason those are not labeled alternatives in ANTLR (subclasses),
        * we may configure the transformer as follows:
        *
        * ```kotlin
        * transformer.registerNodeFactory(XYZContext::class) { ctx -> transformer.transform(ctx.children[0]) }
        * ```
        *
        * However, if the result of `transformer.transform(ctx.children[0])` is an instance of a Node with a child
        * annotated with `@Mapped("someProperty")`, the transformer will think that it has to populate that child,
        * according to the configuration determined by reflection. When it tries to do so, the "source" of the node will
        * be an instance of `XYZContext` that does not have a child named `someProperty`, and the transformation will fail.
        */
        NodeFactory<Source, Output> WithSkipChildren(bool skip = true)
        {
            this.SkipChildren = skip;
            return this;
        }

        public Func<Source, object> Getter(string path)
        {
            return (Source src) =>
            {
                object sub = src;
                foreach (var elem in path.Split('.'))
                {
                    if (sub == null)
                        break;
                    sub = GetSubExpression(sub, elem);
                }
                return sub;
            };
        }

        private object GetSubExpression(object src, string elem)
        {
            List<object> list = new List<object>();
            if (src.GetType().GetInterfaces().Any(
                    i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                return (src as IEnumerable<object>).Select(it => GetSubExpression(it, elem)).ToList();
            }
            else
            {
                var sourceProp = src.GetType().GetProperties().FirstOrDefault(it => it.Name == elem);
                if (sourceProp == null)
                {
                    var sourceMethod = src.GetType().GetMethods()
                        .FirstOrDefault(it => it.Name == elem && it.GetParameters().Length == 1)
                        ?? throw new Exception($"{elem} not found in {src} ({src.GetType()})");
                    return sourceMethod.Invoke(src, null);
                }
                else
                {
                    return (sourceProp as PropertyInfo).GetValue(src);
                }
            }
        }

        /*internal ChildNodeFactory<ChildSource, Target, Child> GetChildNodeFactory<ChildSource, Target, Child>(           
           TypeInfo nodeClass,
           string parameterName
        )
        {
            var childKey = nodeClass.FullName + "#" + parameterName;
            var childNodeFactory = this.Children[childKey];
            if (childNodeFactory == null)
            {
                childNodeFactory = this.Children[parameterName];
            }
            return childNodeFactory as ChildNodeFactory<ChildSource, Target, Child>;
        }*/
    }

    /**
    * Information on how to retrieve a child node.
    */
    public class ChildNodeFactory<Source, Target, Child>
    {
        public string Name { get; set; }
        public Func<Source, Object> Get { get; set; }
        public Action<Target, Child> Setter { get; set; }

        public ChildNodeFactory(string name, Func<Source, Object> get, Action<Target, Child> setter)
        {
            Name = name;
            Get = get;
            Setter = setter;
        }

        public void Set(Target node, Child child)
        {
            if (Setter == null)
            {
                throw new InvalidOperationException($"Unable to set value {Name} in {node}");
            }

            try
            {
                Setter(node, child);
            }
            catch (Exception e)
            {
                throw new Exception($"{Name} could not set child {child} of {node} using {Setter}", e);
            }
        }
    }

    /**
     * Implementation of a tree-to-tree transformation. 
     * For each source node type, we can register a factory that knows how to create a transformed node. 
     * Then, this transformer can read metadata in the transformed node to recursively transform 
     * and assign children. If no factory is provided for a source node type, a GenericNode 
     * is created, and the processing of the subtree stops there.
    */
    public class ASTTransformer
    {
        /**
        * Additional issues found during the transformation process.
        */
        public List<Issue> Issues { get; set; } = new List<Issue>();
        public bool AllowGenericNode { get; set; } = true;

        /**
         * 
         * Factories that map from source tree node to target tree node.
         */
        public Dictionary<Type, NodeFactory<Object, Node>> Factories { get; set; } = new Dictionary<Type, NodeFactory<Object, Node>>();

        private static Dictionary<string, ISet<Type>> _knownClasses = new Dictionary<string, ISet<Type>>();
        public Dictionary<string, ISet<Type>> KnownClasses { get; private set; } = _knownClasses;

        /**
         * This ensures that the generated value is a single Node or null.
         */
        public Node Transform(object source, Node parent = null)
        {
            var result = TransformIntoNodes(source, parent);
            switch (result.Count)
            {
                case 0:
                    return null;
                case 1:
                    var node = result.First() as Node;
                    return node;
                default:
                    throw new InvalidOperationException("Cannot transform into a single Node as multiple nodes where produced");
            }
        }

        /**
         * Performs the transformation of a node and, recursively, its descendants.
         */
        public List<Node> TransformIntoNodes(object source, Node parent = null)
        {
            if (source == null)
                return new List<Node>();
            if (source.GetType().IsGenericType && (source.GetType().GetGenericTypeDefinition() == typeof(List<>)))
                throw new Exception("Mapping error: received collection when value was expected");
            var factory = GetNodeFactory<Object, Node>(source.GetType());
            var nodes = new List<Node>();
            if (factory != null)
            {
                nodes = MakeNodes(factory, source, AllowGenericNode);
                if (!factory.SkipChildren && !factory.ChildrenSetAtConstruction)
                {
                    nodes.ForEach(node => SetChildren(factory, source, node));
                }
                nodes.ForEach(node =>
                {
                    factory.Finalizer(node);
                    node.Parent = parent;
                });
            }
            else
            {
                if (AllowGenericNode)
                {
                    var origin = AsOrigin(source);
                    nodes = new List<Node>() { new GenericNode(parent).WithOrigin(origin) };
                    Issues.Add(
                        Issue.Semantic(
                            $"Source node not mapped: {source.GetType().FullName}",
                            origin?.Position,
                            IssueSeverity.Warning
                        )
                    );
                }
                else
                {
                    throw new InvalidOperationException($"Unable to translate node {source} (class ${source.GetType().FullName})");
                }
            }
            return nodes;
        }

        private void SetChildren(NodeFactory<Object, Node> factory, Object source, Node node)
        {
            node.GetType().ProcessProperties(
                new HashSet<string>(),
                pd =>
                {
                    var childKey = node.GetType().FullName + "#" + pd.Name;
                    var childNodeFactory = factory.GetChildNodeFactory<Object, Object, Object, Object, Node>(node.GetType(), pd.Name);
                    if (childNodeFactory != null)
                    {
                        if (childNodeFactory != Transformation.NO_CHILD_NODE)
                        {
                            SetChild(childNodeFactory, source, node, pd);
                        }
                    }
                    else
                    {
                        var targetProp = node.GetType().GetProperties().Where(it => it.Name == pd.Name).FirstOrDefault();
                        MappedAttribute mapped = targetProp?.GetCustomAttribute(typeof(MappedAttribute)) as MappedAttribute;
                        if (targetProp.CanWrite && mapped != null)
                        {
                            string path = mapped.Path;
                            if(String.IsNullOrEmpty(path))
                                path = targetProp.Name;
                            childNodeFactory = new ChildNodeFactory<Object, Object, Object>(
                                childKey, factory.Getter(path), (Object obj, Object value) => {
                                    targetProp.GetSetMethod().Invoke(obj, new object[] { value });
                                }
                            );
                            factory.Children[childKey] = childNodeFactory;
                            SetChild(childNodeFactory, source, node, pd);
                        }
                        else
                        {
                            factory.Children[childKey] = Transformation.NO_CHILD_NODE;
                        }
                    }
                }
            );
        }

        protected Origin AsOrigin(Object source)
        {
            if (source is Origin)
                return source as Origin;
            else
                return null;
        }

        protected void SetChild(
            ChildNodeFactory<Object, Object, Object> childNodeFactory,
            Object source,
            Node node,
            PropertyTypeDescription pd
        )
        {
            var childFactory = childNodeFactory as ChildNodeFactory<Object, Object, Object>;
            var childrenSource = childFactory.Get(GetSource(node, source));
            Object child;
            if (pd.Multiple)
            {
                child = (childrenSource as List<object>)
                    .Select(it => TransformIntoNodes(it, node)).ToList()
                    .SelectMany(it => it);
            }
            else
            {
                child = Transform(childrenSource, node);
            }
            try
            {
                childNodeFactory.Set(node, child);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not set child {childNodeFactory}", ex);
            }
        }

        protected Object GetSource(Node node, Object source)
        {
            return source;
        }

        protected List<Node> MakeNodes<S, T>
            (
                NodeFactory<S, T> factory,
                S source,
                bool allowGenericNode = true
            )
            where T : Node
        {
            List<Node> nodes;
            try
            {
                nodes = factory.Constructor(source, this, factory) as List<Node>;
            }
            catch (Exception ex)
            {
                if (allowGenericNode)
                    nodes = new List<Node>() { new GenericErrorNode(ex) };
                else
                    throw ex;
            }
            nodes.ForEach(node =>
            {
                if (node.Origin == null)
                {
                    node.WithOrigin(AsOrigin(source));
                }

            });            
            return nodes;
        }

        protected NodeFactory<S, T> GetNodeFactory<S, T>(Type kClass)
            where T : Node
        {
            var factory = Factories[kClass];
            if (factory != null)
            {
                return factory as NodeFactory<S, T>;
            }
            else
            {
                if (kClass == typeof(Object))
                {
                    return null;
                }
                foreach (var superclass in kClass.Superclasses())
                {
                    var nodeFactory = GetNodeFactory<S, T>(superclass);
                    if (nodeFactory != null)
                        return nodeFactory;
                }
            }
            return null;
        }

        public NodeFactory<S, T> RegisterNodeFactory<S, T>(
            Type kClass, 
            Func<S, ASTTransformer, NodeFactory<S, T>, T> factory
        )
         where T : Node
        {
            var nodeFactory = NodeFactory<S, T>.Single(factory, null, null);
            Factories[kClass] = nodeFactory as NodeFactory<Object, Node>;
            return nodeFactory;
        }

        public NodeFactory<S, T> RegisterMultipleNodeFactory<S, T>(
            Type kClass,
            Func<S, ASTTransformer, NodeFactory<S, T>, List<T>> factory
        )
         where T : Node
        {
            var nodeFactory = new NodeFactory<S, T>(factory, null, null);
            Factories[kClass] = nodeFactory as NodeFactory<Object, Node>;
            return nodeFactory;
        }

        public NodeFactory<S, T> RegisterNodeFactory<S, T>(
            Type kClass,
            Func<S, ASTTransformer, T> factory
        )
         where T : Node
        {
            return RegisterNodeFactory<S, T>(kClass, (source, transformer, _) => factory(source, transformer));            
        }

        //inline fun <reified S : Any, T : Node> registerNodeFactory(
        //crossinline factory: S.(ASTTransformer) -> T?
        //): NodeFactory<S, T> = registerNodeFactory(S::class) { source, transformer, _ -> source.factory(transformer)}
        //public NodeFactory<S, T> RegisterNodeFactory<S, T>(            
        //    Func<S, ASTTransformer, T> factory
        //)
        // where T : Node
        //{
        //    return RegisterNodeFactory<S, T>(typeof(S), (source, transformer, _) => factory(source, transformer));
        //}

        public NodeFactory<S, T> RegisterNodeFactory<S, T>(
            Type kClass,
            Func<S, T> factory
        )
         where T : Node
        {
            return RegisterNodeFactory<S, T>(kClass, (input, _, empty) => factory(input));
        }

        public NodeFactory<S, T> RegisterMultipleNodeFactory<S, T>(
            Type kClass,
            Func<S, List<T>> factory
        )
         where T : Node
        {
            return RegisterMultipleNodeFactory<S, T>(kClass, (input, _, empty) => factory(input));
        }

        public NodeFactory<S, T> RegisterNodeFactory<S, T>()
         where T : Node
        {
            return RegisterNodeFactory<S, T>(typeof(S), typeof(T));
        }

        public NodeFactory<S, Node> NotTranslatedDirectly<S, T>()
            where T : Node
        {
            throw new InvalidOperationException($"A Node of this type (${this.GetType().FullName}) should never be translated directly. " +
                "It is expected that the container will not delegate the translation of this node but it will " +
                "handle it directly");
        }

        internal ParameterValue GetConstructorParameterValue<S, T>(NodeFactory<S, T> thisFactory, Type target, S source, ParameterInfo kParameter)
            where T : Node
        {
            try
            {
                var childNodeFactory = thisFactory.GetChildNodeFactory<object, T, object, S, T>(target, kParameter.Name);
                if (childNodeFactory == null)
                {
                    if (kParameter.IsOptional)
                        return new AbsentParameterValue();
                    
                    throw new InvalidOperationException($"We do not know how to produce parameter {kParameter.Name} for {target}");
                }
                else
                {
                    var childSource = childNodeFactory.Get.Invoke(source);
                    if(childSource == null)
                    {
                        return new AbsentParameterValue();
                    }
                    else if (childSource.IsAList())
                    {                        
                        return new PresentParameterValue(
                                (childSource as IEnumerable<object>).SelectMany(it => TransformIntoNodes(it).ToList())
                            );
                    }
                    else if (childSource is string)
                    {
                        return new PresentParameterValue(childSource as String);
                    }
                    else
                    {
                        if(kParameter.ParameterType == typeof(string) && childSource is IParseTree)
                        {
                            return new PresentParameterValue((childSource as IParseTree).GetText());
                        }
                        else if( kParameter.ParameterType.IsACollection())
                        {
                            return new PresentParameterValue(TransformIntoNodes(childSource));
                        }
                        else
                        {
                            return new PresentParameterValue(Transform(childSource));
                        }
                    }
                }
            }      
            catch ( Exception t )
            {
                throw new InvalidOperationException($"Issue while populating parameter {kParameter.Name} in " +
                                $"constructor {target.FullName}.{target.PreferredConstructor()}",
                            t);
            }
        }

        public NodeFactory<S, T> RegisterNodeFactory<S, T>(Type source, Type target)
         where T : Node
        {
            RegisterKnownClass(target);
            // We are looking for any constructor with does not take parameters or have default
            // values for all its parameters
            var emptyLikeConstructor = target.GetConstructors()
                .Where(it => it.GetParameters().All(param => param.IsOptional));
            var nodeFactory = NodeFactory<S, T>.Single(
                (sourceNF, _, thisFactory) =>
                {
                    if (target.IsSealed)
                    {
                        throw new InvalidOperationException($"Unable to instantiate sealed class {target}");
                    }
                    // We check `childrenSetAtConstruction` and not `emptyLikeConstructor` because, while we set this value
                    // initially based on `emptyLikeConstructor` being equal to null, this can be later changed in `withChild`,
                    // so we should really check the value that `childrenSetAtConstruction` time has when we actually invoke
                    // the factory.
                    if(thisFactory.ChildrenSetAtConstruction)
                    {
                        var constructor = target.PreferredConstructor();
                        // the original Kolasu version is different because the equivalent of the
                        // invoke method in Kotlin accepts parameters associated to the names
                        // while in C# you have to supply the arguments in the correct order
                        var constructorParamValues = constructor.GetParameters().Select(it => (it, cp: GetConstructorParameterValue<S, T>(thisFactory, target, sourceNF, it)))
                                                        //.Where(it => it.cp is PresentParameterValue)                                                        
                                                        .ToDictionary(key => key.it, val => val.cp);
                        //var constructorParamValues = constructor.GetParameters().Select(it => GetConstructorParameterValue<S, T>(thisFactory, target, sourceNF, it)).ToArray();

                        try
                        {
                            var instance = constructor.Invoke(constructorParamValues.Values.ToArray());
                            (instance as Node).Children.ForEach(child => child.Parent = instance as Node);
                            return (T) instance;
                        }
                        catch (Exception t)
                        {
                            throw new InvalidOperationException(
                                $"Invocation of constructor {constructor} failed. " +
                                $"We passed: {constructorParamValues.Select(it => $"{it.Key}={it.Value}").Aggregate("", (text, next) => text += next + ", ", it => it)} ",
                                t);
                        }
                    }
                    else
                    {
                        if (emptyLikeConstructor == null)
                        {
                            throw new InvalidOperationException(
                                "childrenSetAtConstruction is set but there is no empty like " +
                                $"constructor for {target}"
                                );
                        }
                        return Activator.CreateInstance<T>();
                    }
                },
                children: null,
                finalizer: null,
                // If I do not have an emptyLikeConstructor, then I am forced to invoke a constructor with parameters and
                // therefore setting the children at construction time.
                // Note that we are assuming that either we set no children at construction time or we set all of them
                childrenSetAtConstruction: emptyLikeConstructor == null
            );
            Factories[source] = nodeFactory as NodeFactory<Object, Node>;
            return nodeFactory;            
        }

        public NodeFactory<S, T> RegisterIdentityTransformation<S, T>(Type nodeClass)
         where T : Node
        {
            return RegisterNodeFactory<S, T>(nodeClass, (node) => { return (T)(node as Node); });
        }

        public void RegisterKnownClass(Type target)
        {
            var qualifiedName = target.FullName;
            var packageName = "";
            if (qualifiedName != null)
            {
                var endIndex = qualifiedName.LastIndexOf('.');
                if(endIndex >= 0)
                {
                    packageName = qualifiedName.Substring(0, endIndex);
                }
            }
            if (!_knownClasses.ContainsKey(packageName))
            {
                _knownClasses.Add(packageName, new HashSet<Type>());
            }
            var set = _knownClasses[packageName];
            set.Add(target);
        }

        public Issue AddIssue(string message, IssueSeverity severity = IssueSeverity.Error, Position position = null)
        {
            var issue = Issue.Semantic(message, position, severity);
            Issues.Add(issue);
            return issue;
        }
    }
}