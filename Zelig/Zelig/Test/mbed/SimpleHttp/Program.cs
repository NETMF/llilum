//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define K64F

namespace Microsoft.Zelig.Test.mbed.SimpleNet
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Microsoft.Llilum.Lwip;

    using System;
    using System.IO;
    using System.Text;

    class Program
    {
        static void Main( )
        {
            NetworkInterface netif = NetworkInterface.GetAllNetworkInterfaces()[0];
            netif.EnableDhcp( );

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(@"http://www.posttestserver.com/post.php");
            webReq.Method = "POST";

            UTF8Encoding enc = new UTF8Encoding();
            var data = UTF8Encoding.UTF8.GetBytes("Hello, World!"); 

            webReq.ContentType = "application/text";
            webReq.ContentLength = data.Length;
            
            var dataStream = webReq.GetRequestStream();

            dataStream.Write( data, 0, data.Length );
            dataStream.Close( );

            var response = webReq.GetResponse();
           
            Console.WriteLine( (((HttpWebResponse)response).StatusDescription) );

            var respData = response.GetResponseStream();
            var reader = new StreamReader(dataStream);

            string responseFromServer = reader.ReadToEnd();

            // Display the content.
            Console.WriteLine( responseFromServer );

            // Clean up the streams.
            reader.Close( );
            dataStream.Close( );
            response.Close( );
        }
    }
}
