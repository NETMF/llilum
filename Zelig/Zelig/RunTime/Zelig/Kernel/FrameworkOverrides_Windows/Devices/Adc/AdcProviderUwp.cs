//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.Adc;

    public class AdcChannelInfoUwp : IAdcChannelInfoUwp
    {
        public int[] AdcPinNumbers { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public int ResolutionInBits { get; set; }
    }

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class AdcProviderUwp
    {
        private sealed class EmptyAdcProviderUwp : AdcProviderUwp
        {
            public override AdcChannelInfoUwp GetAdcChannelInfo()
            {
                throw new NotImplementedException();
            }
        }

        public abstract AdcChannelInfoUwp GetAdcChannelInfo();

        public static extern AdcProviderUwp Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyAdcProviderUwp))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}