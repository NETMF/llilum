using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CortexM0OnMBED.HardwareModel.Drivers
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using Chipset = Microsoft.CortexM0OnCMSISCore;


    public class SerialPortsManager : Chipset.SerialPortsManager
    {
        public override void Initialize()
        {
        }

        public override string[] GetPortNames()
        {
            return RT.HardwareProvider.Instance.GetSerialPorts();
        }

        public override System.IO.Ports.SerialStream Open(ref RT.BaseSerialStream.Configuration cfg)
        {
            StandardSerialPort port = new StandardSerialPort(ref cfg/*, id, 3*/);

            port.Open();

            return port;
        }
    }
}
