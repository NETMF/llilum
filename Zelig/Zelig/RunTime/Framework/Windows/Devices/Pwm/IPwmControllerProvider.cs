//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Pwm.Provider
{
    public interface IPwmControllerProvider
    {
        double ActualFrequency
        {
            get;
        }

        double MaxFrequency
        {
            get;
        }

        double MinFrequency
        {
            get;
        }

        int PinCount
        {
            get;
        }

        double SetDesiredFrequency(double frequency);

        void AcquirePin(int pin);

        void ReleasePin(int pin);

        void EnablePin(int pin);

        void DisablePin(int pin);

        void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity);
    }
}
