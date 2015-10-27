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


    public abstract partial class ArmPlatform : ZeligIR.Abstractions.Platform
    {
        public enum Comparison : uint
        {
            Equal                    = 0x00, // EQ       => Z set
            NotEqual                 = 0x01, // NE       => Z clear
            CarrySet                 = 0x02, // CS/HS    => C set
            CarryClear               = 0x03, // CC/LO    => C clear
            Negative                 = 0x04, // MI       => N set
            PositiveOrZero           = 0x05, // PL       => N clear
            Overflow                 = 0x06, // VS       => V set
            NoOverflow               = 0x07, // VC       => V clear
            UnsignedHigherThan       = 0x08, // HI       => C set and Z clear
            UnsignedLowerThanOrSame  = 0x09, // LS       => C clear or Z set
            SignedGreaterThanOrEqual = 0x0A, // GE       => N set and V set, or N clear and V clear (N == V)
            SignedLessThan           = 0x0B, // LT       => N set and V clear, or N clear and V set (N != V)
            SignedGreaterThan        = 0x0C, // GT       => Z clear, and either N set and V set, or N clear and V clear (Z ==0, N == V)
            SignedLessThanOrEqual    = 0x0D, // LE       => Z set, or N set and V clear, or N clear and V set (Z == 1 or N != V)
            Always                   = 0x0E, // AL       Always (unconditional)
            NotValid                 = 0x0F, //

            UnsignedHigherThanOrSame = CarrySet  ,
            UnsignedLowerThan        = CarryClear,
        }

        [Flags]
        public enum Capabilities : uint
        {
            ARMv4  = 0x00000001,
            ARMv5  = 0x00000002,
            ARMv7M = 0x00000004,
            ARMv7R = 0x00000008,
            ARMv7A = 0x00000010,
            VFPv2  = 0x00010000,
        }

        //
        // State
        //

        private List< Runtime.Memory.Range >               m_memoryBlocks;
        private Capabilities                               m_processorCapabilities;

        private ZeligIR.Abstractions.PlacementRequirements m_memoryRequirement_VectorTable;
        private ZeligIR.Abstractions.PlacementRequirements m_memoryRequirement_Bootstrap;
        private ZeligIR.Abstractions.PlacementRequirements m_memoryRequirement_Code;
        private ZeligIR.Abstractions.PlacementRequirements m_memoryRequirement_Data_RW;
        private ZeligIR.Abstractions.PlacementRequirements m_memoryRequirement_Data_RO;

        private bool                                       m_fGotVectorTable;
        private MethodRepresentation                       m_vectorTable;

        //
        // Constructor Methods
        //

        protected ArmPlatform( Capabilities processorCapabilities )
        {
            AllocateState( processorCapabilities );
        }

        protected ArmPlatform( ZeligIR.TypeSystemForCodeTransformation typeSystem            ,
                               MemoryMapCategory                       memoryMap             ,
                               Capabilities                            processorCapabilities ) : base( typeSystem )
        {
            AllocateState( processorCapabilities );

            if(memoryMap != null)
            {
                ConfigureMemoryMap( memoryMap );
            }

            //--//

            List< ZeligIR.Abstractions.RegisterDescriptor > regs = new List< ZeligIR.Abstractions.RegisterDescriptor >();
            
            CreateRegisters( regs );

            m_registers = regs.ToArray();
        }


        public override InstructionSet GetInstructionSetProvider()
        {
            if(m_instructionSetProvider == null)
            {
                InstructionSetVersion iset = new InstructionSetVersion(this.PlatformName, this.PlatformVersion, this.PlatformVFP);

                if(this.HasVFPv2)
                {
                    m_instructionSetProvider = new TargetModel.ArmProcessor.InstructionSet_VFP( iset );
                }
                else
                {
                    m_instructionSetProvider = new TargetModel.ArmProcessor.InstructionSet( iset );
                }
            }

            return m_instructionSetProvider;
        }

        protected virtual void AllocateState( Capabilities processorCapabilities )
        {
            m_processorCapabilities         = processorCapabilities;

            m_memoryBlocks                  = new List< Runtime.Memory.Range >();

            m_memoryRequirement_VectorTable = CreatePlacement( Runtime.MemoryUsage.VectorsTable );
            m_memoryRequirement_Bootstrap   = CreatePlacement( Runtime.MemoryUsage.Bootstrap    );
            m_memoryRequirement_Code        = CreatePlacement( Runtime.MemoryUsage.Code         );
            m_memoryRequirement_Data_RO     = CreatePlacement( Runtime.MemoryUsage.DataRO       );
            m_memoryRequirement_Data_RW     = CreatePlacement( Runtime.MemoryUsage.DataRW       );
        }

        private static ZeligIR.Abstractions.PlacementRequirements CreatePlacement( Runtime.MemoryUsage usage )
        {
            ZeligIR.Abstractions.PlacementRequirements pr = new ZeligIR.Abstractions.PlacementRequirements( sizeof(uint), 0 );

            pr.AddConstraint( usage );

            return pr;
        }

        //
        // Helper Methods
        //

        public override void ApplyTransformation( TransformationContext context )
        {
            ZeligIR.TransformationContextForCodeTransformation context2 = (ZeligIR.TransformationContextForCodeTransformation)context;

            context2.Push( this );

            base.ApplyTransformation( context2 );

            context2.Transform( ref m_memoryBlocks    );

            context2.Transform( ref m_registers       );
            context2.Transform( ref m_scratchRegister );

            context2.Pop();
        }

        public override void RegisterForNotifications( ZeligIR.TypeSystemForCodeTransformation  ts    ,
                                                       ZeligIR.CompilationSteps.DelegationCache cache )
        {
            cache.Register( this );

            cache.Register( new ArmPlatform.PrepareForRegisterAllocation( this ) );
        }

        public override TypeRepresentation GetRuntimeType( ZeligIR.TypeSystemForCodeTransformation ts      ,
                                                           ZeligIR.Abstractions.RegisterDescriptor regDesc )
        {
            WellKnownTypes wkt = ts.WellKnownTypes;

            if(regDesc.InIntegerRegisterFile)
            {
                return wkt.System_UIntPtr;
            }
            else if(regDesc.InFloatingPointRegisterFile)
            {
                if(regDesc.IsDoublePrecision)
                {
                    return wkt.System_Double;
                }
                else
                {
                    return wkt.System_Single;
                }
            }
            else
            {
                return wkt.System_UInt32;
            }
        }

        public override bool CanFitInRegister( TypeRepresentation td )
        {
            if(td.IsFloatingPoint)
            {
                if(this.HasVFPv2)
                {
                    return true;
                }
            }

            return (td.SizeOfHoldingVariableInWords <= 1);
        }

        //--//

        public void AddMemoryBlock( Runtime.Memory.Range range )
        {
            m_memoryBlocks.Add( range );
        }

        public void ConfigureMemoryMap( MemoryMapCategory memoryMap )
        {
            foreach(AbstractCategory.ValueContext ctx in memoryMap.SearchValues( typeof(MemoryCategory) ))
            {
                MemoryCategory           val   = (MemoryCategory)ctx.Value;
                Runtime.MemoryAttributes flags = val.Characteristics;
                string                   name  = null;
                Runtime.MemoryUsage      usage = Runtime.MemoryUsage.Undefined;
                Type                     hnd   = null;

                if((flags & (Runtime.MemoryAttributes.FLASH | Runtime.MemoryAttributes.RAM)) == 0)
                {
                    //
                    // Only interested in programmable memories, not peripherals.
                    //
                    continue;
                }

                MemorySectionAttribute attrib = ReflectionHelper.GetAttribute< MemorySectionAttribute >( ctx.Field, false );
                if(attrib != null)
                {
                    name  = attrib.Name;
                    usage = attrib.Usage;
                    hnd   = attrib.ExtensionHandler;
                }

                //--//

                uint address = val.BaseAddress;

                if(hnd != null)
                {
                    object obj = Activator.CreateInstance( hnd );

                    if(obj is IMemoryMapper)
                    {
                        IMemoryMapper itf = (IMemoryMapper)obj;

                        address = itf.GetCacheableAddress( address );
                    }
                }

                UIntPtr beginning  = new UIntPtr( address                   );
                UIntPtr end        = new UIntPtr( address + val.SizeInBytes );
                          
                List< Runtime.Memory.Range > ranges = new List< Runtime.Memory.Range >();

                ranges.Add( new Runtime.Memory.Range( beginning, end, name, flags, usage, hnd ) );

                foreach(ReserveBlockAttribute resAttrib in ReflectionHelper.GetAttributes< ReserveBlockAttribute >( ctx.Field, false ))
                {
                    UIntPtr blockStart = AddressMath.Increment( beginning , resAttrib.Offset );
                    UIntPtr blockEnd   = AddressMath.Increment( blockStart, resAttrib.Size   );

                    for(int i = ranges.Count; --i >= 0; )
                    {
                        var rng = ranges[i];

                        Runtime.Memory.Range.SubstractionAction action = rng.ComputeSubstraction( blockStart, blockEnd );

                        if(action != Runtime.Memory.Range.SubstractionAction.RemoveNothing)
                        {
                            ranges.RemoveAt( i );

                            if(action == Runtime.Memory.Range.SubstractionAction.RemoveStart  ||
                               action == Runtime.Memory.Range.SubstractionAction.RemoveMiddle  )
                            {
                                ranges.Insert( i, rng.CloneSettings( blockEnd, rng.End ) );
                            }

                            if(action == Runtime.Memory.Range.SubstractionAction.RemoveMiddle ||
                               action == Runtime.Memory.Range.SubstractionAction.RemoveEnd     )
                            {
                                ranges.Insert( i, rng.CloneSettings( rng.Start, blockStart ) );
                            }
                        }
                    }
                }

                foreach(var rng in ranges)
                {
                    AddMemoryBlock( rng );
                }
            }
        }

        //--//

        public override void GetListOfMemoryBlocks( List< Runtime.Memory.Range > lst )
        {
            lst.AddRange( m_memoryBlocks );
        }

        public override ZeligIR.Abstractions.PlacementRequirements GetMemoryRequirements( object obj )
        {
            if(m_fGotVectorTable == false)
            {
                m_fGotVectorTable = true;
                m_vectorTable     = m_typeSystem.TryGetHandler( Runtime.HardwareException.VectorTable );
            }

            //
            // TODO: Based on profiling data, we could decide to put some code or data in RAM.
            //
            MethodRepresentation md  = null;

            if(obj is ZeligIR.ControlFlowGraphStateForCodeTransformation)
            {
                md = ((ZeligIR.ControlFlowGraphStateForCodeTransformation)obj).Method;
            }
            else if(obj is ZeligIR.BasicBlock)
            {
                md = ((ZeligIR.BasicBlock)obj).Owner.Method;
            }

            if(md != null)
            {
                ZeligIR.Abstractions.PlacementRequirements pr = m_typeSystem.GetPlacementRequirements( md );
                if(pr != null)
                {
                    return pr;
                }

                if(md == m_vectorTable)
                {
                    return m_memoryRequirement_VectorTable;
                }

                switch(m_typeSystem.ExtractHardwareExceptionSettingsForMethod( md ))
                {
                    case Runtime.HardwareException.Bootstrap:
                    case Runtime.HardwareException.Reset:
                        return m_memoryRequirement_Bootstrap;
                }

                return m_memoryRequirement_Code;
            }
            else if(obj is ZeligIR.DataManager.DataDescriptor)
            {
                ZeligIR.DataManager.DataDescriptor         dd = (ZeligIR.DataManager.DataDescriptor)obj;
                ZeligIR.Abstractions.PlacementRequirements pr = dd.PlacementRequirements;

                if(pr != null)
                {
                    return pr;
                }

                if(dd.IsMutable)
                {
                    pr = m_memoryRequirement_Data_RW;
                }
                else
                {
                    pr = m_memoryRequirement_Data_RO;
                }

                return pr;
            }

            throw TypeConsistencyErrorException.Create( "Unable to determine the memory requirements for {0}", obj );
        }

        public override ZeligIR.ImageBuilders.CompilationState CreateCompilationState( ZeligIR.ImageBuilders.Core                         core ,
                                                                                       ZeligIR.ControlFlowGraphStateForCodeTransformation cfg  )
        {
            return new ArmCompilationState( core, cfg );
        }

        //
        // Access Methods
        //

        public override string PlatformName
        {
            get { return InstructionSetVersion.Platform_ARM; }
        }

        public override string PlatformVersion
        {
            get 
            {
                string ver = InstructionSetVersion.PlatformVersion_4;

                if(0 != (m_processorCapabilities & Capabilities.ARMv4))
                {
                    ver = InstructionSetVersion.PlatformVersion_4;
                }
                else if(0 != ( m_processorCapabilities & Capabilities.ARMv5 ))
                {
                    ver = InstructionSetVersion.PlatformVersion_5;
                }
                else if(0 != ( m_processorCapabilities & Capabilities.ARMv7M ))
                {
                    ver = InstructionSetVersion.PlatformVersion_7M;
                }

                return ver;
            }
        }

        public override string PlatformVFP
        {
            get 
            {
                string vfp = InstructionSetVersion.PlatformVFP_NoVFP;

                if(0 != ( m_processorCapabilities & Capabilities.VFPv2 ))
                {
                    vfp = InstructionSetVersion.PlatformVFP_VFP;
                }

                return vfp;
            }
        }

        public override bool PlatformBigEndian
        {
            get { return false; }
        }

        public override bool CanUseMultipleConditionCodes
        {
            get
            {
                return false;
            }
        }

        public override int EstimatedCostOfLoadOperation
        {
            get
            {
                return 1 + 3;
            }
        }

        public override int EstimatedCostOfStoreOperation
        {
            get
            {
                return 1 + 2;
            }
        }

        public override uint MemoryAlignment
        {
            get
            {
                return sizeof(uint);
            }
        }

        public Capabilities ProcessorCapabilities
        {
            get
            {
                return m_processorCapabilities;
            }
        }

        public bool HasVFPv2
        {
            get
            {
                return (m_processorCapabilities & ArmPlatform.Capabilities.VFPv2) != 0;
            }
        }
    }
}
