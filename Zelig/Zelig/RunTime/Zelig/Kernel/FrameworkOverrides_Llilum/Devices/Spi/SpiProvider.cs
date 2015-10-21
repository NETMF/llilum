//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using Microsoft.Llilum.Devices.Spi;

    //--//

    public class SpiChannelInfo : ISpiChannelInfo
    {
        public int Mosi { get; set; }
        public int Miso { get; set; }
        public int Sclk { get; set; }
        public int ChipSelect { get; set; }
        public int SetupTime { get; set; }
        public int HoldTime { get; set; }
        public bool ReserveMisoPin { get; set; }
        public bool ActiveLow { get; set; }
    }

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class SpiProvider
    {
        private sealed class EmptySpiProvider : SpiProvider
        {
            public override bool SpiBusySupported
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override SpiChannel CreateSpiChannel(ISpiChannelInfo channelInfo)
            {
                throw new NotImplementedException();
            }

            public override SpiChannelInfo GetSpiChannelInfo(int id)
            {
                throw new NotImplementedException();
            }
        }

        public abstract SpiChannel CreateSpiChannel(ISpiChannelInfo channelInfo);

        public abstract SpiChannelInfo GetSpiChannelInfo(int id);

        public abstract bool SpiBusySupported
        {
            get;
        }

        public static extern SpiProvider Instance
        {
            [SingletonFactory(Fallback = typeof(EmptySpiProvider))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
