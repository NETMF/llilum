//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.LLVM;
    
    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;
    using TS      = Microsoft.Zelig.Runtime.TypeSystem;

    public partial class LlvmForArmV7MCompilationState : ArmCompilationState
    {
        private _Function                                          m_function;
        private LLVMModuleManager                                  m_manager;
        private TS.MethodRepresentation                            m_method;
        private TS.WellKnownFields                                 m_wkf;
        private TS.WellKnownTypes                                  m_wkt;
        private GrowOnlyHashTable<ZeligIR.Expression, ValueCache>  m_localValues;
        private GrowOnlyHashTable<ZeligIR.BasicBlock, _BasicBlock> m_blocks;

        protected LlvmForArmV7MCompilationState( ) // Default constructor required by TypeSystemSerializer.
        {
        }

        internal LlvmForArmV7MCompilationState( ZeligIR.ImageBuilders.Core owner,
                                         ZeligIR.ControlFlowGraphStateForCodeTransformation cfg )
            : base( owner, cfg )
        {
            var typeSystem = owner.TypeSystem;

            m_manager       = typeSystem.Module;
            m_method        = cfg.Method;
            m_wkf           = typeSystem.WellKnownFields;
            m_wkt           = typeSystem.WellKnownTypes;
            m_localValues   = HashTableFactory.New<ZeligIR.Expression, ValueCache>();
            m_blocks        = HashTableFactory.New<ZeligIR.BasicBlock, _BasicBlock>();
            
            //
            // HACK!!! Support Windows on its own compilation abstraction
            //
            //m_manager.SectionNameProvider = new Thumb2EabiSectionNameProvider( typeSystem );

            if (typeSystem.PlatformAbstraction.PlatformFamily == TargetModel.Win32.InstructionSetVersion.Platform_Family__Win32)
            {
                m_manager.SectionNameProvider = new Win32SectionNameProvider(typeSystem);
            }
            else
            {
                m_manager.SectionNameProvider = new Thumb2EabiSectionNameProvider(typeSystem);
            }
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( ZeligIR.TransformationContextForCodeTransformation context )
        {
            context.Push( this );

            base.ApplyTransformation( context );

            context.Pop( );
        }

        //--//

        protected override void PrepareDataStructures( )
        {
            base.PrepareDataStructures( );
            
            m_function = m_manager.GetOrInsertFunction( m_method );
            m_function.SetInternalLinkage( );
            m_manager .ConvertTypeLayoutsToLLVM( );

            ReleaseAllLocks( );
        }

        //protected ZeligIR.Abstractions.RegisterDescriptor GetNextRegister( ZeligIR.Abstractions.RegisterDescriptor reg )
        //{
        //    ZeligIR.Abstractions.Platform pa = m_cfg.TypeSystem.PlatformAbstraction;
        //    return pa.GetRegisterForEncoding( reg.Encoding + 1 );
        //}

        //--//

        protected override void AssignStackLocations_RecordRegister( ZeligIR.PhysicalRegisterExpression regVar )
        {

        }

        protected override void AssignStackLocations_RecordStackOut( ZeligIR.StackLocationExpression stackVar )
        {
        }

        protected override void AssignStackLocations_RecordStackLocal( List<StateForLocalStackAssignment> states )
        {
        }

        protected override void AssignStackLocations_Finalize( )
        {
        }

        protected override void FixupEmptyRegion( ZeligIR.ImageBuilders.SequentialRegion reg )
        {
            //
            // TODO: lt72: enable proper encoding 
            // 
            var sec = reg.GetSectionOfFixedSize( sizeof(uint) );

            //InstructionSet.Opcode_Breakpoint enc = this.Encoder.PrepareForBreakpoint;

            //enc.Prepare( EncodingDefinition_ARM.c_cond_AL,      // uint ConditionCodes ,
            //             0xFFFF                             );  // uint Value          );

            sec.Write( 0xDEADBEEF );
        }

        protected override void DumpOpcodes( System.IO.TextWriter textWriter )
        {
        }

        //
        // Access Methods
        //

        public LlvmForArmV7M LLVMPlatform
        {
            get
            {
                return ( LlvmForArmV7M )m_cfg.TypeSystem.PlatformAbstraction;
            }
        }
    }
}