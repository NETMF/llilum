//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Win32
{
    using System;
    using System.IO.Ports;
    using RT = Microsoft.Zelig.Runtime;

    //--//

    public class SerialPortsManager : RT.SerialPortsManager
    {
        public override string[] GetPortNames( )
        {
            return null;
        }

        public override void Initialize( )
        {
        }

        public override SerialStream Open( ref RT.BaseSerialStream.Configuration cfg )
        {
            throw new NotImplementedException( );
        }
    }
}
