//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;

    public partial class ArmCompilationState
    {
        private static EncodingDefinition_ARM s_Encoding = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();
        //
        // State
        //

        private uint m_pendingCondition;

        //
        // Helper Methods
        //

        private InstructionSet Encoder
        {
            get
            {
                return m_owner.GetInstructionSetProvider();
            }
        }

        private InstructionSet_VFP Encoder_VFP
        {
            get
            {
                return (InstructionSet_VFP)this.Encoder;
            }
        }

        internal static InstructionSet.Opcode GetOpcode( ZeligIR.ImageBuilders.CompilationState cs     ,
                                                         ZeligIR.ImageBuilders.SequentialRegion reg    ,
                                                         uint                                   offset )
        {
            ArmCompilationState cs2 = (ArmCompilationState)cs;

            return cs2.Encoder.Decode( reg.ReadUInt( offset ) );
        }

        internal static void SetOpcode( ZeligIR.ImageBuilders.CompilationState cs     ,
                                        ZeligIR.ImageBuilders.SequentialRegion reg    ,
                                        uint                                   offset ,
                                        InstructionSet.Opcode                  op     )
        {
            reg.Write( offset, op.Encode() );
        }

        //--//

        private void PrepareCondition( ZeligIR.ConditionCodeExpression.Comparison condIR )
        {
            Abstractions.ArmPlatform.Comparison condARM;

            switch(condIR)
            {
                case ZeligIR.ConditionCodeExpression.Comparison.Equal                   : condARM = ArmPlatform.Comparison.Equal                   ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.NotEqual                : condARM = ArmPlatform.Comparison.NotEqual                ; break;
                                                                                                                   
                case ZeligIR.ConditionCodeExpression.Comparison.Negative                : condARM = ArmPlatform.Comparison.Negative                ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.PositiveOrZero          : condARM = ArmPlatform.Comparison.PositiveOrZero          ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.Overflow                : condARM = ArmPlatform.Comparison.Overflow                ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.NoOverflow              : condARM = ArmPlatform.Comparison.NoOverflow              ; break;
                                                                                                                   
                case ZeligIR.ConditionCodeExpression.Comparison.UnsignedHigherThanOrSame: condARM = ArmPlatform.Comparison.UnsignedHigherThanOrSame; break;
                case ZeligIR.ConditionCodeExpression.Comparison.UnsignedLowerThan       : condARM = ArmPlatform.Comparison.UnsignedLowerThan       ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.UnsignedHigherThan      : condARM = ArmPlatform.Comparison.UnsignedHigherThan      ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.UnsignedLowerThanOrSame : condARM = ArmPlatform.Comparison.UnsignedLowerThanOrSame ; break;
                                                                                                                   
                case ZeligIR.ConditionCodeExpression.Comparison.SignedGreaterThanOrEqual: condARM = ArmPlatform.Comparison.SignedGreaterThanOrEqual; break;
                case ZeligIR.ConditionCodeExpression.Comparison.SignedLessThan          : condARM = ArmPlatform.Comparison.SignedLessThan          ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.SignedGreaterThan       : condARM = ArmPlatform.Comparison.SignedGreaterThan       ; break;
                case ZeligIR.ConditionCodeExpression.Comparison.SignedLessThanOrEqual   : condARM = ArmPlatform.Comparison.SignedLessThanOrEqual   ; break;

                default:
                    throw NotImplemented();
            }

            PrepareCondition( condARM );
        }

        private void PrepareCondition( Abstractions.ArmPlatform.Comparison condARM )
        {
            m_pendingCondition = (uint)condARM;
        }

        private void EnqueueOpcode( InstructionSet.Opcode enc )
        {
            m_pendingCondition = EncodingDefinition_ARM.c_cond_AL;

            FlushOperatorContext();

            m_activeCodeSection.Write( enc.Encode() );
        }

        //--//

        private void SetConditionBit()
        {
            uint                                   offset  = m_activeCodeSection.Offset - sizeof(uint);
            ZeligIR.ImageBuilders.SequentialRegion context = m_activeCodeSection.Context;
            InstructionSet                         encoder = this.Encoder;

            InstructionSet.Opcode op = encoder.Decode( context.ReadUInt( offset ) );

            if(op is InstructionSet.Opcode_DataProcessing)
            {
                InstructionSet.Opcode_DataProcessing op2 = (InstructionSet.Opcode_DataProcessing)op;

                op2.SetCC = true;
            }
            else if(op is InstructionSet.Opcode_Multiply)
            {
                InstructionSet.Opcode_Multiply op2 = (InstructionSet.Opcode_Multiply)op;

                op2.SetCC = true;
            }
            else if(op is InstructionSet.Opcode_MultiplyLong)
            {
                InstructionSet.Opcode_MultiplyLong op2 = (InstructionSet.Opcode_MultiplyLong)op;

                op2.SetCC = true;
            }

            context.Write( offset, op.Encode() );
        }

        private void Build32BitIntegerExpression( ZeligIR.Operator                        opSource ,
                                                  ZeligIR.Abstractions.RegisterDescriptor reg      ,
                                                  object                                  obj      )
        {
            CHECKS.ASSERT( reg.InIntegerRegisterFile, "Expecting an integer register, got {0}", reg );

            int val;

            if(GetValue( obj, out val ))
            {
                uint valSeed;
                uint valRot;
                bool fInverted;

                if(CanEncodeAs8BitImmediate( obj, out valSeed, out valRot, out fInverted ))
                {
                    if(fInverted)
                    {
                        EmitOpcode__MVN_CONST( reg, valSeed, valRot );
                    }
                    else
                    {
                        EmitOpcode__MOV_CONST( reg, valSeed, valRot );
                    }

                    return;
                }
            }

            //--//

            obj = NormalizeValue( obj );

            var level = GetEncodingLevelForConstant( opSource );

            switch(level)
            {
                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.Immediate:
                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.SmallLoad:
                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.NearRelativeLoad:
                    CreateRelocation_LDR( opSource, obj, EncodingDefinition_ARM.c_PC_offset );
                    EmitOpcode__LDR( reg, m_reg_PC, 0 );
                    break;

                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad16Bit:
                    CreateRelocation_MOV( opSource, obj, EncodingDefinition_ARM.c_PC_offset, 2 );

                    EmitOpcode__MOV_CONST( m_reg_Scratch,                0, 0 );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0 );

                    EmitOpcode__LDRwithRotate( reg, m_reg_PC, m_reg_Scratch, 0 );
                    break;

                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad24Bit:
                    CreateRelocation_MOV( opSource, obj, EncodingDefinition_ARM.c_PC_offset, 3 );

                    EmitOpcode__MOV_CONST( m_reg_Scratch,                0, 0 );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0 );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0 );

                    EmitOpcode__LDRwithRotate( reg, m_reg_PC, m_reg_Scratch, 0 );
                    break;

                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad32Bit:
                    CreateRelocation_MOV( opSource, obj, EncodingDefinition_ARM.c_PC_offset, 4 );

                    EmitOpcode__MOV_CONST( m_reg_Scratch,                0, 0 );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0 );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0 );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0 );

                    EmitOpcode__LDRwithRotate( reg, m_reg_PC, m_reg_Scratch, 0 );
                    break;

                default:
                    throw NotImplemented();
            }
        }

        private void Build32BitFloatingPointExpression( ZeligIR.Operator                        opSource ,
                                                        ZeligIR.Abstractions.RegisterDescriptor reg      ,
                                                        object                                  obj      )
        {
            CHECKS.ASSERT( reg.InFloatingPointRegisterFile && !reg.IsDoublePrecision, "Expecting a single-precision register, got {0}", reg );

            BuildFloatingPointExpression( opSource, reg, obj );
        }

        private void Build64BitFloatingPointExpression( ZeligIR.Operator                        opSource ,
                                                        ZeligIR.Abstractions.RegisterDescriptor reg      ,
                                                        object                                  obj      )
        {
            CHECKS.ASSERT( reg.InFloatingPointRegisterFile && reg.IsDoublePrecision, "Expecting a double-precision register, got {0}", reg );

            BuildFloatingPointExpression( opSource, reg, obj );
        }

        private void BuildFloatingPointExpression( ZeligIR.Operator                        opSource ,
                                                   ZeligIR.Abstractions.RegisterDescriptor reg      ,
                                                   object                                  obj      )
        {
            CHECKS.ASSERT( reg.InFloatingPointRegisterFile, "Expecting a floating-point, got {0}", reg );

            obj = NormalizeValue( obj );

            var level = GetEncodingLevelForConstant( opSource );

            switch(level)
            {
                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.Immediate:
                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.SmallLoad:
                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.NearRelativeLoad:
                    CreateRelocation_FLD( opSource, obj, EncodingDefinition_ARM.c_PC_offset );
                    EmitOpcode__FLD( reg, m_reg_PC, 0 );
                    break;

                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad16Bit:
                    CreateRelocation_MOV( opSource, obj, EncodingDefinition_ARM.c_PC_offset, 2 );

                    EmitOpcode__MOV_CONST( m_reg_Scratch,                0, 0     );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0     );
                    EmitOpcode__ADD      ( m_reg_Scratch, m_reg_Scratch, m_reg_PC );
                    EmitOpcode__FLD      ( reg          , m_reg_Scratch, 0        );
                    break;

                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad24Bit:
                    CreateRelocation_MOV( opSource, obj, EncodingDefinition_ARM.c_PC_offset, 3 );

                    EmitOpcode__MOV_CONST( m_reg_Scratch,                0, 0     );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0     );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0     );
                    EmitOpcode__ADD      ( m_reg_Scratch, m_reg_Scratch, m_reg_PC );
                    EmitOpcode__FLD      ( reg          , m_reg_Scratch, 0        );
                    break;

                case ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad32Bit:
                    CreateRelocation_MOV( opSource, obj, EncodingDefinition_ARM.c_PC_offset, 4 );

                    EmitOpcode__MOV_CONST( m_reg_Scratch,                0, 0     );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0     );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0     );
                    EmitOpcode__OR_CONST ( m_reg_Scratch, m_reg_Scratch, 0, 0     );
                    EmitOpcode__ADD      ( m_reg_Scratch, m_reg_Scratch, m_reg_PC );
                    EmitOpcode__FLD      ( reg          , m_reg_Scratch, 0        );
                    break;

                default:
                    throw NotImplemented();
            }
        }

        private static bool CanEncodeAs8BitImmediate(     object o         ,
                                                      out uint   valSeed   ,
                                                      out uint   valRot    ,
                                                      out bool   fInverted )
        {
            valSeed   = 0;
            valRot    = 0;
            fInverted = false;

            int val;

            if(GetValue( o, out val ) == false)
            {
                return false;
            }

            if(CanEncodeAs8BitImmediate( val, out valSeed, out valRot ) == true)
            {
                return true;
            }

            if(CanEncodeAs8BitImmediate( ~val, out valSeed, out valRot ) == true)
            {
                fInverted = true;
                return true;
            }

            return false;
        }

        private static bool CanEncodeAs8BitImmediate(     int  val     ,
                                                      out uint valSeed ,
                                                      out uint valRot  )
        {
            return s_Encoding.check_DataProcessing_ImmediateValue( (uint)val, out valSeed, out valRot );
        }

        //--//

        private static void EncodeAs8BitImmediate(     object o       ,
                                                   out uint   valSeed ,
                                                   out uint   valRot  )
        {
            EncodeAs8BitImmediate( o, false, out valSeed, out valRot );
        }

        private static bool EncodeAs8BitImmediate(     object o                   ,
                                                       bool   fCanUseInvertedForm ,
                                                   out uint   valSeed             ,
                                                   out uint   valRot              )
        {
            valSeed = 0;
            valRot  = 0;

            int val;

            if(GetValue( o, out val ))
            {
                if(CanEncodeAs8BitImmediate( val, out valSeed, out valRot ))
                {
                    return false;
                }

                if(fCanUseInvertedForm && CanEncodeAs8BitImmediate( ~val, out valSeed, out valRot ))
                {
                    return true;
                }
            }

            throw TypeConsistencyErrorException.Create( "Cannot encode as 8-bit immediate: {0}", o );
        }

        //--//

        public void EmitOpcode__BR( int offset )
        {
            InstructionSet.Opcode_Branch enc = this.Encoder.PrepareForBranch;

            enc.Prepare( m_pendingCondition,  // uint ConditionCodes ,
                         offset            ,  // int  Offset         ,
                         false             ); // bool IsLink         );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__BL( int offset )
        {
            InstructionSet.Opcode_Branch enc = this.Encoder.PrepareForBranch;

            enc.Prepare( m_pendingCondition,  // uint ConditionCodes ,
                         offset            ,  // int  Offset         ,
                         true              ); // bool IsLink         );

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__Store( ZeligIR.Abstractions.RegisterDescriptor Rd     ,
                                       int                                     offset ,
                                       ZeligIR.Abstractions.RegisterDescriptor Rs     ,
                                       TypeRepresentation                      target )
        {
            CHECKS.ASSERT( Math.Abs( offset ) < this.ArmPlatform.GetOffsetLimit( target ), "Offset too big for encoding Store( {0}[{1}] )", target, offset );

            if(Rs.InFloatingPointRegisterFile)
            {
                EmitOpcode__FST( Rs, Rd, offset );
            }
            else
            {
                switch(target.SizeOfHoldingVariable)
                {
                    case 1:
                        EmitOpcode__STRB( Rd, offset, Rs, true, false );
                        break;

                    case 2:
                        EmitOpcode__STRH( Rd, offset, Rs, true, false );
                        break;

                    case 4:
                        EmitOpcode__STR( Rd, offset, Rs, true, false );
                        break;

                    default:
                        throw NotImplemented();
                }
            }
        }

        public void EmitOpcode__STR( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                     int                                     offset    ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                     bool                                    preIndex  ,
                                     bool                                    writeBack )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;
            bool                                       fUp;
            uint                                       uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ),  // uint Rn             ,
                         false                   ,  // bool IsLoad         ,
                         preIndex                ,  // bool PreIndex       ,
                         fUp                     ,  // bool Up             ,
                         writeBack               ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rs ),  // uint Rd             ,
                         false                   ,  // bool IsByte         ,
                         uOffset                 ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__STRB( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                      int                                     offset    ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                      bool                                    preIndex  ,
                                      bool                                    writeBack )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;
            bool                                       fUp;
            uint                                       uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ),  // uint Rn             ,
                         false                   ,  // bool IsLoad         ,
                         preIndex                ,  // bool PreIndex       ,
                         fUp                     ,  // bool Up             ,
                         writeBack               ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rs ),  // uint Rd             ,
                         true                    ,  // bool IsByte         ,
                         uOffset                 ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__STRH( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                      int                                     offset    ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                      bool                                    preIndex  ,
                                      bool                                    writeBack )
        {
            InstructionSet.Opcode_HalfwordDataTransfer_2 enc = this.Encoder.PrepareForHalfwordDataTransfer_2;
            bool                                         fUp;
            uint                                         uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition                  ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rd )            ,  // uint Rn             ,
                         false                               ,  // bool IsLoad         ,
                         preIndex                            ,  // bool PreIndex       ,
                         fUp                                 ,  // bool Up             ,
                         writeBack                           ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rs )            ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_halfwordkind_U2,  // uint Kind           ,
                         uOffset                             ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__Push( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ),  // uint Rn             ,
                         false                   ,  // bool IsLoad         ,
                         true                    ,  // bool PreIndex       ,
                         false                   ,  // bool Up             ,
                         true                    ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rs ),  // uint Rd             ,
                         false                   ,  // bool IsByte         ,
                         sizeof(uint)            ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__Pop( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ),  // uint Rn             ,
                         true                    ,  // bool IsLoad         ,
                         false                   ,  // bool PreIndex       ,
                         true                    ,  // bool Up             ,
                         true                    ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rs ),  // uint Rd             ,
                         false                   ,  // bool IsByte         ,
                         sizeof(uint)            ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__Load( ZeligIR.Abstractions.RegisterDescriptor Rd     ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs     ,
                                      int                                     offset ,
                                      TypeRepresentation                      target )
        {
            CHECKS.ASSERT( Math.Abs( offset ) < this.ArmPlatform.GetOffsetLimit( target ), "Offset too big for encoding Load( {0}[{1}] )", target, offset );

            if(Rd.InFloatingPointRegisterFile)
            {
                EmitOpcode__FLD( Rd, Rs, offset );
            }
            else
            {
                switch(target.SizeOfHoldingVariable)
                {
                    case 1:
                        if(target.IsSigned)
                        {
                            EmitOpcode__LDRSB( Rd, Rs, offset, true, false );
                        }
                        else
                        {
                            EmitOpcode__LDRB( Rd, Rs, offset, true, false );
                        }
                        break;

                    case 2:
                        if(target.IsSigned)
                        {
                            EmitOpcode__LDRSH( Rd, Rs, offset, true, false );
                        }
                        else
                        {
                            EmitOpcode__LDRH( Rd, Rs, offset, true, false );
                        }
                        break;

                    case 4:
                        EmitOpcode__LDR( Rd, Rs, offset );
                        break;

                    default:
                        throw NotImplemented();
                }
            }
        }

        public void EmitOpcode__LDRwithRotate( ZeligIR.Abstractions.RegisterDescriptor Rd    ,
                                               ZeligIR.Abstractions.RegisterDescriptor Rs    ,
                                               ZeligIR.Abstractions.RegisterDescriptor Ridx  ,
                                               uint                                    shift )
        {
            InstructionSet.Opcode_SingleDataTransfer_2 enc = this.Encoder.PrepareForSingleDataTransfer_2;

            enc.Prepare( m_pendingCondition            ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs )      ,  // uint Rn             ,
                         true                          ,  // bool IsLoad         ,
                         true                          ,  // bool PreIndex       ,
                         true                          ,  // bool Up             ,
                         false                         ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rd   )    ,  // uint Rd             ,
                         false                         ,  // bool IsByte         ,
                         GetIntegerEncoding( Ridx )    ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL,  // uint ShiftType      ,
                         shift                         ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }


        public void EmitOpcode__PLD( ZeligIR.Abstractions.RegisterDescriptor Rs     ,
                                     int                                     offset )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;
            bool                                       fUp;
            uint                                       uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( EncodingDefinition_ARM.c_cond_UNUSED,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs ),  // uint Rn             ,
                         true                    ,  // bool IsLoad         ,
                         true                    ,  // bool PreIndex       ,
                         fUp                     ,  // bool Up             ,
                         false                   ,  // bool WriteBack      ,
                         0xF                     ,  // uint Rd             ,
                         true                    ,  // bool IsByte         ,
                         uOffset                 ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LDR( ZeligIR.Abstractions.RegisterDescriptor Rd     ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs     ,
                                     int                                     offset )
        {
            EmitOpcode__LDR( Rd, Rs, offset, true, false );
        }

        public void EmitOpcode__LDR( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                     int                                     offset    ,
                                     bool                                    preIndex  ,
                                     bool                                    writeBack )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;
            bool                                       fUp;
            uint                                       uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs ),  // uint Rn             ,
                         true                    ,  // bool IsLoad         ,
                         preIndex                ,  // bool PreIndex       ,
                         fUp                     ,  // bool Up             ,
                         writeBack               ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rd ),  // uint Rd             ,
                         false                   ,  // bool IsByte         ,
                         uOffset                 ); // uint Offset         ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LDRSH( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                       ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                       int                                     offset    ,
                                       bool                                    preIndex  ,
                                       bool                                    writeBack )
        {
            InstructionSet.Opcode_HalfwordDataTransfer_2 enc = this.Encoder.PrepareForHalfwordDataTransfer_2;
            bool                                         fUp;
            uint                                         uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition                  ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs )            ,  // uint Rn             ,
                         true                                ,  // bool IsLoad         ,
                         preIndex                            ,  // bool PreIndex       ,
                         fUp                                 ,  // bool Up             ,
                         writeBack                           ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rd )            ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_halfwordkind_I2,  // uint Kind           ,
                         uOffset                             ); // uint Offset         

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LDRH( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                      int                                     offset    ,
                                      bool                                    preIndex  ,
                                      bool                                    writeBack )
        {
            InstructionSet.Opcode_HalfwordDataTransfer_2 enc = this.Encoder.PrepareForHalfwordDataTransfer_2;
            bool                                         fUp;
            uint                                         uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition                  ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs )            ,  // uint Rn             ,
                         true                                ,  // bool IsLoad         ,
                         preIndex                            ,  // bool PreIndex       ,
                         fUp                                 ,  // bool Up             ,
                         writeBack                           ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rd )            ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_halfwordkind_U2,  // uint Kind           ,
                         uOffset                             ); // uint Offset         

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LDRSB( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                       ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                       int                                     offset    ,
                                       bool                                    preIndex  ,
                                       bool                                    writeBack )
        {
            InstructionSet.Opcode_HalfwordDataTransfer_2 enc = this.Encoder.PrepareForHalfwordDataTransfer_2;
            bool                                         fUp;
            uint                                         uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition                  ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs )            ,  // uint Rn             ,
                         true                                ,  // bool IsLoad         ,
                         preIndex                            ,  // bool PreIndex       ,
                         fUp                                 ,  // bool Up             ,
                         writeBack                           ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rd )            ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_halfwordkind_I1,  // uint Kind           ,
                         uOffset                             ); // uint Offset         

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LDRB( ZeligIR.Abstractions.RegisterDescriptor Rd        ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                      int                                     offset    ,
                                      bool                                    preIndex  ,
                                      bool                                    writeBack )
        {
            InstructionSet.Opcode_SingleDataTransfer_1 enc = this.Encoder.PrepareForSingleDataTransfer_1;
            bool                                       fUp;
            uint                                       uOffset;

            if(offset < 0)
            {
                fUp     = false;
                uOffset = (uint)-offset;
            }
            else
            {
                fUp     = true;
                uOffset = (uint)offset;
            }

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rs ),  // uint Rn             ,
                         true                    ,  // bool IsLoad         ,
                         preIndex                ,  // bool PreIndex       ,
                         fUp                     ,  // bool Up             ,
                         writeBack               ,  // bool WriteBack      ,
                         GetIntegerEncoding( Rd ),  // uint Rd             ,
                         true                    ,  // bool IsByte         ,
                         uOffset                 ); // uint Offset         ,

            EnqueueOpcode( enc );
        }


        //--//

        public void EmitOpcode__MOV_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MVN_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_MVN,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__ADD_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_ADD,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__ADC_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_ADC,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__SUB_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_SUB,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__SBC_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint valSeed ,
                                           uint valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_SBC,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__RSB_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_RSB,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__RSC_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_RSC,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__CMP_CONST( ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         0                                 ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_CMP,  // uint Alu               ,
                         true                              ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__TST_CONST( ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         0                                 ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_TST,  // uint Alu               ,
                         true                              ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__AND_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs  ,
                                           int                                     val )
        {
            uint valSeed;
            uint valRot;

            EncodeAs8BitImmediate( val, out valSeed, out valRot );

            EmitOpcode__AND_CONST( Rd, Rs, valSeed, valRot );
        }

        public void EmitOpcode__AND_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_AND,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__BIC_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs  ,
                                           int                                     val )
        {
            uint valSeed;
            uint valRot;

            EncodeAs8BitImmediate( val, out valSeed, out valRot );

            EmitOpcode__BIC_CONST( Rd, Rs, valSeed, valRot );
        }

        public void EmitOpcode__BIC_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_BIC,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__OR_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                          ZeligIR.Abstractions.RegisterDescriptor Rs  ,
                                          int                                     val )
        {
            uint valSeed;
            uint valRot;

            EncodeAs8BitImmediate( val, out valSeed, out valRot );

            EmitOpcode__OR_CONST( Rd, Rs, valSeed, valRot );
        }

        public void EmitOpcode__OR_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                          ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                          uint                                    valSeed ,
                                          uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_ORR,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__EOR_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd  ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs  ,
                                           int                                     val )
        {
            uint valSeed;
            uint valRot;

            EncodeAs8BitImmediate( val, out valSeed, out valRot );

            EmitOpcode__EOR_CONST( Rd, Rs, valSeed, valRot );
        }

        public void EmitOpcode__EOR_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd      ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs      ,
                                           uint                                    valSeed ,
                                           uint                                    valRot  )
        {
            InstructionSet.Opcode_DataProcessing_1 enc = this.Encoder.PrepareForDataProcessing_1;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes    ,
                         GetIntegerEncoding( Rs )          ,  // uint Rn                ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd                ,
                         EncodingDefinition_ARM.c_operation_EOR,  // uint Alu               ,
                         false                             ,  // bool SetCC             ,
                         valSeed                           ,  // uint ImmediateSeed     ,
                         valRot                            ); // uint ImmediateRotation ,

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__MOV_IfDifferent( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                                 ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            if(Rd != Rs)
            {
                EmitOpcode__MOV( Rd, Rs );
            }
        }

        public void EmitOpcode__MOV( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rs )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MVN( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MVN,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rs )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__ADD( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_ADD,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__ADC( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_ADC,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__SUB( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_SUB,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__SBC( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_SBC,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__RSC( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_RSC,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__CMP( ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         0                                 ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_CMP,  // uint Alu            ,
                         true                              ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__TST( ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         0                                 ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_TST,  // uint Alu            ,
                         true                              ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__AND( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_AND,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__OR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                    ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                    ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_ORR,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__EOR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn )          ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_EOR,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         0                                 ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LSL( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_DataProcessing_3 enc = this.Encoder.PrepareForDataProcessing_3;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         0                                 ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         GetIntegerEncoding( Rs )          ); // uint Rs             ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LSR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_DataProcessing_3 enc = this.Encoder.PrepareForDataProcessing_3;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         0                                 ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSR    ,  // uint ShiftType      ,
                         GetIntegerEncoding( Rs )          ); // uint Rs             ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__ASR( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs )
        {
            InstructionSet.Opcode_DataProcessing_3 enc = this.Encoder.PrepareForDataProcessing_3;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         0                                 ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rm )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_ASR    ,  // uint ShiftType      ,
                         GetIntegerEncoding( Rs )          ); // uint Rs             ,

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__LSL_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd    ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs    ,
                                           uint                                    shift )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rs )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSL    ,  // uint ShiftType      ,
                         shift                             ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__LSR_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd    ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs    ,
                                           uint                                    shift )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rs )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_LSR    ,  // uint ShiftType      ,
                         shift                             ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__ASR_CONST( ZeligIR.Abstractions.RegisterDescriptor Rd    ,
                                           ZeligIR.Abstractions.RegisterDescriptor Rs    ,
                                           uint                                    shift )
        {
            InstructionSet.Opcode_DataProcessing_2 enc = this.Encoder.PrepareForDataProcessing_2;

            enc.Prepare( m_pendingCondition                ,  // uint ConditionCodes ,
                         EncodingDefinition_ARM.c_register_r0  ,  // uint Rn             ,
                         GetIntegerEncoding( Rd )          ,  // uint Rd             ,
                         EncodingDefinition_ARM.c_operation_MOV,  // uint Alu            ,
                         false                             ,  // bool SetCC          ,
                         GetIntegerEncoding( Rs )          ,  // uint Rm             ,
                         EncodingDefinition_ARM.c_shift_ASR    ,  // uint ShiftType      ,
                         shift                             ); // uint ShiftValue     ,

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__MULL( ZeligIR.Abstractions.RegisterDescriptor RdHi      ,
                                      ZeligIR.Abstractions.RegisterDescriptor RdLo      ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rs        ,
                                      ZeligIR.Abstractions.RegisterDescriptor Rm        ,
                                      bool                                    fIsSigned )
        {
            InstructionSet.Opcode_MultiplyLong enc = this.Encoder.PrepareForMultiplyLong;

            enc.Prepare( m_pendingCondition        ,  // uint ConditionCodes ,
                         GetIntegerEncoding( RdHi ),  // uint RdHi           ,
                         GetIntegerEncoding( RdLo ),  // uint RdLo           ,
                         GetIntegerEncoding( Rs   ),  // uint Rs             ,
                         GetIntegerEncoding( Rm   ),  // uint Rm             ,
                         false                     ,  // bool SetCC          ,
                         false                     ,  // bool IsAccumulate   ,
                         fIsSigned                 ); // bool IsSigned       )

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__MUL( ZeligIR.Abstractions.RegisterDescriptor Rd ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rs ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm )
        {
            InstructionSet.Opcode_Multiply enc = this.Encoder.PrepareForMultiply;

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rd ),  // uint Rd             ,
                         0                       ,  // uint Rn             ,
                         GetIntegerEncoding( Rs ),  // uint Rs             ,
                         GetIntegerEncoding( Rm ),  // uint Rm             ,
                         false                   ,  // bool SetCC          ,
                         false                   ); // bool IsAccumulate   

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__STMFD( ZeligIR.Abstractions.RegisterDescriptor Rn  ,
                                       uint                                    lst )
        {
            EmitOpcode__STM( Rn, lst, true, false, true );
        }

        public void EmitOpcode__STM( ZeligIR.Abstractions.RegisterDescriptor Rn        ,
                                     uint                                    lst       ,
                                     bool                                    PreIndex  ,
                                     bool                                    Up        ,
                                     bool                                    WriteBack )
        {
            InstructionSet.Opcode_BlockDataTransfer enc = this.Encoder.PrepareForBlockDataTransfer;

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn ),  // uint Rn             ,
                         false                   ,  // bool IsLoad         ,
                         PreIndex                ,  // bool PreIndex       ,
                         Up                      ,  // bool Up             ,
                         WriteBack               ,  // bool WriteBack      ,
                         lst                     ,  // uint Lst            ,
                         false                   ); // bool LoadPSR        );

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__LDMFD( ZeligIR.Abstractions.RegisterDescriptor Rn      ,
                                       uint                                    lst     ,
                                       bool                                    LoadPSR )
        {
            EmitOpcode__LDM( Rn, lst, false, true, true, LoadPSR );
        }

        public void EmitOpcode__LDM( ZeligIR.Abstractions.RegisterDescriptor Rn        ,
                                     uint                                    lst       ,
                                     bool                                    PreIndex  ,
                                     bool                                    Up        ,
                                     bool                                    WriteBack ,
                                     bool                                    LoadPSR   )
        {
            InstructionSet.Opcode_BlockDataTransfer enc = this.Encoder.PrepareForBlockDataTransfer;

            enc.Prepare( m_pendingCondition      ,  // uint ConditionCodes ,
                         GetIntegerEncoding( Rn ),  // uint Rn             ,
                         true                    ,  // bool IsLoad         ,
                         PreIndex                ,  // bool PreIndex       ,
                         Up                      ,  // bool Up             ,
                         WriteBack               ,  // bool WriteBack      ,
                         lst                     ,  // uint Lst            ,
                         LoadPSR                 ); // bool LoadPSR        );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__SWI( uint val )
        {
            InstructionSet.Opcode_SoftwareInterrupt enc = this.Encoder.PrepareForSoftwareInterrupt;

            enc.Prepare( m_pendingCondition,  // uint ConditionCodes ,
                         val               ); // uint Value          );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MRS( bool                                    UseSPSR ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rd      )
        {
            InstructionSet.Opcode_MRS enc = this.Encoder.PrepareForMRS;

            enc.Prepare( m_pendingCondition       ,  // uint ConditionCodes ,
                         UseSPSR                  ,  // bool UseSPSR        ,
                         GetIntegerEncoding( Rd ) ); // uint Rd             );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MSR( bool                                    UseSPSR ,
                                     uint                                    Fields  ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rm      )
        {
            InstructionSet.Opcode_MSR_1 enc = this.Encoder.PrepareForMSR_1;

            enc.Prepare( m_pendingCondition       ,  // uint ConditionCodes ,
                         UseSPSR                  ,  // bool UseSPSR        ,
                         Fields                   ,  // uint Fields         ,
                         GetIntegerEncoding( Rm ) ); // uint Rm             );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MSR_CONST( bool UseSPSR ,
                                           uint Fields  ,
                                           uint valSeed ,
                                           uint valRot  )
        {
            InstructionSet.Opcode_MSR_2 enc = this.Encoder.PrepareForMSR_2;

            enc.Prepare( m_pendingCondition,  // uint ConditionCodes    ,
                         UseSPSR           ,  // bool UseSPSR           ,
                         Fields            ,  // uint Fields            ,
                         valSeed           ,  // uint ImmediateSeed     ,
                         valRot            ); // uint ImmediateRotation );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MCR( uint                                    CpNum ,
                                     uint                                    Op1   ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rn    ,
                                     uint                                    CRn   ,
                                     uint                                    CRm   ,
                                     uint                                    Op2   )
        {
            InstructionSet.Opcode_CoprocRegisterTransfer enc = this.Encoder.PrepareForCoprocRegisterTransfer;

            enc.Prepare( m_pendingCondition       ,  // uint ConditionCodes ,
                         false                    ,  // bool IsMRC          ,
                         Op1                      ,  // uint Op1            ,
                         Op2                      ,  // uint Op2            ,
                         CpNum                    ,  // uint CpNum          ,
                         CRn                      ,  // uint CRn            ,
                         CRm                      ,  // uint CRm            ,
                         GetIntegerEncoding( Rn ) ); // uint Rd             );

            EnqueueOpcode( enc );
        }

        public void EmitOpcode__MRC( uint                                    CpNum ,
                                     uint                                    Op1   ,
                                     ZeligIR.Abstractions.RegisterDescriptor Rd    ,
                                     uint                                    CRn   ,
                                     uint                                    CRm   ,
                                     uint                                    Op2   )
        {
            InstructionSet.Opcode_CoprocRegisterTransfer enc = this.Encoder.PrepareForCoprocRegisterTransfer;

            enc.Prepare( m_pendingCondition       ,  // uint ConditionCodes ,
                         true                     ,  // bool IsMRC          ,
                         Op1                      ,  // uint Op1            ,
                         Op2                      ,  // uint Op2            ,
                         CpNum                    ,  // uint CpNum          ,
                         CRn                      ,  // uint CRn            ,
                         CRm                      ,  // uint CRm            ,
                         GetIntegerEncoding( Rd ) ); // uint Rd             );

            EnqueueOpcode( enc );
        }

        //--//

        public void EmitOpcode__BKPT( uint val )
        {
            InstructionSet.Opcode_Breakpoint enc = this.Encoder.PrepareForBreakpoint;

            enc.Prepare( m_pendingCondition,  // uint ConditionCodes ,
                         val               ); // uint Value          );

            EnqueueOpcode( enc );
        }


        //--//

        //
        // Debug Methods
        //

        protected override void DumpOpcodes( System.IO.TextWriter textWriter )
        {
            InstructionSet encoder = this.Encoder;

            textWriter.WriteLine( "{0}", m_cfg );

            foreach(ZeligIR.ImageBuilders.SequentialRegion reg in m_associatedRegions)
            {
                reg.Dump( textWriter );
            }

            textWriter.WriteLine();
            textWriter.WriteLine();
        }
    }
}