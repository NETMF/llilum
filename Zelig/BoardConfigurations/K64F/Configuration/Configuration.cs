//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.BoardConfigurations
{
    using Microsoft.Zelig.Configuration.Environment;

    [DisplayName("K64F MBED Hosted")]
    public sealed class K64FMBEDHosted : ProductCategory
    {
        [AllowedOptions(typeof(mBed))]
        [Defaults("CoreClockFrequency", 120000000UL)]
        [Defaults("RealTimeClockFrequency", 1000000UL)]
        public ProcessorCategory Processor;
    }

    [DisplayName("CMSIS-Core Memory Map for K64F")]
    public sealed class K64FCMSISCoreMemoryMap : MemoryMapCategory
    {
    }

    [DisplayName("LLVM Hosted Compilation for K64F")]
    [Defaults("Platform", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV7M_VFP))]
    [Defaults("CallingConvention", typeof(Microsoft.Zelig.Configuration.Environment.Abstractions.Architectures.ArmV7MCallingConvention))]
    [Defaults("Product", typeof(K64FMBEDHosted))]
    [Defaults("MemoryMap", typeof(K64FCMSISCoreMemoryMap))]
    public sealed class K64FMBEDHostedCompilationSetup : CompilationSetupCategory
    {
    }
}
