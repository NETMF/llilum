//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Spi
{
    using System;

    public abstract class SpiChannel : IDisposable
    {
        public abstract ISpiChannelInfo GetChannelInfo();

        public abstract void SetupPins(ISpiChannelInfo channelInfo, bool useAlternateCsPin, int alternateCsPin);

        public abstract void SetupChannel(int bits, SpiMode mode, bool isSlave);

        public abstract void SetupTiming(int frequencyInHz, int setupTime, int holdTime);

        public abstract void WriteRead(byte[] writeBuffer, byte[] readBuffer, int startReadOffset);

        public abstract void Dispose();
    }

    public enum SpiMode
    {
        Cpol0Cpha0 = 0,
        Cpol0Cpha1,
        Cpol1Cpha0,
        Cpol1Cpha1,
    }
}
