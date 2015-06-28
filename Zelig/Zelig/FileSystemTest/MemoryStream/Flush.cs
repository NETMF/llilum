////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class MemoryStreamFlush : IMFTestInterface
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

        [TestMethod]
        public MFTestResults VerifyFlush()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] data = MFUtilities.GetRandomBytes(5000);
                    ms.Write(data, 0, data.Length);
                    ms.Flush();
                    if (ms.Length != 5000)
                    {
                        Log.Exception( "Expected 5000 bytes, but got " + ms.Length );
                        return MFTestResults.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception( "Unexpected exception", ex );
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
                    new MFTestMethod( VerifyFlush, "VerifyFlush" ),
                };
             }
        }
    }
}
