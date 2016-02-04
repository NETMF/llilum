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
    using Llilum.Devices.Gpio;


    class Program
    {
        static void Main( )
        {
            NetworkInterface netif = NetworkInterface.GetAllNetworkInterfaces()[0];
            netif.EnableDhcp( );

            GpioPin redPin = GpioPin.TryCreateGpioPin((int)Llilum.K64F.PinName.LED_RED);
            GpioPin greenPin = GpioPin.TryCreateGpioPin((int)Llilum.K64F.PinName.LED_GREEN);
            GpioPin bluePin = GpioPin.TryCreateGpioPin((int)Llilum.K64F.PinName.LED_BLUE);

            redPin.Direction = PinDirection.Output;
            greenPin.Direction = PinDirection.Output;
            bluePin.Direction = PinDirection.Output;

            // Complete 10 HTTP requests
            for(int i = 0; i < 10; i++)
            {
                // LED is active low on K64F
                // Red means we have not gotten our request back
                redPin.Write(0);
                greenPin.Write(1);
                bluePin.Write(1);

                // Create the HTTP request
                // NOTE: Users need to replace this with their server IP address!
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(@"http://10.91.68.176:8080");
                webReq.Method = "POST";

                var data = UTF8Encoding.UTF8.GetBytes(string.Format("I am K64F. Request number: {0}", i));
                webReq.ContentType = "application/text";
                webReq.ContentLength = data.Length;

                // Write the request
                var dataStream = webReq.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();

                BugCheck.Log("------ start ------");

                // Receive the response
                var response = webReq.GetResponse();

                BugCheck.Log("====================");
                BugCheck.Log((((HttpWebResponse)response).StatusDescription));
                BugCheck.Log("====================");
                BugCheck.Log(response.ContentLength.ToString());
                BugCheck.Log("====================");
                BugCheck.Log(response.ContentType);
                BugCheck.Log("====================");

                // Getting data from POST request is not standard, but is still allowed
                var respData = response.GetResponseStream();

                // Turn off the red LED once the request comes back
                redPin.Write(1);

                var reader = new StreamReader(respData);
                string responseFromServer = reader.ReadToEnd();

                // Check to see if the result contains a color
                if (responseFromServer.Contains("green"))
                {
                    greenPin.Write(0);
                }
                else if (responseFromServer.Contains("blue"))
                {
                    bluePin.Write(0);
                }
                else
                {
                    // No color in result? Make the LED show white
                    redPin.Write(0);
                    greenPin.Write(0);
                    bluePin.Write(0);
                }

                // Display the content.
                BugCheck.Log(responseFromServer);

                // Clean up the streams.
                reader.Close();
                response.Close();

                webReq.Dispose();

                // Wait 2 seconds until the next transaction
                Thread.Sleep(2000);
            }
        }
    }
}
