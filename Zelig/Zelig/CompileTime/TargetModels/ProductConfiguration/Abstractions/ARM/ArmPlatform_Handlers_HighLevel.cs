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
        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_SetRegister" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_SetRegisterFP32" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_SetRegisterFP64" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_SetSystemRegister" )]
        private void Handle_ProcessorARM_SetRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator                               op       = nc.GetOperatorAndThrowIfNotCall();
            ZeligIR.ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
            uint                                               encoding = nc.ExtractConstantUIntParameter( op, 1 );

            var regDesc = nc.TypeSystem.PlatformAbstraction.GetRegisterForEncoding( encoding );
            if(regDesc == null)
            {
                //
                // We cannot fail, because code for different processors could be mixed in the same assembly and we don't know until later.
                //
////            throw TypeConsistencyErrorException.Create( "Attempt to set non-existing register: {0}", encoding );
                op.Delete();
            }
            else
            {
                var opNew = ZeligIR.SingleAssignmentOperator.New( op.DebugInfo, cfg.AllocatePhysicalRegister( regDesc ), op.ThirdArgument );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

                opNew.AddAnnotation( ZeligIR.DontRemoveAnnotation.Create( nc.TypeSystem ) );
            }

            nc.MarkAsModified();
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_GetRegister" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_GetRegisterFP32" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_GetRegisterFP64" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_GetSystemRegister" )]
        private void Handle_ProcessorARM_GetRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator                               op       = nc.GetOperatorAndThrowIfNotCall();
            ZeligIR.ControlFlowGraphStateForCodeTransformation cfg      = nc.CurrentCFG;
            uint                                               encoding = nc.ExtractConstantUIntParameter( op, 1 );
            var                                                lhs      = op.FirstResult;

            var regDesc = nc.TypeSystem.PlatformAbstraction.GetRegisterForEncoding( encoding );
            if(regDesc == null)
            {
                //
                // We cannot fail, because code for different processors could be mixed in the same assembly and we don't know until later.
                //
////            throw TypeConsistencyErrorException.Create( "Attempt to get non-existing register: {0}", encoding );
                var opNew = ZeligIR.SingleAssignmentOperator.New( op.DebugInfo, lhs, nc.TypeSystem.CreateConstant( lhs.Type, 0 ) );
                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );
            }
            else
            {
                var reg = cfg.AllocatePhysicalRegister( regDesc );

                ZeligIR.Operator opNew;

                //
                // The PC processor reads 8 bytes past the current value, let's adjust it.
                //
                if(regDesc.Encoding == TargetModel.ArmProcessor.EncodingDefinition.c_register_pc)
                {
                    opNew = ZeligIR.BinaryOperator.New( op.DebugInfo, ZeligIR.BinaryOperator.ALU.SUB, false, false, lhs, reg, nc.TypeSystem.CreateConstant( TargetModel.ArmProcessor.EncodingDefinition.c_PC_offset ) );
                }
                else
                {
                    opNew = ZeligIR.SingleAssignmentOperator.New( op.DebugInfo, lhs, reg );
                }

                op.AddAnnotation( ZeligIR.PreInvalidationAnnotation.Create( nc.TypeSystem, reg ) );

                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );
            }

            nc.MarkAsModified();
        }
        
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_SetStatusRegister" )]
        private void Handle_ProcessorARM_SetStatusRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_SetStatusRegister( nc, false );
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_SetSavedStatusRegister" )]
        private void Handle_ProcessorARM_SetSavedStatusRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_SetStatusRegister( nc, true );
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_GetStatusRegister" )]
        private void Handle_ProcessorARM_GetStatusRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_GetStatusRegister( nc, false );
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_GetSavedStatusRegister" )]
        private void Handle_ProcessorARM_GetSavedStatusRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_GetStatusRegister( nc, true );
        }
        
        private void Handle_ProcessorARM_SetStatusRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc       ,
                                                            bool                                                        fUseSPSR )
        {
            ZeligIR.CallOperator op     = nc.GetOperatorAndThrowIfNotCall();
            ZeligIR.Expression   exVal  = op.ThirdArgument;
            uint                 fields = nc.ExtractConstantUIntParameter( op, 1 );

            ZeligIR.Operator opNew = ARM.SetStatusRegisterOperator.New( op.DebugInfo, fUseSPSR, fields, exVal );
            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        private void Handle_ProcessorARM_GetStatusRegister( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc       ,
                                                            bool                                                        fUseSPSR )
        {
            ZeligIR.CallOperator op  = nc.GetOperatorAndThrowIfNotCall();

            ZeligIR.Operator opNew = ARM.GetStatusRegisterOperator.New( op.DebugInfo, fUseSPSR, op.FirstResult );
            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_MoveToCoprocessor" )]
        private void Handle_ProcessorARM_MoveToCoprocessor( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator op    = nc.GetOperatorAndThrowIfNotCall();
            uint                 CpNum = nc.ExtractConstantUIntParameter( op, 1 );
            uint                 Op1   = nc.ExtractConstantUIntParameter( op, 2 );
            uint                 CRn   = nc.ExtractConstantUIntParameter( op, 3 );
            uint                 CRm   = nc.ExtractConstantUIntParameter( op, 4 );
            uint                 Op2   = nc.ExtractConstantUIntParameter( op, 5 );

            ZeligIR.Operator opNew = ARM.MoveToCoprocessor.New( op.DebugInfo, CpNum, Op1, CRn, CRm, Op2, op.Arguments[6] );
            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }


        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_MoveFromCoprocessor" )]
        private void Handle_ProcessorARM_MoveFromCoprocessor( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator op    = nc.GetOperatorAndThrowIfNotCall();
            uint                 CpNum = nc.ExtractConstantUIntParameter( op, 1 );
            uint                 Op1   = nc.ExtractConstantUIntParameter( op, 2 );
            uint                 CRn   = nc.ExtractConstantUIntParameter( op, 3 );
            uint                 CRm   = nc.ExtractConstantUIntParameter( op, 4 );
            uint                 Op2   = nc.ExtractConstantUIntParameter( op, 5 );

            ZeligIR.Operator opNew = ARM.MoveFromCoprocessor.New( op.DebugInfo, CpNum, Op1, CRn, CRm, Op2, op.FirstResult );
            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        
        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARM_Breakpoint" )]
        //////[ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARMv6_Breakpoint" )]
        //////[ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "ProcessorARMv7_Breakpoint" )]
        private void Handle_ProcessorARM_Breakpoint( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator op    = nc.GetOperatorAndThrowIfNotCall();
            uint                 Value = nc.ExtractConstantUIntParameter( op, 1 );

            ZeligIR.Operator opNew = ARM.BreakpointOperator.New( op.DebugInfo, Value );
            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }
        
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_ScratchedRegisters" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_ScratchedRegisters( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator op = nc.GetOperatorAndThrowIfNotCall();

            var  cc   = nc.TypeSystem.CallingConvention;
            var  pa   = nc.TypeSystem.PlatformAbstraction;
            uint mask = 0;

            foreach(var reg in pa.GetRegisters())
            {
                if(reg.InIntegerRegisterFile && cc.ShouldSaveRegister( reg ) == false)
                {
                    mask |= 1u << (int)(reg.Encoding - EncodingDefinition.c_register_r0);
                }
            }

            var opNew = ZeligIR.SingleAssignmentOperator.New( op.DebugInfo, op.FirstResult, nc.TypeSystem.CreateConstant( mask ) );

            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PushRegisters" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_PushRegisters( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator op = nc.GetOperatorAndThrowIfNotCall();

            if(nc.IsParameterConstant( op, 1 ) &&
               nc.IsParameterConstant( op, 2 ) &&
               nc.IsParameterConstant( op, 3 ) &&
               nc.IsParameterConstant( op, 4 )  )
            {
                uint indexRegister           = nc.ExtractConstantUIntParameter( op, 1 );
                bool fWriteBackIndexRegister = nc.ExtractConstantBoolParameter( op, 2 );
                bool fAddComputedRegisters   = nc.ExtractConstantBoolParameter( op, 3 );
                uint registerMask            = nc.ExtractConstantUIntParameter( op, 4 );
                var  exReg                   = nc.GetVariableForRegisterEncoding( indexRegister );

                var opNew = ARM.MoveIntegerRegistersOperator.New( op.DebugInfo, false, fWriteBackIndexRegister, fAddComputedRegisters, false, registerMask, exReg );
                opNew.AddAnnotation( ZeligIR.PreInvalidationAnnotation.Create( nc.TypeSystem, exReg ) );

                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PopRegisters" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv7_MethodWrapperHelpers_PopRegisters" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_PopRegisters( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator op = nc.GetOperatorAndThrowIfNotCall();

            if(nc.IsParameterConstant( op, 1 ) &&
               nc.IsParameterConstant( op, 2 ) &&
               nc.IsParameterConstant( op, 3 ) &&
               nc.IsParameterConstant( op, 4 ) &&
               nc.IsParameterConstant( op, 5 )  )
            {
                uint indexRegister           = nc.ExtractConstantUIntParameter( op, 1 );
                bool fWriteBackIndexRegister = nc.ExtractConstantBoolParameter( op, 2 );
                bool fAddComputedRegisters   = nc.ExtractConstantBoolParameter( op, 3 );
                bool fRestoreSPSR            = nc.ExtractConstantBoolParameter( op, 4 );
                uint registerMask            = nc.ExtractConstantUIntParameter( op, 5 );
                var  exReg                   = nc.GetVariableForRegisterEncoding( indexRegister );

                var opNew = ARM.MoveIntegerRegistersOperator.New( op.DebugInfo, true, fWriteBackIndexRegister, fAddComputedRegisters, fRestoreSPSR, registerMask, exReg );
                opNew.AddAnnotation( ZeligIR.PreInvalidationAnnotation.Create( nc.TypeSystem, exReg ) );

                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv5_VFP_MethodWrapperHelpers_PushFpRegisters" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv7_VFP_MethodWrapperHelpers_PushFpRegisters" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_PushFpRegisters( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_MethodWrapperHelpers_MoveFpRegisters( nc, false );
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv5_VFP_MethodWrapperHelpers_PopFpRegisters" )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv7_VFP_MethodWrapperHelpers_PopFpRegisters" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_PopFpRegisters( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_MethodWrapperHelpers_MoveFpRegisters( nc, true );
        }

        private void Handle_ProcessorARM_MethodWrapperHelpers_MoveFpRegisters( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc    ,
                                                                               bool                                                        fLoad )
        {
            ZeligIR.CallOperator op = nc.GetOperatorAndThrowIfNotCall();

            if(nc.IsParameterConstant( op, 1 ) &&
               nc.IsParameterConstant( op, 2 ) &&
               nc.IsParameterConstant( op, 3 ) &&
               nc.IsParameterConstant( op, 4 ) &&
               nc.IsParameterConstant( op, 5 )  )
            {
                uint indexRegister           = nc.ExtractConstantUIntParameter( op, 1 );
                bool fWriteBackIndexRegister = nc.ExtractConstantBoolParameter( op, 2 );
                bool fAddComputedRegisters   = nc.ExtractConstantBoolParameter( op, 3 );
                uint registerLow             = nc.ExtractConstantUIntParameter( op, 4 );
                uint registerHigh            = nc.ExtractConstantUIntParameter( op, 5 );
                var  exReg                   = nc.GetVariableForRegisterEncoding( indexRegister );

                var opNew = ARM.MoveFloatingPointRegistersOperator.New( op.DebugInfo, fLoad, fWriteBackIndexRegister, fAddComputedRegisters, registerLow, registerHigh, exReg );
                opNew.AddAnnotation( ZeligIR.PreInvalidationAnnotation.Create( nc.TypeSystem, exReg ) );

                op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

                nc.MarkAsModified();
            }
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PushStackFrame" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_PushStackFrame( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_MethodWrapperHelpers_MoveStackFrame( nc, true );
        }

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PopStackFrame" )]
        private void Handle_ProcessorARM_MethodWrapperHelpers_PopStackFrame( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            Handle_ProcessorARM_MethodWrapperHelpers_MoveStackFrame( nc, false );
        }

        private void Handle_ProcessorARM_MethodWrapperHelpers_MoveStackFrame( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc    ,
                                                                              bool                                                        fPush )
        {
            ZeligIR.CallOperator op = nc.GetOperatorAndThrowIfNotCall();

            var opNew = ARM.MoveStackPointerOperator.New( op.DebugInfo, fPush );
            op.SubstituteWithOperator( opNew, ZeligIR.Operator.SubstitutionFlags.Default );

            nc.MarkAsModified();
        }


        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
        //--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//

        [ZeligIR.CompilationSteps.PhaseFilter( typeof(ZeligIR.CompilationSteps.Phases.HighLevelTransformations) )]
        [ZeligIR.CompilationSteps.CallToWellKnownMethodHandler( "Solo_DSP_MatrixMultiply__MultiplyAndAccumulate" )]
        private void Handle_Solo_DSP_MatrixMultiply__MultiplyAndAccumulate( ZeligIR.CompilationSteps.PhaseExecution.NotificationContext nc )
        {
            ZeligIR.CallOperator                               op         = nc.GetOperatorAndThrowIfNotCall();
            ZeligIR.ControlFlowGraphStateForCodeTransformation cfg        = nc.CurrentCFG;
            ZeligIR.TypeSystemForCodeTransformation            ts         = nc.TypeSystem;
            ZeligIR.VariableExpression                         varRes     = op.FirstResult;
            ZeligIR.Expression                                 exAbase    = op.SecondArgument;
            ZeligIR.Expression                                 exBbase    = op.ThirdArgument;
            ZeligIR.Expression                                 exN        = op.FourthArgument;
            int                                                vectorSize = nc.ExtractConstantIntParameter( op, 4 );

            if(vectorSize < 1 || vectorSize > 8)
            {
                throw TypeConsistencyErrorException.Create( "Invalid vector size for {0}: {1}", op, vectorSize );
            }

            //--//

            var wkt     = ts.WellKnownTypes;
            var tdValue = varRes.Type;
            var tdPtr   = ts.CreateUnmanagedPointerToType( tdValue );

            CHECKS.ASSERT( tdValue == wkt.System_Single, "Invalid result type, expecting 'float', got '{0}'", tdValue );

            var                        debugInfo    = op.DebugInfo;
            var                        pa           = ts.PlatformAbstraction;
            ZeligIR.NormalBasicBlock   bbEntry      = new ZeligIR.NormalBasicBlock( cfg );
            ZeligIR.NormalBasicBlock   bbExit       = new ZeligIR.NormalBasicBlock( cfg );
            ZeligIR.NormalBasicBlock   bbLoopHeader = new ZeligIR.NormalBasicBlock( cfg );
            ZeligIR.NormalBasicBlock   bbLoopBody   = new ZeligIR.NormalBasicBlock( cfg );
            ZeligIR.VariableExpression varAptr      = cfg.AllocateTypedPhysicalRegister( tdPtr, pa.GetRegisterForEncoding( TargetModel.ArmProcessor.EncodingDefinition.c_register_r4 ), null, null, 0 );
            ZeligIR.VariableExpression varBptr      = cfg.AllocateTypedPhysicalRegister( tdPtr, pa.GetRegisterForEncoding( TargetModel.ArmProcessor.EncodingDefinition.c_register_r5 ), null, null, 0 );
            ZeligIR.VariableExpression varAend      = cfg.AllocateTypedPhysicalRegister( tdPtr, pa.GetRegisterForEncoding( TargetModel.ArmProcessor.EncodingDefinition.c_register_r6 ), null, null, 0 );
            ZeligIR.VariableExpression varSize      = cfg.AllocateTemporary            ( wkt.System_Int32, null );

            var regsLeftBankBase   = GenerateVector( cfg, pa, vectorSize, TargetModel.ArmProcessor.EncodingDefinition_VFP.c_register_s8  );
            var regsRightBankBase  = GenerateVector( cfg, pa, vectorSize, TargetModel.ArmProcessor.EncodingDefinition_VFP.c_register_s16 );
            var regsResultBankBase = GenerateVector( cfg, pa, vectorSize, TargetModel.ArmProcessor.EncodingDefinition_VFP.c_register_s24 );

            //--//

            //
            // Entry:
            //  float* Aptr = Abase;
            //  float* Bptr = Bbase;
            //  float* Aend = &Aptr[N];
            //
            //  <Initialize Vector Processor with vector length = VectorSize>
            //  <Zero out the intermediate result registers>
            //
            //  goto LoopHeader;
            //
            {
                var vecOp1 = ARM.VectorHack_Prepare.New( debugInfo, vectorSize, regsResultBankBase[0].RegisterDescriptor );

                bbEntry.AddOperator( ZeligIR.SingleAssignmentOperator.New( debugInfo, varAptr, exAbase ) );
                bbEntry.AddOperator( ZeligIR.SingleAssignmentOperator.New( debugInfo, varBptr, exBbase ) );

                bbEntry.AddOperator( ZeligIR.BinaryOperator.New( debugInfo, ZeligIR.BinaryOperator.ALU.MUL, false, false, varSize, exN, ts.CreateConstant( (int)tdValue.SizeOfHoldingVariable ) ) );
                bbEntry.AddOperator( ZeligIR.BinaryOperator.New( debugInfo, ZeligIR.BinaryOperator.ALU.ADD, false, false, varAend, varAptr, varSize                                             ) );

                bbEntry.AddOperator( ARM.VectorHack_Initialize.New( debugInfo, vectorSize ) );

                bbEntry.AddOperator( vecOp1 );
                AddSideEffects( ts, vecOp1, regsResultBankBase );

                bbEntry.FlowControl = ZeligIR.UnconditionalControlOperator.New( debugInfo, bbLoopHeader );
            }

            //--//

            //
            // LoopHeader:
            //  if Aptr < Aend goto LoopBody else goto Exit;
            //
            {
                bbLoopHeader.FlowControl = ZeligIR.CompareConditionalControlOperator.New( debugInfo, ZeligIR.CompareAndSetOperator.ActionCondition.LT, false, varAptr, varAend, bbExit, bbLoopBody );
            }

            //--//

            //
            // LoopBody:
            //  <Load Left  Data using R4>
            //  <Load Right Data using R5>
            //  <Multiply And Accumulate S24 = S8 * S12>
            //
            //  goto LoopHeader;
            // 
            {
                var vecOp1 = ARM.VectorHack_LoadData             .New( debugInfo, vectorSize, regsLeftBankBase [0].RegisterDescriptor, varAptr, varAptr );
                var vecOp2 = ARM.VectorHack_LoadData             .New( debugInfo, vectorSize, regsRightBankBase[0].RegisterDescriptor, varBptr, varBptr );
                var vecOp3 = ARM.VectorHack_MultiplyAndAccumulate.New( debugInfo, vectorSize, regsLeftBankBase [0].RegisterDescriptor, regsRightBankBase[0].RegisterDescriptor, regsResultBankBase[0].RegisterDescriptor );

                bbLoopBody.AddOperator( vecOp1 );
                AddSideEffects( ts, vecOp1, regsLeftBankBase );

                bbLoopBody.AddOperator( vecOp2 );
                AddSideEffects( ts, vecOp2, regsRightBankBase );

                bbLoopBody.AddOperator( vecOp3 );
                AddSideEffects( ts, vecOp3, regsLeftBankBase   );
                AddSideEffects( ts, vecOp3, regsRightBankBase  );
                AddSideEffects( ts, vecOp3, regsResultBankBase );

                bbLoopBody.FlowControl = ZeligIR.UnconditionalControlOperator.New( debugInfo, bbLoopHeader );
            }

            //
            // Exit:
            //  <Finalize the vector sums into the result>
            //  <Cleanup the Vector Processor>
            //
            {
                var vecOp1 = ARM.VectorHack_Finalize.New( debugInfo, vectorSize, regsResultBankBase[0].RegisterDescriptor, varRes );

                bbExit.AddOperator( ARM.VectorHack_Cleanup.New( debugInfo, vectorSize ) );

                bbExit.AddOperator( vecOp1 );
                AddSideEffects( ts, vecOp1, regsResultBankBase );
            }

            //--//

            op.SubstituteWithSubGraph( bbEntry, bbExit );

            nc.MarkAsModified();
        }

        private ZeligIR.PhysicalRegisterExpression[] GenerateVector( ZeligIR.ControlFlowGraphStateForCodeTransformation cfg        ,
                                                                     ZeligIR.Abstractions.Platform                      pa         ,
                                                                     int                                                vectorSize ,
                                                                     uint                                               encoding   )
        {
            var res = new ZeligIR.PhysicalRegisterExpression[vectorSize];

            for(int i = 0; i < vectorSize; i++)
            {
                res[i] = cfg.AllocatePhysicalRegister( pa.GetRegisterForEncoding( encoding++ ) );
            }

            return res;
        }

        private void AddSideEffects( ZeligIR.TypeSystemForCodeTransformation ts   ,
                                     ZeligIR.Operator                        op   ,
                                     ZeligIR.PhysicalRegisterExpression[]    regs )
        {
            foreach(var reg in regs)
            {
                op.AddAnnotation( ZeligIR.PostInvalidationAnnotation.Create( ts, reg ) );
            }
        }
    }
}
