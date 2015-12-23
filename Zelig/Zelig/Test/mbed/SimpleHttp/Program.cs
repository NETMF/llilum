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
    using Microsoft.Zelig.Runtime;

    using System;
    using System.IO;
    using System.Text;

    class Program
    {
        static void Main( )
        {
            NetworkInterface netif = NetworkInterface.GetAllNetworkInterfaces()[0];
            netif.EnableDhcp( );

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(@"http://httpbin.org/get");
            //webReq.Method = "POST";

            //UTF8Encoding enc = new UTF8Encoding();
            //var data = UTF8Encoding.UTF8.GetBytes("Hello, World!"); 

            //webReq.ContentType = "application/text";
            //webReq.ContentLength = data.Length;
            
            //var dataStream = webReq.GetRequestStream();

            //dataStream.Write( data, 0, data.Length );
            //dataStream.Close( );
            
            BugCheck.Log( "------ start ------" );

            var response = webReq.GetResponse();
           
            BugCheck.Log( "====================" );
            BugCheck.Log( (((HttpWebResponse)response).StatusDescription) );
            BugCheck.Log( "====================" );
            BugCheck.Log( response.ContentLength.ToString() );
            BugCheck.Log( "====================" );
            BugCheck.Log( response.ContentType );
            BugCheck.Log( "====================" );
            BugCheck.Log( response.ToString() );
            BugCheck.Log( "====================" );

            var respData = response.GetResponseStream();
            var reader = new StreamReader(respData);

            string responseFromServer = reader.ReadToEnd();

            // Display the content.
            BugCheck.Log( responseFromServer );

            // Clean up the streams.
            reader.Close( );
            //dataStream.Close( );
            response.Close( );
        }
    }
}
