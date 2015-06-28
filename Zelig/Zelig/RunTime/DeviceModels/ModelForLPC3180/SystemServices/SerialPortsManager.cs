//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180.Runtime
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
                       "UART3",
                       "UART4",
                       "UART5",
                       "UART6",
                   };
        }

        public override System.IO.Ports.SerialStream Open( ref RT.BaseSerialStream.Configuration cfg )
        {
            switch(cfg.PortName)
            {
                case "UART3": return Open( ref cfg, StandardUART.Id.UART3 );
                case "UART4": return Open( ref cfg, StandardUART.Id.UART4 );
                case "UART5": return Open( ref cfg, StandardUART.Id.UART5 );
                case "UART6": return Open( ref cfg, StandardUART.Id.UART6 );
            }

            return null;
        }

        private System.IO.Ports.SerialStream Open( ref RT.BaseSerialStream.Configuration cfg ,
                                                       StandardUART.Id                   id  )
        {
            Drivers.StandardSerialPort port;

            switch(id)
            {
                case StandardUART.Id.UART3: port = new Drivers.StandardSerialPort( ref cfg, id, INTC.IRQ_INDEX_IIR3 ); break;
                case StandardUART.Id.UART4: port = new Drivers.StandardSerialPort( ref cfg, id, INTC.IRQ_INDEX_IIR4 ); break;
                case StandardUART.Id.UART5: port = new Drivers.StandardSerialPort( ref cfg, id, INTC.IRQ_INDEX_IIR5 ); break;
                case StandardUART.Id.UART6: port = new Drivers.StandardSerialPort( ref cfg, id, INTC.IRQ_INDEX_IIR6 ); break;

                default:
                    return null;
            }

            port.Open( true );

            return port;
        }
    }
}
