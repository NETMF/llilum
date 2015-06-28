////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class MemoryStreamCanRead : IMFTestInterface
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
        public MFTestResults CanRead_Default_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanRead is true for default Ctor");
                using (MemoryStream fs = new MemoryStream())
                {
                    if (!fs.CanRead)
                    {
                        Log.Exception( "Expected CanRead == true, but got CanRead == false" );
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
        public MFTestResults CanRead_Byte_Ctor()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                Log.Comment("Verify CanRead is true for Byte[] Ctor");
                byte[] buffer = new byte[1024];
                using (MemoryStream fs = new MemoryStream(buffer))
                {
                    if (!fs.CanRead)
                    {
                        Log.Exception( "Expected CanRead == true, but got CanRead == false" );
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
                    new MFTestMethod( CanRead_Default_Ctor, "CanRead_Default_Ctor" ),
                    new MFTestMethod( CanRead_Byte_Ctor, "CanRead_Byte_Ctor" ),
                };
             }
        }
    }
}
