//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [DisplayName("MM9691LP (Ollie 768)")]
    public sealed class MM9691LP : ProcessorCategory
    {
        public class MemoryMapper : IMemoryMapper
        {
            const uint CacheableMask = 0x80000000u;

            //--//

            public uint GetCacheableAddress( uint address )
            {
                return address | CacheableMask;
            }

            public uint GetUncacheableAddress( uint address )
            {
                return address & ~CacheableMask;
            }
        }

        [DependsOn(typeof(MM9691LP))]
        [DisplayName("Internal 384KB Static RAM")]
        [Defaults    ( "BaseAddress"    , 0x08000000                                      )]
        [Defaults    ( "SizeInBytes"    , 384 * 1024                                      )]
        [Defaults    ( "WordSize"       , 32                                              )]
        [Defaults    ( "WaitStates"     , 0                                               )]
        [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM                    |
                                          Runtime.MemoryAttributes.RandomAccessMemory     |
                                          Runtime.MemoryAttributes.InternalMemory         |
                                          Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.MM9691LP.RamMemoryHandler), HardwareModelAttribute.Kind.Memory)]
        public sealed class InternalRAM384KB : RamMemoryCategory
        {
        }

        [DependsOn(typeof(MM9691LP))]
        [DisplayName("Internal 768KB Static RAM")]
        [Defaults    ( "BaseAddress"    , 0x08000000                                      )]
        [Defaults    ( "SizeInBytes"    , 768 * 1024                                      )]
        [Defaults    ( "WordSize"       , 32                                              )]
        [Defaults    ( "WaitStates"     , 0                                               )]
        [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM                    |
                                          Runtime.MemoryAttributes.RandomAccessMemory     |
                                          Runtime.MemoryAttributes.InternalMemory         |
                                          Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.MM9691LP.RamMemoryHandler), HardwareModelAttribute.Kind.Memory)]
        public sealed class InternalRAM768KB : RamMemoryCategory
        {
        }

        [DependsOn(typeof(MM9691LP))]
        [DisplayName("Cache Controller")]
        [Defaults( "WordSize"   , 32 )]
        [Defaults( "WaitStates" , 0  )]
        [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.MM9691LP.CacheMemoryHandler), HardwareModelAttribute.Kind.Memory)]
        public sealed class CacheController : CacheControllerCategory
        {
            public override uint GetUncacheableAddress( uint address )
            {
                return Emulation.ArmProcessor.Chipset.MM9691LP.CacheMemoryHandler.GetUncacheableAddress( address );
            }
        }

        [AllowedOptions(typeof(MM9691LP.InternalRAM768KB))]
        [Defaults( "ConnectedToBus", typeof(MM9691LP.CacheController) )]
        public RamMemoryCategory RamChip;

        [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.MM9691LP), HardwareModelAttribute.Kind.PeripheralsGroup)]
        [Defaults( "ConnectedToBus", typeof(MM9691LP.CacheController))]
        public PeripheralCategory[] Peripherals;

        [AllowedOptions(typeof(MM9691LP.CacheController))]
        [Defaults( "ConnectedToBus", typeof(MM9691LP) )]
        public CacheControllerCategory Cache;
    }

    [DisplayName("LPC3180")]
    public sealed class LPC3180 : ProcessorCategory
    {
        [DependsOn(typeof(LPC3180))]
        [DisplayName("Internal 64KB Static RAM")]
        [Defaults    ( "BaseAddress"    , 0x08000000                                      )]
        [Defaults    ( "SizeInBytes"    , 64 * 1024                                       )]
        [Defaults    ( "WordSize"       , 32                                              )]
        [Defaults    ( "WaitStates"     , 0                                               )]
        [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM                    |
                                          Runtime.MemoryAttributes.RandomAccessMemory     |
                                          Runtime.MemoryAttributes.InternalMemory         |
                                          Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
////    [HardwareModel(typeof(Emulation.ArmProcessor.LPC3180.RamMemoryHandler), HardwareModelAttribute.Kind.Memory)]
        public sealed class InternalRAM64KB : RamMemoryCategory
        {
        }

        [AllowedOptions(typeof(LPC3180.InternalRAM64KB))]
        [Defaults( "ConnectedToBus", typeof(LPC3180))]
        public RamMemoryCategory RamChip;

////    [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.MM9691LP), HardwareModelAttribute.Kind.PeripheralsGroup)]
        [Defaults( "ConnectedToBus", typeof(LPC3180))]
        public PeripheralCategory[] Peripherals;

        [DisplayName("AHB Clock Frequency")]
        [LinkToConfigurationOption("LPC3180__AHBClockFrequency")]
        public ulong AHBClockFrequency;

        [DisplayName("Use wide DRAM Bus")]
        [LinkToConfigurationOption("LPC3180__DRAMWideBus")]
        public bool DRAMWideBus;
    }

    [DisplayName("PXA27x")]
    public sealed class PXA27x : ProcessorCategory
    {
        [DependsOn(typeof(PXA27x))]
        [DisplayName("Internal 256KB Static RAM")]
        [Defaults    ( "BaseAddress"    , 0x5C000000                                      )]
        [Defaults    ( "SizeInBytes"    , 256 * 1024                                      )]
        [Defaults    ( "WordSize"       , 32                                              )]
        [Defaults    ( "WaitStates"     , 0                                               )]
        [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM                    |
                                          Runtime.MemoryAttributes.RandomAccessMemory     |
                                          Runtime.MemoryAttributes.InternalMemory         |
                                          Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.PXA27x.RamMemoryHandler), HardwareModelAttribute.Kind.Memory)]
        public sealed class InternalRAM256KB : RamMemoryCategory
        {
        }

        [DependsOn( typeof( PXA27x ) )]
        [DisplayName( "Internal 32MB SDRAM" )]
        [Defaults( "BaseAddress", 0x20000000 )]
        [Defaults( "SizeInBytes", 32 * 1024 * 1024 )]
        [Defaults( "WordSize", 32 )]
        [Defaults( "WaitStates", 0 )]
        [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM |
                                          Runtime.MemoryAttributes.RandomAccessMemory |
                                          Runtime.MemoryAttributes.ExternalMemory |
                                          Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [HardwareModel( typeof( Emulation.ArmProcessor.Chipset.PXA27x.RamMemoryHandler ), HardwareModelAttribute.Kind.Memory )]
        public sealed class InternalSDRAM32MB : RamMemoryCategory
        {
        }


        [DependsOn( typeof( PXA27x ) )]
        [DisplayName( "Internal 32MB FLASH" )]
        [Defaults( "BaseAddress", 0x02000000 )]
        [Defaults( "SizeInBytes", 32 * 1024 * 1024 )]
        [Defaults( "WordSize", 32 )]
        [Defaults( "WaitStates", 0 )]
        [EnumDefaults( "Characteristics", Runtime.MemoryAttributes.RAM |
                                          Runtime.MemoryAttributes.RandomAccessMemory |
                                          Runtime.MemoryAttributes.InternalMemory |
                                          Runtime.MemoryAttributes.ConfiguredAtEntryPoint )]
        [HardwareModel( typeof( Emulation.ArmProcessor.Chipset.PXA27x.RamMemoryHandler ), HardwareModelAttribute.Kind.Memory )]
        public sealed class InternalFlash32MB : RamMemoryCategory
        {
        }

        [DependsOn(typeof(PXA27x))]
        [DisplayName("Cache Controller")]
        [Defaults( "WordSize"   , 32 )]
        [Defaults( "WaitStates" , 0  )]
        [HardwareModel(typeof(Emulation.ArmProcessor.Chipset.PXA27x.CacheMemoryHandler), HardwareModelAttribute.Kind.Memory)]
        public sealed class CacheController : CacheControllerCategory
        {
            public override uint GetUncacheableAddress( uint address )
            {
                return Emulation.ArmProcessor.Chipset.PXA27x.CacheMemoryHandler.GetUncacheableAddress( address );
            }
        }

        [AllowedOptions(typeof(PXA27x.InternalRAM256KB))]
        [Defaults( "ConnectedToBus", typeof( PXA27x.CacheController ) )]
        public RamMemoryCategory RamChip;

        [AllowedOptions( typeof( PXA27x.InternalFlash32MB ) )]
        [Defaults( "ConnectedToBus", typeof( PXA27x.CacheController ) )]
        public RamMemoryCategory FlashChip;

        [AllowedOptions( typeof( PXA27x.InternalSDRAM32MB ) )]
        [Defaults( "ConnectedToBus", typeof( PXA27x.CacheController ) )]
        public RamMemoryCategory SDRamChip;

        [HardwareModel( typeof( Emulation.ArmProcessor.Chipset.PXA27x ), HardwareModelAttribute.Kind.PeripheralsGroup )]
        [Defaults( "ConnectedToBus", typeof( PXA27x.CacheController ) )]
        public PeripheralCategory[] Peripherals;

        [AllowedOptions( typeof( PXA27x.CacheController ) )]
        [Defaults( "ConnectedToBus", typeof( PXA27x ) )]
        public CacheControllerCategory Cache;

////    [DisplayName("AHB Clock Frequency")]
////    [LinkToConfigurationOption("LPC3180__AHBClockFrequency")]
////    public ulong AHBClockFrequency;

////    [DisplayName("Use wide DRAM Bus")]
////    [LinkToConfigurationOption("LPC3180__DRAMWideBus")]
////    public bool DRAMWideBus;
    }

    
    [DisplayName("mBed")]
    public sealed class mBed : ProcessorCategory
    {
    }
}
