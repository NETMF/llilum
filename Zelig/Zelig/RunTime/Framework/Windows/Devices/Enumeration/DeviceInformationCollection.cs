//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#define INCOMPLETE
namespace Windows.Devices.Enumeration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
#if (!INCOMPLETE)
    //using Windows.Foundation;
    //using Windows.Foundation.Collections;
    //using Windows.Foundation.Metadata;

    //[ContractVersion(typeof(UniversalApiContract), 65536u), DualApiPartition(version = 100794368u), MarshalingBehavior(MarshalingType.Agile)]
    //public sealed class DeviceInformationCollection : IReadOnlyList<DeviceInformation>, IIterable<DeviceInformation>
#endif
    public sealed class DeviceInformationCollection : List<DeviceInformation>
    {
        /// <summary>The number of DeviceInformation objects in the collection.</summary>
        /// <returns>The number of DeviceInformation objects in the collection.</returns>
        public uint Size
        {
            get;
        }

        /// <summary>Gets the DeviceInformation object at the specified index.</summary>
        /// <returns>The DeviceInformation object at the specified index.</returns>
        /// <param name="index">The index.</param>
        public DeviceInformation GetAt([In] uint index)
        {
            return base[ (int)index ];
        }

        /// <summary>Returns the index of the specified DeviceInformation object in the collection.</summary>
        /// <returns>true if the method succeeded; otherwise, false.</returns>
        /// <param name="value">The DeviceInformation object in the collection.</param>
        /// <param name="index">The index.</param>
        public bool IndexOf([In] DeviceInformation value, out uint index)
        {
            index = (uint)base.IndexOf(value);
            return (index >= 0);
        }

        public uint GetMany([In] uint startIndex, [Out] DeviceInformation[] items)
        {
            throw new NotImplementedException( );
        }
#if (!INCOMPLETE)
        public IIterator<DeviceInformation> First()
        {

        }
#endif
    }
}
