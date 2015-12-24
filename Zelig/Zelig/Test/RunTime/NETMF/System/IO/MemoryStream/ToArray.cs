////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class ToArray : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // TODO: Add your set up steps here.  
            return InitializeResult.ReadyToGo;               
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }
        
        public override TestResult Run( string[] args )
        {
            TestResult result = TestResult.Pass;
            
            result |= Assert.CheckFailed( Ctor_ToArray( ) );
            result |= Assert.CheckFailed( VerifyValues( ) );
            result |= Assert.CheckFailed( ChangeLengths( ) );

            return result;
        }

        #region Helper methods
        private bool VerifyArray(byte[] data, int expected)
        {
            bool result = true;

            Log.Comment("Verify Length");
            if (data.Length != expected)
            {
                result = false;
                Log.Exception("Expected " + expected + " bytes, but got " + data.Length);
            }

            Log.Comment("Verify pattern in array");
            int nextbyte = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != nextbyte)
                {
                    result = false;
                    Log.Exception("Byte in position " + i + " has wrong value: " + data[i]);
                }

                // Reset if wraps past 255
                if (++nextbyte > 255)
                    nextbyte = 0;
            } 
            return result;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public TestResult Ctor_ToArray()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Dynamic Stream");
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] stream = ms.ToArray();
                    if (stream.Length != 0)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected length 0, but got length " + stream.Length);
                    }
                }

                Log.Comment("Static Stream");
                using (MemoryStream ms = new MemoryStream(new byte[512]))
                {
                    byte[] stream = ms.ToArray();
                    if (stream.Length != 512)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected length 512, but got length " + stream.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult VerifyValues()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify Data");
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Write 1000 bytes in specific pattern");
                    MemoryStreamHelper.Write(ms, 1000);
                    byte[] stream = ms.ToArray();

                    if (!VerifyArray(stream, 1000))
                        result = TestResult.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult ChangeLengths()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify array is still valid after truncation (copy array)");
                using (MemoryStream ms = new MemoryStream())
                {
                    MemoryStreamHelper.Write(ms, 1000);
                    ms.SetLength(200);
                    ms.Flush();
                    byte[] stream = ms.ToArray();
                    if (!VerifyArray(stream, 200))
                        result = TestResult.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        #endregion Test Cases
    }
}
