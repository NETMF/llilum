//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Support.mbed
{
    using System;
    using System.Runtime.InteropServices;

    //--//

    public delegate void ThreadStart( UIntPtr argument ); 

    public static class Threading
    {
        [DllImport( "C" )]
        public static extern UIntPtr CreateNativeContext( UIntPtr entryPoint, UIntPtr stack, int stackSize );
        
        [DllImport( "C" )]
        public static extern void Yield( UIntPtr nativeContext );
        
        [DllImport( "C" )]
        public static extern void Retire( UIntPtr nativeContext );

        [DllImport( "C" )]
        public static extern void SwitchToContext( UIntPtr nativeContext );

        [DllImport( "C" )]
        public static extern UIntPtr GetPriority( UIntPtr nativeContext );

        [DllImport( "C" )]
        public static extern void SetPriority( UIntPtr nativeContext, UIntPtr priority );
    }
}
