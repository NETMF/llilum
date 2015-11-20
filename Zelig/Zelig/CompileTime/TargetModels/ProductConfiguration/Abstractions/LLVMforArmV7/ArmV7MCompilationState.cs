//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Zelig.LLVM;

    using IR = Microsoft.Zelig.CodeGeneration.IR;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public partial class ArmV7MCompilationState : IR.ImageBuilders.CompilationState
    {
        private _Function m_function;
        private LLVMModuleManager m_manager;
        private TS.MethodRepresentation m_method;
        private TS.WellKnownFields m_wkf;
        private TS.WellKnownTypes m_wkt;
        private GrowOnlyHashTable<IR.Expression, ValueCache> m_localValues;
        private GrowOnlyHashTable<IR.BasicBlock, _BasicBlock> m_blocks;
        private IDictionary<IR.PhiOperator, _Value> m_pendingPhiNodes;

        protected ArmV7MCompilationState( ) // Default constructor required by TypeSystemSerializer.
        {
        }

        internal ArmV7MCompilationState( IR.ImageBuilders.Core owner,
                                         IR.ControlFlowGraphStateForCodeTransformation cfg )
            : base( owner, cfg )
        {
            m_method = cfg.Method;
            m_wkf = cfg.TypeSystem.WellKnownFields;
            m_wkt = cfg.TypeSystem.WellKnownTypes;
            m_localValues = HashTableFactory.New<IR.Expression, ValueCache>( );
            m_blocks = HashTableFactory.New<IR.BasicBlock, _BasicBlock>( );
            m_pendingPhiNodes = new Dictionary<IR.PhiOperator, _Value>( );
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( IR.TransformationContextForCodeTransformation context )
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
            m_manager.ConvertTypeLayoutsToLLVM( );

            ReleaseAllLocks( );
        }

        protected IR.Abstractions.RegisterDescriptor GetNextRegister( IR.Abstractions.RegisterDescriptor reg )
        {
            IR.Abstractions.Platform pa = m_cfg.TypeSystem.PlatformAbstraction;
            return pa.GetRegisterForEncoding( reg.Encoding + 1 );
        }

        //--//

        protected override void AssignStackLocations_RecordRegister( IR.PhysicalRegisterExpression regVar )
        {

        }

        protected override void AssignStackLocations_RecordStackOut( IR.StackLocationExpression stackVar )
        {
        }

        protected override void AssignStackLocations_RecordStackLocal( List<StateForLocalStackAssignment> states )
        {
        }

        protected override void AssignStackLocations_Finalize( )
        {
        }

        protected override void FixupEmptyRegion( IR.ImageBuilders.SequentialRegion reg )
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

        public ArmV7M LLVMPlatform
        {
            get
            {
                return ( ArmV7M )m_cfg.TypeSystem.PlatformAbstraction;
            }
        }
    }
}