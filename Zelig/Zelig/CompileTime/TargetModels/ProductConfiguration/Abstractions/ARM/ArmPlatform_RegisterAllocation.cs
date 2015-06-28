//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmPlatform
    {
        //
        // State
        //

        private ZeligIR.Abstractions.RegisterDescriptor[] m_registers;
        private ZeligIR.Abstractions.RegisterDescriptor   m_scratchRegister;
        private InstructionSet                            m_instructionSetProvider;

        //
        // Helper Methods
        //

        public override ZeligIR.Abstractions.RegisterDescriptor[] GetRegisters()
        {
            return m_registers;
        }

        public override ZeligIR.Abstractions.RegisterDescriptor GetScratchRegister()
        {
            return m_scratchRegister;
        }

        public override ZeligIR.Abstractions.RegisterDescriptor GetRegisterForEncoding( uint encoding )
        {
            foreach(var regDesc in m_registers)
            {
                if(regDesc.Encoding == encoding)
                {
                    return regDesc;
                }
            }

            return null;
        }

        public override void ComputeNumberOfFragmentsForExpression(     TypeRepresentation                                                sourceTd        ,
                                                                        ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment kind            ,
                                                                    out uint                                                              sourceFragments ,
                                                                    out bool                                                              fDirect         )
        {
            if(sourceTd is ReferenceTypeRepresentation ||
               sourceTd is PointerTypeRepresentation    )
            {
                CHECKS.ASSERT( sourceTd.SizeOfHoldingVariableInWords == 1, "Unexpected size for pointer type {0}: {1}", sourceTd, sourceTd.SizeOfHoldingVariableInWords );

                sourceFragments = 1;
                fDirect         = true;
            }
            else
            {
                CHECKS.ASSERT( sourceTd is ValueTypeRepresentation, "Unexpected type {0}", sourceTd );

                if(sourceTd.IsFloatingPoint)
                {
                    if(this.HasVFPv2)
                    {
                        sourceFragments = 1;
                        fDirect         = true;
                        return;
                    }
                }

                sourceFragments = sourceTd.SizeOfHoldingVariableInWords;
                fDirect         = false;
            }
        }


        protected virtual void CreateRegisters( List< ZeligIR.Abstractions.RegisterDescriptor > lst )
        {
            ZeligIR.Abstractions.RegisterDescriptor reg;

            for(uint encoding = EncodingDefinition_ARM.c_register_r0; encoding <= EncodingDefinition_ARM.c_register_r11; encoding++)
            {
                reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitIntegerRegister( lst, string.Format( "R{0}", encoding ), encoding, encoding, ZeligIR.Abstractions.RegisterClass.AvailableForAllocation );
            }

            //--//

            //
            // Reserved as a scratch register, so we cannot use it during register allocation.
            //
            reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitIntegerRegister( lst, "R12", EncodingDefinition_ARM.c_register_r12, EncodingDefinition_ARM.c_register_r12, ZeligIR.Abstractions.RegisterClass.None );

            m_scratchRegister = reg;

            //--//

            //
            // Intrinsic use.
            //

            reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitIntegerRegister( lst, "SP", EncodingDefinition_ARM.c_register_sp, EncodingDefinition_ARM.c_register_sp, ZeligIR.Abstractions.RegisterClass.Special | ZeligIR.Abstractions.RegisterClass.StackPointer );

            //--//

            reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitIntegerRegister( lst, "LR", EncodingDefinition_ARM.c_register_lr, EncodingDefinition_ARM.c_register_lr, ZeligIR.Abstractions.RegisterClass.AvailableForAllocation | ZeligIR.Abstractions.RegisterClass.LinkAddress );

            //--//

            //
            // Intrinsic use.
            //

            reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitIntegerRegister( lst, "PC", EncodingDefinition_ARM.c_register_pc, EncodingDefinition_ARM.c_register_pc, ZeligIR.Abstractions.RegisterClass.Special | ZeligIR.Abstractions.RegisterClass.ProgramCounter );

            //--//

            if(this.HasVFPv2)
            {
                uint encoding32Lo = EncodingDefinition_VFP_ARM.c_register_s0;
                uint encoding32Hi = EncodingDefinition_VFP_ARM.c_register_s1;
                uint encoding64   = EncodingDefinition_VFP_ARM.c_register_d0;

                for(uint i = 0; i < 16; i++)
                {
                    //
                    // S0 and S1 are aliases for D0, etc.
                    // 
                    // To express that, the PhysicalStorageOffset of S<2*idx> is the same as D<idx>. Since D<idx> is twice as big as S<2*idx>, it also covers S<2*idx+1>.
                    //
                    var reg32Lo = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitFloatingPointRegister( lst, string.Format( "S{0}", i * 2     ), encoding32Lo, encoding32Lo, ZeligIR.Abstractions.RegisterClass.AvailableForAllocation, true  );
                    var reg32Hi = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitFloatingPointRegister( lst, string.Format( "S{0}", i * 2 + 1 ), encoding32Hi, encoding32Hi, ZeligIR.Abstractions.RegisterClass.AvailableForAllocation, false );
                    var reg64   = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor64bitFloatingPointRegister( lst, string.Format( "D{0}", i         ), encoding64  , encoding32Lo, ZeligIR.Abstractions.RegisterClass.AvailableForAllocation        );

                    reg32Lo.AddInterference( reg64   );
                    reg32Hi.AddInterference( reg64   );
                    reg64  .AddInterference( reg32Lo );
                    reg64  .AddInterference( reg32Hi );

                    encoding32Lo += 2;
                    encoding32Hi += 2;
                    encoding64   += 1;
                }

                reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitSystemRegister( lst, "FPSID", EncodingDefinition_VFP_ARM.c_register_FPSID, EncodingDefinition_VFP_ARM.c_register_FPSID, ZeligIR.Abstractions.RegisterClass.Special );
                reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitSystemRegister( lst, "FPSCR", EncodingDefinition_VFP_ARM.c_register_FPSCR, EncodingDefinition_VFP_ARM.c_register_FPSCR, ZeligIR.Abstractions.RegisterClass.Special );
                reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitSystemRegister( lst, "FPEXC", EncodingDefinition_VFP_ARM.c_register_FPEXC, EncodingDefinition_VFP_ARM.c_register_FPEXC, ZeligIR.Abstractions.RegisterClass.Special );
            }

            //--//

            reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitSystemRegister( lst, "CPSR", EncodingDefinition_ARM.c_register_cpsr, EncodingDefinition_ARM.c_register_cpsr, ZeligIR.Abstractions.RegisterClass.Special | ZeligIR.Abstractions.RegisterClass.StatusRegister );
            reg = ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitSystemRegister( lst, "SPSR", EncodingDefinition_ARM.c_register_spsr, EncodingDefinition_ARM.c_register_spsr, ZeligIR.Abstractions.RegisterClass.Special | ZeligIR.Abstractions.RegisterClass.StatusRegister );
        }
    }
}
