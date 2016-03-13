//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM
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

        public abstract int[ ] LedPins
        {
            get;
        }

        public abstract int[ ] PwmPins
        {
            get;
        }

        //
        // Serial Methods
        //
        public abstract string[ ] GetSerialPorts( );

        public abstract SerialPortInfo GetSerialPortInfo( string portName );

        public abstract int GetSerialPortIRQ( string portName );

        //////public abstract void RemapSerialPortInterrupts( );

        //
        // System timer
        //
        public abstract int GetSystemTimerIRQ( );

        //--//

        //
        // Factory methods
        //

        public static extern Board Instance
        {
            [SingletonFactory( )]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}

