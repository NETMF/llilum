﻿//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F091
{
    using RT   = Microsoft.Zelig.Runtime;
    using LLOS = Zelig.LlilumOSAbstraction.API;

    public sealed class Device : Microsoft.CortexM0OnMBED.Device
    {
        [RT.MemoryUsage(RT.MemoryUsage.Stack, ContentsUninitialized = true, AllocateFromHighAddress = true)]
        static readonly uint[] s_bootstrapStackSTM32F091 = new uint[ 1024 / sizeof( uint ) ]; 

        //
        // Access Methods
        //

        public override uint[] BootstrapStack
        {
            get
            {
                return s_bootstrapStackSTM32F091;
            }
        }
    }
}
