////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;




namespace FileSystemTest
{
    public class Open_FM : IMFTestInterface
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

                Directory.CreateDirectory(testDir);
                Directory.SetCurrentDirectory(testDir);
            }
            catch (Exception ex)
            {
                Log.Comment("Skipping: Unable to initialize file system" + ex.Message);
                return InitializeResult.Skip;
            }
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region Local vars
        private const string fileName = "file1.tmp";
        private const string file2Name = "file2.txt";
        private const string testDir = "Open_FM";

        #endregion Local vars

        #region Helper methods
        private MFTestResults TestMethod(FileMode fm)
        {
            Log.Comment("Starting tests in FileMode: " + fm.ToString());
            FileInfo fil2;
            StreamWriter sw2;
            Stream fs2 = null;
            String str2;
            int iCountErrors = 0;


            if (File.Exists(fileName))
                File.Delete(fileName);

            Log.Comment("File does not exist");
            //------------------------------------------------------------------

            fil2 = new FileInfo(fileName);
            switch (fm)
            {
                case FileMode.CreateNew:
                case FileMode.Create:
                case FileMode.OpenOrCreate:

                    try
                    {
                        Log.Comment( "With a null string" );
                        iCountErrors = 0; // ZeligBUG not resetting the value here leads to uninit iCountErrors value
                        fs2 = File.Open( null, fm );
                        if(!File.Exists( fileName ))
                        {
                            iCountErrors++;
                            Log.Exception( "File not created, FileMode==" + fm.ToString() );
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Comment("Expected exception thrown :: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        iCountErrors = 1;
                        Log.Exception("Unexpected exception thrown :: " + ex.ToString());
                    }

                    Log.Comment("with an empty string");
                    try
                    {
                        fs2 = File.Open("", fm);
                        if (!File.Exists(fileName))
                        {
                            iCountErrors++;
                            Log.Exception("File not created, FileMode==" + fm.ToString());
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        Log.Comment("Expected exception thrown :: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        iCountErrors++;
                        Log.Exception("Unexpected exception thrown :: " + ex.ToString());
                    }

                    fs2 = File.Open(fileName, fm);
                    if (!File.Exists(fileName))
                    {
                        iCountErrors++;
                        Log.Exception("File not created, FileMode==" + fm.ToString());
                    }
                    fs2.Close();
                    break;
                case FileMode.Open:
                case FileMode.Truncate:
                    try
                    {
                        Log.Comment( "Open or Truncate" );
                        iCountErrors = 0; // ZeligBUG not resetting the value here leads to uninit iCountErrors value
                        fs2 = File.Open( fileName, fm );
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (IOException fexc)
                    {
                        Log.Comment("Caught expected exception, fexc==" + fexc.Message);
                        iCountErrors = 0;
                    }
                    catch (Exception exc)
                    {
                        iCountErrors = 1;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                case FileMode.Append:
                    try
                    {
                        Log.Comment( "Append" );
                        fs2 = File.Open(fileName, fm);
                        fs2.Write(new Byte[] { 54, 65, 54, 90 }, 0, 4);
                        if (fs2.Length != 4)
                        {
                            iCountErrors++;
                            Log.Exception("Unexpected file length .... " + fs2.Length);
                        }
                        fs2.Close();
                        iCountErrors = 0; // ZeligBUG not resetting the value here leads to uninit iCountErrors value
                    }
                    catch (Exception exc)
                    {
                        iCountErrors = 1;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                default:
                    iCountErrors = 1;
                    Log.Exception("Invalid mode.");
                    break;
            }


            if (File.Exists(fileName))
                File.Delete(fileName);

            if(iCountErrors > 0) return MFTestResults.Fail;

            //------------------------------------------------------------------


            Log.Comment("File already exists");
            //------------------------------------------------------------------

            sw2 = new StreamWriter(fileName);
            str2 = "Du er en ape";
            sw2.Write(str2);
            sw2.Close();
            switch (fm)
            {
                case FileMode.CreateNew:
                    try
                    {
                        fs2 = File.Open( fileName, fm );
                        iCountErrors++;
                        Log.Exception("Expected exception not thrown");
                        fs2.Close();
                    }
                    catch (IOException aexc)
                    {
                        Log.Comment("Caught expected exception, aexc==" + aexc.Message);
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                case FileMode.Create:
                    fs2 = File.Open(fileName, fm);
                    if (fs2.Length != 0)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect length of file==" + fil2.Length);
                    }
                    fs2.Close();
                    break;
                case FileMode.OpenOrCreate:
                case FileMode.Open:
                    fs2 = File.Open(fileName, fm);
                    if (fs2.Length != str2.Length)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect length on file==" + fil2.Length);
                    }
                    fs2.Close();
                    break;
                case FileMode.Truncate:
                    fs2 = File.Open(fileName, fm);
                    if (fs2.Length != 0)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect length on file==" + fil2.Length);
                    }
                    fs2.Close();
                    break;
                case FileMode.Append:
                    try
                    {
                        fs2 = File.Open(fileName, fm);
                        fs2.Write(new Byte[] { 54, 65, 54, 90 }, 0, 4);
                        if (fs2.Length != 16)
                        {  // already 12 characters are written to the file.
                            iCountErrors++;
                            Log.Exception("Unexpected file length .... " + fs2.Length);
                        }
                        fs2.Close();
                    }
                    catch (Exception exc)
                    {
                        iCountErrors++;
                        Log.Exception("Incorrect exception thrown, exc==" + exc.ToString());
                    }
                    break;
                default:
                    iCountErrors++;
                    Log.Exception("Invalid mode.");
                    break;
            }

            if (File.Exists(fileName))
                File.Delete(fileName);

            return iCountErrors == 0 ? MFTestResults.Pass : MFTestResults.Fail;
        }

        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults FileMode_Append()
        {
            return TestMethod(FileMode.Append);
        }

        [TestMethod]
        public MFTestResults FileMode_Create()
        {
            return TestMethod(FileMode.Create);
        }

        [TestMethod]
        public MFTestResults FileMode_CreateNew()
        {
            return TestMethod(FileMode.CreateNew);
        }

        [TestMethod]
        public MFTestResults FileMode_Open()
        {
            return TestMethod(FileMode.Open);
        }

        [TestMethod]
        public MFTestResults FileMode_OpenOrCreate()
        {
            return TestMethod(FileMode.OpenOrCreate);
        }

        [TestMethod]
        public MFTestResults FileMode_Truncate()
        {
            return TestMethod(FileMode.Truncate);
        }

        [TestMethod]
        public MFTestResults Invalid_FileMode()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                // Cleanup
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                try
                {
                    Log.Comment("-1 FileMode");
                    File.Open(file2Name, (FileMode)(-1));
                    Log.Exception( "Unexpected FileMode" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("0 FileMode");
                    File.Open(file2Name, 0);
                    Log.Exception( "Unexpected FileMode" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("7 FileMode");
                    File.Open(file2Name, (FileMode)7);
                    Log.Exception( "Unexpected FileMode" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                {
                    /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message ); 
                    result = MFTestResults.Pass;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            Log.Comment("Verify no file created");
            if (File.Exists(file2Name))
            {
                Log.Exception( "Unexpected file found" );
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Invalid_FileAccess()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                // Cleanup
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                try
                {
                    Log.Comment("-1 FileAccess");
                    File.Open(file2Name, FileMode.OpenOrCreate, (FileAccess)(-1));
                    Log.Exception( "Unexpected FileAccess" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("0 FileAccess");
                    File.Open(file2Name, FileMode.OpenOrCreate, (FileAccess)0);
                    Log.Exception( "Unexpected FileAccess" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("4 FileAccess");
                    File.Open(file2Name, FileMode.OpenOrCreate, (FileAccess)4);
                    Log.Exception( "Unexpected FileAccess" );
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

            Log.Comment("Verify no file created");
            if (File.Exists(file2Name))
            {
                Log.Exception( "Unexpected file found" );
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Invalid_FileShare()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                // Cleanup
                if (File.Exists(file2Name))
                    File.Delete(file2Name);

                try
                {
                    Log.Comment("-1 FileShare");
                    File.Open(file2Name, FileMode.OpenOrCreate, FileAccess.ReadWrite, (FileShare)(-1));
                    Log.Exception( "Unexpected FileShare" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("74 FileShare");
                    File.Open(file2Name, FileMode.OpenOrCreate, FileAccess.ReadWrite, (FileShare)5);
                    Log.Exception( "Unexpected FileShare" );
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

            Log.Comment("Verify no file created");
            if (File.Exists(file2Name))
            {
                Log.Exception( "Unexpected file found" );
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
                    new MFTestMethod( FileMode_Append, "FileMode_Append" ),
                    new MFTestMethod( FileMode_Create, "FileMode_Create" ),
                    new MFTestMethod( FileMode_CreateNew, "FileMode_CreateNew" ),
                    new MFTestMethod( FileMode_Open, "FileMode_Open" ),
                    new MFTestMethod( FileMode_OpenOrCreate, "FileMode_OpenOrCreate" ),
                    new MFTestMethod( FileMode_Truncate, "FileMode_Truncate" ),
                    new MFTestMethod( Invalid_FileMode, "Invalid_FileMode" ),
                    new MFTestMethod( Invalid_FileAccess, "Invalid_FileAccess" ),
                    new MFTestMethod( Invalid_FileShare, "Invalid_FileShare" ),
                };
             }
        }
    }
}
