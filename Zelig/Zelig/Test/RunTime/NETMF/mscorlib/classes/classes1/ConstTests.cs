////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//#define GET_FIELD_WITH_BINDING_FLAGS_IMPLEMENTED

using System;
using System.Reflection;
using Microsoft.Zelig.Test;

namespace Microsoft.Zelig.Test
{
    public class ConstTests : TestBase, ITestInterface
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

        public override TestResult Run(string[] args)
        {
            TestResult result = TestResult.Pass;

            string testName = "Const";
            result |= Assert.CheckFailed(Const1_Test(), testName, 1);
            result |= Assert.CheckFailed(Const2_Test(), testName, 2);
            result |= Assert.CheckFailed(Const3_Test(), testName, 3);
            result |= Assert.CheckFailed(Const4_Test(), testName, 4);
            result |= Assert.CheckFailed(Const5_Test(), testName, 5);
            result |= Assert.CheckFailed(Const6_Test(), testName, 6);
            result |= Assert.CheckFailed(Const9_Test(), testName, 9);
            result |= Assert.CheckFailed(Const11_Test(), testName, 11);
            result |= Assert.CheckFailed(Const12_Test(), testName, 12);
            result |= Assert.CheckFailed(Const13_Test(), testName, 13);
            result |= Assert.CheckFailed(Const14_Test(), testName, 14);
            result |= Assert.CheckFailed(Const15_Test(), testName, 15);
            result |= Assert.CheckFailed(Const16_Test(), testName, 16);
            result |= Assert.CheckFailed(Const17_Test(), testName, 17);
            result |= Assert.CheckFailed(Const18_Test(), testName, 18);
            result |= Assert.CheckFailed(Const19_Test(), testName, 19);
            result |= Assert.CheckFailed(Const20_Test(), testName, 20);
            result |= Assert.CheckFailed(Const21_Test(), testName, 21);
            result |= Assert.CheckFailed(Const24_Test(), testName, 24);
            result |= Assert.CheckFailed(Const25_Test(), testName, 25);
            result |= Assert.CheckFailed(Const26_Test(), testName, 26);
            result |= Assert.CheckFailed(Const27_Test(), testName, 27);
            result |= Assert.CheckFailed(Const28_Test(), testName, 28);
            result |= Assert.CheckFailed(Const30_Test(), testName, 30);
            result |= Assert.CheckFailed(Const32_Test(), testName, 32);
            result |= Assert.CheckFailed(Const33_Test(), testName, 33);
            result |= Assert.CheckFailed(Const34_Test(), testName, 34);
            result |= Assert.CheckFailed(Const35_Test(), testName, 35);
#if GET_FIELD_WITH_BINDING_FLAGS_IMPLEMENTED
            result |= Assert.CheckFailed(Const42_Test(), testName, 42);
            result |= Assert.CheckFailed(Const43_Test(), testName, 43);
#endif
            result |= Assert.CheckFailed(Const44_Test(), testName, 44);
            result |= Assert.CheckFailed(Const56_Test(), testName, 56);
            result |= Assert.CheckFailed(Const57_Test(), testName, 57);

            return result;
        }

        //--//
        //--//
        //--//

        //Const test methods
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\const
        //The following tests were removed because they were build failure tests:
        //7,8,10,23,29,31,36-41,45-52,55
        //The following tests were removed because they caused the .Net Metadata Processor to crash
        //22,54, these tests failed in the Baseline document
        //Test 53 was removed because of an Attributes error this test was skipped in the Baseline document
        //20,42,43

