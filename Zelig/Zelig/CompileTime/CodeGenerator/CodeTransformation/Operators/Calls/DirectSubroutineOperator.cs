//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class DirectSubroutineOperator : SubroutineOperator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             | // It only allocates in the stack space.
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // Constructor Methods
        //

        private DirectSubroutineOperator( Debugging.DebugInfo  debugInfo ,
                                          MethodRepresentation md        ) : base( debugInfo, cCapabilities, md )
        {
        }

        //--//

        public static DirectSubroutineOperator New( Debugging.DebugInfo  debugInfo ,
                                                    MethodRepresentation md        ,
                                                    Expression[]         rhs       )
        {
            DirectSubroutineOperator res = new DirectSubroutineOperator( debugInfo, md );

            res.SetRhsArray( rhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            MethodRepresentation md = context.ConvertMethod( m_md );

            return RegisterAndCloneState( context, new DirectSubroutineOperator( m_debugInfo, md ) );
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
            sb.Append( "DirectSubroutineOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if(this.Arguments.Length > 0)
            {
                sb.Append( " " );
                for(int i = 0; i < this.Arguments.Length; i++)
                {
                    if(i != 0) sb.Append( ", " );

                    this.Arguments[i].InnerToString( sb );
                }
                sb.Append( " " );
            }

            return dumper.FormatOutput( "directSubCall  {0}({1})", this.TargetMethod, sb.ToString() );
        }
    }
}