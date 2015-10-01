//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class ProcessorCategory : AbstractCategory
    {
        [DisplayName("Processor Clock Frequency")]
        [LinkToConfigurationOption("System__CoreClockFrequency")]
        public ulong CoreClockFrequency;

        [DisplayName("Peripherals Clock Frequency")]
        [LinkToConfigurationOption("System__PeripheralsClockFrequency")]
        public ulong PeripheralsClockFrequency;

        [DisplayName("RealTime Clock Frequency")]
        [LinkToConfigurationOption("System__RealTimeClockFrequency")]
        public ulong RealTimeClockFrequency;
    }
}
