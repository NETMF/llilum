//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction
{
    using System.Runtime.InteropServices;

    public static class Debug
    {
        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_DEBUG_Break( uint code );

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_DEBUG_LogText( char* text, int textLength );
    }
}
