//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.Adc;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class AdcProvider
    {
        private sealed class EmptyAdcProvider : AdcProvider
        {
            public override AdcChannel CreateAdcPin(int pinNumber)
            {
                throw new NotImplementedException();
            }
        }

        public abstract AdcChannel CreateAdcPin(int pinNumber);

        public static extern AdcProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyAdcProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
