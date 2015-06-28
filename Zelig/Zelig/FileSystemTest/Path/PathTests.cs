/*---------------------------------------------------------------------
* PathTests.cs - file description
* Version: 1.0
* Author: 
* Created: 
* 
* Tests the basic functionality of Socket objects 
* ---------------------------------------------------------------------*/

using System;
using System.Reflection;


using System.IO;


namespace FileSystemTest
{

    public class PathTests : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // delete the Path DOTNETMF_FS_EMULATION
            try { IOTests.IntializeVolume(); }
            catch { return InitializeResult.Skip; }
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults PathTest0_ChangeExtension()
        {
            bool testResult = false;

            try
            {
                string newPath = Path.ChangeExtension(@"pathtest\testdir\test.ext1", ".ext2");
                testResult = String.Equals(newPath, @"pathtest\testdir\test.ext2");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// If new extension is null, then file extension should be removed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_ChangeExtension1()
        {
            bool testResult = false;

            try
            {
                string newPath = Path.ChangeExtension(@"pathtest\testdir\test.ext1", null);
                testResult = String.Equals(newPath, @"pathtest\testdir\test");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// If path is null, it should be returned unmodified.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_ChangeExtension2()
        {
            bool testResult = false;

            try
            {
                string newPath = Path.ChangeExtension(null, ".ext");
                testResult = (newPath == null);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// If path is empty, it should be returned unmodified.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_ChangeExtension3()
        {
            bool testResult = false;

            try
            {
                string newPath = Path.ChangeExtension("", ".ext");
                testResult = (newPath == "");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// If path is empty, it should be returned unmodified.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_ChangeExtension4()
        {
            bool testResult = false;

            try
            {
                string newPath = Path.ChangeExtension("", ".ext");
                testResult = (newPath == "");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Negative test case, path contains invalid character.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_ChangeExtension5()
        {
            bool testResult = true;

            try
            {
                string newPath = Path.ChangeExtension(@"pathtest\testdir\test>.ext1", ".ext");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Expected Exception", ex);
                testResult = true;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Extension does not contain dot.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_ChangeExtension6()
        {
            bool testResult = false;

            try
            {
                string newPath = Path.ChangeExtension(@"pathtest\testdir\test.ext1", "ext");
                testResult = String.Equals(newPath, @"pathtest\testdir\test.ext");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        

        [TestMethod]
        public MFTestResults PathTest1_GetDirectoryName()
        {
            bool testResult = false;

            try
            {
                string dirName = Path.GetDirectoryName(@"pathtest\testdir\test.ext1");
                testResult = String.Equals(dirName, @"pathtest\testdir");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Get parent directory name of a directory
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_GetDirectoryName1()
        {
            bool testResult = false;

            try
            {
                string dirName = Path.GetDirectoryName(@"pathtest\testdir");
                testResult = String.Equals(dirName, @"pathtest");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Parent of root directory
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_GetDirectoryName2()
        {
            bool testResult = false;

            try
            {
                string dirName = Path.GetDirectoryName(IOTests.Volume.RootDirectory);
                testResult = String.Equals(dirName, @"\");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Parent of root directory
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_GetDirectoryName3()
        {
            bool testResult = false;

            try
            {
                string dirName = Path.GetDirectoryName(@"");
            }
            catch (ArgumentException)
            {
                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        /// <summary>
        /// Parent of root directory
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_GetDirectoryName4()
        {
            bool testResult = false;

            try
            {
                string dirName = Path.GetDirectoryName(null);
                testResult = (dirName == null);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }


        /// <summary>
        /// Parent of root directory
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public MFTestResults PathTest_GetDirectoryName5()
        {
            bool testResult = true;

            try
            {
                string dirName = Path.GetDirectoryName(@"abc\abc>");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Expected Exception", ex);
                testResult = true;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetDirectoryName6()
        {
            bool testResult = true;

            try
            {
                string dirName = Path.GetDirectoryName(@"\\\abc\abc>");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Expected Exception", ex);
                testResult = true;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFullPath0()
        {
            bool testResult = false;

            try
            {
                string fullPath = Path.GetFullPath(@"dir1\dir2\file.ext");
                testResult = String.Equals(fullPath, IOTests.Volume.RootDirectory + @"\dir1\dir2\file.ext");

                Log.Comment(fullPath);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFullPath1()
        {
            bool testResult = false;

            try
            {
                string fullPath = Path.GetFullPath(IOTests.Volume.RootDirectory + @"\dir1\dir2\file.ext");
                testResult = String.Equals(fullPath, IOTests.Volume.RootDirectory + @"\dir1\dir2\file.ext");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFullPath2()
        {
            bool testResult = false;

            try
            {
                string fullPath = Path.GetFullPath(IOTests.Volume.RootDirectory + @"\dir1\..\dir2\file.ext");
                testResult = String.Equals(fullPath, IOTests.Volume.RootDirectory + @"\dir2\file.ext");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetExtension1()
        {
            bool testResult = false;

            try
            {
                string ext = Path.GetExtension(@"pathtest\testdir\test");
                testResult = String.Equals(ext, "");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetExtension2()
        {
            bool testResult = false;

            try
            {
                string ext = Path.GetExtension(null);
                testResult = (ext == null);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetExtension3()
        {
            bool testResult = true;

            try
            {
                try
                {
                    string ext = Path.GetExtension(@"pathtest\testdir\test>.txt");
                    testResult = false;
                }
                catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: res=" + testResult.ToString() + ", " + ae.Message ); }
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetExtension4()
        {
            bool testResult = false;

            try
            {
                string ext = Path.GetExtension(@"pathtest\testdir\test.txt.txt2.txt3");
                testResult = String.Equals(ext, ".txt3");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest4_GetFileName()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileName(@"pathtest\testdir\test.ext2");
                testResult = String.Equals(fileName, "test.ext2");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileName1()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileName(@"pathtest\testdir\test.ext2\");
                testResult = String.Equals(fileName, "");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileName2()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileName(@"pathtest\testdir\test.ext2:");
            }
            catch (ArgumentException)
            {
                testResult = true;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileName3()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileName(null);
                testResult = (fileName == null);
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileName4()
        {
            bool testResult = true;

            try
            {
                string fileName = Path.GetFileName(@"pathtest\testdir\test.ext2>");
                testResult = false;
            }
            catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: res=" + testResult.ToString() + ", " + ae.Message ); }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest5_GetFileNameWithoutExtension()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(@"pathtest\testdir\test.ext2");
                testResult = String.Equals(fileName, "test");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileNameWithoutExtension1()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(@"pathtest\testdir\test.ext.ext1.ext2");
                testResult = String.Equals(fileName, "test.ext.ext1");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileNameWithoutExtension2()
        {
            bool testResult = false;

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(@"pathtest\testdir\test");
                testResult = String.Equals(fileName, "test");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_GetFileNameWithoutExtension3()
        {
            bool testResult = true;

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(@"pathtest\testdir\test>");
                testResult = false;
            }
            catch (ArgumentException ae) { /* pass case */ Log.Comment( "Got correct exception: res=" + testResult.ToString() + ", " + ae.Message ); }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }
            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest6_GetPathRoot()
        {
            bool testResult = false;

            try
            {
                string root = Path.GetPathRoot(@"pathtest\testdir\test.ext2");
                testResult = String.Equals(root, "");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest7_HasExtension()
        {
            bool testResult = false;

            try
            {
                testResult = Path.HasExtension(@"pathtest\testdir\test.ext2");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_HasExtension1()
        {
            bool testResult = false;

            try
            {
                testResult = !Path.HasExtension(@"pathtest\testdir\test");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_HasExtension2()
        {
            bool testResult = false;

            try
            {
                testResult = !Path.HasExtension(@"pathtest\testdir\test.ext2\");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest8_IsPathRooted()
        {
            bool testResult = false;

            try
            {
                testResult = Path.IsPathRooted(@"\pathtest\testdir\test.ext2");                
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_IsPathRooted1()
        {
            bool testResult = false;

            try
            {
                testResult = !Path.IsPathRooted(@"pathtest\testdir\test");
            }
            catch (Exception ex)
            {
                testResult = false;
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_IsPathRooted2()
        {
            bool testResult = false;

            try
            {
                /// Weird situation, according to MSDN this may be valid.
                testResult = Path.IsPathRooted(@"\\\\\\");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_IsPathRooted3()
        {
            bool testResult = false;

            try
            {
                testResult = Path.IsPathRooted(@"\");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_IsPathRooted4()
        {
            bool testResult = false;

            try
            {
                testResult = Path.IsPathRooted(@"\COM1");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest9_Combine()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"pathtest\testdir\test", "test2");
                testResult = String.Equals(path, @"pathtest\testdir\test\test2");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine1()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"pathtest\testdir\test", @"\test2");
                testResult = String.Equals(path, @"\test2");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine2()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"\pathtest\testdir\test", @"\test2");
                testResult = String.Equals(path, @"\test2");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine3()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"\pathtest\testdir\test", @"\test2 ");
                testResult = String.Equals(path, @"\test2 ");
                /// Note: There's some difference between MSDN documentation of this case, and actual behavior. Notice
                /// the whitespace after test2.
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine4()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"\pathtest\testdir\test", @"test2*.txt");
                testResult = String.Equals(path, @"\pathtest\testdir\test\test2*.txt");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine5()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"\pathtest\testdir\test", @"..\test2.txt");
                testResult = String.Equals(path, @"\pathtest\testdir\test\..\test2.txt");
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine6()
        {
            bool testResult = false;

            try
            {
                string path = Path.Combine(@"^*&)(_=@&#2.*(.txt", @"dir\test2.txt");
                testResult = String.Equals(path, @"^*&)(_=@&#2.*(.txt\dir\test2.txt");                
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        [TestMethod]
        public MFTestResults PathTest_Combine7()
        {
            bool testResult = true;

            try
            {
                string path = Path.Combine("//" + IOTests.Volume.Name + "//dir", @"dir\test2.txt");
                testResult = false;
            }
            catch (Exception ex)
            {
                Log.Exception("Expected Exception", ex);
                testResult = true;
            }

            return (testResult ? MFTestResults.Pass : MFTestResults.Fail);
        }

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( PathTest0_ChangeExtension, "PathTest0_ChangeExtension" ),
                    new MFTestMethod( PathTest_ChangeExtension1, "PathTest_ChangeExtension1" ),
                    new MFTestMethod( PathTest_ChangeExtension2, "PathTest_ChangeExtension2" ),
                    new MFTestMethod( PathTest_ChangeExtension3, "PathTest_ChangeExtension3" ),
                    new MFTestMethod( PathTest_ChangeExtension4, "PathTest_ChangeExtension4" ),
                    new MFTestMethod( PathTest_ChangeExtension5, "PathTest_ChangeExtension5" ),
                    new MFTestMethod( PathTest_ChangeExtension6, "PathTest_ChangeExtension6" ),
                    new MFTestMethod( PathTest1_GetDirectoryName, "PathTest1_GetDirectoryName" ),
                    new MFTestMethod( PathTest_GetDirectoryName1, "PathTest_GetDirectoryName1" ),
                    new MFTestMethod( PathTest_GetDirectoryName2, "PathTest_GetDirectoryName2" ),
                    new MFTestMethod( PathTest_GetDirectoryName3, "PathTest_GetDirectoryName3" ),
                    new MFTestMethod( PathTest_GetDirectoryName4, "PathTest_GetDirectoryName4" ),
                    new MFTestMethod( PathTest_GetDirectoryName5, "PathTest_GetDirectoryName5" ),
                    new MFTestMethod( PathTest_GetDirectoryName6, "PathTest_GetDirectoryName6" ),
                    new MFTestMethod( PathTest_GetFullPath0, "PathTest_GetFullPath0" ),
                    new MFTestMethod( PathTest_GetFullPath1, "PathTest_GetFullPath1" ),
                    new MFTestMethod( PathTest_GetFullPath2, "PathTest_GetFullPath2" ),
                    new MFTestMethod( PathTest_GetExtension1, "PathTest_GetExtension1" ),
                    new MFTestMethod( PathTest_GetExtension2, "PathTest_GetExtension2" ),
                    new MFTestMethod( PathTest_GetExtension3, "PathTest_GetExtension3" ),
                    new MFTestMethod( PathTest_GetExtension4, "PathTest_GetExtension4" ),
                    new MFTestMethod( PathTest4_GetFileName, "PathTest4_GetFileName" ),
                    new MFTestMethod( PathTest_GetFileName1, "PathTest_GetFileName1" ),
                    new MFTestMethod( PathTest_GetFileName2, "PathTest_GetFileName2" ),
                    new MFTestMethod( PathTest_GetFileName3, "PathTest_GetFileName3" ),
                    new MFTestMethod( PathTest_GetFileName4, "PathTest_GetFileName4" ),
                    new MFTestMethod( PathTest5_GetFileNameWithoutExtension, "PathTest5_GetFileNameWithoutExtension" ),
                    new MFTestMethod( PathTest_GetFileNameWithoutExtension1, "PathTest_GetFileNameWithoutExtension1" ),
                    new MFTestMethod( PathTest_GetFileNameWithoutExtension2, "PathTest_GetFileNameWithoutExtension2" ),
                    new MFTestMethod( PathTest_GetFileNameWithoutExtension3, "PathTest_GetFileNameWithoutExtension3" ),
                    new MFTestMethod( PathTest6_GetPathRoot, "PathTest6_GetPathRoot" ),
                    new MFTestMethod( PathTest7_HasExtension, "PathTest7_HasExtension" ),
                    new MFTestMethod( PathTest_HasExtension1, "PathTest_HasExtension1" ),
                    new MFTestMethod( PathTest_HasExtension2, "PathTest_HasExtension2" ),
                    new MFTestMethod( PathTest8_IsPathRooted, "PathTest8_IsPathRooted" ),
                    new MFTestMethod( PathTest_IsPathRooted1, "PathTest_IsPathRooted1" ),
                    new MFTestMethod( PathTest_IsPathRooted2, "PathTest_IsPathRooted2" ),
                    new MFTestMethod( PathTest_IsPathRooted3, "PathTest_IsPathRooted3" ),
                    new MFTestMethod( PathTest_IsPathRooted4, "PathTest_IsPathRooted4" ),
                    new MFTestMethod( PathTest9_Combine, "PathTest9_Combine" ),
                    new MFTestMethod( PathTest_Combine1, "PathTest_Combine1" ),
                    new MFTestMethod( PathTest_Combine2, "PathTest_Combine2" ),
                    new MFTestMethod( PathTest_Combine3, "PathTest_Combine3" ),
                    new MFTestMethod( PathTest_Combine4, "PathTest_Combine4" ),
                    new MFTestMethod( PathTest_Combine5, "PathTest_Combine5" ),
                    new MFTestMethod( PathTest_Combine6, "PathTest_Combine6" ),
                    new MFTestMethod( PathTest_Combine7, "PathTest_Combine7" ),
                };
             }
        }
    }
}
