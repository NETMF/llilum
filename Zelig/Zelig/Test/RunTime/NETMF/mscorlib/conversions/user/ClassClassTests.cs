////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.Zelig.Test
{
    public class ClassClassTests : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            Log.Comment("These tests examine the conversion (casting) of classes at various levels of inheritance trees.");
            Log.Comment("The two trees are SDer : S : SBase and TDer : T : TBase.");
            Log.Comment("The names of the tests describe the tests by listing which two classes will be converted between");
            Log.Comment("Followed by which (The source or the destination) will contain a cast definition");
            Log.Comment("Followed further by 'i's or 'e's to indicate which of the cast definition and the actual cast are");
            Log.Comment("implicit or explicit.");
            Log.Comment("");
            Log.Comment("For example, SBase_T_Source_i_e tests the conversion of SBase to T, with an implicit definition");
            Log.Comment("of the cast in the SBase class, and an explicit cast in the body of the method.");
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
            
            string testName = "ClassClass_";
            int testNumber = 0;
            result |= Assert.CheckFailed( ClassClass_SBase_TBase_Source_i_e_Test(), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_TBase_Source_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_TBase_Dest_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_TBase_Dest_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_T_Source_i_i_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_T_Source_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_T_Source_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_T_Dest_i_i_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_T_Dest_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_T_Dest_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_TDer_Source_i_i_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_TDer_Source_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SBase_TDer_Source_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TBase_Source_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TBase_Source_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TBase_Dest_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TBase_Dest_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_T_Source_i_i_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_T_Source_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_T_Source_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_T_Dest_i_i_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_T_Dest_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_T_Dest_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TDer_Source_i_i_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TDer_Source_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_S_TDer_Source_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SDer_TBase_Dest_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SDer_TBase_Dest_e_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SDer_T_Dest_i_e_Test( ), testName, ++testNumber );
            result |= Assert.CheckFailed( ClassClass_SDer_T_Dest_e_e_Test( ), testName, ++testNumber );

            return result;
        }

        //ClassClass Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\10_classes\ClassClass
        //SBase_TBase_Source_i_e,SBase_TBase_Source_e_e,SBase_TBase_Dest_i_e,SBase_TBase_Dest_e_e,SBase_T_Source_i_i,SBase_T_Source_i_e,SBase_T_Source_e_e,SBase_T_Dest_i_i,SBase_T_Dest_i_e,SBase_T_Dest_e_e,SBase_TDer_Source_i_i,SBase_TDer_Source_i_e,SBase_TDer_Source_e_e,S_TBase_Source_i_e,S_TBase_Source_e_e,S_TBase_Dest_i_e,S_TBase_Dest_e_e,S_T_Source_i_i,S_T_Source_i_e,S_T_Source_e_e,S_T_Dest_i_i,S_T_Dest_i_e,S_T_Dest_e_e,S_TDer_Source_i_i,S_TDer_Source_i_e,S_TDer_Source_e_e,SDer_TBase_Dest_i_e,SDer_TBase_Dest_e_e,SDer_T_Dest_i_e,SDer_T_Dest_e_e



        //Test Case Calls 
        [TestMethod]
        public TestResult ClassClass_SBase_TBase_Source_i_e_Test()
        {
            if (ClassClassTestClass_SBase_TBase_Source_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_TBase_Source_e_e_Test()
        {
            if (ClassClassTestClass_SBase_TBase_Source_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_TBase_Dest_i_e_Test()
        {
            if (ClassClassTestClass_SBase_TBase_Dest_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_TBase_Dest_e_e_Test()
        {
            if (ClassClassTestClass_SBase_TBase_Dest_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_T_Source_i_i_Test()
        {
            if (ClassClassTestClass_SBase_T_Source_i_i.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_T_Source_i_e_Test()
        {
            if (ClassClassTestClass_SBase_T_Source_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_T_Source_e_e_Test()
        {
            if (ClassClassTestClass_SBase_T_Source_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_T_Dest_i_i_Test()
        {
            if (ClassClassTestClass_SBase_T_Dest_i_i.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_T_Dest_i_e_Test()
        {
            if (ClassClassTestClass_SBase_T_Dest_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_T_Dest_e_e_Test()
        {
            if (ClassClassTestClass_SBase_T_Dest_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_TDer_Source_i_i_Test()
        {
            if (ClassClassTestClass_SBase_TDer_Source_i_i.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_TDer_Source_i_e_Test()
        {
            if (ClassClassTestClass_SBase_TDer_Source_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SBase_TDer_Source_e_e_Test()
        {
            if (ClassClassTestClass_SBase_TDer_Source_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TBase_Source_i_e_Test()
        {
            if (ClassClassTestClass_S_TBase_Source_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TBase_Source_e_e_Test()
        {
            if (ClassClassTestClass_S_TBase_Source_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TBase_Dest_i_e_Test()
        {
            if (ClassClassTestClass_S_TBase_Dest_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TBase_Dest_e_e_Test()
        {
            if (ClassClassTestClass_S_TBase_Dest_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_T_Source_i_i_Test()
        {
            if (ClassClassTestClass_S_T_Source_i_i.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_T_Source_i_e_Test()
        {
            if (ClassClassTestClass_S_T_Source_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_T_Source_e_e_Test()
        {
            if (ClassClassTestClass_S_T_Source_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_T_Dest_i_i_Test()
        {
            if (ClassClassTestClass_S_T_Dest_i_i.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_T_Dest_i_e_Test()
        {
            if (ClassClassTestClass_S_T_Dest_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_T_Dest_e_e_Test()
        {
            if (ClassClassTestClass_S_T_Dest_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TDer_Source_i_i_Test()
        {
            if (ClassClassTestClass_S_TDer_Source_i_i.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TDer_Source_i_e_Test()
        {
            if (ClassClassTestClass_S_TDer_Source_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_S_TDer_Source_e_e_Test()
        {
            if (ClassClassTestClass_S_TDer_Source_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SDer_TBase_Dest_i_e_Test()
        {
            if (ClassClassTestClass_SDer_TBase_Dest_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SDer_TBase_Dest_e_e_Test()
        {
            if (ClassClassTestClass_SDer_TBase_Dest_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SDer_T_Dest_i_e_Test()
        {
            if (ClassClassTestClass_SDer_T_Dest_i_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ClassClass_SDer_T_Dest_e_e_Test()
        {
            if (ClassClassTestClass_SDer_T_Dest_e_e.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        //Compiled Test Cases 
        class ClassClassTestClass_SBase_TBase_Source_i_e_SBase        {
            static public implicit operator ClassClassTestClass_SBase_TBase_Source_i_e_TBase(ClassClassTestClass_SBase_TBase_Source_i_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_TBase_Source_i_e_TBase();
            }
        }
        class ClassClassTestClass_SBase_TBase_Source_i_e_S: ClassClassTestClass_SBase_TBase_Source_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_TBase_Source_i_e_SDer: ClassClassTestClass_SBase_TBase_Source_i_e_S        {
        }
        class ClassClassTestClass_SBase_TBase_Source_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_TBase_Source_i_e_T: ClassClassTestClass_SBase_TBase_Source_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_TBase_Source_i_e_TDer: ClassClassTestClass_SBase_TBase_Source_i_e_T        {
        }
        class ClassClassTestClass_SBase_TBase_Source_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TBase_Source_i_e_S s = new ClassClassTestClass_SBase_TBase_Source_i_e_S();
                ClassClassTestClass_SBase_TBase_Source_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_TBase_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e_SBase        {
            static public explicit operator ClassClassTestClass_SBase_TBase_Source_e_e_TBase(ClassClassTestClass_SBase_TBase_Source_e_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_TBase_Source_e_e_TBase();
            }
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e_S: ClassClassTestClass_SBase_TBase_Source_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e_SDer: ClassClassTestClass_SBase_TBase_Source_e_e_S        {
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e_T: ClassClassTestClass_SBase_TBase_Source_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e_TDer: ClassClassTestClass_SBase_TBase_Source_e_e_T        {
        }
        class ClassClassTestClass_SBase_TBase_Source_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TBase_Source_e_e_S s = new ClassClassTestClass_SBase_TBase_Source_e_e_S();
                ClassClassTestClass_SBase_TBase_Source_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_TBase_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e_S: ClassClassTestClass_SBase_TBase_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e_SDer: ClassClassTestClass_SBase_TBase_Dest_i_e_S        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e_TBase        {
            static public implicit operator ClassClassTestClass_SBase_TBase_Dest_i_e_TBase(ClassClassTestClass_SBase_TBase_Dest_i_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_TBase_Dest_i_e_TBase();
            }
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e_T: ClassClassTestClass_SBase_TBase_Dest_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e_TDer: ClassClassTestClass_SBase_TBase_Dest_i_e_T        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TBase_Dest_i_e_S s = new ClassClassTestClass_SBase_TBase_Dest_i_e_S();
                ClassClassTestClass_SBase_TBase_Dest_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_TBase_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e_S: ClassClassTestClass_SBase_TBase_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e_SDer: ClassClassTestClass_SBase_TBase_Dest_e_e_S        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e_TBase        {
            static public explicit operator ClassClassTestClass_SBase_TBase_Dest_e_e_TBase(ClassClassTestClass_SBase_TBase_Dest_e_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_TBase_Dest_e_e_TBase();
            }
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e_T: ClassClassTestClass_SBase_TBase_Dest_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e_TDer: ClassClassTestClass_SBase_TBase_Dest_e_e_T        {
        }
        class ClassClassTestClass_SBase_TBase_Dest_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TBase_Dest_e_e_S s = new ClassClassTestClass_SBase_TBase_Dest_e_e_S();
                ClassClassTestClass_SBase_TBase_Dest_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_TBase_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_T_Source_i_i_SBase        {
            static public implicit operator ClassClassTestClass_SBase_T_Source_i_i_T(ClassClassTestClass_SBase_T_Source_i_i_SBase foo)
            {
                return new ClassClassTestClass_SBase_T_Source_i_i_T();
            }
        }
        class ClassClassTestClass_SBase_T_Source_i_i_S: ClassClassTestClass_SBase_T_Source_i_i_SBase        {
        }
        class ClassClassTestClass_SBase_T_Source_i_i_SDer: ClassClassTestClass_SBase_T_Source_i_i_S        {
        }
        class ClassClassTestClass_SBase_T_Source_i_i_TBase        {
        }
        class ClassClassTestClass_SBase_T_Source_i_i_T: ClassClassTestClass_SBase_T_Source_i_i_TBase        {
        }
        class ClassClassTestClass_SBase_T_Source_i_i_TDer: ClassClassTestClass_SBase_T_Source_i_i_T        {
        }
        class ClassClassTestClass_SBase_T_Source_i_i
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_T_Source_i_i_S s = new ClassClassTestClass_SBase_T_Source_i_i_S();
                ClassClassTestClass_SBase_T_Source_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_T_Source_i_e_SBase        {
            static public implicit operator ClassClassTestClass_SBase_T_Source_i_e_T(ClassClassTestClass_SBase_T_Source_i_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_T_Source_i_e_T();
            }
        }
        class ClassClassTestClass_SBase_T_Source_i_e_S: ClassClassTestClass_SBase_T_Source_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_T_Source_i_e_SDer: ClassClassTestClass_SBase_T_Source_i_e_S        {
        }
        class ClassClassTestClass_SBase_T_Source_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_T_Source_i_e_T: ClassClassTestClass_SBase_T_Source_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_T_Source_i_e_TDer: ClassClassTestClass_SBase_T_Source_i_e_T        {
        }
        class ClassClassTestClass_SBase_T_Source_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_T_Source_i_e_S s = new ClassClassTestClass_SBase_T_Source_i_e_S();
                ClassClassTestClass_SBase_T_Source_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_T_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_T_Source_e_e_SBase        {
            static public explicit operator ClassClassTestClass_SBase_T_Source_e_e_T(ClassClassTestClass_SBase_T_Source_e_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_T_Source_e_e_T();
            }
        }
        class ClassClassTestClass_SBase_T_Source_e_e_S: ClassClassTestClass_SBase_T_Source_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_T_Source_e_e_SDer: ClassClassTestClass_SBase_T_Source_e_e_S        {
        }
        class ClassClassTestClass_SBase_T_Source_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_T_Source_e_e_T: ClassClassTestClass_SBase_T_Source_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_T_Source_e_e_TDer: ClassClassTestClass_SBase_T_Source_e_e_T        {
        }
        class ClassClassTestClass_SBase_T_Source_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_T_Source_e_e_S s = new ClassClassTestClass_SBase_T_Source_e_e_S();
                ClassClassTestClass_SBase_T_Source_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_T_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_T_Dest_i_i_SBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_i_S: ClassClassTestClass_SBase_T_Dest_i_i_SBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_i_SDer: ClassClassTestClass_SBase_T_Dest_i_i_S        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_i_TBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_i_T: ClassClassTestClass_SBase_T_Dest_i_i_TBase        {
            static public implicit operator ClassClassTestClass_SBase_T_Dest_i_i_T(ClassClassTestClass_SBase_T_Dest_i_i_SBase foo)
            {
                return new ClassClassTestClass_SBase_T_Dest_i_i_T();
            }
        }
        class ClassClassTestClass_SBase_T_Dest_i_i_TDer: ClassClassTestClass_SBase_T_Dest_i_i_T        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_i
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_T_Dest_i_i_S s = new ClassClassTestClass_SBase_T_Dest_i_i_S();
                ClassClassTestClass_SBase_T_Dest_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_T_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_e_S: ClassClassTestClass_SBase_T_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_e_SDer: ClassClassTestClass_SBase_T_Dest_i_e_S        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_e_T: ClassClassTestClass_SBase_T_Dest_i_e_TBase        {
            static public implicit operator ClassClassTestClass_SBase_T_Dest_i_e_T(ClassClassTestClass_SBase_T_Dest_i_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_T_Dest_i_e_T();
            }
        }
        class ClassClassTestClass_SBase_T_Dest_i_e_TDer: ClassClassTestClass_SBase_T_Dest_i_e_T        {
        }
        class ClassClassTestClass_SBase_T_Dest_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_T_Dest_i_e_S s = new ClassClassTestClass_SBase_T_Dest_i_e_S();
                ClassClassTestClass_SBase_T_Dest_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_T_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_T_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_e_e_S: ClassClassTestClass_SBase_T_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_e_e_SDer: ClassClassTestClass_SBase_T_Dest_e_e_S        {
        }
        class ClassClassTestClass_SBase_T_Dest_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_T_Dest_e_e_T: ClassClassTestClass_SBase_T_Dest_e_e_TBase        {
            static public explicit operator ClassClassTestClass_SBase_T_Dest_e_e_T(ClassClassTestClass_SBase_T_Dest_e_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_T_Dest_e_e_T();
            }
        }
        class ClassClassTestClass_SBase_T_Dest_e_e_TDer: ClassClassTestClass_SBase_T_Dest_e_e_T        {
        }
        class ClassClassTestClass_SBase_T_Dest_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_T_Dest_e_e_S s = new ClassClassTestClass_SBase_T_Dest_e_e_S();
                ClassClassTestClass_SBase_T_Dest_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_T_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i_SBase        {
            static public implicit operator ClassClassTestClass_SBase_TDer_Source_i_i_TDer(ClassClassTestClass_SBase_TDer_Source_i_i_SBase foo)
            {
                return new ClassClassTestClass_SBase_TDer_Source_i_i_TDer();
            }
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i_S: ClassClassTestClass_SBase_TDer_Source_i_i_SBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i_SDer: ClassClassTestClass_SBase_TDer_Source_i_i_S        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i_TBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i_T: ClassClassTestClass_SBase_TDer_Source_i_i_TBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i_TDer: ClassClassTestClass_SBase_TDer_Source_i_i_T        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_i
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TDer_Source_i_i_S s= new ClassClassTestClass_SBase_TDer_Source_i_i_S();
                ClassClassTestClass_SBase_TDer_Source_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e_SBase        {
            static public implicit operator ClassClassTestClass_SBase_TDer_Source_i_e_TDer(ClassClassTestClass_SBase_TDer_Source_i_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_TDer_Source_i_e_TDer();
            }
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e_S: ClassClassTestClass_SBase_TDer_Source_i_e_SBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e_SDer: ClassClassTestClass_SBase_TDer_Source_i_e_S        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e_T: ClassClassTestClass_SBase_TDer_Source_i_e_TBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e_TDer: ClassClassTestClass_SBase_TDer_Source_i_e_T        {
        }
        class ClassClassTestClass_SBase_TDer_Source_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TDer_Source_i_e_S s = new ClassClassTestClass_SBase_TDer_Source_i_e_S();
                ClassClassTestClass_SBase_TDer_Source_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_TDer_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e_SBase        {
            static public explicit operator ClassClassTestClass_SBase_TDer_Source_e_e_TDer(ClassClassTestClass_SBase_TDer_Source_e_e_SBase foo)
            {
                return new ClassClassTestClass_SBase_TDer_Source_e_e_TDer();
            }
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e_S: ClassClassTestClass_SBase_TDer_Source_e_e_SBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e_SDer: ClassClassTestClass_SBase_TDer_Source_e_e_S        {
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e_T: ClassClassTestClass_SBase_TDer_Source_e_e_TBase        {
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e_TDer: ClassClassTestClass_SBase_TDer_Source_e_e_T        {
        }
        class ClassClassTestClass_SBase_TDer_Source_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SBase_TDer_Source_e_e_S s = new ClassClassTestClass_SBase_TDer_Source_e_e_S();
                ClassClassTestClass_SBase_TDer_Source_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SBase_TDer_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TBase_Source_i_e_SBase        {
        }
        class ClassClassTestClass_S_TBase_Source_i_e_S: ClassClassTestClass_S_TBase_Source_i_e_SBase        {
            static public implicit operator ClassClassTestClass_S_TBase_Source_i_e_TBase(ClassClassTestClass_S_TBase_Source_i_e_S foo)
            {
                return new ClassClassTestClass_S_TBase_Source_i_e_TBase();
            }
        }
        class ClassClassTestClass_S_TBase_Source_i_e_SDer: ClassClassTestClass_S_TBase_Source_i_e_S        {
        }
        class ClassClassTestClass_S_TBase_Source_i_e_TBase        {
        }
        class ClassClassTestClass_S_TBase_Source_i_e_T: ClassClassTestClass_S_TBase_Source_i_e_TBase        {
        }
        class ClassClassTestClass_S_TBase_Source_i_e_TDer: ClassClassTestClass_S_TBase_Source_i_e_T        {
        }
        class ClassClassTestClass_S_TBase_Source_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TBase_Source_i_e_S s = new ClassClassTestClass_S_TBase_Source_i_e_S();
                ClassClassTestClass_S_TBase_Source_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_TBase_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TBase_Source_e_e_SBase        {
        }
        class ClassClassTestClass_S_TBase_Source_e_e_S: ClassClassTestClass_S_TBase_Source_e_e_SBase        {
            static public explicit operator ClassClassTestClass_S_TBase_Source_e_e_TBase(ClassClassTestClass_S_TBase_Source_e_e_S foo)
            {
                return new ClassClassTestClass_S_TBase_Source_e_e_TBase();
            }
        }
        class ClassClassTestClass_S_TBase_Source_e_e_SDer: ClassClassTestClass_S_TBase_Source_e_e_S        {
        }
        class ClassClassTestClass_S_TBase_Source_e_e_TBase        {
        }
        class ClassClassTestClass_S_TBase_Source_e_e_T: ClassClassTestClass_S_TBase_Source_e_e_TBase        {
        }
        class ClassClassTestClass_S_TBase_Source_e_e_TDer: ClassClassTestClass_S_TBase_Source_e_e_T        {
        }
        class ClassClassTestClass_S_TBase_Source_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TBase_Source_e_e_S s = new ClassClassTestClass_S_TBase_Source_e_e_S();
                ClassClassTestClass_S_TBase_Source_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_TBase_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TBase_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_S_TBase_Dest_i_e_S: ClassClassTestClass_S_TBase_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_S_TBase_Dest_i_e_SDer: ClassClassTestClass_S_TBase_Dest_i_e_S        {
        }
        class ClassClassTestClass_S_TBase_Dest_i_e_TBase        {
            static public implicit operator ClassClassTestClass_S_TBase_Dest_i_e_TBase(ClassClassTestClass_S_TBase_Dest_i_e_S foo)
            {
                return new ClassClassTestClass_S_TBase_Dest_i_e_TBase();
            }
        }
        class ClassClassTestClass_S_TBase_Dest_i_e_T: ClassClassTestClass_S_TBase_Dest_i_e_TBase        {
        }
        class ClassClassTestClass_S_TBase_Dest_i_e_TDer: ClassClassTestClass_S_TBase_Dest_i_e_T        {
        }
        class ClassClassTestClass_S_TBase_Dest_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TBase_Dest_i_e_S s = new ClassClassTestClass_S_TBase_Dest_i_e_S();
                ClassClassTestClass_S_TBase_Dest_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_TBase_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TBase_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_S_TBase_Dest_e_e_S: ClassClassTestClass_S_TBase_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_S_TBase_Dest_e_e_SDer: ClassClassTestClass_S_TBase_Dest_e_e_S        {
        }
        class ClassClassTestClass_S_TBase_Dest_e_e_TBase        {
            static public explicit operator ClassClassTestClass_S_TBase_Dest_e_e_TBase(ClassClassTestClass_S_TBase_Dest_e_e_S foo)
            {
                return new ClassClassTestClass_S_TBase_Dest_e_e_TBase();
            }
        }
        class ClassClassTestClass_S_TBase_Dest_e_e_T: ClassClassTestClass_S_TBase_Dest_e_e_TBase        {
        }
        class ClassClassTestClass_S_TBase_Dest_e_e_TDer: ClassClassTestClass_S_TBase_Dest_e_e_T        {
        }
        class ClassClassTestClass_S_TBase_Dest_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TBase_Dest_e_e_S s = new ClassClassTestClass_S_TBase_Dest_e_e_S();
                ClassClassTestClass_S_TBase_Dest_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_TBase_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_T_Source_i_i_SBase        {
        }
        class ClassClassTestClass_S_T_Source_i_i_S: ClassClassTestClass_S_T_Source_i_i_SBase        {
            static public implicit operator ClassClassTestClass_S_T_Source_i_i_T(ClassClassTestClass_S_T_Source_i_i_S foo)
            {
                return new ClassClassTestClass_S_T_Source_i_i_T();
            }
        }
        class ClassClassTestClass_S_T_Source_i_i_SDer: ClassClassTestClass_S_T_Source_i_i_S        {
        }
        class ClassClassTestClass_S_T_Source_i_i_TBase        {
        }
        class ClassClassTestClass_S_T_Source_i_i_T: ClassClassTestClass_S_T_Source_i_i_TBase        {
        }
        class ClassClassTestClass_S_T_Source_i_i_TDer: ClassClassTestClass_S_T_Source_i_i_T        {
        }
        class ClassClassTestClass_S_T_Source_i_i
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_T_Source_i_i_S s = new ClassClassTestClass_S_T_Source_i_i_S();
                ClassClassTestClass_S_T_Source_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_T_Source_i_e_SBase        {
        }
        class ClassClassTestClass_S_T_Source_i_e_S: ClassClassTestClass_S_T_Source_i_e_SBase        {
            static public implicit operator ClassClassTestClass_S_T_Source_i_e_T(ClassClassTestClass_S_T_Source_i_e_S foo)
            {
                return new ClassClassTestClass_S_T_Source_i_e_T();
            }
        }
        class ClassClassTestClass_S_T_Source_i_e_SDer: ClassClassTestClass_S_T_Source_i_e_S        {
        }
        class ClassClassTestClass_S_T_Source_i_e_TBase        {
        }
        class ClassClassTestClass_S_T_Source_i_e_T: ClassClassTestClass_S_T_Source_i_e_TBase        {
        }
        class ClassClassTestClass_S_T_Source_i_e_TDer: ClassClassTestClass_S_T_Source_i_e_T        {
        }
        class ClassClassTestClass_S_T_Source_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_T_Source_i_e_S s = new ClassClassTestClass_S_T_Source_i_e_S();
                ClassClassTestClass_S_T_Source_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_T_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_T_Source_e_e_SBase        {
        }
        class ClassClassTestClass_S_T_Source_e_e_S: ClassClassTestClass_S_T_Source_e_e_SBase        {
            static public explicit operator ClassClassTestClass_S_T_Source_e_e_T(ClassClassTestClass_S_T_Source_e_e_S foo)
            {
                return new ClassClassTestClass_S_T_Source_e_e_T();
            }
        }
        class ClassClassTestClass_S_T_Source_e_e_SDer: ClassClassTestClass_S_T_Source_e_e_S        {
        }
        class ClassClassTestClass_S_T_Source_e_e_TBase        {
        }
        class ClassClassTestClass_S_T_Source_e_e_T: ClassClassTestClass_S_T_Source_e_e_TBase        {
        }
        class ClassClassTestClass_S_T_Source_e_e_TDer: ClassClassTestClass_S_T_Source_e_e_T        {
        }
        class ClassClassTestClass_S_T_Source_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_T_Source_e_e_S s = new ClassClassTestClass_S_T_Source_e_e_S();
                ClassClassTestClass_S_T_Source_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_T_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_T_Dest_i_i_SBase        {
        }
        class ClassClassTestClass_S_T_Dest_i_i_S: ClassClassTestClass_S_T_Dest_i_i_SBase        {
        }
        class ClassClassTestClass_S_T_Dest_i_i_SDer: ClassClassTestClass_S_T_Dest_i_i_S        {
        }
        class ClassClassTestClass_S_T_Dest_i_i_TBase        {
        }
        class ClassClassTestClass_S_T_Dest_i_i_T: ClassClassTestClass_S_T_Dest_i_i_TBase        {
            static public implicit operator ClassClassTestClass_S_T_Dest_i_i_T(ClassClassTestClass_S_T_Dest_i_i_S foo)
            {
                return new ClassClassTestClass_S_T_Dest_i_i_T();
            }
        }
        class ClassClassTestClass_S_T_Dest_i_i_TDer: ClassClassTestClass_S_T_Dest_i_i_T        {
        }
        class ClassClassTestClass_S_T_Dest_i_i
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_T_Dest_i_i_S s = new ClassClassTestClass_S_T_Dest_i_i_S();
                ClassClassTestClass_S_T_Dest_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_T_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_S_T_Dest_i_e_S: ClassClassTestClass_S_T_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_S_T_Dest_i_e_SDer: ClassClassTestClass_S_T_Dest_i_e_S        {
        }
        class ClassClassTestClass_S_T_Dest_i_e_TBase        {
        }
        class ClassClassTestClass_S_T_Dest_i_e_T: ClassClassTestClass_S_T_Dest_i_e_TBase        {
            static public implicit operator ClassClassTestClass_S_T_Dest_i_e_T(ClassClassTestClass_S_T_Dest_i_e_S foo)
            {
                return new ClassClassTestClass_S_T_Dest_i_e_T();
            }
        }
        class ClassClassTestClass_S_T_Dest_i_e_TDer: ClassClassTestClass_S_T_Dest_i_e_T        {
        }
        class ClassClassTestClass_S_T_Dest_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_T_Dest_i_e_S s = new ClassClassTestClass_S_T_Dest_i_e_S();
                ClassClassTestClass_S_T_Dest_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_T_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_T_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_S_T_Dest_e_e_S: ClassClassTestClass_S_T_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_S_T_Dest_e_e_SDer: ClassClassTestClass_S_T_Dest_e_e_S        {
        }
        class ClassClassTestClass_S_T_Dest_e_e_TBase        {
        }
        class ClassClassTestClass_S_T_Dest_e_e_T: ClassClassTestClass_S_T_Dest_e_e_TBase        {
            static public explicit operator ClassClassTestClass_S_T_Dest_e_e_T(ClassClassTestClass_S_T_Dest_e_e_S foo)
            {
                return new ClassClassTestClass_S_T_Dest_e_e_T();
            }
        }
        class ClassClassTestClass_S_T_Dest_e_e_TDer: ClassClassTestClass_S_T_Dest_e_e_T        {
        }
        class ClassClassTestClass_S_T_Dest_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_T_Dest_e_e_S s = new ClassClassTestClass_S_T_Dest_e_e_S();
                ClassClassTestClass_S_T_Dest_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_T_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TDer_Source_i_i_SBase        {
        }
        class ClassClassTestClass_S_TDer_Source_i_i_S: ClassClassTestClass_S_TDer_Source_i_i_SBase        {
            static public implicit operator ClassClassTestClass_S_TDer_Source_i_i_TDer(ClassClassTestClass_S_TDer_Source_i_i_S foo)
            {
                return new ClassClassTestClass_S_TDer_Source_i_i_TDer();
            }
        }
        class ClassClassTestClass_S_TDer_Source_i_i_SDer: ClassClassTestClass_S_TDer_Source_i_i_S        {
        }
        class ClassClassTestClass_S_TDer_Source_i_i_TBase        {
        }
        class ClassClassTestClass_S_TDer_Source_i_i_T: ClassClassTestClass_S_TDer_Source_i_i_TBase        {
        }
        class ClassClassTestClass_S_TDer_Source_i_i_TDer: ClassClassTestClass_S_TDer_Source_i_i_T        {
        }
        class ClassClassTestClass_S_TDer_Source_i_i
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TDer_Source_i_i_S s = new ClassClassTestClass_S_TDer_Source_i_i_S();
                ClassClassTestClass_S_TDer_Source_i_i_T t;
                try
                {
                    t = s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TDer_Source_i_e_SBase        {
        }
        class ClassClassTestClass_S_TDer_Source_i_e_S: ClassClassTestClass_S_TDer_Source_i_e_SBase        {
            static public implicit operator ClassClassTestClass_S_TDer_Source_i_e_TDer(ClassClassTestClass_S_TDer_Source_i_e_S foo)
            {
                return new ClassClassTestClass_S_TDer_Source_i_e_TDer();
            }
        }
        class ClassClassTestClass_S_TDer_Source_i_e_SDer: ClassClassTestClass_S_TDer_Source_i_e_S        {
        }
        class ClassClassTestClass_S_TDer_Source_i_e_TBase        {
        }
        class ClassClassTestClass_S_TDer_Source_i_e_T: ClassClassTestClass_S_TDer_Source_i_e_TBase        {
        }
        class ClassClassTestClass_S_TDer_Source_i_e_TDer: ClassClassTestClass_S_TDer_Source_i_e_T        {
        }
        class ClassClassTestClass_S_TDer_Source_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TDer_Source_i_e_S s = new ClassClassTestClass_S_TDer_Source_i_e_S();
                ClassClassTestClass_S_TDer_Source_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_TDer_Source_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_S_TDer_Source_e_e_SBase        {
        }
        class ClassClassTestClass_S_TDer_Source_e_e_S: ClassClassTestClass_S_TDer_Source_e_e_SBase        {
            static public explicit operator ClassClassTestClass_S_TDer_Source_e_e_TDer(ClassClassTestClass_S_TDer_Source_e_e_S foo)
            {
                return new ClassClassTestClass_S_TDer_Source_e_e_TDer();
            }
        }
        class ClassClassTestClass_S_TDer_Source_e_e_SDer: ClassClassTestClass_S_TDer_Source_e_e_S        {
        }
        class ClassClassTestClass_S_TDer_Source_e_e_TBase        {
        }
        class ClassClassTestClass_S_TDer_Source_e_e_T: ClassClassTestClass_S_TDer_Source_e_e_TBase        {
        }
        class ClassClassTestClass_S_TDer_Source_e_e_TDer: ClassClassTestClass_S_TDer_Source_e_e_T        {
        }
        class ClassClassTestClass_S_TDer_Source_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_S_TDer_Source_e_e_S s = new ClassClassTestClass_S_TDer_Source_e_e_S();
                ClassClassTestClass_S_TDer_Source_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_S_TDer_Source_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e_S: ClassClassTestClass_SDer_TBase_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e_SDer: ClassClassTestClass_SDer_TBase_Dest_i_e_S        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e_TBase        {
            static public implicit operator ClassClassTestClass_SDer_TBase_Dest_i_e_TBase(ClassClassTestClass_SDer_TBase_Dest_i_e_SDer foo)
            {
                return new ClassClassTestClass_SDer_TBase_Dest_i_e_TBase();
            }
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e_T: ClassClassTestClass_SDer_TBase_Dest_i_e_TBase        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e_TDer: ClassClassTestClass_SDer_TBase_Dest_i_e_T        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SDer_TBase_Dest_i_e_S s = new ClassClassTestClass_SDer_TBase_Dest_i_e_S();
                ClassClassTestClass_SDer_TBase_Dest_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SDer_TBase_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e_S: ClassClassTestClass_SDer_TBase_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e_SDer: ClassClassTestClass_SDer_TBase_Dest_e_e_S        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e_TBase        {
            static public explicit operator ClassClassTestClass_SDer_TBase_Dest_e_e_TBase(ClassClassTestClass_SDer_TBase_Dest_e_e_SDer foo)
            {
                return new ClassClassTestClass_SDer_TBase_Dest_e_e_TBase();
            }
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e_T: ClassClassTestClass_SDer_TBase_Dest_e_e_TBase        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e_TDer: ClassClassTestClass_SDer_TBase_Dest_e_e_T        {
        }
        class ClassClassTestClass_SDer_TBase_Dest_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SDer_TBase_Dest_e_e_S s = new ClassClassTestClass_SDer_TBase_Dest_e_e_S();
                ClassClassTestClass_SDer_TBase_Dest_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SDer_TBase_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SDer_T_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SDer_T_Dest_i_e_S: ClassClassTestClass_SDer_T_Dest_i_e_SBase        {
        }
        class ClassClassTestClass_SDer_T_Dest_i_e_SDer: ClassClassTestClass_SDer_T_Dest_i_e_S        {
        }
        class ClassClassTestClass_SDer_T_Dest_i_e_TBase        {
        }
        class ClassClassTestClass_SDer_T_Dest_i_e_T: ClassClassTestClass_SDer_T_Dest_i_e_TBase        {
            static public implicit operator ClassClassTestClass_SDer_T_Dest_i_e_T(ClassClassTestClass_SDer_T_Dest_i_e_SDer foo)
            {
                return new ClassClassTestClass_SDer_T_Dest_i_e_T();
            }
        }
        class ClassClassTestClass_SDer_T_Dest_i_e_TDer: ClassClassTestClass_SDer_T_Dest_i_e_T        {
        }
        class ClassClassTestClass_SDer_T_Dest_i_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SDer_T_Dest_i_e_S s = new ClassClassTestClass_SDer_T_Dest_i_e_S();
                ClassClassTestClass_SDer_T_Dest_i_e_T t;
                try
                {
                    t = (ClassClassTestClass_SDer_T_Dest_i_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
        class ClassClassTestClass_SDer_T_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SDer_T_Dest_e_e_S: ClassClassTestClass_SDer_T_Dest_e_e_SBase        {
        }
        class ClassClassTestClass_SDer_T_Dest_e_e_SDer: ClassClassTestClass_SDer_T_Dest_e_e_S        {
        }
        class ClassClassTestClass_SDer_T_Dest_e_e_TBase        {
        }
        class ClassClassTestClass_SDer_T_Dest_e_e_T: ClassClassTestClass_SDer_T_Dest_e_e_TBase        {
            static public explicit operator ClassClassTestClass_SDer_T_Dest_e_e_T(ClassClassTestClass_SDer_T_Dest_e_e_SDer foo)
            {
                return new ClassClassTestClass_SDer_T_Dest_e_e_T();
            }
        }
        class ClassClassTestClass_SDer_T_Dest_e_e_TDer: ClassClassTestClass_SDer_T_Dest_e_e_T        {
        }
        class ClassClassTestClass_SDer_T_Dest_e_e
        {
            public static bool testMethod()
            {
                ClassClassTestClass_SDer_T_Dest_e_e_S s = new ClassClassTestClass_SDer_T_Dest_e_e_S();
                ClassClassTestClass_SDer_T_Dest_e_e_T t;
                try
                {
                    t = (ClassClassTestClass_SDer_T_Dest_e_e_T)s;
                }
                catch (System.Exception)
                {
                    Log.Comment("System.Exception caught");
                }
                return true;
            }
        }
    }
}
