//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.ImageBuilders
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.TargetModel.ArmProcessor;


    public partial class CompilationState
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        private void EmitCodeForBasicBlocks()
        {
            var isProcessed = new BitVector( m_basicBlocks.Length );
            var pending = new Stack<BasicBlock>( m_basicBlocks.Length );

            foreach(BasicBlock block in m_basicBlocks)
            {
                pending.Push( block );
            }

            // Emit blocks in dominance order.
            while(pending.Count > 0)
            {
                BasicBlock block = pending.Pop();

                // Skip dead and already-processed blocks.
                if((block.SpanningTreeIndex == -1) || isProcessed[block.SpanningTreeIndex])
                {
                    continue;
                }

                // If the block's immediate dominator hasn't been processed, process it first.
                BasicBlock idom = m_immediateDominators[block.SpanningTreeIndex];
                if((idom != block) && !isProcessed[idom.SpanningTreeIndex])
                {
                    pending.Push( block );
                    pending.Push( idom );
                    continue;
                }

                EmitCodeForBasicBlock(block);
                isProcessed[block.SpanningTreeIndex] = true;
            }

            FlushUnfinishedBlocks();
        }

        public virtual void EmitCodeForBasicBlock( BasicBlock bb )
        {
            m_activeCodeRegion        = GetRegion( bb );
            m_activeCodeSection       = m_activeCodeRegion.GetSectionOfVariableSize( sizeof(uint) );
            m_activeHardwareException = m_owner.TypeSystem.ExtractHardwareExceptionSettingsForMethod( bb.Owner.Method );

            //--//

            TypeSystemForCodeTransformation ts = m_cfg.TypeSystem;
            Abstractions.Platform           pa = ts.PlatformAbstraction;
            Abstractions.CallingConvention  cc = ts.CallingConvention;

            //
            // We want to track the exact liveness state of all the variables.
            //
            // To do that, we first emit tracking info for all the variables alive on entry to a basic block.
            //
            BitVector aliveHistory = m_livenessAtBasicBlockEntry[bb.SpanningTreeIndex].Clone();

            for(int idx = 0; idx < m_variables.Length; idx++)
            {
                if(aliveHistory[idx] == true)
                {
                    TrackVariable( m_variables[idx], true );
                }
            }

            m_fStopProcessingOperatorsForCurrentBasicBlock = false;

            foreach(Operator op in bb.Operators)
            {
                //
                // See which pointers are generated or killed.
                //
                UpdateLivenessInfo( aliveHistory, m_livenessAtOperator[op.SpanningTreeIndex] );

                //--//

                if(EmitCodeForBasicBlock_ShouldSkip( op ))
                {
                    continue;
                }

                EmitCodeForBasicBlock_EmitOperator( op );

                if(m_fStopProcessingOperatorsForCurrentBasicBlock)
                {
                    break;
                }
            }

            EmitCodeForBasicBlock_FlushOperators();

            UpdateLivenessInfo( aliveHistory, m_livenessAtBasicBlockExit[bb.SpanningTreeIndex] );

            //--//

            m_activeCodeRegion  = null;
            m_activeCodeSection = null;
        }

        public virtual void FlushUnfinishedBlocks( )
        {
        }

        protected virtual bool EmitCodeForBasicBlock_ShouldSkip( Operator op )
        {
            //
            // Skip any meta-operators.
            //
            if(op.IsMetaOperator && !(op is InitialValueOperator))
            {
                return true;
            }

            return false;
        }

        protected abstract uint EmitCodeForBasicBlock_EstimateMinimumSize( Operator op );

        protected abstract void EmitCodeForBasicBlock_EmitOperator( Operator op );

        protected abstract void EmitCodeForBasicBlock_FlushOperators();


        //--//

        private void UpdateLivenessInfo( BitVector aliveHistory ,
                                         BitVector alive        )
        {
            aliveHistory.XorInPlace( alive );

            //
            // Now 'aliveHistory' contains the set of variables that changed liveness status.
            //
            // We do two passes, first track the variables that went dead, then the ones that became alive.
            // The reason for this is that multiple variables map to the same physical register,
            // it's normal for a register to die and be born on the same instruction.
            // The end result should be a register that is still alive.
            // We'll compress the table later, when we switch from variables to actual storage places.
            //
            foreach(int idx in aliveHistory)
            {
                if(alive[idx] == false)
                {
                    TrackVariable( m_variables[idx], false );
                }
            }

            foreach(int idx in aliveHistory)
            {
                if(alive[idx] == true)
                {
                    TrackVariable( m_variables[idx], true );
                }
            }

            aliveHistory.Assign( alive );
        }
    }
}