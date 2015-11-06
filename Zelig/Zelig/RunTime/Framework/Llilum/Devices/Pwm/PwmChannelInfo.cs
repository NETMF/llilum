//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//
//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Pwm
{
    public interface IPwmChannelInfoUwp
    {
        int[] PwmPinNumbers { get; }

        int MaxFrequency { get; }

        int MinFrequency { get; }
    }
}
