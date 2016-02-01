////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;


namespace Microsoft.Zelig.Test
{
    public class StringBuilderTests : TestBase, ITestInterface
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

        #region Fields

        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

        #endregion

        public override TestResult Run( string[] args )
        {
            TestResult result = TestResult.Pass;

            string testName = "Test_0_AppendTest_";
            result |= Assert.CheckFailed(Test_0_AppendTest_0(), testName, 0);

            testName = "Test_1_RemoveTest_";
            result |= Assert.CheckFailed(Test_1_RemoveTest_0(), testName, 0);

            testName = "Test_2_InsertTest_";
            //result |= Assert.CheckFailed(Test_2_InsertTest_0(), testName, 0); // https://github.com/NETMF/llilum/issues/109, https://github.com/NETMF/llilum/issues/108

            testName = "Test_3_ReplaceTest_";
            result |= Assert.CheckFailed(Test_3_ReplaceTest_0(), testName, 0);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_1(), testName, 1);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_2(), testName, 2);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_3(), testName, 3);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_4(), testName, 4);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_5(), testName, 5);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_6(), testName, 6);
            result |= Assert.CheckFailed(Test_3_ReplaceTest_7(), testName, 7);

            testName = "Test_4_CapacityTest_";
            result |= Assert.CheckFailed(Test_4_CapacityTest_0(), testName, 0);

            return result;
        }

        #region Append Tests
        
        [TestMethod]
        public TestResult Test_0_AppendTest_0()
        {

            bool result = false;

            stringBuilder.Append(true);

            result = stringBuilder.ToString() == Boolean.TrueString;

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(false);

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString);

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(byte.MinValue);

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue);

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(new char[] { 'x', 'a' });

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue, char.MinValue, "xa");

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(double.Epsilon);

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue, char.MinValue, "xa", double.Epsilon.ToString());

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(float.Epsilon);

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue, char.MinValue, "xa", double.Epsilon.ToString(), float.Epsilon.ToString());

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(int.MaxValue);

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue, char.MinValue, "xa", double.Epsilon.ToString(), float.Epsilon.ToString(), int.MaxValue);

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append(long.MaxValue);

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue, char.MinValue, "xa", double.Epsilon.ToString(), float.Epsilon.ToString(), int.MaxValue, long.MaxValue);

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Append((object)"string");

            result = stringBuilder.ToString() == string.Concat(Boolean.TrueString, Boolean.FalseString, byte.MinValue, char.MinValue, "xa", double.Epsilon.ToString(), float.Epsilon.ToString(), int.MaxValue, long.MaxValue, "string");

            if (!result)
            {
                return TestResult.Fail;
            }

            return (result == true) ? TestResult.Pass : TestResult.Fail;
        }

        #endregion

        #region Remove Tests

        [TestMethod]
        public TestResult Test_1_RemoveTest_0()
        {

            bool result = false;

            result = stringBuilder.Clear().ToString() == string.Empty;

            if (!result)
            {
                return TestResult.Fail;
            }

            string testString = "0123456789";

            stringBuilder.Append(testString);

            stringBuilder.Remove(0, 1);

            result = stringBuilder.ToString() == "123456789";

            if (!result)
            {
                return TestResult.Fail;
            }

            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            result = stringBuilder.ToString() == "12345678";

            if (!result)
            {
                return TestResult.Fail;
            }

            return (result == true) ? TestResult.Pass : TestResult.Fail;
        }

        #endregion

        #region Insert Tests

        [TestMethod]
        public TestResult Test_2_InsertTest_0()
        {
            stringBuilder.Clear();

            bool result = false;

            string testString = "0000";

            stringBuilder.Append(testString);

            stringBuilder.Insert(0, "x", 2);

            result = stringBuilder.ToString() == "xx0000";

            if (!result) return TestResult.Fail;

            stringBuilder.Insert(stringBuilder.Length, "x", 2);

            result = stringBuilder.ToString() == "xx0000xx";

            return (result == false) ? TestResult.Fail : TestResult.Pass;

        }

        #endregion

        #region Replace Tests

        [TestMethod]
        public TestResult Test_3_ReplaceTest_0()
        {

            stringBuilder.Clear();

            bool result = false;

            string testString = "0000";

            stringBuilder.Append(testString);

            stringBuilder.Append("_");

            stringBuilder.Append(testString);

            stringBuilder.Replace(testString, "xx");

            result = stringBuilder.ToString() == "xx_xx";

            return (result == false) ? TestResult.Fail : TestResult.Pass;

        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_1()
        {

            stringBuilder.Clear();

            bool result = false;

            string testString = "BEGIN";

            //stringBuilder.Append("BEGIN_MID_END");

            stringBuilder.Append('B');
            stringBuilder.Append('E');
            stringBuilder.Append('G');
            stringBuilder.Append('I');
            stringBuilder.Append('N');
            stringBuilder.Append('_');
            stringBuilder.Append('M');
            stringBuilder.Append('I');
            stringBuilder.Append('D');
            stringBuilder.Append('_');
            stringBuilder.Append('E');
            stringBuilder.Append('N');
            stringBuilder.Append('D');

            stringBuilder.Replace(testString, "xx");

            result = stringBuilder.ToString() == "xx_MID_END";

            return (result == false) ? TestResult.Fail : TestResult.Pass;

        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_2()
        {

            bool result = false;

            string testString = "MID";

            stringBuilder.Replace(testString, "xx");

            result = stringBuilder.ToString() == "xx_xx_END";

            return (result == false) ? TestResult.Fail : TestResult.Pass;

        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_3()
        {

            bool result = false;

            string testString = "END";

            stringBuilder.Replace(testString, "xx");

            result = stringBuilder.ToString() == "xx_xx_xx";

            return (result == false) ? TestResult.Fail : TestResult.Pass;

        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_4()
        {
            string testString = "The quick br!wn d#g jumps #ver the lazy cat.";
            bool result = false;
            stringBuilder = new System.Text.StringBuilder(testString);

            stringBuilder.Replace('#', '!', 15, 29);        // Some '#' -> '!'

            result = stringBuilder.ToString() == "The quick br!wn d!g jumps !ver the lazy cat.";
            if (!result) return TestResult.Fail;

            stringBuilder.Replace('!', 'o');                // All '!' -> 'o'
            result = stringBuilder.ToString() == "The quick brown dog jumps over the lazy cat.";
            if (!result) return TestResult.Fail;

            stringBuilder.Replace("cat", "dog");            // All "cat" -> "dog"
            result = stringBuilder.ToString() == "The quick brown dog jumps over the lazy dog.";
            if (!result) return TestResult.Fail;

            stringBuilder.Replace("dog", "fox", 15, 20);    // Some "dog" -> "fox"
            result = stringBuilder.ToString() == "The quick brown fox jumps over the lazy dog.";
            if (!result) return TestResult.Fail;

            return TestResult.Pass;
        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_5()
        {
            stringBuilder.Clear();

            stringBuilder.Append("12345");
            stringBuilder.Replace("45", "def");
            return stringBuilder.ToString() == "123def" ? TestResult.Pass : TestResult.Fail;
        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_6()
        {
            stringBuilder.Clear();

            stringBuilder.Append("[{1234}]Test}]");
            stringBuilder.Replace("}]", "}]example");
            return stringBuilder.ToString() == "[{1234}]exampleTest}]example" ? TestResult.Pass : TestResult.Fail;
        }

        [TestMethod]
        public TestResult Test_3_ReplaceTest_7()
        {
            var random = new Random();
            for (int i = 0; i < 1000; i++)
            {
                string sRaw, sFind, sReplace;
                GenerateFuzzyParameters(out sRaw, out sFind, out sReplace, random);

                stringBuilder.Clear();
                stringBuilder.Append(sRaw);
                stringBuilder.Replace(sFind, sReplace);
                string sMFOutput = stringBuilder.ToString();

                string sNativeOutput = NativeReplace(sRaw, sFind, sReplace);

                if (sMFOutput != sNativeOutput)
                {
                    Log.Comment("StringBuilder(\"" + sRaw + "\").Replace(\"" + sFind + "\", \"" + sReplace + "\") returns \"" + sMFOutput + "\"");
                    return TestResult.Fail;
                }
            }

            return TestResult.Pass;
        }

        void GenerateFuzzyParameters(out string sRaw, out string sFind, out string sReplace, Random random)
        {
            int cFind = random.Next(1, 4);
            sFind = RandomString(random, 2, 6);
            sReplace = RandomString(random, 4, 10);

            sRaw = string.Empty;
            for (int i = 0; i < cFind; i++)
            {
                if (i > 0 || random.Next() % 5 > 0)
                {
                    sRaw += RandomString(random, 2, 6);
                }

                sRaw += sFind;
            }

            if (random.Next() % 5 > 0)
            {
                sRaw += RandomString(random, 2, 6);
            }
        }

        string RandomString(Random random, int iLenMin, int iLenMax)
        {
            string sChars = "abcdefghijklmnopqrstuvwxyz0123456789{}[]-=+()";
            int length = random.Next(iLenMin, iLenMax);

            string sOutput = string.Empty;
            for (int i = 0; i < length; i++)
            {
                sOutput += sChars[random.Next(0, sChars.Length - 1)];
            }

            return sOutput;
        }

        string NativeReplace(string sRaw, string sFind, string sReplace)
        {
            string sOutput = sRaw;
            int i = 0;

            while (i < sOutput.Length)
            {
                int p = sOutput.IndexOf(sFind, i);
                if (p < 0)
                {
                    break;
                }

                sOutput = sOutput.Substring(0, p) + sReplace + sOutput.Substring(p + sFind.Length);
                i = p + sReplace.Length;
            }

            return sOutput;
        }

        #endregion

        #region Capacity Tests

        [TestMethod]
        public TestResult Test_4_CapacityTest_0()
        {

            stringBuilder.Length = 0;

            stringBuilder.Capacity = 5;

            bool result = false;

            result = stringBuilder.ToString() == string.Empty;

            if (!result)
            {
                return TestResult.Fail;
            }

            string testString = "0000";

            stringBuilder.Append(string.Empty);

            stringBuilder.Append(testString);

            stringBuilder.Append(string.Empty);

            //should allocate here

            stringBuilder.Append("_");
            stringBuilder.Append("_");

            //result is true if capacity is > 5

            result = stringBuilder.Capacity > 5;

            if (!result)
            {
                return TestResult.Fail;
            }

            return (result == false) ? TestResult.Fail : TestResult.Pass;

        }

        #endregion
    }

    static class RandomExtension
    {
        static public int Next(this Random random, int iMin, int iMax)
        {
            return random.Next(iMax - iMin) + iMin;
        }
    }
}
