//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Adc.Provider
{
    using System.Collections.Generic;

    public interface IAdcProvider
    {
        IList<IAdcControllerProvider> GetControllers();
    }
}
