//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

namespace Microsoft.iMote2Tester.UnitTests
{
    using System;
    using System.IO.Ports;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    using Chipset = Microsoft.DeviceModels.Chipset.PXA27x;

    /// <summary>
    /// common interface for the test cases
    /// </summary>
    public interface ITest
    {
        /// <summary>
        /// prepares the test case
        /// </summary>
        void Prepare();
        /// <summary>
        /// executes the test case
        /// </summary>
        void Run();

        /// <summary>
        /// retrieves the results of the test case
        /// </summary>
        String Result { get; }

        /// <summary>
        /// if true, test completed with no error
        /// </summary>
        bool Success { get; }
    }

}
