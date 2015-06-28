//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public sealed class SetStatusRegisterOperator : Operator
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

        private bool m_fUseSPSR;
        private uint m_fields;

        //
        // Constructor Methods
        //

        private SetStatusRegisterOperator( Debugging.DebugInfo debugInfo ,
                                           bool                fUseSPSR  ,
                                           uint                fields    ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_fUseSPSR = fUseSPSR;
            m_fields   = fields;
        }

        //--//

        public static SetStatusRegisterOperator New( Debugging.DebugInfo debugInfo ,
                                                     bool                fUseSPSR  ,
                                                     uint                fields    ,
                                                     Expression          ex        )
        {
            SetStatusRegisterOperator res = new SetStatusRegisterOperator( debugInfo, fUseSPSR, fields );

            res.SetRhs( ex );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new SetStatusRegisterOperator( m_debugInfo, m_fUseSPSR, m_fields ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_fUseSPSR );
            context2.Transform( ref m_fields   );

            context2.Pop();
        }

        //--//

        //
        // Access Methods
        //

        public bool UseSPSR
        {
            get
            {
                return m_fUseSPSR;
            }
        }

        public uint Fields
        {
            get
            {
                return m_fields;
            }
        }

        //--//

        public override bool ShouldNotBeRemoved
        {
            [System.Diagnostics.DebuggerHidden]
            get
            {
                return true;
            }
        }

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "SetStatusRegisterOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "<{0} status register:0x{1:X1}> = {2}", m_fUseSPSR ? "saved" : "current", m_fields, this.FirstArgument );
        }
    }
}