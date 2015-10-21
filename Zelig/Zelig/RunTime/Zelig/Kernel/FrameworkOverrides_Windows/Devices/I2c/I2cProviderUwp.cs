//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.I2c;

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class I2cProviderUwp
    {
        sealed class EmptyI2cProviderUwp : I2cProviderUwp
        {
            public override I2cChannelInfo GetI2cChannelInfo(string busId)
            {
                throw new NotImplementedException();
            }

            public override string[] GetI2cChannels()
            {
                throw new NotImplementedException();
            }
        }

        public abstract I2cChannelInfo GetI2cChannelInfo(string busId);

        public abstract string[] GetI2cChannels();

        public static extern I2cProviderUwp Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyI2cProviderUwp))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
