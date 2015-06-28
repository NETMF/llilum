//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class ReturnControlOperator : ControlOperator
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

        private ReturnControlOperator() : base( null, cCapabilities, OperatorLevel.Lowest )
        {
        }

        //--//

        public static ReturnControlOperator New()
        {
            ReturnControlOperator res = new ReturnControlOperator();

            return res;
        }

        public static ReturnControlOperator New( Expression result )
        {
            ReturnControlOperator res = new ReturnControlOperator();

            res.SetRhs( result );

            return res;
        }

        public static ReturnControlOperator New( Expression[] fragments )
        {
            ReturnControlOperator res = new ReturnControlOperator();

            res.SetRhsArray( fragments );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            return RegisterAndCloneState( context, new ReturnControlOperator() );
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

        public override bool CanPropagateCopy( Expression exOld ,
                                               Expression exNew )
        {
            //
            // The Rvalues are used to keep alive the actual method result, so we cannot change them!!
            //
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
            sb.Append( "ReturnControlOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            System.Text.StringBuilder sb   = new System.Text.StringBuilder( "return" );
            object[]                  args = new object[this.Arguments.Length];

            for(int i = 0; i < this.Arguments.Length; i++)
            {
                string fmt;

                if(i == 0)
                {
                    fmt = " {0}{1}{2}";
                }
                else
                {
                    fmt = ", {0}{1}{2}";
                }

                sb.AppendFormat( fmt, "{", i, "}" );

                args[i] = this.Arguments[i];
            }

            return dumper.FormatOutput( sb.ToString(), args );
        }
    }
}