//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public sealed class GetStatusRegisterOperator : Operator
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

        //
        // Constructor Methods
        //

        private GetStatusRegisterOperator( Debugging.DebugInfo debugInfo ,
                                           bool                fUseSPSR  ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_fUseSPSR = fUseSPSR;
        }

        //--//

        public static GetStatusRegisterOperator New( Debugging.DebugInfo debugInfo ,
                                                     bool                fUseSPSR  ,
                                                     VariableExpression  ex        )
        {
            GetStatusRegisterOperator res = new GetStatusRegisterOperator( debugInfo, fUseSPSR );

            res.SetLhs( ex );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new GetStatusRegisterOperator( m_debugInfo, m_fUseSPSR ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_fUseSPSR );

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

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "GetStatusRegisterOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = <{1} status register>", this.FirstResult, m_fUseSPSR ? "saved" : "current" );
        }
    }
}