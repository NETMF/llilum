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
    using Microsoft.DeviceModels.Chipset.PXA27x.Drivers;


    /// <summary>
    /// Unit Tests for the LEDs
    /// </summary>
    public class LEDTests : ITest
    {

        #region Generic LED tests
        /// <summary>
        /// LED test
        /// </summary>
        public void TestCase1()
        {
            Chipset.PowerManager.Instance.ReleaseReadDisableHold();

            var gpio = Chipset.GPIO.Instance;

            gpio.EnableAsOutputPin(103, false);
            gpio.EnableAsOutputPin(104, false);
            gpio.EnableAsOutputPin(105, false);

            bool state = true;

            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(500);
                gpio[103] = state;
                ASSERT(gpio[103] == state, true, "wrong setting");

                System.Threading.Thread.Sleep(500);
                gpio[104] = state;
                ASSERT(gpio[104] == state, true, "wrong setting");

                System.Threading.Thread.Sleep(500);
                gpio[105] = state;
                ASSERT(gpio[105] == state, true, "wrong setting");

                state = !state;
            }

            state = true;

            gpio[103] = state;
            gpio[104] = state;
            gpio[105] = state;
            ASSERT(gpio[103] == state, true, "wrong setting");
            ASSERT(gpio[104] == state, true, "wrong setting");
            ASSERT(gpio[105] == state, true, "wrong setting");
        }
        #endregion


        #region ITest Members
        private bool m_success = true;
        private int assertionId = 0;

        /// <summary>
        /// if true, test completed with no error
        /// </summary>
        public bool Success
        {
            get
            {
                return m_success;
            }
        }

        /// <summary>
        /// checks that an object is not null
        /// </summary>
        /// <param name="o"></param>
        /// <param name="failureMessage"></param>
        public void ASSERT_NOT_NULL(object o, String failureMessage)
        {
            assertionId++;

            if (null == o)
            {
                Result += "\r\n# " + assertionId + " Object <null>: " + failureMessage;
                m_success = false;
            }
        }
        /// <summary>
        /// checks that a condition is met
        /// </summary>
        /// <param name="current"></param>
        /// <param name="expected"></param>
        /// <param name="failureMessage"></param>
        public void ASSERT(bool current, bool expected, String failureMessage)
        {
            assertionId++;

            if (current != expected)
            {
                Result += "\r\n#" + assertionId + " " + current + "!=" + expected + "(expected): " + failureMessage;
                m_success = false;
            }
        }

        /// <summary>
        /// Prepares the output port test case
        /// </summary>
        public void Prepare()
        {
            Result = "";
        }

        /// <summary>
        /// Executes the output port test case
        /// </summary>
        public void Run()
        {
            try
            {
                TestCase1();
            }
            catch
            {
                ASSERT_NOT_NULL(null, "test case 1 - ");
            }
        }

        /// <summary>
        /// Retrieves the results of the output port test case
        /// </summary>
        public string Result { get; private set; }
        #endregion

    }

}
