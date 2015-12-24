////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class CanWrite : TestBase, ITestInterface
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
            
            result |= Assert.CheckFailed( CanWrite_Default_Ctor( ) );
            result |= Assert.CheckFailed( CanWrite_Byte_Ctor( ) );

            return result;
        }

        [TestMethod]
        public TestResult CanWrite_Default_Ctor()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify CanWrite is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanWrite)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected CanWrite == true, but got CanWrite == false");
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
        public TestResult CanWrite_Byte_Ctor()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify CanWrite is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanWrite)
                    {
                        result = TestResult.Fail;
                        Log.Exception("Expected CanWrite == true, but got CanWrite == false");
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
