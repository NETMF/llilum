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
    /// Unit Tests for the I2C subsystem
    /// </summary>
    public class SerialTests : ITest
    {

        #region Generic Serial tests
        /// <summary>
        /// serial port test
        /// </summary>
        public void TestCase1()
        {
            var port = new SerialPort("STUART");

            port.BaudRate = 9600;
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.ReadTimeout = SerialPort.InfiniteTimeout;
            port.WriteTimeout = SerialPort.InfiniteTimeout;

            port.Open();

            port.Write("Hello World!");

            byte[] writeBuf = new byte[1];

            for (int i = 0; i < 100; i++)
            {
                var stopwatch = new System.Diagnostics.Stopwatch();

                stopwatch.Start();

                const int total = 32 * 1024;

                int ch = 0;

                for (int j = 0; j < total; j++)
                {
                    writeBuf[0] = (byte)(48 + ch);

                    port.Write(writeBuf, 0, 1);

                    ch = (ch + 1) % 32;
                }

                stopwatch.Stop();

                port.Write("\r\n");
                port.Write("\r\n");

                port.Write(string.Format("Took {0} to send {1} bytes", stopwatch.ElapsedMilliseconds, total));

                ////            byte[] buf = new byte[64];
                ////
                ////            while(true)
                ////            {
                ////                for(int i = 0; i < 64; i++)
                ////                {
                ////                    buf[i] = port.Read();
                ////                }
                ////
                ////                //port.Write( (byte)'>' );
                ////                for(int i = 0; i < 64; i++)
                ////                {
                ////                    port.Write( buf[i] );
                ////                }
                ////                //port.Write( (byte)'<' );
                ////            }
                ////
                ////            GC.Collect();
            }
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
