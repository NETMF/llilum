//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define COLLECT_PERFORMANCE_DATA_FOR_CONTROLLER

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class Controller
    {
        //
        // State
        //

        private readonly TypeSystemForCodeTransformation m_typeSystem;
        private readonly CallsDataBase                   m_callsDatabase;
        private readonly LinkedList< PhaseDriver >       m_phases;

        private          PhaseDriver                     m_currentPhase;


        //
        // Constructor Methods
        //

        public Controller( TypeSystemForCodeTransformation typeSystem, List<String> phases )
        {
            m_typeSystem = typeSystem;
            m_callsDatabase = new CallsDataBase( );
            m_phases = new LinkedList<PhaseDriver>( );

            //--//

            var lst = new List<PhaseDriver>( );

            CreatePhaseDrivers( lst, phases );

            var orderingForward  = HashTableFactory.NewWithReferenceEquality<PhaseDriver, List<PhaseDriver>>( );
            var orderingBackward = HashTableFactory.NewWithReferenceEquality<PhaseDriver, List<PhaseDriver>>( );

            CollectPhaseOrderingConstraints( lst, orderingForward, orderingBackward );

            ValidateConstraints( lst, orderingForward );

            SortPhaseDrivers( lst, orderingBackward );

            DumpPhaseOrdering( );
        }

        public Controller( TypeSystemForCodeTransformation typeSystem ) : this( typeSystem, null )
        {
        }

        //
        // Helper Methods
        //

        private void CreatePhaseDrivers( List<PhaseDriver> lst, List<String> userDisabledPhases )
        {
            var assembly = System.Reflection.Assembly.GetCallingAssembly();

            foreach(Type t in assembly.GetTypes())
            {
                if(t.IsAbstract == false && t.IsSubclassOf( typeof(PhaseDriver) ))
                {
                    bool fDisabled = false;

                    // check if this phase appears in the list crafted by the user
                    if( userDisabledPhases != null && userDisabledPhases.Count > 0 ) 
                    {
                        foreach( var p in userDisabledPhases ) 
                        {
                            if(t.Name == p)
                            {
                                fDisabled = true;
                                break;
                            }
                        }
                    }

                    // check if phase has been disabled through attribute
                    if( !fDisabled )
                    {
                        // check that this phase is not disabled 
                        // disabled phases cannot run
                        var e = t.CustomAttributes.GetEnumerator( );
                        while( e.MoveNext( ) )
                        {
                            if( e.Current.AttributeType == typeof( PhaseDisabledAttribute ) )
                            {
                                fDisabled = true;
                                break;
                            }
                        }
                    }

                    var phase = (PhaseDriver)Activator.CreateInstance(t, this);

                    phase.Disabled = fDisabled;

                    lst.Add(phase);
                }
            }
        }

        private static void CollectPhaseOrderingConstraints( List             < PhaseDriver                      > lst              ,
                                                             GrowOnlyHashTable< PhaseDriver, List< PhaseDriver > > orderingForward  ,
                                                             GrowOnlyHashTable< PhaseDriver, List< PhaseDriver > > orderingBackward )
        {
            foreach(var phase in lst)
            {
                var phaseAfter = FindPhaseByType( lst, phase.ExecuteAfter );
                if(phaseAfter != null)
                {
                    HashTableWithListFactory.AddUnique( orderingForward , phaseAfter, phase      );
                    HashTableWithListFactory.AddUnique( orderingBackward, phase     , phaseAfter );
                }

                var phaseBefore = FindPhaseByType( lst, phase.ExecuteBefore );
                if(phaseBefore != null)
                {
                    HashTableWithListFactory.AddUnique( orderingForward , phase      , phaseBefore );
                    HashTableWithListFactory.AddUnique( orderingBackward, phaseBefore, phase       );
                }
            }
        }

        private static void ValidateConstraints( List             < PhaseDriver                      > lst             ,
                                                 GrowOnlyHashTable< PhaseDriver, List< PhaseDriver > > orderingForward )
        {
            var set = SetFactory.NewWithReferenceEquality< PhaseDriver >();

            foreach(var phase in lst)
            {
                set.Clear();

                if(FindOrderingLoop( orderingForward, set, phase, phase ))
                {
                    throw TypeConsistencyErrorException.Create( "Found loop in phase ordering constraints" );
                }
            }
        }

        private static bool FindOrderingLoop( GrowOnlyHashTable< PhaseDriver, List< PhaseDriver > > ordering   ,
                                              GrowOnlySet      < PhaseDriver                      > set        ,
                                              PhaseDriver                                           phaseStart ,
                                              PhaseDriver                                           phase      )
        {
            set.Insert( phase );

            List< PhaseDriver > lst;

            if(ordering.TryGetValue( phase, out lst ))
            {
                foreach(var phaseNext in lst)
                {
                    if(phaseNext == phaseStart)
                    {
                        return true;
                    }

                    if(FindOrderingLoop( ordering, set, phaseStart, phaseNext ))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SortPhaseDrivers( List             < PhaseDriver                      > lst              ,
                                       GrowOnlyHashTable< PhaseDriver, List< PhaseDriver > > orderingBackward )
        {
            //
            // Keep looking for a phase that doesn't have any pending predecessors.
            // This is guaranteed to make progress, since we have already proved there are no loops.
            //
            while(lst.Count > 0)
            {
                foreach(var phase in lst)
                {
                    List< PhaseDriver > lstPredecessors;
                    bool                fAdd = true;

                    if(orderingBackward.TryGetValue( phase, out lstPredecessors ))
                    {
                        foreach(var phasePredecessor in lstPredecessors)
                        {
                            if(phasePredecessor.m_node == null)
                            {
                                //
                                // Predecessor not inserted, can't add to the list.
                                //
                                fAdd = false;
                                break;
                            }
                        }
                    }

                    if(fAdd)
                    {
                        phase.m_node = m_phases.AddLast( phase );

                        lst.Remove( phase );
                        break;
                    }
                }
            }
        }

        private void DumpPhaseOrdering()
        {
            Console.WriteLine("");
            Console.WriteLine("Phase Ordering:");

            foreach(var phase in m_phases)
            {
                if (phase.Disabled)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  {0}: {1} (Disabled)", phase.PhaseIndex, phase);
                    Console.ForegroundColor = color;
                }
                else
                {
                    Console.WriteLine("  {0}: {1}", phase.PhaseIndex, phase);
                }
            }
            Console.WriteLine( "" );
            Console.WriteLine( "" );
        }

        //--//

        public void ExecuteSteps( bool fGenerateImageOnly = false )
        {
            Console.WriteLine( "Compiling..." );
            Console.WriteLine( "" );

            //
            // 1) Expand the Call Closure to include all the overrides of virtual methods.
            //    Question: should we wait until we know all the types used by the application?
            //
            // 2) For all the ForceDevirtualization types, find the implementation. Virtual calls should be changed to normal calls.
            //    Question: should we use a ClassExtension-like mechanism? Not for now.
            //
            // 3) ImplicitInstance == you cannot create objects for this type. For now, it's not-implemented!
            //
            // 4) ForceDevirtualization = there should be only one subclass used by the application
            //      => keep track of referenced types, marking some types as unavailable. For now, we could simplify and abort on multiple sub-classes.
            //

            RegisterPhases();

            m_currentPhase = m_phases.First.Value;

            while(m_currentPhase != null)
            {
                if(fGenerateImageOnly && ((m_currentPhase is Phases.GenerateImage) == false))
                {
                    m_currentPhase = m_currentPhase.NextPhase; 
                    continue;
                }

                if(m_currentPhase.Disabled)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine( "Skipping phase: {0}", m_currentPhase );
                    Console.ForegroundColor = color;

                    m_currentPhase = m_currentPhase.NextPhase; 
                    continue;
                }

                using(new Transformations.ExecutionTiming( m_currentPhase.ToString() ))
                {
                    m_typeSystem.NotifyCompilationPhaseInner( m_currentPhase );

#if COLLECT_PERFORMANCE_DATA_FOR_CONTROLLER
                    PerformanceCounters.ContextualTiming timing = new PerformanceCounters.ContextualTiming();
                    timing.Start( this, string.Format( "Phase__{0}", m_currentPhase ) );
#endif

                    var nextPhase = m_currentPhase.Execute();

#if COLLECT_PERFORMANCE_DATA_FOR_CONTROLLER
                    timing.Stop();
#endif

                    if(nextPhase != null)
                    {
                        foreach(var phase in m_phases)
                        {
                            phase.ValidatePhaseMovement( m_currentPhase, nextPhase );
                        }
                    }

                    m_currentPhase = nextPhase;
                }
            }
        }

        private void RegisterPhases()
        {
            var cache = m_typeSystem.GetEnvironmentService< IR.CompilationSteps.DelegationCache >();

            cache.Register( new Handlers.MethodTransformations() );
            cache.Register( new Handlers.ProtectRequiredEntities() );
            cache.Register( new Handlers.WellKnownFieldHandlers() );
            cache.Register( new Handlers.WellKnownMethodHandlers() );

            cache.Register( new Handlers.OperatorHandlers_HighLevel() );
            cache.Register( new Handlers.OperatorHandlers_HighLevelToMidLevel() );
            cache.Register( new Handlers.OperatorHandlers_FromImplicitToExplicitExceptions() );
            cache.Register( new Handlers.OperatorHandlers_ReferenceCountingGarbageCollection() );
            cache.Register( new Handlers.OperatorHandlers_ConvertUnsupportedOperatorsToMethodCalls() );
            cache.Register( new Handlers.OperatorHandlers_ExpandAggregateTypes() );
            cache.Register( new Handlers.OperatorHandlers_MidLevelToLowLevel() );

            cache.Register( new Handlers.Optimizations() );

            m_typeSystem.PlatformAbstraction.RegisterForNotifications( m_typeSystem, cache );
            m_typeSystem.CallingConvention  .RegisterForNotifications( m_typeSystem, cache );
        }

        //--//

        internal T FindPhase< T >() where T : PhaseDriver
        {
            return (T)FindPhaseByType( typeof(T) );
        }

        internal PhaseDriver FindPhaseByType( Type type )
        {
            return FindPhaseByType( m_phases, type );
        }

        private static PhaseDriver FindPhaseByType( IEnumerable< PhaseDriver > lst  ,
                                                    Type                       type )
        {
            if(type != null)
            {
                foreach(var phase in lst)
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

        //
        // Access Methods
        //

        internal TypeSystemForCodeTransformation TypeSystem
        {
            get
            {
                return m_typeSystem;
            }
        }

        internal CallsDataBase CallsDatabase
        {
            get
            {
                return m_callsDatabase;
            }
        }

        public GrowOnlyHashTable< MethodRepresentation, GrowOnlySet< object > > EntitiesReferencedByMethods
        {
            get
            {
                CHECKS.ASSERT( m_currentPhase.PhaseIndex > FindPhase< Phases.ComputeCallsClosure >().PhaseIndex, "Cannot access EntitiesReferencedByMethods property at phase {0}!", m_currentPhase );

                var phase = this.FindPhase< Phases.ComputeCallsClosure >();

                return phase.EntitiesReferencedByMethods;
            }
        }
    }
}
