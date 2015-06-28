////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;

namespace FileSystemTest
{
    public class MemoryStreamWrite : IMFTestInterface
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

        //TODO Test with position longer then length


        #region Local Helper methods
        private bool TestWrite(MemoryStream ms, int length)
        {
            return TestWrite(ms, length, length, 0);
        }

        private bool TestWrite(MemoryStream ms, int length, long ExpectedLength)
        {
            return TestWrite(ms, length, length, 0, ExpectedLength);
        }

        private bool TestWrite(MemoryStream ms, int BufferLength, int BytesToWrite, int Offset)
        {
            return TestWrite(ms, BufferLength, BytesToWrite, Offset, ms.Position + BytesToWrite);
        }

        private bool TestWrite(MemoryStream ms, int BufferLength, int BytesToWrite, int Offset, long ExpectedLength)
        {
            bool result = true;
            long startLength = ms.Position;
            byte nextbyte = (byte)(startLength & 0xFF);

            byte[] byteBuffer = new byte[BufferLength];
            for (int i = Offset; i < (Offset + BytesToWrite); i++)
            {
                byteBuffer[i] = (byte)nextbyte;

                nextbyte++;
            }

            ms.Write(byteBuffer, Offset, BytesToWrite);
            ms.Flush();
            if (ExpectedLength < ms.Length)
            {
                result = false;
                Log.Exception("Expeceted final length of " + ExpectedLength + " bytes, but got " + ms.Length + " bytes");
            }
            return result;
        }
        #endregion Local Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidCases()
        {
            MemoryStream fs = new MemoryStream();
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
                    fs = new MemoryStream();
                    fs.Dispose();
                    fs.Write(writebuff, 0, writebuff.Length);
                    Log.Exception( "Expected ObjectDisposedException" );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException ode) 
                { 
                    /* pass case */

                    Log.Comment( "Got correct exception: " + ode.Message );
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
        public MFTestResults VanillaWrite_Dynamic_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Write 256 bytes of data");
                    if (!TestWrite(ms, 256))
                        return MFTestResults.Fail;

                    Log.Comment("Write middle of buffer");
                    if (!TestWrite(ms, 256, 100, 100))
                        return MFTestResults.Fail;

                    // 1000 - 256 - 100 = 644
                    Log.Comment("Write start of buffer");
                    if (!TestWrite(ms, 1000, 644, 0))
                        return MFTestResults.Fail;

                    Log.Comment("Write end of buffer");
                    if (!TestWrite(ms, 1000, 900, 100))
                        return MFTestResults.Fail;

                    Log.Comment("Rewind and verify all bytes written");
                    ms.Seek(0, SeekOrigin.Begin);
                    if (!MemoryStreamHelper.VerifyRead(ms))
                        return MFTestResults.Fail;

                    Log.Comment("Verify Read validation with UTF8 string");
                    ms.SetLength(0);
                    string test = "MFFramework Test";
                    ms.Write(UTF8Encoding.UTF8.GetBytes(test), 0, test.Length);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] readbuff = new byte[20];
                    int readlen = ms.Read(readbuff, 0, readbuff.Length);
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

        [TestMethod]
        public MFTestResults VanillaWrite_Static_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    Log.Comment("Write 256 bytes of data");
                    if (!TestWrite(ms, 256, 1024))
                        return MFTestResults.Fail;

                    Log.Comment("Write middle of buffer");
                    if (!TestWrite(ms, 256, 100, 100, 1024))
                        return MFTestResults.Fail;

                    // 1000 - 256 - 100 = 644
                    Log.Comment("Write start of buffer");
                    if (!TestWrite(ms, 1000, 644, 0, 1024))
                        return MFTestResults.Fail;

