////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class GetSetCurrentDirectory : IMFTestInterface
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
                Directory.CreateDirectory(TestDir + "\\" + Mid1 + "\\" + Tail1);
                Directory.CreateDirectory(TestDir + "\\" + Mid1 + "\\" + Tail2);
                Directory.CreateDirectory(TestDir + "\\" + Mid2 + "\\" + Tail1);
                Directory.CreateDirectory(TestDir + "\\" + Mid2 + "\\" + Tail2);
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

        #region local vars
        private const string TestDir = "ExistsDirectory";
        private const string Mid1 = "Mid1";
        private const string Mid2 = "Mid2";
        private const string Tail1 = "Tail1";
        private const string Tail2 = "Tail2";
        #endregion local vars

        #region Helper functions
        private bool TestGetSet(params string[] nodes)
        {
            string path = "";
            if (nodes.Length > 0)
            {
                path = nodes[0];
                for (int i = 1; i < nodes.Length; i++)
                {
                    path += "\\" + nodes[i];
                }
            }
            string expected = NormalizePath(path);
            Log.Comment("Changing path to " + path);
            Directory.SetCurrentDirectory(path);
            string result = Directory.GetCurrentDirectory();
            
            if (result != expected)
            {
                Log.Exception("Set failed.  Current directory is " + result);
                Log.Exception("Expected " + expected);
                return false;
            }
            return true;
        }

        private string NormalizePath(string newpath)
        {
            // Not rooted - get full path from Relative
            if (!(newpath.Substring(0, 1) == @"\"))
            {
                newpath = Directory.GetCurrentDirectory() + @"\" + newpath;
            }

            string path = "";
            int skipCount = 0;
            string[] nodes = newpath.Split('\\');
            // first node is always empty since we are rooted, so stop at 2nd node (i=1)
            for (int i = nodes.Length - 1; i > 0; i--)
            {
                // Drop . nodes (current)
                if (!(nodes[i] == "."))
                {
                    if (nodes[i] == "..")
                    {
                        skipCount++;
                    }
                    else
                    {
                        if (skipCount == 0)
                            //append node
                            path = @"\" + nodes[i] + path;
                        else
                            //skip node
                            skipCount--;
                    }
                }
            }

            return path;
        }

        #endregion Helper functions
        #region Test Cases

        [TestMethod]
        public MFTestResults GetSetRelative()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);

                Log.Comment("Walk tree using relative path");
                if (!TestGetSet(TestDir))
                    return MFTestResults.Fail;

                if (!TestGetSet(Mid1))
                    return MFTestResults.Fail;

                if (!TestGetSet(Tail1))
                    return MFTestResults.Fail;

                if (!TestGetSet("."))
                    return MFTestResults.Fail; 
                
                if (!TestGetSet(".."))
                    return MFTestResults.Fail;

                if (!TestGetSet(Tail2))
                    return MFTestResults.Fail;

                if (!TestGetSet(@"..\.."))
                    return MFTestResults.Fail;

                if (!TestGetSet(Mid2, Tail2))
                    return MFTestResults.Fail;

                if (!TestGetSet(@"..\..\.."))
                    return MFTestResults.Fail;

                // Complex path, should result in path to Tail2
                if (!TestGetSet(TestDir, ".", Mid2, "..", Mid2, ".", Tail2))
                    return MFTestResults.Fail;

                if (!TestGetSet("..", "..", Mid1, Tail1))
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
        public MFTestResults GetSetAboslute()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Walk tree using absolute path");
                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir))
                    return MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, Mid1))
                    return MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, Mid2, Tail2))
                    return MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, "."))
                    return MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, ".."))
                    return MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, Mid2, Tail2, @"..\..", Mid2, Tail2))
                    return MFTestResults.Fail;

                // Complex path, should result in path to Tail2
                if (!TestGetSet(IOTests.Volume.RootDirectory, TestDir, ".", Mid2, "..", Mid2, ".", Tail2))
                    return MFTestResults.Fail;

                if (!TestGetSet(IOTests.Volume.RootDirectory))
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
        public MFTestResults InvalidArgs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("null");
                    Directory.SetCurrentDirectory(null);
                    Log.Exception( "Expected ArgumentNullException got " + Directory.GetCurrentDirectory() );
                    return MFTestResults.Fail;
                }
                catch(ArgumentNullException ane)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ane.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("String.Empty");
                    Directory.SetCurrentDirectory(string.Empty);
                    Log.Exception( "Expected ArgumentException got " + Directory.GetCurrentDirectory() );
                    return MFTestResults.Fail;
                }
                catch(ArgumentException ae)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Whitespace");
                    Directory.SetCurrentDirectory("    ");
                    Log.Exception( "Expected ArgumentException got " + Directory.GetCurrentDirectory() );
                    return MFTestResults.Fail;
                }
                catch(ArgumentException ae1)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae1.Message );
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
        public MFTestResults InvalidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("NonExistentDir");
                    Directory.SetCurrentDirectory("NonExistentDir");
                    Log.Exception( "Expected ArgumentException got " + Directory.GetCurrentDirectory() );
                    return MFTestResults.Fail;
                }
                catch(IOException ioe)
                { /* pass case, DirectoryNotFound */
                    Log.Comment( "Got correct exception: " + ioe.Message );
                }

                try
                {
                    Log.Comment(@"non exist mount \foo");
                    Directory.SetCurrentDirectory(@"\foo");
                    Log.Exception( "Expected ArgumentException got " + Directory.GetCurrentDirectory() );
                    return MFTestResults.Fail;
                }
                catch(IOException ioe1)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ioe1.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment(@"Move before root - ..\..\..\..\..");
                    Directory.SetCurrentDirectory(@"..\..\..\..\..");
                    Log.Exception( "Expected ArgumentException got " + Directory.GetCurrentDirectory() );
                    return MFTestResults.Fail;
                }
                catch(ArgumentException ae)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message );
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

        #endregion Test Cases

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( GetSetRelative, "GetSetRelative" ),
                    new MFTestMethod( GetSetAboslute, "GetSetAboslute" ),
                    new MFTestMethod( InvalidArgs, "InvalidArgs" ),
                    new MFTestMethod( InvalidCases, "InvalidCases" ),
                };
             }
        }
    }
}
