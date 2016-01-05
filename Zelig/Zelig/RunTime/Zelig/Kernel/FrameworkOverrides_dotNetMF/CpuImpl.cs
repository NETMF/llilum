using System;

using NETMF = Microsoft.SPOT.Hardware;


namespace Microsoft.Zelig.Runtime
{
    [ExtendClass(typeof(NETMF.Cpu), NoConstructors = true)]
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


