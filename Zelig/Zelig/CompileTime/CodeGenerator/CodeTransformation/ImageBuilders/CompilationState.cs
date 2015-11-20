//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public abstract partial class CompilationState : IDisposable
    {
        protected class StateForLocalStackAssignment
        {
            //
            // State
            //

            internal StackLocationExpression m_var;
            internal BitVector               m_liveness                         = new BitVector();
            internal BitVector               m_interferesWith                   = new BitVector();
            internal BitVector               m_sharesSourceVariableWith         = new BitVector();
            internal BitVector               m_sharesOffsetOfSourceVariableWith = new BitVector();
            internal uint                    m_offset;
            //
            // Constructor Methods
            //

            internal StateForLocalStackAssignment( StackLocationExpression var )
            {
                m_var    = var;
                m_offset = uint.MaxValue;
            }

            //
            // Helper Methods
            //

            internal void SetLiveness( BitVector[]                   livenessMap ,
                                       BitVector[]                   defVectors  ,
                                       VariableExpression.Property[] properties  ,
                                       Operator[]                    operators   )
            {
                int varIdx = m_var.SpanningTreeIndex;

                //
                // TODO: Once we have proper pointer analysis, we could weaken the check.
                //       For now, we just don't want to have this variable aliased.
                //
                if((properties[varIdx] & VariableExpression.Property.AddressTaken) != 0)
                {
                    m_liveness.SetRange( 0, operators.Length );
                }
                else
                {
                    m_liveness.OrInPlace( livenessMap[varIdx] );

                    //
                    // We also include all the operators that follow a definition of the stack location,
                    // because a dead assignment won't appear in the liveness map but it does change the state of the stack.
                    //
                    foreach(int defIdx in defVectors[varIdx])
                    {
                        m_liveness.Set( defIdx + 1 );
                    }
                }
            }

            //--//

            internal static void Correlate( List< StateForLocalStackAssignment > states )
            {
                for(int stateIdx = 0; stateIdx < states.Count; stateIdx++)
                {
                    StateForLocalStackAssignment state     = states[stateIdx];
                    VariableExpression           srcVar    = state.m_var.SourceVariable;
                    uint                         srcOffset = state.m_var.SourceOffset;

                    state.m_sharesSourceVariableWith        .Set( stateIdx );
                    state.m_sharesOffsetOfSourceVariableWith.Set( stateIdx );

                    for(int stateIdx2 = stateIdx + 1; stateIdx2 < states.Count; stateIdx2++)
                    {
                        StateForLocalStackAssignment state2     = states[stateIdx2];
                        VariableExpression           srcVar2    = state2.m_var.SourceVariable;
                        uint                         srcOffset2 = state2.m_var.SourceOffset;

                        if(state.m_liveness.IsIntersectionEmpty( state2.m_liveness ) == false)
                        {
                            state .m_interferesWith.Set( stateIdx2 );
                            state2.m_interferesWith.Set( stateIdx  );
                        }

                        if(srcVar == srcVar2)
                        {
                            state .m_sharesSourceVariableWith.Set( stateIdx2 );
                            state2.m_sharesSourceVariableWith.Set( stateIdx  );

                            if(srcOffset == srcOffset2)
                            {
                                state .m_sharesOffsetOfSourceVariableWith.Set( stateIdx2 );
                                state2.m_sharesOffsetOfSourceVariableWith.Set( stateIdx  );
                            }
                        }
                    }
                }
            }

            public static uint ComputeLayout( List< StateForLocalStackAssignment > states )
            {
                BitVector[] offsetsUsage = BitVector.SharedEmptyArray;

                for(int stateIdx = 0; stateIdx < states.Count; stateIdx++)
                {
                    StateForLocalStackAssignment state = states[stateIdx];

                    if(state.m_offset == uint.MaxValue)
                    {
                        VariableExpression srcVar = state.m_var.AggregateVariable;
                        uint               size   = srcVar.Type.SizeOfHoldingVariableInWords;
                        BitVector          liveness;

                        if(size == 1)
                        {
                            CHECKS.ASSERT( state.m_var.SourceOffset == 0, "Mismatch between SourceOffset and Size of object" );

                            //
                            // Simple case, we don't have to worry about keeping the various fields in the same order.
                            //
                            liveness = state.m_liveness;

                            if(liveness.Cardinality > 0)
                            {
                                uint offset = 0;
                                
                                while(IsOffsetAvailable( ref offsetsUsage, liveness, offset, 1 ) == false)
                                {
                                    offset++;
                                }

                                MarkOffsetAsUsed( offsetsUsage, liveness, offset, 1 );

                                state.m_var.AllocationOffset = offset * sizeof(uint);
                                state.m_offset               = offset;
                            }
                            else
                            {
                                state.m_var.Number = int.MaxValue;
                                state.m_offset     = uint.MaxValue - 1;
                            }
                        }
                        else
                        {
                            //
                            // Complex case, we have to ensure that the various fields are kept in the same order.
                            //

                            //
                            // Compute the liveness for the whole object.
                            //
                            liveness = new BitVector();

                            foreach(int stateIdx2 in state.m_sharesSourceVariableWith)
                            {
                                StateForLocalStackAssignment state2 = states[stateIdx2];

                                liveness.OrInPlace( state2.m_liveness );
                            }

                            if(liveness.Cardinality > 0)
                            {
                                uint offset = 0;
                                
                                while(IsOffsetAvailable( ref offsetsUsage, liveness, offset, size ) == false)
                                {
                                    offset++;
                                }

                                MarkOffsetAsUsed( offsetsUsage, liveness, offset, size );

                                foreach(int stateIdx2 in state.m_sharesSourceVariableWith)
                                {
                                    StateForLocalStackAssignment state2 = states[stateIdx2];

                                    //
                                    // If there are multiple variables mapping to the same field, their liveness should be disjoined.
                                    //
                                    foreach(int stateIdx3 in state2.m_interferesWith)
                                    {
                                        if(state2.m_sharesOffsetOfSourceVariableWith[ stateIdx3 ])
                                        {
                                            throw TypeConsistencyErrorException.Create( "Unsupported scenario: multiple variables mapping to the same field of the same stack variable are alive at the same time: {0} {1}", state2.m_var, states[stateIdx3].m_var );
                                        }
                                    }

                                    state2.m_var.AllocationOffset = offset * sizeof(uint) + state2.m_var.SourceOffset;
                                    state2.m_offset               = offset;
                                }
                            }
                            else
                            {
                                foreach(int stateIdx2 in state.m_sharesSourceVariableWith)
                                {
                                    StateForLocalStackAssignment state2 = states[stateIdx2];
                                
                                    state2.m_var.Number = int.MaxValue;
                                    state2.m_offset     = uint.MaxValue - 1;
                                }
                            }
                        }
                    }
                }

                return (uint)offsetsUsage.Length;
            }

            //--//

            private static bool IsOffsetAvailable( ref BitVector[] offsetsUsage ,
                                                       BitVector   liveness     ,
                                                       uint        offset       ,
                                                       uint        size         )
            {
                while(size > 0)
                {
                    offsetsUsage = ArrayUtility.EnsureSizeOfNotNullArray( offsetsUsage, (int)(offset + 1) );

                    BitVector vec = offsetsUsage[offset];

                    if(vec != null && vec.IsIntersectionEmpty( liveness ) == false)
                    {
                        return false;
                    }

                    offset++;
                    size--;
                }

                return true;
            }

            private static void MarkOffsetAsUsed( BitVector[] offsetsUsage ,
                                                  BitVector   liveness     ,
                                                  uint        offset       ,
                                                  uint        size         )
            {
                while(size > 0)
                {
                    BitVector vec = offsetsUsage[offset];

                    if(vec == null)
                    {
                        offsetsUsage[offset] = liveness.Clone();
                    }
                    else
                    {
                        vec.OrInPlace( liveness );
                    }

                    offset++;
                    size--;
                }
            }
        }

        //
        // State
        //

        protected Core                                        m_owner;
        protected ControlFlowGraphStateForCodeTransformation  m_cfg;
                                                            
        protected List< SequentialRegion >                    m_associatedRegions;
        protected SequentialRegion[]                          m_basicBlockToRegions;
                                                            
        //                                                  
        // Cached values, not persisted.                    
        //                                                  
                                                            
        private   IDisposable                                 m_cfgLock;
        protected BasicBlock[]                                m_basicBlocks;
        protected BasicBlock[]                                m_immediateDominators;
        protected Operator[]                                  m_operators;
        protected VariableExpression[]                        m_variables;
        protected BitVector[]                                 m_livenessAtBasicBlockEntry; // It's indexed as [<basic block index>][<variable index>]
        protected BitVector[]                                 m_livenessAtBasicBlockExit;  // It's indexed as [<basic block index>][<variable index>]
        protected BitVector[]                                 m_livenessAtOperator;        // It's indexed as [<operator index>][<variable index>]
        protected DataFlow.ControlTree.NaturalLoops           m_naturalLoops;

        private   List< Slot >                                m_topLevelSlots;
        private   AtomicSlot[]                                m_schedulingLookup;
        private   AtomicSlot[]                                m_schedulingLoopExits;
        private   BitVector                                   m_vecColdCode;
        private   BitVector                                   m_vecZeroLengthCandidates;
        private   BitVector                                   m_vecReachableFromColdCode;
        private   BitVector                                   m_vecReachableFromEntry;

        //
        // Constructor Methods
        //

        protected CompilationState() // Default constructor required by TypeSystemSerializer.
        {
            m_associatedRegions = new List< SequentialRegion >();
        }

        protected CompilationState( Core                                       owner ,
                                    ControlFlowGraphStateForCodeTransformation cfg   ) : this()
        {
            m_owner = owner;
            m_cfg   = cfg;
        }

        //
        // Helper Methods
        //

        public virtual void ApplyTransformation( TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            context.TransformGeneric( ref m_owner               );
            context.TransformGeneric( ref m_cfg                 );
            context.Transform       ( ref m_associatedRegions   );
            context.Transform       ( ref m_basicBlockToRegions );

            context.Pop();
        }

        //--//

        internal protected int CompareSchedulingWeights( BasicBlock left  ,
                                                         BasicBlock right )
        {
            var slotLeft  = GetSchedulingSlot( left  );
            var slotRight = GetSchedulingSlot( right );

            return slotLeft.Weight.CompareTo( slotRight.Weight );
        }

        internal protected bool AreBasicBlocksAdjacent( BasicBlock pre  ,
                                                        BasicBlock post )
        {
            var slotPre  = GetSchedulingSlot( pre  );
            var slotPost = GetSchedulingSlot( post );

            if(IsZeroLengthCandidate( pre ))
            {
                return slotPre.SchedulingIndex == slotPost.SchedulingIndex;
            }

            return slotPre.SchedulingIndex + 1 == slotPost.SchedulingIndex;
        }

        //--//

        internal void CompileMethod()
        {
            PrepareDataStructures();

            AssignStackLocations( );

            AllocateBasicBlockRegions( );

            CollectSchedulingPreferences( );

            PropagateColdCodeHints( );

            AllocateSchedulingSlots( );

            OrderBasicBlocks( );

            AssignSchedulingIndex( );

            EmitCodeForBasicBlocks();
        }

        //--//

        protected virtual void PrepareDataStructures()
        {
            m_cfgLock                   = m_cfg.GroupLock( m_cfg.LockSpanningTree() ,
                                                           m_cfg.LockDominance   () ,
                                                           m_cfg.LockLiveness    () );

            m_basicBlocks               = m_cfg.DataFlow_SpanningTree_BasicBlocks;
            m_immediateDominators       = m_cfg.DataFlow_ImmediateDominators;
            m_operators                 = m_cfg.DataFlow_SpanningTree_Operators;
            m_variables                 = m_cfg.DataFlow_SpanningTree_Variables;

            //
            // We need to build a slightly different liveness information: Stack Locations should be tracked as aggregates and not invalidated by method calls.
            //
            var liveness = DataFlow.LivenessAnalysis.Compute( m_cfg, true );

            m_livenessAtBasicBlockEntry = liveness.LivenessAtBasicBlockEntry;
            m_livenessAtBasicBlockExit  = liveness.LivenessAtBasicBlockExit;
            m_livenessAtOperator        = liveness.LivenessAtOperator;

////        if(m_cfg.ToString() == sDebugTarget)
////        {
////        }

            m_naturalLoops              = DataFlow.ControlTree.NaturalLoops.Execute( m_cfg );

            //--//

            int bbNum = m_basicBlocks.Length;

            m_basicBlockToRegions       = new SequentialRegion[bbNum];

            m_topLevelSlots             = new List< Slot >();
            m_schedulingLookup          = new AtomicSlot[bbNum];
            m_vecColdCode               = new BitVector( bbNum );
            m_vecZeroLengthCandidates   = new BitVector( bbNum );
            m_vecReachableFromColdCode  = new BitVector( bbNum );
            m_vecReachableFromEntry     = new BitVector( bbNum );

            //--//

            //
            // Sort loops from the deep to the shallow.
            //

            m_naturalLoops.Loops.Sort( (x, y) => y.Depth.CompareTo( x.Depth ) );
        }

        private void AllocateBasicBlockRegions()
        {
            foreach(BasicBlock bb in m_basicBlocks)
            {
                SequentialRegion reg = new SequentialRegion( m_owner, bb, m_owner.TypeSystem.PlatformAbstraction.GetMemoryRequirements( bb ) );

                m_basicBlockToRegions[bb.SpanningTreeIndex] = reg;

                TrackRegion( reg );
            }
        }

        //--//

        private void PropagateColdCodeHints()
        {
            //
            // Try to maximize the number of basic blocks that are cold, making sure that a block becomes cold if it's dominated by a cold block.
            //
            foreach(BasicBlock bb in m_basicBlocks)
            {
                if(bb is EntryBasicBlock)
                {
                    PropagateReachableFromEntry( bb.SpanningTreeIndex );
                }

                if(IsColdCode( bb ))
                {
                    PropagateReachableFromColdCode( bb.SpanningTreeIndex );
                }
            }
        }

        private void PropagateReachableFromEntry( int bbIdx )
        {
            if(m_vecReachableFromEntry.Set( bbIdx ))
            {
                foreach(BasicBlockEdge edge in m_basicBlocks[bbIdx].Successors)
                {
                    BasicBlock bbNext = edge.Successor;

                    if(IsColdCode( bbNext ))
                    {
                        continue;
                    }

                    PropagateReachableFromEntry( bbNext.SpanningTreeIndex );
                }
            }
        }
 
        private void PropagateReachableFromColdCode( int bbIdx )
        {
            if(m_vecReachableFromColdCode.Set( bbIdx ))
            {
                foreach(BasicBlockEdge edge in m_basicBlocks[bbIdx].Successors)
                {
                    BasicBlock bbNext    = edge.Successor;
                    int        bbNextIdx = bbNext.SpanningTreeIndex;

                    PropagateReachableFromColdCode( bbNextIdx );
                }
            }
        }

        //--//

        public void Dispose()
        {
            if(m_cfgLock != null)
            {
                if(m_naturalLoops != null)
                {
                    m_naturalLoops.Dispose();
                    m_naturalLoops = null;
                }

                m_cfgLock.Dispose();
                m_cfgLock = null;
            }
        }

        public void ReleaseAllLocks()
        {
            if(m_cfgLock != null)
            {
                m_cfgLock.Dispose();
                m_cfgLock = null;
            }
        }

        //--//

        private void AssignStackLocations()
        {
            if(m_cfg.Method.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.StackNotAvailable ) == false)
            {
                VariableExpression.Property[]        properties  = m_cfg.DataFlow_PropertiesOfVariables;
                BitVector[]                          livenessMap = m_cfg.DataFlow_VariableLivenessMap; // It's indexed as [<variable index>][<operator index>]
                BitVector[]                          defVectors  = m_cfg.DataFlow_BitVectorsForDefinitionChains;
                int                                  varsNum     = m_variables.Length;
                List< StateForLocalStackAssignment > states      = new List< StateForLocalStackAssignment >();

                for(int varIdx = 0; varIdx < varsNum; varIdx++)
                {
                    VariableExpression var = m_variables[varIdx];

                    PhysicalRegisterExpression regVar = var as PhysicalRegisterExpression;
                    if(regVar != null)
                    {
                        AssignStackLocations_RecordRegister( regVar );
                    }

                    StackLocationExpression stackVar = var as StackLocationExpression;
                    if(stackVar != null)
                    {
                        if(stackVar.StackPlacement == StackLocationExpression.Placement.Local)
                        {
                            StateForLocalStackAssignment state = new StateForLocalStackAssignment( stackVar );

                            //
                            // Try to minimize the amount of stack required, using the lifetime information
                            // to carve a slice of the stack just for the amount of time we need to keep the object around.
                            //
                            state.SetLiveness( livenessMap, defVectors, properties, m_operators );

                            states.Add( state );
                        }
                        else if(stackVar.StackPlacement == StackLocationExpression.Placement.Out)
                        {
                            AssignStackLocations_RecordStackOut( stackVar );
                        }
                    }
                }

                StateForLocalStackAssignment.Correlate( states );

                AssignStackLocations_RecordStackLocal( states );

                //--//

                AssignStackLocations_Finalize();
            }
        }

        protected abstract void AssignStackLocations_RecordRegister( PhysicalRegisterExpression regVar );

        protected abstract void AssignStackLocations_RecordStackOut( StackLocationExpression stackVar );

        protected abstract void AssignStackLocations_RecordStackLocal( List< StateForLocalStackAssignment > states );

        protected abstract void AssignStackLocations_Finalize();

        //--//

        protected abstract void FixupEmptyRegion( SequentialRegion reg );

        //--//

        public abstract bool CreateCodeMaps();

        //--//

        internal void TrackRegion( SequentialRegion reg )
        {
            m_associatedRegions.Add( reg );
        }

        //--//

        protected SequentialRegion[] GetSortedCodeRegions()
        {
            SequentialRegion[] regArray = ArrayUtility.CopyNotNullArray( m_basicBlockToRegions );

            Core.SortRegions( regArray );

            return regArray;
        }

        //
        // Access Methods
        //

        public Core Owner
        {
            get
            {
                return m_owner;
            }
        }

        public SequentialRegion[] BasicBlockRegions
        {
            get
            {
                return m_basicBlockToRegions;
            }
        }

        internal List< SequentialRegion > AssociatedRegions
        {
            get
            {
                return m_associatedRegions;
            }
        }
    }
}