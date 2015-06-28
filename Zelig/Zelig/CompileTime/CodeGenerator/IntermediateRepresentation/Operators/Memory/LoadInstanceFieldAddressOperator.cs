//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class LoadInstanceFieldAddressOperator : LoadAddressOperator
    {
        //
        // Constructor Methods
        //

        private LoadInstanceFieldAddressOperator( Debugging.DebugInfo  debugInfo    ,
                                                  OperatorCapabilities capabilities ,
                                                  OperatorLevel        level        ,
                                                  FieldRepresentation  field        ) : base( debugInfo, capabilities, level, field )
        {
        }

        //--//

        public static LoadInstanceFieldAddressOperator New( Debugging.DebugInfo debugInfo  ,
                                                            FieldRepresentation field      ,
                                                            VariableExpression  lhs        ,
                                                            Expression          rhsObj     ,
                                                            bool                fNullCheck )
        {
            OperatorLevel        level;
            OperatorCapabilities capabilities = OperatorCapabilities.IsNonCommutative                   |
                                                OperatorCapabilities.DoesNotMutateExistingStorage       |
                                                OperatorCapabilities.DoesNotAllocateStorage             |
                                                OperatorCapabilities.DoesNotReadExistingMutableStorage  |
                                                OperatorCapabilities.DoesNotReadThroughPointerOperands  |
                                                OperatorCapabilities.DoesNotWriteThroughPointerOperands |
                                                OperatorCapabilities.DoesNotCapturePointerOperands      ;

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

            LoadInstanceFieldAddressOperator res = new LoadInstanceFieldAddressOperator( debugInfo, capabilities, level, field );

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

            return RegisterAndCloneState( context, new LoadInstanceFieldAddressOperator( m_debugInfo, m_capabilities, m_level, fd ) );
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
            sb.Append( "LoadInstanceFieldAddressOperator(" );

            base.InnerToString( sb );

            sb.Append( ")" );
        }

        public override string FormatOutput( IIntermediateRepresentationDumper dumper )
        {
            return dumper.FormatOutput( "{0} = address of [{1}, {2}]{3}", this.FirstResult, this.FirstArgument, this.Field, this.MayThrow ? " MayThrow" : "" );
        }
    }
}