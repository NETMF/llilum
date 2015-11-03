//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Configuration.Environment;

    [DisplayName("LPC1768 MBED Hosted")]
    public sealed class LPC1768MBEDHosted : ProductCategory
    {
        [AllowedOptions(typeof(mBed))]
        [Defaults("CoreClockFrequency", 96000000UL)]
        [Defaults("RealTimeClockFrequency", 1000000UL)]
        public ProcessorCategory Processor;
    }

    [DisplayName("CMSIS-Core Memory Map for LPC1768")]
    public sealed class LPC1768CMSISCoreMemoryMap : MemoryMapCategory
    {
    }

    [DisplayName("LLVM Hosted Compilation for LPC1768")]
    [Defaults("Platform", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV7M))]
    [Defaults("CallingConvention", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV7MCallingConvention))]
    [Defaults("Product", typeof(LPC1768MBEDHosted))]
    [Defaults("MemoryMap", typeof(LPC1768CMSISCoreMemoryMap))]
    public sealed class LPC1768MBEDHostedCompilationSetup : CompilationSetupCategory
    {
    }
}
