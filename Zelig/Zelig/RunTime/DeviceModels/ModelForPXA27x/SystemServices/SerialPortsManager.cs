//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x.Runtime
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class SerialPortsManager : RT.SerialPortsManager
    {
        //
        // State
        //

        //
        // Helper Methods
        //

        public override void Initialize()
        {
        }

        public override string[] GetPortNames()
        {
            return new string[]
                   {
                       "FFUART",
                       "BTUART",
                       "STUART",
                   };
        }

        public override System.IO.Ports.SerialStream Open( ref RT.BaseSerialStream.Configuration cfg )
        {
            switch (cfg.PortName)
            {
                case "FFUART": return Open(ref cfg, UART.Id.FFUART);
                case "BTUART": return Open(ref cfg, UART.Id.BTUART);
                case "STUART": return Open(ref cfg, UART.Id.STUART);
            }

            return null;
        }

        private System.IO.Ports.SerialStream Open(ref RT.BaseSerialStream.Configuration cfg,
                                                       UART.Id id)
        {
            Drivers.SerialPort port;

            switch (id)
            {
                case UART.Id.FFUART: port = new Drivers.SerialPort(ref cfg, id, InterruptController.IRQ_INDEX_FFUART); break;
                case UART.Id.BTUART: port = new Drivers.SerialPort(ref cfg, id, InterruptController.IRQ_INDEX_BTUART); break;
                case UART.Id.STUART: port = new Drivers.SerialPort(ref cfg, id, InterruptController.IRQ_INDEX_STUART); break;

                default:
                    return null;
            }

            port.Open();

            return port;
        }
    }
}
