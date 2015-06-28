//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class PartialAssignmentOperator : Operator
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

        private uint m_srcOffset;
        private uint m_dstOffset;

        //
        // Constructor Methods
        //

        private PartialAssignmentOperator( Debugging.DebugInfo debugInfo ,
                                           uint                srcOffset ,
                                           uint                dstOffset ) : base( debugInfo, cCapabilities, Operator.OperatorLevel.Lowest )
        {
            m_srcOffset = srcOffset;
            m_dstOffset = dstOffset;
        }

        //--//

        public static PartialAssignmentOperator New( Debugging.DebugInfo debugInfo ,
                                                     VariableExpression  lhs       ,
                                                     uint                dstOffset ,
                                                     Expression          rhs       ,
                                                     uint                srcOffset )
        {
            PartialAssignmentOperator res = new PartialAssignmentOperator( debugInfo, srcOffset, dstOffset );

            res.SetLhs( lhs );
            res.SetRhs( rhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_srcOffset );
            context.Transform( ref m_dstOffset );

            context.Pop();
        }

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new PartialAssignmentOperator( m_debugInfo, m_srcOffset, m_dstOffset ) );
        }

        //--//

        //
        // Access Methods
        //

        public override bool PerformsNoActions
        {
            get
            {
                return false;
            }
        }

        //--//

        //
        // Access Methods
        //

        public uint SourceOffset
        {
            get
            {
                return m_srcOffset;
            }
        }

        public uint DestinationOffset
        {
            get
            {
                return m_dstOffset;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "PartialAssignmentOperator(" );

            base.InnerToString( sb );

            sb.AppendFormat( " SourceOffset: {0}"     , m_srcOffset );
            sb.AppendFormat( " DestinationOffset: {0}", m_dstOffset );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            if(this.FirstResult.Type.SizeOfHoldingVariableInWords < this.FirstArgument.Type.SizeOfHoldingVariableInWords)
            {
                return dumper.FormatOutput( "{0} = slice {1} at offset {2}", this.FirstResult, this.FirstArgument, m_srcOffset );
            }
            else
            {
                return dumper.FormatOutput( "{0} = insert {1} at offset {2}", this.FirstResult, this.FirstArgument, m_dstOffset );
            }
        }
   }
}
