//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public partial class ControlFlowGraphStateForCodeTransformation
    {
        class CacheInfo_SpanningTree : CachedInfo
        {
            //
            // State
            //

            internal BasicBlock[]           m_basicBlocks;
            internal BasicBlock[]           m_ancestors;
            internal Operator[]             m_operators;
            internal VariableExpression[]   m_variables;
            internal VariableExpression[][] m_variablesByStorage;
            internal VariableExpression[][] m_variablesByAggregate;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "SpanningTree" ))
                {
                    while(true)
                    {
                        cfg.UpdateFlowInformation();

                        foreach(BasicBlock bb in cfg.m_basicBlocks)
                        {
                            bb.SpanningTreeIndex = -1;
                        }

                        if(m_variables != null)
                        {
                            foreach(VariableExpression var in m_variables)
                            {
                                var.SpanningTreeIndex = -1;
                            }
                        }

                        //--//

                        m_ancestors            = null;
                        m_variablesByStorage   = null;
                        m_variablesByAggregate = null;

                        DataFlow.ControlTree.SpanningTree.Compute( cfg, out m_basicBlocks, out m_operators, out m_variables );

#if DEBUG
                        foreach(Operator op in m_operators)
                        {
                            CHECKS.ASSERT( op.BasicBlock.Owner == cfg, "Operator {0} does not belong to {1}", op, cfg.m_md );
                        }
#endif

                        //--//

                        //
                        // If any basic block got removed from the flow graph, it will become unreachable.
                        // Detect that and recompute the flow information.
                        //
                        bool fUpdateFlowInformation = false;


                        foreach(Operator op in m_operators)
                        {
                            PhiOperator phiOp = op as PhiOperator;
                            if(phiOp != null)
                            {
                                fUpdateFlowInformation |= phiOp.AdjustLinkage();
                            }
                        }

                        foreach(BasicBlock bb in cfg.m_basicBlocks)
                        {
                            if(bb.SpanningTreeIndex == -1)
                            {
                                CHECKS.ASSERT( ArrayUtility.FindReferenceInNotNullArray( m_basicBlocks, bb ) == -1, "{0} belongs in the spanning tree for {1} without an index", bb, cfg );

                                bb.Delete();

                                fUpdateFlowInformation = true;
                            }
                        }

                        cfg.m_basicBlocks = m_basicBlocks;

                        if(!fUpdateFlowInformation) break;
                    }
                }
            }
        }

        //
        // Constructor Methods
        //

        //--//

        //
        // Helper Methods
        //

        private static VariableExpression[][] BuildStorageTable( VariableExpression[] variables )
        {
            int                    varNum             = variables.Length;
            VariableExpression[][] variablesByStorage = new VariableExpression[varNum][];

            for(int varIdx = 0; varIdx < varNum; varIdx++)
            {
                if(variablesByStorage[varIdx] == null)
                {
                    VariableExpression var = variables[varIdx];

                    if(var.AliasedVariable is LowLevelVariableExpression)
                    {
                        VariableExpression[] array = VariableExpression.SharedEmptyArray;

                        //
                        // We only need to scan forward.
                        // If we got here, it was because we didn't encounter an equivalent storage before this point.
                        //
                        for(int varIdx2 = varIdx; varIdx2 < varNum; varIdx2++)
                        {
                            VariableExpression var2 = variables[varIdx2];

                            if(var2 != null && var.IsTheSamePhysicalEntity( var2 ))
                            {
                                array = ArrayUtility.AppendToNotNullArray( array, var2 );
                            }
                        }

                        //
                        // Propagate the table to all the variables mapped to the same storage.
                        //
                        foreach(VariableExpression var3 in array)
                        {
                            variablesByStorage[var3.SpanningTreeIndex] = array;
                        }
                    }
                    else
                    {
                        variablesByStorage[varIdx] = new VariableExpression[] { var };
                    }
                }
            }

            return variablesByStorage;
        }

        private static VariableExpression[][] BuildAggregationTable( VariableExpression[] variables )
        {
            int                    varNum               = variables.Length;
            VariableExpression[][] variablesByAggregate = new VariableExpression[varNum][];

            for(int varIdx = 0; varIdx < varNum; varIdx++)
            {
                if(variablesByAggregate[varIdx] == null)
                {
                    VariableExpression var = variables[varIdx];

                    VariableExpression[] array = VariableExpression.SharedEmptyArray;

                    //
                    // We only need to scan forward.
                    // If we got here, it was because we didn't encounter an equivalent storage before this point.
                    //
                    for(int varIdx2 = varIdx; varIdx2 < varNum; varIdx2++)
                    {
                        VariableExpression var2 = variables[varIdx2];

                        if(var2 != null && var.IsTheSameAggregate( var2 ))
                        {
                            array = ArrayUtility.AppendToNotNullArray( array, var2 );
                        }
                    }

                    //
                    // Propagate the table to all the variables mapped to the same storage.
                    //
                    foreach(VariableExpression var3 in array)
                    {
                        variablesByAggregate[var3.SpanningTreeIndex] = array;
                    }
                }
            }

            return variablesByAggregate;
        }

        //--//

        public IDisposable LockSpanningTree()
        {
            var ci = GetCachedInfo< CacheInfo_SpanningTree >();

            ci.Lock();

            return ci;
        }

        //--//

        public OperatorEnumeratorProvider< T > FilterOperators< T >() where T : Operator
        {
            return new OperatorEnumeratorProvider< T >( this.DataFlow_SpanningTree_Operators );
        }

        public struct OperatorEnumeratorProvider< T > where T : Operator
        {
            //
            // State
            //

            private readonly Operator[] m_values;

            //
            // Constructor Methods
            //

            internal OperatorEnumeratorProvider( Operator[] values )
            {
                m_values = values;
            }

            //
            // Helper Methods
            //

            public OperatorEnumerator< T > GetEnumerator()
            {
                return new OperatorEnumerator< T >( m_values );
            }
        }

        public struct OperatorEnumerator< T > where T : Operator
        {
            //
            // State
            //

            private readonly Operator[] m_values;
            private T                   m_current;
            private int                 m_index;

            //
            // Constructor Methods
            //

            internal OperatorEnumerator( Operator[] values )
            {
                m_values  = values;
                m_current = null;
                m_index   = 0;
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                while(m_index < m_values.Length)
                {
                    T res = m_values[m_index++] as T;
                    if(res != null)
                    {
                        if(res.BasicBlock != null) // Skip deleted operators.
                        {
                            m_current = res;
                            return true;
                        }
                    }
                }

                m_current = null;
                return false;
            }

            public T Current
            {
                get
                {
                    return m_current;
                }
            }
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlock[] DataFlow_SpanningTree_BasicBlocks
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_SpanningTree >();

                return ci.m_basicBlocks;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BasicBlock[] DataFlow_SpanningTree_Ancestors
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_SpanningTree >();

                if(ci.m_ancestors == null)
                {
                    ci.m_ancestors = DataFlow.ControlTree.SpanningTree.ComputeAncestors( ci.m_basicBlocks );
                }

                return ci.m_ancestors;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public Operator[] DataFlow_SpanningTree_Operators
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_SpanningTree >();

                return ci.m_operators;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public VariableExpression[] DataFlow_SpanningTree_Variables
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_SpanningTree >();

                return ci.m_variables;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public VariableExpression[][] DataFlow_SpanningTree_VariablesByStorage // It's indexed as VariableExpression[<variable index>][<all the variables that have the same storage location>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_SpanningTree >();

                if(ci.m_variablesByStorage == null)
                {
                    ci.m_variablesByStorage = BuildStorageTable( ci.m_variables );
                }

                return ci.m_variablesByStorage;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public VariableExpression[][] DataFlow_SpanningTree_VariablesByAggregate // It's indexed as VariableExpression[<variable index>][<all the variables that have the same storage location>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_SpanningTree >();

                if(ci.m_variablesByAggregate == null)
                {
                    ci.m_variablesByAggregate = BuildAggregationTable( ci.m_variables );
                }

                return ci.m_variablesByAggregate;
            }
        }
    }
}
