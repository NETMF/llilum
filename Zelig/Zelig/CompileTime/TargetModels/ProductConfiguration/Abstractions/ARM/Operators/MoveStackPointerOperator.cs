//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.ARM
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.CodeGeneration.IR;


    public class MoveStackPointerOperator : Operator
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

        private bool m_fPush;

        //
        // Constructor Methods
        //

        private MoveStackPointerOperator( Debugging.DebugInfo debugInfo ,
                                          bool                fPush     ) : base( debugInfo, cCapabilities, OperatorLevel.Lowest )
        {
            m_fPush                 = fPush;
        }

        //--//

        public static MoveStackPointerOperator New( Debugging.DebugInfo debugInfo ,
                                                    bool                fPush     )
        {
            var res = new MoveStackPointerOperator( debugInfo, fPush );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new MoveStackPointerOperator( m_debugInfo, m_fPush ) );
        }

        //--//

        public override void ApplyTransformation( TransformationContextForIR context )
        {
            TransformationContextForCodeTransformation context2 = (TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_fPush );

            context2.Pop();
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

        public bool IsPush
        {
            get
            {
                return m_fPush;
            }
        }

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.AppendFormat( "MoveStackPointerOperator({0}", m_fPush ? "Push" : "Pop" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0}", m_fPush ? "<pushStackFrame>" : "<pullStackFrame>" );
        }
    }
}