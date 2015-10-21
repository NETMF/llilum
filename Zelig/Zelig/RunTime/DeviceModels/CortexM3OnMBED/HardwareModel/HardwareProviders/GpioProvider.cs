//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;
    using Framework = Microsoft.Llilum.Devices.Gpio;
    using Runtime = Microsoft.Zelig.Runtime;

    public sealed class GpioProvider : Runtime.GpioProvider
    {
        public sealed override Framework.GpioPin CreateGpioPin(int pinNumber)
        {
            if (!Runtime.HardwareProvider.Instance.TryReservePins(pinNumber))
            {
                return null;
            }
            return new GpioPin(pinNumber);
        }
    }
}
