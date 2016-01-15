//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using Framework = Microsoft.Llilum.Devices.Spi;
    using RT        = Microsoft.Zelig.Runtime;

    public abstract class SpiProvider : RT.SpiProvider
    {
        public override Framework.SpiChannel CreateSpiChannel(Framework.ISpiChannelInfo channelInfo)
        {
            return new SpiChannel(channelInfo);
        }
    }
}
