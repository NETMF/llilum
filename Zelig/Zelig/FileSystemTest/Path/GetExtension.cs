////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;



using System.IO;

namespace FileSystemTest
{
    public class GetExtension : IMFTestInterface
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

        private bool TestGetExtension(string path, string expected)
        {
            string result = Path.GetExtension(path);
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
                if (!TestGetExtension(null, null))
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
        public MFTestResults StringEmpty()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetExtension(String.Empty, String.Empty))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension("", ""))
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
        public MFTestResults PathNoExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetExtension("\\jabba\\de\\hutt", ""))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension("jabba\\de\\hutt", ""))
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
        public MFTestResults MultiDots()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetExtension("luke.......sky....", ""))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension("luke.sky.Walker...", ""))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension(@"luke.sky.Walker.\..", ""))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension(@"lukeskyWalker\.", ""))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension(@"luke.sky.Walker.", ""))
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
        public MFTestResults ValidExension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestGetExtension(@"dir1\dir2\file.1", ".1"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension(@"\sd1\dir1\dir2\file.to", ".to"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestGetExtension(@"\sd1\dir1\dir2\file.txt", ".txt"))
                {
                    return MFTestResults.Fail;
                }
                //File name has special chars, but valid extension
                if (!TestGetExtension(@"foo.bar.fkl;fkds92-509450-4359.$#%()#%().%#(%)_#(%_).cool", ".cool"))
                {
                    return MFTestResults.Fail;
                }
                //Extension has special chars
                string extension = ".$#@$_)+_)!@@!!@##&_$)#_";
                if (!TestGetExtension("foo" + extension, extension))
                {
                    return MFTestResults.Fail;
                } 
                if (!TestGetExtension(@"\sd1\dir1\dir2\file.longextensionname", ".longextensionname"))
                {
                    return MFTestResults.Fail;
                }
                string verylong = "." + new string('x', 256);
                if (!TestGetExtension(@"\sd1\dir1\dir2\file" + verylong, verylong))
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
                        string dir = Path.GetExtension(path);
                                                
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
                    new MFTestMethod( StringEmpty, "StringEmpty" ),
                    new MFTestMethod( PathNoExtension, "PathNoExtension" ),
                    new MFTestMethod( MultiDots, "MultiDots" ),
                    new MFTestMethod( ValidExension, "ValidExension" ),
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                };
             }
        }
    }
}
