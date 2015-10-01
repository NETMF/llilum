//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.CortexM3
{
    using Microsoft.Zelig.Runtime;
    using System.Runtime.CompilerServices;


    public abstract class Board
    {
        public class SpiChannelInfo
        {
            public int Mosi;
            public int Miso;
            public int Sclk;
            public int ChipSelect;
            public int ChipSelectLines;
            public int MaxFreq;
            public int MinFreq;
            public bool Supports16;
            public int SetupTime;
            public int HoldTime;
            public bool ActiveLow;  
        }

        public class I2cChannelInfo
        {
            public int SdaPin;
            public int SclPin;
        }

        //--//

        public abstract int GetSpiChannelIndexFromString( string busId );

        // The cases should match the device selector strings
        public abstract SpiChannelInfo GetSpiChannelInfo( int id );

        public abstract string[] GetSpiChannels();

        public abstract int PinCount
        {
            get;
        }

        public abstract int PinToIndex( int pin );
        
        public abstract int NCPin
        {
            get;
        }

        public abstract bool SpiBusySupported
        {
            get;
        }

        //
        // I2C Methods
        //
        public abstract I2cChannelInfo GetI2cChannelInfo(int index);

        public abstract string[] GetI2cChannels();

        public abstract int GetI2cChannelIndexFromString(string busId);
        
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

