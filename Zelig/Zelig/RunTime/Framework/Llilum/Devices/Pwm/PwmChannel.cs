//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Pwm
{
    using System;

    public abstract class PwmChannel : IDisposable
    {
        public abstract void Dispose();

        public abstract void InitializePin();

        public abstract void SetDutyCycle(float ratio);

        public abstract void SetPulseWidth(int microSeconds);

        public abstract void SetPeriod(int microSeconds);
    }
}