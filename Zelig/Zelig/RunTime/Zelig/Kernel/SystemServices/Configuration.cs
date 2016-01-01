//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public static class Configuration
    {
        //
        // Calls to this method get redirected to the EntryPoint of the application.
        //
        [TS.WellKnownMethod( "Configuration_ExecuteApplication" )]
        public static extern void ExecuteApplication();

        public static ulong CoreClockFrequency
        {
            [ConfigurationOption("System__CoreClockFrequency")]
            get
            {
                return 0;
            }
        }

        public static ulong RealTimeClockFrequency
        {
            [ConfigurationOption("System__RealTimeClockFrequency")]
            get
            {
                return 0;
            }
        }

        public static int DefaultThreadPooThreads
        {
            [ConfigurationOption("System__Runtime_DefaultThreadPooThreads")]
            get
            {
                return 2;
            }
        }

        public static int DefaultTimerPooThreads
        {
            [ConfigurationOption("System__Runtime_DefaultTimerPooThreads")]
            get
            {
                return 1;
            }
        }
    }
}
