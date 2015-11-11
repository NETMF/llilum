//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public sealed partial class ArmV5_VFP
    {
        public class FloatingPointEmulation
        {
            const string c_Convert_LongToFloat          = "SoftVFP_Convert_LongToFloat";
            const string c_Convert_UnsignedLongToFloat  = "SoftVFP_Convert_UnsignedLongToFloat";
            const string c_Convert_DoubleToFloat        = "SoftVFP_Convert_DoubleToFloat";
            const string c_Convert_LongToDouble         = "SoftVFP_Convert_LongToDouble";
            const string c_Convert_UnsignedLongToDouble = "SoftVFP_Convert_UnsignedLongToDouble";
            const string c_Convert_FloatToLong          = "SoftVFP_Convert_FloatToLong";
            const string c_Convert_FloatToUnsignedLong  = "SoftVFP_Convert_FloatToUnsignedLong";
            const string c_Convert_DoubleToLong         = "SoftVFP_Convert_DoubleToLong";
            const string c_Convert_DoubleToUnsignedLong = "SoftVFP_Convert_DoubleToUnsignedLong";

            const string c_BinaryOperations_FloatRem    = "SoftVFP_BinaryOperations_FloatRem";
            const string c_BinaryOperations_DoubleRem   = "SoftVFP_BinaryOperations_DoubleRem";

            //--//

            [ZeligIR.CompilationSteps.CallClosureHandler( typeof(ZeligIR.ConvertOperator) )]
            private static void Protect_ConvertOperator( ZeligIR.CompilationSteps.ComputeCallsClosure.Context host   ,
                                                         ZeligIR.Operator                                     target )
            {
                ZeligIR.ConvertOperator                 op         = (ZeligIR.ConvertOperator)target;
                ZeligIR.TypeSystemForCodeTransformation ts         =                          host.TypeSystem;
                TypeRepresentation.BuiltInTypes         kindInput  =                          op.InputKind;
                TypeRepresentation.BuiltInTypes         kindOutput =                          op.OutputKind;
                string                                  name       =                          null;

                switch(kindOutput)
                {
                    case TypeRepresentation.BuiltInTypes.R4:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToFloat        ; break;
                            case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToFloat; break;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.R8:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToDouble        ; break;
                            case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToDouble; break;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.I8:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToLong ; break;
                            case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToLong; break;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.U8:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToUnsignedLong ; break;
                            case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToUnsignedLong; break;
                        }
                        break;
                }

                if(name != null)
                {
                    host.CoverObject( ts.GetWellKnownMethod( name ) );
                }
            }

            [ZeligIR.CompilationSteps.CallClosureHandler( typeof(ZeligIR.BinaryOperator) )]
            private static void Protect_BinaryOperator( ZeligIR.CompilationSteps.ComputeCallsClosure.Context host   ,
                                                        ZeligIR.Operator                                     target )
            {
                ZeligIR.BinaryOperator                  op   = (ZeligIR.BinaryOperator)target;
                TypeRepresentation                      td   =                         op.FirstArgument.Type;
                ZeligIR.TypeSystemForCodeTransformation ts   =                         host.TypeSystem;
                WellKnownTypes                          wkt  =                         ts.WellKnownTypes;
                string                                  name =                         null;

                if(td == wkt.System_Single)
                {
                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.REM: name = c_BinaryOperations_FloatRem; break;
                    }
                }
                else if(td == wkt.System_Double)
                {
                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.REM: name = c_BinaryOperations_DoubleRem; break;
                    }
                }

                if(name != null)
                {
                    host.CoverObject( ts.GetWellKnownMethod( name ) );
                }
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.FromImplicitToExplicitExceptions) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConvertOperator) )]
            private static void Handle_ConvertOperator_Exceptions( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.ConvertOperator    op  = (ZeligIR.ConvertOperator)nc.CurrentOperator;
                ZeligIR.VariableExpression lhs = op.FirstResult;
                ZeligIR.Expression         rhs = op.FirstArgument;

                if(op.CheckOverflow)
                {
                    ZeligIR.ConvertOperator opNew = ZeligIR.ConvertOperator.New( op.DebugInfo, op.InputKind, op.OutputKind, false, lhs, rhs );

                    op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                    //
                    // BUGBUG: We are dropping the overflow check!!
                    //
////                CreateOverflowCheck( nc, op, opNew );

                    nc.MarkAsModified();
                }
            }

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConvertOperator) )]
            private static void Handle_ConvertOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.ConvertOperator                 op         = (ZeligIR.ConvertOperator)nc.CurrentOperator;
                ZeligIR.TypeSystemForCodeTransformation ts         =                          nc.TypeSystem;
                TypeRepresentation.BuiltInTypes         kindInput  =                          op.InputKind;
                TypeRepresentation.BuiltInTypes         kindOutput =                          op.OutputKind;
                string                                  name       =                          null;

                switch(kindOutput)
                {
                    case TypeRepresentation.BuiltInTypes.R4:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToFloat        ; break;
                            case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToFloat; break;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.R8:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.I8: name = c_Convert_LongToDouble        ; break;
                            case TypeRepresentation.BuiltInTypes.U8: name = c_Convert_UnsignedLongToDouble; break;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.I8:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToLong ; break;
                            case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToLong; break;
                        }
                        break;

                    case TypeRepresentation.BuiltInTypes.U8:
                        switch(kindInput)
                        {
                            case TypeRepresentation.BuiltInTypes.R4: name = c_Convert_FloatToUnsignedLong ; break;
                            case TypeRepresentation.BuiltInTypes.R8: name = c_Convert_DoubleToUnsignedLong; break;
                        }
                        break;
                }

                if(name != null)
                {
                    MethodRepresentation md = ts.GetWellKnownMethod( name );

                    ZeligIR.Expression[] rhs = ts.AddTypePointerToArgumentsOfStaticMethod( md, op.FirstArgument, ts.CreateConstant( ts.WellKnownTypes.System_Boolean, op.CheckOverflow ) );

                    ZeligIR.StaticCallOperator opCall = ZeligIR.StaticCallOperator.New( op.DebugInfo, ZeligIR.CallOperator.CallKind.Direct, md, op.Results, rhs );

                    op.SubstituteWithOperator( opCall, ZeligIR.Operator.SubstitutionFlags.Default );

                    nc.MarkAsModified();
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ConvertUnsupportedOperatorsToMethodCalls) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.BinaryOperator) )]
            private static void Handle_BinaryOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.BinaryOperator op     = (ZeligIR.BinaryOperator)nc.CurrentOperator;
                ZeligIR.Expression     exSrc1 =                         op.FirstArgument;
                ZeligIR.Expression     exSrc2 =                         op.SecondArgument;
                TypeRepresentation     tdSrc1 =                         exSrc1.Type;
                TypeRepresentation     tdSrc2 =                         exSrc2.Type;

                if(tdSrc1.IsFloatingPoint &&
                   tdSrc2.IsFloatingPoint  )
                {
                    ZeligIR.ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
                    ZeligIR.TypeSystemForCodeTransformation            ts       = nc.TypeSystem;
                    ZeligIR.VariableExpression                         exRes    = op.FirstResult;
                    TypeRepresentation                                 tdRes    = exRes .Type;
                    uint                                               sizeRes  = tdRes .SizeOfHoldingVariableInWords;
                    uint                                               sizeSrc1 = tdSrc1.SizeOfHoldingVariableInWords;
                    uint                                               sizeSrc2 = tdSrc2.SizeOfHoldingVariableInWords;
                    string                                             name     = null;

                    CHECKS.ASSERT( sizeSrc1 == sizeSrc2, "Cannot compare entities of different size: {0} <=> {1}", exSrc1, exSrc2 );

                    if(sizeSrc1 == 1)
                    {
                        switch(op.Alu)
                        {
                            case ZeligIR.BinaryOperator.ALU.REM: name = c_BinaryOperations_FloatRem; break;
                        }
                    }
                    else if(sizeSrc1 == 2)
                    {
                        switch(op.Alu)
                        {
                            case ZeligIR.BinaryOperator.ALU.REM: name = c_BinaryOperations_DoubleRem; break;
                        }
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Unsupported inputs for binary operator: {0}", op );
                    }

                    if(name != null)
                    {
                        ts.SubstituteWithCallToHelper( name, op );

                        nc.MarkAsModified();
                    }
                }
            }
        }
    }
}
