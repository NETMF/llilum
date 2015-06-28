namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    internal sealed class ArmCodeRelocation_MOV : ZeligIR.ImageBuilders.CodeRelocation
    {
        private static EncodingDefinition_ARM s_Encoding = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();

        //
        // State
        //

        int m_numOpcodes;

        //
        // Constructor Methods
        //

        private ArmCodeRelocation_MOV() // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmCodeRelocation_MOV( ZeligIR.ImageBuilders.SequentialRegion region     ,
                                        uint                                   offset     ,
                                        ZeligIR.ImageBuilders.CodeConstant     target     ,
                                        ZeligIR.ImageBuilders.CompilationState cs         ,
                                        ZeligIR.Operator                       origin     ,
                                        int                                    skew       ,
                                        int                                    numOpcodes ) : base( region, offset, sizeof(uint), target, cs, origin, skew )
        {
            m_numOpcodes = Math.Min( numOpcodes, 4 );
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( ZeligIR.TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_numOpcodes );

            context.Pop();
        }

        //--//

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            int range;

            switch(m_numOpcodes)
            {
                case 1 : range = 0x000000FF; break;
                case 2 : range = 0x0000FFFF; break;
                case 3 : range = 0x00FFFFFF; break;
                default: range = 0x3FFFFFFF; break;
            }

            lowerBound = this.EffectiveSkew - range;
            upperBound = this.EffectiveSkew + range;
        }

        protected override bool ApplyRelocation( uint srcAddress ,
                                                 uint dstAddress )
        {
            int  relPos = ((int)dstAddress - (int)srcAddress) - this.EffectiveSkew;
            uint val;
            uint op0;
            uint op1;

            if(relPos <= 0)
            {
                val = ~(uint)relPos;
                op0 = EncodingDefinition_ARM.c_operation_MVN;
                op1 = EncodingDefinition_ARM.c_operation_BIC;
            }
            else
            {
                val = (uint)relPos;
                op0 = EncodingDefinition_ARM.c_operation_MOV;
                op1 = EncodingDefinition_ARM.c_operation_ORR;
            }

            for(uint pos = 0; pos < m_numOpcodes; pos++)
            {
                InstructionSet.Opcode_DataProcessing_1 op = (InstructionSet.Opcode_DataProcessing_1)ArmCompilationState.GetOpcode( m_cs, m_region, m_offset + pos * sizeof(uint) );
                uint                                   immRes;
                uint                                   rotRes;

                s_Encoding.check_DataProcessing_ImmediateValue( val & (0xFFu << (8 * (int)pos)), out immRes, out rotRes );

                op.Alu               = pos == 0 ? op0 : op1;
                op.ImmediateSeed     = immRes;
                op.ImmediateRotation = rotRes;

                ArmCompilationState.SetOpcode( m_cs, m_region, m_offset + pos * sizeof(uint), op );
            }

            return true;
        }
        
        protected override void NotifyFailedRelocation()
        {
            ArmCompilationState                                     cs = (ArmCompilationState)m_cs;
            ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel level;

            switch(m_numOpcodes)
            {
                case 1 :
                case 2 : level = ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad16Bit; break;
                case 3 : level = ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad24Bit; break;
                default: level = ZeligIR.ImageBuilders.Core.ConstantAddressEncodingLevel.FarRelativeLoad32Bit; break;
            }

            cs.Owner.IncreaseEncodingLevelForConstant( m_origin, level );
        }

        //
        // Access Methods
        //

        private int EffectiveSkew
        {
            get
            {
                return m_skew + m_numOpcodes * sizeof(uint);
            }
        }

        public int EncodingLength
        {
            get
            {
                return m_numOpcodes;
            }
        }
    }
}
