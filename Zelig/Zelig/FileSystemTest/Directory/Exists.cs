////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class Exists : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.           
            try
            {
                IOTests.IntializeVolume();
                Directory.CreateDirectory(TestDir);
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
        #endregion local vars

        #region Helper functions
        private bool TestExists(string path, bool exists)
        {
            Log.Comment("Checking for " + path);
            if (Directory.Exists(path) != exists)
            {
                Log.Exception("Expeceted " + exists + " but got " + !exists);
                return false;
            }
            return true;
        }
        #endregion Helper functions
        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            bool dir;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null");
                    dir = Directory.Exists(null);
                    Log.Exception( "Expected ArgumentNullException, but got " + dir );
                    return MFTestResults.Fail;
                }
                catch (ArgumentNullException ane) 
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ane.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("String.Empty");
                    dir = Directory.Exists(String.Empty);
                    Log.Exception( "Expected ArgumentException, but got " + dir );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae)
                { /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("White Space");
                    dir = Directory.Exists("       ");
                    Log.Exception( "Expected ArgumentException, but got " + dir );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae1)
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
        public MFTestResults CurrentDirectory()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestExists(".", true))
                    return MFTestResults.Fail;

                if (!TestExists("..", true))
                    return MFTestResults.Fail;

                if (!TestExists(Directory.GetCurrentDirectory(), true))
                    return MFTestResults.Fail;

                Log.Comment("Set relative Directory");
                Directory.SetCurrentDirectory(TestDir);

                if (!TestExists(".", true))
                    return MFTestResults.Fail;

                if (!TestExists(Directory.GetCurrentDirectory(), true))
                    return MFTestResults.Fail;

                if (!TestExists("..", true))
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
        public MFTestResults NonExistentDirs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestExists("unknown directory", false))
                    return MFTestResults.Fail;

                if (!TestExists(IOTests.Volume.RootDirectory + @"\Dir1\dir2", false))
                    result =  MFTestResults.Fail;

                if (!TestExists(@"BAR\", false))
                    return MFTestResults.Fail;

                try
                {
                    bool test = TestExists("XX:\\", false);
                    Log.Comment("Expected ArgumentException, got " + test);
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
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

        [TestMethod]
        public MFTestResults PathTooLong()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = new string('x', 500);
                bool exists = Directory.Exists(path);
                Log.Exception("Expected IOException, got " + exists);
                return MFTestResults.Fail;
            }
            catch (IOException ioe) 
            { 
                /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                result = MFTestResults.Pass;
            } // PathTooLong
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
            try
            {
                Directory.CreateDirectory(TestDir);

                if (!TestExists(TestDir.ToLower(), true))
                    return MFTestResults.Fail;

                if (!TestExists(TestDir.ToUpper(), true))
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
        public MFTestResults MultiSpaceExists()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string dir = Directory.GetCurrentDirectory() + @"\Microsoft Visual Studio .NET\Frame work\V1.0.0.0000";
                Directory.CreateDirectory(dir);

                if (!TestExists(dir, true))
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
                    new MFTestMethod( CurrentDirectory, "CurrentDirectory" ),
                    new MFTestMethod( NonExistentDirs, "NonExistentDirs" ),
                    new MFTestMethod( PathTooLong, "PathTooLong" ),
                    new MFTestMethod( CaseInsensitive, "CaseInsensitive" ),
                    new MFTestMethod( MultiSpaceExists, "MultiSpaceExists" ),
                };
             }
        }
    }
}
