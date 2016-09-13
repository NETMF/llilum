//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Configuration.Environment;
    

    [DisplayName("LPC1768")]
    public sealed class LPC1768SoC : ProcessorCategory
    {
        [DependsOn(typeof(LPC1768SoC))]
        [DisplayName("Internal 32KB Static RAM")]
        [Defaults    ( "BaseAddress"    , 0x10000000                              )]
        [Defaults    ( "SizeInBytes"    , 32 * 1024                               )]
        [Defaults    ( "WordSize"       , 32                                      )]
        [Defaults    ( "WaitStates"     , 0                                       )]
        [EnumDefaults( "Characteristics", MemoryAttributes.RAM                    |
                                          MemoryAttributes.RandomAccessMemory     |
                                          MemoryAttributes.InternalMemory         |
                                          MemoryAttributes.ConfiguredAtEntryPoint )]
        public sealed class InternalRAM32KB : RamMemoryCategory
        {
        }


        [DependsOn( typeof( LPC1768SoC ) )]
        [DisplayName( "Internal 512KB FLASH" )]
        [Defaults( "BaseAddress", 0x00000000 )]
        [Defaults( "SizeInBytes", 512 * 1024 )]
        [Defaults( "WordSize"   , 32 )]
        [Defaults( "WaitStates" , 0 )]
        [EnumDefaults( "Characteristics", MemoryAttributes.FLASH                    |
                                          MemoryAttributes.RandomAccessMemory       |
                                          MemoryAttributes.InternalMemory           |
                                          MemoryAttributes.ConfiguredAtEntryPoint   )]
        public sealed class InternalFlash512KB : FlashMemoryCategory
        {
        }
    }
    
    //--//
    //--//
    //--//
        
    [DisplayName( "Memory Map for LPC1768" )]
    public sealed class LPC1768MemoryMap : MemoryMapCategory
    {

        [MergeEnumDefaults( "Characteristics", MemoryAttributes.InternalMemory         |
                                               MemoryAttributes.RandomAccessMemory     |
                                               MemoryAttributes.ConfiguredAtEntryPoint )]
        [MemorySection( MemoryUsage.Stack        |
                        MemoryUsage.Heap         |
                        MemoryUsage.DataRW       |
                        MemoryUsage.Code )]
        [AllowedOptions( typeof( LPC1768SoC.InternalRAM32KB ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
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
        [AllowedOptions( typeof( LPC1768SoC.InternalFlash512KB ) )]
        [Defaults( "BaseAddress", 0x00000000U )]
        public FlashMemoryCategory InternalFlashChip;
    }
    
    //--//
    //--//
    //--//

    [DisplayName("LPC1768 MBED")]
    public sealed class LPC1768 : ProductCategory
    {
        [AllowedOptions(typeof(LPC1768SoC))]
        [Defaults("CoreClockFrequency"     , 96000000UL)]
        [Defaults("RealTimeClockFrequency" , 1000000UL )]
        [Defaults("DefaultThreadPoolThreads", 2)]
        [Defaults("DefaultTimerPoolThreads" , 1)]
        public LPC1768SoC Processor;

        //--//
        
        [AllowedOptions( typeof( LPC1768SoC.InternalRAM32KB ) )]
        [Defaults( "BaseAddress", 0x10000000U )]
        public RamMemoryCategory InternalRam;

        [AllowedOptions( typeof( LPC1768SoC.InternalFlash512KB ) )]
        [Defaults( "BaseAddress", 0x00000000U )]
        public FlashMemoryCategory InternalFlashChip;
    }
    
    //--//
    //--//
    //--//

    [DisplayName("LLVM Compilation for LPC1768")]
    [Defaults("Platform", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7M))]
    [Defaults("CallingConvention", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7MCallingConvention))]
    [Defaults("Product", typeof(LPC1768))]
    [Defaults("MemoryMap", typeof(LPC1768MemoryMap))]
    public sealed class LPC1768MBEDCompilationSetup : CompilationSetupCategory
    {
    }
}
