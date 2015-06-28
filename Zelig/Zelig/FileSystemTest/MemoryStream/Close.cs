////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class Close : IMFTestInterface
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
        public MFTestResults VerifyClose()
        {
            MFTestResults result = MFTestResults.Pass;
            try
            {
                MemoryStream ms = new MemoryStream();
                ms.WriteByte(0);
                Log.Comment("Close stream");
                ms.Close();

                try
                {
                    Log.Comment("Verify actually closed by writing to it");
                    ms.WriteByte(0);
                    Log.Exception( "Expected ObjectDisposedException" );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException ode) 
                { 
                    /* pass case */ Log.Comment( "Got correct exception: " + ode.Message );
                    result = MFTestResults.Pass;
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
                    new MFTestMethod( VerifyClose, "VerifyClose" ),
                };
             }
        }
    }
}
