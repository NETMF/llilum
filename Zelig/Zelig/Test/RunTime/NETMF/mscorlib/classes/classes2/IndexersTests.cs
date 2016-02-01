////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using Microsoft.Zelig.Test;

namespace Microsoft.Zelig.Test
{
    public class IndexersTests : TestBase, ITestInterface
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

        public override TestResult Run(string[] args)
        {
            TestResult result = TestResult.Pass;

            string testName = "Indexers";
            result |= Assert.CheckFailed(Indexers1_Test(), testName, 1);
            result |= Assert.CheckFailed(Indexers2_Test(), testName, 2);
            result |= Assert.CheckFailed(Indexers3_Test(), testName, 3);
            result |= Assert.CheckFailed(Indexers4_Test(), testName, 4);
            result |= Assert.CheckFailed(Indexers5_Test(), testName, 5);
            result |= Assert.CheckFailed(Indexers6_Test(), testName, 6);
            result |= Assert.CheckFailed(Indexers10_Test(), testName, 10);
            result |= Assert.CheckFailed(Indexers11_Test(), testName, 11);
            result |= Assert.CheckFailed(Indexers12_Test(), testName, 12);
            result |= Assert.CheckFailed(Indexers14_Test(), testName, 14);
            result |= Assert.CheckFailed(Indexers18_Test(), testName, 18);
            result |= Assert.CheckFailed(Indexers23_Test(), testName, 23);
            result |= Assert.CheckFailed(Indexers29_Test(), testName, 29);
            result |= Assert.CheckFailed(Indexers32_Test(), testName, 32);
            result |= Assert.CheckFailed(Indexers33_Test(), testName, 33);
            result |= Assert.CheckFailed(Indexers37_Test(), testName, 37);
            result |= Assert.CheckFailed(Indexers38_Test(), testName, 38);
            result |= Assert.CheckFailed(Indexers39_Test(), testName, 39);
            result |= Assert.CheckFailed(Indexers42_Test(), testName, 42);
            result |= Assert.CheckFailed(Indexers43_Test(), testName, 43);
            result |= Assert.CheckFailed(Indexers46_Test(), testName, 46);
            result |= Assert.CheckFailed(Indexers47_Test(), testName, 47);
            result |= Assert.CheckFailed(Indexers48_Test(), testName, 48);
            result |= Assert.CheckFailed(Indexers49_Test(), testName, 49);
            result |= Assert.CheckFailed(Indexers50_Test(), testName, 50);
            result |= Assert.CheckFailed(Indexers51_Test(), testName, 51);
            result |= Assert.CheckFailed(Indexers52_Test(), testName, 52);
            result |= Assert.CheckFailed(Indexers53_Test(), testName, 53);
            result |= Assert.CheckFailed(Indexers55_Test(), testName, 55);
            result |= Assert.CheckFailed(Indexers56_Test(), testName, 56);

            return result;
        }


        //Indexers Test methods
        //All test methods ported from folder current\test\cases\client\CLR\Conformance\10_classes\Indexers
        //The following tests were included in the Baseline document:
        //01,02,03,04,05,06,10,11,12,14,18,23,29,32,33,37,38,39,42,43,46,47,48,49,50,51,52,53,54,55,56
        //52,54 Failed in the Baseline Document, test 54 has been removed because it would not compile.

