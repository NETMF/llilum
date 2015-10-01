//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.NohauLPC3180Loader
{
    using System;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class Peripherals : RT.Peripherals
    {
        public override void Initialize()
        {
        }

        public override void Activate()
        {
        }

        public override void EnableInterrupt( uint index )
        {
        }

        public override void DisableInterrupt( uint index )
        {
        }

        public override void CauseInterrupt()
        {
        }

        public override void WaitForInterrupt()
        {
        }

        public override void ContinueUnderNormalInterrupt( Continuation dlg )
        {
        }

        public override void ProcessInterrupt()
        {
        }

        public override void ProcessFastInterrupt()
        {
        }

        public override ulong GetPerformanceCounterFrequency()
        {
            return 0;
        }

        public override unsafe uint ReadPerformanceCounter()
        {
            return 0;
        }
    }
}
