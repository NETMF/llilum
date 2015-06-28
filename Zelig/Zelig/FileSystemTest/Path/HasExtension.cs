////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class HasExtension : IMFTestInterface
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

        private bool expected;

        private bool TestHasExtension(string path)
        {
            Log.Comment("Path: '" + path + "'");
            if (Path.HasExtension(path) & expected)
                return true;
            Log.Exception("Expected " + expected);
            return false;
        }

        #endregion helper methods

        [TestMethod]
        public MFTestResults Negative()
        {
            MFTestResults result = MFTestResults.Pass;
            expected = false;
            try
            {
                if (TestHasExtension(null))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(""))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(string.Empty))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(@"\"))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension("file"))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(@"\sd\file"))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(@"\\Machine"))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(@"\\Machine\directory"))
                {
                    return MFTestResults.Fail;
                }
                if (TestHasExtension(@"\\Machine\directory\file"))
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
        public MFTestResults Positive()
        {
            MFTestResults result = MFTestResults.Pass;
            expected = true;
            try
            {
                if (!TestHasExtension(".txt"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\file.txt"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestHasExtension("file.x"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\sd\file.comp"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\\Machine\directory.dir"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\\Machine\directory\file.zz"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestHasExtension(@"\sd\file." + new string('f',256)))
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
                        bool dir = Path.HasExtension(path);
                        if ((path.Length == 0) && (dir == false))
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

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( Negative, "Negative" ),
                    new MFTestMethod( Positive, "Positive" ),
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                };
             }
        }
    }
}
