////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class MemoryStreamCanSeek : IMFTestInterface
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
        public MFTestResults CanSeek_Default_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanSeek is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanSeek)
                    {
                        Log.Exception( "Expected CanSeek == true, but got CanSeek == false" );
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

        [TestMethod]
        public MFTestResults CanSeek_Byte_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanSeek is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanSeek)
                    {
                        Log.Exception( "Expected CanSeek == true, but got CanSeek == false" );
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
                    new MFTestMethod( CanSeek_Default_Ctor, "CanSeek_Default_Ctor" ),
                    new MFTestMethod( CanSeek_Byte_Ctor, "CanSeek_Byte_Ctor" ),
                };
             }
        }
    }
}
