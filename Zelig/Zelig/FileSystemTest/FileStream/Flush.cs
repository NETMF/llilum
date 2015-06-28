////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class Flush : IMFTestInterface
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
        private const string TestDir = "Flush";
        private const string fileName = "test.tmp";
        private const string sourceFile = "test.src";
        #endregion local vars

        #region Test Cases
        [TestMethod]
        public MFTestResults Flush_Closed()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Create);
                FileStreamHelper.Write(fs, 500);
                fs.Close();
                try
                {
                    fs.Flush();
                }
                catch (ObjectDisposedException ode) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ode.Message );
                    result = MFTestResults.Pass;
                }
            }
            catch (Exception ex)
            {
                // previous Bug# 21655
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Flush_WriteByte()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify flush of 5000k using WriteByte");
                using (FileStream fs = new FileStream(sourceFile, FileMode.CreateNew))
                {
                    FileStreamHelper.Write(fs, 5000);
                    fs.Flush();
                    if (fs.Length != 5000)
                    {
                        Log.Comment( "Expected 5000 bytes in file" );
                        return MFTestResults.Fail;
                    }
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
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
        public MFTestResults Flush_Write()
        {
            Log.Comment("This test case requires the previous test case to write a properly formatted 5k file");
            if (!FileStreamHelper.VerifyFile(sourceFile, 5000))
                return MFTestResults.Skip;

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // read bytes from source file
                byte[] data = File.ReadAllBytes(sourceFile);

                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    if (fs.Length != 5000)
                    {
                        Log.Comment( "Expected 5000 bytes in file" );
                        return MFTestResults.Fail;
                    }
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
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
        #endregion Test Cases

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( Flush_Closed, "Flush_Closed" ),
                    new MFTestMethod( Flush_WriteByte, "Flush_WriteByte" ),
                    new MFTestMethod( Flush_Write, "Flush_Write" ),
                };
             }
        }
    }
}
