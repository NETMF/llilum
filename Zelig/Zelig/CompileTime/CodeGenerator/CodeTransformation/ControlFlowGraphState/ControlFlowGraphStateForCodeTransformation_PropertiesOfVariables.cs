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
        class CacheInfo_PropertiesOfVariables : CachedInfo
        {
            //
            // State
            //

            internal VariableExpression.Property[] m_propertiesOfVariables;

            //
            // Helper Methods
            //

            protected override void Update()
            {
                ControlFlowGraphStateForCodeTransformation cfg = (ControlFlowGraphStateForCodeTransformation)m_owner;

                using(new PerformanceCounters.ContextualTiming( cfg, "PropertiesOfVariables" ))
                {
                    Operator                   [] operators  = cfg.DataFlow_SpanningTree_Operators;
                    VariableExpression         [] variables  = cfg.DataFlow_SpanningTree_Variables;
                    VariableExpression.Property[] properties = new VariableExpression.Property[variables.Length];

                    foreach(VariableExpression var in variables)
                    {
                        if(var.AliasedVariable is PhysicalRegisterExpression)
                        {
                            properties[var.SpanningTreeIndex] |= VariableExpression.Property.PhysicalRegister;
                        }
                    }

                    foreach(Operator op in operators)
                    {
                        if(op is AddressAssignmentOperator)
                        {
                            ExpandAddressTakenThroughFragments( cfg, variables, properties, op.FirstArgument );
                        }
                    }

                    m_propertiesOfVariables = properties;
                }
            }

            private static void ExpandAddressTakenThroughFragments( ControlFlowGraphStateForCodeTransformation cfg        ,
                                                                    VariableExpression[]                       variables  ,
                                                                    VariableExpression.Property[]              properties ,
                                                                    Expression                                 ex         )
            {
                if(ex is VariableExpression)
                {
                    CHECKS.ASSERT( ex.SpanningTreeIndex != -1, "Encountered variable not belonging to the spanning tree: {0}", ex );

                    if((properties[ex.SpanningTreeIndex] & VariableExpression.Property.AddressTaken) == 0)
                    {
                        properties[ex.SpanningTreeIndex] |= VariableExpression.Property.AddressTaken;

                        //
                        // Expand from fragment to original variable.
                        //
                        Expression[] fragments;

                        if(ex is LowLevelVariableExpression)
                        {
                            LowLevelVariableExpression exLow = (LowLevelVariableExpression)ex;

                            fragments = cfg.GetFragmentsForExpression( exLow.SourceVariable );
                        }
                        else
                        {
                            fragments = cfg.GetFragmentsForExpression( ex );
                        }

                        if(fragments != null)
                        {
                            //
                            // Expand to all the fragments.
                            //
                            foreach(Expression var in fragments)
                            {
                                ExpandAddressTakenThroughFragments( cfg, variables, properties, var );
                            }
                        }
                    }
                }
            }
        }

        //
        // Helper Methods
        //

        public IDisposable LockPropertiesOfVariables()
        {
            var ci = GetCachedInfo< CacheInfo_PropertiesOfVariables >();

            ci.Lock();

            return ci;
        }

        //
        // Access Methods
        //

        [System.Diagnostics.DebuggerBrowsable( System.Diagnostics.DebuggerBrowsableState.Never )]
        public VariableExpression.Property[] DataFlow_PropertiesOfVariables
        {
            get
            {
                var ci = GetCachedInfo< CacheInfo_PropertiesOfVariables >();

                return ci.m_propertiesOfVariables;
            }
        }

        //
        // Debug Methods
        //

        public override string ToPrettyString( Operator op )
        {
            return PrettyDumper.Dump( op );
        }
    }
}
