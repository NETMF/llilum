//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Windows.Devices.Adc.Provider
{
    public interface IAdcProvider
    {
        IList<IAdcControllerProvider> GetControllers();
    }
}
