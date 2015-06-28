////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;

namespace FileSystemTest
{
    public class FileDelete : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.                
            try
            {
                IOTests.IntializeVolume();

                Directory.CreateDirectory(sourceDir);
                Directory.CreateDirectory("Test " + sourceDir);
                Directory.SetCurrentDirectory(sourceDir);
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
        private const string file1Name = "file1.tmp";
        private const string file2Name = "file2.txt";
        private const string sourceDir = "source";
        #endregion Local vars

        #region Helper methods

        private bool TestDelete(string file)
        {
            bool success = true;
            Log.Comment("Deleting " + file);
            if (!File.Exists(file))
            {
                Log.Comment("Create " + file);
                File.Create(file).Close();
                if (!File.Exists(file))
                {
                    Log.Exception("Could not find file after creation!");
                    success = false;
                }
            }
            File.Delete(file);
            if (File.Exists(file))
            {
                Log.Exception("File still exists after delete!");
                success = false;
            }

            return success;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults ArgumentExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("Null argument");
                    File.Delete(null);
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentNullException ane) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ane.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("String.Empty argument");
                    File.Delete(string.Empty);
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Whitespace argument");
                    File.Delete("       ");
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("*.* argument");
                    File.Delete("*.*");
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Current dir '.' argument");
                    File.Delete(".");
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                } // UnauthorizedAccess 

                try
                {
                    Log.Comment("parent dir '..' argument");
                    File.Delete("..");
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                } // UnauthorizedAccess 
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception: " + ex.Message);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults IOExceptionTests()
        {
            MFTestResults result = MFTestResults.Pass;
            FileStream fs = null;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());
                try
                {
                    Log.Comment("non-existent file");
                    File.Delete("non-existent.file");
                    /// No exception is thrown for non existent file.
                }
                catch (IOException) 
                {
                    Log.Exception( "Unexpected IOException" );
                    return MFTestResults.Fail;
                }

                try
                {
                    Log.Comment("Read only file");
                    File.Create(file1Name).Close();
                    File.SetAttributes(file1Name, FileAttributes.ReadOnly);
                    File.Delete(file1Name);
                    Log.Exception( "Expected IOException" );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                } // UnauthorizedAccess 
                finally
                {
                    if (File.Exists(file1Name))
                    {
                        Log.Comment("Clean up read only file");
                        File.SetAttributes(file1Name, FileAttributes.Normal);
                        File.Delete(file1Name);
                    }
                }

                try
                {
                    Log.Comment("file in use");
                    fs = File.Create(file1Name);
                    File.Delete(file1Name);
                    Log.Exception( "Expected IOException" );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                }
                finally
                {
                    if (fs != null)
                    {
                        Log.Comment("Clean up file in use");
                        fs.Close();
                        File.Delete(file1Name);
                    }
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
            FileStream fs = null;
            try
            {
                Log.Comment("Current Directory: " + Directory.GetCurrentDirectory());

                Log.Comment("relative delete");
                if (!TestDelete(file1Name))
                    return MFTestResults.Fail;

                Log.Comment("absolute delete");
                if (!TestDelete(Directory.GetCurrentDirectory() + "\\" + file1Name))
                    return MFTestResults.Fail;

                Log.Comment("Case insensitive lower delete");
                File.Create(file1Name).Close();
                if (!TestDelete(file1Name.ToLower()))
                    return MFTestResults.Fail;

                Log.Comment("Case insensitive UPPER delete");
                File.Create(file2Name).Close();
                if (!TestDelete(file2Name.ToUpper()))
                    return MFTestResults.Fail; 
                
                Log.Comment("Write content to file");
                byte[] hello = UTF8Encoding.UTF8.GetBytes("Hello world!");
                fs = File.Create(file1Name);
                fs.Write(hello, 0, hello.Length);
                fs.Close();
                if (!TestDelete(file1Name))
                    return MFTestResults.Fail;

                Log.Comment("relative . delete");
                File.Create(file2Name).Close();
                if (!TestDelete(@".\" + file2Name))
                    return MFTestResults.Fail;

                Log.Comment("relative .. delete");
                File.Create(Path.Combine(IOTests.Volume.RootDirectory, file2Name)).Close();
                if (!TestDelete(@"..\" + file2Name))
                    return MFTestResults.Fail;

                Log.Comment("hidden file delete");
                File.Create(file1Name).Close();
                File.SetAttributes(file1Name, FileAttributes.Hidden);
                if (!TestDelete(file1Name))
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
        public MFTestResults SpecialFileNames()
        {
            MFTestResults result = MFTestResults.Pass;
            char[] special = new char[] { '!', '#', '$', '%', '\'', '(', ')', '+', '-', '.', '@', '[', ']', '_', '`', '{', '}', '~' };

            try
            {
                Log.Comment("Create file each with special char file names");
                for (int i = 0; i < special.Length; i++)
                {
                    string file = i + "_" + new string(new char[] { special[i] }) + "_z.file";
                    if (!TestDelete(file))
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
                    new MFTestMethod( ArgumentExceptionTests, "ArgumentExceptionTests" ),
                    new MFTestMethod( IOExceptionTests, "IOExceptionTests" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                    new MFTestMethod( SpecialFileNames, "SpecialFileNames" ),
                };
             }
        }
    }
}
