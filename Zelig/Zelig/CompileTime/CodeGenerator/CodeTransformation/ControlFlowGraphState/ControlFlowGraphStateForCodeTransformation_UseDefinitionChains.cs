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
        class CacheInfo_UseDefinitionChains : CachedInfo
        {
            //
            // State
            //

            internal Operator[]   m_operators;
            internal BitVector[]  m_uses;
            internal BitVector[]  m_definitions;
            internal Operator[][] m_usesAsOperatorSet;
            internal Operator[][] m_definitionsAsOperatorSet;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "UseDefinitionChains" ))
                {
                    VariableExpression[] variables = cfg.DataFlow_SpanningTree_Variables;

                    m_operators                = cfg.DataFlow_SpanningTree_Operators;
                    m_usesAsOperatorSet        = null;
                    m_definitionsAsOperatorSet = null;
                    m_uses                     = BitVector.AllocateBitVectors( variables.Length, m_operators.Length );
                    m_definitions              = BitVector.AllocateBitVectors( variables.Length, m_operators.Length );

                    for(int i = 0; i < m_operators.Length; i++)
                    {
                        Operator op = m_operators[i];

                        foreach(var an in op.FilterAnnotations< InvalidationAnnotation >())
                        {
                            m_definitions[an.Target.SpanningTreeIndex].Set( i );
                        }

                        foreach(var ex in op.Results)
                        {
                            m_definitions[ex.SpanningTreeIndex].Set( i );
                        }

                        foreach(var ex in op.Arguments)
                        {
                            if(ex is VariableExpression)
                            {
                                m_uses[ex.SpanningTreeIndex].Set( i );
                            }
                        }
                    }
                }
            }
        }

        //
        // Helper Methods
        //

        public Operator FindSingleDefinition( Expression ex )
        {
            if(ex is VariableExpression)
            {
                Operator[] defChain = this.DataFlow_DefinitionChains[ex.SpanningTreeIndex];

                if(defChain.Length == 1)
                {
                    return defChain[0];
                }
            }

            return null;
        }

        public Operator FindSingleUse( Expression ex )
        {
            if(ex is VariableExpression)
            {
                Operator[] useChain = this.DataFlow_UseChains[ex.SpanningTreeIndex];

                if(useChain.Length == 1)
                {
                    return useChain[0];
                }
            }

            return null;
        }

        public static Operator FindOrigin( Expression   ex        ,
                                           Operator[][] defChains ,
                                           BitVector    visited   )
        {
            visited.ClearAll();

            while(true)
            {
                var res = ex as VariableExpression;
                if(res != null)
                {
                    Operator singleDef = CheckSingleDefinition( defChains, res );
                    if(singleDef != null)
                    {
                        if(singleDef is AbstractAssignmentOperator && singleDef.Arguments.Length == 1)
                        {
                            if(visited.Set( singleDef.SpanningTreeIndex ) == false)
                            {
                                //
                                // Detected loop.
                                //
                                return null;
                            }

                            ex = singleDef.FirstArgument;
                            continue;
                        }

                        return singleDef;
                    }
                }

                return null;
            }
        }

        //--//

        private static Operator[][] CreateFinalArrays( BitVector[] bits      ,
                                                       Operator[]  operators )
        {
            int          len = bits.Length;
            Operator[][] res = new Operator[len][];

            for(int i = 0; i < len; i++)
            {
                int num = bits[i].Cardinality;

                if(num == 0)
                {
                    res[i] = Operator.SharedEmptyArray;
                }
                else
                {
                    Operator[] res2 = new Operator[num];

                    res[i] = res2;

                    int j = 0;

                    foreach(int pos in bits[i])
                    {
                        res2[j++] = operators[pos];
                    }
                }
            }

            return res;
        }

        //--//

        public IDisposable LockUseDefinitionChains()
        {
            var ci = GetCachedInfo< CacheInfo_UseDefinitionChains >();

            ci.Lock();

            return ci;
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_BitVectorsForUseChains // It's indexed as Operator[<variable index>][<operator index>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_UseDefinitionChains >();

                return ci.m_uses;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public BitVector[] DataFlow_BitVectorsForDefinitionChains // It's indexed as Operator[<variable index>][<operator index>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_UseDefinitionChains >();

                return ci.m_definitions;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public Operator[][] DataFlow_UseChains // It's indexed as Operator[<variable index>][<Operator set>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_UseDefinitionChains >();

                if(ci.m_usesAsOperatorSet == null)
                {
                    ci.m_usesAsOperatorSet = CreateFinalArrays( ci.m_uses, ci.m_operators );
                }

                return ci.m_usesAsOperatorSet;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public Operator[][] DataFlow_DefinitionChains // It's indexed as Operator[<variable index>][<Operator set>]
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_UseDefinitionChains >();

                if(ci.m_definitionsAsOperatorSet == null)
                {
                    ci.m_definitionsAsOperatorSet = CreateFinalArrays( ci.m_definitions, ci.m_operators );
                }

                return ci.m_definitionsAsOperatorSet;
            }
        }

        //--//

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public GrowOnlyHashTable< VariableExpression, Operator[] > DataFlow_UseChainsLookup
        {
            get
            {
                GrowOnlyHashTable< VariableExpression, Operator[] > useChainsLookup = HashTableFactory.NewWithReferenceEquality< VariableExpression, Operator[] >();

                Operator[][] useChains = this.DataFlow_UseChains;

                foreach(VariableExpression var in this.DataFlow_SpanningTree_Variables)
                {
                    useChainsLookup[var] = useChains[var.SpanningTreeIndex];
                }

                return useChainsLookup;
            }
        }

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public GrowOnlyHashTable< VariableExpression, Operator > DataFlow_SingleDefinitionLookup
        {
            get
            {
                GrowOnlyHashTable< VariableExpression, Operator > defLookup = HashTableFactory.NewWithReferenceEquality< VariableExpression, Operator >();

                Operator[][] defChains = this.DataFlow_DefinitionChains;

                foreach(VariableExpression var in this.DataFlow_SpanningTree_Variables)
                {
                    Operator[] defs = defChains[var.SpanningTreeIndex];
                    if(defs.Length == 1)
                    {
                        defLookup[var] = defs[0];
                    }
                }

                return defLookup;
            }
        }
    }
}
