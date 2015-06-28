//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class StoreInstanceFieldOperator : StoreFieldOperator
    {
        //
        // Constructor Methods
        //

        private StoreInstanceFieldOperator( Debugging.DebugInfo  debugInfo    ,
                                            OperatorCapabilities capabilities ,
                                            OperatorLevel        level        ,
                                            FieldRepresentation  field        ) : base( debugInfo, capabilities, level, field )
        {
        }

        //--//

        public static StoreInstanceFieldOperator New( Debugging.DebugInfo debugInfo  ,
                                                      FieldRepresentation field      ,
                                                      Expression          rhsObj     ,
                                                      Expression          rhsVal     ,
                                                      bool                fNullCheck )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                  |
                                                OperatorCapabilities.MayMutateExistingStorage          |
                                                OperatorCapabilities.DoesNotAllocateStorage            |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands ;

            if(rhsObj.Type is PointerTypeRepresentation    ||
               rhsObj.Type is BoxedValueTypeRepresentation  )
            {
                capabilities |= OperatorCapabilities.MayWriteThroughPointerOperands |
                                OperatorCapabilities.MayCapturePointerOperands      ;
            }
            else
            {
                capabilities |= OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                OperatorCapabilities.DoesNotCapturePointerOperands      ;
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

            StoreInstanceFieldOperator res = new StoreInstanceFieldOperator( debugInfo, capabilities, level, field );

            res.SetRhs( rhsObj, rhsVal );

            return res;
        }

        //--//

        //
        // Helper Methods
        //

        public override Operator Clone( CloningContext context )
        {
            FieldRepresentation fd = context.ConvertField( this.Field );

            return RegisterAndCloneState( context, new StoreInstanceFieldOperator( m_debugInfo, m_capabilities, m_level, fd ) );
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
            sb.Append( "StoreInstanceFieldOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "[{0}, {1}] <= {2}{3}", this.FirstArgument, this.Field, this.SecondArgument, this.MayThrow ? " MayThrow" : "" );
        }
    }
}