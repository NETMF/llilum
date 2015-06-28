//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmPlatform
    {
        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ExpandAggregateTypes) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.SetStatusRegisterOperator) )]
        private static void Handle_ARM_SetStatusRegisterOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ARM.SetStatusRegisterOperator           op = (ARM.SetStatusRegisterOperator)nc.CurrentOperator;
            ZeligIR.TypeSystemForCodeTransformation ts =                                nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                ZeligIR.BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                                debugInfo    = op.DebugInfo;
                ZeligIR.Expression                                 exSrc        = op.FirstArgument;
                ZeligIR.Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );

                CHECKS.ASSERT( exSrc.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exSrc );

                ARM.SetStatusRegisterOperator opNew = ARM.SetStatusRegisterOperator.New( debugInfo, op.UseSPSR, op.Fields, srcFragments[0] );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ExpandAggregateTypes) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.GetStatusRegisterOperator) )]
        private static void Handle_ARM_GetStatusRegisterOperator( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ARM.GetStatusRegisterOperator           op = (ARM.GetStatusRegisterOperator)nc.CurrentOperator;
            ZeligIR.TypeSystemForCodeTransformation ts =                                nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                ZeligIR.BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                                debugInfo    = op.DebugInfo;
                ZeligIR.VariableExpression                         exDst        = op.FirstResult;
                ZeligIR.Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );

                CHECKS.ASSERT( exDst.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exDst );

                ARM.GetStatusRegisterOperator opNew = ARM.GetStatusRegisterOperator.New( debugInfo, op.UseSPSR, (ZeligIR.VariableExpression)dstFragments[0] );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ExpandAggregateTypes) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.MoveToCoprocessor) )]
        private static void Handle_ARM_MoveToCoprocessor( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ARM.MoveToCoprocessor                   op = (ARM.MoveToCoprocessor)nc.CurrentOperator;
            ZeligIR.TypeSystemForCodeTransformation ts =                        nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                ZeligIR.BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                                debugInfo    = op.DebugInfo;
                ZeligIR.Expression                                 exSrc        = op.FirstArgument;
                ZeligIR.Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );

                CHECKS.ASSERT( exSrc.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exSrc );

                ARM.MoveToCoprocessor opNew = ARM.MoveToCoprocessor.New( debugInfo, op.CpNum, op.Op1, op.CRn, op.CRm, op.Op2, srcFragments[0] );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ExpandAggregateTypes) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.MoveFromCoprocessor) )]
        private static void Handle_ARM_MoveFromCoprocessor( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ARM.MoveFromCoprocessor                 op = (ARM.MoveFromCoprocessor)nc.CurrentOperator;
            ZeligIR.TypeSystemForCodeTransformation ts =                          nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                ZeligIR.BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                                debugInfo    = op.DebugInfo;
                ZeligIR.VariableExpression                         exDst        = op.FirstResult;
                ZeligIR.Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );

                CHECKS.ASSERT( exDst.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exDst );

                ARM.MoveFromCoprocessor opNew = ARM.MoveFromCoprocessor.New( debugInfo, op.CpNum, op.Op1, op.CRn, op.CRm, op.Op2, (ZeligIR.VariableExpression)dstFragments[0] );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ExpandAggregateTypes) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.VectorHack_Finalize) )]
        private static void Handle_ARM_VectorHack_Finalize( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ARM.VectorHack_Finalize                 op = (ARM.VectorHack_Finalize)nc.CurrentOperator;
            ZeligIR.TypeSystemForCodeTransformation ts =                          nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                ZeligIR.BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                                debugInfo    = op.DebugInfo;
                ZeligIR.VariableExpression                         exDst        = op.FirstResult;
                ZeligIR.Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );

                CHECKS.ASSERT( exDst.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exDst );

                ARM.VectorHack_Finalize opNew = ARM.VectorHack_Finalize.New( debugInfo, op.Size, op.ResultBankBase, (ZeligIR.VariableExpression)dstFragments[0] );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.ExpandAggregateTypes) )]
        [ZeligIR.CompilationSteps.OperatorHandler( typeof(ARM.VectorHack_LoadData) )]
        private static void Handle_ARM_VectorHack_LoadData( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ARM.VectorHack_LoadData                 op = (ARM.VectorHack_LoadData)nc.CurrentOperator;
            ZeligIR.TypeSystemForCodeTransformation ts =                          nc.TypeSystem;

            if(ts.OperatorAlreadyInScalarForm( op ) == false)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg          = nc.CurrentCFG;
                ZeligIR.BasicBlock                                 current      = op.BasicBlock;
                Debugging.DebugInfo                                debugInfo    = op.DebugInfo;
                ZeligIR.VariableExpression                         exDst        = op.FirstResult;
                ZeligIR.Expression                                 exSrc        = op.FirstArgument;
                ZeligIR.Expression[]                               dstFragments = cfg.GetFragmentsForExpression( exDst );
                ZeligIR.Expression[]                               srcFragments = cfg.GetFragmentsForExpression( exSrc );

                CHECKS.ASSERT( exDst.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exDst );
                CHECKS.ASSERT( exSrc.Type.SizeOfHoldingVariableInWords == 1, "Expecting a word-sized expression, got {0}", exSrc );

                ARM.VectorHack_LoadData opNew = ARM.VectorHack_LoadData.New( debugInfo, op.Size, op.DestinationBankBase, (ZeligIR.VariableExpression)dstFragments[0], srcFragments[0] );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.CopyAnnotations );

                nc.MarkAsModified();
            }
        }
    }
}
