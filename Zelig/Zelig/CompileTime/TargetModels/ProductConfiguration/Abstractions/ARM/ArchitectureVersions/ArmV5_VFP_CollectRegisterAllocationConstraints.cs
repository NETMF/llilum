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
        class CollectRegisterAllocationConstraints
        {
            //
            // Helper Methods
            //

            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
            //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.SingleAssignmentOperator) )]
            private void Handle_SingleAssignmentOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                var op = (ZeligIR.SingleAssignmentOperator)nc.CurrentOperator;

                if(op.FirstArgument is ZeligIR.ConstantExpression)
                {
                    SetConstraintOnLhsBasedOnType( nc, op.Results, 0 );
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.LoadIndirectOperator) )]
            private void Handle_LoadIndirectOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                SetConstraintOnLhsBasedOnType( nc, nc.CurrentOperator.Results, 0 );
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.StoreIndirectOperator) )]
            private void Handle_StoreIndirectOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                SetConstraintOnRhsBasedOnType( nc, nc.CurrentOperator.Arguments, 1 );
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.IndirectOperator          ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.AddressAssignmentOperator ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.IndirectSubroutineOperator) )]
            private void Handle_AddressForFirstArgument( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                SetConstraintOnRHS( nc, 0, ZeligIR.Abstractions.RegisterClass.Address );
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.SetIfConditionIsTrueOperator) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConversionOperator          ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.MoveFromCoprocessor             ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.GetStatusRegisterOperator       ) )]
            private void Handle_IntegerForResult( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                SetConstraintOnLHS( nc, 0, ZeligIR.Abstractions.RegisterClass.Integer );
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.MultiWayConditionalControlOperator) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConversionOperator                ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.MoveToCoprocessor                     ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.SetStatusRegisterOperator             ) )]
            private void Handle_IntegerForFirstArgument( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                SetConstraintOnRHS( nc, 0, ZeligIR.Abstractions.RegisterClass.Integer );
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConvertOperator) )]
            private void Handle_ConvertOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.ConvertOperator op = (ZeligIR.ConvertOperator)nc.CurrentOperator;
                bool                    fInputFloat;
                bool                    fOutputFloat;

                switch(op.InputKind)
                {
                    case TypeRepresentation.BuiltInTypes.R4:
                    case TypeRepresentation.BuiltInTypes.R8:
                        fInputFloat = true;
                        break;

                    default:
                        fInputFloat = false;
                        break;
                }

                switch(op.OutputKind)
                {
                    case TypeRepresentation.BuiltInTypes.R4:
                    case TypeRepresentation.BuiltInTypes.R8:
                        fOutputFloat = true;
                        break;

                    default:
                        fOutputFloat = false;
                        break;
                }

                CHECKS.ASSERT( fOutputFloat || fInputFloat, "Expecting at least one floating point argument to {0}", op );

                if(fOutputFloat || fInputFloat)
                {
                    SetConstraintOnRHS( nc, 0, op.InputKind  == TypeRepresentation.BuiltInTypes.R8 ? ZeligIR.Abstractions.RegisterClass.DoublePrecision : ZeligIR.Abstractions.RegisterClass.SinglePrecision );
                    SetConstraintOnLHS( nc, 0, op.OutputKind == TypeRepresentation.BuiltInTypes.R8 ? ZeligIR.Abstractions.RegisterClass.DoublePrecision : ZeligIR.Abstractions.RegisterClass.SinglePrecision );
                }
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.AbstractUnaryOperator ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.AbstractBinaryOperator) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.VectorHack_Finalize       ) )]
            private void Handle_GenericComputation( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.Operator op = nc.CurrentOperator;

                SetConstraintOnResultsBasedOnType  ( nc, op.Results   );
                SetConstraintOnArgumentsBasedOnType( nc, op.Arguments );
            }

            [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.CollectRegisterAllocationConstraints) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.CompareOperator           ) )]
            [ZeligIR.CompilationSteps.OperatorHandler( typeof(ZeligIR.ConditionalCompareOperator) )]
            private void Handle_CompareOperators( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
            {
                ZeligIR.Operator op = nc.CurrentOperator;

                SetConstraintOnArgumentsBasedOnType( nc, op.Arguments );
            }
        }
    }
}
