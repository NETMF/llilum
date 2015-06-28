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


    internal sealed class ArmCodeRelocation_Branch : ZeligIR.ImageBuilders.CodeRelocation
    {
        //
        // State
        //

        bool m_fConditional;

        //
        // Constructor Methods
        //

        private ArmCodeRelocation_Branch() // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmCodeRelocation_Branch( ZeligIR.ImageBuilders.SequentialRegion region       ,
                                           uint                                   offset       ,
                                           ZeligIR.BasicBlock                     target       ,
                                           ZeligIR.ImageBuilders.CompilationState cs           ,
                                           ZeligIR.Operator                       origin       ,
                                           int                                    skew         ,
                                           bool                                   fConditional ) : base( region, offset, sizeof(uint), target, cs, origin, skew )
        {
            m_fConditional = fConditional;
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( ZeligIR.TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_fConditional );

            context.Pop();
        }

        //--//

        public override void GetAllowedRelocationRange( out int lowerBound ,
                                                        out int upperBound )
        {
            lowerBound = m_skew + (int.MinValue >> 8) * sizeof(uint);
            upperBound = m_skew + (int.MaxValue >> 8) * sizeof(uint);
        }

        protected override bool ApplyRelocation( uint srcAddress ,
                                                 uint dstAddress )
        {
            InstructionSet.Opcode_Branch op = (InstructionSet.Opcode_Branch)ArmCompilationState.GetOpcode( m_cs, m_region, m_offset );

            op.Offset = ((int)dstAddress - (int)srcAddress) - m_skew;

            ArmCompilationState.SetOpcode( m_cs, m_region, m_offset, op );

            return true;
        }

        protected override void NotifyFailedRelocation()
        {
            ArmCompilationState cs = (ArmCompilationState)m_cs;

            cs.Owner.IncreaseEncodingLevelForBranch( m_origin, (ZeligIR.BasicBlock)m_target, m_fConditional );
        }

        //
        // Access Methods
        //

        public bool IsConditional
        {
            get
            {
                return m_fConditional;
            }
        }
    }
}
