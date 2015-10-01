//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;

    #region LPC1768

    [DisplayName( "LPC1768 MBED Hosted" )]
    public sealed class LPC1768MBEDHosted : ProductCategory
    {
        [AllowedOptions( typeof( mBed ) )]
        [Defaults( "CoreClockFrequency", 96000000UL )]
        [Defaults( "RealTimeClockFrequency", 1000000UL )]
        public ProcessorCategory Processor;
    }

    [DisplayName( "CMSIS-Core Memory Map for LPC1768" )]
    public sealed class LPC1768CMSISCoreMemoryMap : MemoryMapCategory
    {
    }

    [DisplayName( "LLVM Hosted Compilation for LPC1768" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMPlatform ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMCallingConvention ) )]
    [Defaults( "Product", typeof( LPC1768MBEDHosted ) )]
    [Defaults( "MemoryMap", typeof( LPC1768CMSISCoreMemoryMap ) )]
    public sealed class LPC1768MBEDHostedCompilationSetup : CompilationSetupCategory
    {
    }

    #endregion // LPC1768

    
    #region K64F

    [DisplayName( "K64F MBED Hosted" )]
    public sealed class K64FMBEDHosted : ProductCategory
    {
        [AllowedOptions( typeof( mBed ) )]
        [Defaults( "CoreClockFrequency", 120000000UL )]
        [Defaults( "RealTimeClockFrequency", 1000000UL )]
        public ProcessorCategory Processor;
    }

    [DisplayName( "CMSIS-Core Memory Map for K64F" )]
    public sealed class K64FCMSISCoreMemoryMap : MemoryMapCategory
    {
    }

    [DisplayName( "LLVM Hosted Compilation for K64F" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMPlatform ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMCallingConvention ) )]
    [Defaults( "Product", typeof( K64FMBEDHosted ) )]
    [Defaults( "MemoryMap", typeof( K64FCMSISCoreMemoryMap ) )]
    public sealed class K64FMBEDHostedCompilationSetup : CompilationSetupCategory
    {
    }

    #endregion // K64F
}
