//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;

    #region LLVM Platform

    [DisplayName( "LLVM Hosted" )]
    public sealed class LLVMHosted : ProductCategory
    {
        [Defaults( "CoreClockFrequency", 416000000UL )]
        [Defaults( "PeripheralsClockFrequency", 13000000UL )]
        [Defaults( "RealTimeClockFrequency", 1000000UL )]
        public ProcessorCategory Processor;
    }

    [DisplayName( "LLVM Hosted Compilation" )]
    [Defaults( "Platform", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMPlatform ) )]
    [Defaults( "CallingConvention", typeof( Microsoft.Zelig.Configuration.Environment.Abstractions.LLVMCallingConvention ) )]
    [Defaults( "Product", typeof( LLVMHosted ) )]
    [Defaults( "MemoryMap", typeof( LLVMHostedMemoryMap ) )]
    public sealed class LLVMHostedCompilationSetup : CompilationSetupCategory
    {
    }

    [DisplayName( "LLVM Hosted Memory Map" )]
    public sealed class LLVMHostedMemoryMap : MemoryMapCategory
    {
    }

    #endregion //LLVM Platform
}
