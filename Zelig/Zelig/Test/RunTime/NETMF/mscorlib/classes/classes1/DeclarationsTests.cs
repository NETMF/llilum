////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.Zelig.Test;

namespace Microsoft.Zelig.Test
{
    public class DeclarationsTests : TestBase, ITestInterface
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

        public override TestResult Run(string[] args)
        {
            TestResult result = TestResult.Pass;

            result |= Assert.CheckFailed(Meta_Test(), "Meta", 0);

            string testName = "BaseClass";
            result |= Assert.CheckFailed(BaseClass1_Test(), testName, 1);
            result |= Assert.CheckFailed(BaseClass2_Test(), testName, 2);
            result |= Assert.CheckFailed(BaseClass3_Test(), testName, 3);
            result |= Assert.CheckFailed(BaseClass4_Test(), testName, 4);
            result |= Assert.CheckFailed(BaseClass10_Test(), testName, 10);
            result |= Assert.CheckFailed(BaseClass13_Test(), testName, 13);
            result |= Assert.CheckFailed(BaseClass25_Test(), testName, 25);
            result |= Assert.CheckFailed(BaseClass29_Test(), testName, 29);

            testName = "Modifiers";
            result |= Assert.CheckFailed(Modifiers2_Test(), testName, 2);
            result |= Assert.CheckFailed(Modifiers3_Test(), testName, 3);
            result |= Assert.CheckFailed(Modifiers4_Test(), testName, 4);
            result |= Assert.CheckFailed(Modifiers6_Test(), testName, 6);
            result |= Assert.CheckFailed(Modifiers7_Test(), testName, 7);
            result |= Assert.CheckFailed(Modifiers8_Test(), testName, 8);
            result |= Assert.CheckFailed(Modifiers10_Test(), testName, 10);
            result |= Assert.CheckFailed(Modifiers11_Test(), testName, 11);
            result |= Assert.CheckFailed(Modifiers12_Test(), testName, 12);
            result |= Assert.CheckFailed(Modifiers13_Test(), testName, 13);
            result |= Assert.CheckFailed(Modifiers14_Test(), testName, 14);
            result |= Assert.CheckFailed(Modifiers23_Test(), testName, 23);
            result |= Assert.CheckFailed(Modifiers24_Test(), testName, 24);
            result |= Assert.CheckFailed(Modifiers25_Test(), testName, 25);
            result |= Assert.CheckFailed(Modifiers26_Test(), testName, 26);
            result |= Assert.CheckFailed(Modifiers31_Test(), testName, 31);

            return result;
        }

        //--//
        //--//
        //--//

        [TestMethod]
        public TestResult Meta_Test()
        {
            return TestResult.Pass;
        }

        //Delcarations Tests
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\Declartations
        //The BaseClass*_Test() functions are ported from Declarations\basclass\basec*.cs files
        //The Modifiers*_Test() functions are ported from Declarations\basclass\mod*.cs files
        //The following tests were removed because they were build failure tests:
        //BaseClass: 5-9,11,12,14-24,26-28
        //Modifiers: 1,5,9,15-22,27-30,32

