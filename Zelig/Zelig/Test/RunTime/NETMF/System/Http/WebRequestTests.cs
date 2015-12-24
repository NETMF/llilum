////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;


using System.Net;

namespace Microsoft.Zelig.Test
{
    public class WebRequestTests : TestBase, ITestInterface
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

        #region Test Cases

        [TestMethod]
        public TestResult ValidConstructorTests()
        {
            TestResult result = TestResult.Pass;

            try
            {
                Log.Comment("string constructor");
                WebRequest wrStr = WebRequest.Create(HttpTests.MSUrl);
                if (wrStr.RequestUri.AbsoluteUri != HttpTests.MSUrl + "/")
                {
                    Log.Exception("Expected " + HttpTests.MSUrl + ", but got " + wrStr.RequestUri.AbsoluteUri);
                    result = TestResult.Fail;
                }

                Log.Comment("uri constructor");
                Uri uri = new Uri(HttpTests.MSUrl);
                WebRequest wrUri = WebRequest.Create(uri);
                if (wrUri.RequestUri.AbsoluteUri != HttpTests.MSUrl + "/")
                {
                    Log.Exception("Expected " + HttpTests.MSUrl + ", but got " + wrUri.RequestUri.AbsoluteUri);
                    result = TestResult.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult InvalidConstructorTests()
        {
            TestResult result = TestResult.Pass;

            try
            {
                Log.Comment("null string");
                string nullString = null;
                try { WebRequest nsWR = WebRequest.Create(nullString); }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = TestResult.Fail;
                }

                Log.Comment("null uri");
                Uri nullUri = null;
                try { WebRequest nuWr = WebRequest.Create(nullUri); }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(ArgumentNullException)))
                        result = TestResult.Fail;
                }

                Log.Comment("invalud URI type");
                try { WebRequest inWr = WebRequest.Create("ftp://ftp.microsoft.com"); }
                catch (Exception ex)
                {
                    if (!HttpTests.ValidateException(ex, typeof(NotSupportedException)))
                        result = TestResult.Fail;
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        [TestMethod]
        public TestResult ValidPropertiesTests()
        {
            TestResult result = TestResult.Pass;

            try
            {
                Log.Comment("WebRequest Properties");
                WebRequest wrStr = WebRequest.Create(HttpTests.MSUrl);

                Log.Comment("Get ContentType property");
                if (null != wrStr.ContentType)
                {
                    Log.Exception("Expected null but got an object");
                    result = TestResult.Fail;
                }

                Log.Comment("Get Method property");
                if (wrStr.Method == System.String.Empty || wrStr.Method.CompareTo("GET") != 0)
                {
                    Log.Exception("Expected Method string but get empty string");
                    result = TestResult.Fail;
                }

                Log.Comment("Get Proxy property - Expect null");
                if (wrStr.Proxy != null)
                {
                    Log.Exception("Expected Proxy property to be null but ");
                    result = TestResult.Fail;
                }


                Log.Comment("Set and Get Timeout property");
                wrStr.Timeout = 90000;
                if (wrStr.Timeout != 90000)
                {
                    Log.Exception("Failed to set TimeOut property");
                    result = TestResult.Fail;
                }

                wrStr.Timeout = 100000;
                if (wrStr.Timeout != 100000)
                {
                    Log.Exception("Failed to set TimeOut property");
                    result = TestResult.Fail;
                }

                Log.Comment("Set ConnectionGroupName property - should get System.NotSupportedException");
                try
                {
                    wrStr.ConnectionGroupName = "test";
                }
                catch (System.NotSupportedException e)
                {
                    Log.Exception("Get exception when set the value of ConnectionGroupName property. " + e.Message.ToString());
                }

                
                Log.Comment("Set PreAuthenticate property");
                try
                {
                    // BUILD BREAK - PreAuthenticate is not defined for HttpWebRequest 
                    //wrStr.PreAuthenticate = true;
                } 
                catch (Exception e)
                {
                    Log.Exception("Bug #61228:  Get exception when set the value of PreAuthenticate property. " + e.Message.ToString());
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }

        #endregion Test Cases
    }
}
