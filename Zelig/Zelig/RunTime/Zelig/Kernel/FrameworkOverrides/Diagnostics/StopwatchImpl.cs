//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Diagnostics.Stopwatch), NoConstructors=true)]
    public class StopwatchImpl
    {
        static uint s_lastCounter;
        static uint s_highPart;

        public static bool QueryPerformanceFrequency( out long Frequency )
        {
            Frequency = (long)Peripherals.Instance.GetPerformanceCounterFrequency();
            return true;
        }

        public static void QueryPerformanceCounter( out long timestamp )
        {
            uint counter = Peripherals.Instance.ReadPerformanceCounter();

            if(counter < s_lastCounter) // Detected wrap around.
            {
                s_highPart++;
            }

            s_lastCounter = counter;

            timestamp = (long)(MathImpl.InsertHighPart( s_highPart ) + counter);
        }
    }
}

