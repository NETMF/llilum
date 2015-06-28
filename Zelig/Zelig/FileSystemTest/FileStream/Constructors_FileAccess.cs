////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;




namespace FileSystemTest
{
    public class Constructors_FileAccess : IMFTestInterface
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
        private const string TestDir = "FileAccess";
        private const string fileName = "test.tmp";
        #endregion local vars

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Bad fileaccess -1");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, (FileAccess)(-1))) { }
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
                    Log.Comment("Bad fileaccess 10");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, (FileAccess)(10))) { }
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
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
        public MFTestResults FileAccess_Read()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Read access to the file. Data can be read from the file. 
                // Combine with Write for read/write access. 

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    Log.Comment("Should be able to read");
                    if (!FileStreamHelper.VerifyRead(fs))
                        return MFTestResults.Fail;

                    Log.Comment("Shouldn't be able to write");
                    try
                    {
                        fs.Write(new byte[] { 1, 2, 3 }, 0, 3);
                        Log.Exception( "Expected NotSupportedException" );
                        return MFTestResults.Fail;
                    }
                    catch (NotSupportedException nse) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + nse.Message );
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

        [TestMethod]
        public MFTestResults FileAccess_Write()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Write access to the file. Data can be written to the file. 
                // Combine with Read for read/write access.

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Write))
                {
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        return MFTestResults.Fail;

                    if (!FileStreamHelper.Write(fs, 500))
                        return MFTestResults.Fail;

                    Log.Comment("Shouldn't be able to read");
                    try
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        int data = fs.ReadByte();
                        Log.Exception( "Expected NotSupportedException" );
                        return MFTestResults.Fail;
                    }
                    catch (NotSupportedException nse) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + nse.Message );
                        result = MFTestResults.Pass;
                    }
                }

                // 300 bytes original + 1000 bytes + 500 bytes
                if (!FileStreamHelper.VerifyFile(fileName, 1800))
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
        public MFTestResults FileAccess_ReadWrite()
        {
            // Clean up incase file exists
            if (File.Exists(fileName))
                File.Delete(fileName);

            MFTestResults result = MFTestResults.Pass;
            try
            {
                // From MSDN:
                // Write access to the file. Data can be written to the file. 
                // Combine with Read for read/write access.

                Log.Comment("Create file for testing");
                using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
                {
                    FileStreamHelper.WriteReadEmpty(fs);
                }

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    Log.Comment("Should be able to write");
                    if (!FileStreamHelper.Write(fs, 1000))
                        return MFTestResults.Fail;

                    Log.Comment("Should be able to read");
                    fs.Seek(0, SeekOrigin.Begin);
                    if (!FileStreamHelper.VerifyRead(fs))
                        return MFTestResults.Fail;

                    Log.Comment("Write after read");
                    fs.Seek(0, SeekOrigin.End);
                    if (!FileStreamHelper.Write(fs, 500))
                        return MFTestResults.Fail;
                }

                // 300 bytes original + 1000 bytes + 500 bytes
                if (!FileStreamHelper.VerifyFile(fileName, 1800))
                    return MFTestResults.Fail;
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
                    new MFTestMethod( InvalidArguments, "InvalidArguments" ),
                    new MFTestMethod( FileAccess_Read, "FileAccess_Read" ),
                    new MFTestMethod( FileAccess_Write, "FileAccess_Write" ),
                    new MFTestMethod( FileAccess_ReadWrite, "FileAccess_ReadWrite" ),
                };
             }
        }
    }
}