                    Log.Comment("Write past end of buffer");
                    try
                    {
                        TestWrite(ms, 50, 1024);
                        Log.Exception( "Expected NotSupportedException" );
                        return MFTestResults.Fail;
                    }
                    catch (NotSupportedException nse) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + nse.Message );
                        result = MFTestResults.Pass;
                    }

                    Log.Comment("Verify failed Write did not move position");
                    if (ms.Position != 1000)
                    {
                        Log.Comment( "Expected position to be 1000, but it is " + ms.Position );
                        return MFTestResults.Fail;
                    }

                    Log.Comment("Write final 24 bytes of static buffer");
                    if (!TestWrite(ms, 24))
                        return MFTestResults.Fail;

                    Log.Comment("Rewind and verify all bytes written");
                    ms.Seek(0, SeekOrigin.Begin);
                    if (!MemoryStreamHelper.VerifyRead(ms))
                        return MFTestResults.Fail;

                    Log.Comment("Verify Read validation with UTF8 string");
                    ms.SetLength(0);
                    string test = "MFFramework Test";
                    ms.Write(UTF8Encoding.UTF8.GetBytes(test), 0, test.Length);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] readbuff = new byte[20];
                    int readlen = ms.Read(readbuff, 0, readbuff.Length);
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

        [TestMethod]
        public MFTestResults ShiftBuffer()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                int bufSize;
                int iCountErrors = 0;

                for (int i = 1; i < 10; i++)
                {
                    bufSize = i;

                    MemoryStream ms = new MemoryStream();
                    for (int j = 0; j < bufSize; ++j)
                        ms.WriteByte((byte)j);

                    // Move everything forward by 1 byte
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] buf = ms.ToArray();
                    ms.Write(buf, 1, bufSize - 1);

                    ms.Seek(0, SeekOrigin.Begin);
                    //we'll read till one before the last since these should be shifted by 1
                    for (int j = 0; j < ms.Length - 1; ++j)
                    {
                        int bit = ms.ReadByte();
                        if (bit != j + 1)
                        {
                            ++iCountErrors;
                            Log.Exception("Err_8324t! Check VSWhdibey #458551, Returned: " + bit + ", Expected: " + (j + 1));
                        }
                    }

                    //last bit should be the same
                    if (ms.ReadByte() != i - 1)
                    {
                        ++iCountErrors;
                        Log.Exception("Err_32947gs! Last bit is not correct Check VSWhdibey #458551");
                    }

                }

                //Buffer sizes of 9 (10 here since we shift by 1) and above doesn't have the above 'optimization' problem
                for (int i = 10; i < 64; i++)
                {
                    bufSize = i;

                    MemoryStream ms = new MemoryStream();
                    for (int j = 0; j < bufSize; ++j)
                        ms.WriteByte((byte)j);

                    // Move everything forward by 1 byte
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] buf = ms.ToArray();
                    ms.Write(buf, 1, bufSize - 1);

                    ms.Seek(0, SeekOrigin.Begin);
                    for (int j = 0; j < ms.Length; ++j)
                    {
                        int bit = ms.ReadByte();
                        if (j != ms.Length - 1)
                        {
                            if (bit != (j + 1))
                            {
                                ++iCountErrors;
                                Log.Exception("Err_235radg_" + i + "! Check VSWhdibey #458551, Returned: " + bit + ", Expected: " + (j + 1));
                            }
                        }
                        else
                            if (bit != j)
                            {
                                ++iCountErrors;
                                Log.Exception("Err_235radg_" + i + "! Check VSWhdibey #458551, Returned: " + bit + ", Expected:" + (j + 1));
                            }
                    }
                }
                if (iCountErrors > 0)
                    return MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults BoundaryCheck()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                for (int i = 250; i < 260; i++)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        TestWrite(ms, i);
                        ms.Position = 0;
                        if (!MemoryStreamHelper.VerifyRead(ms))
                            return MFTestResults.Fail;

                        Log.Comment("Position: " + ms.Position);
                        Log.Comment("Length: " + ms.Length);
                        if (i != ms.Position | i != ms.Length)
                        {
                            Log.Exception( "Expected Position and Length to be " + i );
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
                    new MFTestMethod( VanillaWrite_Dynamic_Ctor, "VanillaWrite_Dynamic_Ctor" ),
                    new MFTestMethod( VanillaWrite_Static_Ctor, "VanillaWrite_Static_Ctor" ),
                    new MFTestMethod( ShiftBuffer, "ShiftBuffer" ),
                    new MFTestMethod( BoundaryCheck, "BoundaryCheck" ),
                };
             }
        }
    }
}
