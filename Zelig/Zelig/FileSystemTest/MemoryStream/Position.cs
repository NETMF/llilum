////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;




namespace FileSystemTest
{
    public class Position : IMFTestInterface
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
        public MFTestResults ObjectDisposed()
        {
            MemoryStream ms = new MemoryStream();
            ms.Close();

            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    long position = ms.Position;
                    Log.Exception( "Expected ObjectDisposedException, but got position " + position );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException) 
                { 
                    /*Pass Case */
                    result = MFTestResults.Pass;
                }

                try
                {
                    ms.Position = 0;
                    Log.Exception( "Expected ObjectDisposedException, but set position" );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException) 
                { 
                    /*Pass Case */
                    result = MFTestResults.Pass;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults InvalidRange()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] buffer = new byte[100];
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    Log.Comment("Try -1 postion");
                    try
                    {
                        ms.Position = -1;
                        Log.Exception( "Expected ArgumentOutOfRangeException" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }

                    Log.Comment("Try Long.MinValue postion");
                    try
                    {
                        ms.Position = long.MinValue;
                        Log.Exception( "Expected ArgumentOutOfRangeException" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults GetSetStaticBuffer()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] buffer = new byte[1000];
                Log.Comment("Get/Set Position with static buffer");
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    if (!GetSetPosition(ms, buffer.Length))
                        return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults GetSetDynamicBuffer()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Get/Set Position with dynamic buffer");
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!GetSetPosition(ms, 1000))
                        return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        #endregion Test Cases

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( ObjectDisposed, "ObjectDisposed" ),
                    new MFTestMethod( InvalidRange, "InvalidRange" ),
                    new MFTestMethod( GetSetStaticBuffer, "GetSetStaticBuffer" ),
                    new MFTestMethod( GetSetDynamicBuffer, "GetSetDynamicBuffer" ),
                };
             }
        }
    }
}
