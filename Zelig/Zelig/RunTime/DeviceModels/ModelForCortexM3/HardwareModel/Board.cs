//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM3
{
    using Microsoft.Zelig.Runtime;
    using System.Runtime.CompilerServices;


    public abstract class Board
    {
        public class SerialPortInfo
        {
            public int RxPin;
            public int TxPin;
            public int RtsPin;
            public int CtsPin;
        }

        //--//

        public abstract int PinCount
        {
            get;
        }

        public abstract int PinToIndex( int pin );
        
        public abstract int NCPin
        {
            get;
        }

        //
        // Serial Methods
        //
        public abstract string[] GetSerialPorts();

        public abstract SerialPortInfo GetSerialPortInfo(string portName);

        //
        // System timer
        //
        public abstract int GetSystemTimerIRQNumber( );

        //
        // Factory methods
        //

        public static extern Board Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}

