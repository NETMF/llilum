//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using Microsoft.Llilum.Devices.Adc;

    [ExtendClass(typeof(AdcPin), NoConstructors = true)]
    class AdcPinImpl
    {
        public static AdcChannel TryAcquireAdcPin(int pinNumber)
        {
            return AdcProvider.Instance.CreateAdcPin(pinNumber);
        }
    }
}
