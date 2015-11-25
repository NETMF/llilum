//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Pwm
{
    using System;

    public enum PwmPolarity
    {
        Normal = 0,
        Inverted,
    };

    public enum PwmPrescaler
    {
        Div1 = 0,
        Div2,
        Div4,
        Div8,
        Div16,
        Div64,
        Div256,
        Div1024
    };

    public abstract class PwmChannel : IDisposable
    {
        public abstract void Dispose();

        public abstract void InitializePin();

        public abstract void SetDutyCycle(float ratio);

        public abstract void SetPulseWidth(int microSeconds);

        public abstract void SetPeriod(int microSeconds);

        public abstract void SetPolarity(PwmPolarity polarity);

        public abstract void SetPrescaler(PwmPrescaler prescaler);

        public abstract void Start( );

        public abstract void Stop( );
    }
}