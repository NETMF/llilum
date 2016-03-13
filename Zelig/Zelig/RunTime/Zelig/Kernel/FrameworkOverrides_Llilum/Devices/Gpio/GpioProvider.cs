//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.Gpio;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class GpioProvider
    {
        private sealed class EmptyGpioProvider : GpioProvider
        {
            public override GpioPin CreateGpioPin(int pinNumber)
            {
                throw new NotImplementedException();
            }

            //////public override void RemapInterrupts( )
            //////{
            //////    throw new NotImplementedException();
            //////}

            public override int GetGpioPinIRQNumber(int pinNumber)
            {
                throw new NotImplementedException();
            }
        }

        public abstract GpioPin CreateGpioPin(int pinNumber);

        //////public abstract void RemapInterrupts( );

        public abstract int GetGpioPinIRQNumber(int pinNumber);

        public static extern GpioProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyGpioProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
