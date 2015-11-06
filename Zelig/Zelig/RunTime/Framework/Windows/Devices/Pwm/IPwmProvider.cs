//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Pwm.Provider
{
    using System.Collections.Generic;

    public interface IPwmProvider
    {
        IList<IPwmControllerProvider> GetControllers();
    }
}
