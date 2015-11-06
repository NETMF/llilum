//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;
    using Framework = Microsoft.Llilum.Devices.Pwm;
    using Runtime = Microsoft.Zelig.Runtime;

    public sealed class PwmProvider : Runtime.PwmProvider
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
