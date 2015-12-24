////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class ReadByte : TestBase, ITestInterface
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
            
            //result |= Assert.CheckFailed( InvalidCases( ) );
            result |= Assert.CheckFailed( VanillaCases( ) );

            return result;
        }

        #region Test Cases
        [TestMethod]
        public TestResult InvalidCases()
        {
            TestResult result = TestResult.Pass;

            try
            {
                MemoryStream ms2 = new MemoryStream();
                MemoryStreamHelper.Write(ms2, 100);
                ms2.Seek(0, SeekOrigin.Begin);
                ms2.Close();

                Log.Comment("Read from closed stream");
                try
                {
                    int readBytes = ms2.ReadByte();
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException, but read " + readBytes + " bytes");
                }
                catch (ObjectDisposedException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult VanillaCases()
        {
            TestResult result = TestResult.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    MemoryStreamHelper.Write(ms, 256);
                    ms.Position = 0;
                    Log.Comment("ReadBytes and verify");
                    for (int i = 0; i < 256; i++)
                    {
                        int b = ms.ReadByte();
                        if (b != i)
                        {
                            result = TestResult.Fail;
                            Log.Exception("Expected " + i + " but got " + b);
                        }
                    }

                    Log.Comment("Bytes past EOS should return -1");
                    int rb = ms.ReadByte();
                    if (rb != -1)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected -1 but got " + rb);
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
        #endregion Test Cases
    }
}
