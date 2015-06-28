//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class RethrowControlOperator : ControlOperator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.MayThrow                           |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // Constructor Methods
        //

        private RethrowControlOperator( Debugging.DebugInfo debugInfo ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented )
        {
        }

        //--//

        public static RethrowControlOperator New( Debugging.DebugInfo debugInfo )
        {
            RethrowControlOperator res = new RethrowControlOperator( debugInfo );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new RethrowControlOperator( m_debugInfo ) );
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
            sb.Append( "RethrowControlOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "rethrow" );
        }
    }
}