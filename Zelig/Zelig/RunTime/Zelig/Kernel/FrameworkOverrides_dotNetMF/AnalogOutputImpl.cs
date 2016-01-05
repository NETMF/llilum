using System;
using System.Runtime.CompilerServices;

using NETMF = Microsoft.SPOT.Hardware;


namespace Microsoft.Zelig.Runtime
{
    [ExtendClass(typeof(NETMF.AnalogOutput), NoConstructors= true )]
    public class AnalogOutputImpl
    { 

        //--//
        
        protected static void Initialize(NETMF.Cpu.AnalogOutputChannel channel, int precisionInBits)
        {

        }
    
        protected static void Uninitialize(NETMF.Cpu.AnalogOutputChannel channel)
        {

        }
    }
}
