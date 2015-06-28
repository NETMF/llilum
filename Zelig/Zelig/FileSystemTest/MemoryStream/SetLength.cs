////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;




namespace FileSystemTest
{
    public class SetLength : IMFTestInterface
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
        public MFTestResults ObjectDisposed()
        {
            MemoryStream ms = new MemoryStream();
            ms.Close();

            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    long length = ms.Length;
                    Log.Exception( "Expected ObjectDisposedException, but got length " + length );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException) 
                { /*Pass Case */
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
        public MFTestResults LengthTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Set initial length to 50, and position to 50");
                    ms.SetLength(50);
                    ms.Position = 50;
                    if (!TestLength(ms, 50))
                        return MFTestResults.Fail;

                    Log.Comment("Write 'foo bar'");
                    StreamWriter sw = new StreamWriter(ms);
                    sw.Write("foo bar");
                    sw.Flush();
                    if (!TestLength(ms, 57))
                        return MFTestResults.Fail;

                    Log.Comment("Shorten Length to 30");
                    ms.SetLength(30);
                    if (!TestLength(ms, 30))
                        return MFTestResults.Fail;

                    Log.Comment("Verify position was adjusted");
                    if (!TestPosition(ms, 30))
                        return MFTestResults.Fail;

                    Log.Comment("Extend length to 100");
                    ms.SetLength(100);
                    if (!TestLength(ms, 100))
                        return MFTestResults.Fail;
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
        public MFTestResults InvalidSetLength()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    try
                    {
                        Log.Comment("-1");
                        ms.SetLength(-1);
                        Log.Exception( "Expected ArgumentOutOfRangeException, but set length" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }

                    try
                    {
                        Log.Comment("-10000");
                        ms.SetLength(-10000);
                        Log.Exception( "Expected ArgumentOutOfRangeException, but set length" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }

                    try
                    {
                        Log.Comment("long.MinValue");
                        ms.SetLength(long.MinValue);
                        Log.Exception( "Expected ArgumentOutOfRangeException, but set length" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }

                    try
                    {
                        Log.Comment("long.MaxValue");
                        ms.SetLength(long.MaxValue);
                        Log.Exception( "Expected IOException, but set length" );
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
                Log.Exception("Unexpected exception: " + ex.Message);
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
                    new MFTestMethod( LengthTests, "LengthTests" ),
                    new MFTestMethod( InvalidSetLength, "InvalidSetLength" ),
                };
             }
        }
    }
}
