//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using Microsoft.Zelig.CodeGeneration.IR;
    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmCompilationState
    {
        //
        // State
        //

        private uint          m_stackOffsetForSAVED;
        private CodeMap.Flags m_headerFlags;

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        public override void EmitCodeForBasicBlock( ZeligIR.BasicBlock bb )
        {
            m_pendingCondition = EncodingDefinition_ARM.c_cond_AL;

            base.EmitCodeForBasicBlock( bb );
        }

        protected override bool EmitCodeForBasicBlock_ShouldSkip( ZeligIR.Operator op )
        {
            return base.EmitCodeForBasicBlock_ShouldSkip( op );
        }

        protected override uint EmitCodeForBasicBlock_EstimateMinimumSize( ZeligIR.Operator op )
        {
            if(EmitCodeForBasicBlock_ShouldSkip( op ))
            {
                return 0;
            }

            if(op is ZeligIR.NopOperator                      ||
               op is ZeligIR.AddActivationRecordEventOperator  )
            {
                return 0;
            }

            if(op is ZeligIR.SingleAssignmentOperator)
            {
                ZeligIR.PhysicalRegisterExpression lhs = op.FirstResult   as ZeligIR.PhysicalRegisterExpression;
                ZeligIR.PhysicalRegisterExpression rhs = op.FirstArgument as ZeligIR.PhysicalRegisterExpression;

                if(lhs != null && rhs != null)
                {
                    if(lhs.RegisterDescriptor == rhs.RegisterDescriptor)
                    {
                        return 0;
                    }
                }
            }

            if(op is ZeligIR.UnconditionalControlOperator)
            {
                ZeligIR.UnconditionalControlOperator opCtrl = (ZeligIR.UnconditionalControlOperator)op;

                ZeligIR.ImageBuilders.Core.BranchEncodingLevel level = m_owner.GetEncodingLevelForBranch( op, opCtrl.TargetBranch, false );
                if(level == ZeligIR.ImageBuilders.Core.BranchEncodingLevel.Skip)
                {
                    return 0;
                }
            }

            if(op is ARM.MoveStackPointerOperator)
            {
                if(m_stackOffsetForSAVED == 0)
                {
                    return 0;
                }
            }

            if(op is ARM.MoveIntegerRegistersOperator)
            {
                uint mask;
                bool fFast;
                bool fSkip;

                PrepareCode_ARM_MoveIntegerRegistersOperator( (ARM.MoveIntegerRegistersOperator)op, out mask, out fFast, out fSkip );
                
                if(fSkip)
                {
                    return 0;
                }
            }

            if(op is ARM.MoveFloatingPointRegistersOperator)
            {
                uint indexLow;
                uint indexHigh;
                bool fSkip;

                PrepareCode_ARM_MoveFloatingPointRegistersOperator( (ARM.MoveFloatingPointRegistersOperator)op, out indexLow, out indexHigh, out fSkip );
                
                if(fSkip)
                {
                    return 0;
                }
            }

            return sizeof(uint);
        }

        protected override void EmitCodeForBasicBlock_EmitOperator( ZeligIR.Operator op )
        {
            AddOperatorContext( op );

            if(op is ZeligIR.UnconditionalControlOperator)
            {
                EmitCode_UnconditionalControlOperator( (ZeligIR.UnconditionalControlOperator)op );
            }
            else if(op is ZeligIR.MultiWayConditionalControlOperator)
            {
                EmitCode_MultiWayConditionalControlOperator( (ZeligIR.MultiWayConditionalControlOperator)op );
            }
            else if(op is ZeligIR.ConditionCodeConditionalControlOperator)
            {
                EmitCode_ConditionCodeConditionalControlOperator( (ZeligIR.ConditionCodeConditionalControlOperator)op );
            }
            else if(op is ZeligIR.DeadControlOperator)
            {
                EmitCode_DeadControlOperator( (ZeligIR.DeadControlOperator)op );
            }
            else if(op is ZeligIR.ReturnControlOperator)
            {
                EmitCode_ReturnControlOperator( (ZeligIR.ReturnControlOperator)op );
            }
            else if(op is ZeligIR.NopOperator)
            {
                EmitCode_NopOperator( (ZeligIR.NopOperator)op );
            }
            else if(op is ZeligIR.SingleAssignmentOperator)
            {
                EmitCode_SingleAssignmentOperator( (ZeligIR.SingleAssignmentOperator)op );
            }
            else if(op is ZeligIR.PartialAssignmentOperator)
            {
                EmitCode_PartialAssignmentOperator( (ZeligIR.PartialAssignmentOperator)op );
            }
            else if(op is ZeligIR.AddressAssignmentOperator)
            {
                EmitCode_AddressAssignmentOperator( (ZeligIR.AddressAssignmentOperator)op );
            }
            else if(op is ZeligIR.CompareOperator)
            {
                EmitCode_CompareOperator( (ZeligIR.CompareOperator)op );
            }
            else if(op is ZeligIR.BitTestOperator)
            {
                EmitCode_BitTestOperator( (ZeligIR.BitTestOperator)op );
            }
            else if(op is ZeligIR.ConditionalCompareOperator)
            {
                EmitCode_ConditionalCompareOperator( (ZeligIR.ConditionalCompareOperator)op );
            }
            else if(op is ZeligIR.AbstractBinaryOperator)
            {
                EmitCode_AbstractBinaryOperator( (ZeligIR.AbstractBinaryOperator)op );
            }
            else if(op is ZeligIR.AbstractUnaryOperator)
            {
                EmitCode_AbstractUnaryOperator( (ZeligIR.AbstractUnaryOperator)op );
            }
            else if(op is ZeligIR.ZeroExtendOperator)
            {
                EmitCode_ZeroExtendOperator( (ZeligIR.ZeroExtendOperator)op );
            }
            else if(op is ZeligIR.SignExtendOperator)
            {
                EmitCode_SignExtendOperator( (ZeligIR.SignExtendOperator)op );
            }
            else if(op is ZeligIR.TruncateOperator)
            {
                EmitCode_TruncateOperator( (ZeligIR.TruncateOperator)op );
            }
            else if(op is ZeligIR.ConvertOperator)
            {
                EmitCode_ConvertOperator( (ZeligIR.ConvertOperator)op );
            }
            else if(op is ZeligIR.SetIfConditionIsTrueOperator)
            {
                EmitCode_SetIfConditionIsTrueOperator( (ZeligIR.SetIfConditionIsTrueOperator)op );
            }
            else if(op is ZeligIR.LoadIndirectOperator)
            {
                EmitCode_LoadIndirectOperator( (ZeligIR.LoadIndirectOperator)op );
            }
            else if(op is ZeligIR.StoreIndirectOperator)
            {
                EmitCode_StoreIndirectOperator( (ZeligIR.StoreIndirectOperator)op );
            }
            else if(op is ZeligIR.DirectSubroutineOperator)
            {
                EmitCode_DirectSubroutineOperator( (ZeligIR.DirectSubroutineOperator)op );
            }
            else if(op is ZeligIR.IndirectSubroutineOperator)
            {
                EmitCode_IndirectSubroutineOperator( (ZeligIR.IndirectSubroutineOperator)op );
            }
            else if(op is ZeligIR.AddActivationRecordEventOperator)
            {
                EmitCode_AddActivationRecordEventOperator( (ZeligIR.AddActivationRecordEventOperator)op );
            }
            else if(op is ZeligIR.ExternalCallOperator)
            {
                EmitCode_ExternalMethodCallOperator( (ZeligIR.ExternalCallOperator)op );
            }
            //--//--//--//--//--//--//--//--//--//--//--//
            else if(op is ARM.BinaryOperatorWithShift)
            {
                EmitCode_ARM_BinaryOperatorWithShift( (ARM.BinaryOperatorWithShift)op );
            }
            else if(op is ARM.LoadIndirectOperatorWithIndexUpdate)
            {
                EmitCode_ARM_LoadIndirectOperatorWithIndexUpdate( (ARM.LoadIndirectOperatorWithIndexUpdate)op );
            }
            else if(op is ARM.StoreIndirectOperatorWithIndexUpdate)
            {
                EmitCode_ARM_StoreIndirectOperatorWithIndexUpdate( (ARM.StoreIndirectOperatorWithIndexUpdate)op );
            }
            else if(op is ARM.SetStatusRegisterOperator)
            {
                EmitCode_ARM_SetStatusRegisterOperator( (ARM.SetStatusRegisterOperator)op );
            }
            else if(op is ARM.GetStatusRegisterOperator)
            {
                EmitCode_ARM_GetStatusRegisterOperator( (ARM.GetStatusRegisterOperator)op );
            }
            else if(op is ARM.MoveFromCoprocessor)
            {
                EmitCode_ARM_MoveFromCoprocessor( (ARM.MoveFromCoprocessor)op );
            }
            else if(op is ARM.MoveToCoprocessor)
            {
                EmitCode_ARM_MoveToCoprocessor( (ARM.MoveToCoprocessor)op );
            }
            else if(op is ARM.MoveIntegerRegistersOperator)
            {
                EmitCode_ARM_MoveIntegerRegistersOperator( (ARM.MoveIntegerRegistersOperator)op );
            }
            else if(op is ARM.MoveFloatingPointRegistersOperator)
            {
                EmitCode_ARM_MoveFloatingPointRegistersOperator( (ARM.MoveFloatingPointRegistersOperator)op );
            }
            else if(op is ARM.MoveStackPointerOperator)
            {
                EmitCode_ARM_MoveStackPointerOperator( (ARM.MoveStackPointerOperator)op );
            }
            else if(op is ARM.BreakpointOperator)
            {
                EmitCode_ARM_BreakpointOperator( (ARM.BreakpointOperator)op );
            }
            else if(op is ARM.VectorHack_Initialize)
            {
                EmitCode_ARM_VectorHack_Initialize( (ARM.VectorHack_Initialize)op );
            }
            else if(op is ARM.VectorHack_Prepare)
            {
                EmitCode_ARM_VectorHack_Prepare( (ARM.VectorHack_Prepare)op );
            }
            else if(op is ARM.VectorHack_LoadData)
            {
                EmitCode_ARM_VectorHack_LoadData( (ARM.VectorHack_LoadData)op );
            }
            else if(op is ARM.VectorHack_MultiplyAndAccumulate)
            {
                EmitCode_ARM_VectorHack_MultiplyAndAccumulate( (ARM.VectorHack_MultiplyAndAccumulate)op );
            }
            else if(op is ARM.VectorHack_Finalize)
            {
                EmitCode_ARM_VectorHack_Finalize( (ARM.VectorHack_Finalize)op );
            }
            else if(op is ARM.VectorHack_Cleanup)
            {
                EmitCode_ARM_VectorHack_Cleanup( (ARM.VectorHack_Cleanup)op );
            }
            else
            {
                throw NotImplemented();
            }
        }

        protected override void EmitCodeForBasicBlock_FlushOperators()
        {
            FlushOperatorContext();
        }

        private void UpdateLivenessInfo( BitVector aliveHistory ,
                                         BitVector alive        )
        {
            aliveHistory.XorInPlace( alive );

            //
            // Now 'aliveHistory' contains the set of variables that changed liveness status.
            //
            // We do two passes, first track the variables that went dead, then the ones that became alive.
            // The reason for this is that multiple variables map to the same physical register,
            // it's normal for a register to die and be born on the same instruction.
            // The end result should be a register that is still alive.
            // We'll compress the table later, when we switch from variables to actual storage places.
            //
            foreach(int idx in aliveHistory)
            {
                if(alive[idx] == false)
                {
                    TrackVariable( m_variables[idx], false );
                }
            }

            foreach(int idx in aliveHistory)
            {
                if(alive[idx] == true)
                {
                    TrackVariable( m_variables[idx], true );
                }
            }

            aliveHistory.Assign( alive );
        }

        private ArmCodeRelocation_Branch CreateRelocation_Branch( ZeligIR.Operator   opSource     ,
                                                                  ZeligIR.BasicBlock bb           ,
                                                                  int                skew         ,
                                                                  bool               fConditional )
        {
            return new ArmCodeRelocation_Branch( m_activeCodeSection.Context, m_activeCodeSection.Offset, bb, this, opSource, skew, fConditional );
        }

        private ArmCodeRelocation_LDR CreateRelocation_LDR( ZeligIR.Operator opSource ,
                                                            object           obj      ,
                                                            int              skew     )
        {
            ZeligIR.ImageBuilders.CodeConstant cc = m_owner.CompileCodeConstant( obj, m_activeCodeRegion );

            ArmCodeRelocation_LDR reloc = new ArmCodeRelocation_LDR( m_activeCodeSection.Context, m_activeCodeSection.Offset, cc, this, opSource, skew );

            cc.Source = reloc;

            return reloc;
        }

        private ArmCodeRelocation_FLD CreateRelocation_FLD( ZeligIR.Operator opSource ,
                                                            object           obj      ,
                                                            int              skew     )
        {
            ZeligIR.ImageBuilders.CodeConstant cc = m_owner.CompileCodeConstant( obj, m_activeCodeRegion );

            ArmCodeRelocation_FLD reloc = new ArmCodeRelocation_FLD( m_activeCodeSection.Context, m_activeCodeSection.Offset, cc, this, opSource, skew );

            cc.Source = reloc;

            return reloc;
        }

        private ArmCodeRelocation_MOV CreateRelocation_MOV( ZeligIR.Operator opSource     ,
                                                            object           obj          ,
                                                            int              skew         ,
                                                            int              numOfOpcodes )
        {
            ZeligIR.ImageBuilders.CodeConstant cc = m_owner.CompileCodeConstant( obj, m_activeCodeRegion );

            ArmCodeRelocation_MOV reloc = new ArmCodeRelocation_MOV( m_activeCodeSection.Context, m_activeCodeSection.Offset, cc, this, opSource, skew, numOfOpcodes );

            cc.Source = reloc;

            return reloc;
        }

        //--//

        private void EmitCode_BranchToBasicBlock( ZeligIR.Operator   opSource     ,
                                                  ZeligIR.BasicBlock bb           ,
                                                  bool               fConditional )
        {
            m_owner.CompileBasicBlock( bb );

            var level = GetEncodingLevelForBranch( opSource, bb, fConditional );

            switch(level)
            {
                case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.Skip:
                    if(AreBasicBlocksAdjacent( opSource.BasicBlock, bb ) == false)
                    {
                        goto case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.ShortBranch;
                    }
                    RecordAdjacencyNeed( opSource, bb );
                    break;

                case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.ShortBranch:
                    CreateRelocation_Branch( opSource, bb, EncodingDefinition_ARM.c_PC_offset, fConditional );
                    EmitOpcode__BR( 0 );
                    break;

                case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.NearRelativeLoad:
                    CreateRelocation_LDR( opSource, bb, EncodingDefinition_ARM.c_PC_offset );
                    EmitOpcode__LDR( m_reg_PC, m_reg_PC, 0 );
                    break;

                default:
                    Build32BitIntegerExpression( opSource, m_reg_Scratch, bb );

                    EmitOpcode__LDRwithRotate( m_reg_PC, m_reg_PC, m_reg_Scratch, 0 );
                    break;
            }
        }

        private void EmitCode_ExternalMethodCallOperator( ZeligIR.ExternalCallOperator op )
        {
            m_pendingCondition = EncodingDefinition_ARM.c_cond_AL;

            FlushOperatorContext();

            if(!op.Encode( m_activeCodeRegion, m_activeCodeSection, m_owner ))
            {
                throw new Exception( "Unable to generate external method call: " + op.DebugInfo != null ? op.DebugInfo.MethodName : "" );
            }
        }

        private void EmitCode_UnconditionalControlOperator( ZeligIR.UnconditionalControlOperator op )
        {
            ZeligIR.BasicBlock bb = op.TargetBranch;

            EmitCode_BranchToBasicBlock( op, bb, false );
        }

        private void EmitCode_MultiWayConditionalControlOperator( ZeligIR.MultiWayConditionalControlOperator op )
        {
            ZeligIR.BasicBlock[]                    bbTargets = op.Targets;
            ZeligIR.BasicBlock                      bbDefault = op.TargetBranchNotTaken;
            ZeligIR.Abstractions.RegisterDescriptor valRD     = GetRegisterDescriptor( op.FirstArgument );

            if(valRD != null)
            {
                uint valSeed;
                uint valRot;

                if(CanEncodeAs8BitImmediate( bbTargets.Length, out valSeed, out valRot ))
                {
                    EmitOpcode__CMP_CONST( valRD, valSeed, valRot );
                }
                else
                {
                    Build32BitIntegerExpression( op, m_reg_Scratch, bbTargets.Length );

                    EmitOpcode__CMP( valRD, m_reg_Scratch );
                }

                //
                // The offset is zero because it's implicit a +8 in the value of PC.
                //
                PrepareCondition( Abstractions.ArmPlatform.Comparison.UnsignedLowerThan ); // Unsigned Lower
                EmitOpcode__LDRwithRotate( m_reg_PC, m_reg_PC, valRD, 2u );

                EmitCode_BranchToBasicBlock( op, bbDefault, true );

                //
                // Emit the table.
                //
                foreach(ZeligIR.BasicBlock bbTarget in bbTargets)
                {
                    m_activeCodeSection.WritePointerToBasicBlock( bbTarget );
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_ConditionCodeConditionalControlOperator( ZeligIR.ConditionCodeConditionalControlOperator op )
        {
            ZeligIR.BasicBlock bbThis     = op.BasicBlock;
            ZeligIR.BasicBlock bbTaken    = op.TargetBranchTaken;
            ZeligIR.BasicBlock bbNotTaken = op.TargetBranchNotTaken;
            bool               fConditionalOnTaken;

            if(AreBasicBlocksAdjacent( bbThis, bbNotTaken ))
            {
                fConditionalOnTaken = true;
            }
            else if(AreBasicBlocksAdjacent( bbThis, bbTaken ))
            {
                fConditionalOnTaken = false;
            }
            else
            {
                if(CompareSchedulingWeights( bbTaken, bbNotTaken ) >= 0)
                {
                    fConditionalOnTaken = true;
                }
                else
                {
                    fConditionalOnTaken = false;
                }
            }

            if(fConditionalOnTaken)
            {
                PrepareCondition( op.Condition );
                EmitCode_BranchToBasicBlock( op, bbTaken   , true  );
                EmitCode_BranchToBasicBlock( op, bbNotTaken, false );
            }
            else
            {
                PrepareCondition( ZeligIR.ConditionCodeExpression.NegateCondition( op.Condition ) );
                EmitCode_BranchToBasicBlock( op, bbNotTaken, true  );
                EmitCode_BranchToBasicBlock( op, bbTaken   , false );
            }
        }

        //--//

        private void EmitCode_DeadControlOperator( ZeligIR.DeadControlOperator op )
        {
            // Nothing to do, we should never reach this point.

            EmitOpcode__SWI( 0 );
        }

        private void EmitCode_ReturnControlOperator( ZeligIR.ReturnControlOperator op )
        {
            if(m_activeHardwareException == Runtime.HardwareException.VectorTable)
            {
                EmitCodeForVector( Runtime.HardwareException.Bootstrap            );
                EmitCodeForVector( Runtime.HardwareException.UndefinedInstruction );
                EmitCodeForVector( Runtime.HardwareException.SoftwareInterrupt    );
                EmitCodeForVector( Runtime.HardwareException.PrefetchAbort        );
                EmitCodeForVector( Runtime.HardwareException.DataAbort            );
                EmitCodeForVector( Runtime.HardwareException.None                 ); // Unused
                EmitCodeForVector( Runtime.HardwareException.Interrupt            );
                EmitCodeForVector( Runtime.HardwareException.FastInterrupt        );
            }
            else if(m_activeHardwareException == Runtime.HardwareException.Bootstrap)
            {
                EmitCodeForVector( Runtime.HardwareException.Reset );
            }
            else
            {
                // Nothing to do, the return values are already in the correct registers/stack locations.
            }
        }

        private void EmitCodeForVector( Runtime.HardwareException he )
        {
            MethodRepresentation md = m_owner.TypeSystem.TryGetHandler( he );

            if(md != null)
            {
                ZeligIR.ControlFlowGraphStateForCodeTransformation cfg = ZeligIR.TypeSystemForCodeTransformation.GetCodeForMethod( md );

                if(cfg == null)
                {


                    return;
                }

                CHECKS.ASSERT( cfg != null, "Cannot compile a method without implementation: {0}", md );

                m_owner.CompileMethod( cfg );

                CreateRelocation_LDR( cfg.EntryBasicBlock.FirstOperator, cfg.EntryBasicBlock, EncodingDefinition_ARM.c_PC_offset );
                EmitOpcode__LDR( m_reg_PC, m_reg_PC, 0 );
            }
            else
            {
                EmitOpcode__SWI( (uint)he );
            }
        }

        //--//

        private void EmitCode_NopOperator( ZeligIR.NopOperator op )
        {
            // TODO: Add a nop opcode for debug builds.
////        if(op.DebugInfo != null)
////        {
////            EmitOpcode__MOV( EncodingDefinition_ARM.c_register_r0, EncodingDefinition_ARM.c_register_r0 );
////        }
        }

        //--//

        private void EmitCode_CompareOperator( ZeligIR.CompareOperator op )
        {
            EmitCode_CommonCompare( op );
        }

        private void EmitCode_ConditionalCompareOperator( ZeligIR.ConditionalCompareOperator op )
        {
            PrepareCondition( op.Condition );

            EmitCode_CommonCompare( op );
        }

        private void EmitCode_CommonCompare( ZeligIR.Operator op )
        {
            ZeligIR.Expression                      left     = op.FirstArgument;
            ZeligIR.Expression                      right    = op.SecondArgument;
            ZeligIR.Abstractions.RegisterDescriptor leftReg  = GetRegisterDescriptor( left  );
            ZeligIR.Abstractions.RegisterDescriptor rightReg = GetRegisterDescriptor( right );

            if(leftReg != null && rightReg != null)
            {
                if(leftReg.InIntegerRegisterFile)
                {
                    EmitOpcode__CMP( leftReg, rightReg );
                }
                else
                {
                    EmitOpcode__FCMP( leftReg, rightReg );
                    EmitOpcode__FMSTAT();
                }
            }
            else if(leftReg != null && right is ZeligIR.ConstantExpression)
            {
                if(leftReg.InIntegerRegisterFile)
                {
                    uint valSeed;
                    uint valRot;

                    EncodeAs8BitImmediate( right, out valSeed, out valRot );

                    EmitOpcode__CMP_CONST( leftReg, valSeed, valRot );
                }
                else
                {
                    if(right.IsEqualToZero())
                    {
                        EmitOpcode__FCMPZ( leftReg );
                        EmitOpcode__FMSTAT();
                    }
                    else
                    {
                        throw NotImplemented();
                    }
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        //--//

        private void EmitCode_BitTestOperator( ZeligIR.BitTestOperator op )
        {
            EmitCode_CommonBitTest( op );
        }

        private void EmitCode_CommonBitTest( ZeligIR.Operator op )
        {
            ZeligIR.Expression                      left     = op.FirstArgument;
            ZeligIR.Expression                      right    = op.SecondArgument;
            ZeligIR.Abstractions.RegisterDescriptor leftReg  = GetRegisterDescriptor( left  );
            ZeligIR.Abstractions.RegisterDescriptor rightReg = GetRegisterDescriptor( right );

            if(leftReg != null && rightReg != null)
            {
                if(leftReg.InIntegerRegisterFile)
                {
                    EmitOpcode__TST( leftReg, rightReg );
                }
                else
                {
                    throw NotImplemented();
                }
            }
            else if(leftReg != null && right is ZeligIR.ConstantExpression)
            {
                if(leftReg.InIntegerRegisterFile)
                {
                    uint valSeed;
                    uint valRot;

                    EncodeAs8BitImmediate( right, out valSeed, out valRot );

                    EmitOpcode__TST_CONST( leftReg, valSeed, valRot );
                }
                else
                {
                    throw NotImplemented();
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        //--//

        private void EmitCode_SetIfConditionIsTrueOperator( ZeligIR.SetIfConditionIsTrueOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor    regRd = GetRegisterDescriptor( op.FirstResult );
            ZeligIR.ConditionCodeExpression.Comparison cond  = op.Condition;

            PrepareCondition(                                                  cond   ); EmitOpcode__MOV_CONST( regRd, 1, 0 );
            PrepareCondition( ZeligIR.ConditionCodeExpression.NegateCondition( cond ) ); EmitOpcode__MOV_CONST( regRd, 0, 0 );
        }

        //--//

        private void EmitCode_SingleAssignmentOperator( ZeligIR.SingleAssignmentOperator op )
        {
            ZeligIR.VariableExpression              lhs    = op.FirstResult;
            ZeligIR.Expression                      rhs    = op.FirstArgument;
            ZeligIR.Abstractions.RegisterDescriptor lhsReg = GetRegisterDescriptor( lhs );
            ZeligIR.Abstractions.RegisterDescriptor rhsReg = GetRegisterDescriptor( rhs );

            if(lhsReg != null && rhsReg != null)
            {
                uint offsetRhs = 0;

                if(op.HasAnnotation<ExternalCallArgumentAnnotation>())
                {
                    if(rhs.Type == m_owner.TypeSystem.WellKnownTypes.System_String)
                    {
                        offsetRhs += 8;
                    }
                    else if(!( rhs.Type is ValueTypeRepresentation || rhs.Type.UnderlyingType is ValueTypeRepresentation ))
                    {
                        offsetRhs += 4;
                    }
                }

                EmitCode_MoveRegisters( lhsReg, 0, rhsReg, offsetRhs );
            }
            else if(lhsReg != null && rhs is ZeligIR.ConstantExpression)
            {
                if(lhsReg.InIntegerRegisterFile)
                {
                    Build32BitIntegerExpression( op, lhsReg, rhs );
                }
                else
                {
                    if(lhsReg.IsDoublePrecision)
                    {
                        Build64BitFloatingPointExpression( op, lhsReg, rhs );
                    }
                    else
                    {
                        Build32BitFloatingPointExpression( op, lhsReg, rhs );
                    }
                }
            }
            else if(lhsReg != null && rhs is ZeligIR.StackLocationExpression)
            {
                int offsetRhs = GetOffsetOfStackLocation( rhs );

                if(op.HasAnnotation<ExternalCallArgumentAnnotation>())
                {
                    if(rhs.Type == m_owner.TypeSystem.WellKnownTypes.System_String)
                    {
                        offsetRhs += 8;
                    }
                    else if(!( rhs.Type is ValueTypeRepresentation || rhs.Type.UnderlyingType is ValueTypeRepresentation ))
                    {
                        offsetRhs += 4;
                    }
                }

                EmitOpcode__Load( lhsReg, m_reg_SP, offsetRhs, rhs.Type );
            }
            else if(lhs is ZeligIR.StackLocationExpression && rhs is ZeligIR.ConstantExpression)
            {
                int offsetLhs = GetOffsetOfStackLocation( lhs );

                Build32BitIntegerExpression( op, m_reg_Scratch, rhs );
    
                EmitOpcode__Store( m_reg_SP, offsetLhs, m_reg_Scratch, lhs.Type );
            }
            else if(lhs is ZeligIR.StackLocationExpression && rhsReg != null)
            {
                int offsetLhs = GetOffsetOfStackLocation( lhs );

                EmitOpcode__Store( m_reg_SP, offsetLhs, rhsReg, lhs.Type );
            }
            else if(lhs is ZeligIR.StackLocationExpression && rhs is ZeligIR.StackLocationExpression)
            {
                int offsetLhs = GetOffsetOfStackLocation( lhs );
                int offsetRhs = GetOffsetOfStackLocation( rhs );

                if(offsetLhs != offsetRhs)
                {
                    EmitOpcode__Load( m_reg_Scratch, m_reg_SP, offsetRhs, rhs.Type );

                    EmitOpcode__Store( m_reg_SP, offsetLhs, m_reg_Scratch, lhs.Type );
                }
                else
                {
                    //
                    // Nothing to do, source and destination are at the same offset.
                    //
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_PartialAssignmentOperator( ZeligIR.PartialAssignmentOperator op )
        {
            ZeligIR.VariableExpression              lhs    = op.FirstResult;
            ZeligIR.Expression                      rhs    = op.FirstArgument;
            ZeligIR.Abstractions.RegisterDescriptor lhsReg = GetRegisterDescriptor( lhs );
            ZeligIR.Abstractions.RegisterDescriptor rhsReg = GetRegisterDescriptor( rhs );

            if(lhsReg != null && rhsReg != null)
            {
                EmitCode_MoveRegisters( lhsReg, op.DestinationOffset, rhsReg, op.SourceOffset );
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_MoveRegisters( ZeligIR.Abstractions.RegisterDescriptor lhsReg    ,
                                             uint                                    lhsOffset ,
                                             ZeligIR.Abstractions.RegisterDescriptor rhsReg    ,
                                             uint                                    rhsOffset )
        {
            switch(ZeligIR.Abstractions.RegisterDescriptor.GetPairState( lhsReg, rhsReg ))
            {
                case ZeligIR.Abstractions.RegisterDescriptor.Pair.Int_Int:
                    if(rhsOffset == 0)
                    {
                        EmitOpcode__MOV_IfDifferent( lhsReg, rhsReg );
                    }
                    else
                    {
                        EmitOpcode__ADD_CONST( lhsReg, rhsReg, rhsOffset, 0 );
                    }
                    break;

                case ZeligIR.Abstractions.RegisterDescriptor.Pair.Int_FP:
                    if(rhsReg.IsDoublePrecision)
                    {
                        if(lhsReg.PhysicalStorageSize == 1)
                        {
                            if(rhsOffset == 0)
                            {
                                EmitOpcode__FMRDL( lhsReg, rhsReg );
                            }
                            else
                            {
                                EmitOpcode__FMRDH( lhsReg, rhsReg );
                            }
                        }
                        else
                        {
                            throw NotImplemented();
                        }
                    }
                    else
                    {
                        EmitOpcode__FMRS( lhsReg, rhsReg );
                    }
                    break;

                case ZeligIR.Abstractions.RegisterDescriptor.Pair.FP_Int:
                    if(lhsReg.IsDoublePrecision)
                    {
                        if(rhsReg.PhysicalStorageSize == 1)
                        {
                            if(lhsOffset == 0)
                            {
                                EmitOpcode__FMDLR( rhsReg, lhsReg );
                            }
                            else
                            {
                                EmitOpcode__FMDHR( rhsReg, lhsReg );
                            }
                        }
                        else
                        {
                            throw NotImplemented();
                        }
                    }
                    else
                    {
                        EmitOpcode__FMSR( rhsReg, lhsReg );
                    }
                    break;

                case ZeligIR.Abstractions.RegisterDescriptor.Pair.FP_FP:
                    EmitOpcode__FCPY_IfDifferent( lhsReg, rhsReg );
                    break;

                case ZeligIR.Abstractions.RegisterDescriptor.Pair.Int_Sys:
                    EmitOpcode__FMRX( lhsReg, rhsReg );
                    break;

                case ZeligIR.Abstractions.RegisterDescriptor.Pair.Sys_Int:
                    EmitOpcode__FMXR( rhsReg, lhsReg );
                    break;

                default:
                    throw TypeConsistencyErrorException.Create( "Unexpected pair of registers for assignment: {0} = {1}", lhsReg, rhsReg );
            }
        }

        private void EmitCode_AddressAssignmentOperator( ZeligIR.AddressAssignmentOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor lhsReg = GetRegisterDescriptor( op.FirstResult );
            ZeligIR.StackLocationExpression         rhs    = (ZeligIR.StackLocationExpression)op.FirstArgument;
            int                                     offset = GetOffsetOfStackLocation( rhs );
            uint                                    valSeed;
            uint                                    valRot;

            if(CanEncodeAs8BitImmediate( offset, out valSeed, out valRot ))
            {
                EmitOpcode__ADD_CONST( lhsReg, m_reg_SP, valSeed, valRot );
            }
            else
            {
                Build32BitIntegerExpression( op, lhsReg, offset );

                EmitOpcode__ADD( lhsReg, m_reg_SP, lhsReg );
            }
        }

        private void EmitCode_AbstractBinaryOperator( ZeligIR.AbstractBinaryOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regLhs                  = GetRegisterDescriptor( op.FirstResult );
            ZeligIR.Expression                      left                    =                        op.FirstArgument;
            ZeligIR.Expression                      right                   =                        op.SecondArgument;
            ZeligIR.Abstractions.RegisterDescriptor regLeft                 = GetRegisterDescriptor( left );
            ZeligIR.Abstractions.RegisterDescriptor regRight                = GetRegisterDescriptor( right );
            bool                                    fIsBin                  = op is ZeligIR.BinaryOperator;
            bool                                    fIsBinWithCarryIn       = op is ZeligIR.BinaryOperatorWithCarryIn;
            bool                                    fIsBinWithCarryInAndOut = op is ZeligIR.BinaryOperatorWithCarryInAndOut;
            bool                                    fIsBinWithCarryOut      = op is ZeligIR.BinaryOperatorWithCarryOut;
            bool                                    fIsLong                 = op is ZeligIR.LongBinaryOperator;

            if(regLeft != null && regRight != null)
            {
                if(regLhs.InIntegerRegisterFile)
                {
                    if(fIsLong)
                    {
                        switch(op.Alu)
                        {
                            case ZeligIR.BinaryOperator.ALU.MUL:
                                ZeligIR.Abstractions.RegisterDescriptor regLhsHi = GetRegisterDescriptor( op.SecondResult );

                                EmitOpcode__MULL( regLhsHi, regLhs, regLeft, regRight, op.Signed );
                                break;

                            default:
                                throw NotImplemented();
                        }
                    }
                    else if(fIsBin || fIsBinWithCarryOut)
                    {
                        switch(op.Alu)
                        {
                            case ZeligIR.BinaryOperator.ALU.ADD:
                                EmitOpcode__ADD( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.SUB:
                                EmitOpcode__SUB( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.MUL:
                                EmitOpcode__MUL( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.AND:
                                EmitOpcode__AND( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.OR:
                                EmitOpcode__OR( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.XOR:
                                EmitOpcode__EOR( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.SHL:
                                EmitOpcode__LSL( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.SHR:
                                if(op.Signed)
                                {
                                    EmitOpcode__ASR( regLhs, regLeft, regRight );
                                }
                                else
                                {
                                    EmitOpcode__LSR( regLhs, regLeft, regRight );
                                }
                                break;

                            default:
                                throw NotImplemented();
                        }
                    }
                    else if(fIsBinWithCarryIn || fIsBinWithCarryInAndOut)
                    {
                        switch(op.Alu)
                        {
                            case ZeligIR.BinaryOperator.ALU.ADD:
                                EmitOpcode__ADC( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.SUB:
                                EmitOpcode__SBC( regLhs, regLeft, regRight );
                                break;

                            default:
                                throw NotImplemented();
                        }
                    }
                    else
                    {
                        throw NotImplemented();
                    }
                }
                else
                {
                    if(fIsBin)
                    {
                        switch(op.Alu)
                        {
                            case ZeligIR.BinaryOperator.ALU.ADD:
                                EmitOpcode__FADD( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.SUB:
                                EmitOpcode__FSUB( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.MUL:
                                EmitOpcode__FMUL( regLhs, regLeft, regRight );
                                break;

                            case ZeligIR.BinaryOperator.ALU.DIV:
                                EmitOpcode__FDIV( regLhs, regLeft, regRight );
                                break;

                            default:
                                throw NotImplemented();
                        }
                    }
                    else
                    {
                        throw NotImplemented();
                    }
                }
            }
            else if(regLeft != null && right is ZeligIR.ConstantExpression)
            {
                if(fIsBin || fIsBinWithCarryOut)
                {
                    bool fCanUseInvertedForm;

                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.ADD:
                        case ZeligIR.BinaryOperator.ALU.SUB:
                        case ZeligIR.BinaryOperator.ALU.AND:
                            fCanUseInvertedForm = true;
                            break;

                        default:
                            fCanUseInvertedForm = false;
                            break;
                    }

                    uint valSeed;
                    uint valRot;
                    bool fInverted = EncodeAs8BitImmediate( right, fCanUseInvertedForm, out valSeed, out valRot );

                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.ADD:
                            if(fInverted)
                            {
                                EmitOpcode__SUB_CONST( regLhs, regLeft, valSeed, valRot );
                            }
                            else
                            {
                                EmitOpcode__ADD_CONST( regLhs, regLeft, valSeed, valRot );
                            }
                            break;

                        case ZeligIR.BinaryOperator.ALU.SUB:
                            if(fInverted)
                            {
                                EmitOpcode__ADD_CONST( regLhs, regLeft, valSeed, valRot );
                            }
                            else
                            {
                                EmitOpcode__SUB_CONST( regLhs, regLeft, valSeed, valRot );
                            }
                            break;

                        case ZeligIR.BinaryOperator.ALU.AND:
                            if(fInverted)
                            {
                                EmitOpcode__BIC_CONST( regLhs, regLeft, valSeed, valRot );
                            }
                            else
                            {
                                EmitOpcode__AND_CONST( regLhs, regLeft, valSeed, valRot );
                            }
                            break;

                        case ZeligIR.BinaryOperator.ALU.OR:
                            EmitOpcode__OR_CONST( regLhs, regLeft, valSeed, valRot );
                            break;

                        case ZeligIR.BinaryOperator.ALU.XOR:
                            EmitOpcode__EOR_CONST( regLhs, regLeft, valSeed, valRot );
                            break;

                        case ZeligIR.BinaryOperator.ALU.SHL:
                            if(valRot == 0)
                            {
                                EmitOpcode__LSL_CONST( regLhs, regLeft, valSeed );
                            }
                            else
                            {
                                EmitOpcode__MOV_CONST( regLhs, 0, 0 );
                            }
                            break;

                        case ZeligIR.BinaryOperator.ALU.SHR:
                            if(valRot == 0)
                            {
                                if(op.Signed)
                                {
                                    EmitOpcode__ASR_CONST( regLhs, regLeft, valSeed );
                                }
                                else
                                {
                                    EmitOpcode__LSR_CONST( regLhs, regLeft, valSeed );
                                }
                            }
                            else
                            {
                                if(op.Signed)
                                {
                                    EmitOpcode__ASR_CONST( regLhs, regLeft, 31 ); // Extend sign.
                                }
                                else
                                {
                                    EmitOpcode__MOV_CONST( regLhs, 0, 0 );
                                }
                            }
                            break;

                        default:
                            throw NotImplemented();
                    }
                }
                else if(fIsBinWithCarryIn || fIsBinWithCarryInAndOut)
                {
                    uint valSeed;
                    uint valRot;

                    EncodeAs8BitImmediate( right, out valSeed, out valRot );

                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.ADD:
                            EmitOpcode__ADC_CONST( regLhs, regLeft, valSeed, valRot );
                            break;

                        case ZeligIR.BinaryOperator.ALU.SUB:
                            EmitOpcode__SBC_CONST( regLhs, regLeft, valSeed, valRot );
                            break;

                        default:
                            throw NotImplemented();
                    }
                }
                else
                {
                    throw NotImplemented();
                }
            }
            else if(left is ZeligIR.ConstantExpression && regRight != null)
            {
                if(fIsBin || fIsBinWithCarryOut)
                {
                    uint valSeed;
                    uint valRot;

                    EncodeAs8BitImmediate( left, out valSeed, out valRot );

                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.SUB:
                            EmitOpcode__RSB_CONST( regLhs, regRight, valSeed, valRot );
                            break;

                        default:
                            throw NotImplemented();
                    }
                }
                else if(fIsBinWithCarryIn || fIsBinWithCarryInAndOut)
                {
                    uint valSeed;
                    uint valRot;

                    EncodeAs8BitImmediate( left, out valSeed, out valRot );

                    switch(op.Alu)
                    {
                        case ZeligIR.BinaryOperator.ALU.SUB:
                            EmitOpcode__RSC_CONST( regLhs, regRight, valSeed, valRot );
                            break;

                        // 'ALU.ADD' should not reach this point, thanks to calls to Operator.EnsureConstantToTheRight
                        default:
                            throw NotImplemented();
                    }
                }
                else
                {
                    throw NotImplemented();
                }
            }
            else
            {
                throw NotImplemented();
            }

            if(fIsBinWithCarryOut || fIsBinWithCarryInAndOut)
            {
                SetConditionBit();
            }
        }

        //--//

        private void EmitCode_AbstractUnaryOperator( ZeligIR.AbstractUnaryOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regLhs = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRhs = GetRegisterDescriptor( op.FirstArgument );

            if(regLhs != null && regRhs != null)
            {
                if(regLhs.InIntegerRegisterFile)
                {
                    switch(op.Alu)
                    {
                        case ZeligIR.UnaryOperator.ALU.NEG:
                            EmitOpcode__RSB_CONST( regLhs, regRhs, 0, 0 );
                            break;

                        case ZeligIR.UnaryOperator.ALU.NOT:
                            EmitOpcode__MVN( regLhs, regRhs );
                            break;

                        default:
                            throw NotImplemented();
                    }
                }
                else
                {
                    switch(op.Alu)
                    {
                        case ZeligIR.UnaryOperator.ALU.NEG:
                            EmitOpcode__FNEG( regLhs, regRhs );
                            break;

                        default:
                            throw NotImplemented();
                    }
                }
            }
            else
            {
                throw NotImplemented();
            }

            if(op is ZeligIR.UnaryOperatorWithCarryOut)
            {
                SetConditionBit();
            }
        }

        private void EmitCode_ZeroExtendOperator( ZeligIR.ZeroExtendOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.FirstArgument );
            uint                                    shift = 8 * (4 - op.SignificantSize);

            switch(shift)
            {
                case 0:
                    EmitOpcode__MOV_IfDifferent( regRd, regRs );
                    break;

                case 24:
                    EmitOpcode__AND_CONST( regRd, regRs, 0xFFu, 0 );
                    break;

                default:
                    EmitOpcode__LSL_CONST( regRd, regRs, shift );
                    EmitOpcode__LSR_CONST( regRd, regRd, shift );
                    break;
            }
        }

        private void EmitCode_SignExtendOperator( ZeligIR.SignExtendOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.FirstArgument );
            uint                                    shift = 8 * (4 - op.SignificantSize);

            switch(shift)
            {
                case 0:
                    EmitOpcode__MOV_IfDifferent( regRd, regRs );
                    break;

                default:
                    EmitOpcode__LSL_CONST( regRd, regRs, shift );
                    EmitOpcode__ASR_CONST( regRd, regRd, shift );
                    break;
            }
        }

        private void EmitCode_TruncateOperator( ZeligIR.TruncateOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.FirstArgument );
            uint                                    shift = 8 * (4 - op.SignificantSize);

            switch(shift)
            {
                case 0:
                    EmitOpcode__MOV_IfDifferent( regRd, regRs );
                    break;

                case 24:
                    EmitOpcode__AND_CONST( regRd, regRs, 0xFFu, 0 );
                    break;

                default:
                    EmitOpcode__LSL_CONST( regRd, regRs, shift );
                    EmitOpcode__LSR_CONST( regRd, regRd, shift );
                    break;
            }
        }

        private void EmitCode_ConvertOperator( ZeligIR.ConvertOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regLhs     = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRhs     = GetRegisterDescriptor( op.FirstArgument );
            TypeRepresentation.BuiltInTypes         kindInput  =                        op.InputKind;
            TypeRepresentation.BuiltInTypes         kindOutput =                        op.OutputKind;

            switch(kindOutput)
            {
                case TypeRepresentation.BuiltInTypes.R4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.I4:
                            EmitOpcode__FSITOS( regLhs, regRhs );
                            return;

                        case TypeRepresentation.BuiltInTypes.U4:
                            EmitOpcode__FUITOS( regLhs, regRhs );
                            return;

                        case TypeRepresentation.BuiltInTypes.R8:
                            EmitOpcode__FCVTSD( regLhs, regRhs );
                            return;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.R8:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.I4:
                            EmitOpcode__FSITOD( regLhs, regRhs );
                            return;

                        case TypeRepresentation.BuiltInTypes.U4:
                            EmitOpcode__FUITOD( regLhs, regRhs );
                            return;

                        case TypeRepresentation.BuiltInTypes.R4:
                            EmitOpcode__FCVTDS( regLhs, regRhs );
                            return;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.I4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4:
                            EmitOpcode__FTOSIS( regLhs, regRhs );
                            return;

                        case TypeRepresentation.BuiltInTypes.R8:
                            EmitOpcode__FTOSID( regLhs, regRhs );
                            return;
                    }
                    break;

                case TypeRepresentation.BuiltInTypes.U4:
                    switch(kindInput)
                    {
                        case TypeRepresentation.BuiltInTypes.R4:
                            EmitOpcode__FTOUIS( regLhs, regRhs );
                            return;

                        case TypeRepresentation.BuiltInTypes.R8:
                            EmitOpcode__FTOUID( regLhs, regRhs );
                            return;
                    }
                    break;
            }

            throw NotImplemented();
        }

        //--//

        private void EmitCode_LoadIndirectOperator( ZeligIR.LoadIndirectOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.FirstArgument );

            if(regRd != null && regRs != null)
            {
                EmitOpcode__Load( regRd, regRs, op.Offset, op.Type );
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_StoreIndirectOperator( ZeligIR.StoreIndirectOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstArgument  );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.SecondArgument );

            EmitOpcode__Store( regRd, op.Offset, regRs, op.Type );
        }

        //--//

        private void EmitCode_DirectSubroutineOperator( ZeligIR.DirectSubroutineOperator op )
        {
            ZeligIR.ControlFlowGraphStateForCodeTransformation cfg = ZeligIR.TypeSystemForCodeTransformation.GetCodeForMethod( op.TargetMethod );

            if(cfg == null)
            {


                return;
            }

            CHECKS.ASSERT( cfg != null, "Cannot compile a method without implementation: {0}", op.TargetMethod );

            ZeligIR.BasicBlock bb = cfg.EntryBasicBlock;

            m_owner.CompileBasicBlock( bb );

            var level = GetEncodingLevelForBranch( op, bb, false );

            bool fDeadJump;

            if((m_headerFlags & CodeMap.Flags.HasIntRegisterSave) != 0)
            {
                fDeadJump = false;
            }
            else
            {
                fDeadJump = op.GetNextOperator() is ZeligIR.DeadControlOperator;
            }

            switch(level)
            {
                case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.Skip:
                case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.ShortBranch:
                    CreateRelocation_Branch( op, bb, EncodingDefinition_ARM.c_PC_offset, false );

                    if(fDeadJump)
                    {
                        EmitOpcode__BR( 0 );
                    }
                    else
                    {
                        EmitOpcode__BL( 0 );
                    }
                    break;

                case ZeligIR.ImageBuilders.Core.BranchEncodingLevel.NearRelativeLoad:
                    if(!fDeadJump)
                    {
                        EmitOpcode__MOV( m_reg_LR, m_reg_PC );
                    }

                    CreateRelocation_LDR( op, bb, EncodingDefinition_ARM.c_PC_offset );
                    EmitOpcode__LDR( m_reg_PC, m_reg_PC, 0 );
                    break;

                default:
                    Build32BitIntegerExpression( op, m_reg_Scratch, bb );

                    EmitOpcode__MOV          ( m_reg_LR, m_reg_PC                   );
                    EmitOpcode__LDRwithRotate( m_reg_PC, m_reg_PC, m_reg_Scratch, 0 );
                    break;
            }
        }

        private void EmitCode_IndirectSubroutineOperator( ZeligIR.IndirectSubroutineOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regPointer = GetRegisterDescriptor( op.FirstArgument );

            EmitOpcode__MOV( m_reg_LR, m_reg_PC   );
            EmitOpcode__MOV( m_reg_PC, regPointer );
        }

        //--//

        private void EmitCode_ARM_BinaryOperatorWithShift( ARM.BinaryOperatorWithShift op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regLhs   = GetRegisterDescriptor( op.FirstResult );
            ZeligIR.Expression                      left     =                        op.FirstArgument;
            ZeligIR.Expression                      right    =                        op.SecondArgument;
            ZeligIR.Expression                      shift    =                        op.ThirdArgument;
            ZeligIR.Abstractions.RegisterDescriptor regLeft  = GetRegisterDescriptor( left  );
            ZeligIR.Abstractions.RegisterDescriptor regRight = GetRegisterDescriptor( right );
            ZeligIR.Abstractions.RegisterDescriptor regShift = GetRegisterDescriptor( shift );

            if(regLeft != null && regRight != null)
            {
                if(regLhs.InIntegerRegisterFile)
                {
                    uint cpuALU;
                    uint cpuSHIFT;

                    switch(op.Alu)
                    {
                        case ZeligIR.AbstractBinaryOperator.ALU.ADD: cpuALU = EncodingDefinition_ARM.c_operation_ADD; break;
                        case ZeligIR.AbstractBinaryOperator.ALU.SUB: cpuALU = EncodingDefinition_ARM.c_operation_SUB; break;
                        case ZeligIR.AbstractBinaryOperator.ALU.AND: cpuALU = EncodingDefinition_ARM.c_operation_AND; break;
                        case ZeligIR.AbstractBinaryOperator.ALU.OR : cpuALU = EncodingDefinition_ARM.c_operation_ORR; break;
                        case ZeligIR.AbstractBinaryOperator.ALU.XOR: cpuALU = EncodingDefinition_ARM.c_operation_EOR; break;

                        default:
                            throw NotImplemented();
                    }

                    switch(op.AluShift)
                    {
                        case ZeligIR.AbstractBinaryOperator.ALU.SHL: cpuSHIFT =                                                   EncodingDefinition_ARM.c_shift_LSL; break;
                        case ZeligIR.AbstractBinaryOperator.ALU.SHR: cpuSHIFT = op.SignedShift ? EncodingDefinition_ARM.c_shift_ASR : EncodingDefinition_ARM.c_shift_LSR; break;

                        default:
                            throw NotImplemented();
                    }

                    if(regShift != null)
                    {
                        InstructionSet.Opcode_DataProcessing_3 enc = this.Encoder.PrepareForDataProcessing_3;

                        enc.Prepare( m_pendingCondition             ,  // uint ConditionCodes ,
                                     GetIntegerEncoding( regLeft )  ,  // uint Rn             ,
                                     GetIntegerEncoding( regLhs  )  ,  // uint Rd             ,
                                     cpuALU                         ,  // uint Alu            ,
                                     false                          ,  // bool SetCC          ,
                                     GetIntegerEncoding( regRight ) ,  // uint Rm             ,
                                     cpuSHIFT                       ,  // uint ShiftType      ,
                                     GetIntegerEncoding( regShift ) ); // uint Rs             ,

                        EnqueueOpcode( enc );
                    }
                    else
                    {
                        uint valSeed;
                        uint valRot;

                        EncodeAs8BitImmediate( shift, false, out valSeed, out valRot );
                        if(valRot == 0)
                        {
                            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;
                            
                            enc.Prepare( m_pendingCondition             ,  // uint ConditionCodes ,
                                         GetIntegerEncoding( regLeft )  ,  // uint Rn             ,
                                         GetIntegerEncoding( regLhs  )  ,  // uint Rd             ,
                                         cpuALU                         ,  // uint Alu            ,
                                         false                          ,  // bool SetCC          ,
                                         GetIntegerEncoding( regRight ) ,  // uint Rm             ,
                                         cpuSHIFT                       ,  // uint ShiftType      ,
                                         valSeed                        ); // uint ShiftValue     ,

                            EnqueueOpcode( enc );
                        }
                        else
                        {
                            throw NotImplemented();
                        }
                    }
                }
                else
                {
                    throw NotImplemented();
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_ARM_LoadIndirectOperatorWithIndexUpdate( ARM.LoadIndirectOperatorWithIndexUpdate op )
        {
            var target = op.Type;
            var offset = op.Offset;

            CHECKS.ASSERT( Math.Abs( offset ) < this.ArmPlatform.GetOffsetLimit( target ), "Offset too big for encoding Store( {0}[{1}] )", target, offset );

            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstResult   );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.FirstArgument );

            CHECKS.ASSERT( regRd == GetRegisterDescriptor( op.FirstResult ), "FirstResult and FirstArgument should point to the same register: {0} != {1}", op.FirstResult, op.FirstArgument );

            if(regRd.InIntegerRegisterFile)
            {
                bool fPreIndex  = (op.PostUpdate == false);
                bool fWriteBack = (op.PostUpdate == false);

                switch(target.SizeOfHoldingVariable)
                {
                    case 1:
                        if(target.IsSigned)
                        {
                            EmitOpcode__LDRSB( regRd, regRs, offset, fPreIndex, fWriteBack );
                        }
                        else
                        {
                            EmitOpcode__LDRB( regRd, regRs, offset, fPreIndex, fWriteBack );
                        }
                        break;

                    case 2:
                        if(target.IsSigned)
                        {
                            EmitOpcode__LDRSH( regRd, regRs, offset, fPreIndex, fWriteBack );
                        }
                        else
                        {
                            EmitOpcode__LDRH( regRd, regRs, offset, fPreIndex, fWriteBack );
                        }
                        break;

                    case 4:
                        EmitOpcode__LDR( regRd, regRs, offset, fPreIndex, fWriteBack );
                        break;

                    default:
                        throw NotImplemented();
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_ARM_StoreIndirectOperatorWithIndexUpdate( ARM.StoreIndirectOperatorWithIndexUpdate op )
        {
            var target = op.Type;
            var offset = op.Offset;

            CHECKS.ASSERT( Math.Abs( offset ) < this.ArmPlatform.GetOffsetLimit( target ), "Offset too big for encoding Store( {0}[{1}] )", target, offset );

            ZeligIR.Abstractions.RegisterDescriptor regRd = GetRegisterDescriptor( op.FirstArgument  );
            ZeligIR.Abstractions.RegisterDescriptor regRs = GetRegisterDescriptor( op.SecondArgument );

            CHECKS.ASSERT( regRd == GetRegisterDescriptor( op.FirstResult ), "FirstResult and FirstArgument should point to the same register: {0} != {1}", op.FirstResult, op.FirstArgument );

            if(regRs.InIntegerRegisterFile)
            {
                bool fPreIndex  = (op.PostUpdate == false);
                bool fWriteBack = (op.PostUpdate == false);

                switch(target.SizeOfHoldingVariable)
                {
                    case 1:
                        EmitOpcode__STRB( regRd, offset, regRs, fPreIndex, fWriteBack );
                        break;

                    case 2:
                        EmitOpcode__STRH( regRd, offset, regRs, fPreIndex, fWriteBack );
                        break;

                    case 4:
                        EmitOpcode__STR( regRd, offset, regRs, fPreIndex, fWriteBack );
                        break;

                    default:
                        throw NotImplemented();
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_ARM_SetStatusRegisterOperator( ARM.SetStatusRegisterOperator op )
        {
            ZeligIR.Expression                      ex    = op.FirstArgument;
            ZeligIR.Abstractions.RegisterDescriptor regEx = GetRegisterDescriptor( ex );

            if(regEx != null)
            {
                EmitOpcode__MSR( op.UseSPSR, op.Fields, regEx );
            }
            else if(ex is ZeligIR.ConstantExpression)
            {
                uint valSeed;
                uint valRot;
                bool fInverted;

                if(CanEncodeAs8BitImmediate( NormalizeValue( ex ), out valSeed, out valRot, out fInverted ) && fInverted == false)
                {
                    EmitOpcode__MSR_CONST( op.UseSPSR, op.Fields, valSeed, valRot );
                }
                else
                {
                    Build32BitIntegerExpression( op, m_reg_Scratch, ex );

                    EmitOpcode__MSR( op.UseSPSR, op.Fields, m_reg_Scratch );
                }
            }
            else
            {
                throw NotImplemented();
            }
        }

        private void EmitCode_ARM_GetStatusRegisterOperator( ARM.GetStatusRegisterOperator op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regLhs = GetRegisterDescriptor( op.FirstResult );

            EmitOpcode__MRS( op.UseSPSR, regLhs );
        }

        //--//

        private void EmitCode_ARM_MoveToCoprocessor( ARM.MoveToCoprocessor op )
        {
            ZeligIR.Expression                      ex     = op.FirstArgument;
            ZeligIR.Abstractions.RegisterDescriptor regRhs = GetRegisterDescriptor( ex );

            if(regRhs != null)
            {
            }
            else if(ex is ZeligIR.ConstantExpression)
            {
                Build32BitIntegerExpression( op, m_reg_Scratch, ex );

                regRhs = m_reg_Scratch;
            }
            else
            {
                throw NotImplemented();
            }

            EmitOpcode__MCR( op.CpNum, op.Op1, regRhs, op.CRn, op.CRm, op.Op2 );
        }

        private void EmitCode_ARM_MoveFromCoprocessor( ARM.MoveFromCoprocessor op )
        {
            ZeligIR.Abstractions.RegisterDescriptor regLhs = GetRegisterDescriptor( op.FirstResult );

            EmitOpcode__MRC( op.CpNum, op.Op1, regLhs, op.CRn, op.CRm, op.Op2 );
        }

        //--//

        private void EmitCode_AddActivationRecordEventOperator( ZeligIR.AddActivationRecordEventOperator op )
        {
            var ev = op.ActivationRecordEvent;

            switch(ev)
            {
                case Runtime.ActivationRecordEvents.EnteringException:
                    m_headerFlags |= CodeMap.Flags.InterruptHandler;
                    break;

                case Runtime.ActivationRecordEvents.NonReachable:
                    m_fStopProcessingOperatorsForCurrentBasicBlock = true;
                    break;

                default:
                    AddNewImageAnnotation( ev );
                    break;
            }
        }

        //--//

        private void EmitCode_ARM_MoveStackPointerOperator( ARM.MoveStackPointerOperator op )
        {
            if(m_stackOffsetForSAVED > 0)
            {
                uint valSeed;
                uint valRot;
    
                if(CanEncodeAs8BitImmediate( (int)m_stackOffsetForSAVED, out valSeed, out valRot ))
                {
                    if(op.IsPush)
                    {
                        if(m_cfg.Method.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.StackNotAvailable ) == false)
                        {
                            EmitOpcode__SUB_CONST( m_reg_SP, m_reg_SP, valSeed, valRot );
                        }
                    }
                    else
                    {
                        if(m_cfg.Method.HasBuildTimeFlag( MethodRepresentation.BuildTimeAttributes.NoReturn ) == false)
                        {
                            EmitOpcode__ADD_CONST( m_reg_SP, m_reg_SP, valSeed, valRot );
                        }
                    }
                }
                else
                {
                    throw NotImplemented();
                }
    
                m_headerFlags |= CodeMap.Flags.HasStackAdjustment;
            }
        }

        private uint ComputeStackSize_ARM_MoveStackPointerOperator( ARM.MoveStackPointerOperator op )
        {
            if(op.IsPush)
            {
                return (m_stackForCalls + m_stackForLocals) * sizeof(uint);
            }

            return 0;
        }

        //--//

        private void EmitCode_ARM_MoveIntegerRegistersOperator( ARM.MoveIntegerRegistersOperator op )
        {
            uint mask;
            bool fFast;
            bool fSkip;

            PrepareCode_ARM_MoveIntegerRegistersOperator( op, out mask, out fFast, out fSkip );

            if(fSkip == false)
            {
                var Rn = GetRegisterDescriptor( op.FirstArgument );

                if(op.IsLoad)
                {
                    if(fFast)
                    {
                        EmitOpcode__MOV( m_reg_PC, m_reg_LR );
                    }
                    else
                    {
                        EmitOpcode__LDM( Rn, mask, false, true, op.ShouldWriteBackIndexRegister, op.ShouldRestoreSPSR );
                    }
                }
                else
                {
                    EmitOpcode__STM( Rn, mask, true, false, op.ShouldWriteBackIndexRegister );

                    m_headerFlags |= CodeMap.Flags.HasIntRegisterSave;
                }
            }
        }

        private uint ComputeStackSize_ARM_MoveIntegerRegistersOperator( ARM.MoveIntegerRegistersOperator op )
        {
            if(op.IsLoad)
            {
                return 0;
            }

            if(m_reg_SP != GetRegisterDescriptor( op.FirstArgument ))
            {
                return 0;
            }

            uint mask;
            bool fFast;
            bool fSkip;

            PrepareCode_ARM_MoveIntegerRegistersOperator( op, out mask, out fFast, out fSkip );

            if(fSkip)
            {
                return 0;
            }

            return BitVector.CountBits( mask ) * sizeof(uint);
        }

        private void PrepareCode_ARM_MoveIntegerRegistersOperator(     ARM.MoveIntegerRegistersOperator op    ,
                                                                   out uint                             mask  ,
                                                                   out bool                             fFast ,
                                                                   out bool                             fSkip )
        {
            var pa = this.ArmPlatform;

            mask  = op.RegisterMask;
            fFast = false;
            fSkip = false;
            
            if(op.ShouldAddComputedRegisters)
            {
                mask |= pa.Emit__GetIntegerList( this, m_registersToSave );
            }

            if(op.IsLoad)
            {
                if(mask == EncodingDefinition_ARM.c_register_lst_pc && pa.Emit__WasRegisterTouched( m_reg_LR, m_registersUsed ) == false)
                {
                    fFast = true;
                }
            }
            else
            {
                if(mask == EncodingDefinition_ARM.c_register_lst_lr && pa.Emit__WasRegisterTouched( m_reg_LR, m_registersUsed ) == false)
                {
                    fSkip = true;
                }
            }
        }

        //--//

        private void EmitCode_ARM_MoveFloatingPointRegistersOperator( ARM.MoveFloatingPointRegistersOperator op )
        {
            uint indexLow;
            uint indexHigh;
            bool fSkip;

            PrepareCode_ARM_MoveFloatingPointRegistersOperator( op, out indexLow, out indexHigh, out fSkip );

            if(fSkip == false)
            {
                var  pa    = this.ArmPlatform;
                var  Rn    = GetRegisterDescriptor( op.FirstArgument );
                var  Fd    = pa.GetRegisterForEncoding( EncodingDefinition_VFP_ARM.c_register_d0 + indexLow / 2 );
                uint count = indexHigh - indexLow + 2;

                if(op.IsLoad)
                {
                    EmitOpcode__FLDM( Rn, Fd, count, false, true, op.ShouldWriteBackIndexRegister );
                }
                else
                {
                    EmitOpcode__FSTM( Rn, Fd, count, true, false, op.ShouldWriteBackIndexRegister );

                    m_headerFlags |= CodeMap.Flags.HasFpRegisterSave;
                }
            }
        }

        private uint ComputeStackSize_ARM_MoveFloatingPointRegistersOperator( ARM.MoveFloatingPointRegistersOperator op )
        {
            if(op.IsLoad)
            {
                return 0;
            }

            if(m_reg_SP != GetRegisterDescriptor( op.FirstArgument ))
            {
                return 0;
            }

            uint indexLow;
            uint indexHigh;
            bool fSkip;

            PrepareCode_ARM_MoveFloatingPointRegistersOperator( op, out indexLow, out indexHigh, out fSkip );

            if(fSkip)
            {
                return 0;
            }

            return BitVector.CountBits( indexHigh - indexLow + 2 ) * sizeof(uint);
        }

        private void PrepareCode_ARM_MoveFloatingPointRegistersOperator(     ARM.MoveFloatingPointRegistersOperator op        ,
                                                                         out uint                                   indexLow  ,
                                                                         out uint                                   indexHigh ,
                                                                         out bool                                   fSkip     )
        {
            var pa = this.ArmPlatform;

            indexLow  = op.RegisterLow;
            indexHigh = op.RegisterHigh;
            
            if(op.ShouldAddComputedRegisters)
            {
                ZeligIR.Abstractions.RegisterDescriptor regLowest;
                ZeligIR.Abstractions.RegisterDescriptor regHighest;
    
                pa.Emit__GetFloatingPointList( this, m_registersToSave, out regLowest, out regHighest );
    
                if(regLowest != null)
                {
                    indexLow  = Math.Min( indexLow , GetDoublePrecisionEncoding( regLowest  ) );
                    indexHigh = Math.Max( indexHigh, GetDoublePrecisionEncoding( regHighest ) );
                }
            }

            fSkip = indexLow >= indexHigh;
        }


        //--//

        private void EmitCode_ARM_BreakpointOperator( ARM.BreakpointOperator op )
        {
            EmitOpcode__BKPT( op.Value );
        }

        //--//--//
        
        private void EmitCode_ARM_VectorHack_Initialize( ARM.VectorHack_Initialize op )
        {
            EmitOpcode__FMRX     ( m_reg_Scratch, m_reg_FPSCR                        );
            EmitOpcode__BIC_CONST( m_reg_Scratch, m_reg_Scratch, 7             << 16 );
            EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, (op.Size - 1) << 16 );
            EmitOpcode__FMXR     ( m_reg_Scratch, m_reg_FPSCR                        );
        }

        private void EmitCode_ARM_VectorHack_Prepare( ARM.VectorHack_Prepare op )
        {
            EmitOpcode__MOV_CONST( m_reg_Scratch, 0, 0 );

            var reg = op.ResultBankBase;

            for(int i = 0; i < op.Size; i++)
            {
                EmitOpcode__FMSR( m_reg_Scratch, reg );

                reg = GetNextRegister( reg );
            }
        }

        private void EmitCode_ARM_VectorHack_LoadData( ARM.VectorHack_LoadData op )
        {
            EmitOpcode__FLDM( GetRegisterDescriptor( op.FirstArgument ), op.DestinationBankBase, (uint)op.Size, false, true, true );
        }

        private void EmitCode_ARM_VectorHack_MultiplyAndAccumulate( ARM.VectorHack_MultiplyAndAccumulate op )
        {
            EmitOpcode__FMAC( op.ResultBankBase, op.LeftBankBase, op.RightBankBase );
        }

        private void EmitCode_ARM_VectorHack_Finalize( ARM.VectorHack_Finalize op )
        {
            var regSrc = op.ResultBankBase;
            var regRes = GetRegisterDescriptor( op.FirstResult );

            switch(op.Size)
            {
                case 1:
                    EmitOpcode__FCPY( regRes, regSrc );
                    break;

                default:
                    var regLeft = regSrc;

                    for(int i = 1; i < op.Size; i++)
                    {
                        regSrc = GetNextRegister( regSrc );

                        EmitOpcode__FADD( regRes, regLeft, regSrc );

                        regLeft = regRes;
                    }
                    break;
            }
        }

        private void EmitCode_ARM_VectorHack_Cleanup( ARM.VectorHack_Cleanup op )
        {
            EmitOpcode__FMRX     ( m_reg_Scratch, m_reg_FPSCR            );
            EmitOpcode__BIC_CONST( m_reg_Scratch, m_reg_Scratch, 7 << 16 );
            EmitOpcode__FMXR     ( m_reg_Scratch, m_reg_FPSCR            );
        }

        //--//--//

        private int GetOffsetOfStackLocation( ZeligIR.Expression ex )
        {
            ZeligIR.StackLocationExpression var = (ZeligIR.StackLocationExpression)ex;

            return (int)var.AllocationOffset;
        }

        private static ZeligIR.Abstractions.RegisterDescriptor GetRegisterDescriptor( ZeligIR.Expression ex )
        {
            return ZeligIR.Abstractions.RegisterDescriptor.ExtractFromExpression( ex );
        }

        public static uint GetIntegerEncoding( ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            CHECKS.ASSERT( reg.InIntegerRegisterFile == true, "Expecting an integer register, got {0}", reg );

            return reg.Encoding - EncodingDefinition_ARM.c_register_r0;
        }

        public static uint GetSystemEncoding( ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            CHECKS.ASSERT( reg.IsSystemRegister, "Expecting a system register, got {0}", reg );

            return reg.Encoding - EncodingDefinition_VFP_ARM.c_register_FPSID;
        }

        public static uint GetSinglePrecisionEncoding( ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            CHECKS.ASSERT( reg.InFloatingPointRegisterFile == true && reg.IsDoublePrecision == false, "Expecting a single-precision register, got {0}", reg );

            return reg.Encoding - EncodingDefinition_VFP_ARM.c_register_s0;
        }

        public static uint GetDoublePrecisionEncoding( ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            CHECKS.ASSERT( reg.InFloatingPointRegisterFile == true && reg.IsDoublePrecision == true, "Expecting a double-precision register, got {0}", reg );

            return (reg.Encoding - EncodingDefinition_VFP_ARM.c_register_d0) * 2;
        }
    }
}