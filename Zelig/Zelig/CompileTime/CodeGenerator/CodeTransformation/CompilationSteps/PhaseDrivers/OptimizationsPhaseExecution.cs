//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define OPTIMIZATION_REPORT_SLOW_METHODS


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class OptimizationsPhaseExecution : SingleMethodPhaseExecution
    {
        class State
        {
            //
            // State
            //

            private GrowOnlySet< OptimizationHandlerAttribute > m_dontExecute;

            //--//

            internal bool CanExecute( OptimizationHandlerAttribute ca )
            {
                if(m_dontExecute != null)
                {
                    if(m_dontExecute.Contains( ca ))
                    {
                        return false;
                    }
                }

                return true;
            }

            internal void SetAsDontExecute( OptimizationHandlerAttribute ca )
            {
                if(m_dontExecute == null)
                {
                    m_dontExecute = SetFactory.NewWithReferenceEquality< OptimizationHandlerAttribute >();
                }

                m_dontExecute.Insert( ca );
            }
        }

        //
        // State
        //

        private GrowOnlyHashTable< MethodRepresentation, State > m_methods;
        private OptimizationHandlerAttribute[]                   m_order;

        //
        // Constructor Methods
        //

        public OptimizationsPhaseExecution( TypeSystemForCodeTransformation typeSystem    ,
                                            DelegationCache                 cache         ,
                                            CallsDataBase                   callsDatabase ,
                                            PhaseDriver                     phase         ) : base( typeSystem, cache, callsDatabase, phase )
        {
            m_methods = HashTableFactory.NewWithReferenceEquality< MethodRepresentation, State >();
            m_order   = new OptimizationHandlerAttribute[0];

            //--//

            CompileOrder();
        }

        //
        // Helper Methods
        //

        public override void Analyze( MethodRepresentation md )
        {
            ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
            if(cfg != null)
            {
                State state;

                if(m_methods.TryGetValue( md, out state ) == false)
                {
                    state = new State();

                    m_methods[md] = state;
                }

                NotificationContext nc = new NotificationContext( this );

                nc.CurrentMethod = md;
                nc.CurrentCFG    = cfg;

#if OPTIMIZATION_REPORT_SLOW_METHODS
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                sw.Start();

                int iterations          = 0;
                int iterations_SSA_In   = 0;
                int iterations_SSA_Out  = 0;
                int iterations_SSA_Undo = 0;
#endif

                while(true)
                {
#if OPTIMIZATION_REPORT_SLOW_METHODS
                    iterations++;
#endif

                    nc.StartScan();

                    foreach(OptimizationHandlerAttribute ca in m_order)
                    {
                        if(state.CanExecute( ca ))
                        {
                            if(ca.RunInExtendedSSAForm)
                            {
#if OPTIMIZATION_REPORT_SLOW_METHODS
                                iterations_SSA_In++;
#endif

                                Transformations.StaticSingleAssignmentForm.InsertPiOperators( cfg );

                                Transformations.StaticSingleAssignmentForm.ConvertInto( cfg );
                            }
                            else if(ca.RunInSSAForm)
                            {
#if OPTIMIZATION_REPORT_SLOW_METHODS
                                iterations_SSA_In++;
#endif

                                Transformations.StaticSingleAssignmentForm.RemovePiOperators( cfg );

                                Transformations.StaticSingleAssignmentForm.ConvertInto( cfg );
                            }
                            else if(ca.RunInNormalForm)
                            {
#if OPTIMIZATION_REPORT_SLOW_METHODS
                                iterations_SSA_Out++;
#endif

                                Transformations.StaticSingleAssignmentForm.ConvertOut( cfg, true );
                            }

                            Transformations.RemoveDeadCode.Execute( cfg, false );

                            NotificationOfTransformation dlg = m_delegationForOptimization[ca];

                            cfg.TraceToFile( "Optimization-Pre - " + dlg.Method );

                            dlg( nc );

                            cfg.TraceToFile( "Optimization-Post" );

                            if(Transformations.StaticSingleAssignmentForm.ShouldTransformInto( cfg ))
                            {
#if OPTIMIZATION_REPORT_SLOW_METHODS
                                iterations_SSA_Undo++;
#endif

                                //
                                // Since the graph is no longer in SSA form, it's best to properly convert it out.
                                //
                                Transformations.StaticSingleAssignmentForm.ConvertOut( cfg, true );
                            }

                            if(ca.RunOnce)
                            {
                                state.SetAsDontExecute( ca );
                            }

                            if(nc.CanContinue == false) break;
                        }
                    }

                    if(nc.CanContinue)
                    {
                        break;
                    }
                }

#if OPTIMIZATION_REPORT_SLOW_METHODS
                sw.Stop();

                if(sw.ElapsedMilliseconds >= 1500)
                {
                    Console.WriteLine( "NOTICE: Optimizations for method '{0}' took {1} milliseconds and {2} iterations! [{3} variables and {4} operators]", cfg.Method.ToShortString(), sw.ElapsedMilliseconds, iterations, cfg.DataFlow_SpanningTree_Variables.Length, cfg.DataFlow_SpanningTree_Operators.Length );
                    Console.WriteLine( "NOTICE: SSA In = {0}, SSA Out = {1}, SSA Undo = {2}", iterations_SSA_In, iterations_SSA_Out, iterations_SSA_Undo );
                    Console.WriteLine();
                }
#endif
            }
        }

        //--//

        private void CompileOrder()
        {
            foreach(OptimizationHandlerAttribute ca in m_delegationForOptimization.Keys)
            {
                if(ca.RunBefore != null)
                {
                    OptimizationHandlerAttribute ca2 = FindTarget( ca, ca.RunBefore );
                    if(ca2 == null)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot find optimization '{0}', needed for proper sequencing of {1}", ca.RunBefore, m_delegationForOptimization[ca].Method );
                    }

                    ca.caToRunAfterThis = ca2;
                }

                if(ca.RunAfter != null)
                {
                    OptimizationHandlerAttribute ca2 = FindTarget( ca, ca.RunAfter );
                    if(ca2 == null)
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot find optimization '{0}', needed for proper sequencing of {1}", ca.RunAfter, m_delegationForOptimization[ca].Method );
                    }

                    ca.caToRunBeforeThis = ca2;
                }
            }

            foreach(OptimizationHandlerAttribute ca in m_delegationForOptimization.Keys)
            {
                if(ArrayUtility.FindReferenceInNotNullArray( m_order, ca ) < 0)
                {
                    if(ca.caToRunBeforeThis != null)
                    {
                        if(ArrayUtility.FindReferenceInNotNullArray( m_order, ca.caToRunBeforeThis  ) < 0)
                        {
                            m_order = ArrayUtility.AppendToNotNullArray( m_order, ca.caToRunBeforeThis );
                        }
                    }

                    if(ca.caToRunAfterThis != null)
                    {
                        int pos = ArrayUtility.FindReferenceInNotNullArray( m_order, ca.caToRunAfterThis );

                        if(pos >= 0)
                        {
                            m_order = ArrayUtility.InsertAtPositionOfNotNullArray( m_order, pos, ca );
                            continue;
                        }
                    }

                    m_order = ArrayUtility.AppendToNotNullArray( m_order, ca );
                }
            }

            //
            // Ensure we have the correct ordering.
            //
            for(int pos = 0; pos < m_order.Length; pos++)
            {
                OptimizationHandlerAttribute ca = m_order[pos];

                if(ca.caToRunAfterThis != null && ArrayUtility.FindReferenceInNotNullArray( m_order, ca.caToRunAfterThis ) < pos)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot find scheduling to put {0} after {1}", m_delegationForOptimization[ca].Method, m_delegationForOptimization[ca.caToRunAfterThis].Method );
                }

                if(ca.caToRunBeforeThis != null && ArrayUtility.FindReferenceInNotNullArray( m_order, ca.caToRunBeforeThis ) > pos)
                {
                    throw TypeConsistencyErrorException.Create( "Cannot find scheduling to put {0} before {1}", m_delegationForOptimization[ca].Method, m_delegationForOptimization[ca.caToRunBeforeThis].Method );
                }
            }
        }

        private OptimizationHandlerAttribute FindTarget( OptimizationHandlerAttribute caSource ,
                                                         string                       id       )
        {
            int    pos = id.IndexOf( '.' );
            string nameType;
            string nameMethod;

            if(pos < 0)
            {
                nameType   = m_delegationForOptimization[caSource].Method.DeclaringType.Name;
                nameMethod = id;
            }
            else
            {
                nameType   = id.Substring( 0, pos   );
                nameMethod = id.Substring(    pos+1 );
            }

            foreach(OptimizationHandlerAttribute ca in m_delegationForOptimization.Keys)
            {
                if(ca != caSource)
                {
                    NotificationOfTransformation dlg = m_delegationForOptimization[ca];

                    System.Reflection.MethodInfo mi = dlg.Method;

                    if(nameMethod == mi.              Name &&
                       nameType   == mi.DeclaringType.Name  )
                    {
                        return ca;
                    }
                }
            }

            return null;
        }

        //--//

        //
        // Access Methods
        //

    }
}
