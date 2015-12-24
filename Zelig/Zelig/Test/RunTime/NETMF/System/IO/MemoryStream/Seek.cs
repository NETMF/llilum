////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;


namespace Microsoft.Zelig.Test
{
    public class Seek : TestBase, ITestInterface
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
            result |= Assert.CheckFailed( ValidCases( ) );

            return result;
        }

        #region Helper methods
        private bool TestSeek(MemoryStream ms, long offset, SeekOrigin origin, long expectedPosition)
        {
            bool result = true;
            long seek = ms.Seek(offset, origin);
            if (seek != ms.Position && seek != expectedPosition)
            {
                result = false;
                Log.Exception("Unexpected seek results!");
                Log.Exception("Expected position: " + expectedPosition);
                Log.Exception("Seek result: " + seek);
                Log.Exception("fs.Position: " + ms.Position);
            }
            return result;
        }

        private bool TestExtend(MemoryStream ms, long offset, SeekOrigin origin, long expectedPosition, long expectedLength)
        {
            bool result = TestSeek(ms, offset, origin, expectedLength);
            ms.WriteByte(1);
            if (ms.Length != expectedLength)
            {
                result = false;
                Log.Exception("Expected seek past end to change length to " + expectedLength + ", but its " + ms.Length);
            }
            return result;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public TestResult InvalidCases()
        {
            MemoryStream fs = new MemoryStream();
            MemoryStreamHelper.Write(fs, 1000);
            long seek;

            TestResult result = TestResult.Pass;
            try
            {
                try
                {
                    Log.Comment("Seek -1 from Begin");
                    seek = fs.Seek(-1, SeekOrigin.Begin);
                    result = TestResult.Fail;
                    Log.Exception("Expected IOException, but got position " + seek);
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment("Seek -1001 from Current - at end from write");
                    seek = fs.Seek(-1001, SeekOrigin.Current);
                    result = TestResult.Fail;
                    Log.Exception("Expected IOException, but got position " + seek);
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment("Seek -1001 from End");
                    seek = fs.Seek(-1001, SeekOrigin.End);
                    result = TestResult.Fail;
                    Log.Exception("Expected IOException, but got position " + seek);
                }
                catch (IOException) { /* pass case */ }

                try
                {
                    Log.Comment("Seek invalid -1 origin");
                    seek = fs.Seek(1, (SeekOrigin)(-1));
                    result = TestResult.Fail;
                    Log.Exception("Expected ArgumentException, but got position " + seek);
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Seek invalid 10 origin");
                    seek = fs.Seek(1, (SeekOrigin)10);
                    result = TestResult.Fail;
                    Log.Exception("Expected ArgumentException, but got position " + seek);
                }
                catch (ArgumentException) { /* pass case */ }

                try
                {
                    Log.Comment("Seek with closed stream");
                    fs.Close();
                    seek = fs.Seek(0, SeekOrigin.Begin);
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got position " + seek);
                }
                catch (ObjectDisposedException) { /* pass case */ }

                try
                {
                    Log.Comment("Seek with disposed stream");
                    fs.Dispose();
                    seek = fs.Seek(0, SeekOrigin.End);
                    result = TestResult.Fail;
                    Log.Exception("Expected ObjectDisposedException, but got position " + seek);
                }
                catch (ObjectDisposedException) { /* pass case */ }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = TestResult.Fail;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }

            return result;
        }

        [TestMethod]
        public TestResult ValidCases()
        {
            TestResult result = TestResult.Pass;
            try
            {
                using (MemoryStream fs = new MemoryStream())
                {
                    MemoryStreamHelper.Write(fs, 1000);

                    Log.Comment("Seek to beginning");
                    if (!TestSeek(fs, 0, SeekOrigin.Begin, 0))
                        result = TestResult.Fail;

                    Log.Comment("Seek forward offset from begging");
                    if (!TestSeek(fs, 10, SeekOrigin.Begin, 0))
                        result = TestResult.Fail;

                    Log.Comment("Seek backwards offset from current");
                    if (!TestSeek(fs, -5, SeekOrigin.Current, 5))
                        result = TestResult.Fail;

                    Log.Comment("Seek forwards offset from current");
                    if (!TestSeek(fs, 20, SeekOrigin.Current, 25))
                        result = TestResult.Fail;

                    Log.Comment("Seek to end");
                    if (!TestSeek(fs, 0, SeekOrigin.End, 1000))
                        result = TestResult.Fail;

                    Log.Comment("Seek backwards offset from end");
                    if (!TestSeek(fs, -35, SeekOrigin.End, 965))
                        result = TestResult.Fail;

                    Log.Comment("Seek past end relative to End");
                    if (!TestExtend(fs, 1, SeekOrigin.End, 1001, 1002))
                        result = TestResult.Fail;

                    Log.Comment("Seek past end relative to Begin");
                    if (!TestExtend(fs, 1002, SeekOrigin.Begin, 1002, 1003))
                        result = TestResult.Fail;

                    Log.Comment("Seek past end relative to Current");
                    if (!TestSeek(fs, 995, SeekOrigin.Begin, 995))
                        result = TestResult.Fail;
                    if (!TestExtend(fs, 10, SeekOrigin.Current, 1005, 1006))
                        result = TestResult.Fail;

                    // 1000 --123456
                    // verify 011001
                    Log.Comment("Verify proper bytes written at end (zero'd bytes from seek beyond end)");
                    byte[] buff = new byte[6];
                    byte[] verify = new byte[] { 0, 1, 1, 0, 0, 1 };
                    fs.Seek(-6, SeekOrigin.End);
                    fs.Read(buff, 0, buff.Length);
                    for (int i = 0; i < buff.Length; i++)
                    {
                        if (buff[i] != verify[i])
                        {
                            result = TestResult.Fail;
                            Log.Comment("Position " + i + ":" + buff[i] + " != " + verify[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                result = TestResult.Fail;
            }

            return result;
        }
        #endregion Test Cases
    }
}