        [TestMethod]
        public TestResult Indexers1_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass1.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers2_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass2.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers3_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass3.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;

        }

        [TestMethod]
        public TestResult Indexers4_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass4.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers5_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass5.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers6_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass6.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers10_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass10.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers11_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" An indexer-declaration may include set of");
            Log.Comment(" attributes, a new modifier, and a valid combination");
            Log.Comment(" of the four access modifiers.");
            if (IndexersTestClass11.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers12_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" The type on an indexer declaration specifies");
            Log.Comment(" the element type of the indexer introduced");
            Log.Comment(" by the declaration");
            if (IndexersTestClass12.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers14_Test()
        {
            Log.Comment(" Unless the indexer is an explicit interface");
            Log.Comment(" member implementation, the type is followed");
            Log.Comment(" by the keyword this.  For an explicit ");
            Log.Comment(" interface member implementation, the type is ");
            Log.Comment(" followed by an interface-type, a . and the ");
            Log.Comment(" keyword this.");
            Log.Comment("This is currently an expected fail, but is resolved in 3.0 see Bug  16341 for details");
            if (IndexersTestClass14.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers18_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" The formal-index-parameter-list specifies");
            Log.Comment(" the parameters of the indexer.  The formal");
            Log.Comment(" parameter list of an indexer corresponds");
            Log.Comment(" to that of a method, except that at least");
            Log.Comment(" one parameter must be specified, and that the");
            Log.Comment(" ref and out parameter modifiers are not");
            Log.Comment(" permitted.");
            if (IndexersTestClass18.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers23_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" The type of an indexer declaration and each ");
            Log.Comment(" of the types referenced in the formal-index");
            Log.Comment(" parameter list must be at least as accessible");
            Log.Comment(" as the indexer itself.");
            if (IndexersTestClass23.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers29_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" The formal parameter list of an indexer defines");
            Log.Comment(" the signature of the indexer.  Specifically, the");
            Log.Comment(" signature of an indexer consists of the number and");
            Log.Comment(" types of its formal parameters.  The element type");
            Log.Comment(" is not a part of an index signature, nor are the");
            Log.Comment(" names of the formal parameters.");
            if (IndexersTestClass29.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers32_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" The formal parameter list of an indexer defines");
            Log.Comment(" the signature of the indexer.  Specifically, the");
            Log.Comment(" signature of an indexer consists of the number and");
            Log.Comment(" types of its formal parameters.  The element type");
            Log.Comment(" is not a part of an index signature, nor are the");
            Log.Comment(" names of the formal parameters.");
            if (IndexersTestClass32.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers33_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" The formal parameter list of an indexer defines");
            Log.Comment(" the signature of the indexer.  Specifically, the");
            Log.Comment(" signature of an indexer consists of the number and");
            Log.Comment(" types of its formal parameters.  The element type");
            Log.Comment(" is not a part of an index signature, nor are the");
            Log.Comment(" names of the formal parameters.");
            if (IndexersTestClass33.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers37_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" With these differences in mind, all rules");
            Log.Comment(" defined in 10.6.2 and 10.6.3 apply to indexer");
            Log.Comment(" accessors as well as property accessors.");
            if (IndexersTestClass37.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers38_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" With these differences in mind, all rules");
            Log.Comment(" defined in 10.6.2 and 10.6.3 apply to indexer");
            Log.Comment(" accessors as well as property accessors.");
            if (IndexersTestClass38.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }
        [TestMethod]
        public TestResult Indexers39_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" With these differences in mind, all rules");
            Log.Comment(" defined in 10.6.2 and 10.6.3 apply to indexer");
            Log.Comment(" accessors as well as property accessors.");
            if (IndexersTestClass39.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers42_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" With these differences in mind, all rules");
            Log.Comment(" defined in 10.6.2 and 10.6.3 apply to indexer");
            Log.Comment(" accessors as well as property accessors.");
            if (IndexersTestClass42.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers43_Test()
        {
            Log.Comment(" Section 10.8");
            Log.Comment(" With these differences in mind, all rules");
            Log.Comment(" defined in 10.6.2 and 10.6.3 apply to indexer");
            Log.Comment(" accessors as well as property accessors.");
            if (IndexersTestClass43.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers46_Test()
        {
            Log.Comment("Testing multiple comma separated indexers");
            if (IndexersTestClass46.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers47_Test()
        {
            Log.Comment("Testing multiple comma separated indexers to a public variable");

            if (IndexersTestClass47.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers48_Test()
        {
            Log.Comment("Testing multiple comma separated indexers with a protected internal get and set");
            if (IndexersTestClass48.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers49_Test()
        {
            Log.Comment("Testing multiple comma separated indexers with an internal get and set");
            if (IndexersTestClass49.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers50_Test()
        {
            Log.Comment("Testing multiple comma separated indexers with a private get and set");
            if (IndexersTestClass50.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers51_Test()
        {
            Log.Comment("Testing multiple comma separated indexers with a public virtual get and set");
            if (IndexersTestClass51.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers52_Test()
        {
            Log.Comment("Testing multiple comma separated indexers with an overridden public virtual get and set");
            if (IndexersTestClass52.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers53_Test()
        {
            Log.Comment("Testing multiple comma separated indexers with an overridden public abstract get and set");
            if (IndexersTestClass53.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers55_Test()
        {
            Log.Comment("Testing 10 explicitly specified indexers");
            if (IndexersTestClass55.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }

        [TestMethod]
        public TestResult Indexers56_Test()
        {
            Log.Comment("Testing a single indexers with an overridden public abstract get");
            if (IndexersTestClass56.testMethod())
            {
                return TestResult.Pass;
            }
            return TestResult.Fail;
        }


        public class IndexersTestClass1
        {

            int intJ;

            int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }


            public static bool testMethod()
            {
                try
                {
                    IndexersTestClass1 test = new IndexersTestClass1();
                    test[1] = 1;
                    if (test[2] == 4)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch { return false; }
            }
        }

        public class IndexersTestClass2
        {

            int intJ;

            public int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }


            public static bool testMethod()
            {
                IndexersTestClass2 test = new IndexersTestClass2();
                test[1] = 1;
                if (test[2] == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass3
        {

            int intJ;

            protected int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }


            public static bool testMethod()
            {
                try
                {
                    IndexersTestClass3 test = new IndexersTestClass3();
                    test[1] = 1;
                    if (test[2] == 4)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch { return false; }
            }
        }

        public class IndexersTestClass4
        {

            int intJ;

            internal int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }


            public static bool testMethod()
            {
                IndexersTestClass4 test = new IndexersTestClass4();
                test[1] = 1;
                if (test[2] == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public class IndexersTestClass5
        {

            int intJ;

            private int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }


            public static bool testMethod()
            {
                IndexersTestClass5 test = new IndexersTestClass5();
                test[1] = 1;
                if (test[2] == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass6
        {

            int intJ;

            protected internal int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }


            public static bool testMethod()
            {
                IndexersTestClass6 test = new IndexersTestClass6();
                test[1] = 1;
                if (test[2] == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass7_Sub
        {

            int intJ;

            protected int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }
        }

        public class IndexersTestClass10_Base
        {

            int intJ;

            protected int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }
        }


        public class IndexersTestClass10 : IndexersTestClass10_Base
        {
            public static bool testMethod()
            {
                IndexersTestClass10 test = new IndexersTestClass10();
                test[1] = 1;
                if (test[1] == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass11_Base
        {

            protected int intJ;

            protected int this[int intI]
            {
                get
                {
                    return -1;
                }
                set
                {
                    intJ = -1;
                }
            }
        }


        public class IndexersTestClass11 : IndexersTestClass11_Base
        {

            new protected int this[int intI]
            {
                get
                {
                    return intI + intJ;
                }
                set
                {
                    intJ = intI + 1;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass11 test = new IndexersTestClass11();
                test[1] = 1;
                if (test[2] == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass12_Sub
        {
            public int intI = 2;
        }


        public class IndexersTestClass12
        {

            IndexersTestClass12_Sub TC;

            IndexersTestClass12_Sub this[int i]
            {
                get
                {
                    return TC;
                }
                set
                {
                    TC = value;
                    TC.intI = TC.intI + i;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass12 test = new IndexersTestClass12();
                test[1] = new IndexersTestClass12_Sub();
                if (test[2].intI == 3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public interface IndexersTestClass14_Base
        {
            int this[int i]
            {
                get;
                set;
            }
        }

        public class IndexersTestClass14 : IndexersTestClass14_Base
        {
            int intI;
            int this[int i]
            {
                get
                {
                    return intI;
                }
                set
                {
                    intI = value;
                }
            }

            int IndexersTestClass14_Base.this[int i]
            {
                get
                {
                    return intI + 1;
                }
                set
                {
                    intI = value + 1;
                }
            }
            public static bool testMethod()
            {
                IndexersTestClass14 test1 = new IndexersTestClass14();
                IndexersTestClass14_Base test2 = new IndexersTestClass14();
                test1[1] = 2;
                test2[2] = 2;
                if ((test1[1] == 2) && (test2[2] == 4))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass18
        {

            int this[int int1, int int2, int int3,
                 int int4, int int5, int int6,
                 int int7, int int8, int int9,
                 int int10]
            {

                get
                {
                    return int1 + int2 + int3 + int4 + int5 +
                        int6 + int7 + int8 + int9 + int10;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass18 test = new IndexersTestClass18();
                if (test[1, 2, 3, 4, 5, 6, 7, 8, 9, 10] == 55)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass23
        {

            private class TestClass
            {
                public int intI = 1;
            }

            private TestClass this[TestClass t]
            {
                get
                {
                    return t;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass23 test = new IndexersTestClass23();
                TestClass TC = new TestClass();
                if (test[TC] == TC)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass29
        {

            public int this[int intI]
            {
                get
                {
                    return intI;
                }
            }

            public int this[long lngL]
            {
                get
                {
                    return (int)lngL + 1;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass29 test = new IndexersTestClass29();
                int i = 1;
                long j = 2;

                if ((test[i] == 1) && (test[j] == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass32
        {

            public int this[int intI]
            {
                get
                {
                    return intI;
                }
            }

            public int this[int intI, int intJ]
            {
                get
                {
                    return intI + intJ;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass32 test = new IndexersTestClass32();
                int i = 1;
                int j = 2;

                if ((test[i] == 1) && (test[i, j] == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass33_Base
        {
            public int this[int intI]
            {
                get
                {
                    return intI;
                }
            }
        }

        public class IndexersTestClass33 : IndexersTestClass33_Base
        {

            public int this[int intI, int intJ]
            {
                get
                {
                    return intI + intJ;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass33 test = new IndexersTestClass33();
                int i = 1;
                int j = 2;

                if ((test[i] == 1) && (test[i, j] == 3))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass37
        {

            public int TestInt;

            public virtual int this[int intI]
            {
                set
                {
                    TestInt = intI + value;
                }
                get
                {
                    return TestInt + intI;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass37 test = new IndexersTestClass37();
                test[2] = 2;
                if (test[2] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass38_Base
        {


            public int TestInt;

            public virtual int this[int intI]
            {
                set
                {
                    TestInt = -1;
                }
                get
                {
                    return -1;
                }
            }
        }

        public class IndexersTestClass38 : IndexersTestClass38_Base
        {

            public override int this[int intI]
            {
                set
                {
                    TestInt = intI + value;
                }
                get
                {
                    return TestInt + intI;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass38_Base test = new IndexersTestClass38();
                test[2] = 2;
                if (test[2] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public abstract class IndexersTestClass39_Base
        {


            public int TestInt;

            public abstract int this[int intI]
            {
                set;
                get;
            }
        }

        public class IndexersTestClass39 : IndexersTestClass39_Base
        {

            public override int this[int intI]
            {
                set
                {
                    TestInt = intI + value;
                }
                get
                {
                    return TestInt + intI;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass39_Base test = new IndexersTestClass39();
                test[2] = 2;
                if (test[2] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass42_Base
        {

            public int TestInt;

            public virtual int this[int intI]
            {
                set
                {
                    TestInt = intI + value;
                }
                get
                {
                    return -1;
                }
            }
        }
        public class IndexersTestClass42 : IndexersTestClass42_Base
        {

            public override int this[int intI]
            {
                get
                {
                    return TestInt + intI;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass42_Base test = new IndexersTestClass42();
                test[2] = 2;
                if (test[2] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass43_Base
        {

            public int TestInt;

            public virtual int this[int intI]
            {
                set
                {
                    TestInt = -1;
                }
                get
                {
                    return TestInt + intI;
                }
            }
        }
        public class IndexersTestClass43 : IndexersTestClass43_Base
        {

            public override int this[int intI]
            {
                set
                {
                    TestInt = intI + value;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass43_Base test = new IndexersTestClass43();
                test[2] = 2;
                if (test[2] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass46
        {

            int intTest;

            public int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass46 mc = new IndexersTestClass46();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass47_Base
        {

            public int intTest;

            protected int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }
        }

        public class IndexersTestClass47 : IndexersTestClass47_Base
        {

            public static bool testMethod()
            {
                IndexersTestClass47 mc = new IndexersTestClass47();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass48_Base
        {

            public int intTest;

            protected internal int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }
        }

        public class IndexersTestClass48 : IndexersTestClass48_Base
        {

            public static bool testMethod()
            {
                IndexersTestClass48 mc = new IndexersTestClass48();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass49_Sub
        {

            public int intTest;

            internal int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }
        }

        public class IndexersTestClass49
        {

            public static bool testMethod()
            {
                IndexersTestClass49_Sub mc = new IndexersTestClass49_Sub();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass50
        {

            int intTest;

            private int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass50 mc = new IndexersTestClass50();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass51_Base
        {

            public int intTest;

            public virtual int this[params int[] values]
            {
                get
                {
                    return intTest + 1;
                }
                set
                {
                    intTest = 0;
                }
            }
        }

        public class IndexersTestClass51 : IndexersTestClass51_Base
        {

            public override int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass51_Base mc = new IndexersTestClass51();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass52_Base
        {

            public int intTest;

            public virtual int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }
        }

        public class IndexersTestClass52 : IndexersTestClass52_Base
        {

            public new int this[params int[] values]
            {
                get
                {
                    return intTest + 1;
                }
                set
                {
                    intTest = 0;
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass52_Base mc = new IndexersTestClass52();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public abstract class IndexersTestClass53_Base
        {

            public int intTest;

            public abstract int this[params int[] values]
            {
                get;
                set;
            }
        }

        public class IndexersTestClass53 : IndexersTestClass53_Base
        {

            public override int this[params int[] values]
            {
                get
                {
                    return intTest;
                }
                set
                {
                    intTest = values[0] + values[1] + values[2];
                }
            }

            public static bool testMethod()
            {
                IndexersTestClass53_Base mc = new IndexersTestClass53();
                mc[1, 2, 3] = 0;
                if (mc[1, 2, 3] == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class IndexersTestClass55
        {

            public int this[int i1, int i2, int i3, int i4, int i5, int i6, int i7, int i8, int i9, int i10]
            {
                get
                {
                    return (i1 + i2 + i3 + i4 + i5 + i6 + i7 + i8 + i9 + i10);
                }
            }

            public static bool testMethod()
            {

                IndexersTestClass55 MC = new IndexersTestClass55();

                if (MC[1, 2, 3, 4, 5, 6, 7, 8, 9, 10] == 55)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public abstract class IndexersTestClass56_Base
        {
            protected int this[int intI]
            {
                get
                {
                    return (intI + 1);
                }
            }
        }

        public class IndexersTestClass56 : IndexersTestClass56_Base
        {

            public int RetInt(int j)
            {
                return base[j];
            }

            public static bool testMethod()
            {

                IndexersTestClass56 MC = new IndexersTestClass56();

                if (MC.RetInt(2) == 3)
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
