////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class DI_Constructor : IMFTestInterface
    {
        #region internal vars
        private const string DIRA = @"DirA";
        private const string DIRB = @"DirB";
        private const string DIR1 = @"Dir1";
        private const string DIR2 = @"Dir2";
        #endregion

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

                Directory.CreateDirectory(GetDir(DIRA, DIR1));
                Directory.CreateDirectory(GetDir(DIRA, DIR2));
                Directory.CreateDirectory(GetDir(DIRB, DIR1));
                Directory.CreateDirectory(GetDir(DIRB, DIR2));
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system", ex);

                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }


        #region Helper Functions
        private string GetDir(params string[] args)
        {
            string dir = IOTests.Volume.RootDirectory;
            for (int i = 0; i < args.Length; i++)
            {
                dir += @"\" + args[i];
            }
            return dir;
        }
        private bool TestDirectoryInfo(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            path = RelativePath(path);
            Log.Comment("Path: '" + path + "'");
            if (dir.FullName != path)
            {
                Log.Exception("Got: '" + dir.FullName + "'");
                return false;
            }
            return true;
        }

        private string RelativePath(string path)
        {
            // rooted
            if (path.Substring(0,1) == "\\")
                return path;
            return Directory.GetCurrentDirectory() + "\\" + path;
        }
        #endregion

        #region Test Cases

        [TestMethod]
        public MFTestResults NullArguments()
        {
            DirectoryInfo dir;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null Constructor");
                    dir = new DirectoryInfo(null);
                    Log.Exception( "Expected Argument exception, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("String.Empty Constructor");
                    dir = new DirectoryInfo(String.Empty);
                    Log.Exception( "Expected Argument exception, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae1) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae1.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("White Space Constructor");
                    dir = new DirectoryInfo("       ");
                    Log.Exception( "Expected Argument exception, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae2)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae2.Message );
                    result = MFTestResults.Pass;
                }

                // Try above root
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
                try
                {
                    Log.Comment(".. above root Constructor");
                    /// .. is a valid location, while ..\\.. is not.
                    dir = new DirectoryInfo("..\\..");
                    Log.Exception( "Expected Argument exception, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae3)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae3.Message );
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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("root");
                if (!TestDirectoryInfo(Directory.GetCurrentDirectory()))
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("test created dirs");
                if (!TestDirectoryInfo(GetDir(DIRA)))
                {
                    return MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRB)))
                {
                    return MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRA,DIR1)))
                {
                    return MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRA, DIR2)))
                {
                    return MFTestResults.Fail;
                } 
                if (!TestDirectoryInfo(GetDir(DIRB, DIR1)))
                {
                    return MFTestResults.Fail;
                }
                if (!TestDirectoryInfo(GetDir(DIRB, DIR2)))
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Case insensitive");
                if (!TestDirectoryInfo(GetDir(DIRA.ToLower())))
                {
                    return MFTestResults.Fail;
                }
                if (!TestDirectoryInfo(GetDir(DIRB.ToUpper())))
                {
                    return MFTestResults.Fail;
                }
                Log.Comment("Relative - set current dir to DirB");
                Directory.SetCurrentDirectory(GetDir(DIRB));
                if (!TestDirectoryInfo(DIR1))
                {
                    return MFTestResults.Fail;
                }
                if (!TestDirectoryInfo(DIR2))
                {
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
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            DirectoryInfo dir;
            try
            {
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { 'b', 'a', 'd', '\\', invalidChar, 'p', 'a', 't', 'h', invalidChar, '.', 't', 'x', 't' });
                        dir = Directory.CreateDirectory(path);
                        if (invalidChar == 0)
                        {
                            Log.Exception("[Known issue for '\\0' char] Expected Argument exception for for '" + path + "' but got: '" + dir + "'");
                        }
                        else
                        {
                            Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir.FullName + "'");
                            return MFTestResults.Fail;
                        }
                    }
                    catch (ArgumentException ae)
                    { /* Pass Case */
                        Log.Comment( "Got correct exception: " + ae.Message );
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
                    new MFTestMethod( NullArguments, "NullArguments" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                };
             }
        }
    }
}
