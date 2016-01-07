//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Win32
{
    using System;
    using RT = Microsoft.Zelig.Runtime;

    //--//

    public class HardwareProvider : RT.HardwareProvider
    {
        static string []s_ports = new string[0];

        public override int InvalidPin
        {
            get
            {
                throw new NotImplementedException( );
            }
        }

        public override int PinCount
        {
            get
            {
                throw new NotImplementedException( );
            }
        }

        public override bool GetSerialPinsFromPortName( string portName, out int txPin, out int rxPin, out int rtsPin, out int ctsPin )
        {
            throw new NotImplementedException( );
        }

        public override string[] GetSerialPorts( )
        {
            return s_ports;
        }

        public override int PinToIndex( int pin )
        {
            throw new NotImplementedException( );
        }
    }
}
