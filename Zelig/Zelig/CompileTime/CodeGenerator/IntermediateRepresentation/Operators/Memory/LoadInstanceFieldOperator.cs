//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LoadInstanceFieldOperator : LoadFieldOperator
    {
        //
        // Constructor Methods
        //

        private LoadInstanceFieldOperator( Debugging.DebugInfo  debugInfo    ,
                                           OperatorCapabilities capabilities ,
                                           OperatorLevel        level        ,
                                           FieldRepresentation  field        ) : base( debugInfo, capabilities, level, field )
        {
        }

        //--//

        public static LoadInstanceFieldOperator New( Debugging.DebugInfo debugInfo  ,
                                                     FieldRepresentation field      ,
                                                     VariableExpression  lhs        ,
                                                     Expression          rhsObj     ,
                                                     bool                fNullCheck )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.MayReadExistingMutableStorage      | // TODO: method analysis could discover the pointer is outside the stack.
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands ;

            if(rhsObj.Type is PointerTypeRepresentation    ||
               rhsObj.Type is BoxedValueTypeRepresentation  )
            {
                capabilities |= OperatorCapabilities.MayReadThroughPointerOperands |
                                OperatorCapabilities.MayCapturePointerOperands     ;
            }
            else
            {
                capabilities |= OperatorCapabilities.DoesNotReadThroughPointerOperands |
                                OperatorCapabilities.DoesNotCapturePointerOperands     ;
            }

            if(fNullCheck)
            {
                level         = OperatorLevel       .ConcreteTypes;
                capabilities |= OperatorCapabilities.MayThrow;
            }
            else
            {
                level         = OperatorLevel       .ConcreteTypes_NoExceptions;
                capabilities |= OperatorCapabilities.DoesNotThrow;
            }

            LoadInstanceFieldOperator res = new LoadInstanceFieldOperator( debugInfo, capabilities, level, field );

            res.SetLhs( lhs    );
            res.SetRhs( rhsObj );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            FieldRepresentation fd = context.ConvertField( this.Field );

            return RegisterAndCloneState( context, new LoadInstanceFieldOperator( m_debugInfo, m_capabilities, m_level, fd ) );
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
            sb.Append( "LoadInstanceFieldOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} <= [{1}, {2}]{3}", this.FirstResult, this.FirstArgument, this.Field, this.MayThrow ? " MayThrow" : "" );
        }
    }
}