//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// NOTICE: The firewall will generally block all incoming connections to a machine from external devices.
// Please go to Windows Firewall -> Advanced Settings, and create an inbound rule that allows connections
// to port 8080
//

namespace HttpTest
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Net.Sockets;
    using System.Collections.Generic;

    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }
            
            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
            {
                throw new ArgumentException("prefixes");
            }
                
            // A responder method is required
            if (method == null)
            {
                throw new ArgumentException("method");
            }

            foreach (string s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _responderMethod = method;
            _listener.Start();
        }

        public void Run()
        {
            Task.Run(() =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        Task.Run(() =>
                        {
                            var context = _listener.GetContext();
                            try
                            {
                                // Received a request
                                Console.WriteLine("\n!!!Received Request!!!\n");
                                Console.WriteLine("Method: {0}\n", context.Request.HttpMethod);
                                Console.WriteLine("Headers: {0}\n", context.Request.Headers);
                                Console.WriteLine("Raw URL: {0}\n", context.Request.RawUrl);

                                // See what data we received
                                using (System.IO.Stream data = context.Request.InputStream)
                                {
                                    using (System.IO.StreamReader reader = new System.IO.StreamReader(data, context.Request.ContentEncoding))
                                    {
                                        Console.WriteLine("Data from request:\n");
                                        Console.WriteLine(reader.ReadToEnd());
                                    }
                                }

                                // Send back a response using our custom function based on request data
                                string response = _responderMethod(context.Request);
                                Console.WriteLine("Sending Response: {0}\n", response);

                                byte[] buf = Encoding.ASCII.GetBytes(response);

                                context.Response.ContentLength64 = buf.Length;
                                context.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch (Exception ex)
                            {
                                // suppress any exceptions and log
                                Console.WriteLine("Server caught an exception: {0}", ex.StackTrace);
                            }
                            finally
                            {
                                // always close the stream
                                Console.WriteLine("Closing the output stream");
                                context.Response.OutputStream.Close();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Server caught an exception: {0}", ex.Message);
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            WebServer server = null;

            //
            // Establish the local endpoint for the socket. 
            //
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            //
            // Pick 1st IPv4 address in the list. 
            //
            List<IPAddress> ipv4Locals = new List<IPAddress>();
            foreach (var addr in ipHostInfo.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipv4Locals.Add(addr);
                }
            }

            try
            {
                Console.WriteLine("Starting the web server...");

                server = new WebServer(SendResponse, "http://localhost:8080/", "http://*:8080/");
                server.Run();

                Console.WriteLine("Web server started.");
                Console.WriteLine("Your possible IPv4 addresses. Use in SimpleHTTP Test Program:");

                foreach (var addr in ipv4Locals)
                {
                    Console.WriteLine("http://{0}:8080", addr.ToString());
                }
                
                Console.WriteLine("Press the Q key to quit.");

                while (Console.ReadKey().KeyChar != 'q' && Console.ReadKey().KeyChar != 'Q')
                {
                    // Block until user wants to quit
                }

                server.Stop();
            }
            catch(System.Net.HttpListenerException ex)
            {
                Console.WriteLine(string.Format("{0} - Need to restart as administrator. \nPress 'Enter' to continue...", ex.Message));
                Console.Read();
                RestartAsAdmin();
            }
        }

        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo("HttpTest.exe") { Verb = "runas" };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        // Respond to the user with their query string in readable format
        public static string SendResponse(HttpListenerRequest request)
        {
            StringBuilder builder = new StringBuilder();
            string color = string.Empty;

            foreach(var key in request.QueryString.AllKeys)
            {
                builder.Append(string.Format("<br/>Key: {0} - Value: {1}", key, request.QueryString.Get(key)));
            }

            // Return a random color so the board will toggle the appropriate LED
            int random = new Random().Next(0, 2);
            if(random > 0)
            {
                color = "blue";
            }
            else
            {
                color = "green";
            }
            Console.WriteLine(string.Format("Web server is telling the board to turn on the {0} light.", color));

            return string.Format("<HTML><BODY>Query string:{0}<br />{1}</BODY></HTML>", builder.ToString(), color);
        }
    }
}
