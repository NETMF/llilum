//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class ConditionalControlOperator : ControlOperator
    {
        //
        // State
        //

        protected BasicBlock m_targetBranchNotTaken;

        //
        // Constructor Methods
        //

        protected ConditionalControlOperator( Debugging.DebugInfo  debugInfo    ,
                                              OperatorCapabilities capabilities ,
                                              OperatorLevel        level        ) : base( debugInfo, capabilities, level )
        {
        }

        //--//

        protected override void UpdateSuccessorInformation()
        {
            m_basicBlock.LinkToNormalBasicBlock( m_targetBranchNotTaken );
        }

        //--//

        //
        // Helper Methods
        //

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            ConditionalControlOperator clone2 = (ConditionalControlOperator)clone;

            clone2.m_targetBranchNotTaken = context.Clone( m_targetBranchNotTaken );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );
            
            base.ApplyTransformation( context );

            context.Transform( ref m_targetBranchNotTaken );

            context.Pop();
        }

        //--//

        public override bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB )
        {
            if(m_targetBranchNotTaken == oldBB)
            {
                m_targetBranchNotTaken = newBB;
    
                BumpVersion();

                return true;
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock TargetBranchNotTaken
        {
            get
            {
                return m_targetBranchNotTaken;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            base.InnerToString( sb );

            sb.AppendFormat( " NotTaken: {0}", m_targetBranchNotTaken.SpanningTreeIndex );
        }
    }
}