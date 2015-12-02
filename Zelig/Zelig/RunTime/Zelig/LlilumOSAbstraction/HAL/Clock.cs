//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.HAL
{
    using System;
    using System.Runtime.InteropServices;

    public static class Clock
    {
        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_CLOCK_GetTicks( );

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_CLOCK_GetClockFrequency( );

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_CLOCK_GetPerformanceCounter( );

        [DllImport( "C" )]
        public static unsafe extern ulong LLOS_CLOCK_GetPerformanceCounterFrequency( );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_CLOCK_DelayCycles(uint cycles);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_CLOCK_Delay(uint microSeconds);
    }
}
