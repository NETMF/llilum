////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;



using System.IO;

namespace FileSystemTest
{
    public class Combine : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.                

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }


        #region helper methods
        private bool TestCombine(string arg1, string arg2, string expected)
        {
            string path = Path.Combine(arg1, arg2);
            Log.Comment("Arg1: '" + arg1 + "'");
            Log.Comment("Arg2: '" + arg2 + "'");
            Log.Comment("Expected: '" + expected + "'");
            if (path != expected)
            {
                Log.Exception("Got: '" + path + "'");
                return false;
            }
            return true;
        }
        # endregion helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults NullArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("null 1st param");
                string path = Path.Combine(null, "");
                Log.Exception("Expected Argument exception, but got path: " + path);
            }
            catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: " + ae.Message ); }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }
            try
            {
                Log.Comment("null 2nd param");
                string path = Path.Combine("", null);
                Log.Exception("Expected Argument exception, but got path: " + path);
            }
            catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: " + ae.Message ); }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Root()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string root = "\\";
                /// Expected output "\\" or root, See remarks section in MSDN Path.Combine
                /// http://msdn.microsoft.com/en-us/library/system.io.path.combine.aspx?PHPSESSID=ca9tbhkv7klmem4g3b2ru2q4d4
                if (!TestCombine(root, root, root))
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
        public MFTestResults SameRoot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string root = "\\nand1";
                if (!TestCombine(root, root, root))
                {
                    return MFTestResults.Fail;
                }
                if (!TestCombine(root + "\\dir", root + "\\dir", root + "\\dir"))
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
        public MFTestResults EmptyArgs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string croot = "\\sd1";
                if (!TestCombine(croot, "", croot))
                {
                    return MFTestResults.Fail;
                }
                if (!TestCombine("", croot, croot))
                {
                    return MFTestResults.Fail;
                }
                if (!TestCombine("", "", ""))
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
        public MFTestResults TwoUnique()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("Hello", "World", "Hello\\World"))
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
        public MFTestResults TwoUniqueWithRoot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\Hello\\", "World", "\\sd1\\Hello\\World"))
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
        public MFTestResults SecondBeginWithSlash()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\Hello", "\\World", "\\World"))
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
        public MFTestResults UNCName()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string unc = @"\\\\radt\VbSsDb\VbTests\shadow\FXBCL\test\auto\System_IO\Path\";
                if (!TestCombine(unc, "World", unc + "World"))
                {
                    return MFTestResults.Fail;
                }

                if (!TestCombine("\\", unc, unc))
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
        public MFTestResults MultipleSubDirs()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\MyDir\\Hello\\", "World\\You\\Are\\My\\Creation", "\\MyDir\\Hello\\World\\You\\Are\\My\\Creation"))
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
        public MFTestResults FirstRootedSecondNot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\MyDirectory\\Sample", "Test", "\\sd1\\MyDirectory\\Sample\\Test"))
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
        public MFTestResults CombineDot()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestCombine("\\sd1\\Directory", ".\\SubDir", "\\sd1\\Directory\\.\\SubDir"))
                {
                    return MFTestResults.Fail;
                }
                if (!TestCombine("\\sd1\\Directory\\..", "SubDir", "\\sd1\\Directory\\..\\SubDir"))
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
        public MFTestResults WhiteSpace()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                //white space inside
                if (!TestCombine("\\sd1\\Directory Name", "Sub Dir", "\\sd1\\Directory Name\\Sub Dir"))
                {
                    return MFTestResults.Fail;
                }
                //white space end of arg1
                /// Since path2 is rooted, it is also the expected result. See MSDN remarks section:
                /// http://msdn.microsoft.com/en-us/library/system.io.path.combine.aspx?PHPSESSID=ca9tbhkv7klmem4g3b2ru2q4d4
                if (!TestCombine("\\sd1\\Directory Name\\ ", "\\Sub Dir", "\\Sub Dir"))
                {
                    return MFTestResults.Fail;
                }
                //white space start of arg2
                if (!TestCombine("\\sd1\\Directory Name", " \\Sub Dir", "\\sd1\\Directory Name\\ \\Sub Dir"))
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
                /// Forward slash is illegal for us, an exception should be thrown.
                TestCombine("//sd1//Directory Name//", "Sub//Dir", "//sd1//Directory Name//Sub//Dir");
                return MFTestResults.Fail;
            }
            catch (Exception ex)
            {
                Log.Exception("Exception: " + ex.Message);
                result = MFTestResults.Pass;
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
                    new MFTestMethod( Root, "Root" ),
                    new MFTestMethod( SameRoot, "SameRoot" ),
                    new MFTestMethod( EmptyArgs, "EmptyArgs" ),
                    new MFTestMethod( TwoUnique, "TwoUnique" ),
                    new MFTestMethod( TwoUniqueWithRoot, "TwoUniqueWithRoot" ),
                    new MFTestMethod( SecondBeginWithSlash, "SecondBeginWithSlash" ),
                    new MFTestMethod( UNCName, "UNCName" ),
                    new MFTestMethod( MultipleSubDirs, "MultipleSubDirs" ),
                    new MFTestMethod( FirstRootedSecondNot, "FirstRootedSecondNot" ),
                    new MFTestMethod( CombineDot, "CombineDot" ),
                    new MFTestMethod( WhiteSpace, "WhiteSpace" ),
                    new MFTestMethod( ForwardSlashes, "ForwardSlashes" ),
                };
             }
        }
    }
}
