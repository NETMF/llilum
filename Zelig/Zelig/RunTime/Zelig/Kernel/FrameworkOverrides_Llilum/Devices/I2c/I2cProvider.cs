//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.I2c;

    public class I2cChannelInfo : II2cChannelInfo
    {
        public int PortIndex { get; set; }

        public int SclPin    { get; set; }

        public int SdaPin    { get; set; }
    }

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class I2cProvider
    {
        sealed class EmptyI2cProvider : I2cProvider
        {
            public override I2cChannel CreateI2cChannel(II2cChannelInfo channelInfo)
            {
                throw new NotImplementedException();
            }

            public override I2cChannelInfo GetI2cChannelInfo(int id)
            {
                throw new NotImplementedException();
            }
        }

        public abstract I2cChannel CreateI2cChannel(II2cChannelInfo channelInfo);

        public abstract I2cChannelInfo GetI2cChannelInfo(int id);

        public static extern I2cProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptyI2cProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
