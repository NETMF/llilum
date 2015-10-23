//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Spi
{
    using System;

    /// <summary>
    /// This is the information we need to use a Llilum.NET SpiDevice
    /// </summary>
    public interface ISpiChannelInfo
    {
        int Mosi { get; }
        int Miso { get; }
        int Sclk { get; }
        int DefaultChipSelect { get; }
        int SetupTime { get; }
        int HoldTime { get; }
        bool ActiveLow { get; }
    }

    /// <summary>
    /// This is the information we need to implement UWP Spi
    /// </summary>
    public interface ISpiChannelInfoUwp
    {
        ISpiChannelInfo ChannelInfo { get; }
        int ChipSelectLines { get; }
        int MaxFreq { get; }
        int MinFreq { get; }
        bool Supports16 { get; }
    }
}
