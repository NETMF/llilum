//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.I2c
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface II2cChannelInfo
    {
        int SdaPin      { get; }
        int SclPin      { get; }
        int PortIndex   { get; }
    }
}
