//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Support.mbed
{
    using System.Runtime.InteropServices;
    
    //--//
    
    public static class Utilities
    {
        [DllImport( "C" )]
        public static unsafe extern int GetANumber( );
        
        [DllImport( "C" )]
        public static unsafe extern void BreakWithTrap( );
        
        [DllImport( "C" )]
        public static unsafe extern void Breakpoint( uint n );
    }
}
