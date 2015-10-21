//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;
    using Framework = Microsoft.Llilum.Devices.I2c;
    using Runtime = Microsoft.Zelig.Runtime;

    public abstract class I2cProvider : Runtime.I2cProvider
    {
        public sealed override Framework.I2cChannel CreateI2cChannel(Framework.II2cChannelInfo channelInfo)
        {
            if(!Runtime.HardwareProvider.Instance.TryReservePins(channelInfo.SclPin, channelInfo.SdaPin))
            {
                return null;
            }
            return new I2cChannel(channelInfo);
        }
    }
}
