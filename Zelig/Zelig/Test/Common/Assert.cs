using System;
using Microsoft.Zelig.Runtime;

namespace Microsoft.Zelig.Test
{
    public static class Assert
    {
        public static TestResult CheckFailed(TestResult result)
        {
            if ((result & TestResult.Fail) != 0)
            {
                BugCheck.Assert(false, BugCheck.StopCode.InvalidOperation);
            }

            return result;
        }

        public static TestResult CheckFailed(TestResult result, string testName, int testNumber)
        {
            if ((result & TestResult.Fail) != 0)
            {
                TestConsole.WriteLine($"Failed: {testName}{testNumber}");
            }
            else if ((result & TestResult.KnownFailure) != 0)
            {
                TestConsole.WriteLine($"Known failure: {testName}{testNumber}");
            }

            return result;
        }

        public static void AreEqual(object expected, object actual)
        {
            AreEqual(expected, actual, null);
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.AreEqual2"]/*' />
        public static void AreEqual(object expected, object actual, string message)
        {
            if (!object.Equals(expected, actual))
            {
                if(message == null) message = String.Empty;

                throw new ApplicationException(message +
                    "\n\texpected:{"
                    + ((expected == null) ? "(null)" : expected.ToString())
                    + "}\n\t but was:{"
                    + ((actual == null) ? "(null)" : actual.ToString())
                    + "}");
            }
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.AreSame"]/*' />
        public static void AreSame(object expected, object actual)
        {
            AreSame(expected, actual, null);
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.AreSame2"]/*' />
        public static void AreSame(object expected, object actual, string message)
        {
            if (expected != actual)
            {
                if(message == null) message = String.Empty;

                throw new ApplicationException(message + 
                    "\n\texpected:{"
                    + ((expected == null) ? "(null)" : expected.ToString())
                    + "}\n\t but was:{"
                    + ((actual == null) ? "(null)" : actual.ToString())
                    + "}");
            }
        }        

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsTrue"]/*' />
        public static void IsTrue(bool condition)
        {
            IsTrue(condition, null);
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsTrue2"]/*' />
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new ApplicationException(message);
            }
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsFalse"]/*' />
        public static void IsFalse(bool condition)
        {
            IsFalse(condition, null);
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsFalse2"]/*' />
        public static void IsFalse(bool condition, string message)
        {
            if (condition)
            {
                throw new ApplicationException(message);
            }
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsNull"]/*' />
        public static void IsNull(object anObject)
        {
            IsNull(anObject, null);
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsNull2"]/*' />
        public static void IsNull(object anObject, string message)
        {
            if (anObject != null)
            { 
                throw new ApplicationException(message); 
            }
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsNotNull"]/*' />
        public static void IsNotNull(object anObject)
        {
            IsNotNull(anObject, null);
        }

        // TODO <include file='doc\Assert.uex' path='docs/doc[@for="Assert.IsNotNull2"]/*' />
        public static void IsNotNull(object anObject, string message)
        {
            if (anObject == null)
            {
                throw new ApplicationException(message);
            }
        }
    }
}
