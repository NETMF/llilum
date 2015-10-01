//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Gpio.Provider
{
    using System;

    public abstract class GpioPinProvider : IDisposable
    {
        public abstract void InitializePin(int pinNumber);

        public abstract int Read();

        public abstract void Write(int value);

        public abstract void SetPinDriveMode(GpioDriveMode driveMode);
        public abstract void Dispose();
    }

    // Duplicating the definition in UWP, so we have our own, internal DriveMode
    public enum GpioDriveMode
    {
        Input = 0,
        Output,
        InputPullUp,
        InputPullDown,
        OutputOpenDrain,
        OutputOpenDrainPullUp,
        OutputOpenSource,
        OutputOpenSourcePullDown,
    }
}
