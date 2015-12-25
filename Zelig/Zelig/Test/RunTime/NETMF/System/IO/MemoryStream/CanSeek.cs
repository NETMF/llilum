////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class CanSeek : TestBase, ITestInterface
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
            
            result |= Assert.CheckFailed( CanSeek_Default_Ctor( ) );
            result |= Assert.CheckFailed( CanSeek_Byte_Ctor( ) );

            return result;
        }

        [TestMethod]
        public TestResult CanSeek_Default_Ctor()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify CanSeek is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanSeek)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected CanSeek == true, but got CanSeek == false");
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

        [TestMethod]
        public TestResult CanSeek_Byte_Ctor()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify CanSeek is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanSeek)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected CanSeek == true, but got CanSeek == false");
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
