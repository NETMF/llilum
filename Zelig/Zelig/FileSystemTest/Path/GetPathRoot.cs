////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;



using System.IO;

namespace FileSystemTest
{
    public class GetPathRoot : IMFTestInterface
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

        private bool TestGetPathRoot(string path, string expected)
        {
            string result = Path.GetPathRoot(path);
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
                if (!TestGetPathRoot(null, null))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"sd1\", ""))
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
                if (!TestGetPathRoot(@"\", @"\"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\dir1", @"\"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\file.text", @"\"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\file\text", @"\"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\sd1\\\\dir\\\\\file\\\\\text\\\\", @"\"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\\machine\dir1\file.tmp", @"\\machine\dir1"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetPathRoot(@"\\machine", @"\\machine"))
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
        public MFTestResults StartWithColon()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    string path = Path.GetFullPath(":file");
                    Log.Exception("Expected NullArgument exception, but got '" + path + "'");
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
                        string dir = Path.GetPathRoot(path);
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
                    new MFTestMethod( StartWithColon, "StartWithColon" ),
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                };
             }
        }
    }
}
