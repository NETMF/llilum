////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;



namespace Microsoft.Zelig.Test
{
    public class Position : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // TODO: Add your set up steps here.
            // if (Setup Fails)
            //    return InitializeResult.Skip;

            return InitializeResult.ReadyToGo;            
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }
        
        public override TestResult Run(string[] args)
        {
            TestResult result = TestResult.Pass;

            //result |= Assert.CheckFailed(ObjectDisposed(), "ObjectDisposed", 0);
            //result |= Assert.CheckFailed(InvalidRange(), "InvalidRange", 0);
            //result |= Assert.CheckFailed(GetSetStaticBuffer(), "GetSetStaticBuffer", 0);
            //result |= Assert.CheckFailed(GetSetDynamicBuffer(), "GetSetDynamicBuffer", 0);

            return result;
        }

        #region Helper methods
        private bool GetSetPosition(MemoryStream ms, int TestLength)
        {
            bool success = true;
            Log.Comment("Move forwards");
            for (int i = 0; i < TestLength; i++)
            {
                ms.Position = i;
                if (ms.Position != i)
                {
                    success = false;
                    Log.Exception("Expected position " + i + " but got position " + ms.Position);
                }
            }
            Log.Comment("Move backwards");
            for (int i = TestLength - 1; i >= 0; i--)
            {
                ms.Position = i;
                if (ms.Position != i)
                {
                    success = false;
                    Log.Exception("Expected position " + i + " but got position " + ms.Position);
                }
            } return success;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public TestResult ObjectDisposed()
        {
            MemoryStream ms = new MemoryStream();
            ms.Close();

            TestResult result = TestResult.Pass;
            try
            {
                try
                {
                    long position = ms.Position;
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got position " + position);
                }
                catch (ObjectDisposedException) { /*Pass Case */ }

                try
                {
                    ms.Position = 0;
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException, but set position");
                }
                catch (ObjectDisposedException) { /*Pass Case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult InvalidRange()
        {
            TestResult result = TestResult.Pass;
            try
            {
                byte[] buffer = new byte[100];
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    Log.Comment("Try -1 postion");
                    try
                    {
                        ms.Position = -1;
                        result = TestResult.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException");
                    }
                    catch (ArgumentOutOfRangeException) { /* pass case */ }

                    Log.Comment("Try Long.MinValue postion");
                    try
                    {
                        ms.Position = long.MinValue;
                        result = TestResult.Fail;
                        Log.Exception("Expected ArgumentOutOfRangeException");
                    }
                    catch (ArgumentOutOfRangeException) { /* pass case */ } 
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
        public TestResult GetSetStaticBuffer()
        {
            TestResult result = TestResult.Pass;
            try
            {
                byte[] buffer = new byte[1000];
                Log.Comment("Get/Set Position with static buffer");
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    if (!GetSetPosition(ms, buffer.Length))
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
        public TestResult GetSetDynamicBuffer()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("Get/Set Position with dynamic buffer");
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!GetSetPosition(ms, 1000))
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
