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

    public class SpiChannelInfoUwp : ISpiChannelInfoUwp
    {
        public int ChipSelectLines { get; set; }
        public int MaxFreq { get; set; }
        public int MinFreq { get; set; }
        public bool Supports16 { get; set; }

        // In order for the Board assemblies to not take a dependency on the framework,
        // we need to set the channel info through a class-specific property and 
        // expose it through the regular interface
        public SpiChannelInfo ChannelInfoKernel { get; set; }
        public ISpiChannelInfo ChannelInfo
        {
            get
            {
                return ChannelInfoKernel;
            }
        }
    }

    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class SpiProviderUwp
    {
        private sealed class EmptySpiProviderUwp : SpiProviderUwp
        {
            public override SpiChannelInfoUwp GetSpiChannelInfo(string busId)
            {
                throw new NotImplementedException();
            }

            public override string[] GetSpiChannels()
            {
                throw new NotImplementedException();
            }
        }

        public abstract SpiChannelInfoUwp GetSpiChannelInfo(string busId);

        public abstract string[] GetSpiChannels();

        public static extern SpiProviderUwp Instance
        {
            [SingletonFactory(Fallback = typeof(EmptySpiProviderUwp))]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}
