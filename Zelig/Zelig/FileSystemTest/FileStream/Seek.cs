////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;




namespace FileSystemTest
{
    public class Seek : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            // These tests rely on underlying file system so we need to make
            // sure we can format it before we start the tests.  If we can't
            // format it, then we assume there is no FS to test on this platform.

            // delete the directory DOTNETMF_FS_EMULATION
            try
            {
                IOTests.IntializeVolume();
                Directory.CreateDirectory(TestDir);
                Directory.SetCurrentDirectory(TestDir);
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system " + ex.Message);
                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }


        [TearDown]
        public void CleanUp()
        {
        }

        #region local vars
        private const string TestDir = "Seek";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Helper methods
        private bool TestSeek(FileStream fs, long offset, SeekOrigin origin, long expectedPosition)
        {
            bool result = true;
            long seek = fs.Seek(offset, origin);
            if (seek != fs.Position && seek != expectedPosition)
            {
                result = false;
                Log.Exception("Unexpected seek results!");
                Log.Exception("Expected position: " + expectedPosition);
                Log.Exception("Seek result: " + seek);
                Log.Exception("fs.Position: " + fs.Position);
            }
            return result;
        }

        private bool TestExtend(FileStream fs, long offset, SeekOrigin origin, long expectedPosition, long expectedLength)
        {
            bool result = TestSeek(fs, offset, origin, expectedLength);
            fs.WriteByte(1);
            if (fs.Length != expectedLength)
            {
                result = false;
                Log.Exception("Expected seek past end to change length to " + expectedLength + ", but its " + fs.Length);
            }
            return result;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidCases()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            FileStream fs = new FileStream(fileName, FileMode.Create);
            FileStreamHelper.Write(fs, 1000);
            long seek;
            
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Seek -1 from Begin");
                    seek = fs.Seek(-1, SeekOrigin.Begin);
                    Log.Exception( "Expected IOException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Seek -1001 from Current - at end from write");
                    seek = fs.Seek(-1001, SeekOrigin.Current);
                    Log.Exception( "Expected IOException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Seek -1001 from End");
                    seek = fs.Seek(-1001, SeekOrigin.End);
                    Log.Exception( "Expected IOException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Seek invalid -1 origin");
                    seek = fs.Seek(1, (SeekOrigin)(-1));
                    Log.Exception( "Expected ArgumentException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Seek invalid 10 origin");
                    seek = fs.Seek(1, (SeekOrigin)10);
                    Log.Exception( "Expected ArgumentException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Seek with closed socket");
                    fs.Close();
                    seek = fs.Seek(0, SeekOrigin.Begin);
                    Log.Exception( "Expected ObjectDisposedException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException ode) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ode.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Seek with disposed socket");
                    fs.Dispose();
                    seek = fs.Seek(0, SeekOrigin.End);
                    Log.Exception( "Expected ObjectDisposedException, but got position " + seek );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException ode) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ode.Message );
                    result = MFTestResults.Pass;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }

            return result;
        }

        [TestMethod]
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    FileStreamHelper.Write(fs, 1000);

                    Log.Comment("Seek to beginning");
                    if (!TestSeek(fs, 0, SeekOrigin.Begin, 0))
                        return MFTestResults.Fail;

                    Log.Comment("Seek forward offset from begging");
                    if (!TestSeek(fs, 10, SeekOrigin.Begin, 0))
                        return MFTestResults.Fail;

                    Log.Comment("Seek backwards offset from current");
                    if (!TestSeek(fs, -5, SeekOrigin.Current, 5))
                        return MFTestResults.Fail;

                    Log.Comment("Seek forwards offset from current");
                    if (!TestSeek(fs, 20, SeekOrigin.Current, 25))
                        return MFTestResults.Fail;

                    Log.Comment("Seek to end");
                    if (!TestSeek(fs, 0, SeekOrigin.End, 1000))
                        return MFTestResults.Fail;

                    Log.Comment("Seek backwards offset from end");
                    if (!TestSeek(fs, -35, SeekOrigin.End, 965))
                        return MFTestResults.Fail;

                    Log.Comment("Seek past end relative to End");
                    if (!TestExtend(fs, 1, SeekOrigin.End, 1001, 1002))
                        return MFTestResults.Fail;

                    Log.Comment("Seek past end relative to Begin");
                    if (!TestExtend(fs, 1002, SeekOrigin.Begin, 1002, 1003))
                        return MFTestResults.Fail;

                    Log.Comment("Seek past end relative to Current");
                    if (!TestSeek(fs, 995, SeekOrigin.Begin, 995))
                        return MFTestResults.Fail;
                    if (!TestExtend(fs, 10, SeekOrigin.Current, 1005, 1006))
                        return MFTestResults.Fail;

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
                            Log.Comment( "Position " + i + ":" + buff[i] + " != " + verify[i] );
                            return MFTestResults.Fail;
                        }
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
                    new MFTestMethod( InvalidCases, "InvalidCases" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                };
             }
        }
    }
}
