////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using System.IO;




namespace FileSystemTest
{
    public class Write : IMFTestInterface
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
        private const string TestDir = "Write";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Local Helper methods
        private bool TestWrite(FileStream fs, int length)
        {
            return TestWrite(fs, length, length, 0);
        }
        private bool TestWrite(FileStream fs, int BufferLength, int BytesToWrite, int Offset)
        {
            bool result = true;
            long startLength = fs.Position;
            byte nextbyte = (byte)(startLength & 0xFF);

            byte[] byteBuffer = new byte[BufferLength];
            for (int i = Offset; i < (Offset + BytesToWrite); i++)
            {
                byteBuffer[i] = (byte)nextbyte;

                nextbyte++;
            }

            fs.Write(byteBuffer, Offset, BytesToWrite);
            fs.Flush();
            if ((startLength + BytesToWrite) < fs.Length)
            {
                result = false;
                Log.Exception("Expeceted final length of " + (startLength + BytesToWrite) + " bytes, but got " + fs.Length + " bytes");
            }
            return result;
        }
        #endregion Local Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidCases()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            FileStream fs = new FileStream(fileName, FileMode.Create);
            byte[] writebuff = new byte[1024];
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Write to null buffer");
                    fs.Write(null, 0, writebuff.Length);
                    Log.Exception( "Expected ArgumentNullException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentNullException ane) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ane.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Write to negative offset");
                    fs.Write(writebuff, -1, writebuff.Length);
                    Log.Exception( "Expected ArgumentOutOfRangeException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentOutOfRangeException aoore) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Write to out of range offset");
                    fs.Write(writebuff, writebuff.Length + 1, writebuff.Length);
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Write negative count");
                    fs.Write(writebuff, 0, -1);
                    // previous Bug # 21669
                    Log.Exception( "Expected ArgumentOutOfRangeException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentOutOfRangeException aoore) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Write count larger then buffer");
                    fs.Write(writebuff, 0, writebuff.Length + 1);
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Write closed stream");
                    fs.Close();
                    fs.Write(writebuff, 0, writebuff.Length);
                    Log.Exception( "Expected ObjectDisposedException" );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException ode) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ode.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Write disposed stream");
                    fs = new FileStream(fileName, FileMode.Open);
                    fs.Dispose();
                    fs.Write(writebuff, 0, writebuff.Length);
                    Log.Exception( "Expected ObjectDisposedException" );
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
        public MFTestResults VanillaWrite()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    Log.Comment("Write 256 bytes of data");
                    if (!TestWrite(fs, 256))
                        return MFTestResults.Fail;

                    Log.Comment("Write middle of buffer");
                    if (!TestWrite(fs, 256, 100, 100))
                        return MFTestResults.Fail;

                    // 1000 - 256 - 100 = 644
                    Log.Comment("Write start of buffer");
                    if (!TestWrite(fs, 1000, 644, 0))
                        return MFTestResults.Fail;

                    Log.Comment("Write end of buffer");
                    if (!TestWrite(fs, 1000, 900, 100))
                        return MFTestResults.Fail;

                    Log.Comment("Rewind and verify all bytes written");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        return MFTestResults.Fail;

                    Log.Comment("Verify Read validation with UTF8 string");
                    fs.SetLength(0);
                    string test = "MFFramework Test";
                    fs.Write(UTF8Encoding.UTF8.GetBytes(test), 0, test.Length);
                    fs.Flush();
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] readbuff = new byte[20];
                    int readlen = fs.Read(readbuff, 0, readbuff.Length);
                    string testResult = new string(Encoding.UTF8.GetChars(readbuff, 0, readlen));
                    if (test != testResult)
                    {
                        Log.Comment( "Exepected: " + test + ", but got: " + testResult );
                        return MFTestResults.Fail;
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
                    new MFTestMethod( VanillaWrite, "VanillaWrite" ),
                };
             }
        }
    }
}
