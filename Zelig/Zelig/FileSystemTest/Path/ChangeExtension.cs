////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;



using System.IO;

namespace FileSystemTest
{
    public class ChangeExtensions : IMFTestInterface
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



        #region class vars
        private const string defaultPath = @"de\\fault.path";
        private const string exe = ".exe";
        private const string cool = ".cool";
        #endregion class vars

        #region helper funtions
        private bool TestChangeExtension(String path, String extension)
        {
            string expected = "";
            int iIndex = path.LastIndexOf('.') ;
            if(iIndex > -1)
            {
                switch(extension)
                {
                    case null:
                        expected = path.Substring( 0, iIndex );
                        break;
                    case "":
                        expected = path.Substring( 0, iIndex + 1 );
                        break;
                    default:
                        expected = path.Substring( 0, iIndex ) + extension;
                        break;
                }
            }
            else if(extension != null)
            {
                expected = path + extension;
            }
            else
            {
                expected = path;
            }

            Log.Comment("Original Path: " + path);
            Log.Comment("Expected Path: " + expected);
            string result = Path.ChangeExtension(path, extension);
            if (result != expected)
            {
                Log.Exception("Got Path: " + result);
                return false;
            }
            return true;
        }
        #endregion helper functions

        #region Test Cases
        [TestMethod]
        public MFTestResults NullArgumentPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string strExtension = Path.ChangeExtension(null, exe);
                Log.Comment("Expect: null");
                if (strExtension != null)
                {
                    Log.Exception("FAIL - Got: " + strExtension);
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
        public MFTestResults NullArgumentExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestChangeExtension(defaultPath, null))
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
        public MFTestResults ZeroLengthPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string strExtension = Path.ChangeExtension("", exe);
                Log.Comment("Expect empty result");
                if ( strExtension != "")
                {
                    Log.Exception("Got: '" + strExtension + "'");
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
        public MFTestResults ZeroLengthExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestChangeExtension(defaultPath, ""))
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
        public MFTestResults StringEmptyPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string strExtension = Path.ChangeExtension("", exe);
                if (strExtension != String.Empty)
                {
                    Log.Exception("Got: '" + strExtension + "'");
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
        public MFTestResults StringEmptyExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                if (!TestChangeExtension(defaultPath, string.Empty))
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
                string strExtension = Path.ChangeExtension("   ", exe);
                Log.Comment("BUG? - The Desktop has the same behavior, but this is their test case, so don't know right behavior");
                Log.Comment("We will wait to hear back from CLR team to decide what the correct behavior is.");
                Log.Exception("Expected ArgumentException, got " + strExtension);
                result = MFTestResults.KnownFailure;
            }
            catch (ArgumentException ae) 
            { 
                /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
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
        public MFTestResults InvalidChars()
        {
            MFTestResults result = MFTestResults.Pass;
            foreach (char badChar in Path.InvalidPathChars)
            {
                try
                {
                    string path = new string(new char[] { badChar, 'b', 'a', 'd', badChar, 'p', 'a', 't', 'h', badChar });
                    Log.FilteredComment("Testing path: " + path);
                    string strExtension = Path.ChangeExtension(path, exe);
                }
                catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: " + ae.Message ); }
                catch (Exception ex)
                {
                    Log.Exception("Unexpected exception: " + ex.Message);
                    return MFTestResults.Fail;
                }
            }
            return result;
        }

        [TestMethod]
        public MFTestResults NoExtensionPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "jabba\\de\\hutt";
                if (! TestChangeExtension(path, exe))
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
        public MFTestResults MultiDotPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "jabba..de..hutt...";
                if (!TestChangeExtension(path, exe))
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
        public MFTestResults ValidExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "jabba\\de\\hutt.solo";
                if (!TestChangeExtension(path, exe))
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
        public MFTestResults SpecialSymbolPath()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = "foo.bar.fkl;fkds92-509450-4359.213213213@*?2-3203-=210";
                if (!TestChangeExtension(path, cool))
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
        public MFTestResults SpecialSymbolExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string extension = ".$#@$_)+_)!@@!!@##&_$)#_";
                if (!TestChangeExtension(defaultPath, extension))
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
        public MFTestResults LongExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string path = new string('a', 256) + exe;
                string extension = "." + new string('b', 256);
                string strExtension = Path.ChangeExtension(path, extension);
                if (!TestChangeExtension(path, extension))
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
        public MFTestResults OneCharExtension()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                string extension = ".z";
                if (!TestChangeExtension(defaultPath, extension))
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

        #endregion Test Cases

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( NullArgumentPath, "NullArgumentPath" ),
                    new MFTestMethod( NullArgumentExtension, "NullArgumentExtension" ),
                    new MFTestMethod( ZeroLengthPath, "ZeroLengthPath" ),
                    new MFTestMethod( ZeroLengthExtension, "ZeroLengthExtension" ),
                    new MFTestMethod( StringEmptyPath, "StringEmptyPath" ),
                    new MFTestMethod( StringEmptyExtension, "StringEmptyExtension" ),
                    new MFTestMethod( WhiteSpace, "WhiteSpace" ),
                    new MFTestMethod( InvalidChars, "InvalidChars" ),
                    new MFTestMethod( NoExtensionPath, "NoExtensionPath" ),
                    new MFTestMethod( MultiDotPath, "MultiDotPath" ),
                    new MFTestMethod( ValidExtension, "ValidExtension" ),
                    new MFTestMethod( SpecialSymbolPath, "SpecialSymbolPath" ),
                    new MFTestMethod( SpecialSymbolExtension, "SpecialSymbolExtension" ),
                    new MFTestMethod( LongExtension, "LongExtension" ),
                    new MFTestMethod( OneCharExtension, "OneCharExtension" ),
                };
             }
        }
    }
}
