//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv5
{
    using System;

    public abstract partial class ProcessorARMv5 : ARMv4.ProcessorARMv4
    {
        //--//

        public override unsafe void FlushCacheLine( UIntPtr target )
        {
            ARMv4.Coprocessor15.CleanDCache     ( target );
            ARMv4.Coprocessor15.DrainWriteBuffer(        );
            ARMv4.Coprocessor15.InvalidateICache( target );
        }

        //--//

        [NoInline]
        public static void EnableRunFastMode()
        {
        }

        protected static void InitializeCache()
        {
            ARMv4.Coprocessor15.InvalidateICache();

            ARMv4.Coprocessor15.InvalidateDCache();

            //
            // Enable ICache
            //
            ARMv4.Coprocessor15.SetControlRegisterBits( ARMv4.Coprocessor15.c_ControlRegister__ICache );
        }
    }
}
