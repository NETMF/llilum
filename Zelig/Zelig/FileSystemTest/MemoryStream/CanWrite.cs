////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class MemoryStreamCanWrite : IMFTestInterface
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
        public MFTestResults CanWrite_Default_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanWrite is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanWrite)
                    {
                        Log.Exception( "Expected CanWrite == true, but got CanWrite == false" );
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
        public MFTestResults CanWrite_Byte_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanWrite is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanWrite)
                    {
                        Log.Exception( "Expected CanWrite == true, but got CanWrite == false" );
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
                    new MFTestMethod( CanWrite_Default_Ctor, "CanWrite_Default_Ctor" ),
                    new MFTestMethod( CanWrite_Byte_Ctor, "CanWrite_Byte_Ctor" ),
                };
             }
        }
    }
}
