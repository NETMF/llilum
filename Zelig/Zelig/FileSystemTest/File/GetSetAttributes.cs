////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class GetSetAttributes : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            // These tests rely on underlying file system so we need to make
            // sure we can format it before we start the tests.  If we can't
            // format it, then we assume there is no FS to test on this platform.

            // delete the directory DOTNETMF_FS_EMULATION
            try
            {
                IOTests.IntializeVolume();

                Directory.CreateDirectory(testDir);
                Directory.SetCurrentDirectory(testDir);
                File.Create(file1Name).Close();
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
        private const string testDir = "GetAttributesDir";
        #endregion Local vars

        #region Helper methods

        private bool TestSetGetAttributes(string path, FileAttributes expected)
        {
            Log.Comment("Setting file " + path + " to attribute " + expected);
            File.SetAttributes(path, expected);
            return TestGetAttributes(path, expected);
        }
        private bool TestGetAttributes(string path, FileAttributes expected)
        {
            Log.Comment("Checking file " + path + " for attribute " + expected);
            FileAttributes fa = File.GetAttributes(path);
            if (fa != expected)
            {
                Log.Exception("Unexpected value - got " + fa);
                return false;
            }
            return true;
        }
        #endregion Helper methods

        #region Test Cases

        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            FileAttributes file;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("Get Null");
                    file = File.GetAttributes(null);
                    Log.Exception( "Expected ArgumentException, but got " + file );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("Get String.Empty");
                    file = File.GetAttributes(String.Empty);
                    Log.Exception( "Expected ArgumentException, but got " + file );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("Get White Space");
                    file = File.GetAttributes("       ");
                    Log.Exception( "Expected ArgumentException, but got " + file );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ae.Message );
                    result = MFTestResults.Pass;
                }

                try
                {
                    Log.Comment("Set Null");
                    File.SetAttributes(null, FileAttributes.Normal);
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
                    Log.Comment("Set String.Empty");
                    File.SetAttributes(String.Empty, FileAttributes.Normal);
                    Log.Exception( "Expected ArgumentException" );
                    return MFTestResults.Fail;
                }
                catch (ArgumentException ae) 
                {
                    /* pass case */
                    Log.Comment( "Got correct exception: " + ae.Message ); 
                    result = MFTestResults.Pass;
                }
                try
                {
                    Log.Comment("Set White Space");
                    File.SetAttributes("       ", FileAttributes.Normal);
                    Log.Exception( "Expected ArgumentException" );
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
        public MFTestResults IOExceptions()
        {
            FileAttributes file;
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Get Check Directory");
                file = File.GetAttributes(Directory.GetCurrentDirectory()); 
                
                try
                {
                    Log.Comment("Get non-existent file");
                    file = File.GetAttributes("non-existent");
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                } // FileNotFound

                Log.Comment("Set Check Directory");
                File.SetAttributes(Directory.GetCurrentDirectory(), FileAttributes.Normal);                     

                try
                {
                    Log.Comment("Set non-existent file");
                    File.SetAttributes("non-existent", FileAttributes.Normal);
                    Log.Exception( "Expected IOException" );
                    return MFTestResults.Fail;
                }
                catch (IOException ioe) 
                { 
                    /* pass case */  Log.Comment( "Got correct exception: " + ioe.Message );
                    result = MFTestResults.Pass;
                }

                Log.Comment("Set Directory");
                File.SetAttributes(file1Name, FileAttributes.Directory);                 

                Log.Comment("Set Normal | ReadOnly");
                File.SetAttributes(file1Name, FileAttributes.Normal | FileAttributes.ReadOnly);                 

                Log.Comment("Set Normal | Hidden");
                File.SetAttributes(file1Name, FileAttributes.Normal | FileAttributes.Hidden);

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
        public MFTestResults ValidCases()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                /// Initial status is Hidden because of the test above.
                /// Log.Comment("Default Normal attribute");
                /// if (!TestGetAttributes(file1Name, FileAttributes.Normal))
                ///     return MFTestResults.Fail;

                Log.Comment("Read Only attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.ReadOnly))
                    return MFTestResults.Fail;

                Log.Comment("Hidden attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.Hidden))
                    return MFTestResults.Fail;

                Log.Comment("ReadOnly & Hidden attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.Hidden | FileAttributes.ReadOnly))
                    return MFTestResults.Fail;

                Log.Comment("Back to Normal attribute");
                if (!TestSetGetAttributes(file1Name, FileAttributes.Normal))
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
                    new MFTestMethod( IOExceptions, "IOExceptions" ),
                    new MFTestMethod( ValidCases, "ValidCases" ),
                };
             }
        }
    }
}
