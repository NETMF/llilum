//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class IndirectSubroutineOperator : SubroutineOperator
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

        private IndirectSubroutineOperator( Debugging.DebugInfo  debugInfo ,
                                            MethodRepresentation md        ) : base( debugInfo, cCapabilities, md )
        {
        }

        //--//

        public static IndirectSubroutineOperator New( Debugging.DebugInfo  debugInfo ,
                                                      MethodRepresentation md        ,
                                                      Expression           exPointer ,
                                                      Expression[]         rhs       )
        {
            IndirectSubroutineOperator res = new IndirectSubroutineOperator( debugInfo, md );

            if(rhs.Length > 0)
            {
                res.SetRhsArray( ArrayUtility.InsertAtHeadOfNotNullArray( rhs, exPointer ) );
            }
            else
            {
                res.SetRhs( exPointer );
            }

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            MethodRepresentation md = context.ConvertMethod( m_md );

            return RegisterAndCloneState( context, new IndirectSubroutineOperator( m_debugInfo, md ) );
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
            sb.Append( "IndirectSubroutineOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            this.FirstArgument.InnerToString( sb );

            if(this.Arguments.Length > 1)
            {
                sb.Append( " => " );
                for(int i = 1; i < this.Arguments.Length; i++)
                {
                    if(i != 1) sb.Append( ", " );

                    this.Arguments[i].InnerToString( sb );
                }
            }

            return dumper.FormatOutput( "indirectSubCall {0}( {1} )", this.TargetMethod, sb.ToString() );
        }
    }
}