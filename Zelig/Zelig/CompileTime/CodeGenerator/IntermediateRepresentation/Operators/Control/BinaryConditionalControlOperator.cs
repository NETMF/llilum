//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class BinaryConditionalControlOperator : ConditionalControlOperator
    {
        //
        // State
        //

        private BasicBlock m_targetBranchTaken;

        //
        // Constructor Methods
        //

        private BinaryConditionalControlOperator( Debugging.DebugInfo  debugInfo    ,
                                                  OperatorCapabilities capabilities ) : base( debugInfo, capabilities, OperatorLevel.ConcreteTypes_NoExceptions )
        {
        }

        //--//

        public static BinaryConditionalControlOperator New( Debugging.DebugInfo debugInfo   ,
                                                            Expression          rhs         ,
                                                            BasicBlock          targetFalse ,
                                                            BasicBlock          targetTrue  )
        {
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotThrow                       |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

            BinaryConditionalControlOperator res = new BinaryConditionalControlOperator( debugInfo, capabilities );

            res.SetRhs( rhs );

            res.m_targetBranchNotTaken = targetFalse;
            res.m_targetBranchTaken    = targetTrue;

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new BinaryConditionalControlOperator( m_debugInfo, m_capabilities ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            BinaryConditionalControlOperator clone2 = (BinaryConditionalControlOperator)clone;

            clone2.m_targetBranchTaken = context.Clone( m_targetBranchTaken );

            base.CloneState( context, clone );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_targetBranchTaken );

            context.Pop();
        }

        //--//

        protected override void UpdateSuccessorInformation()
        {
            base.UpdateSuccessorInformation();

            m_basicBlock.LinkToNormalBasicBlock( m_targetBranchTaken );
        }

        //--//

        public override bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB )
        {
            bool fChanged = base.SubstituteTarget( oldBB, newBB );

            if(m_targetBranchTaken == oldBB)
            {
                m_targetBranchTaken = newBB;

                BumpVersion();

                fChanged = true;
            }

            return fChanged;
        }

        //--//

        //
        // Access Methods
        //

        public BasicBlock TargetBranchTaken
        {
            get
            {
                return m_targetBranchTaken;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "BinaryConditionalControlOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Taken: {0}", m_targetBranchTaken.SpanningTreeIndex );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "if {0} != ZERO then goto {1} else goto {2}", this.FirstArgument, m_targetBranchTaken, m_targetBranchNotTaken );
        }
    }
}