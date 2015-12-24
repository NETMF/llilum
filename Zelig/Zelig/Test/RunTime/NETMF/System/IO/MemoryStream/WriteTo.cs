////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;




namespace Microsoft.Zelig.Test
{
    public class WriteTo : TestBase, ITestInterface
    {
        private bool _fileSystemInit;

        [SetUp]
        public InitializeResult Initialize()
        {
            // These tests rely on underlying file system so we need to make
            // sure we can format it before we start the tests.  If we can't
            // format it, then we assume there is no FS to test on this platform.

            //////// delete the directory DOTNETMF_FS_EMULATION
            //////try
            //////{
            //////    IOTests.IntializeVolume();
            //////    _fileSystemInit = true;
            //////}
            //////catch (Exception ex)
            //////{
            //////    Log.Exception("Skipping: Unable to initialize file system", ex);
            //////    _fileSystemInit = false;
            //////}
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
            
            //result |= Assert.CheckFailed( InvalidArgs( ) );
            //result |= Assert.CheckFailed( WriteTo_FileStream( ) );
            result |= Assert.CheckFailed( WriteTo_MemoryStream( ) );

            return result;
        }

        #region Test Cases

        [TestMethod]
        public TestResult InvalidArgs()
        {
            TestResult result = TestResult.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Initialize stream");
                    MemoryStreamHelper.Write(ms, 1000);

                    try
                    {
                        Log.Comment("null stream");
                        ms.WriteTo(null);
                        result = TestResult.Fail;
                        Log.Exception("Expected ArgumentNullException");
                    }
                    catch (ArgumentNullException) { /* pass case */ }

                    if (_fileSystemInit)
                    {
                        try
                        {
                            Log.Comment("pass in read-only stream");
                            using (FileStream fs = new FileStream("readonly", FileMode.OpenOrCreate, FileAccess.Read))
                            {
                                ms.WriteTo(fs);
                            }
                        }
                        catch (NotSupportedException) { /* pass case */ }
                    }

                    try
                    {
                        Log.Comment("Target Stream closed");
                        MemoryStream mst = new MemoryStream();
                        mst.Close();
                        ms.WriteTo(mst);
                        result = TestResult.Fail;
                        Log.Exception("Expected ObjectDisposedException");
                    }
                    catch (ObjectDisposedException) { /* pass case */ }

                    try
                    {
                        Log.Comment("Current Stream closed");
                        ms.Close();
                        using (MemoryStream mst = new MemoryStream())
                        {
                            ms.WriteTo(mst);
                            result = TestResult.Fail;
                            Log.Exception("Expected ObjectDisposedException");
                        }
                    }
                    catch (ObjectDisposedException) { /* pass case */ }
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
        public TestResult WriteTo_FileStream()
        {
            // Don't run test if no FileSystem
            if (!_fileSystemInit)
                return TestResult.Skip;

            TestResult result = TestResult.Pass;
            string fileName = "WriteTo_FileStream.txt";
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Initialize stream with 1234 bytes");
                    MemoryStreamHelper.Write(ms, 1234);

                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        Log.Comment("WriteTo FileStream");
                        ms.WriteTo(fs);
                    }

                    Log.Comment("Verify closed file");
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        if (fs.Length != 1234)
                        {
                            result = TestResult.Fail;
                            Log.Exception("Expected 1234 bytes, but got " + fs.Length);
                        }
                        if (!MemoryStreamHelper.VerifyRead(fs))
                            result = TestResult.Fail;
                    }
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
        public TestResult WriteTo_MemoryStream()
        {
            TestResult result = TestResult.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Log.Comment("Initialize stream with 1234 bytes");
                    MemoryStreamHelper.Write(ms, 1234);

                    using (MemoryStream ms2 = new MemoryStream())
                    {
                        Log.Comment("WriteTo MemoryStream");
                        ms.WriteTo(ms2);

                        Log.Comment("Verify 2nd MemoryStream");
                        if (ms2.Length != 1234)
                        {
                            result = TestResult.Fail;
                            Log.Exception("Expected 1234 bytes, but got " + ms2.Length);
                        }
                        ms2.Position = 0;
                        if (!MemoryStreamHelper.VerifyRead(ms2))
                            result = TestResult.Fail;
                    }
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
