//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.LLVMHosted
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using Microsoft.Zelig.Runtime;

    public sealed class Device : RT.Device
    {

        public override void PreInitializeProcessorAndMemory( )
        {
        }

        public override void MoveCodeToProperLocation( )
        {
            throw new Exception( "MoveCodeToProperLocation not implemented" );
        }
    }
}
