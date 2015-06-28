//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.TargetModel.ArmProcessor;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;


    public partial class ArmCompilationState : ZeligIR.ImageBuilders.CompilationState
    {
        //
        // State
        //

        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_Scratch;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_R0;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_R1;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_R2;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_R3;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_R4;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_LR;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_SP;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_PC;

        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_D0;
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_FPSCR;  
        internal ZeligIR.Abstractions.RegisterDescriptor m_reg_FPEXC;  

        private  BitVector                               m_registersUsed   = new BitVector();
        private  BitVector                               m_registersToSave = new BitVector();
        private  uint                                    m_stackForWrapper;
        private  uint                                    m_stackForLocals;
        private  uint                                    m_stackForCalls;

        //
        // Constructor Methods
        //

        protected ArmCompilationState() // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmCompilationState( ZeligIR.ImageBuilders.Core                         owner ,
                                      ZeligIR.ControlFlowGraphStateForCodeTransformation cfg   ) : base( owner, cfg )
        {
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( ZeligIR.TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Transform( ref m_registersUsed   );
            context.Transform( ref m_registersToSave );
            context.Transform( ref m_stackForWrapper );
            context.Transform( ref m_stackForLocals  );
            context.Transform( ref m_stackForCalls   );

            context.Pop();
        }

        //--//

        protected override void PrepareDataStructures()
        {
            base.PrepareDataStructures();

            //--//

            ZeligIR.Abstractions.Platform pa = m_cfg.TypeSystem.PlatformAbstraction;

            m_reg_Scratch = pa.GetScratchRegister();
            m_reg_R0      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_r0 );
            m_reg_R1      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_r1 );
            m_reg_R2      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_r2 );
            m_reg_R3      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_r3 );
            m_reg_R4      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_r4 );
            m_reg_LR      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_lr );
            m_reg_SP      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_sp );
            m_reg_PC      = pa.GetRegisterForEncoding( EncodingDefinition_ARM.c_register_pc );

            m_reg_D0      = pa.GetRegisterForEncoding( EncodingDefinition_VFP_ARM.c_register_d0    );  
            m_reg_FPSCR   = pa.GetRegisterForEncoding( EncodingDefinition_VFP_ARM.c_register_FPSCR );  
            m_reg_FPEXC   = pa.GetRegisterForEncoding( EncodingDefinition_VFP_ARM.c_register_FPEXC );  
        }


        protected ZeligIR.Abstractions.RegisterDescriptor GetNextRegister( ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            ZeligIR.Abstractions.Platform pa = m_cfg.TypeSystem.PlatformAbstraction;

            return pa.GetRegisterForEncoding( reg.Encoding + 1 );
        }

        //--//

        protected override void AssignStackLocations_RecordRegister( ZeligIR.PhysicalRegisterExpression regVar )
        {
            m_registersUsed.Set( (int)regVar.RegisterDescriptor.Encoding );
        }

        protected override void AssignStackLocations_RecordStackOut( ZeligIR.StackLocationExpression stackVar )
        {
            m_stackForCalls = Math.Max( m_stackForCalls, (uint)stackVar.Number + 1 );
        }

        protected override void AssignStackLocations_RecordStackLocal( List< StateForLocalStackAssignment > states )
        {
            m_stackForLocals = StateForLocalStackAssignment.ComputeLayout( states );
        }

        protected override void AssignStackLocations_Finalize()
        {
            ZeligIR.TypeSystemForCodeTransformation ts = m_cfg.TypeSystem;
            ZeligIR.Abstractions.Platform           pa = ts.PlatformAbstraction;
            ZeligIR.Abstractions.CallingConvention  cc = ts.CallingConvention;
            Runtime.HardwareException               he;

            pa.ComputeSetOfRegistersToSave( cc, m_cfg, m_registersUsed, out m_registersToSave, out he );

            uint savedBytes = ComputeSizeOfStackWrapper();

            uint stackOffsetForOUT    = 0;
            uint stackOffsetForLOCAL  = stackOffsetForOUT   + m_stackForCalls  * sizeof(uint);
            uint stackOffsetForSAVED  = stackOffsetForLOCAL + m_stackForLocals * sizeof(uint);
            uint stackOffsetForIN     = stackOffsetForSAVED + savedBytes;

            m_stackOffsetForSAVED = stackOffsetForSAVED;

            foreach(var var in m_variables)
            {
                var stackVar = var as ZeligIR.StackLocationExpression;
                if(stackVar != null)
                {
                    switch(stackVar.StackPlacement)
                    {
                        case ZeligIR.StackLocationExpression.Placement.In   : stackVar.AllocationOffset = stackOffsetForIN    + (uint)stackVar.Number * sizeof(uint); break;
                        case ZeligIR.StackLocationExpression.Placement.Local: stackVar.AllocationOffset = stackOffsetForLOCAL +       stackVar.AllocationOffset     ; break;
                        case ZeligIR.StackLocationExpression.Placement.Out  : stackVar.AllocationOffset = stackOffsetForOUT   + (uint)stackVar.Number * sizeof(uint); break;
                    }
                }
            }
        }

        private uint ComputeSizeOfStackWrapper()
        {
            uint               res   = 0;
            ZeligIR.BasicBlock bb    = m_cfg.EntryBasicBlock;
            ZeligIR.BasicBlock bbEnd = m_cfg.NormalizedEntryBasicBlock;

            while(bb != null && bb != bbEnd)
            {
                foreach(var op in bb.Operators)
                {
                    var op2 = op as ARM.MoveIntegerRegistersOperator;
                    if(op2 != null)
                    {
                        res += ComputeStackSize_ARM_MoveIntegerRegistersOperator( op2 );
                        continue;
                    }

                    var op3 = op as ARM.MoveFloatingPointRegistersOperator;
                    if(op3 != null)
                    {
                        res += ComputeStackSize_ARM_MoveFloatingPointRegistersOperator( op3 );
                        continue;
                    }

                    ZeligIR.VariableExpression var;
                    long                       value;

                    if(op.IsAddOrSubAgainstConstant( out var, out value ) && GetRegisterDescriptor( var ) == m_reg_SP)
                    {
                        res = (uint)(res - value);
                        continue;
                    }

                    var op4 = op as ZeligIR.UnconditionalControlOperator;
                    if(op4 != null)
                    {
                        bb = op4.TargetBranch;
                        break;
                    }

                    if(op is ZeligIR.ControlOperator)
                    {
                        throw NotImplemented();
                    }
                }
            }

            return res;
        }

        protected override void FixupEmptyRegion( ZeligIR.ImageBuilders.SequentialRegion reg )
        {
            var sec = reg.GetSectionOfFixedSize( sizeof(uint) );

            InstructionSet.Opcode_Breakpoint enc = this.Encoder.PrepareForBreakpoint;

            enc.Prepare( EncodingDefinition_ARM.c_cond_AL,      // uint ConditionCodes ,
                         0xFFFF                             );  // uint Value          );

            sec.Write( enc.Encode() );
        }

        //
        // Access Methods
        //

        public ArmPlatform ArmPlatform
        {
            get
            {
                return (ArmPlatform)m_cfg.TypeSystem.PlatformAbstraction;
            }
        }
    }
}