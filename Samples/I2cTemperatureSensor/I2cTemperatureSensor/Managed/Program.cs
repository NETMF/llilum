//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Managed
{
    using Windows.Devices.Gpio;
    using Windows.Devices.I2c;
    using Windows.Devices.Enumeration;

    // This is our board-specific assembly
    using Microsoft.Llilum.LPC1768;

    class Program
    {
        // This array will only have byte 0x0 in it
        public static byte[] i2cReadWrite1 = new byte[] { 0x0 };
        public static byte[] i2cReadWrite2 = new byte[] { 0x0, 0x0 };

        //
        // Set temperature range in order to filter bad readings.
        //
        private const double c_MinimumTemperature = 10.0;
        private const double c_MaximumTemperature = 50.0;

        // These are the pins on the LPC1768 dev board
        static int[] pinNumbers =
        {
            (int)PinName.LED1,
            (int)PinName.LED2,
            (int)PinName.LED3,
            (int)PinName.LED4,
        };

        /// <summary>
        /// Writes a command to the I2cDevice, then reads the result (temperature) and formats it
        /// </summary>
        /// <param name="device"></param>
        /// <returns>Temperature reading</returns>
        public static double ReadTemp(I2cDevice device)
        {
            double temp;
            i2cReadWrite1[0] = 0x0;
            I2cTransferResult res = device.WriteReadPartial(i2cReadWrite1, i2cReadWrite2);
            if (res.Status == I2cTransferStatus.SlaveAddressNotAcknowledged)
            {
                // Note: if you are running this sample where temp is < 0 degrees, you should probably relocate
                // The board is not connected
                temp = -1;
            }
            else
            {
                temp = ((i2cReadWrite2[0] << 8) | i2cReadWrite2[1]) / 256.0;
            }

            return temp;
        }

        private static double GetAveragedTemperature(int samples, I2cDevice i2cDevice)
        {
            double avg = 0.0;

            for(int i=0; i<samples; i++)
            {
                double tmp = 0.0;

                while(tmp < c_MinimumTemperature || tmp > c_MaximumTemperature)
                {
                    tmp = ReadTemp( i2cDevice );
                }

                avg += tmp;
            }

            avg /= samples;

            return avg;
        }

        static void Main()
        {
            var controller = GpioController.GetDefault();
            var pins = new GpioPin[pinNumbers.Length];
            const int c_SampleCount = 4;

            for (int i = 0; i < pinNumbers.Length; ++i)
            {
                GpioPin pin = controller.OpenPin(pinNumbers[i]);

                // Start with all LEDs off.
                pin.Write(GpioPinValue.Low);
                pin.SetDriveMode(GpioPinDriveMode.Output);

                pins[i] = pin;
            }

            // Keep the first pin on, unless we hit an error
            pins[0].Write(GpioPinValue.High);

            // Initialize the I2cDevice
            string i2cDeviceSelector = I2cDevice.GetDeviceSelector("I2C1");
            var i2cAcqs = DeviceInformation.FindAllAsync(i2cDeviceSelector);
            string i2cBusId = i2cAcqs[0].Id;

            I2cDevice i2cDevice = I2cDevice.FromIdAsync(i2cBusId, new I2cConnectionSettings(slaveAddress: 0x90)
            {
                SharingMode = I2cSharingMode.Shared
            });

            //
            // Get average starting temperature
            //
            double firstRead = GetAveragedTemperature(c_SampleCount, i2cDevice);
            const double scale = 0.5;

            while (true)
            {
                double tempDiff = GetAveragedTemperature(c_SampleCount, i2cDevice);
                tempDiff -= firstRead;

                pins[0].Write((tempDiff > 0 * scale) ? GpioPinValue.High : GpioPinValue.Low);
                pins[1].Write((tempDiff > 1 * scale) ? GpioPinValue.High : GpioPinValue.Low);
                pins[2].Write((tempDiff > 2 * scale) ? GpioPinValue.High : GpioPinValue.Low);
                pins[3].Write((tempDiff > 3 * scale) ? GpioPinValue.High : GpioPinValue.Low);
            }
        }
    }
}
