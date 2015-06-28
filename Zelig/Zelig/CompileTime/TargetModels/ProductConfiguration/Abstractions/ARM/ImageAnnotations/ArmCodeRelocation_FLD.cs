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


    internal sealed class ArmCodeRelocation_FLD : ZeligIR.ImageBuilders.CodeRelocation
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        private ArmCodeRelocation_FLD() // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmCodeRelocation_FLD( ZeligIR.ImageBuilders.SequentialRegion region ,
                                        uint                                   offset ,
                                        ZeligIR.ImageBuilders.CodeConstant     target ,
                                        ZeligIR.ImageBuilders.CompilationState cs     ,
                                        ZeligIR.Operator                       origin ,
                                        int                                    skew   ) : base( region, offset, sizeof(uint), target, cs, origin, skew )
        {
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( ZeligIR.TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Pop();
        }

        //--//

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            lowerBound = m_skew - ((1 << 8) - 1) * sizeof(uint);
            upperBound = m_skew + ((1 << 8) - 1) * sizeof(uint);
        }

        protected override bool ApplyRelocation( uint srcAddress ,
                                                 uint dstAddress )
        {
            InstructionSet_VFP.Opcode_VFP_DataTransfer op = (InstructionSet_VFP.Opcode_VFP_DataTransfer)ArmCompilationState.GetOpcode( m_cs, m_region, m_offset );

            if(op.Rn != EncodingDefinition.c_register_pc)
            {
                throw NotImplemented();
            }

            int relPos = ((int)dstAddress - (int)srcAddress) - m_skew;

            if(relPos < 0)
            {
                op.Up     =        false;
                op.Offset = ((uint)-relPos) / sizeof(uint);
            }
            else
            {
                op.Up     =        true;
                op.Offset = ((uint)relPos) / sizeof(uint);
            }

            ArmCompilationState.SetOpcode( m_cs, m_region, m_offset, op );

            return true;
        }

        protected override void NotifyFailedRelocation()
        {
            ArmCompilationState cs = (ArmCompilationState)m_cs;

            cs.Owner.IncreaseEncodingLevelForConstant( m_origin );
        }

        //
        // Access Methods
        //

    }
}
