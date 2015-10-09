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


    [ExtendClass(typeof(System.IO.Ports.SerialStream), NoConstructors = true)]
    public class SerialStreamImpl
    {
        //
        // Helper Methods
        //

        internal static System.IO.Ports.SerialStream Create(string portName,
                                                             int baudRate,
                                                             System.IO.Ports.Parity parity,
                                                             int dataBits,
                                                             System.IO.Ports.StopBits stopBits,
                                                             int readBufferSize,
                                                             int writeBufferSize,
                                                             int readTimeout,
                                                             int writeTimeout,
                                                             System.IO.Ports.Handshake handshake,
                                                             bool dtrEnable,
                                                             bool rtsEnable,
                                                             bool discardNull,
                                                             byte parityReplace)
        {
            var cfg = new BaseSerialStream.Configuration(portName) {
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                ReadBufferSize = readBufferSize,
                WriteBufferSize = writeBufferSize,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout,
                Handshake = handshake,
                DtrEnable = dtrEnable,
                RtsEnable = rtsEnable,
                DiscardNull = discardNull,
                ParityReplace = parityReplace,
            };

            return SerialPortsManager.Instance.Open(ref cfg);
        }
    }
}
