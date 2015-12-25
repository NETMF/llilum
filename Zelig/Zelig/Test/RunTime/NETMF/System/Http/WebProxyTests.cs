////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;


using System.Net;

namespace Microsoft.Zelig.Test
{
    public class WebProxyTests : TestBase, ITestInterface
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


        [TestMethod]
        public TestResult TestDefaultWebProxy()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy()");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);
            WebProxy proxyObject = new WebProxy();

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri != uri)
            {
                result = TestResult.Fail;
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebProxyConstructor1()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy(string)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            string proxy = "itgproxy.redmond.corp.microsoft.com";
            WebProxy proxyObject = new WebProxy(proxy);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri.Host != proxy)
            {
                result = TestResult.Fail;
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebProxyConstructor2()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy(string, bool)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            string proxy = "itgproxy.redmond.corp.microsoft.com";
            WebProxy proxyObject = new WebProxy(proxy, true);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri.Host != proxy)
            {
                result = TestResult.Fail;
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebProxyConstructor3()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy(string, int)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            string proxy = "itgproxy.redmond.corp.microsoft.com";
            WebProxy proxyObject = new WebProxy(proxy, 80);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri.Host != proxy)
            {
                result = TestResult.Fail;
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebProxyConstructor4()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy(System.Uri, bool)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            WebProxy proxyObject = new WebProxy(uri, true);

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri != uri)
            {
                result = TestResult.Fail;
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebProxyConstructor5()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy(System.Uri)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            WebProxy proxyObject = new WebProxy(uri);

            if (proxyObject.BypassProxyOnLocal)
            {
                result = TestResult.Fail;
            }

            WebRequest wr = WebRequest.Create(uri);
            wr.Proxy = proxyObject;

            Uri wrUri = wr.Proxy.GetProxy(uri);

            if (wrUri != uri)
            {
                result = TestResult.Fail;
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebProxyInvalidserverAddress()
        {
            TestResult result = TestResult.Pass;

            Log.Comment("Set proxy using WebProxy(string, bool)");
            UriProperties props = new UriProperties("http", "www.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            try
            {
                WebProxy proxyObject = new WebProxy("ht1p:itgproxy", true);
                result = TestResult.Fail;
            }
            catch (ArgumentException ex)
            {
                Log.Exception("Expect ArgumentException: ", ex);
            }
            
            try
            {
                WebProxy proxyObject = new WebProxy(string.Empty, true);
                result = TestResult.Fail;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Exception("Expect ArgumentOutOfRangeException: ", ex);
            }

            return result;
        }
    }
}
