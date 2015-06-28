////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class CreateDirectory : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            try
            {
                IOTests.IntializeVolume();

                Directory.CreateDirectory("CreateDirectory");
            }
            catch (Exception ex)
            {
                Log.Exception("Skipping: Unable to initialize file system", ex);
                return InitializeResult.Skip;
            }            
            // Add your functionality here.                

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
        private bool TestCreateDirectory(string path)
        {
            DirectoryInfo dir = Directory.CreateDirectory(path);
            // Append current dir if not rooted
            if (path.Substring(0, 1) != @"\")
            {
                path = Directory.GetCurrentDirectory() + "\\" + path;
            }

            Log.Comment("Path: '" + path + "'");
            if (dir.FullName != path)
            {
                Log.Exception("Got: '" + dir.FullName + "'");
                return false;
            }
            return VerifyDirectory(path);
        }

        private bool VerifyDirectory(string path)
        {
            string parent = GetParentDirectory(path, 1);
            string[] validate = Directory.GetDirectories(parent);
            foreach (string vpath in validate)
            {
                // check case insensitive
                if (vpath.ToLower() == path.ToLower())
                    return true;
            }
            Log.Exception("Did not find " + path + " after creation.");
            return false;
        }

        private String GetParentDirectory(String strDirName, int iNumUpFolder)
        {
            string parentDir = iNumUpFolder > 0 ? strDirName : "";

            // make sure we account for file name
            if(File.Exists( strDirName ))
            {
                parentDir = Path.GetDirectoryName( strDirName );
            }

            while(iNumUpFolder > 0)
            {
                parentDir = Path.GetDirectoryName( parentDir );

                iNumUpFolder--;
            }

            return parentDir;

            //int iIndex = 0;
            //while (iNumUpFolder != 0)
            //{
            //    if (strDirName.IndexOf('\\') > -1 || strDirName.IndexOf('/') > -1)
            //    {
            //        iIndex = strDirName.LastIndexOf('\\');
            //        if (iIndex == -1)
            //            iIndex = strDirName.IndexOf('/');

            //        if (iIndex != -1)
            //            strDirName = strDirName.Substring(0, iIndex);
            //    }
            //    iNumUpFolder--;
            //}
            //return strDirName;
        }

        #endregion

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            DirectoryInfo dir;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null Constructor");
                    dir = Directory.CreateDirectory(null);
                    Log.Exception( "Expected ArgumentNullException, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentNullException ane) 
                { 
                    /* pass case */
                    Log.Comment( "Got correct exception: " + ane.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("String.Empty Constructor");
                    dir = Directory.CreateDirectory(String.Empty);
                    Log.Exception( "Expected ArgumentException, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("White Space Constructor");
                    dir = Directory.CreateDirectory("       ");
                    Log.Exception( "Expected ArgumentException, but got " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae2) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae2.Message );
                    result = MFTestResults.Pass;
                }
                Log.Comment("Colon test cases");
                string[] values = new string[] { ":", ":t", ":test", "te:", "test:", "te:st"};
                foreach (string value in values)
                {
                    try
                    {
                        Log.Comment("Testing for exception: " + value);
                        dir = new DirectoryInfo(value);
                        Log.Exception( "Expected ArgumentException, but got " + dir.FullName );
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentException ae3) 
                    { /* pass case */
                        Log.Comment( "Got correct exception: " + ae3.Message );
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
        public MFTestResults NewLineTabs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify tabs/newlines truncated");
                try
                {
                    Directory.CreateDirectory("TabNLTest1\t");
                }
                catch (ArgumentException ae) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                if (VerifyDirectory(IOTests.Volume.RootDirectory + "\\TabNLTest1"))
                    return MFTestResults.Fail;

                Log.Comment("Verify tabs/newlines truncated");
                try
                {
                    Directory.CreateDirectory("TabNLTest2\t\t\t\t\t\t");
                }
                catch (ArgumentException ae1) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae1.Message );
                    result = MFTestResults.Pass;
                }
                if (VerifyDirectory(IOTests.Volume.RootDirectory + "\\TabNLTest2"))
                    return MFTestResults.Fail;

                Log.Comment("Verify tabs/newlines truncated");
                try
                {
                    Directory.CreateDirectory("TabNLTest3\n");
                }
                catch (ArgumentException ae2) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae2.Message );
                    result = MFTestResults.Pass;
                }
                if (VerifyDirectory(IOTests.Volume.RootDirectory + "\\TabNLTest3"))
                    return MFTestResults.Fail;

                try
                {
                    Directory.CreateDirectory("TabNLTest4\n\n\n\n\n\n\n");
                }
                catch (ArgumentException ae3) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae3.Message );
                    result = MFTestResults.Pass;
                }
                if (VerifyDirectory(IOTests.Volume.RootDirectory + "\\TabNLTest4"))
                    return MFTestResults.Fail;

                try
                {
                    Directory.CreateDirectory("TabNLTest5\n\n\t\t\n\n\t");
                }
                catch (ArgumentException ae4) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae4.Message );
                    result = MFTestResults.Pass;
                }
                if (VerifyDirectory(IOTests.Volume.RootDirectory + "\\TabNLTest5"))
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
        public MFTestResults Dots()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                DirectoryInfo dir;

                Directory.SetCurrentDirectory( IOTests.Volume.RootDirectory );

                try
                {
                    Log.Comment( "Create current dir . (no-op)" );
                    dir = Directory.CreateDirectory( "." );
                    if(dir.FullName != Directory.GetCurrentDirectory())
                    {
                        Log.Exception( "Expected no-op create current dir, but got: " + dir.FullName );
                        return MFTestResults.Fail;
                    }
                }
                catch(ArgumentException ioex)
                {
                    /* pass case */
                    Log.Comment( "Got correct exception: " + ioex.Message );
                    result = MFTestResults.Pass;
                }

                Log.Comment("Create parent dir .. (no-op)");
                try
                {
                    dir = Directory.CreateDirectory("..");
                    Log.Exception( "Expected IOException, but got: " + dir.FullName );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                }

                Log.Comment("Create directory starting in .");
                if (!TestCreateDirectory(".StartDotDirectory"))
                    return MFTestResults.Fail;

                Log.Comment("Create directory ending in .");
                dir = Directory.CreateDirectory("DotDirectory.");
                if (!VerifyDirectory(Directory.GetCurrentDirectory() + @"\DotDirectory"))
                {
                    Log.Exception( "Expected no-op create current dir, but got: " + dir.FullName );
                    return MFTestResults.Fail;
                }

                Log.Comment("Create directory ending in ..");
                dir = Directory.CreateDirectory("DoubleDotDirectory..");
                if (!VerifyDirectory(Directory.GetCurrentDirectory() + @"\DoubleDotDirectory"))
                {
                    Log.Exception( "Expected no-op create current dir, but got: " + dir.FullName );
                    return MFTestResults.Fail;
                }

                Log.Comment("Create directory ending in ...");
                dir = Directory.CreateDirectory("TripleDotDirectory...");
                if (!VerifyDirectory(Directory.GetCurrentDirectory() + @"\TripleDotDirectory"))
                {
                    Log.Exception( "Expected no-op create current dir, but got: " + dir.FullName );
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
        public MFTestResults TooLongPath()
        {
            // Desktop throws TooLongException for these cases.  Currently test expects ArgumentException, but throws generic Exception.
            MFTestResults result = MFTestResults.Pass;
            DirectoryInfo dir;
            try
            {
                string longBlock = new string('y', 135);
                try
                {
                    Log.Comment("Too long single dir");
                    dir = Directory.CreateDirectory(longBlock + longBlock);
                    Log.Exception("Expected ArgumentException exception, but got: " + dir.FullName);
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { /* Pass Case */
                    Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                } // PathTooLong
                try
                {
                    Log.Comment("Too long dir in path");
                    dir = Directory.CreateDirectory(@"\sd1\" + longBlock + "\\" + longBlock + "\\" + longBlock + longBlock + "\\dir");
                    Log.Exception("Expected NullArgument exception, but got: " + dir.FullName);
                    return MFTestResults.Fail;
                }
                catch (IOException ioe1) 
                { /* Pass Case */
                    Log.Comment( "Got correct exception: " + ioe1.Message );
                    result = MFTestResults.Pass;
                } // PathTooLong
                try
                {
                    Log.Comment("Max lengh");
                    dir = Directory.CreateDirectory(new string('a', (int)UInt16.MaxValue + 1));
                    Log.Exception("Expected NullArgument exception, but got: " + dir.FullName);
                    return MFTestResults.Fail;
                }
                catch (IOException ioe2) 
                { /* Pass Case */
                    Log.Comment( "Got correct exception: " + ioe2.Message );
                    result = MFTestResults.Pass;
                } // PathTooLong

                Log.Comment("first phase complete.");

                // chech bounds
                string currDir = Directory.GetCurrentDirectory();
                int len = currDir.Length;
                if (currDir.Substring(currDir.Length - 1) != (Path.DirectorySeparatorChar.ToString()))
                    len++;

                Log.Comment("second phase started");
                /// Problem here is while path is less than 260 in code below, ultimate
                /// path in emulator is well above 260, causing exception from emulator.
                if (IOTests.Volume.FileSystem.ToUpper() != "WINFS")
                {
                    for (int i = 225 - len; i < 275 - len; i++)
                    {
                        Log.Comment("Trying length " + i);
                        try
                        {
                            String str1 = new String('a', i);
                            dir = Directory.CreateDirectory(str1);
                            if (dir.Name.Length >= IOTests.Volume.RootDirectory.Length + 258 /*(FSMaxPathLength)*/)
                            {
                                Log.Exception( "Didn't Throw: " + ( i + len ) + " - " + dir.Name.Length );
                                return MFTestResults.Fail;
                            }

                            // Delete the directory we just created so we won't fail because of a limitation on number of 
                            // entries in a directory (i.e. the root directory in FAT16)
                            Directory.Delete(str1);
                        }
                        catch (IOException)
                        {
                            if ((len + i) >= IOTests.Volume.RootDirectory.Length + 246 /*(FS_MAX_DIRECTORY_LENGTH)*/)
                            {
                                // Expected, directory path is too long
                            }
                            else if ((len + i) < 260)
                            {
                                Log.Exception("Threw too early: " + (i + len));

                                return MFTestResults.Fail;
                            }
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

        [TestMethod]
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { 'b', 'a', 'd', '\\', invalidChar, 'p', 'a', 't', 'h', invalidChar, '.', 't', 'x', 't' });
                        DirectoryInfo dir = Directory.CreateDirectory(path);
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

        [TestMethod]
        public MFTestResults CreateExistingDirectory()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                /// Creating existing directory should not fail.
                DirectoryInfo dir1 = Directory.CreateDirectory(IOTests.Volume.RootDirectory + "\\CreateDirectory");
                DirectoryInfo dir2 = Directory.CreateDirectory(IOTests.Volume.RootDirectory + "\\CreateDirectory");                
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
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);

                if (!TestCreateDirectory("ValidTestDir1"))
                    return MFTestResults.Fail;

                if (!TestCreateDirectory(IOTests.Volume.RootDirectory + "\\ValidTestDir2"))
                    return MFTestResults.Fail; 
                
                if (!TestCreateDirectory(@"ValidTestDir3\test\TesT"))
                    return MFTestResults.Fail;

                // fails because GetDirectories has bug
                if (!TestCreateDirectory(@"ValidTestDir3\test\TesT2"))
                    return MFTestResults.Fail;

                if (!TestCreateDirectory(@"ValidTestDir3\test\TesT3"))
                    return MFTestResults.Fail; 
                
                if (!TestCreateDirectory(IOTests.Volume.RootDirectory + "\\ValidTestDir3\\test2\\TesT"))
                    return MFTestResults.Fail; 
                
                if (!TestCreateDirectory(IOTests.Volume.RootDirectory + "\\ValidTestDir4\\test\\TesT"))
                    return MFTestResults.Fail;

                // relative
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory + "\\ValidTestDir1");
                if (Directory.GetCurrentDirectory() != IOTests.Volume.RootDirectory + "\\ValidTestDir1")
                {
                    Log.Exception("Failed to set current directory.  Relative tests are not accurate");
                    return MFTestResults.Fail;
                }

                if (!TestCreateDirectory("RelTestDir1"))
                    return MFTestResults.Fail;

                if (!TestCreateDirectory("RelTestDir2"))
                    return MFTestResults.Fail;

                if (!TestCreateDirectory(@"ValidTestDir2\test"))
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
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                    new MFTestMethod( NewLineTabs, "NewLineTabs" ),
                    new MFTestMethod( Dots, "Dots" ),
                    new MFTestMethod( TooLongPath, "TooLongPath" ),
                    new MFTestMethod( CreateExistingDirectory, "CreateExistingDirectory" ),
                    new MFTestMethod( InvalidArguments, "InvalidArguments" ),
                };
             }
        }
    }
}
