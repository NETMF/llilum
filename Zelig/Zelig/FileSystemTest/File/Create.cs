////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class Create : IMFTestInterface
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

                Directory.CreateDirectory(sourceDir);
                Directory.CreateDirectory("Test " + sourceDir);
                Directory.SetCurrentDirectory(sourceDir);
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
        private const string file1Name = "File1.tmp";
        private const string sourceDir = "source";
        #endregion Local vars

        #region Helper methods

        private bool TestCreate(string file)
        {
            return TestCreate(file, 1000);
        }
        private bool TestCreate(string file, int buffer)
        {
            bool success = true;
            Log.Comment("Create " + file + " of size " + buffer);
            if (File.Exists(file))
            {
                Log.Exception("Test space dirty, cleaning up!");
                File.Delete(file);
            }

            string dir = Path.GetDirectoryName(Path.GetFullPath(file));
            if(( dir != null ) && ( dir.Length > 0 ) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory( dir );
            }

            FileStream fs = null;
            try
            {
                fs = File.Create(file, buffer);
                if (!File.Exists(file))
                {
                    Log.Exception("Could not find file after creation!");
                    success = false;
                }
                if (fs.Length != 0)
                {
                    Log.Exception("Incorrect file length == " + fs.Length);
                    success = false;
                }
                if (fs.Position != 0)
                {
                    Log.Exception("Incorrect file postion == " + fs.Position);
                    success = false;
                }
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                File.Delete(file);
            }
            return success;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("Null Constructor");
                    FileStream fs = File.Create(null);
                    Log.Exception( "Expected ArgumentException" );
                    fs.Close();
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("String.Empty Constructor");
                    FileStream fs = File.Create(string.Empty);
                    Log.Exception( "Expected ArgumentException" );
                    fs.Close();
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Whitespace Constructor");
                    FileStream fs = File.Create("       ");
                    Log.Exception( "Expected ArgumentException" );
                    fs.Close();
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Negative buffer Constructor");
                    FileStream fs = File.Create(file1Name, -10);
                    Log.Exception( "Expected ArgumentException" );
                    fs.Close();
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Current dir '.' Constructor");
                    FileStream fs = File.Create(".");
                    Log.Exception( "Expected ArgumentException" );
                    fs.Close();
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                } // UnauthorizedAccess 
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            string file1Dir1 = Path.Combine(Path.Combine(IOTests.Volume.RootDirectory, sourceDir), file1Name);
            string filedirspace = Path.Combine(Path.Combine(IOTests.Volume.RootDirectory, "Test " + sourceDir), "Test " + file1Name);
            try
            {
                Log.Comment("relative create");
                if (!TestCreate(file1Name))
                    return MFTestResults.Fail;

                Log.Comment("absolute create");
                if (!TestCreate(file1Dir1))
                    return MFTestResults.Fail;

                Log.Comment("elative .. Create");
                if (!TestCreate(@"..\" + file1Name))
                    return MFTestResults.Fail;

                Log.Comment("relative . Create");
                if (!TestCreate(@".\" + file1Name))
                    return MFTestResults.Fail;

                Log.Comment("Create at root");
                if (!TestCreate(Path.Combine(IOTests.Volume.RootDirectory, file1Name)))
                    return MFTestResults.Fail;

                Log.Comment("white space in file name");
                if (!TestCreate(@"test " + file1Name))
                    return MFTestResults.Fail;

                Log.Comment("white space in path & file name");
                if (!TestCreate(filedirspace))
                    return MFTestResults.Fail;

                Log.Comment("max int buffer");
                if (!TestCreate(file1Name, int.MaxValue))
                    return MFTestResults.Fail;

                Log.Comment("1 int buffer");
                if (!TestCreate(file1Name, 1))
                    return MFTestResults.Fail;

                Log.Comment("0 int buffer");
                if (!TestCreate(file1Name, 0))
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
        public MFTestResults CaseInsensitive()
        {
            MFTestResults result = MFTestResults.Pass;
            string file1Dir1 = Path.Combine(Path.Combine(IOTests.Volume.RootDirectory, sourceDir.ToLower()), file1Name.ToLower());
            string filedirspace = Path.Combine(Path.Combine(IOTests.Volume.RootDirectory, "Test " + sourceDir.ToUpper()), "Test " + file1Name.ToUpper());

            try
            {
                // to lower
                if (!TestCreate(file1Dir1))
                    return MFTestResults.Fail;

                // to upper
                if (!TestCreate(filedirspace))
                    return MFTestResults.Fail;

                if (!TestCreate(file1Dir1.ToLower()))
                    return MFTestResults.Fail;

                if (!TestCreate(file1Dir1.ToUpper()))
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
        public MFTestResults SpecialFileNames()
        {
            MFTestResults result = MFTestResults.Pass;
            char[] special = new char[] { '!', '#', '$', '%', '\'', '(', ')', '+', '-', '.', '@', '[', ']', '_', '`', '{', '}', '~' };

            try
            {
                Log.Comment("Create file each with special char file names");
                for (int i = 0; i < special.Length; i++)
                {
                    string file = i + "_" + new string(new char[] { special[i] }) + "_z.file";
                    if (!TestCreate(file))
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
        public MFTestResults InvalidPathChars()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string file = new string(new char[] { 'b', 'a', 'd', invalidChar, 'f', 'i', 'l', 'e', invalidChar, '.', 't', 'x', 't' });
                        FileStream fs = File.Create(file);
                        if (invalidChar == 0)
                        {
                            Log.Exception("Known failure for null");
                            result = MFTestResults.KnownFailure;
                        }
                        else
                        {
                            Log.Exception( "Expected Argument exception for '" + file + "' but got: '" + fs.Name + "'" );
                            fs.Close();
                            File.Delete( file );
                            return MFTestResults.Fail;
                        }
                        fs.Close();
                        File.Delete(file);
                    }
                    catch (ArgumentException ae) 
                    { 
                        /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
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
        #endregion Test Cases

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                    new MFTestMethod( CaseInsensitive, "CaseInsensitive" ),
                    new MFTestMethod( SpecialFileNames, "SpecialFileNames" ),
                    new MFTestMethod( InvalidPathChars, "InvalidPathChars" ),
                };
             }
        }
    }
}
