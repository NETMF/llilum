//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LeaveControlOperator : ControlOperator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // State
        //

        private BasicBlock m_targetBranch;

        //
        // Constructor Methods
        //

        private LeaveControlOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.ObjectOriented )
        {
        }

        //--//

        public static LeaveControlOperator New( Debugging.DebugInfo debugInfo ,
                                                BasicBlock          target    )
        {
            LeaveControlOperator res = new LeaveControlOperator( debugInfo );

            res.m_targetBranch = target;

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new LeaveControlOperator( m_debugInfo ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            LeaveControlOperator clone2 = (LeaveControlOperator)clone;

            clone2.m_targetBranch = context.Clone( m_targetBranch );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_targetBranch );

            context.Pop();
        }

        //--//

        public override bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB )
        {
            if(m_targetBranch == oldBB)
            {
                m_targetBranch = newBB;

                BumpVersion();

                return true;
            }

            return false;
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock TargetBranch
        {
            get
            {
                return m_targetBranch;
            }
        }

        //--//

        protected override void UpdateSuccessorInformation()
        {
            m_basicBlock.LinkToNormalBasicBlock( m_targetBranch );
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "LeaveControlOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "leave {0}", m_targetBranch );
        }
    }
}