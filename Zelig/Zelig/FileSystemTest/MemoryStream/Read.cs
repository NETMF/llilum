////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;

namespace FileSystemTest
{
    public class MemoryStreamRead : IMFTestInterface
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

        #region Local Helper methods
        private bool TestRead(MemoryStream ms, int length)
        {
            return TestRead(ms, length, length, length);
        }
        private bool TestRead(MemoryStream ms, int BufferLength, int BytesToRead, int BytesExpected)
        {
            bool result = true;
            byte nextbyte = (byte)(ms.Position & 0xFF);

            byte[] byteBuffer = new byte[BufferLength];

            int bytesRead = ms.Read(byteBuffer, 0, BytesToRead);
            if (bytesRead != BytesExpected)
            {
                result = false;
                Log.Exception("Expected " + BytesToRead + " bytes, but got " + bytesRead + " bytes");
            }

            for (int i = 0; i < bytesRead; i++)
            {
                if (byteBuffer[i] != nextbyte)
                {
                    result = false;
                    Log.Exception("Byte in position " + i + " has wrong value: " + byteBuffer[i]);
                }
                nextbyte++;
            }
            return result;
        }
        #endregion Local Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidCases()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("null Buffer");
                    try
                    {
                        int read = ms.Read(null, 0, 0);
                        Log.Exception( "Expected ArgumentNullException, but read " + read + " bytes" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentNullException ane) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + ane.Message );
                        result = MFTestResults.Pass;
                    }

                    Log.Comment("negative offset");
                    try
                    {
                        int read = ms.Read(new byte[]{1}, -1, 0);
                        Log.Exception( "Expected ArgumentOutOfRangeException, but read " + read + " bytes" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }

                    Log.Comment("negative count");
                    try
                    {
                        int read = ms.Read(new byte[] { 1 }, 0, -1);
                        Log.Exception( "Expected ArgumentOutOfRangeException, but read " + read + " bytes" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentOutOfRangeException aoore) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + aoore.Message );
                        result = MFTestResults.Pass;
                    }

                    Log.Comment("offset exceeds buffer size");
                    try
                    {
                        int read = ms.Read(new byte[] { 1 }, 2, 0);
                        Log.Exception( "Expected ArgumentException, but read " + read + " bytes" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentException ae) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                        result = MFTestResults.Pass;
                    }
                    
                    Log.Comment("count exceeds buffer size");
                    try
                    {
                        int read = ms.Read(new byte[] { 1 }, 0, 2);
                        Log.Exception( "Expected ArgumentException, but read " + read + " bytes" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentException ae) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                        result = MFTestResults.Pass;
                    }
                }

                MemoryStream ms2 = new MemoryStream();
                MemoryStreamHelper.Write(ms2, 100);
                ms2.Seek(0, SeekOrigin.Begin);
                ms2.Close();

                Log.Comment("Read from closed stream");
                try
                {
                    int readBytes = ms2.Read(new byte[] { 50 }, 0, 50);
                    Log.Exception( "Expected ObjectDisposedException, but read " + readBytes + " bytes" );
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
                Log.Exception("Unexpected exception", ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VanillaRead()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Write to stream then reset to beginning
                    MemoryStreamHelper.Write(ms, 1000);
                    ms.Seek(0, SeekOrigin.Begin);

                    Log.Comment("Read 256 bytes of data");
                    if (!TestRead(ms, 256))
                        return MFTestResults.Fail;

                    Log.Comment("Request less bytes then buffer");
                    if (!TestRead(ms, 256, 100, 100))
                        return MFTestResults.Fail;

                    // 1000 - 256 - 100 = 644
                    Log.Comment("Request more bytes then file");
                    if (!TestRead(ms, 1000, 1000, 644))
                        return MFTestResults.Fail;

                    Log.Comment("Request bytes after EOF");
                    if (!TestRead(ms, 100, 100, 0))
                        return MFTestResults.Fail;

                    Log.Comment("Rewind and read entire file in one buffer larger then file");
                    ms.Seek(0, SeekOrigin.Begin);
                    if (!TestRead(ms, 1001, 1001, 1000))
                        return MFTestResults.Fail;

                    Log.Comment("Rewind and read from middle");
                    ms.Position = 500;
                    if (!TestRead(ms, 256))
                        return MFTestResults.Fail;

                    Log.Comment("Read position after EOS");
                    ms.Position = ms.Length + 10;
                    if (!TestRead(ms, 100, 100, 0))
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
                    new MFTestMethod( InvalidCases, "InvalidCases" ),
                    new MFTestMethod( VanillaRead, "VanillaRead" ),
                };
             }
        }
    }
}
