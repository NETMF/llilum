////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;



using System.IO;

namespace FileSystemTest
{
    public class GetFileName : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region helper methods

        private bool TestGetFileName(string path, string expected)
        {
            string result = Path.GetFileName(path);
            Log.Comment("Path: '" + path + "'");
            Log.Comment("Expected: '" + expected + "'");
            if (result != expected)
            {
                Log.Exception("Got: '" + result + "'");
                return false;
            }
            return true;
        }

        #endregion helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults Null()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetFileName(null, null))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName("", ""))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(string.Empty, string.Empty))
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
                if (!TestGetFileName(@"file.tmp", "file.tmp"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(@"directory\file.tmp", "file.tmp"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(@"\directory\file.tmp", "file.tmp"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(@"\\machine\directory\file.tmp", "file.tmp"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(@"\\machine\directory\file", "file"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(@"\\machine\directory\file name", "file name"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetFileName(@"\\machine\directory\file.t name.exe", "file.t name.exe"))
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
        public MFTestResults ForwardSlash()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                TestGetFileName(@"/directory/file.tmp", "file.tmp");
                return MFTestResults.Fail;
            }
            catch (ArgumentException)
            {
                result = MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults NoFile()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetFileName(@"\sd1\dir\", ""))
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
            try
            {
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { invalidChar, 'b', 'a', 'd', '.', invalidChar, 'p', 'a', 't', 'h', invalidChar });
                        string dir = Path.GetFileName(path);

                        if ((path.Length == 0) && (dir.Length == 0))
                        {
                            /// If path is empty string, returned value is also empty string (same behavior in desktop)
                            /// no exception thrown.
                        }
                        else
                        {
                            Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                            return MFTestResults.Fail;
                        }
                    }
                    catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: " + ae.Message ); }
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
                    new MFTestMethod( Null, "Null" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                    new MFTestMethod( ForwardSlash, "ForwardSlash" ),
                    new MFTestMethod( NoFile, "NoFile" ),
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                };
             }
        }
    }
}
