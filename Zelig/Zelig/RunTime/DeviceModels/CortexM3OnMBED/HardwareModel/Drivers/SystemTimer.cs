//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM3OnMBED.Drivers
{
    using ChipsetModel = Microsoft.CortexM0OnMBED;

    /// <summary>
    /// This class implements the internal system timer. All times are in ticks (time agnostic)
    /// however in practice, and due to limitations of mbed 1 tick is equal to 1 uS (micro second)
    /// </summary>
    public abstract class SystemTimer : ChipsetModel.Drivers.SystemTimer
    {
    }
}
