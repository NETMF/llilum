//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Configuration.Environment;


    [DisplayName("STM32L152")]
    public sealed class STM32L152SoC : ProcessorCategory
    {
        [DependsOn(typeof(STM32L152SoC))]
        [DisplayName("Internal 80KB Static RAM")]
        [Defaults("BaseAddress", 0x20000000)]
        [Defaults("SizeInBytes", 80 * 1024)]
        [Defaults("WordSize", 32)]
        [Defaults("WaitStates", 0)]
        [EnumDefaults("Characteristics", MemoryAttributes.RAM |
                                          MemoryAttributes.RandomAccessMemory |
                                          MemoryAttributes.InternalMemory |
                                          MemoryAttributes.ConfiguredAtEntryPoint)]
        public sealed class InternalRAM80KB : RamMemoryCategory
        {
        }


        [DependsOn(typeof(STM32L152SoC))]
        [DisplayName("Internal 512KB FLASH")]
        [Defaults("BaseAddress", 0x00000000)]
        [Defaults("SizeInBytes", 512 * 1024)]
        [Defaults("WordSize", 32)]
        [Defaults("WaitStates", 0)]
        [EnumDefaults("Characteristics", MemoryAttributes.FLASH |
                                          MemoryAttributes.RandomAccessMemory |
                                          MemoryAttributes.InternalMemory |
                                          MemoryAttributes.ConfiguredAtEntryPoint)]
        public sealed class InternalFlash512KB : FlashMemoryCategory
        {
        }
    }

    //--//
    //--//
    //--//

    [DisplayName("Memory Map for STM32L152")]
    public sealed class STM32L152MemoryMap : MemoryMapCategory
    {

        [MergeEnumDefaults("Characteristics", MemoryAttributes.InternalMemory |
                                               MemoryAttributes.RandomAccessMemory |
                                               MemoryAttributes.ConfiguredAtEntryPoint)]
        [MemorySection(MemoryUsage.Stack |
                        MemoryUsage.Heap |
                        MemoryUsage.DataRW |
                        MemoryUsage.Code)]
        [AllowedOptions(typeof(STM32L152SoC.InternalRAM80KB))]
        [Defaults("BaseAddress", 0x20000000U)]
        public RamMemoryCategory InternalRamChip;

        [MergeEnumDefaults("Characteristics", MemoryAttributes.InternalMemory |
                                               MemoryAttributes.RandomAccessMemory |
                                               MemoryAttributes.ConfiguredAtEntryPoint |
                                               MemoryAttributes.LoadedAtEntrypoint)]
        [MemorySection(MemoryUsage.Bootstrap |
                        MemoryUsage.Code |
                        MemoryUsage.DataRO |
                        MemoryUsage.VectorsTable |
                        MemoryUsage.Relocation)]
        [AllowedOptions(typeof(STM32L152SoC.InternalFlash512KB))]
        [Defaults("BaseAddress", 0x00000000U)]
        public FlashMemoryCategory InternalFlashChip;
    }

    //--//
    //--//
    //--//

    [DisplayName("STM32L152 MBED")]
    public sealed class STM32L152 : ProductCategory
    {
        [AllowedOptions(typeof(STM32L152SoC))]
        [Defaults("CoreClockFrequency", 32000000UL)]
        [Defaults("RealTimeClockFrequency", 1000000UL)]
        public STM32L152SoC Processor;

        //--//

        [AllowedOptions(typeof(STM32L152SoC.InternalRAM80KB))]
        [Defaults("BaseAddress", 0x20000000U)]
        public RamMemoryCategory InternalRam;

        [AllowedOptions(typeof(STM32L152SoC.InternalFlash512KB))]
        [Defaults("BaseAddress", 0x00000000U)]
        public FlashMemoryCategory InternalFlashChip;
    }

    //--//
    //--//
    //--//

    [DisplayName("LLVM Compilation for STM32L152")]
    [Defaults("Platform", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7M))]
    [Defaults("CallingConvention", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7MCallingConvention))]
    [Defaults("Product", typeof(STM32L152))]
    [Defaults("MemoryMap", typeof(STM32L152MemoryMap))]
    public sealed class STM32L152MBEDCompilationSetup : CompilationSetupCategory
    {
    }
}
