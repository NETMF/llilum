//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Microsoft.Llilum.Devices.Pwm;

    [ExtendClass(typeof(PwmPin), NoConstructors = true)]
    class PwmPinImpl
    {
        public static PwmChannel TryAcquirePwmPin(int pinNumber)
        {
            return PwmProvider.Instance.TryCreatePwmPin(pinNumber);
        }
    }
}
