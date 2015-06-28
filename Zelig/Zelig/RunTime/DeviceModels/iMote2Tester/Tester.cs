//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

//#define TEST_SERIAL
//#define TEST_LED
#define TEST_I2C

namespace Microsoft.iMote2Tester
{
    using System;
    using System.IO.Ports;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    using Chipset = Microsoft.DeviceModels.Chipset.PXA27x;
    using Microsoft.iMote2Tester.UnitTests;

    unsafe class Tester
    {

        static void Main()
        {
            ITest unitTest;

#if TEST_SERIAL

            unitTest = new SerialTests();
            unitTest.Prepare();
            unitTest.Run();
            Console.WriteLine("Test " + (unitTest.Success ? "Succeeded" : "FAILED") + ", with result: " + unitTest.Result);

#endif

#if TEST_LED

            unitTest = new LEDTests();
            unitTest.Prepare();
            unitTest.Run();
            Console.WriteLine("Test " + (unitTest.Success ? "Succeeded" : "FAILED") + ", with result: " + unitTest.Result);

#endif

#if TEST_I2C

            unitTest = new I2CBusTests();
            unitTest.Prepare();
            unitTest.Run();
            Console.WriteLine("Test " + (unitTest.Success ? "Succeeded" : "FAILED") + ", with result: " + unitTest.Result);

#endif

        }
    }
}
