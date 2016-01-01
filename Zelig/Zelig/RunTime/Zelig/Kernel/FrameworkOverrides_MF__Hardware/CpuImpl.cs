using System;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Microsoft.Zelig.Runtime
{
    [ExtendClass(typeof(Microsoft.SPOT.Hardware.Cpu))]
    public static class CpuImpl
    {
        public static uint SystemClock
        {
            get
            {
                return (uint)Configuration.CoreClockFrequency;
            }
        }

        public static uint SlowClock
        {
            get
            {
                return (uint)Configuration.RealTimeClockFrequency;
            }
        }

        public static TimeSpan GlitchFilterTime
        {
            get
            {
                return new TimeSpan( 0 );
            }
            
            set
            {
                throw new NotImplementedException( ); 
            }
        }
    }
}