        [TestMethod]
        public TestResult Const1_Test()
        {
            //Ported from const1.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass1.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const2_Test()
        {
            //Ported from const2.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass2.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const3_Test()
        {
            //Ported from const3.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass3.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;

        }

        [TestMethod]
        public TestResult Const4_Test()
        {
            //Ported from const4.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass4.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const5_Test()
        {
            //Ported from const5.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass5.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const6_Test()
        {
            //Ported from const6.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass6.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }


        [TestMethod]
        public TestResult Const9_Test()
        {
            //Ported from const9.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant-declaration may include a set of attributes,");
            Log.Comment("a new modifier, and one of four access modifiers.  The");
            Log.Comment("attributes and modifiers apply to all of the members ");
            Log.Comment("declared by the constant declaration.");
            if (ConstTestClass9.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }


        [TestMethod]
        public TestResult Const11_Test()
        {
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            //Ported from const11.cs
            if (ConstTestClass11.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const12_Test()
        {
            //Ported from const12.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass12.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const13_Test()
        {
            //Ported from const13.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass13.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const14_Test()
        {
            //Ported from const14.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass14.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const15_Test()
        {
            //Ported from const15.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass15.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const16_Test()
        {
            //Ported from const16.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass16.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const17_Test()
        {
            //Ported from const17.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass17.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const18_Test()
        {
            //Ported from const18.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass18.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const19_Test()
        {
            //Ported from const19.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass19.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const20_Test()
        {
            //Ported from const20.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass20.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const21_Test()
        {
            //Ported from const21.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass21.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const24_Test()
        {
            //Ported from const24.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass24.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const25_Test()
        {
            //Ported from const25.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant itself can participate in a constant-expression.");
            Log.Comment("Thus, a constant may be used in any construct that requires");
            Log.Comment("a constant-expression.  Examples of such constructs include");
            Log.Comment("case labels, goto case statements, enum member declarations,");
            Log.Comment("attributes, and other constant declarations.");
            if (ConstTestClass25.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const26_Test()
        {
            //Ported from const26.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant itself can participate in a constant-expression.");
            Log.Comment("Thus, a constant may be used in any construct that requires");
            Log.Comment("a constant-expression.  Examples of such constructs include");
            Log.Comment("case labels, goto case statements, enum member declarations,");
            Log.Comment("attributes, and other constant declarations.");
            if (ConstTestClass26.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const27_Test()
        {
            //Ported from const27.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant itself can participate in a constant-expression.");
            Log.Comment("Thus, a constant may be used in any construct that requires");
            Log.Comment("a constant-expression.  Examples of such constructs include");
            Log.Comment("case labels, goto case statements, enum member declarations,");
            Log.Comment("attributes, and other constant declarations.");
            if (ConstTestClass27.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const28_Test()
        {
            //Ported from const28.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant itself can participate in a constant-expression.");
            Log.Comment("Thus, a constant may be used in any construct that requires");
            Log.Comment("a constant-expression.  Examples of such constructs include");
            Log.Comment("case labels, goto case statements, enum member declarations,");
            Log.Comment("attributes, and other constant declarations.");
            if (ConstTestClass28.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }


        [TestMethod]
        public TestResult Const30_Test()
        {
            //Ported from const30.cs
            Log.Comment("Section 10.3");
            Log.Comment("Constants are permitted to depend on other constants");
            Log.Comment("within the same project as long as the dependencies");
            Log.Comment("are not of a circular nature.  The compiler automatically");
            Log.Comment("arranges to evaluate the constant declarations in the");
            Log.Comment("appropriate order.");
            if (ConstTestClass30.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }



        [TestMethod]
        public TestResult Const32_Test()
        {
            //Ported from const32.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass32.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const33_Test()
        {
            //Ported from const33.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass33.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const34_Test()
        {
            //Ported from const34.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass34.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const35_Test()
        {
            //Ported from const35.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type specified in a constant declaration");
            Log.Comment("must be byte, char, short, int, long, float,");
            Log.Comment("double, double, bool, string, an enum-type,");
            Log.Comment("or a reference type.  Each constant-expression");
            Log.Comment("must yield a value of the target type or of a ");
            Log.Comment("type that can be converted to the target type");
            Log.Comment("by implicit conversion.");
            if (ConstTestClass35.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const42_Test()
        {
            //Ported from const42.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant declarator introduces a new member");
            Log.Comment("This test is expected to fail");
            if (ConstTestClass42.test())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 17246");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return TestResult.Fail;
            }
            return TestResult.Pass;
        }

        [TestMethod]
        public TestResult Const43_Test()
        {
            //Ported from const43.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant declarator introduces a new member");
            Log.Comment("This test is expected to fail");
            if (ConstTestClass43.test())
            {
                Log.Comment("This failure indicates a test is now passing that previously failed by design.");
                Log.Comment("It previously marked as known failure because of bug # 17246");
                Log.Comment("The Test owner needs to verify that the change was intentional and remove the known failure.");
                return TestResult.Fail;
            }
            return TestResult.Pass;
        }

        [TestMethod]
        public TestResult Const44_Test()
        {
            //Ported from const44.cs
            Log.Comment("Section 10.3");
            Log.Comment("The type of a constant must be at least as acccessible as the constant itself.");
            if (ConstTestClass44.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }


        [TestMethod]
        public TestResult Const56_Test()
        {
            //Ported from const56.cs
            Log.Comment("Section 10.3");
            Log.Comment("...the only possible value for constants of reference-types other than ");
            Log.Comment("string is null");
            if (ConstTestClass56.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Const57_Test()
        {
            //Ported from const57.cs
            Log.Comment("Section 10.3");
            Log.Comment("A constant declaration that declares multiple constants is equivalent to ");
            Log.Comment("multiple declarations of single constants with the same attributes, ");
            Log.Comment("modifiers, and type. ");
            if (ConstTestClass57.test())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        //Const test classes
        class ConstTestClass1
        {
            const int intI = 2;

            public static bool test()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass2_base
        {
            public const int intI = 1;
        }

        class ConstTestClass2 : ConstTestClass2_base
        {
            new const int intI = 2;

            public static bool test()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass3
        {
            public const int intI = 2;

            public static bool test()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass4
        {
            protected const int intI = 2;

            public static bool test()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass5
        {
            internal const int intI = 2;

            public static bool test()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass6
        {

            private const int intI = 2;

            public static bool test()
            {
                if (intI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass7_sub
        {
            protected const int intI = 2;
        }

        class ConstTestClass9_sub
        {
            public const int intI = 2, intJ = 3;
        }

        class ConstTestClass9
        {

            public static bool test()
            {
                if ((ConstTestClass9_sub.intI == 2) && (ConstTestClass9_sub.intJ == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass11
        {

            const byte byteB = 2;

            public static bool test()
            {
                if (byteB == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass12
        {

            const char charC = 'b';

            public static bool test()
            {
                if (charC == 'b')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass13
        {

            const short shortS = 2;

            public static bool test()
            {
                if (shortS == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass14
        {
            //This appears to be a duplicate of Test 1
            const int IntI = 2;

            public static bool test()
            {
                if (IntI == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass15
        {

            const long longL = 2L;

            public static bool test()
            {
                if (longL == 2L)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass16
        {

            const float floatF = 2.0F;

            public static bool test()
            {
                if (floatF == 2.0F)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass17
        {

            const double doubleD = 2.0D;

            public static bool test()
            {
                if (doubleD == 2.0D)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass18
        {
            //Is this OK to cast?
            const double doubleD = (double)2.0M;

            public static bool test()
            {
                if (doubleD == (double)2.0M)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass19
        {

            const bool boolB = true;

            public static bool test()
            {
                if (boolB)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass20
        {

            const string stringS = "mytest";

            public static bool test()
            {
                if (stringS.Equals("mytest"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        enum ConstTestClass21Enum { a = 1, b = 2 }

        class ConstTestClass21
        {

            const ConstTestClass21Enum enumE = ConstTestClass21Enum.a;

            public static bool test()
            {
                if (enumE == ConstTestClass21Enum.a)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass24
        {

            const double doubleD = 2.0F;

            public static bool test()
            {
                if (doubleD == 2.0D)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass25
        {

            const int MyInt = 1;

            public static bool test()
            {

                int intI = 1;

                switch (intI)
                {
                    case MyInt:
                        return true;
                    default:
                        return false;
                }
            }
        }

        class ConstTestClass26
        {

            const int MyInt = 1;

            public static bool test()
            {

                int intI = 2;

                switch (intI)
                {
                    case 1:
                        return true;
                    case 2:
                        goto case MyInt;
                    default:
                        return false;
                }
            }
        }

        class ConstTestClass27
        {
            const int MyInt = 2;

            enum MyEnum { a = 1, b = MyInt }

            public static bool test()
            {
                if (MyEnum.b == (MyEnum)2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass28
        {

            const int MyInt = 2;
            const int MyTest = MyInt;

            public static bool test()
            {
                if (MyTest == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass30_sub
        {
            public const int X = ConstTestClass30.Z + 1;
            public const int Y = 10;
        }
        class ConstTestClass30
        {
            public const int Z = ConstTestClass30_sub.Y + 1;
            public static bool test()
            {
                if ((ConstTestClass30_sub.Y == 10) && (ConstTestClass30.Z == 11) && (ConstTestClass30_sub.X == 12))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass32
        {

            const sbyte sbyteS = 2;

            public static bool test()
            {
                if (sbyteS == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass33
        {

            const ushort ushortU = 2;

            public static bool test()
            {
                if (ushortU == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass34
        {

            const uint uintU = 2;

            public static bool test()
            {
                if (uintU == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass35
        {

            const ulong ulongU = 2;

            public static bool test()
            {
                if (ulongU == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        class ConstTestClass42
        {
            const int intI = 10;

            public static bool test()
            {
                bool retval = false;
                FieldInfo fi = typeof(ConstTestClass42).GetField("intI", BindingFlags.NonPublic | BindingFlags.Static);
                if (null != fi)
                    retval = true;
                return retval;
            }
        }

        class ConstTestClass43
        {

            const int intI = 10;

            public static bool test()
            {
                bool retval = false;
                FieldInfo fi = typeof(ConstTestClass43).GetField("intI", BindingFlags.NonPublic | BindingFlags.Static);
                if (null != fi)
                    if ((int)fi.GetValue(ConstTestClass43.intI) == 10)
                        retval = true;
                return retval;
            }
        }

        class ConstTestClass44
        {

            enum E { zero, one, two, three };
            const E enumE = E.two;

            public static bool test()
            {
                bool retval = false;

                if (enumE == E.two)
                    retval = true;
                return retval;
            }
        }

        class ConstTestClass55_sub
        {
            int _i;
            public ConstTestClass55_sub(int i) { _i = i; }
            public int GetI() { return _i; }
        }

        class ConstTestClass56_sub
        {
            int _i;
            public ConstTestClass56_sub(int i) { _i = i; }
            public int GetI() { return _i; }
        }

        class ConstTestClass56
        {

            public readonly ConstTestClass56_sub mc = new ConstTestClass56_sub(10);

            public static bool test()
            {
                bool retval = false;
                ConstTestClass56 mmc = new ConstTestClass56();
                if (mmc.mc.GetI() == 10)
                    retval = true;
                return retval;
            }
        }

        class ConstTestClass57_sub_A
        {
            public const double X = 1.0, Y = 2.0, Z = 3.0;
        }

        class ConstTestClass57_sub_B
        {
            public const double X = 1.0;
            public const double Y = 2.0;
            public const double Z = 3.0;
        }

        class ConstTestClass57
        {
            public static bool test()
            {
                bool retval = false;
                ConstTestClass57 mmc = new ConstTestClass57();
                if ((ConstTestClass57_sub_A.X == ConstTestClass57_sub_B.X)
                    && (ConstTestClass57_sub_A.Y == ConstTestClass57_sub_B.Y)
                    && (ConstTestClass57_sub_A.Z == ConstTestClass57_sub_B.Z))
                    retval = true;
                return retval;
            }
        }

    }
}
