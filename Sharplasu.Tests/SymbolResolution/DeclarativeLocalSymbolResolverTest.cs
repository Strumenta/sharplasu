using Strumenta.Sharplasu.Model;
using Strumenta.Sharplasu.SymbolResolution;
using Strumenta.Sharplasu.Testing;
using Strumenta.Sharplasu.Tests.Models.SimpleLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Strumenta.Sharplasu.Traversing;

namespace Strumenta.Sharplasu.Tests.SymbolResolution
{
    [TestClass]
    public class DeclarativeLocalSymbolResolverTest
    {
        public class CompilationUnit : Node
        {
            public List<TypeDecl> Content { get; set; } = new List<TypeDecl>();            

            public CompilationUnit(List<TypeDecl>? content = null)
            {
                Content = content ?? new List<TypeDecl>();
            }
        }

        public class TypeDecl : Node, Named
        {
            public string Name { get; set; }

            public TypeDecl(string name) 
            {
                Name = name;
            }
        }

        private class ClassDecl : TypeDecl
        {
            public ReferenceByName<ClassDecl>? Superclass { get; set; }
            public List<FeatureDecl> Features { get; set; }
            public List<OperationDecl> Operations { get; set; }

            public ClassDecl(string name, ReferenceByName<ClassDecl>? superclass = null, List<FeatureDecl>? features = null, List<OperationDecl>? operations = null)
                : base(name)
            {
                Name = name;
                Superclass = superclass;
                Features = features ?? new List<FeatureDecl>();
                Operations = operations ?? new List<OperationDecl>();
            }
        }

        private class FeatureDecl : Node, Named
        {
            public string Name { get; set; }
            public ReferenceByName<TypeDecl> Type { get; set; }

            public FeatureDecl(string name, ReferenceByName<TypeDecl> type) : base()
            {
                Name = name;
                Type = type;
            }
        }

        private class ParameterDecl : Node, Named
        {
            public string Name { get; set; }
            public ReferenceByName<TypeDecl> Type { get; set; }

            public ParameterDecl(string name, ReferenceByName<TypeDecl> type) : base()
            {
                Name = name;
                Type = type;
            }
        }

        private class OperationDecl : Node, Named
        {
            public string Name { get; set; }
            public List<ParameterDecl> Parameters { get; set; }
            public List<StmtNode> Statements { get; set; }
            public ReferenceByName<TypeDecl>? Returns { get; set; }

            public OperationDecl(string name, List<ParameterDecl>? parameters = null, List<StmtNode>? statements = null, ReferenceByName<TypeDecl>? returns = null) : base()
            {
                Name = name;
                Parameters = parameters ?? new List<ParameterDecl>();
                Statements = statements ?? new List<StmtNode>();
                Returns = returns;
            }
        }

        private class StmtNode : Node, Strumenta.Sharplasu.Model.Statement { }

        private class DeclarationStmt : StmtNode, Named
        {
            public string Name { get; set; }
            public ExprNode Value { get; set; }

            public DeclarationStmt(string name, ExprNode value)
            {
                Name = name;
                Value = value;
            }
        }

        private class AssignmentStmt : StmtNode
        {
            public ExprNode Lhs { get; set; }
            public ExprNode Rhs { get; set; }

            public AssignmentStmt(ExprNode lhs, ExprNode rhs) : base()
            {
                Lhs = lhs;
                Rhs = rhs;
            }
        }

        private class ExprNode : Node, Strumenta.Sharplasu.Model.Expression { }

        private class RefExpr : ExprNode
        {
            public ExprNode Context { get; set; }
            public ReferenceByName<Named> Symbol { get; set; }
            public RefExpr(ExprNode context, ReferenceByName<Named> symbol) : base() 
            {
                Context = context;
                Symbol = symbol;
            }
        }

        private class CallExpr : ExprNode
        {            
            public ReferenceByName<OperationDecl> Operation { get; set; }
            public List<ExprNode> Arguments { get; set; }
            public CallExpr(ReferenceByName<OperationDecl> operation, List<ExprNode>? arguments = null) : base()
            {
                Operation = operation;
                Arguments = arguments ?? new List<ExprNode>();
            }
        }

