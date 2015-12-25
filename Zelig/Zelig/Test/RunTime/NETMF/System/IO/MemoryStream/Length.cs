////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class Length : TestBase, ITestInterface
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
            
            //result |= Assert.CheckFailed( ObjectDisposed( ) );
            result |= Assert.CheckFailed( LengthTests( ) );

            return result;
        }

        #region Helper methods
        private bool TestLength(MemoryStream ms, long expectedLength)
        {
            if (ms.Length != expectedLength)
            {
                Log.Exception("Expected length " + expectedLength + " but got, " + ms.Length);
                return false;
            }
            return true;
        }

        private bool TestPosition(MemoryStream ms, long expectedPosition)
        {
            if (ms.Position != expectedPosition)
            {
                Log.Exception("Expected position " + expectedPosition + " but got, " + ms.Position);
                return false;
            }
            return true;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public TestResult ObjectDisposed()
        {
            TestResult result = TestResult.Pass;

            try
            {
                MemoryStream ms = new MemoryStream();
                ms.Close();
                
                try
                {
                    long length = ms.Length;
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got length " + length);
                }
                catch (ObjectDisposedException) { /*Pass Case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult LengthTests()
        {
            TestResult result = TestResult.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Set initial length to 50, and position to 50");
                    ms.SetLength(50);
                    ms.Position = 50;
                    if (!TestLength(ms, 50))
                        result = TestResult.Fail;

                    Log.Comment("Write 'foo bar'");
                    StreamWriter sw = new StreamWriter(ms);
                    sw.Write("foo bar");
                    sw.Flush();
                    if (!TestLength(ms, 57))
                        result = TestResult.Fail;

                    Log.Comment("Shorten Length to 30");
                    ms.SetLength(30);
                    if (!TestLength(ms, 30))
                        result = TestResult.Fail;

                    Log.Comment("Verify position was adjusted");
                    if (!TestPosition(ms, 30))
                        result = TestResult.Fail;

                    Log.Comment("Extend length to 100");
                    ms.SetLength(100);
                    if (!TestLength(ms, 100))
                        result = TestResult.Fail;
                }

                Log.Comment("Verify memorystream is 0 bytes after close");
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!TestLength(ms, 0))
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
