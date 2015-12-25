////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Microsoft.Llilum.Lwip;
using System;


using System.Net;

namespace Microsoft.Zelig.Test
{
    public class WebExceptionTests : TestBase, ITestInterface
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


        #region Helper methods

        private TestResult VerifyStream(HttpWebResponse response, HttpServer server)
        {
            TestResult result = TestResult.Pass;

            using (System.IO.Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    string page = HttpTests.ReadStream("Client", responseStream);
                    if (page != server.ResponseString)
                    {
                        Log.Exception("Expect " + server.ResponseString + " but get " + responseStream.ToString());
                        result = TestResult.Fail;
                    }
                }
                else
                {
                    result = TestResult.Fail;
                    Log.Exception("[Client] Expected stream, but got null");
                }
            }

            return result;
        }

        #endregion Helper methods


        #region Test
        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_ConnectionClosed()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/ConnClose.html");  //expect ConnectionClosed
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ConnectionClosed");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_KeepAliveFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/KeepAliveFailure.html");  //expect KeepAliveFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect KeepAliveFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_Pending()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/Pending.html");  //expect Pending
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect Pending");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_PipelineFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/PipelineFailure.html");  //expect PipelineFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect PipelineFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_ProxyNameResolutionFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/ProxyNameResolutionFailure.html");  //expect ProxyNameResolutionFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ProxyNameResolutionFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_ReceiveFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/ReceiveFailure.html");  //expect ReceiveFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ReceiveFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_RequestCanceled()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/RequestCanceled.html");  //expect RequestCanceled
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect RequestCanceled");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_SecureChannelFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/SecureChannelFailure.html");  //expect SecureChannelFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect SecureChannelFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_SendFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/SendFailure.html");  //expect SendFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect SendFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_Success()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/Success.html");  //expect Success
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect Success");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_Timeout()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/Timeout.html");  //expect Timeout
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect Timeout");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_TrustFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/TrustFailure.html");  //expect TrustFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect TrustFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_ConnectFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/ConnectFailure.html");  //expect ConnectFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ConnectFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_NameResolutionFailure()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/NameResolutionFailure.html");  //expect NameResolutionFailure
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect NameResolutionFailure");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_ProtocolError()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/ProtocolError.html");  //expect ProtocolError
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ProtocolError");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }


        [TestMethod]
        public TestResult TestWebExceptionHTTP1_1_ServerProtocolViolation()
        {
            TestResult result = TestResult.Pass;


            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + HttpServer.s_CurrentPort.ToString() + "/webexception/ServerProtocolViolation.html");  //expect ServerProtocolViolation
            wr.UserAgent = ".Net Micro Framwork Device/4.0";
            wr.Method = "HEAD";

            Log.Comment("Initial version: " + wr.ProtocolVersion);  //Default version is 1.1

            Log.Comment("Set Version 1.1");
            wr.ProtocolVersion = new Version(1, 1);

            HttpServer server = new HttpServer("http", ref result)
            {
                RequestUri = wr.RequestUri,
                RequestHeaders = wr.Headers,
                ResponseString = ""
            };

            try
            {
                // Setup server
                server.StartServer();

                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                Log.Comment("Expect ServerProtocolViolation");

                VerifyStream(response, server);

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("Exception caught: ", ex);
            }
            finally
            {
                //Stop server
                server.StopServer();
            }

            return result;
        }

        #endregion Test
    }
}
