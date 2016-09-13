//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Configuration.Environment;

    [DisplayName("K64F")]
    public sealed class K64FSoC : ProcessorCategory
    {
        [DependsOn(typeof(K64FSoC))]
        [DisplayName("Internal 256KB Static RAM")]
        [Defaults    ( "BaseAddress"    , 0x1FFF0000U                             )]
        [Defaults    ( "SizeInBytes"    , 256 * 1024                              )]
        [Defaults    ( "WordSize"       , 32                                      )]
        [Defaults    ( "WaitStates"     , 0                                       )]
        [EnumDefaults( "Characteristics", MemoryAttributes.RAM                    |
                                          MemoryAttributes.RandomAccessMemory     |
                                          MemoryAttributes.InternalMemory         |
                                          MemoryAttributes.ConfiguredAtEntryPoint )]
        public sealed class InternalRAM256KB : RamMemoryCategory
        {
        }


        [DependsOn( typeof( K64FSoC ) )]
        [DisplayName( "Internal 512KB FLASH" )]
        [Defaults( "BaseAddress", 0x00000000 )]
        [Defaults( "SizeInBytes", 1024 * 1024 )]
        [Defaults( "WordSize"   , 32 )]
        [Defaults( "WaitStates" , 0 )]
        [EnumDefaults( "Characteristics", MemoryAttributes.FLASH                    |
                                          MemoryAttributes.RandomAccessMemory       |
                                          MemoryAttributes.InternalMemory           |
                                          MemoryAttributes.ConfiguredAtEntryPoint   )]
        public sealed class InternalFlash1024KB : FlashMemoryCategory
        {
        }
    }

    //--//
    //--//
    //--//

    [DisplayName( "Memory Map for K64F" )]
    public sealed class K64FMemoryMap : MemoryMapCategory
    {

        [MergeEnumDefaults( "Characteristics", MemoryAttributes.InternalMemory         |
                                               MemoryAttributes.RandomAccessMemory     |
                                               MemoryAttributes.ConfiguredAtEntryPoint )]
        [MemorySection( MemoryUsage.Stack        |
                        MemoryUsage.Heap         |
                        MemoryUsage.DataRW       |
                        MemoryUsage.Code )]
        [AllowedOptions( typeof( K64FSoC.InternalRAM256KB ) )]
        [Defaults( "BaseAddress", 0x1FFF0000U )]
        public RamMemoryCategory InternalRamChip;

        [MergeEnumDefaults( "Characteristics", MemoryAttributes.InternalMemory         |
                                               MemoryAttributes.RandomAccessMemory     |
                                               MemoryAttributes.ConfiguredAtEntryPoint |
                                               MemoryAttributes.LoadedAtEntrypoint     )]
        [MemorySection( MemoryUsage.Bootstrap    |
                        MemoryUsage.Code         |
                        MemoryUsage.DataRO       |
                        MemoryUsage.VectorsTable |
                        MemoryUsage.Relocation   )]
        [AllowedOptions( typeof( K64FSoC.InternalFlash1024KB ) )]
        [Defaults( "BaseAddress", 0x00000000U )]
        public FlashMemoryCategory InternalFlashChip;
    }

    //--//
    //--//
    //--//

    [DisplayName("K64F MBED")]
    public sealed class K64F : ProductCategory
    {
        [AllowedOptions(typeof(K64FSoC))]
        [Defaults("CoreClockFrequency"    , 120000000UL)]
        [Defaults("RealTimeClockFrequency", 1000000UL  )]
        [Defaults("DefaultThreadPoolThreads", 3)]
        [Defaults("DefaultTimerPoolThreads" , 2)]
        public K64FSoC Processor;

        //--//

        [AllowedOptions( typeof( K64FSoC.InternalRAM256KB ) )]
        [Defaults( "BaseAddress", 0x1FFF0000U )]
        public RamMemoryCategory InternalRam;

        [AllowedOptions( typeof( K64FSoC.InternalFlash1024KB ) )]
        [Defaults( "BaseAddress", 0x00000000U )]
        public FlashMemoryCategory InternalFlashChip;
    }

    //--//
    //--//
    //--//
    [DisplayName("LLVM Compilation for K64F")]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7M_VFP ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7MCallingConvention ) )]
    [Defaults("Product", typeof(K64F))]
    [Defaults("MemoryMap", typeof(K64FMemoryMap))]
    public sealed class K64FMBEDCompilationSetup : CompilationSetupCategory
    {
    }

}
