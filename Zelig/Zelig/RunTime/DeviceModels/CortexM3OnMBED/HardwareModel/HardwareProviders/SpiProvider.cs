//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;
    using Framework = Microsoft.Llilum.Devices.Spi;
    using Runtime = Microsoft.Zelig.Runtime;

    public abstract class SpiProvider : Runtime.SpiProvider
    {
        public sealed override Framework.SpiChannel CreateSpiChannel(Framework.ISpiChannelInfo channelInfo)
        {
            return new SpiChannel(channelInfo);
        }
    }
}
