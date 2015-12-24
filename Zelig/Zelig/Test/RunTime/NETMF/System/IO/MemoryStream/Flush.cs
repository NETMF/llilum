////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;




namespace Microsoft.Zelig.Test
{
    public class Flush : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;             
        }

        [TearDown]
        public void CleanUp()
        {
        }
        
        public override TestResult Run( string[] args )
        {
            TestResult result = TestResult.Pass;
            
            result |= Assert.CheckFailed( VerifyFlush( ) );

            return result;
        }

        [TestMethod]
        public TestResult VerifyFlush()
        {
            TestResult result = TestResult.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] data = Utilities.GetRandomBytes(5000);
                    ms.Write(data, 0, data.Length);
                    ms.Flush();
                    if (ms.Length != 5000)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected 5000 bytes, but got " + ms.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                result = TestResult.Fail;
                Log.Exception("Unexpected exception", ex);
            }
            return result;
        }
    }
}
