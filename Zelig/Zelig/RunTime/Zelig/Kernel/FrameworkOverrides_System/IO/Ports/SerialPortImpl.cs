//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.IO.Ports.SerialPort),NoConstructors=true)]
    public class SerialPortImpl
    {
        //
        // Helper Methods
        //

        public static string[] GetPortNames()
        {
            return SerialPortsManager.Instance.GetPortNames();
        }
    }
}
