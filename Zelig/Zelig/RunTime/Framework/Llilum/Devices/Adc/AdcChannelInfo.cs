//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.Adc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IAdcChannelInfoUwp
    {
        int[] AdcPinNumbers { get; }

        int MaxValue { get; }

        int MinValue { get; }

        int ResolutionInBits { get; }
    }
}
