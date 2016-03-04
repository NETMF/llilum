//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TEST_ADC


using Windows.Devices.Adc;

namespace Microsoft.Zelig.Test.mbed.Simple
{
    internal partial class Program
    {
        private static AdcController s_adcController;
        private static AdcChannel    s_adcChannel;
        private static bool          s_adcInitialized = false;

        //--//

        private static float TestADC()
        {
#if TEST_ADC 
            if(s_adcInitialized == false)
            {
                s_adcController = AdcController.GetDefaultAsync();

                // This is the left potentiometer on the mBed application board
                s_adcChannel = s_adcController.OpenChannel(4);

                s_adcInitialized = true;
            }

            return ( (float)s_adcChannel.ReadValue( ) ) / s_adcController.MaxValue * 2; 
#else
            return 0;
#endif
        }
    }
}
