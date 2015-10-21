//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.I2c
{
    using System;

    public abstract class I2cChannel : IDisposable
    {
        public abstract II2cChannelInfo GetChannelInfo();

        public abstract void Initialize(II2cChannelInfo channelInfo);

        public abstract void SetFrequency(int hz);

        public abstract int Write(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop);

        public abstract int Read(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop);

        public abstract void Dispose();
    }
}
