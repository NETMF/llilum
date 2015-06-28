//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define REPORT_SLOW_METHODS


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class PhaseExecution
    {
        public delegate void NotificationOfTransformation            ( NotificationContext nc                                   );
        public delegate void NotificationOfTransformationForAttribute( NotificationContext nc, CustomAttributeRepresentation ca );

        //--//

        public class NotificationContext
        {
            [Flags]
            enum NotificationStatus
            {
                Active   = 0x00000000,
                Skip     = 0x00000001, // Don't invoke any other delegate for this operator.
                Stop     = 0x00000002, // Don't invoke any delegates at all.
                Modified = 0x00000004, // The CFG has been touched.
            }

            //
            // State
            //

            private readonly PhaseExecution                             m_owner;
            private          NotificationStatus                         m_status;
                                                       
            public           MethodRepresentation                       CurrentMethod;
            public           ControlFlowGraphStateForCodeTransformation CurrentCFG;
                                         
            public           Operator                                   CurrentOperator;
            public           object                                     Value;

            //
            // Constructor Methods
            //

            internal NotificationContext( PhaseExecution owner )
            {
                m_owner = owner;
            }

            //
            // Helper methods
            //

            internal void StartScan()
            {
                m_status = NotificationStatus.Active;
            }

            internal void ClearSkip()
            {
                m_status &= ~NotificationStatus.Skip;
            }

            public void StopScan()
            {
                m_status = NotificationStatus.Stop;
            }

            public void MarkAsModified()
            {
                m_status |= NotificationStatus.Skip | NotificationStatus.Modified;
            }

            //--//

            public void SetMaxAbstractionLevelEncountered( Operator.OperatorLevel level )
            {
                if(m_owner.m_maxAbstractionLevelEncountered < level)
                {
                    m_owner.m_maxAbstractionLevelEncountered = level;
                }
            }

            public CallOperator GetOperatorAndThrowIfNotCall()
            {
                CallOperator op = this.CurrentOperator as CallOperator;

                if(op == null)
                {
                    throw TypeConsistencyErrorException.Create( "'{0}' cannot be used with {1}, it's a restricted method", this.Value, this.CurrentOperator );
                }

                return op;
            }

            public TemporaryVariableExpression AllocateTemporary( TypeRepresentation td )
            {
                return this.CurrentCFG.AllocateTemporary( td, null );
            }

            //--//

            public bool IsParameterConstant( CallOperator op    ,
                                             int          index )
            {
                var ex      = op.Arguments[index];
                var exConst = ex as ConstantExpression;

                return (exConst != null);
            }

            public uint ExtractConstantUIntParameter( CallOperator op    ,
                                                      int          index )
            {
                var ex      = op.Arguments[index];
                var exConst = ex as ConstantExpression;

                if(exConst != null)
                {
                    ulong val;
                    
                    if(exConst.GetAsUnsignedInteger( out val ))
                    {
                        return (uint)val;
                    }
                }

                throw TypeConsistencyErrorException.Create( "Expecting integer constant for {0}-th parameter in call to {1}, got {2}", index + 1, op.TargetMethod.ToShortString(), ex );
            }

            public int ExtractConstantIntParameter( CallOperator op    ,
                                                    int          index )
            {
                var ex      = op.Arguments[index];
                var exConst = ex as ConstantExpression;

                if(exConst != null)
                {
                    long val;
                    
                    if(exConst.GetAsSignedInteger( out val ))
                    {
                        return (int)val;
                    }
                }

                throw TypeConsistencyErrorException.Create( "Expecting integer constant for {0}-th parameter in call to {1}, got {2}", index + 1, op.TargetMethod.ToShortString(), ex );
            }

            public bool ExtractConstantBoolParameter( CallOperator op    ,
                                                      int          index )
            {
                var ex      = op.Arguments[index];
                var exConst = ex as ConstantExpression;

                if(exConst != null)
                {
                    ulong val;

                    if(exConst.GetAsUnsignedInteger( out val ))
                    {
                        return val != 0;
                    }
                }

                throw TypeConsistencyErrorException.Create( "Expecting boolean constant for {0}-th parameter in call to {1}, got {2}", index + 1, op.TargetMethod.ToShortString(), ex );
            }

            public PhysicalRegisterExpression GetVariableForRegisterEncoding( uint encoding )
            {
                return GetVariableForRegisterEncoding( this.TypeSystem.WellKnownTypes.System_UIntPtr, encoding );
            }

            public PhysicalRegisterExpression GetVariableForRegisterEncoding( TypeRepresentation td       ,
                                                                              uint               encoding )
            {
                return this.CurrentCFG.AllocateTypedPhysicalRegister( td, this.TypeSystem.PlatformAbstraction.GetRegisterForEncoding( encoding ), null, null, 0 );
            }

            //--//

            //
            // Access methods
            //

            public TypeSystemForCodeTransformation TypeSystem
            {
                get
                {
                    return m_owner.m_typeSystem;
                }
            }

            public PhaseDriver Phase
            {
                get
                {
                    return m_owner.m_phase;
                }
            }

            public bool CanContinue
            {
                get
                {
                    return m_status == NotificationStatus.Active;
                }
            }

            public bool ShouldSkip
            {
                get
                {
                    return (m_status & NotificationStatus.Skip) != 0;
                }
            }

            public bool ShouldStop
            {
                get
                {
                    return (m_status & NotificationStatus.Stop) != 0;
                }
            }

            public bool WasModified
            {
                get
                {
                    return (m_status & NotificationStatus.Modified) != 0;
                }
            }
        }

        protected class NotificationIssuer
        {
            List< NotificationOfTransformation > m_list;

            internal NotificationIssuer()
            {
                m_list = new List< NotificationOfTransformation >();
            }

            internal void Add( NotificationOfTransformation dlg )
            {
                if(m_list.Contains( dlg ) == false)
                {
                    m_list.Add( dlg );
                }
            }

            internal bool Invoke( NotificationContext nc )
            {
                if(nc.ShouldSkip) return false;

                foreach(NotificationOfTransformation dlg in m_list)
                {
                    if(nc.CurrentCFG != null)
                    {
                        nc.CurrentCFG.TraceToFile( dlg );
                    }

                    dlg( nc );

                    if(nc.ShouldSkip) return false;
                    if(nc.ShouldStop) return false;
                }

                return true;
            }
        }

        //
        // State
        //

        protected TypeSystemForCodeTransformation                                                 m_typeSystem;
        protected DelegationCache                                                                 m_cache;
        protected CallsDataBase                                                                   m_callsDatabase;
        protected PhaseDriver                                                                     m_phase;
        protected Operator.OperatorLevel                                                          m_maxAbstractionLevelEncountered;
                                                                                     
        private   NotificationIssuer                                                              m_delegationForFlowGraph_Before;
        private   NotificationIssuer                                                              m_delegationForFlowGraph_After;
        private   GrowOnlyHashTable< BaseRepresentation          , NotificationIssuer           > m_delegationForEntities;
        private   GrowOnlyHashTable< MethodRepresentation        , NotificationIssuer           > m_delegationForCallEntities;
        private   GrowOnlyHashTable< Type                        , NotificationIssuer           > m_delegationForOperators;
        private   GrowOnlyHashTable< Type                        , NotificationIssuer           > m_delegationForArguments;
        protected GrowOnlyHashTable< OptimizationHandlerAttribute, NotificationOfTransformation > m_delegationForOptimization;

        private   GrowOnlySet      < Operator                                                   > m_operatorNotificationHistory;

        //
        // Constructor Methods
        //

        public PhaseExecution( TypeSystemForCodeTransformation typeSystem    ,
                               DelegationCache                 cache         ,
                               CallsDataBase                   callsDatabase ,
                               PhaseDriver                     phase         )
        {
            m_typeSystem                     = typeSystem;
            m_cache                          = cache;
            m_callsDatabase                  = callsDatabase;
            m_phase                          = phase;
            m_maxAbstractionLevelEncountered = Operator.OperatorLevel.Lowest;

            m_delegationForFlowGraph_Before  = new NotificationIssuer();
            m_delegationForFlowGraph_After   = new NotificationIssuer();
            m_delegationForEntities          = HashTableFactory.NewWithReferenceEquality< BaseRepresentation          , NotificationIssuer           >();
            m_delegationForCallEntities      = HashTableFactory.NewWithReferenceEquality< MethodRepresentation        , NotificationIssuer           >();
            m_delegationForOperators         = HashTableFactory.NewWithReferenceEquality< Type                        , NotificationIssuer           >();
            m_delegationForArguments         = HashTableFactory.NewWithReferenceEquality< Type                        , NotificationIssuer           >();
            m_delegationForOptimization      = HashTableFactory.NewWithReferenceEquality< OptimizationHandlerAttribute, NotificationOfTransformation >();
            m_operatorNotificationHistory    = SetFactory      .NewWithReferenceEquality< Operator                                                   >();

            //--//

            cache.HookNotifications( typeSystem, this, phase );
        }

        //
        // Helper Methods
        //

        protected void AnalyzeMethod( MethodRepresentation md )
        {
            NotificationContext nc = new NotificationContext( this );

            nc.CurrentMethod = md;
            nc.CurrentCFG    = TypeSystemForCodeTransformation.GetCodeForMethod( md );

#if REPORT_SLOW_METHODS
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            int iterations = 0;
#endif

            using(new PerformanceCounters.ContextualTiming( nc.CurrentCFG, "AnalyzeMethod" ))
            {
                while(true)
                {
#if REPORT_SLOW_METHODS
                    iterations++;
#endif

                    if(BeginScan    ( nc ) &&
                       ConvertMethod( nc ) &&
                       EndScan      ( nc )  )
                    {
                        break;
                    }
                }
            }

#if REPORT_SLOW_METHODS
            sw.Stop();

            if(sw.ElapsedMilliseconds >= 1500)
            {
                int variablesCount = (nc.CurrentCFG != null ? nc.CurrentCFG.DataFlow_SpanningTree_Variables.Length : 0); 
                int operatorsCount = (nc.CurrentCFG != null ? nc.CurrentCFG.DataFlow_SpanningTree_Operators.Length : 0);

                Console.WriteLine( "NOTICE: Processing for method '{0}' took {1} milliseconds and {2} iterations! [{3} variables and {4} operators]", nc.CurrentMethod.ToShortString(), sw.ElapsedMilliseconds, iterations, variablesCount, operatorsCount );
                Console.WriteLine();
            }
#endif
        }

        protected virtual bool BeginScan( NotificationContext nc )
        {
            m_operatorNotificationHistory.Clear();

            nc.StartScan();

            return m_delegationForFlowGraph_Before.Invoke( nc );
        }

        protected virtual bool ConvertMethod( NotificationContext nc )
        {
            if(nc.CurrentCFG == null)
            {
                //
                // Nothing to do.
                //
                return true;
            }

            //
            // Shortcut the processing of the method, in case we don't need to track too many details.
            //
            {
                bool fGotEntities  = (m_delegationForEntities .Count != 0 || m_delegationForCallEntities.Count != 0);
                bool fGotOperators = (m_delegationForOperators.Count != 0);
                bool fGotArguments = (m_delegationForArguments.Count != 0);

                if(fGotEntities  == false &&
                   fGotArguments == false  )
                {
                    if(fGotOperators)
                    {
                        foreach(Operator op in nc.CurrentCFG.DataFlow_SpanningTree_Operators)
                        {
                            nc.CurrentOperator = op;
                            nc.Value           = op;

                            nc.ClearSkip();

                            if(m_operatorNotificationHistory.Insert( op ) == false)
                            {
                                InvokeByType( m_delegationForOperators, op, nc );
                            }

                            if(nc.ShouldStop) return false;
                            if(nc.ShouldSkip) continue;

                            CallOperator call = op as CallOperator;
                            if(call != null)
                            {
                                m_callsDatabase.RegisterCallSite( call );
                            }
                        }

                        if(nc.WasModified) return false;
                    }

                    return true;
                }
            }

            bool fContinue = Transformations.ScanCodeWithCallback.Execute( m_typeSystem, this, nc.CurrentCFG, delegate( Operator op, object target )
            {
                nc.CurrentOperator = op;
                nc.Value           = target;

                nc.ClearSkip();

                while(true)
                {
                    BaseRepresentation br = target as BaseRepresentation;

                    if(br != null)
                    {
                        NotificationIssuer ni;

                        if(m_delegationForEntities.TryGetValue( br, out ni ))
                        {
                            if(!ni.Invoke( nc )) break;
                        }

                        MethodRepresentation md     = br as MethodRepresentation;
                        CallOperator         opCall = op as CallOperator;

                        if(md != null && opCall != null && opCall.TargetMethod == md)
                        {
                            if(m_delegationForCallEntities.TryGetValue( md, out ni ))
                            {
                                if(!ni.Invoke( nc )) break;
                            }
                        }
                    }

                    if(target != null)
                    {
                        if(!InvokeByType( m_delegationForArguments, target, nc )) break;
                    }

                    if(op != null && m_operatorNotificationHistory.Insert( op ) == false)
                    {
                        if(!InvokeByType( m_delegationForOperators, op, nc )) break;
                    }

                    break;
                }

                if(nc.ShouldStop) return Transformations.ScanCodeWithCallback.CallbackResult.Stop;
                if(nc.ShouldSkip) return Transformations.ScanCodeWithCallback.CallbackResult.SkipToNextOperator;

                CallOperator call = target as CallOperator;
                if(call != null)
                {
                    m_callsDatabase.RegisterCallSite( call );
                }

                return Transformations.ScanCodeWithCallback.CallbackResult.Proceed;
            } );

            if(fContinue)
            {
                fContinue = (nc.WasModified == false);
            }

            return fContinue;
        }

        protected virtual bool EndScan( NotificationContext nc )
        {
            if(!m_delegationForFlowGraph_After.Invoke( nc ))
            {
                return false;
            }

            if(!nc.CanContinue)
            {
                return false;
            }

            ControlFlowGraphStateForCodeTransformation cfg = nc.CurrentCFG;
            if(cfg != null)
            {
                nc.SetMaxAbstractionLevelEncountered( nc.Phase.ValidateOperatorLevels( cfg ) );
            }

            return true;
        }

        //--//

        public void RegisterForNotificationOfOptimization( OptimizationHandlerAttribute ca     ,
                                                           NotificationOfTransformation target )
        {
            m_delegationForOptimization[ca] = target;
        }

        public void RegisterForNotificationOfFlowGraph( NotificationOfTransformation target ,
                                                        bool                         fFirst )
        {
            if(fFirst)
            {
                m_delegationForFlowGraph_Before.Add( target );
            }
            else
            {
                m_delegationForFlowGraph_After.Add( target );
            }
        }

        public void RegisterForNotificationOfOperatorsByType( Type                         cls    ,
                                                              NotificationOfTransformation target )
        {
            RegisterForNotification( m_delegationForOperators, cls, target );
        }

        public void RegisterForNotificationOfOperatorByTypeOfArguments( Type                         cls    ,
                                                                        NotificationOfTransformation target )
        {
            RegisterForNotification( m_delegationForArguments, cls, target );
        }

        public void RegisterForNotificationOfOperatorByEntities( BaseRepresentation           br     ,
                                                                 NotificationOfTransformation target )
        {
            if(br != null)
            {
                RegisterForNotification( m_delegationForEntities, br, target );
            }
        }

        public void RegisterForNotificationOfCallOperatorByEntities( MethodRepresentation         md     ,
                                                                     NotificationOfTransformation target )
        {
            if(md != null)
            {
                RegisterForNotification( m_delegationForCallEntities, md, target );
            }
        }

        //--//

        private void RegisterForNotification<T>( GrowOnlyHashTable< T, NotificationIssuer > ht     ,
                                                 T                                          key    ,
                                                 NotificationOfTransformation               target ) where T : class
        {
            NotificationIssuer res;

            if(ht.TryGetValue( key, out res ) == false)
            {
                res = new NotificationIssuer();

                ht[key] = res;
            }

            res.Add( target );
        }

        //--//

        private bool InvokeByType( GrowOnlyHashTable< Type, NotificationIssuer > ht     ,
                                   object                                        target ,
                                   NotificationContext                           nc     )
        {
            Type type = target.GetType();

            while(type != null)
            {
                NotificationIssuer ni;

                if(ht.TryGetValue( type, out ni ))
                {
                    if(!ni.Invoke( nc )) return false;
                }

                type = type.BaseType;
            }

            return true;
        }

        //--//

        //
        // Access Methods
        //

        public Operator.OperatorLevel MaxAbstractionLevelEncountered
        {
            get
            {
                return m_maxAbstractionLevelEncountered;
            }
        }
    }
}
