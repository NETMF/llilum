//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class SynchronousSocketListener
    {
        // Incoming data from the client.
        public static string data = null;

        public static void StartListening( )
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ipv4Local = null;
            foreach( var addr in ipHostInfo.AddressList)
            {
                if(addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipv4Local = addr;

                    break;
                }
            }

            IPHostEntry ipHostInfo2 = Dns.GetHostEntry("bing.com");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipv4Local, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind( localEndPoint );
                listener.Listen( 10 );
                Socket handler = null;


                // Start listening for connections.
                while(true)
                {
                    Console.WriteLine( "Waiting for a connection..." );

                    handler = listener.Accept( );

                    Console.WriteLine( "... incoming connection for socket:" + handler.LocalEndPoint.ToString( ) );

                    Task.Run( new Action( delegate
                    {
                        handler.ReceiveTimeout = 10 * 1000; // ten seconds timeout
                        
                        int count = 0;

                        while(++count <= 10)
                        {
                            // An incoming connection needs to be processed.
                            while(true)
                            {
                                Console.WriteLine( String.Format( "Performing read #{0:D2} from '{1}'", handler.LocalEndPoint.ToString( ) ) ); 

                                bytes = new byte[ 1024 ];

                                int bytesRec = handler.Receive(bytes);

                                Console.WriteLine( "... read " + bytesRec + " bytes" );

                                var moreData = Encoding.ASCII.GetString( bytes, 0, bytesRec );

                                Console.WriteLine( "received data: " + moreData );

                                data += moreData;

                                if(data.IndexOf( "\n" ) > -1)
                                {
                                    break;
                                }
                            }

                            // Show the data on the console.
                            Console.WriteLine( "Text received : {0}", data );

                            // Echo the data back to the client.
                            byte[] msg = Encoding.ASCII.GetBytes(data);

                            Console.WriteLine( "... sending to: " + handler.LocalEndPoint.ToString( ) );

                            handler.Send( msg );
                        }

                        handler.Close( );
                    } )
                    );
                }
            }
            catch(Exception e)
            {
                Console.WriteLine( e.ToString( ) );
            }

            Console.WriteLine( "\nPress ENTER to continue..." );
            Console.Read( );

        }

        public static int Main( String[] args )
        {
            StartListening( );
            return 0;
        }

    }
}