//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Windows.Devices.Spi
{
    using System.Collections.Generic;

    public sealed class SpiBusInfo
    {
        // This needs to be marked as internal so the end user cannot instantiate it
        internal SpiBusInfo()
        {
        }

        /// <summary>
        /// Gets the number of chip select lines available on the bus.
        /// </summary>
        /// <value>Number of chip select lines.</value>
        public int ChipSelectLineCount { get; internal set; }

        /// <summary>
        /// Maximum clock cycle frequency of the bus.
        /// </summary>
        /// <value>The clock cycle in Hz.</value>
        public int MaxClockFrequency { get; internal set; }

        /// <summary>
        /// Minimum clock cycle frequency of the bus.
        /// </summary>
        /// <value>The clock cycle in Hz.</value>
        public int MinClockFrequency { get; internal set; }

        /// <summary>
        /// Gets the bit lengths that can be used on the bus for transmitting data.
        /// </summary>
        /// <value>The supported data lengths.</value>
        /// 

        // BUGBUG: TODO TODO: implement IReadOnlyList
        public /*IReadOnlyList<int>*/ List<int> SupportedDataBitLengths { get; internal set; }
    }
}
