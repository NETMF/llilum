////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;



namespace Microsoft.Zelig.Test
{
    public class MemoryStream_Ctor : TestBase, ITestInterface
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
            
            //result |= Assert.CheckFailed( InvalidArguments( ) );
            result |= Assert.CheckFailed( Valid_Default_Ctor( ) );
            //result |= Assert.CheckFailed( Variable_Buffer_Ctor( ) );

            return result;
        }

        #region Helper methods
        private bool ValidateMemoryStream(MemoryStream ms, int ExpectedLength)
        {
            bool success = true;
            Log.Comment("Check Length");
            if (ms.Length != ExpectedLength)
            {
                success = false;
                Log.Exception("Expected Length 0, but got Length " + ms.Length);
            }
            Log.Comment("Check CanSeek");
            if (!ms.CanSeek)
            {
                success = false;
                Log.Exception("Expected CanSeek to be true, but was false");
            }
            Log.Comment("Check CanRead");
            if (!ms.CanRead)
            {
                success = false;
                Log.Exception("Expected CanRead to be true, but was false");
            } Log.Comment("Check CanWrite");
            if (!ms.CanWrite)
            {
                success = false;
                Log.Exception("Expected CanWrite to be true, but was false");
            }
            if (ms.Position != 0)
            {
                success = false;
                Log.Exception("Expected Position to be 0, but was " + ms.Position);
            }
            return success;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public TestResult InvalidArguments()
        {
            TestResult result = TestResult.Pass;
            try
            {
                try
                {
                    Log.Comment("null buffer");
                    using (MemoryStream fs = new MemoryStream(null)) { }
                    result = TestResult.Fail;
                    Log.Exception("Expected ArgumentException");
                }
                catch (ArgumentException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception",  ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult Valid_Default_Ctor()
        {
            TestResult result = TestResult.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!ValidateMemoryStream(ms, 0))
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
        public TestResult Variable_Buffer_Ctor()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Verify buffer constructors length 0-100");
                for (int i = 0; i < 100; i++)
                {
                    byte[] buffer = new byte[i];
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        if (!ValidateMemoryStream(ms, i))
                            result = TestResult.Fail;

                        Log.Comment("Try to extend beyond buffer length");
                        try 
                        { 
                            ms.SetLength(i + 1);
                            result = TestResult.Fail;
                            Log.Exception("Expected NotSupportedException");
                        }
                        catch (NotSupportedException) { /* pass case */ }

                        Log.Comment("Truncate to 0");
                        for (int j = buffer.Length; j >= 0; j--)
                        {
                            ms.SetLength(j);
                        }
                    }
                }
                Log.Comment("Verify 10k buffer constructor");
                byte[] largeBuffer = new byte[10000];
                using (MemoryStream ms = new MemoryStream(largeBuffer))
                {
                    if (!ValidateMemoryStream(ms, largeBuffer.Length))
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