        private class NewExpr : ExprNode
        {
            public ReferenceByName<ClassDecl> Clazz { get; set; }            
            public NewExpr(ReferenceByName<ClassDecl> clazz) : base()
            {
                Clazz = clazz;
            }
        }
        private CompilationUnit GetCompilationUnit()
        {
            var cu = new CompilationUnit(
                new List<TypeDecl>()
                {
                    new ClassDecl(
                        name: "class_0",
                        features: new List<FeatureDecl>()
                        {
                            new FeatureDecl("feature_0", new ReferenceByName<TypeDecl>("class_1"))
                        },
                        operations: new List<OperationDecl>()
                        {
                            new OperationDecl(
                                name: "operation_0",
                                returns: new ReferenceByName<TypeDecl>("class_0"),
                                statements: new List<StmtNode>()
                                {
                                    new AssignmentStmt(
                                        lhs: new RefExpr(
                                            context: new CallExpr(
                                                    new ReferenceByName<OperationDecl>("operation_0")
                                                ),
                                            symbol: new ReferenceByName<Named>("feature_0")
                                        ),
                                        rhs: new RefExpr(
                                            context: new CallExpr(
                                                    new ReferenceByName<OperationDecl>("operation_0")
                                                ),
                                            symbol: new ReferenceByName<Named>("feature_0")
                                        )
                                    )
                                }
                            )
                        }
                    ),
                    new ClassDecl("class_1")
                }
            );
            cu.AssignParents();
            return cu;
        }
        
        private DeclarativeLocalSymbolResolver GetFullSymbolResolver()
        {
            return DeclarativeLocalSymbolResolver.SymbolResolver(
                    (decl) =>
                    {
                        decl.ScopeFor(typeof(ClassDecl).GetProperty("Superclass"), (CompilationUnit compilationUnit) =>
                        {
                            var scope = new Scope();
                            compilationUnit.Content.OfType<ClassDecl>().ToList().ForEach(it => scope.Define(it));
                            return scope;
                        });

                        decl.ScopeFor(typeof(FeatureDecl).GetProperty("Type"), (CompilationUnit compilationUnit) =>
                        {
                            var scope = new Scope();
                            compilationUnit.Content.ForEach(it => scope.Define(it));
                            return scope;
                        });

                        decl.ScopeFor(typeof(RefExpr).GetProperty("Symbol"), (RefExpr refExpr) =>
                        {
                            Scope? scope = null;
                            if (refExpr.Context != null)
                            {
                                scope = decl.GetScope(typeof(RefExpr).GetProperty("Symbol"), refExpr.Context);
                            }                            
                            return scope;
                        });

                        decl.ScopeFor(typeof(RefExpr).GetProperty("Symbol"), (CallExpr callExpr) =>
                        {
                            Scope scope = new Scope();
                            if (!callExpr.Operation.Resolved)
                            {
                                decl.ResolveProperty(typeof(CallExpr).GetProperty("Operation"), callExpr);
                            }
                            if (callExpr.Operation.Referred != null && !callExpr.Operation.Referred!.Returns!.Resolved)
                            {
                                decl.ResolveProperty(typeof(OperationDecl).GetProperty("Returns"), callExpr.Operation.Referred);
                            }
                            if (callExpr.Operation.Referred != null && callExpr.Operation.Referred!.Returns!.Referred != null)
                            {
                                var returnType = callExpr.Operation.Referred.Returns.Referred;
                                if (returnType is ClassDecl)
                                {
                                    (returnType as ClassDecl)?.Features.ForEach(it => scope.Define(it));
                                    (returnType as ClassDecl)?.Operations.ForEach(it => scope.Define(it));
                                }                                
                            }
                            return scope;
                        });

                        decl.ScopeFor(typeof(RefExpr).GetProperty("Symbol"), (NewExpr newExpr) =>
                        {
                            Scope scope = new Scope();
                            if (!newExpr.Clazz.Resolved)
                            {
                                decl.ResolveProperty(typeof(NewExpr).GetProperty("Clazz"), newExpr);
                            }                            
                            if (newExpr.Clazz.Referred != null)
                            {
                                var returnType = newExpr.Clazz.Referred;
                                if (returnType is ClassDecl)
                                {
                                    returnType.Features.ForEach(it => scope.Define(it));
                                    returnType.Operations.ForEach(it => scope.Define(it));
                                }
                            }
                            return scope;
                        });

                        decl.ScopeFor(typeof(CallExpr).GetProperty("Operation"), (CallExpr callExpr) =>
                        {
                            Scope scope = null;
                            var it = callExpr.FindAncestorOfType<ClassDecl>();
                            if (it != null)
                                decl.GetScope(typeof(CallExpr).GetProperty("Operation"), it);
                            return scope;
                        });

                        decl.ScopeFor(typeof(CallExpr).GetProperty("Operation"), (ClassDecl classDecl) =>
                        {
                            Scope scope = new Scope();
                            classDecl.Operations.ForEach(it => scope.Define(it));
                            return scope;
                        });

                        decl.ScopeFor(typeof(OperationDecl).GetProperty("Returns"), (OperationDecl operationDecl) =>
                        {
                            Scope scope = null;
                            var it = operationDecl.FindAncestorOfType<CompilationUnit>();
                            if (it != null)
                                decl.GetScope(typeof(OperationDecl).GetProperty("Returns"), it);
                            return scope;
                        });

                        decl.ScopeFor(typeof(OperationDecl).GetProperty("Returns"), (CompilationUnit compilationUnit) =>
                        {
                            Scope scope = new Scope();
                            compilationUnit.Content.ForEach(it => scope.Define(it));
                            return scope;
                        });
                    }
                );
        }

