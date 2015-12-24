////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Microsoft.Llilum.Lwip;
using System;


using System.Net;

namespace Microsoft.Zelig.Test
{
    public class WebResponseTests : TestBase, ITestInterface
    {
        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");

            try
            {
                NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            }
            catch
            {
                return InitializeResult.Skip;
            }


            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("Cleaning up after the tests.");

            // TODO: Add your clean up steps here.
        }

        [TestMethod]
        public TestResult NotSupportExceptionTest()
        {
            TestResult result = TestResult.Pass;
            try
            {
                Log.Comment("WebResponse Test");
                HttpWebRequest wrStr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/");

                HttpServer server = new HttpServer("http", ref result)
                {
                    RequestUri = wrStr.RequestUri,
                    RequestHeaders = wrStr.Headers,
                    ResponseString = "<html><body>WebResponse Test</body></html>"
                };

                server.StartServer();

                WebRequest wr = wrStr;

                WebResponse wresp = wr.GetResponse();

                Log.Comment("Check ResponseUri property");
                if (wresp.ResponseUri.AbsoluteUri != wr.RequestUri.AbsoluteUri)
                {
                    Log.Exception("Expected " + HttpTests.MSUrl + ", but got " + wresp.ResponseUri.AbsoluteUri);
                    result = TestResult.Fail;
                }

                Log.Comment("Check ContentType property");
                if (wresp.ContentType != "")
                {
                    Log.Exception("Expected: " + wr.ContentType);
                    result = TestResult.Fail;
                }

                Log.Comment("Invoke WebResponse.Close()");
                try
                {
                    wresp.Close();
                }
                catch (System.NotSupportedException e)
                {
                    Log.Exception("Get exception when invoke WebResponse.Close(). " + e.Message.ToString());
                    result = TestResult.Fail;
                }
                finally
                {
                    //Stop server
                    server.StopServer();
                }

            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = TestResult.Fail;
            }

            return result;
        }
    }
}
