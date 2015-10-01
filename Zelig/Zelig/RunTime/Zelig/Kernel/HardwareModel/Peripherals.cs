//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;


    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class Peripherals
    {
        public delegate void Continuation();

        //
        // Helper Methods
        //

        public abstract void Initialize();

        public abstract void Activate();

        public abstract void EnableInterrupt( uint index );

        public abstract void DisableInterrupt( uint index );

        public abstract void CauseInterrupt();

        public abstract void ContinueUnderNormalInterrupt( Continuation dlg );

        public abstract void WaitForInterrupt();

        public abstract void ProcessInterrupt();

        public abstract void ProcessFastInterrupt();

        //--//

        public abstract ulong GetPerformanceCounterFrequency();

        public abstract uint ReadPerformanceCounter();

        //
        // Access Methods
        //

        public static extern Peripherals Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
