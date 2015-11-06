//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.Pwm;

    public class PwmChannelInfoUwp : IPwmChannelInfoUwp
    {
        public int MaxFrequency { get; set; }

        public int MinFrequency { get; set; }

        public int[] PwmPinNumbers { get; set; }
    }

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class PwmProviderUwp
    {
        private sealed class EmptyPwmProviderUwp : PwmProviderUwp
        {
            public override PwmChannelInfoUwp GetPwmChannelInfo()
            {
                throw new NotImplementedException();
            }
        }

        public abstract PwmChannelInfoUwp GetPwmChannelInfo();

        public static extern PwmProviderUwp Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyPwmProviderUwp))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}