        [TestMethod]
        public TestResult BaseClass1_Test()
        {
            Log.Comment("Tests an int declaration with assignment in a base class");
            if (BaseClassTestClass1.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult BaseClass2_Test()
        {

            Log.Comment("Tests a function declaration in a implementing class still");
            Log.Comment("works after child is cast as an implemented interface");
            if (BaseClassTestClass2.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult BaseClass3_Test()
        {

            Log.Comment("Tests a function declaration in an implementing class still works after child is cast as");
            Log.Comment("each of two implemented interfaces");
            if (BaseClassTestClass3.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;

        }

        [TestMethod]
        public TestResult BaseClass4_Test()
        {
            Log.Comment("Tests a function declaration in a child class still works after child is cast as");
            Log.Comment("its parent class and an interface it implements");
            if (BaseClassTestClass4.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult BaseClass10_Test()
        {
            Log.Comment("Section 10.1");
            Log.Comment("The base classes of a class are the direct base");
            Log.Comment("class and its base classes.  In other words, the");
            Log.Comment("set of base classes is the transitive closure of the ");
            Log.Comment("direct base class relatationship.");
            if (BaseClassTestClass10.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        
        [TestMethod]
        public TestResult BaseClass13_Test()
        {
            Log.Comment("Section 10.1");
            Log.Comment("Note that a class does not depend on the");
            Log.Comment("classes that are nested within it. ");
            if (BaseClassTestClass13.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult BaseClass25_Test()
        {
            Log.Comment("10.1.2.1 ");
            Log.Comment("inheriting from nested types");
            if (BaseClassTestClass25.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult BaseClass29_Test()
        {
            Log.Comment("10.1.2.1 ");
            Log.Comment("inheriting from nested types");
            
            if (BaseClassTestClass29.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers2_Test()
        {
            Log.Comment("Testing  a public int inside an inner class with modifier 'new' ");
            
            if (ModifiersTestClass2.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers3_Test()
        {

            Log.Comment("Testing  a public int directly inside a public class");
            if (ModifiersTestClass3.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;

        }

        [TestMethod]
        public TestResult Modifiers4_Test()
        {

            Log.Comment("Testing  a public int inside an inner class with modifier 'public' ");
            if (ModifiersTestClass4.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers6_Test()
        {

            Log.Comment("Testing  a public int inside an inner class with modifier 'protected' ");
            if (ModifiersTestClass6.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers7_Test()
        {

            Log.Comment("Testing  a public int directly inside an internal class");
            if (ModifiersTestClass7.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers8_Test()
        {
            Log.Comment("Testing  a public int inside an inner class with modifier 'internal' ");
            if (ModifiersTestClass8.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers10_Test()
        {

            Log.Comment("Testing  a public int inside an inner class with modifier 'private' ");
            if (ModifiersTestClass10.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers11_Test()
        {

            Log.Comment("Testing  a public int inside an abstract class that is implemented");
            if (ModifiersTestClass11.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers12_Test()
        {

            Log.Comment("Testing  a public int inside an inner abstract class that is implemented");
            if (ModifiersTestClass12.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers13_Test()
        {
            Log.Comment("Testing  a public int directly inside a sealed class");
            
            if (ModifiersTestClass13.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers14_Test()
        {

            Log.Comment("Testing  a public int inside an inner sealed class");
            if (ModifiersTestClass14.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers23_Test()
        {
            Log.Comment("An abstract class cannot be instantiated, and it is");
            Log.Comment("an error to use the new operator on an abstract class.");
            Log.Comment("While it is possible to have variables and values whose");
            Log.Comment("compile-time types are abstract, such variables and values");
            Log.Comment("will necessarily either be null or contain references");
            Log.Comment("to instances of non-abstract classes derived from the ");
            Log.Comment("abstract types.");
            if (ModifiersTestClass23.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers24_Test()
        {
            Log.Comment("An abstract class cannot be instantiated, and it is");
            Log.Comment("an error to use the new operator on an abstract class.");
            Log.Comment("While it is possible to have variables and values whose");
            Log.Comment("compile-time types are abstract, such variables and values");
            Log.Comment("will necessarily either be null or contain references");
            Log.Comment("to instances of non-abstract classes derived from the ");
            Log.Comment("abstract types.");
            if (ModifiersTestClass24.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers25_Test()
        {
            Log.Comment("Section 10.1");
            Log.Comment("An abstract class is permitted (but not required)");
            Log.Comment("to contain abstract methods and accessors.");
            if (ModifiersTestClass25.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers26_Test()
        {
            Log.Comment("Section 10.1");
            Log.Comment("An abstract class is permitted (but not required)");
            Log.Comment("to contain abstract methods and accessors.");
            if (ModifiersTestClass26.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Modifiers31_Test()
        {

            Log.Comment("Section 10.1");
            Log.Comment("When a non-abstract class is derived from");
            Log.Comment("an abstract class, the non-abstract class must");
            Log.Comment("be include actual implementations of all inherited ");
            Log.Comment("abstract methods and accessors.  Such implementations");
            Log.Comment("are provided by overriding the abstract methods");
            Log.Comment("and accessors.");
            if (ModifiersTestClass31.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        class BaseClassTestClass1_Base
        {
            public int intI = 2;
        }

        class BaseClassTestClass1 : BaseClassTestClass1_Base
        {
            public static bool testMethod()
            {
                BaseClassTestClass1 MC = new BaseClassTestClass1();
                if (MC.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        interface BaseClassTestClass2_Base
        {
            int RetInt();
        }

        class BaseClassTestClass2 : BaseClassTestClass2_Base
        {

            public int RetInt()
            {
                return 2;
            }

            public static bool testMethod()
            {
                BaseClassTestClass2 MC = new BaseClassTestClass2();
                BaseClassTestClass2_Base Test = (BaseClassTestClass2_Base)MC;
                if (Test.RetInt() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        interface BaseClassTestClass3_Base1
        {
            int RetInt();
        }

        interface BaseClassTestClass3_Base2
        {
            int RetInt2();
        }

        class BaseClassTestClass3 : BaseClassTestClass3_Base1, BaseClassTestClass3_Base2
        {

            public int RetInt()
            {
                return 2;
            }

            public int RetInt2()
            {
                return 3;
            }

            public static bool testMethod()
            {
                BaseClassTestClass3 MC = new BaseClassTestClass3();
                BaseClassTestClass3_Base1 Test1 = (BaseClassTestClass3_Base1)MC;
                BaseClassTestClass3_Base2 Test2 = (BaseClassTestClass3_Base2)MC;
                if ((Test1.RetInt() == 2) && (Test2.RetInt2() == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        class BaseClassTestClass4_Base1
        {
            public int RetInt()
            {
                return 2;
            }
        }

        interface BaseClassTestClass4_Base2
        {
            int RetInt2();
        }

        class BaseClassTestClass4 : BaseClassTestClass4_Base1, BaseClassTestClass4_Base2
        {

            public int RetInt2()
            {
                return 3;
            }

            public static bool testMethod()
            {
                BaseClassTestClass4 MC = new BaseClassTestClass4();
                BaseClassTestClass4_Base1 Test1 = (BaseClassTestClass4_Base1)MC;
                BaseClassTestClass4_Base2 Test2 = (BaseClassTestClass4_Base2)MC;
                if ((Test1.RetInt() == 2) && (Test2.RetInt2() == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        class BaseClassTestClass10_Base1
        {
            public int intI = 2;
        }

        class BaseClassTestClass10_Base2 : BaseClassTestClass10_Base1
        {
            public int intJ = 3;
        }

        class BaseClassTestClass10 : BaseClassTestClass10_Base2
        {
            public static bool testMethod()
            {
                BaseClassTestClass10 MC = new BaseClassTestClass10();
                if ((MC.intI == 2) && (MC.intJ == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class BaseClassTestClass13
        {
            class BaseClassTestClass13_Sub : BaseClassTestClass13
            {
                public int intI = 2;
            }

            public static bool testMethod()
            {
                BaseClassTestClass13_Sub test = new BaseClassTestClass13_Sub();
                if (test.intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class BaseClassTestClass25_Base1
        {
            public class BaseClassTestClass25_Sub1 : BaseClassTestClass25_Base1 { }
            public interface BaseClassTestClass25_Interface1 { }
            public class BaseClassTestClass25_Sub2 : BaseClassTestClass25_Sub1, BaseClassTestClass25_Interface1
            { }
        }

        class BaseClassTestClass25 : BaseClassTestClass25_Base1.BaseClassTestClass25_Sub2, BaseClassTestClass25_Base1.BaseClassTestClass25_Interface1
        {
            public static bool testMethod()
            {
                BaseClassTestClass25 m = new BaseClassTestClass25();
                if ((BaseClassTestClass25_Base1.BaseClassTestClass25_Sub2)m == null)
                    return false;

                if ((BaseClassTestClass25_Base1.BaseClassTestClass25_Interface1)m == null)
                    return false;
                return true;
            }
        }

        public class BaseClassTestClass29_SubC : BaseClassTestClass29_SubA.BaseClassTestClass29_SubB{}
        class BaseClassTestClass29_SubG : BaseClassTestClass29_SubC{}
        class BaseClassTestClass29_SubGG : BaseClassTestClass29_SubA.D{}
        class BaseClassTestClass29_SubGGG : BaseClassTestClass29_SubD, BaseClassTestClass29_SubA.BaseClassTestClass29_SubC.II, BaseClassTestClass29_SubA.BaseClassTestClass29_SubB.I { }

        public class BaseClassTestClass29_SubA
        {
	        public class BaseClassTestClass29_SubB : BaseClassTestClass29_SubC.I
	        {
		        public interface I{}	
	        }
	        public class BaseClassTestClass29_SubC
	        {
		        public interface I{}
		        public interface II : I {}
	        }

	        public class D : BaseClassTestClass29_SubC.II
	        {
		        public class DD : D, BaseClassTestClass29_SubC.I, BaseClassTestClass29_SubC.II
		        {
			        public interface I : BaseClassTestClass29_SubC.II{}
		        }
	        }
        }

        class BaseClassTestClass29_SubD : BaseClassTestClass29_SubC{}
        class BaseClassTestClass29_SubDD : BaseClassTestClass29_SubA.D{}
        class BaseClassTestClass29_SubDDD : BaseClassTestClass29_SubD, BaseClassTestClass29_SubA.BaseClassTestClass29_SubC.II, BaseClassTestClass29_SubA.BaseClassTestClass29_SubB.I{}

        class BaseClassTestClass29
        {
            public static bool testMethod()
            {
                return true;
            }
        }


        class ModifiersTestClass2_Base1
        {
            public class Inner
            {
                public int IntI = 1;
            }
        }

        class ModifiersTestClass2 : ModifiersTestClass2_Base1
        {

            // new
            new class Inner
            {
                public int IntI = 2;
            }

            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class ModifiersTestClass3
        {

            public int IntI = 2;

            public static bool testMethod()
            {
                ModifiersTestClass3 Test = new ModifiersTestClass3();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ModifiersTestClass4
        {

            //public 
            public class Inner
            {
                public int IntI = 2;
            }

            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ModifiersTestClass6
        {
            protected class Inner
            {
                public int IntI = 2;
            }
            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal class ModifiersTestClass7
        {

            public int IntI = 2;

            public static bool testMethod()
            {
                ModifiersTestClass7 Test = new ModifiersTestClass7();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ModifiersTestClass8
        {

            // internal
            internal class Inner
            {
                public int IntI = 2;
            }

            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ModifiersTestClass10
        {

            // private
            private class Inner
            {
                public int IntI = 2;
            }

            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public abstract class ModifiersTestClass11_Base
        {
            public int IntI = 2;
        }

        public class ModifiersTestClass11 : ModifiersTestClass11_Base
        {
            public static bool testMethod()
            {
                ModifiersTestClass11 Test = new ModifiersTestClass11();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class ModifiersTestClass12
        {

            // abstract
            abstract class BaseClass
            {
                public int IntI = 2;
            }

            class Inner : BaseClass { }

            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        sealed class ModifiersTestClass13
        {

            public int IntI = 2;

            public static bool testMethod()
            {
                ModifiersTestClass13 Test = new ModifiersTestClass13();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ModifiersTestClass14
        {

            //sealed 
            sealed class Inner
            {
                public int IntI = 2;
            }

            public static bool testMethod()
            {
                Inner Test = new Inner();
                if (Test.IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        abstract class ModifiersTestClass23_Sub1 { }

        class ModifiersTestClass23
        {

            ModifiersTestClass23_Sub1 A;

            public static bool testMethod()
            {
                ModifiersTestClass23 test = new ModifiersTestClass23();
                if (test.A == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        abstract class ModifiersTestClass24_Abstract1
        {
            public abstract int retInt();
        }

        class ModifiersTestClass24_Derived1 : ModifiersTestClass24_Abstract1
        {
            override public int retInt()
            {
                return 2;
            }
        }

        class ModifiersTestClass24
        {

            ModifiersTestClass24_Abstract1 A = new ModifiersTestClass24_Derived1();

            public static bool testMethod()
            {

                ModifiersTestClass24 test = new ModifiersTestClass24();

                if (test.A.retInt() == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        abstract class ModifiersTestClass25_Abstract1 { }

        class ModifiersTestClass25 : ModifiersTestClass25_Abstract1
        {
            public static bool testMethod()
            {
                return true;
            }
        }

        abstract class ModifiersTestClass26_Abstract1
        {
            public abstract void foo();
            public abstract int intI
            {
                get;
                set;
            }
        }

        class ModifiersTestClass26
        {
            public static bool testMethod()
            {
                return true;
            }
        }

        abstract class ModifiersTestClass31_SubA
        {
            public abstract void F();
        }

        abstract class ModifiersTestClass31_SubB : ModifiersTestClass31_SubA
        {
            public void G() { }
        }

        class ModifiersTestClass31 : ModifiersTestClass31_SubB
        {

            int MyInt = 0;

            public override void F()
            {
                MyInt = 1;
            }

            public static bool testMethod()
            {
                ModifiersTestClass31 test = new ModifiersTestClass31();
                ModifiersTestClass31_SubA abstest = test;
                abstest.F();
                if (test.MyInt == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
