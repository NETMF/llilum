//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Handlers
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public class ProtectRequiredEntities
    {
        [CompilationSteps.CallClosureHandler( typeof(InstanceCallOperator) )]
        private static void Protect_MethodPointers( ComputeCallsClosure.Context host   ,
                                                    Operator                    target )
        {
            InstanceCallOperator call = (InstanceCallOperator)target;

            if(call.CallType == CallOperator.CallKind.Virtual)
            {
                Keep_VTablesGetter ( host );
                Keep_VTables       ( host );
                Keep_MethodPointers( host );
            }

            if(call.TargetMethod is InstanceMethodRepresentation)
            {
                Keep_NullCheck( host );
            }

            if(call.TargetMethod.OwnerType is InterfaceTypeRepresentation)
            {
                Keep_InterfaceMap( host );
            }
        }

        [CompilationSteps.CallClosureHandler( typeof(FieldOperator))]
        private static void Protect_FieldOperator( ComputeCallsClosure.Context host   ,
                                                   Operator                    target )
        {
            Keep_NullCheck( host );
        }

        [CompilationSteps.CallClosureHandler( typeof(ElementOperator) )]
        private static void Protect_ElementOperator( ComputeCallsClosure.Context host   ,
                                                     Operator                    target )
        {
            Keep_NullCheck ( host );
            Keep_BoundCheck( host );
            Keep_ArraySize ( host );
        }

        [CompilationSteps.CallClosureHandler( typeof(BinaryOperator)     )]
        [CompilationSteps.CallClosureHandler( typeof(SignExtendOperator) )]
        [CompilationSteps.CallClosureHandler( typeof(TruncateOperator)   )]
        [CompilationSteps.CallClosureHandler( typeof(UnaryOperator)      )]
        [CompilationSteps.CallClosureHandler( typeof(ZeroExtendOperator) )]
        private static void Protect_OverflowOperators( ComputeCallsClosure.Context host   ,
                                                       Operator                    target )
        {
            Keep_OverflowCheck( host );
        }

        [CompilationSteps.CallClosureHandler( typeof(BinaryOperator) )]
        private static void Protect_BinaryOperator( ComputeCallsClosure.Context host   ,
                                                    Operator                    target )
        {
            BinaryOperator                  op  = (BinaryOperator)target;
            TypeRepresentation              td  =                 op.FirstArgument.Type;
            TypeSystemForCodeTransformation ts  =                 host.TypeSystem;
            WellKnownTypes                  wkt =                 ts.WellKnownTypes;
            WellKnownMethods                wkm =                 ts.WellKnownMethods;

            if(td == wkt.System_Int32  ||
               td == wkt.System_UInt32  )
            {
                if(op.Signed)
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.DIV: host.CoverObject( wkm.Helpers_BinaryOperations_IntDiv ); break;
                        case BinaryOperator.ALU.REM: host.CoverObject( wkm.Helpers_BinaryOperations_IntRem ); break;
                    }
                }
                else
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.DIV: host.CoverObject( wkm.Helpers_BinaryOperations_UintDiv ); break;
                        case BinaryOperator.ALU.REM: host.CoverObject( wkm.Helpers_BinaryOperations_UintRem ); break;
                    }
                }
            }
            else if(td == wkt.System_Int64  ||
                    td == wkt.System_UInt64  )
            {
                if(op.Signed)
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.MUL: host.CoverObject( wkm.Helpers_BinaryOperations_LongMul ); break;
                        case BinaryOperator.ALU.DIV: host.CoverObject( wkm.Helpers_BinaryOperations_LongDiv ); break;
                        case BinaryOperator.ALU.REM: host.CoverObject( wkm.Helpers_BinaryOperations_LongRem ); break;
                        case BinaryOperator.ALU.SHL: host.CoverObject( wkm.Helpers_BinaryOperations_LongShl ); break;
                        case BinaryOperator.ALU.SHR: host.CoverObject( wkm.Helpers_BinaryOperations_LongShr ); break;
                    }
                }
                else
                {
                    switch(op.Alu)
                    {
                        case BinaryOperator.ALU.MUL: host.CoverObject( wkm.Helpers_BinaryOperations_UlongMul ); break;
                        case BinaryOperator.ALU.DIV: host.CoverObject( wkm.Helpers_BinaryOperations_UlongDiv ); break;
                        case BinaryOperator.ALU.REM: host.CoverObject( wkm.Helpers_BinaryOperations_UlongRem ); break;
                        case BinaryOperator.ALU.SHL: host.CoverObject( wkm.Helpers_BinaryOperations_UlongShl ); break;
                        case BinaryOperator.ALU.SHR: host.CoverObject( wkm.Helpers_BinaryOperations_UlongShr ); break;
                    }
                }
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        private static void Keep_VTablesGetter( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownMethods.VTable_Get );
        }

        private static void Keep_VTables( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownFields.ObjectHeader_VirtualTable );
        }

        private static void Keep_MethodPointers( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownFields.VTable_MethodPointers );
            host.CoverObject( host.TypeSystem.WellKnownFields.CodePointer_Target    );
        }

        private static void Keep_InterfaceMap( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownMethods.VTable_GetInterface );
        }

        private static void Keep_ArraySize( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownFields.ArrayImpl_m_numElements );
        }

        private static void Keep_NullCheck( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownMethods.ThreadImpl_ThrowNullException );
        }

        private static void Keep_BoundCheck( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownMethods.ThreadImpl_ThrowIndexOutOfRangeException );
        }

        private static void Keep_OverflowCheck( ComputeCallsClosure.Context host )
        {
            host.CoverObject( host.TypeSystem.WellKnownMethods.ThreadImpl_ThrowOverflowException );
        }
    }
}
