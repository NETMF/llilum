//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Adc
{
    using System;

    public abstract class AdcChannel : IDisposable
    {
        public abstract void Dispose();

        public abstract void InitializePin();

        public abstract uint ReadUnsigned();

        public abstract float Read();
    }
}