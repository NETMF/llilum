//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.I2c.Provider
{
    using System;

    public abstract class I2cChannel : IDisposable
    {
        public abstract void Initialize(int sdaPin, int sclPin);

        public abstract void SetFrequency(int hz);

        public abstract int Write(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop);

        public abstract int Read(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop);

        public abstract void Dispose();

        public abstract int CurrentFrequency
        {
            get;
        }
    }
}
