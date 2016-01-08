////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;


using System.Net;

namespace Microsoft.Zelig.Test
{
    public class HttpWebResponseTests : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        public override TestResult Run( string[] args )
        {
            return TestResult.Pass;
        }
        
        //--//
        //--//
        //--//


        [TestMethod]
        public TestResult NotImplemented()
        {
            // TODO: Add your tests here.

            return TestResult.Skip;
        }
    }
}
