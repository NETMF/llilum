//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class SynchronousSocketListener
    {
        internal class StringsBuffer
        {
            // Incoming data from the client.
            private List<string> m_data;

            internal StringsBuffer( )
            {
                m_data = new List<string>( );
            }

            internal void Add( string s )
            {
                m_data.Add( s );
            }

            internal List<String> Data
            {
                get
                {
                    return m_data;
                }
            }
        }

        //--//

        internal static int StartListening( StringsBuffer data )
        {
            //
            // Establish the local endpoint for the socket. 
            //
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            //
            // Pick 1st IPv4 address in the list. 
            //
            IPAddress ipv4Local = null;
            foreach(var addr in ipHostInfo.AddressList)
            {
                if(addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipv4Local = addr;

                    break;
                }
            }

            //
            // Create a TCP/IP socket.
            //
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            int bytesReceived = 0;
            try
            {
                var localEndPoint = new IPEndPoint(ipv4Local, 11000);

                listener.Bind( localEndPoint );
                listener.Listen( 10 );
                Socket handler = null;

                //
                // Start listening for connections.
                //
                int connection    = 1;
                while(true)
                {
                    Console.WriteLine( String.Format( "========= Waiting on {0} for connection #{1} =========", localEndPoint.ToString( ), connection++ ) );

                    handler = listener.Accept( );

                    Console.WriteLine( String.Format( " => => => => => => incoming connection from {0} !", handler.LocalEndPoint.ToString( ) ) );

                    //Task.Run( new Action( delegate
                    //{
                        try
                        {
                            handler.ReceiveTimeout = 10 * 1000; // ten seconds timeout

                            int count = 0;

                            var buffer = new byte[ 1024 ];

                            // An incoming connection needs to be processed.
                            while(true)
                            {
                                ++count;

                                Console.WriteLine( String.Format( "Performing read #{0:D4} from '{1}'. Bytes Received so far: {2}",
                                    count,
                                    handler.RemoteEndPoint.ToString( ),
                                    bytesReceived
                                    ) );

                                int bytesRead = handler.Receive(buffer);

                                bytesReceived += bytesRead;

                                var moreData = Encoding.ASCII.GetString( buffer, 0, bytesRead );

                                data.Add( moreData );

                                //
                                // Echo data back
                                //
                                handler.Send( buffer, 0, bytesReceived, SocketFlags.None );

                                if(moreData.IndexOf( "TEST_COMPLETED" ) != -1)
                                {
                                    Console.WriteLine( "" );
                                    Console.WriteLine( String.Format( "<= <= <= <= <= <= {0} completed", handler.RemoteEndPoint.ToString() ) );

                                    break;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine( "" );
                            Console.WriteLine( String.Format( "<= <= <= <= <= <= EXCEPTION CAUGHT on {0}", handler.RemoteEndPoint.ToString() ) );
                            Console.WriteLine( ex.StackTrace );
                        }
                        finally
                        {
                            handler.Close( );
                        }
                    //} )
                    //);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine( e.ToString( ) );
            }

            Console.WriteLine( "\nPress ENTER to continue..." );
            Console.Read( );

            return bytesReceived;
        }

        void Accumulate( byte[] buffer )
        {

        }

        //--//

        public static int Main( String[] args )
        {
            var allData = new StringsBuffer();

            return StartListening( allData );
        }
    }
}
