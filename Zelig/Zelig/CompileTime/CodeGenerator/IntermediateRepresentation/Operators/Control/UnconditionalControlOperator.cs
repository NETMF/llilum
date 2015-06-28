//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class UnconditionalControlOperator : ControlOperator
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

        private UnconditionalControlOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static UnconditionalControlOperator New( Debugging.DebugInfo debugInfo ,
                                                        BasicBlock          target    )
        {
            UnconditionalControlOperator res = new UnconditionalControlOperator( debugInfo );

            res.m_targetBranch = target;

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new UnconditionalControlOperator( m_debugInfo ) );
        }

        protected override void CloneState( CloningContext context ,
                                            Operator       clone   )
        {
            UnconditionalControlOperator clone2 = (UnconditionalControlOperator)clone;

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

        protected override void UpdateSuccessorInformation()
        {
            m_basicBlock.LinkToNormalBasicBlock( m_targetBranch );
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

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "UnconditionalControlOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " Target: {0}", m_targetBranch.SpanningTreeIndex );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "goto {0}", dumper.CreateLabel( m_targetBranch ) );
        }
    }
}