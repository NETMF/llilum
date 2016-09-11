//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class CallGraph
    {
        class MethodRepresentationNode : ITreeNode<ControlOperator>
        {
            private readonly MethodRepresentation m_method;
            
            //--//

            internal MethodRepresentationNode( MethodRepresentation md )
            {
                m_method = md;
            }

            internal MethodRepresentation Method
            {
                get
                {
                    return m_method;
                }
            }

            public ITreeEdge<ControlOperator>[ ] Predecessors
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }

            public int SpanningTreeIndex
            {
                get
                {
                    throw new NotImplementedException( );
                }

                set
                {
                    throw new NotImplementedException( );
                }
            }

            public ITreeEdge<ControlOperator>[ ] Successors
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }

            public ControlOperator FlowControl
            {
                get
                {
                    throw new NotImplementedException( );
                }
            }
        }

        class CallGraphSpanningTree : SpanningTree<MethodRepresentationNode, ControlOperator> 
        {
            //
            // State
            //

            CallGraph        m_callGraph;
            List< Operator > m_operators;

            //
            // Constructor Methods
            //

            private CallGraphSpanningTree( CallGraph callGraph, MethodRepresentation entry ) : base( )
            {
                m_callGraph = callGraph;
                m_operators = new List<Operator>( );

                var mre = new MethodRepresentationNode( entry ); 

                Visit( mre );
            }
            
            //--//

            protected override void ProcessBefore( ITreeNode<ControlOperator> node )
            {
                var mdn = (MethodRepresentationNode)node;

                mdn.SpanningTreeIndex = m_nodes.Count; m_nodes.Add( mdn );

                var bb1 = node as BasicBlock;

                foreach(Operator op in bb1.Operators)
                {
                    op.SpanningTreeIndex = m_operators.Count; m_operators.Add( op );

                    foreach(var an in op.FilterAnnotations<InvalidationAnnotation>( ))
                    {
                        //AddExpression( an.Target );
                    }

                    foreach(var ex in op.Results)
                    {
                        //AddExpression( ex );
                    }

                    foreach(var ex in op.Arguments)
                    {
                        //AddExpression( ex );
                    }
                }
            }

            internal static void Compute(    CallGraph            callGraph, 
                                             MethodRepresentation entry, 
                                         out Operator[ ]          operators )
            {
                var tree = new CallGraphSpanningTree( callGraph, entry );
                
                operators = tree.m_operators.ToArray( );
            }
        }

        //--//
        //--//
        //--//

        public delegate bool ShouldIncludeInClosureDelegate( MethodRepresentation md );

        //
        // State
        //

        private TypeSystemForCodeTransformation                                         m_typeSystem;

        private GrowOnlyHashTable< MethodRepresentation, List< MethodRepresentation > > m_methods;
        private GrowOnlyHashTable< MethodRepresentation, List< MethodRepresentation > > m_methodsThatRequireStaticConstructors;
        private GrowOnlySet      < MethodRepresentation                               > m_methodsUsedInDelegates;
        //////private Operator[]                                                              m_operators;

        //
        // Constructor Methods
        //

        public CallGraph( TypeSystemForCodeTransformation typeSystem )
        {
            m_typeSystem                           = typeSystem;

            m_methods                              = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, List< MethodRepresentation > >();
            m_methodsThatRequireStaticConstructors = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, List< MethodRepresentation > >();
            m_methodsUsedInDelegates               = SetFactory      .NewWithReferenceEquality< MethodRepresentation                               >();

            //--//

            typeSystem.EnumerateFlowGraphs( delegate ( ControlFlowGraphStateForCodeTransformation cfg )
            {
                var tdCodePointer = m_typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_TypeSystem_CodePointer;

                foreach(Operator op in cfg.DataFlow_SpanningTree_Operators)
                {
                    MethodRepresentationOperator opMd = op as MethodRepresentationOperator;
                    if(opMd != null)
                    {
                        m_methodsUsedInDelegates.Insert( opMd.Method );
                    }

                    FieldOperator opFd = op as FieldOperator;
                    if(opFd != null)
                    {
                        FieldRepresentation fd = opFd.Field;

                        InstanceFieldRepresentation fdI = fd as InstanceFieldRepresentation;
                        if(fdI != null)
                        {
                            fd = fdI.ImplementationOf;
                        }

                        if(fd is StaticFieldRepresentation)
                        {
                            MethodRepresentation mdCCtor = fd.OwnerType.FindDefaultStaticConstructor();
                            if(mdCCtor != null)
                            {
                                MethodRepresentation mdFrom = cfg.Method;

                                if(mdFrom != mdCCtor)
                                {
                                    HashTableWithListFactory.AddUnique( m_methodsThatRequireStaticConstructors, mdFrom, mdCCtor );
                                }
                            }
                        }
                    }

                    foreach(var ex in op.Arguments)
                    {
                        var exConst = ex as ConstantExpression;
                        if(exConst != null && exConst.Type == tdCodePointer)
                        {
                            var od = (DataManager.ObjectDescriptor)exConst.Value;
                            if(od != null)
                            {
                                var cp = (CodePointer)od.Source;

                                var md = m_typeSystem.DataManagerInstance.GetCodePointerFromUniqueID( cp.Target ) as MethodRepresentation;
                                if(md != null)
                                {
                                    m_methodsUsedInDelegates.Insert( md );
                                }
                            }
                        }
                    }
                }
            } );

            typeSystem.EnumerateMethods( delegate ( MethodRepresentation md )
            {
                Analyze( md );
            } );
            
            //////CallGraphSpanningTree.Compute(  this, 
            //////                                typeSystem.GetWellKnownMethod( "Bootstrap_Initialization" ),
            //////                                out m_operators );
        }

        //
        // Helper Methods
        //

        public GrowOnlySet< MethodRepresentation > ComputeClosure( MethodRepresentation           md  ,
                                                                   ShouldIncludeInClosureDelegate dlg )
        {
            GrowOnlySet< MethodRepresentation > set = SetFactory.NewWithReferenceEquality< MethodRepresentation >();

            set.Insert( md );

            ExpandClosure( set, md, dlg );

            return set;
        }

        private void ExpandClosure( GrowOnlySet< MethodRepresentation > set ,
                                    MethodRepresentation                md  ,
                                    ShouldIncludeInClosureDelegate      dlg )
        {
            ExpandClosure( set, md, dlg, m_methods                              );
            ExpandClosure( set, md, dlg, m_methodsThatRequireStaticConstructors );
        }

        private void ExpandClosure( GrowOnlySet< MethodRepresentation >                                     set ,
                                    MethodRepresentation                                                    md  ,
                                    ShouldIncludeInClosureDelegate                                          dlg ,
                                    GrowOnlyHashTable< MethodRepresentation, List< MethodRepresentation > > ht  )
        {
            List< MethodRepresentation > lst;

            if(ht.TryGetValue( md, out lst ))
            {
                foreach(MethodRepresentation mdNext in lst)
                {
                    if(set.Insert( mdNext ) == false)
                    {
                        if(dlg != null && dlg( mdNext ) == false)
                        {
                            continue;
                        }

                        ExpandClosure( set, mdNext, dlg );
                    }
                }
            }
        }

        //--//

        private void Analyze( MethodRepresentation mdFrom )
        {
            List< MethodRepresentation > lst;
            
            if(HashTableWithListFactory.Create( m_methods, mdFrom, out lst ))
            {
                //
                // Already analyzed.
                //
                return;
            }

            ControlFlowGraphStateForCodeTransformation cfgFrom = TypeSystemForCodeTransformation.GetCodeForMethod( mdFrom );
            if(cfgFrom != null)
            {
                foreach(var call in cfgFrom.FilterOperators< CallOperator >())
                {
                    MethodRepresentation mdTo = call.TargetMethod;

                    switch(call.CallType)
                    {
                        case CallOperator.CallKind.Direct:
                        case CallOperator.CallKind.Overridden:
                        case CallOperator.CallKind.OverriddenNoCheck:
                            AddEdge( mdFrom, mdTo );
                            break;

                        case CallOperator.CallKind.Virtual:
                        case CallOperator.CallKind.Indirect:
                            AddVirtualEdge( mdFrom, (VirtualMethodRepresentation)mdTo );
                            break;

                        default:
                            throw TypeConsistencyErrorException.Create( "Unsupported call '{0}' found in '{1}'", call, cfgFrom );
                    }

                    TypeRepresentation td = mdTo.OwnerType;

                    if(td.IsAbstract == false && td.IsSubClassOf( m_typeSystem.WellKnownTypes.System_Delegate, null ))
                    {
                        switch(mdTo.Name)
                        {
                            case "Invoke":
                                foreach(MethodRepresentation mdDlg in m_methodsUsedInDelegates)
                                {
                                    if(mdDlg.MatchSignature( mdTo, null ))
                                    {
                                        AddEdge( mdFrom, mdDlg );
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private void AddVirtualEdge( MethodRepresentation        mdFrom ,
                                     VirtualMethodRepresentation mdTo   )
        {
            //
            // Include all the overrides for this method in all the subclasses already reached by the closure computation.
            //
            int                index = mdTo.FindVirtualTableIndex();
            TypeRepresentation td    = mdTo.OwnerType;

            AddOverrides( mdFrom, td, index );

            //--//

            //
            // The method belongs to an interface, we have to include all the implementations in all the touched types.
            //
            if(td is InterfaceTypeRepresentation)
            {
                InterfaceTypeRepresentation itf      = (InterfaceTypeRepresentation)td;
                int                         itfIndex = mdTo.FindInterfaceTableIndex();

                foreach(TypeRepresentation td2 in m_typeSystem.InterfaceImplementors[itf])
                {
                    MethodRepresentation mdOverride = td2.FindInterfaceTable( itf )[itfIndex];

                    AddEdge( mdFrom, mdOverride );
                }
            }
        }

        private void AddOverrides( MethodRepresentation mdFrom ,
                                   TypeRepresentation   td     ,
                                   int                  index  )
        {
            List< TypeRepresentation > lst;

            if(m_typeSystem.DirectDescendant.TryGetValue( td, out lst ))
            {
                foreach(TypeRepresentation td2 in lst)
                {
                    AddEdge( mdFrom, td2.MethodTable[index] );

                    AddOverrides( mdFrom, td2, index );
                }
            }
        }

        private void AddEdge( MethodRepresentation from ,
                              MethodRepresentation to   )
        {
            HashTableWithListFactory.AddUnique( m_methods, from, to );

            Analyze( to );
        }

        //
        // Access Methods
        //

        public GrowOnlyHashTable< MethodRepresentation, List< MethodRepresentation > > Methods
        {
            get
            {
                return m_methods;
            }
        }

        public GrowOnlyHashTable< MethodRepresentation, List< MethodRepresentation > > MethodsThatRequireStaticConstructors
        {
            get
            {
                return m_methodsThatRequireStaticConstructors;
            }
        }
    }
}
