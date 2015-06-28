//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class AddActivationRecordEventOperator : Operator
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

        private Runtime.ActivationRecordEvents m_ev;

        //
        // Constructor Methods
        //

        private AddActivationRecordEventOperator( Debugging.DebugInfo            debugInfo ,
                                                  Runtime.ActivationRecordEvents ev        ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_ev = ev;
        }

        //--//

        public static AddActivationRecordEventOperator New( Debugging.DebugInfo            debugInfo ,
                                                            Runtime.ActivationRecordEvents ev        )
        {
            var res = new AddActivationRecordEventOperator( debugInfo, ev );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new AddActivationRecordEventOperator( m_debugInfo, m_ev ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_ev );

            context.Pop();
        }

        //--//

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            return false;
        }

        //
        // Access Methods
        //

        public override bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return true;
            }
        }

        public Runtime.ActivationRecordEvents ActivationRecordEvent
        {
            get
            {
                return m_ev;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "AddActivationRecordEventOperator({0}", m_ev );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<activationRecord {0}>", m_ev );
        }
    }
}