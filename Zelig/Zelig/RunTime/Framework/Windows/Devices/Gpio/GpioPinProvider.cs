//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Gpio.Provider
{
    using System;

    public interface IGpioPinProvider : IDisposable
    {
        //event TypedEventHandler<IGpioPinProvider, GpioPinProviderValueChangedEventArgs> ValueChanged;

        TimeSpan DebounceTimeout
        {
            get;
            set;
        }

        int PinNumber
        {
            get;
        }

        GpioSharingMode SharingMode
        {
            get;
        }

        bool IsDriveModeSupported(GpioPinDriveMode driveMode);

        GpioPinDriveMode GetDriveMode();

        GpioPinValue Read();

        void Write(GpioPinValue value);

        void SetPinDriveMode(GpioDriveMode driveMode);
    }

    public enum ProviderGpioPinEdge
    {
        FallingEdge,
        RisingEdge,
    }

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
