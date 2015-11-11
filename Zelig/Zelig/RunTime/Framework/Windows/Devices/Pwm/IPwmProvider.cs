//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

using System.Collections.Generic;

namespace Windows.Devices.Pwm.Provider
{
    public interface IPwmProvider
    {
        IList<IPwmControllerProvider> GetControllers();
    }
}
