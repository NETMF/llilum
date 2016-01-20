


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
    using System.Threading;
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
                var localEndPoint = new IPEndPoint( ipv4Local, 11000);

                listener.Bind( localEndPoint );
                listener.Listen( 10 );
                Socket handler = null;

                StringBuilder builder = new StringBuilder();

                //
                // Start listening for connections.
                //
                int connection = 1;
                while(true)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = NextColor;
                    Console.Write( String.Format( "Waiting on {0} for connection #{1}... ", localEndPoint.ToString( ), connection++ ) );

                    handler = listener.Accept( );

                    Console.Write( String.Format( "Connected to {0}\r\n", handler.LocalEndPoint.ToString( ) ) );

                    //Task.Run( new Action( delegate
                    //{
                    try
                    {
                        handler.ReceiveTimeout = 120 * 1 * 1000; // ten seconds timeout

                        int count = 0;

                        var buffer = new byte[ 1024 ];

                        // An incoming connection needs to be processed.
                        while(true)
                        {
                            ++count;

                            Console.Write( String.Format( "Performing read #{0:D4} from '{1}'. Bytes Received so far: {2:D8}\r",
                                count,
                                handler.RemoteEndPoint.ToString( ),
                                bytesReceived
                                ) );

                            int bytesRead = handler.Receive(buffer);

                            bytesReceived += bytesRead;

                            var moreData = Encoding.ASCII.GetString( buffer, 0, bytesRead );

                            builder.Append( moreData );

                            string finalString = builder.ToString();

                            // Ensure we got the return char terminated request
                            if(finalString.IndexOf( "\n" ) != -1)
                            {
                                builder.Clear( );

                                data.Add( finalString );
                            }

                            //
                            // Echo data back
                            //
                            handler.Send( buffer, 0, bytesRead, SocketFlags.None );

                            if(finalString.IndexOf( "TEST_COMPLETED" ) != -1)
                            {
                                Console.Write( String.Format( "Performing read #{0:D4} from '{1}'. Bytes Received so far: {2:D8}\r\n",
                                    count,
                                    handler.RemoteEndPoint.ToString( ),
                                    bytesReceived
                                    ) );

                                break;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine( "" );
                        Console.WriteLine( String.Format( "EXCEPTION CAUGHT on {0}", handler.RemoteEndPoint.ToString( ) ) );
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

        //--//

        static readonly ConsoleColor[] s_colors = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Cyan };
        static int s_currentColorIndex = -1;
        public static ConsoleColor NextColor
        {
            get
            {
                return s_colors[ Interlocked.Increment( ref s_currentColorIndex ) % s_colors.Length ];
            }
        }

        //--//

        public static int Main( String[] args )
        {
            var allData = new StringsBuffer();

            return StartListening( allData );
        }
    }
}
