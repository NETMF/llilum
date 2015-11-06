//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.Pwm;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class PwmProvider
    {
        private sealed class EmptyPwmProvider : PwmProvider
        {
            public override PwmChannel TryCreatePwmPin(int pinNumber)
            {
                throw new NotImplementedException();
            }
        }

        public abstract PwmChannel TryCreatePwmPin(int pinNumber);

        public static extern PwmProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyPwmProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
