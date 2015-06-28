////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace FileSystemTest
{
    public class ReadByte : IMFTestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            // TODO: Add your set up steps here.  
            return InitializeResult.ReadyToGo;              
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        #region Test Cases
        [TestMethod]
        public MFTestResults InvalidCases()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                MemoryStream ms2 = new MemoryStream();
                MemoryStreamHelper.Write(ms2, 100);
                ms2.Seek(0, SeekOrigin.Begin);
                ms2.Close();

                Log.Comment("Read from closed stream");
                try
                {
                    int readBytes = ms2.ReadByte();
                    Log.Exception( "Expected ObjectDisposedException, but read " + readBytes + " bytes" );
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException ode) { /* pass case */ Log.Comment( "Got correct exception: " + ode.Message ); }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected exception", ex);
                return MFTestResults.Fail;
            }

            return result;
        }

        [TestMethod]
        public MFTestResults VanillaCases()
        {
            MFTestResults result = MFTestResults.Pass;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    MemoryStreamHelper.Write(ms, 256);
                    ms.Position = 0;
                    Log.Comment("ReadBytes and verify");
                    for (int i = 0; i < 256; i++)
                    {
                        int b = ms.ReadByte();
                        if (b != i)
                        {
                            Log.Exception( "Expected " + i + " but got " + b );
                            return MFTestResults.Fail;
                        }
                    }

                    Log.Comment("Bytes past EOS should return -1");
                    int rb = ms.ReadByte();
                    if (rb != -1)
                    {
                        Log.Exception( "Expected -1 but got " + rb );
                        return MFTestResults.Fail;
                    }
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
                    new MFTestMethod( InvalidCases, "InvalidCases" ),
                    new MFTestMethod( VanillaCases, "VanillaCases" ),
                };
             }
        }
    }
}
