//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LoadStaticFieldOperator : LoadFieldOperator
    {
        private const OperatorCapabilities cCapabilities = OperatorCapabilities.IsNonCommutative                   |
                                                           OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                           OperatorCapabilities.DoesNotAllocateStorage             |
                                                           OperatorCapabilities.MayReadExistingMutableStorage      | // TODO: Are static fields part of the heap?
                                                           OperatorCapabilities.DoesNotThrow                       |
                                                           OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                           OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                           OperatorCapabilities.DoesNotCapturePointerOperands      ;

        //
        // Constructor Methods
        //

        private LoadStaticFieldOperator( Debugging.DebugInfo debugInfo ,
                                         FieldRepresentation field     ) : base( debugInfo, cCapabilities, OperatorLevel.FullyObjectOriented, field )
        {
        }

        //--//

        public static LoadStaticFieldOperator New( Debugging.DebugInfo debugInfo ,
                                                   FieldRepresentation field     ,
                                                   VariableExpression  lhs       )
        {
            LoadStaticFieldOperator res = new LoadStaticFieldOperator( debugInfo, field );

            res.SetLhs( lhs );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            FieldRepresentation fd = context.ConvertField( this.Field );

            return RegisterAndCloneState( context, new LoadStaticFieldOperator( m_debugInfo, fd ) );
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
            sb.Append( "LoadStaticFieldOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} <= [{1}]", this.FirstResult, this.Field );
        }
    }
}