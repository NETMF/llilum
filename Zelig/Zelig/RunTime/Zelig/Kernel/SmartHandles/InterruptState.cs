//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.SmartHandles
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    //
    // These methods are not really implemented as internal call,
    // a open class will provide the implementation, based on the target platform.
    //
    public struct InterruptState : IDisposable
    {
        //
        // Constructor Methods
        //

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern InterruptState( uint cpsr );

        //
        // Helper Methods
        //

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern void Dispose();

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern void Toggle();

        //--//

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern InterruptState Disable();

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern InterruptState DisableAll();

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern InterruptState Enable();

        [MethodImpl( MethodImplOptions.InternalCall )]
        public static extern InterruptState EnableAll();

        //
        // Access Methods
        //

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern uint GetPreviousState();

        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern HardwareException GetCurrentExceptionMode();
    }
}
