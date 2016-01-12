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

        [DisplayName("Number of Threads in Thread Pool")]
        [LinkToConfigurationOption("System__Runtime_DefaultThreadPoolThreads")]
        public int DefaultThreadPoolThreads;

        [DisplayName("Number of Threads in Timer Pool")]
        [LinkToConfigurationOption("System__Runtime_DefaultTimerPooThreads")]
        public int DefaultTimerPoolThreads;
    }
}
