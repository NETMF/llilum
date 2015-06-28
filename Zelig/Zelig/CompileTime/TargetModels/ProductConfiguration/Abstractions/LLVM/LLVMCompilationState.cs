//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//




namespace Microsoft.Zelig.Configuration.Environment.Abstractions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.LLVM;

    using ZeligIR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public partial class LLVMCompilationState : ZeligIR.ImageBuilders.CompilationState
    {
        private LLVM._Function m_function;
        private LLVMModuleManager m_manager;
        private TS.MethodRepresentation m_method;
        private GrowOnlyHashTable<ZeligIR.Expression,LLVM._Value> m_localValues;
        private TS.WellKnownFields     m_wkf;
        private TS.WellKnownTypes      m_wkt;

        protected LLVMCompilationState( ) // Default constructor required by TypeSystemSerializer.
        {
        }

        internal LLVMCompilationState( ZeligIR.ImageBuilders.Core owner,
                                      ZeligIR.ControlFlowGraphStateForCodeTransformation cfg )
            : base( owner, cfg )
        {
            m_method = cfg.Method;
            m_localValues = HashTableFactory.New<ZeligIR.Expression, LLVM._Value>( );
            m_wkf = cfg.TypeSystem.WellKnownFields;
            m_wkt = cfg.TypeSystem.WellKnownTypes;
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

            m_manager = Owner.TypeSystem.Module;
            m_function = m_manager.GetOrInsertFunction( m_method );
            m_function.SetInternalLinkage( );

            //Miguel: Review: I can probably get rid of this...
            m_manager.ConvertTypeLayoutsToLLVM( );
            
        }


        protected ZeligIR.Abstractions.RegisterDescriptor GetNextRegister( ZeligIR.Abstractions.RegisterDescriptor reg )
        {
            ZeligIR.Abstractions.Platform pa = m_cfg.TypeSystem.PlatformAbstraction;
            return pa.GetRegisterForEncoding( reg.Encoding + 1 );
        }

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

        }

        protected override void DumpOpcodes( System.IO.TextWriter textWriter )
        {

        }

        public override bool CreateCodeMaps( )
        {
            return false;
        }

        //
        // Access Methods
        //

        public LLVMPlatform LLVMPlatform
        {
            get
            {
                return ( LLVMPlatform )m_cfg.TypeSystem.PlatformAbstraction;
            }
        }
    }
}