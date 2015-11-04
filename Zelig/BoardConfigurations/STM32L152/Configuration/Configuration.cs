//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Configuration.Environment;

    [DisplayName("STM32L152 MBED Hosted")]
    public sealed class STM32L152MBEDHosted : ProductCategory
    {
        [AllowedOptions(typeof(mBed))]
        [Defaults("CoreClockFrequency", 32000000UL)]
        [Defaults("RealTimeClockFrequency", 1000000UL)]
        public ProcessorCategory Processor;
    }

    [DisplayName("CMSIS-Core Memory Map for STM32L152")]
    public sealed class STM32L152CMSISCoreMemoryMap : MemoryMapCategory
    {
    }

    [DisplayName("LLVM Hosted Compilation for STM32L152")]
    [Defaults("Platform", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV7M))]
    [Defaults("CallingConvention", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV7MCallingConvention))]
    [Defaults("Product", typeof(STM32L152MBEDHosted))]
    [Defaults("MemoryMap", typeof(STM32L152CMSISCoreMemoryMap))]
    public sealed class STM32L152MBEDHostedCompilationSetup : CompilationSetupCategory
    {
    }
}
