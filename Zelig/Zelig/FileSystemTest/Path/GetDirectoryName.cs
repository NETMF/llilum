////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;



using System.IO;

namespace FileSystemTest
{
    public class GetDirectoryName : IMFTestInterface
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

        private bool TestGetDirectoryName(string path, string expected)
        {
            string result = Path.GetDirectoryName(path);
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
        public MFTestResults NullPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName(null, null))
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
        public MFTestResults Vanilla()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("Hello\\file.tmp", "Hello"))
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
        public MFTestResults StartWithSlash()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //single slash
                if (!TestGetDirectoryName("\\Root\\File", "\\Root"))
                {
                    return MFTestResults.Fail;
                }
                //root double slash
                if (!TestGetDirectoryName("\\\\Machine\\Directory\\File", "\\\\Machine\\Directory"))
                {
                    return MFTestResults.Fail;
                }
                //root triple slash, this will throw an exception.

                try
                {
                    TestGetDirectoryName("\\\\\\Machine\\Directory\\File", null);
                    return MFTestResults.Fail;
                }
                catch (ArgumentException)
                {
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
        public MFTestResults WhiteSpace()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //white space in directory
                if (!TestGetDirectoryName("\\root\\Directory Name\\Hello.tmp file.tmp", "\\root\\Directory Name"))
                {
                    return MFTestResults.Fail;
                }
                //white space in file
                if (!TestGetDirectoryName("\\root\\Directory Name\\File Name.tmp file.tmp", "\\root\\Directory Name"))
                {
                    return MFTestResults.Fail;
                } 
                //white space at Root
                if (!TestGetDirectoryName("\\root\\Hello.tmp file.tmp", "\\root"))
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
        public MFTestResults ForwardSlashes()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                /// Forward slash is invalid.
                TestGetDirectoryName("//root//Director//file.tmp", "\\root\\Directory");
                return MFTestResults.Fail;
            }
            catch (ArgumentException)
            {
                result = MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Exception("Exception: " + ex.Message);
                return MFTestResults.Fail;
            }
            return result;
        }

        [TestMethod]
        public MFTestResults EndingDirectory()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\root\\Dir1\\Dir2\\", "\\root\\Dir1\\Dir2"))
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
        public MFTestResults DeepTree()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string deepTree = "\\root\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2\\Dir2\\Dir1\\Dir2\\Dir1\\Dir2";
                if (!TestGetDirectoryName(deepTree + "\\File.exe", deepTree))
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
        public MFTestResults RootPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\", null))
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
        public MFTestResults UNCPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\\\Machine\\Directory\\File", "\\\\Machine\\Directory"))
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
        public MFTestResults DotsAtEnd()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetDirectoryName("\\root\\test\\.", "\\root\\test"))
                {
                    return MFTestResults.Fail;
                }
                try
                {
                    Path.GetDirectoryName("\\root\\test\\ .");
                    Log.Exception("Expected Argument exception");
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
                string[] invalidArgs = { "", " ", "  ", "\t", "\n", "\r\n" };
                foreach (string path in invalidArgs)
                {
                    try
                    {
                        string dir = Path.GetDirectoryName(path);
                        Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                        return MFTestResults.Fail;
                    }
                    catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: " + ae.Message ); }
                }
                foreach (char invalidChar in Path.GetInvalidPathChars())
                {
                    try
                    {
                        Log.Comment("Invalid char ascii val = " + (int)invalidChar);
                        string path = new string(new char[] { invalidChar, 'b', 'a', 'd', invalidChar, 'p', 'a', 't', 'h', invalidChar });
                        string dir = Path.GetDirectoryName(path);
                        Log.Exception("Expected Argument exception for '" + path + "' but got: '" + dir + "'");
                        return MFTestResults.Fail;
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
                    new MFTestMethod( NullPath, "NullPath" ),
                    new MFTestMethod( Vanilla, "Vanilla" ),
                    new MFTestMethod( StartWithSlash, "StartWithSlash" ),
                    new MFTestMethod( WhiteSpace, "WhiteSpace" ),
                    new MFTestMethod( ForwardSlashes, "ForwardSlashes" ),
                    new MFTestMethod( EndingDirectory, "EndingDirectory" ),
                    new MFTestMethod( DeepTree, "DeepTree" ),
                    new MFTestMethod( RootPath, "RootPath" ),
                    new MFTestMethod( UNCPath, "UNCPath" ),
                    new MFTestMethod( DotsAtEnd, "DotsAtEnd" ),
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                };
             }
        }
    }
}
