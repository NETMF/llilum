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
        
        [DllImport("C")]
        public static unsafe extern void DebugLog0(char* message, uint length);

        [DllImport("C")]
        public static unsafe extern void DebugLog1(char* message, uint length, int p1);

        [DllImport("C")]
        public static unsafe extern void DebugLog2(char* message, uint length, int p1, int p2);

        [DllImport("C")]
        public static unsafe extern void DebugLog3(char* message, uint length, int p1, int p2, int p3);

        [DllImport("C")]
        public static unsafe extern void DebugLog4(char* message, uint length, int p1, int p2, int p3, int p4);

        [DllImport("C")]
        public static unsafe extern void DebugLog5(char* message, uint length, int p1, int p2, int p3, int p4, int p5);
    }
}
