////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class FileExists : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            try
            {
                IOTests.IntializeVolume();

                Directory.CreateDirectory(testDir);
                Directory.SetCurrentDirectory(testDir);
                File.Create(file1Name).Close();
                File.Create(IOTests.Volume.RootDirectory + "\\" + file2Name).Close();
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
        private const string file2Name = "File2.txt";
        private const string testDir = "ExistsDir";
        #endregion Local vars

        #region Helper methods

        private bool TestExists(string path, bool exists)
        {
            Log.Comment("Checking for " + path);
            if (File.Exists(path) != exists)
            {
                Log.Exception("Expeceted " + exists + " but got " + !exists);
                return false;
            }
            return true;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            bool file;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Null");
                    file = File.Exists(null); /// MSDN: No exception thrown.
                    
                }
                catch (ArgumentNullException) 
                {
                    return MFTestResults.Fail;                    
                }

                try
                {
                    Log.Comment("String.Empty");
                    file = File.Exists(String.Empty);
                }
                catch (ArgumentNullException)
                {
                    return MFTestResults.Fail;
                }

                try
                {
                    Log.Comment("White Space");
                    file = File.Exists("       ");                    
                }
                catch (ArgumentNullException)
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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("relative to current");
                if (!TestExists(file1Name, true))
                    return MFTestResults.Fail;

                Log.Comment(". current directory");
                if (!TestExists(@".\" + file1Name, true))
                    return MFTestResults.Fail;

                Log.Comment(".. parent directory");
                if (!TestExists(@"..\" + file2Name, true))
                    return MFTestResults.Fail;

                Log.Comment("absolute path");
                if (!TestExists(Directory.GetCurrentDirectory() + "\\" + file1Name, true))
                    return MFTestResults.Fail;

                Log.Comment("current directory name");
                if (!TestExists(Directory.GetCurrentDirectory(), false))
                    return MFTestResults.Fail;

                Log.Comment("Set to root");
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);

                Log.Comment(". current directory");
                if (!TestExists(@".\" + file2Name, true))
                    return MFTestResults.Fail;

                Log.Comment("relative child");
                if (!TestExists(testDir + "\\" + file1Name, true))
                    return MFTestResults.Fail;

                Log.Comment("child directory name");
                if (!TestExists(testDir, false))
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
        public MFTestResults NonExistentFiles()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
                Log.Comment("Current directory: " + Directory.GetCurrentDirectory());

                Log.Comment("Non-exist at root");
                if (!TestExists(@"..\" + file1Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Dot - .");
                if (!TestExists("." + file1Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Double dot - ..");
                if (!TestExists(".." + file1Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Non-exist relative");
                if (!TestExists(file1Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Non-exist in child dir");
                if (!TestExists(testDir + "\\" + file2Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Non-exist absolute");
                if (!TestExists(IOTests.Volume.RootDirectory + "\\" + testDir + "\\" + file2Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Non-exist directory absolute");
                if (!TestExists(IOTests.Volume.RootDirectory + "\\" + testDir + "\\non-existent\\" + file2Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Wild card - *" + file1Name);
                if (!TestExists("*" + file1Name, false))
                    return MFTestResults.Fail;

                Log.Comment("Wild card - *");
                if (!TestExists("*", false))
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
        public MFTestResults PathTooLong()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = new string('x', 500);
                bool exists = File.Exists(path);                
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
            Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory + "\\" + testDir);
            Log.Comment("Current directory: " + Directory.GetCurrentDirectory());

            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestExists(file1Name.ToLower(), true))
                    return MFTestResults.Fail;

                if (!TestExists(file1Name.ToUpper(), true))
                    return MFTestResults.Fail;

                if (!TestExists(IOTests.Volume.RootDirectory + "\\" + testDir.ToLower() + "\\" + file1Name.ToLower(), true))
                    return MFTestResults.Fail;

                if (!TestExists(IOTests.Volume.RootDirectory + "\\" + testDir.ToUpper() + "\\" + file1Name.ToUpper(), true))
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
                string fileName = dir + "\\test file with spaces.txt";
                File.Create(fileName).Close();

                if (!TestExists(fileName, true))
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
                    new MFTestMethod( ValidCases, "ValidCases" ),
                    new MFTestMethod( NonExistentFiles, "NonExistentFiles" ),
                    new MFTestMethod( PathTooLong, "PathTooLong" ),
                    new MFTestMethod( CaseInsensitive, "CaseInsensitive" ),
                    new MFTestMethod( MultiSpaceExists, "MultiSpaceExists" ),
                };
             }
        }
    }
}
