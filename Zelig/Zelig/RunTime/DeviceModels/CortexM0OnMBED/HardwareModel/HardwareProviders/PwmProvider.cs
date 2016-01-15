//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using Framework = Microsoft.Llilum.Devices.Pwm;
    using Runtime = Microsoft.Zelig.Runtime;

    public abstract class PwmProvider : Runtime.PwmProvider
    {
        public sealed override Framework.PwmChannel TryCreatePwmPin(int pinNumber)
        {
            if (!Runtime.HardwareProvider.Instance.TryReservePins(pinNumber))
            {
                return null;
            }
            return new PwmChannel(pinNumber);
        }
    }
}
