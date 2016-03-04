//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define LPC1768_FOR_GPIO_TOGGLING_PERF


namespace Microsoft.Zelig.Test.mbed.Simple
{
    using System;

    using LPC1768 = Llilum.LPC1768;


    partial class Program
    {
        private static void TestGpioPerf()
        {
#if LPC1768_FOR_GPIO_TOGGLING_PERF
            using(var pinOut = Runtime.GpioProvider.Instance.CreateGpioPin( (int)LPC1768.PinName.p28 ))
            {
                pinOut.Direction = Llilum.Devices.Gpio.PinDirection.Output;
                pinOut.Mode      = Llilum.Devices.Gpio.PinMode.Default;

                for(int i = 0; i<0x1000; i++)
                {
                    pinOut.Write( 0 );
                    pinOut.Write( 1 );
                }
            }
#endif // LPC1768_FOR_GPIO_TOGGLING_PERF
        }

    }
}
