////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.Zelig.Test
{
    public class ImpenumTests : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

            // Add your functionality here.       

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests");
        }

        public override TestResult Run( string[] args )
        {
            TestResult result = TestResult.Pass;

            result |= Assert.CheckFailed( Impenum_zero_Test( ) );

            return result;
        }

        //Impenum Test methods
        //The following test was ported from folder current\test\cases\client\CLR\Conformance\10_classes\Impenum
        //zero
        //The following test cases were dropped because they had casting erros
        //IComparable001,IComparable002,IComparable003,IComparable004
        //They were skipped in the Baseline document


 
        [TestMethod]
        public TestResult Impenum_zero_Test()
        {
            Log.Comment("Tests whether 0 can be converted to various enum types...");
            if (ImpenumTestClass_zero.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        
        enum ImpenumTestClass_zero_Test1 : int
        {
            a,
            b,
            c
        }
        enum ImpenumTestClass_zero_Test2 : short
        {
            a,
            b,
            c
        }
        enum ImpenumTestClass_zero_Test3 : long
        {
            a,
            b,
            c
        }
        public class ImpenumTestClass_zero
        {
            public static bool testMethod()
            {
                ImpenumTestClass_zero_Test1 t1 = 0;
                ImpenumTestClass_zero_Test2 t2 = 0;
                ImpenumTestClass_zero_Test3 t3 = 0;
                return true;
            }
        }

    }
}

