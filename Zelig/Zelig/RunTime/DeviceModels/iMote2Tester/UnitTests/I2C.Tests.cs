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
    public class I2CBusTests : ITest
    {

        #region Scan Bus test
        /// <summary>
        /// scan bus test
        /// </summary>
        public void TestCase1()
        {
            int found = 0;
            ushort startFrom = 0;
            int clockRateKHz = 400;


            I2CDevice.Configuration config = new I2CDevice.Configuration(startFrom, clockRateKHz);
            using (I2CDevice i2cDevice = new I2CDevice(config))
            {
                bool found34, found49, found4a;
                found34 = found49 = found4a = false;

                //keep out 0 and 127)
                for (int i = 1; i < 126; i++)
                {
                    i2cDevice.Config = new I2CDevice.Configuration((ushort)(1 + ((i + startFrom) % 126)), 400);
                    I2CDevice.I2CTransaction unit1 = i2cDevice.CreateReadTransaction(new byte[] { 0 });

                    int len = i2cDevice.Execute(new I2CDevice.I2CTransaction[] { unit1 }, 200);
                    if (len > 0)
                    {
                        ASSERT(len == 1, true, "not all data sent");
                        found34 |= (i2cDevice.Config.Address == 0x34);
                        found49 |= (i2cDevice.Config.Address == 0x49);
                        found4a |= (i2cDevice.Config.Address == 0x4a);
                        found++;
                    }
                    else
                    {
                        ASSERT(len < 0, false, "illegal return value for execute");
                    }
                }

                ASSERT(found == 3, true, "more devices than expected");
                ASSERT(found34, true, "device 0x34 not found");
                ASSERT(found49, true, "device 0x49 not found");
                ASSERT(found4a, true, "device 0x4a not found");
            }
        }
        #endregion

        #region Write config data to AD converter
        /// <summary>
        /// scan bus test
        /// </summary>
        public void TestCase2()
        {
            I2CDevice.Configuration config = new I2CDevice.Configuration(0x34, 400);
            using (I2CDevice i2cDevice = new I2CDevice(config))
            {
                I2CDevice.I2CTransaction unit1 = i2cDevice.CreateWriteTransaction(new byte[] { 0, 0 });

                int len = i2cDevice.Execute(new I2CDevice.I2CTransaction[] { unit1 }, 200);
                ASSERT(len > 0, true, "not all data sent");
                ASSERT(len == 2, true, "illegal return value for execute");
            }
        }
        #endregion

        #region Read channel data from AD converter
        /// <summary>
        /// scan bus test
        /// </summary>
        public void TestCase3()
        {
            I2CDevice.Configuration config = new I2CDevice.Configuration(0x34, 400);
            using (I2CDevice i2cDevice = new I2CDevice(config))
            {
                I2CDevice.I2CTransaction unit1 = i2cDevice.CreateReadTransaction(new byte[] { 0, 0, 0, 0 });

                int len = i2cDevice.Execute(new I2CDevice.I2CTransaction[] { unit1 }, 200);
                ASSERT(len > 0, true, "not all data sent");
                ASSERT(len == 4, true, "illegal return value for execute");

                bool dataOk = false;
                for (int i = 0; i < unit1.Buffer.Length; i++)
                {
                    dataOk |= (unit1.Buffer[i] != 0);
                }

                ASSERT(dataOk, true, "no data from converter");
            }
        }
        #endregion

        #region check for illegal data
        /// <summary>
        /// scan bus test
        /// </summary>
        public void TestCase4()
        {
            I2CDevice.Configuration config = new I2CDevice.Configuration(0x34, 400);
            using (I2CDevice i2cDevice = new I2CDevice(config))
            {
                try
                {
                    i2cDevice.CreateReadTransaction(null);
                    ASSERT(true, false, "illegal data");
                }
                catch
                {
                    //expected
                }

                try
                {
                    i2cDevice.CreateWriteTransaction(null);
                    ASSERT(true, false, "illegal data");
                }
                catch
                {
                    //expected
                }


                try
                {
                    int len = i2cDevice.Execute(null, 200);
                    ASSERT(true, false, "illegal data");
                }
                catch
                {
                    //expected
                }

                try
                {

                    int len = i2cDevice.Execute(new I2CDevice.I2CTransaction[0], 200);
                    ASSERT(len == 0, true, "illegal data");
                }
                catch
                {
                    ASSERT(true, false, "illegal data");
                }
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

            try
            {
                TestCase2();
            }
            catch
            {
                ASSERT_NOT_NULL(null, "test case 2 - ");
            }

            try
            {
                TestCase3();
            }
            catch
            {
                ASSERT_NOT_NULL(null, "test case 3 - ");
            }

            try
            {
                TestCase4();
            }
            catch
            {
                ASSERT_NOT_NULL(null, "test case 4 - ");
            }
        }

        /// <summary>
        /// Retrieves the results of the output port test case
        /// </summary>
        public string Result { get; private set; }
        #endregion

    }

    #region Test helper classes
    public class I2CDevice : IDisposable
    {

        #region Constructor/Destructor
        public I2CDevice(I2CDevice.Configuration config)
        {
            this.Config = config;
        }

        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool fDisposing)
        {
        }
        #endregion

        #region Public API
        public Configuration Config;

        public int Execute(I2CDevice.I2CTransaction[] xActions, int timeout)
        {
            if (Config.Address == Microsoft.DeviceModels.Chipset.PXA27x.I2C.Instance.ISAR)
                return -1;// throw new ArgumentOutOfRangeException("Config.Address");

            if (null == xActions) throw new ArgumentNullException("xActions");
            if (0 == xActions.Length) return 0;

            I2CBus_Transaction action = new I2CBus_Transaction(Config.Address, Config.ClockRateKhz, xActions);
            I2C.Instance.StartTransactionAsMaster(action);

            if (!action.WaitForCompletion(timeout))
            {
                I2C.Instance.StopTransactionAsMaster();
                return 0;
            }

            return action.BytesTransacted;
        }

        public I2CDevice.I2CReadTransaction CreateReadTransaction(byte[] buffer)
        {
            return new I2CReadTransaction(buffer);
        }

        public I2CDevice.I2CWriteTransaction CreateWriteTransaction(byte[] buffer)
        {

            return new I2CWriteTransaction(buffer);
        }
        #endregion

        #region Nested classes
        public sealed class I2CReadTransaction : I2CTransaction
        {
            internal I2CReadTransaction(byte[] buffer)
                : base(true, buffer)
            {
            }
        }

        public sealed class I2CWriteTransaction : I2CTransaction
        {
            internal I2CWriteTransaction(byte[] buffer)
                : base(false, buffer)
            {
            }
        }

        public class I2CTransaction : I2CBus_TransactionUnit
        {
            internal I2CTransaction(bool isReadTransaction, byte[] buffer)
                : base(isReadTransaction, buffer)
            {
            }
        }

        public class Configuration
        {
            public readonly ushort Address;
            public readonly int ClockRateKhz;

            public Configuration(ushort address, int clockRateKhz)
            {
                this.Address = address;
                this.ClockRateKhz = clockRateKhz;
            }
        }
        #endregion

    }
    #endregion

}
