//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using Microsoft.Zelig.Runtime.TypeSystem;
    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using System;
    using System.Collections.Generic;
    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;

    public sealed class LLVMPlatform : ArmPlatform
    {
        private const Capabilities c_ProcessorCapabilities = Capabilities.ARMv7M;
        private const String       c_LLVM                  = "LLVM";

        //
        // State
        //

        private ZeligIR.Abstractions.RegisterDescriptor[] m_registers;

        //
        // Constructor Methods
        //

        public LLVMPlatform( ZeligIR.TypeSystemForCodeTransformation typeSystem,
                            MemoryMapCategory                        memoryMap )
            : base( typeSystem, memoryMap, c_ProcessorCapabilities )
        {
            //Create just one allocable register.
            List< ZeligIR.Abstractions.RegisterDescriptor > regs = new List<ZeligIR.Abstractions.RegisterDescriptor>( );
            ZeligIR.Abstractions.RegisterDescriptor.CreateTemplateFor32bitIntegerRegister( regs, "0", 0, 0, ZeligIR.Abstractions.RegisterClass.AvailableForAllocation );
            m_registers = regs.ToArray( );
        }

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation ts,
                                                       ZeligIR.CompilationSteps.DelegationCache cache )
        {
        }

        public override ZeligIR.ImageBuilders.CompilationState CreateCompilationState( ZeligIR.ImageBuilders.Core core,
                                                                               ZeligIR.ControlFlowGraphStateForCodeTransformation cfg )
        {
            return new LLVMCompilationState( core, cfg );
        }

        //--//

        //
        // Access Methods
        //

        public override string PlatformName
        {
            get { return c_LLVM; }
        }

        public override string PlatformVersion
        {
            get
            {
                return InstructionSetVersion.PlatformVersion_7M;
            }
        }

        public override string PlatformVFP
        {
            get
            {
                return InstructionSetVersion.PlatformVFP_VFP;
            }
        }

        public override bool PlatformBigEndian
        {
            get { return false; }
        }


        public override InstructionSet GetInstructionSetProvider( )
        {
            throw new Exception( "GetInstructionSetProvider not implemented" );
        }

        public override ZeligIR.Abstractions.RegisterDescriptor[ ] GetRegisters( )
        {
            return m_registers;
        }

        public override ZeligIR.Abstractions.RegisterDescriptor GetRegisterForEncoding( uint regNum )
        {
            throw new Exception( "GetRegisterForEncoding not implemented" );
        }

        public override ZeligIR.Abstractions.RegisterDescriptor GetScratchRegister( )
        {
            throw new Exception( "GetScratchRegister not implemented" );
        }

        public override void ComputeNumberOfFragmentsForExpression( TypeRepresentation sourceTd,
                                                                    ZeligIR.ControlFlowGraphStateForCodeTransformation.KindOfFragment kind,
                                                                    out uint sourceFragments,
                                                                    out bool fDirect )
        {
            throw new Exception( "ComputeNumberOfFragmentsForExpression not implemented" );
        }

        public override bool CanFitInRegister( TypeRepresentation td )
        {
            return true;
        }

        //--//

        public override TypeRepresentation GetMethodWrapperType( )
        {
            return m_typeSystem.GetWellKnownType( "Microsoft_Zelig_LLVM_MethodWrapper" );
        }

        public override bool HasRegisterContextArgument( MethodRepresentation md )
        {
            return false;
        }

        public override void ComputeSetOfRegistersToSave( ZeligIR.Abstractions.CallingConvention cc,
                                                                   ZeligIR.ControlFlowGraphStateForCodeTransformation cfg,
                                                                   BitVector modifiedRegisters,
                                                               out BitVector registersToSave,
                                                               out Runtime.HardwareException he )
        {
            throw new Exception( "ComputeSetOfRegistersToSave not implemented" );
        }

        //--//

        protected override void ComputeRegisterFlushFixup( BitVector registersToSave )
        {
        }

        protected override bool CanFitTypeInPhysicalRegister( TypeRepresentation td )
        {
            return true;
        }

        //--//

        public override bool CanPropagateCopy( ZeligIR.SingleAssignmentOperator opSrc,
                                               ZeligIR.Operator opDst,
                                               int exIndexInDst,
                                               ZeligIR.VariableExpression[ ] variables,
                                               BitVector[ ] variableUses,
                                               BitVector[ ] variableDefinitions,
                                               ZeligIR.Operator[ ] operators )
        {
            return true;
        }
    }
}