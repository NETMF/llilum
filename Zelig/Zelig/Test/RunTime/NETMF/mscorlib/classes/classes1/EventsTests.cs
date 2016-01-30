////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.Zelig.Test;

namespace Microsoft.Zelig.Test
{
    public class EventsTests : TestBase, ITestInterface
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
            Log.Comment("Cleaning up after the tests");
        }
        
        public override TestResult Run( string[] args )
        {
            TestResult result = TestResult.Pass;
            
            result |= Assert.CheckFailed( Events1_Test( ) );

            return result;
        }

        //--//
        //--//
        //--//

        //Events Test methods
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\Events
        //The following tests were removed because they were build failure tests:
        //2-9,12-41

        [TestMethod]
        public TestResult Events1_Test()
        {
            Log.Comment("This is testing an obsolete event structure, but should pass.");
            if (EventsTestClass1.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }


        public delegate void EventsTestClass1_EventHandler1();

        public class EventsTestClass1_Event1
        {

            [Obsolete("This is Obsolete")]
            public event EventsTestClass1_EventHandler1 CMyEvent;

            public void Fire()
            {
                if (CMyEvent != null) CMyEvent();
            }
        }

        public class EventsTestClass1_Sub1
        {


            public static void MyMeth(){}

            public static void Main_old()
            {
                EventsTestClass1_Event1 mc = new EventsTestClass1_Event1();
                mc.CMyEvent += new EventsTestClass1_EventHandler1(MyMeth);
                mc.Fire();
            }
        }

        class EventsTestClass1
        {
            public static bool testMethod()
            {
                try
                {
                    EventsTestClass1_Sub1.Main_old();
                }
                catch
                {
                    return false;
                }
                return true;
                   
            }
        }


    }
}
