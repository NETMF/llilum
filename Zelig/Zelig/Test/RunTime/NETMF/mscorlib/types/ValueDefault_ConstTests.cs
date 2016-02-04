////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.Zelig.Test
{
    public class ValueDefault_ConstTests : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests");

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

            result |= Assert.CheckFailed( ValueDefault_Const01_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const02_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const03_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const04_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const05_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const06_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const07_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const09_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const11_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const12_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const14_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const15_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const16_Test( ) );
            result |= Assert.CheckFailed( ValueDefault_Const17_Test( ) );

            return result;
        }

        //ValueDefault_Const Test methods
        //The following tests were ported from folder current\test\cases\client\CLR\Conformance\4_values\Value\Default_Const
        //01,02,03,04,05,06,07,09,11,12,14,15,16,17
        //All of these tests passed in the Baseline document

        //Test Case Calls 
        [TestMethod]
        public TestResult ValueDefault_Const01_Test()
        {
            Log.Comment("Testing byte == 0");
            if (ValueDefault_ConstTestClass01.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const02_Test()
        {
            Log.Comment("Testing short == 0");
            if (ValueDefault_ConstTestClass02.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const03_Test()
        {
            Log.Comment("Testing int == 0");
            if (ValueDefault_ConstTestClass03.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const04_Test()
        {
            Log.Comment("Testing long == 0L");
            if (ValueDefault_ConstTestClass04.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const05_Test()
        {
            Log.Comment("Testing char == \x0000");
            if (ValueDefault_ConstTestClass05.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const06_Test()
        {
            Log.Comment("Testing float == 0.0f");
            if (ValueDefault_ConstTestClass06.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const07_Test()
        {
            Log.Comment("Testing double == 0.0d");
            if (ValueDefault_ConstTestClass07.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const09_Test()
        {
            Log.Comment("Testing bool == false");
            if (ValueDefault_ConstTestClass09.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const11_Test()
        {
            Log.Comment("Testing enum");
            if (ValueDefault_ConstTestClass11.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const12_Test()
        {
            Log.Comment("Testing struct");
            if (ValueDefault_ConstTestClass12.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const14_Test()
        {
            Log.Comment("Testing sbyte == 0");
            if (ValueDefault_ConstTestClass14.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const15_Test()
        {
            Log.Comment("Testing ushort == 0");
            if (ValueDefault_ConstTestClass15.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const16_Test()
        {
            Log.Comment("Testing uint == 0");
            if (ValueDefault_ConstTestClass16.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult ValueDefault_Const17_Test()
        {
            Log.Comment("Testing ulong == 0");
            if (ValueDefault_ConstTestClass17.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        //Compiled Test Cases 
        public class ValueDefault_ConstTestClass01
        {
            public static bool testMethod()
            {
                byte b = new byte();
                if (b == (byte)0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass02
        {
            public static bool testMethod()
            {
                short s = new short();
                if (s == (short)0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass03
        {
            public static bool testMethod()
            {
                int i = new int();
                if (i == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass04
        {
            public static bool testMethod()
            {
                long l = new long();
                if (l == 0L)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass05
        {
            public static bool testMethod()
            {
                char c = new char();
                if (c == '\x0000')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass06
        {
            public static bool testMethod()
            {
                float f = new float();
                if (f == 0.0f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass07
        {
            public static bool testMethod()
            {
                double d = new double();
                if (d == 0.0d)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass09
        {
            public static bool testMethod()
            {
                bool b = new bool();
                if (b == false)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        enum E { a = 1, b = 2 };
        public class ValueDefault_ConstTestClass11
        {
            public static bool testMethod()
            {
                E e = new E();
                if (e == ((E)0))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        struct MyStruct
        {
            public int I;
            public Object MyObj;
        }
        public class ValueDefault_ConstTestClass12
        {
            public static bool testMethod()
            {
                MyStruct TC = new MyStruct();
                if ((TC.I == 0) && (TC.MyObj == null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass14
        {
            public static bool testMethod()
            {
                sbyte b = new sbyte();
                if (b == (sbyte)0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass15
        {
            public static bool testMethod()
            {
                ushort b = new ushort();
                if (b == (ushort)0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass16
        {
            public static bool testMethod()
            {
                uint b = new uint();
                if (b == (uint)0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public class ValueDefault_ConstTestClass17
        {
            public static bool testMethod()
            {
                ulong b = new ulong();
                if (b == (ulong)0)
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
