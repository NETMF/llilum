//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;

namespace Windows.Devices.Spi.Provider
{
    public abstract class SpiChannel : IDisposable
    {
        public abstract void SetupPins(int mosiPin, int misoPin, int sclPin, int csPin, bool useAlternateCsPin, bool activeLowCS );

        public abstract void SetupChannel(int bits, int mode, bool isSlave);

        public abstract void SetupTiming(int frequencyInHz, int setupTime, int holdTime);
        
        public abstract void WriteRead( byte[ ] writeBuffer, byte[ ] readBuffer, int startReadOffset );

        public abstract void Dispose( );
    }
}
