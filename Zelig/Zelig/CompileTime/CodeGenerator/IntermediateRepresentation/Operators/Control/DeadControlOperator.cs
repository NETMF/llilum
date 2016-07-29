//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig;
    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class DeadControlOperator : ControlOperator, IDeadBranch
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
        // Constructor Methods
        //

        private DeadControlOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static DeadControlOperator New( Debugging.DebugInfo debugInfo )
        {
            DeadControlOperator res = new DeadControlOperator( debugInfo );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new DeadControlOperator( m_debugInfo ) );
        }

        //--//

        protected override void UpdateSuccessorInformation()
        {
        }

        //--//

        public override bool SubstituteTarget( BasicBlock oldBB ,
                                               BasicBlock newBB )
        {
            return false;
        }

        //--//

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "DeadControlOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "dead code" );
        }
    }
}