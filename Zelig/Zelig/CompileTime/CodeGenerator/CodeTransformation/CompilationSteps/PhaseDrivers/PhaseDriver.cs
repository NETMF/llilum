//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class PhaseDriver
    {
        public delegate void Notification( PhaseDriver host, TypeSystemForCodeTransformation typeSystem );

        //
        // State
        //

        internal  LinkedListNode< PhaseDriver > m_node;
        protected Controller                    m_context;
        protected Notification                  m_delegationForPhase_Before;
        protected Notification                  m_delegationForPhase_After;

        //
        // Constructor Methods
        //

        protected PhaseDriver( Controller context )
        {
            m_context = context;
        }

        public bool Disabled
        {
            get;
            set;
        }

        //
        // Helper Methods
        //

        public static int CompareOrder( PhaseDriver phase ,
                                        Type        type  )
        {
            if(phase == null)
            {
                return -1;
            }

            var phaseTarget = phase.FindPhaseByType( type );
            if(phaseTarget == null)
            {
                return 1;
            }

            return phase.PhaseIndex.CompareTo( phaseTarget.PhaseIndex );
        }

        public int ComparePositionTo<T>() where T : PhaseDriver
        {
            PhaseDriver target = FindPhase<T>();

            return this.PhaseIndex.CompareTo( target.PhaseIndex );
        }

        public T FindPhase< T >() where T : PhaseDriver
        {
            return (T)FindPhaseByType( typeof(T) );
        }

        public PhaseDriver FindPhaseByType( Type type )
        {
            if(m_node != null)
            {
                foreach(var phase in m_node.List)
                {
                    if(phase.GetType() == type)
                    {
                        return phase;
                    }
                }
            }

            return null;
        }

        //--//

        public void RegisterForNotification( Notification target ,
                                             bool         fFirst )
        {
            if(fFirst)
            {
                m_delegationForPhase_Before += target;
            }
            else
            {
                m_delegationForPhase_After += target;
            }
        }

        //--//

        public PhaseDriver Execute()
        {
            ControlFlowGraphStateForCodeTransformation.SetPhaseForTrace( this );

            this.DelegationCache.HookNotifications( this.TypeSystem, this );

            {
                var dlg = m_delegationForPhase_Before;
                if(dlg != null)
                {
                    m_delegationForPhase_Before = null;

                    dlg( this, this.TypeSystem );
                }
            }

            var res = Run();

            DumpTypeSystemContent();

            {
                var dlg = m_delegationForPhase_After;
                if(dlg != null)
                {
                    m_delegationForPhase_After = null;

                    dlg( this, this.TypeSystem );
                }
            }

            return res;
        }

        public abstract PhaseDriver Run();

        //--//

        protected void ValidateAbstractionLevel( Operator.OperatorLevel maxLevel )
        {
            var phase = this;

            while(phase != null)
            {
                var attrib = ReflectionHelper.GetAttribute< PhaseLimitAttribute >( phase, true );

                if(attrib != null && attrib.Level < maxLevel)
                {
                    // LT72: mask inconsistencies to cope with missign optimizations
                    // throw TypeConsistencyErrorException.Create("Found operator at level {0} when limit was {1}", maxLevel, attrib.Level);

                    // LT72: log instead of throwing exception
                    Console.WriteLine(String.Format("Found operator at level {0} when limit was {1}", maxLevel, attrib.Level));
                    Console.WriteLine();
                }

                phase = phase.PreviousPhase;
            }
        }

        public virtual void ValidatePhaseMovement( PhaseDriver prevPhase ,
                                                   PhaseDriver nextPhase )
        {
            var attrib = ReflectionHelper.GetAttribute< PhaseOrderingAttribute >( this, true );
            if(attrib != null)
            {
                if(attrib.IsPipelineBlock && prevPhase.PhaseIndex > this.PhaseIndex)
                {
                    if(nextPhase.PhaseIndex < this.PhaseIndex)
                    {
                        throw TypeConsistencyErrorException.Create( "Detected attempt to move past a pipeline block: {0} < {1}", nextPhase.GetType().FullName, this.GetType().FullName );
                    }
                }
            }
        }

        internal Operator.OperatorLevel ValidateOperatorLevels( ControlFlowGraphStateForCodeTransformation cfg )
        {
            Operator.OperatorLevel limit = this.MaximumAllowedLevel;
            Operator.OperatorLevel max   = Operator.OperatorLevel.Lowest;
            var                    ts    = this.TypeSystem;

            foreach(Operator op in cfg.DataFlow_SpanningTree_Operators)
            {
                Operator.OperatorLevel level = ts.GetLevel( op );

                if(max < level)
                {
                    max = level;
                }

                if(level > limit)
                {
                    // LT72: mask inconsistencies to cope with missign optimizations
                    // throw TypeConsistencyErrorException.Create( "Found operator at level {0} when limit was {1}: {2}", level, limit, op );
                
                    // LT72: log instead of throwing exception
                    Console.WriteLine(String.Format("Found operator at level {0} when limit was {1}: {2}", level, limit, op));
                    Console.WriteLine();

}
            }

            return max;
        }

        //--//

        protected void PerformSequentialExecutionOfPhase( bool fInline                 ,
                                                          bool fExcludeGenericEntities )
        {
            var rpe = new SingleMethodPhaseExecution( this.TypeSystem, this.DelegationCache, this.CallsDataBase, this );

            if(fInline)
            {
                this.CallsDataBase.ResetCallSites();
            }

            this.TypeSystem.EnumerateMethods( delegate( MethodRepresentation md )
            {
                if(fExcludeGenericEntities)
                {
                    if(md.IsOpenMethod || md.OwnerType.IsOpenType)
                    {
                        return;
                    }
                }

                rpe.Analyze( md );
            } );

            if(fInline)
            {
                while(true)
                {
                    this.CallsDataBase.AnalyzeForInlining();

                    GrowOnlySet< ControlFlowGraphStateForCodeTransformation > touched = this.CallsDataBase.ExecuteInlining( this.TypeSystem );
                    if(touched.Count == 0)
                    {
                        break;
                    }

                    foreach(ControlFlowGraphStateForCodeTransformation cfg in touched)
                    {
                        rpe.Analyze( cfg.Method );
                    }
                }
            }

            ValidateAbstractionLevel( rpe.MaxAbstractionLevelEncountered );
        }

        protected void PerformParallelExecutionOfPhase( bool fInline                 ,
                                                        bool fExcludeGenericEntities )
        {
            var maxLevel = Operator.OperatorLevel.Lowest;

            if(fInline)
            {
                this.CallsDataBase.ResetCallSites();
            }

            ParallelTransformationsHandler.EnumerateMethods( this.TypeSystem, delegate( ParallelTransformationsHandler.Operation phase, MethodRepresentation md, ref object state )
            {
                var rpe = (SingleMethodPhaseExecution)state;

                switch(phase)
                {
                    case ParallelTransformationsHandler.Operation.Initialize:
                        state = new SingleMethodPhaseExecution( this.TypeSystem, this.DelegationCache, this.CallsDataBase, this );
                        break;

                    case ParallelTransformationsHandler.Operation.Execute:
                        if(fExcludeGenericEntities)
                        {
                            if(md.IsOpenMethod || md.OwnerType.IsOpenType)
                            {
                                break;
                            }
                        }

                        rpe.Analyze( md );
                        break;

                    case ParallelTransformationsHandler.Operation.Shutdown:
                        if(maxLevel < rpe.MaxAbstractionLevelEncountered)
                        {
                            maxLevel = rpe.MaxAbstractionLevelEncountered;
                        }
                        break;
                }
            } );

            if(fInline)
            {
                while(true)
                {
                    this.CallsDataBase.AnalyzeForInlining();

                    var touched = this.CallsDataBase.ExecuteInlining( this.TypeSystem );
                    if(touched.Count == 0)
                    {
                        break;
                    }

                    ParallelTransformationsHandler.EnumerateMethods( this.TypeSystem, delegate( ParallelTransformationsHandler.Operation phase, MethodRepresentation md, ref object state )
                    {
                        var rpe = (SingleMethodPhaseExecution)state;

                        switch(phase)
                        {
                            case ParallelTransformationsHandler.Operation.Initialize:
                                state = new SingleMethodPhaseExecution( this.TypeSystem, this.DelegationCache, this.CallsDataBase, this );
                                break;

                            case ParallelTransformationsHandler.Operation.Execute:
                                var cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
                                if(cfg != null && touched.Contains( cfg ))
                                {
                                    rpe.Analyze( md );
                                }
                                break;

                            case ParallelTransformationsHandler.Operation.Shutdown:
                                if(maxLevel < rpe.MaxAbstractionLevelEncountered)
                                {
                                    maxLevel = rpe.MaxAbstractionLevelEncountered;
                                }
                                break;
                        }
                    } );
                }
            }

            ValidateAbstractionLevel( maxLevel );
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            return this.GetType().Name;
        }

        //
        // Access Methods
        //

        public int PhaseIndex
        {
            get
            {
                int index = 0;

                for(var phase = this; phase.PreviousPhase != null; phase =  phase.PreviousPhase)
                {
                    index++;
                }

                return index;
            }
        }

        public PhaseDriver PreviousPhase
        {
            get
            {
                if(m_node != null)
                {
                    var prev = m_node.Previous;
                    if(prev != null)
                    {
                        return prev.Value;
                    }
                }

                return null;
            }
        }

        public PhaseDriver NextPhase
        {
            get
            {
                if(m_node != null)
                {
                    var next = m_node.Next;
                    if(next != null)
                    {
                        return next.Value;
                    }
                }

                return null;
            }
        }

        public Type ExecuteBefore
        {
            get
            {
                var attrib = ReflectionHelper.GetAttribute< PhaseOrderingAttribute >( this, true );

                return attrib != null ? attrib.ExecuteBefore : null;
            }
        }

        public Type ExecuteAfter
        {
            get
            {
                var attrib = ReflectionHelper.GetAttribute< PhaseOrderingAttribute >( this, true );

                return attrib != null ? attrib.ExecuteAfter : null;
            }
        }

        protected TypeSystemForCodeTransformation TypeSystem
        {
            get
            {
                return m_context.TypeSystem;
            }
        }

        public CallsDataBase CallsDataBase
        {
            get
            {
                return m_context.CallsDatabase;
            }
        }

        protected DelegationCache DelegationCache
        {
            get
            {
                return this.TypeSystem.GetEnvironmentService< DelegationCache >();
            }
        }

        protected Operator.OperatorLevel MaximumAllowedLevel
        {
            get
            {
                var attrib = ReflectionHelper.GetAttribute< PhaseLimitAttribute >( this, true );

                if(attrib != null)
                {
                    return attrib.Level;
                }

                var prev = this.PreviousPhase;

                if(prev != null)
                {
                    return prev.MaximumAllowedLevel;
                }

                return Operator.OperatorLevel.Highest;
            }
        }

        protected void DumpTypeSystemContent()
        {
            int fields = 0;
            int methods = 0;
            foreach (var td in m_context.TypeSystem.Types)
            {
                fields += td.Fields.Length;
                methods += td.Methods.Length;
            }
            Console.WriteLine("Types:   {0}", m_context.TypeSystem.Types.Count);
            Console.WriteLine("Fields:  {0}", fields);
            Console.WriteLine("Methods: {0}", methods);
        }
    }
}
