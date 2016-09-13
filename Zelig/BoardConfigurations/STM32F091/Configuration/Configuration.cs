//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Configuration.Environment;

    [DisplayName("STM32F091")]
    public sealed class STM32F091SoC : ProcessorCategory
    {
        [DependsOn(typeof(STM32F091SoC))]
        [DisplayName("Internal 32KB Static RAM")]
        [Defaults("BaseAddress", 0x20000000U)]
        [Defaults("SizeInBytes", 32 * 1024)]
        [Defaults("WordSize", 32)]
        [Defaults("WaitStates", 0)]
        [EnumDefaults("Characteristics",  MemoryAttributes.RAM |
                                          MemoryAttributes.RandomAccessMemory |
                                          MemoryAttributes.InternalMemory |
                                          MemoryAttributes.ConfiguredAtEntryPoint)]
        public sealed class InternalRAM32KB : RamMemoryCategory
        {
        }


        [DependsOn(typeof(STM32F091SoC))]
        [DisplayName("Internal 256KB FLASH")]
        [Defaults("BaseAddress", 0x08000000U)]
        [Defaults("SizeInBytes", 256 * 1024)]
        [Defaults("WordSize", 32)]
        [Defaults("WaitStates", 0)]
        [EnumDefaults("Characteristics",  MemoryAttributes.FLASH |
                                          MemoryAttributes.RandomAccessMemory |
                                          MemoryAttributes.InternalMemory |
                                          MemoryAttributes.ConfiguredAtEntryPoint)]
        public sealed class InternalFlash256KB : FlashMemoryCategory
        {
        }
    }

    //--//
    //--//
    //--//

    [DisplayName("Memory Map for STM32F091")]
    public sealed class STM32F091MemoryMap : MemoryMapCategory
    {

        [MergeEnumDefaults("Characteristics", MemoryAttributes.InternalMemory |
                                               MemoryAttributes.RandomAccessMemory |
                                               MemoryAttributes.ConfiguredAtEntryPoint)]
        [MemorySection(MemoryUsage.Stack |
                        MemoryUsage.Heap |
                        MemoryUsage.DataRW |
                        MemoryUsage.Code)]
        [AllowedOptions(typeof(STM32F091SoC.InternalRAM32KB))]
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
        [AllowedOptions(typeof(STM32F091SoC.InternalFlash256KB))]
        [Defaults("BaseAddress", 0x08000000U)]
        public FlashMemoryCategory InternalFlashChip;
    }

    //--//
    //--//
    //--//

    [DisplayName("STM32F091 MBED")]
    public sealed class STM32F091 : ProductCategory
    {
        [AllowedOptions(typeof(STM32F091SoC))]
        [Defaults("CoreClockFrequency", 48000000UL)]
        [Defaults("RealTimeClockFrequency", 1000000UL)]
        [Defaults("DefaultThreadPoolThreads", 1)]
        [Defaults("DefaultTimerPoolThreads", 1)]
        public STM32F091SoC Processor;

        //--//

        [AllowedOptions(typeof(STM32F091SoC.InternalRAM32KB))]
        [Defaults("BaseAddress", 0x20000000U)]
        public RamMemoryCategory InternalRam;

        [AllowedOptions(typeof(STM32F091SoC.InternalFlash256KB))]
        [Defaults("BaseAddress", 0x08000000U)]
        public FlashMemoryCategory InternalFlashChip;
    }

    //--//
    //--//
    //--//

    [DisplayName("LLVM Compilation for STM32F091")]
    [Defaults("Platform", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV6M))]
    [Defaults("CallingConvention", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.LlvmForArmV7MCallingConvention))]
    [Defaults("Product", typeof(STM32F091))]
    [Defaults("MemoryMap", typeof(STM32F091MemoryMap))]
    public sealed class STM32F091MBEDCompilationSetup : CompilationSetupCategory
    {
    }

}
