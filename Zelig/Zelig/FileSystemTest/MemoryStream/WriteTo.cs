////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class WriteTo : IMFTestInterface
    {
        private bool _fileSystemInit;

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
                _fileSystemInit = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system", ex);
                _fileSystemInit = false;
            }
            return InitializeResult.ReadyToGo;              
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArgs()
        {
            MFTestResults result = MFTestResults.Pass;
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
                        Log.Exception( "Expected ArgumentNullException" );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentNullException ane) 
                    { 
                        /* pass case */ 
                        Log.Comment( "Got correct exception: " + ane.Message );
                        result = MFTestResults.Pass;
                    }

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
                        catch (NotSupportedException nse) 
                        { 
                            /* pass case */ Log.Comment( "Got correct exception: " + nse.Message );
                            result = MFTestResults.Pass;
                        }
                    }

                    try
                    {
                        Log.Comment("Target Stream closed");
                        MemoryStream mst = new MemoryStream();
                        mst.Close();
                        ms.WriteTo(mst);
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
                        Log.Comment("Current Stream closed");
                        ms.Close();
                        using (MemoryStream mst = new MemoryStream())
                        {
                            ms.WriteTo(mst);
                            Log.Exception( "Expected ObjectDisposedException" );
                            return MFTestResults.Fail;
                        }
                    }
                    catch (ObjectDisposedException ode) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + ode.Message );
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
        public MFTestResults WriteTo_FileStream()
        {
            // Don't run test if no FileSystem
            if (!_fileSystemInit)
                return MFTestResults.Skip;

            MFTestResults result = MFTestResults.Pass;
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
                            Log.Exception( "Expected 1234 bytes, but got " + fs.Length );
                            return MFTestResults.Fail;
                        }
                        if (!MemoryStreamHelper.VerifyRead(fs))
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

        [TestMethod]
        public MFTestResults WriteTo_MemoryStream()
        {
            MFTestResults result = MFTestResults.Pass;
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
                            Log.Exception( "Expected 1234 bytes, but got " + ms2.Length );
                            return MFTestResults.Fail;
                        }
                        ms2.Position = 0;
                        if (!MemoryStreamHelper.VerifyRead(ms2))
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
                    new MFTestMethod( InvalidArgs, "InvalidArgs" ),
                    new MFTestMethod( WriteTo_FileStream, "WriteTo_FileStream" ),
                    new MFTestMethod( WriteTo_MemoryStream, "WriteTo_MemoryStream" ),
                };
             }
        }
    }
}
