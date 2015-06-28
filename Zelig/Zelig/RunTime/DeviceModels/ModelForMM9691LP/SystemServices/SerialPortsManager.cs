//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP.Runtime
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
                       "COM1",
                       "COM2",
                   };
        }

        public override System.IO.Ports.SerialStream Open( ref RT.BaseSerialStream.Configuration cfg )
        {
            switch(cfg.PortName)
            {
                case "COM1": return Open( ref cfg, 1 );
                case "COM2": return Open( ref cfg, 0 );
            }

            return null;
        }

        private System.IO.Ports.SerialStream Open( ref RT.BaseSerialStream.Configuration cfg      ,
                                                       int                               usartNum )
        {
            Drivers.SerialPort port;

            switch(usartNum)
            {
                case 0:
                case 1: port = new Drivers.SerialPort( ref cfg, usartNum ); break;

                default:
                    return null;
            }

            port.Open();

            return port;
        }
    }
}
