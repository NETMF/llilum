//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TEST_PWM


namespace Microsoft.Zelig.Test.mbed.Simple
{
    using Windows.Devices.Pwm;


    internal partial class Program
    {
        private static void TestPWM( int pwmPinNumber )
        {
#if TEST_PWM
            var pwmController = PwmController.GetDefaultAsync();
            pwmController.SetDesiredFrequency(1000000);

            var pwmPin = pwmController.OpenPin( pwmPinNumber );

            pwmPin.SetActiveDutyCyclePercentage(0.7F);

            pwmPin.Polarity = PwmPulsePolarity.ActiveLow;

            pwmPin.Start();
#endif
        }
    }
}
