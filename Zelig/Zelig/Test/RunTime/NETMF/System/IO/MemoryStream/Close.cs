////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class Close : TestBase, ITestInterface
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
            
            //result |= Assert.CheckFailed( VerifyClose( ) );

            return result;
        }

        [TestMethod]
        public TestResult VerifyClose()
        {
            TestResult result = TestResult.Pass;
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.WriteByte(0);
                Log.Comment("Close stream");
                ms.Close();

                try
                {
                    Log.Comment("Verify actually closed by writing to it");
                    ms.WriteByte(0);
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException");
                }
                catch (ObjectDisposedException) { /* pass case */ }
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
