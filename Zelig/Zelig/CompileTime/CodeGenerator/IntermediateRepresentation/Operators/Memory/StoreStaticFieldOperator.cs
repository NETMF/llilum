//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class StoreStaticFieldOperator : StoreFieldOperator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.MayMutateExistingStorage           | // TODO: Are static fields part of the heap?
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // Constructor Methods
        //

        private StoreStaticFieldOperator( Debugging.DebugInfo debugInfo ,
                                          FieldRepresentation field     ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented, field )
        {
        }

        //--//

        public static StoreStaticFieldOperator New( Debugging.DebugInfo debugInfo ,
                                                    FieldRepresentation field     ,
                                                    Expression          val       )
        {
            StoreStaticFieldOperator res = new StoreStaticFieldOperator( debugInfo, field );

            res.SetRhs( val );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            FieldRepresentation fd = context.ConvertField( this.Field );

            return RegisterAndCloneState( context, new StoreStaticFieldOperator( m_debugInfo, fd ) );
        }

        //
        // Access Methods
        //

        //--//

        //
        // Debug Methods
        //

        public override void InnerToString( System.Text.StringBuilder sb )
        {
            sb.Append( "StoreStaticFieldOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "[{0}] <= {1}", this.Field, this.FirstArgument );
        }
    }
}