        private DeclarativeLocalSymbolResolver GetPartialSymbolResolver()
        {            
            return DeclarativeLocalSymbolResolver.SymbolResolver(
                    (decl) =>
                    {
                        decl.ScopeFor(typeof(ClassDecl).GetProperty("Superclass"), (CompilationUnit compilationUnit) =>
                        {
                            var scope = new Scope();
                            compilationUnit.Content.OfType<ClassDecl>().ToList().ForEach(it => scope.Define(it));
                            return scope;
                        });
                    }
                );
        }

        [TestMethod]
        public void TestSymbolResolution()
        {
            var cu = GetCompilationUnit();            
            cu.AssertNotAllReferencesResolved();
            GetFullSymbolResolver().ResolveSymbols(cu);
            cu.AssertAllReferencesResolved();
        }

        [TestMethod]
        public void TestIncrementalSymbolResolutionDevelopment()
        {
            var cu = GetCompilationUnit();
            // pre-condition - v1
            cu.AssertNotAllReferencesResolved(typeof(ClassDecl).GetProperty("Superclass"));
            cu.AssertNotAllReferencesResolved(typeof(FeatureDecl).GetProperty("Type"));
            cu.AssertNotAllReferencesResolved(typeof(RefExpr).GetProperty("Symbol"));
            cu.AssertNotAllReferencesResolved(typeof(CallExpr).GetProperty("Operation"));
            cu.AssertNotAllReferencesResolved(typeof(OperationDecl).GetProperty("Returns"));
            cu.AssertNotAllReferencesResolved();
            // resolution - v1
            GetPartialSymbolResolver().ResolveSymbols(cu);
            // post-condition - v1 (pre-condition - v2)
            cu.AssertAllReferencesResolved(typeof(ClassDecl).GetProperty("Superclass"));
            cu.AssertNotAllReferencesResolved(typeof(FeatureDecl).GetProperty("Type"));
            cu.AssertNotAllReferencesResolved(typeof(RefExpr).GetProperty("Symbol"));
            cu.AssertNotAllReferencesResolved(typeof(CallExpr).GetProperty("Operation"));
            cu.AssertNotAllReferencesResolved(typeof(OperationDecl).GetProperty("Returns"));
            cu.AssertNotAllReferencesResolved();
            // resolution - v2;
            GetFullSymbolResolver().ResolveSymbols(cu);
            // post-condition - v2;
            cu.AssertAllReferencesResolved(typeof(ClassDecl).GetProperty("Superclass"));
            cu.AssertAllReferencesResolved(typeof(FeatureDecl).GetProperty("Type"));
            cu.AssertAllReferencesResolved(typeof(RefExpr).GetProperty("Symbol"));
            cu.AssertAllReferencesResolved(typeof(CallExpr).GetProperty("Operation"));
            cu.AssertAllReferencesResolved(typeof(OperationDecl).GetProperty("Returns"));
            cu.AssertAllReferencesResolved();            
        }
    }
}
