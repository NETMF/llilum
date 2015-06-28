////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class MemoryStream_Ctor : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            return InitializeResult.ReadyToGo;  
        }

        [TearDown]
        public void CleanUp()
        {
        }

        #region Helper methods
        private bool ValidateMemoryStream(MemoryStream ms, int ExpectedLength)
        {
            bool success = true;
            Log.Comment("Check Length");
            if (ms.Length != ExpectedLength)
            {
                success = false;
                Log.Exception("Expected Length 0, but got Length " + ms.Length);
            }
            Log.Comment("Check CanSeek");
            if (!ms.CanSeek)
            {
                success = false;
                Log.Exception("Expected CanSeek to be true, but was false");
            }
            Log.Comment("Check CanRead");
            if (!ms.CanRead)
            {
                success = false;
                Log.Exception("Expected CanRead to be true, but was false");
            } Log.Comment("Check CanWrite");
            if (!ms.CanWrite)
            {
                success = false;
                Log.Exception("Expected CanWrite to be true, but was false");
            }
            if (ms.Position != 0)
            {
                success = false;
                Log.Exception("Expected Position to be 0, but was " + ms.Position);
            }
            return success;
        }
        #endregion Helper methods

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidArguments()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                try
                {
                    Log.Comment("null buffer");
                    using (MemoryStream fs = new MemoryStream(null)) { }
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
                Log.Exception("Unexpected exception",  ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Valid_Default_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (!ValidateMemoryStream(ms, 0))
                        return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Variable_Buffer_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify buffer constructors length 0-100");
                for (int i = 0; i < 100; i++)
                {
                    byte[] buffer = new byte[i];
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        if (!ValidateMemoryStream(ms, i))
                            return MFTestResults.Fail;

                        Log.Comment("Try to extend beyond buffer length");
                        try 
                        { 
                            ms.SetLength(i + 1);
                            Log.Exception( "Expected NotSupportedException" );
                            return MFTestResults.Fail;
                        }
                        catch (NotSupportedException nse) 
                        { 
                            /* pass case */ Log.Comment( "Got correct exception: " + nse.Message );
                            result = MFTestResults.Pass;
                        }

                        Log.Comment("Truncate to 0");
                        for (int j = buffer.Length; j >= 0; j--)
                        {
                            ms.SetLength(j);
                        }
                    }
                }
                Log.Comment("Verify 10k buffer constructor");
                byte[] largeBuffer = new byte[10000];
                using (MemoryStream ms = new MemoryStream(largeBuffer))
                {
                    if (!ValidateMemoryStream(ms, largeBuffer.Length))
                        return MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
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
                    new MFTestMethod( Valid_Default_Ctor, "Valid_Default_Ctor" ),
                    new MFTestMethod( Variable_Buffer_Ctor, "Variable_Buffer_Ctor" ),
                };
             }
        }
    }
}
