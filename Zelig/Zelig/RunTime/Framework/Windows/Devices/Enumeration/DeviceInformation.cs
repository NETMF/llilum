//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#define INCOMPLETE
namespace Windows.Devices.Enumeration
{
    using System;
    using System.Runtime.InteropServices;
#if (!INCOMPLETE)
    //using Windows.Foundation;
    //using Windows.Foundation.Collections;
    //using Windows.Foundation.Metadata;

    /// <summary>Represents a device.</summary>
    //[ContractVersion(typeof(UniversalApiContract), 65536u), DualApiPartition(version = 100794368u), MarshalingBehavior(MarshalingType.Agile), Static(typeof(IDeviceInformationStatics2), 65536u, typeof(UniversalApiContract)), Static(typeof(IDeviceInformationStatics), 65536u, typeof(UniversalApiContract)), Threading(ThreadingModel.Both)]
#endif
    public sealed class DeviceInformation // : IDeviceInformation, IDeviceInformation2
    {
        private DeviceInformation(string id, string name)
        {
            Id = id;
            Name = name;
        }
        
        /// <summary>A string representing the identity of the device.</summary>
        /// <returns>A string representing the identity of the device.</returns>
        public string Id
        {
            get;
        }

        public bool IsDefault
        {
            get;
        }

        /// <summary>Indicates whether this device is enabled.</summary>
        /// <returns>Indicates whether this device is enabled.</returns>
        public bool IsEnabled
        {
            get;
        }

        /// <summary>The name of the device.</summary>
        /// <returns>The name of the device. This name is in the best available language for the app.</returns>
        public string Name
        {
            get;
        }
#if (!INCOMPLETE)
        //public EnclosureLocation EnclosureLocation
        //{
        //    get;
        //}

        /// <summary>Property store containing well-known values as well as additional properties that can be specified during device enumeration.</summary>
        /// <returns>The property store for the device.</returns>
        public IMapView<string, object> Properties
        {
            [return: HasVariant]
            get;
        }

        public DeviceInformationKind Kind
        {
            get;
        }

        public DeviceInformationPairing Pairing
        {
            get;
        }

        /// <summary>Updates the properties of an existing DeviceInformation object.</summary>
        /// <param name="updateInfo">Indicates the properties to update.</param>
        public void Update([In] DeviceInformationUpdate updateInfo)
        {

        }

        //[RemoteAsync]
        public IAsyncOperation<DeviceThumbnail> GetThumbnailAsync()
        {

        }

        /// <summary>Gets a glyph for the device.</summary>
        /// <returns>The object for managing the asynchronous operation that will return a DeviceThumbnail</returns>
        //[RemoteAsync]
        public IAsyncOperation<DeviceThumbnail> GetGlyphThumbnailAsync()
        {

        }

        public static string GetAqsFilterFromDeviceClass([In] DeviceClass deviceClass)
        {

        }

        //[Overload("CreateFromIdAsyncWithKindAndAdditionalProperties")]
        public static IAsyncOperation<DeviceInformation> CreateFromIdAsync([In] string deviceId, [In] IIterable<string> additionalProperties, [In] DeviceInformationKind kind)
        {

        }

        //[Overload("FindAllAsyncWithKindAqsFilterAndAdditionalProperties")]
        public static IAsyncOperation<DeviceInformationCollection> FindAllAsync([In] string aqsFilter, [In] IIterable<string> additionalProperties, [In] DeviceInformationKind kind)
        {

        }

        //[Overload("CreateWatcherWithKindAqsFilterAndAdditionalProperties")]
        public static DeviceWatcher CreateWatcher([In] string aqsFilter, [In] IIterable<string> additionalProperties, [In] DeviceInformationKind kind)
        {

        }

        //[Overload("CreateFromIdAsync"), RemoteAsync]
        public static IAsyncOperation<DeviceInformation> CreateFromIdAsync([In] string deviceId)
        {

        }

        //[Overload("CreateFromIdAsyncAdditionalProperties")]
        public static IAsyncOperation<DeviceInformation> CreateFromIdAsync([In] string deviceId, [In] IIterable<string> additionalProperties)
        {

        }

        //[Overload("FindAllAsync"), RemoteAsync]
        //public static IAsyncOperation<DeviceInformationCollection> FindAllAsync()
        public static DeviceInformationCollection FindAllAsync()
        {

        }

        [DefaultOverload, Overload("FindAllAsyncDeviceClass"), RemoteAsync]
        public static IAsyncOperation<DeviceInformationCollection> FindAllAsync([In] DeviceClass deviceClass)
        {

        }
#endif
        /// <summary>Enumerates DeviceInformation objects matching the specified Advanced Query Syntax (AQS) string.</summary>
        /// <returns>The object for managing the asynchronous operation.</returns>
        /// <param name="aqsFilter">An AQS string that filters the DeviceInformation objects to enumerate. Typically this string is retrieved from the GetDeviceSelector method of a class that interacts with devices. For example, GetDeviceSelector retrieves the string for the StorageDevice class.</param>
        //[Overload("FindAllAsyncAqsFilter"), RemoteAsync]
        //public static IAsyncOperation<DeviceInformationCollection> FindAllAsync([In] string aqsFilter)
        public static DeviceInformationCollection FindAllAsync([In] string aqsFilter)
        {
            return new DeviceInformationCollection()
            {
                new DeviceInformation(aqsFilter, aqsFilter)
            };
        }
#if (!INCOMPLETE)
        //[Overload("FindAllAsyncAqsFilterAndAdditionalProperties")]
        public static IAsyncOperation<DeviceInformationCollection> FindAllAsync([In] string aqsFilter, [In] IIterable<string> additionalProperties)
        {

        }

        /// <summary>Creates a DeviceWatcher for all devices.</summary>
        /// <returns>The created DeviceWatcher.</returns>
        //[Overload("CreateWatcher")]
        public static DeviceWatcher CreateWatcher()
        {

        }

        /// <summary>Creates a DeviceWatcher for devices matching the specified DeviceClass.</summary>
        /// <returns>The created DeviceWatcher.</returns>
        /// <param name="deviceClass">The class of device to enumerate using the DeviceWatcher.</param>
        [DefaultOverload, Overload("CreateWatcherDeviceClass")]
        public static DeviceWatcher CreateWatcher([In] DeviceClass deviceClass)
        {

        }

        //[Overload("CreateWatcherAqsFilter")]
        public static DeviceWatcher CreateWatcher([In] string aqsFilter)
        {

        }

        //[Overload("CreateWatcherAqsFilterAndAdditionalProperties")]
        public static DeviceWatcher CreateWatcher([In] string aqsFilter, [In] IIterable<string> additionalProperties)
        {

        }
#endif
    }
}
