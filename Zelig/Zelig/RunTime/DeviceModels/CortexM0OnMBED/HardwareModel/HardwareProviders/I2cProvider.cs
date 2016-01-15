//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using Framework = Microsoft.Llilum.Devices.I2c;
    using RT        = Microsoft.Zelig.Runtime;

    public abstract class I2cProvider : RT.I2cProvider
    {
        public override Framework.I2cChannel CreateI2cChannel(Framework.II2cChannelInfo channelInfo)
        {
            if(!RT.HardwareProvider.Instance.TryReservePins(channelInfo.SclPin, channelInfo.SdaPin))
            {
                return null;
            }
            return new I2cChannel(channelInfo);
        }
    }
}
