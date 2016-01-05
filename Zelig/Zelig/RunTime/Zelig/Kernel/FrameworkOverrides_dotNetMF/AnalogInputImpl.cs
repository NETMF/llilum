using System;
using System.Runtime.CompilerServices;

using NETMF = Microsoft.SPOT.Hardware;


namespace Microsoft.Zelig.Runtime
{
    [ExtendClass(typeof(NETMF.AnalogInput), NoConstructors= true )]
    public class AnalogInputImpl
    {

        //--//
        
        protected static void Initialize(NETMF.Cpu.AnalogChannel channel, int precisionInBits)
        {

        }
        
        protected static void Uninitialize(NETMF.Cpu.AnalogChannel channel)
        {

        }       
    }
}


