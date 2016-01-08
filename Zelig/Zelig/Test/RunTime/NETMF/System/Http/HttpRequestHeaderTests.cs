////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.Llilum.Lwip;
using System;


using System.Net;

namespace Microsoft.Zelig.Test
{
    public class HttpRequestHeaderTests : TestBase, ITestInterface
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

        public override TestResult Run( string[] args )
        {
            return TestResult.Pass;
        }
        
        //--//
        //--//
        //--//


        #region Helper methods
        private TestResult Verify(System.Net.WebHeaderCollection wrc, System.Net.WebHeaderCollection RequestHeaders)
        {
            TestResult result = TestResult.Pass;

            try
            {
                Log.Comment("Get Headers - User-Agent");
                if (wrc["User-Agent"] != RequestHeaders["User-Agent"])
                {
                    Log.Exception("User-Agent property value is incorrect.");
                    result = TestResult.Fail;
                }

                Log.Comment("Get Headers - Connection");
                if (wrc["Connection"] != RequestHeaders["Connection"])
                {
                    Log.Exception("Connection property value is incorrect.");
                    result = TestResult.Fail;
                }

                Log.Comment("Get Headers - Host");
                if (wrc["Host"] != RequestHeaders["Host"])
                {
                    Log.Exception("Host property value is incorrect.");
                    result = TestResult.Fail;
                }
            }
            catch (Exception ex)
            {
                if (!HttpTests.ValidateException(ex, typeof(InvalidOperationException)))
                    result = TestResult.Fail;
            }

            return result;
        }

        #endregion Helper methods


        #region Test
        [TestMethod]
        public TestResult ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;
                
                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public TestResult ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1_Https()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://www.microsoft.com:443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("https", 443, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult InValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1_Https()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 1:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public TestResult InvalidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1_FTP()
        {
            TestResult result = TestResult.Pass;
            UriProperties props = new UriProperties("ftp", "//ftp.microsoft.com");
            Uri uri = new Uri(props.OriginalUri);

            Log.Comment("Negative Test case 2:");
            Log.Comment("Create WebRequest with FTP uri");
            try
            {
                HttpWebRequest wrftp = (HttpWebRequest)WebRequest.Create(uri);
            }
            catch (System.NotSupportedException)
            {
                Log.Comment("Create WebRequest with FTP uri - Expected System.NotSupportedException");
            }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp.microsoft.com");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public TestResult InvalidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_1()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 3:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();

                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }
        

        [TestMethod]
        public TestResult ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult ValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0_HTTPS()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("https", 443, ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult InValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0_HTTPS()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:443/");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 4:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult InValidDefaultTestGetHTTPRequestHeaderAfterCreateHTTP1_0_FTP()
        {
            TestResult result = TestResult.Pass;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://ftp");
            wr.UserAgent = ".Net Micro Framwork Device/4.0";

            Log.Comment("Negative Test case 5:");
            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.0");
            wr.ProtocolVersion = new Version(1, 0);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
            };

            try
            {
                // Setup server
                server.StartServer();
                System.Net.WebHeaderCollection wrc = wr.Headers;

                // Tests
                Verify(wrc, server.RequestHeaders);
            }
            catch (Exception ex)
            {
                Log.Exception("[Client] Unexpected Exception", ex);
                result = TestResult.Fail;
            }
            finally
            {
                // Stop server
                server.StopServer();
            }

            return result;
        }


        #endregion Test
    }
}
