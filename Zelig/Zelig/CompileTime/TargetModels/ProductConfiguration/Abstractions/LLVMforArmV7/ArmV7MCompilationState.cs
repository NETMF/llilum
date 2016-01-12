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

    public partial class ArmV7MCompilationState : ArmCompilationState
    {
        private _Function                                          m_function;
        private LLVMModuleManager                                  m_manager;
        private TS.MethodRepresentation                            m_method;
        private TS.WellKnownFields                                 m_wkf;
        private TS.WellKnownTypes                                  m_wkt;
        private GrowOnlyHashTable<ZeligIR.Expression, ValueCache>  m_localValues;
        private GrowOnlyHashTable<ZeligIR.BasicBlock, _BasicBlock> m_blocks;

        protected ArmV7MCompilationState( ) // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmV7MCompilationState( ZeligIR.ImageBuilders.Core owner,
                                         ZeligIR.ControlFlowGraphStateForCodeTransformation cfg )
            : base( owner, cfg )
        {
            m_method = cfg.Method;
            m_wkf = cfg.TypeSystem.WellKnownFields;
            m_wkt = cfg.TypeSystem.WellKnownTypes;
            m_localValues = HashTableFactory.New<ZeligIR.Expression, ValueCache>();
            m_blocks = HashTableFactory.New<ZeligIR.BasicBlock, _BasicBlock>();
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

            m_manager  = Owner.TypeSystem.Module;
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

        public ArmV7M LLVMPlatform
        {
            get
            {
                return ( ArmV7M )m_cfg.TypeSystem.PlatformAbstraction;
            }
        }
    }
}