//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Managed
{
    using System.Collections.Generic;
    using Windows.Devices.Gpio;
    using Microsoft.Zelig.Support.mbed;
    using Windows.Devices.I2c;
    using Windows.Devices.Enumeration;

    // This is our board-specific assembly
    using Microsoft.Zelig.LPC1768;

    class Program
    {
        // Temperature boundaries for which pin to light up
        const double TEMP0 = 29;
        const double TEMP1 = 29.5;
        const double TEMP2 = 30;
        const double TEMP3 = 30.5;

        // This array will only have byte 0x0 in it
        public static byte[] i2cReadWrite1 = new byte[] { 0x0 };
        public static byte[] i2cReadWrite2 = new byte[2];

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

        static void Main()
        {
            

            var controller = GpioController.GetDefault();
            var pins = new GpioPin[pinNumbers.Length];

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

            while (true)
            {
                double temp = ReadTemp(i2cDevice);
                if (temp < TEMP0)
                {
                    pins[0].Write(GpioPinValue.Low);
                    pins[1].Write(GpioPinValue.Low);
                    pins[2].Write(GpioPinValue.Low);
                    pins[3].Write(GpioPinValue.Low);
                }
                else if (temp >= TEMP0 && temp < TEMP1)
                {
                    pins[0].Write(GpioPinValue.High);
                    pins[1].Write(GpioPinValue.Low);
                    pins[2].Write(GpioPinValue.Low);
                    pins[3].Write(GpioPinValue.Low);
                }
                else if (temp >= TEMP1 && temp < TEMP2)
                {
                    pins[0].Write(GpioPinValue.High);
                    pins[1].Write(GpioPinValue.High);
                    pins[2].Write(GpioPinValue.Low);
                    pins[3].Write(GpioPinValue.Low);
                }
                else if (temp >= TEMP2 && temp < TEMP3)
                {
                    pins[0].Write(GpioPinValue.High);
                    pins[1].Write(GpioPinValue.High);
                    pins[2].Write(GpioPinValue.High);
                    pins[3].Write(GpioPinValue.Low);
                }
                else
                {
                    pins[0].Write(GpioPinValue.High);
                    pins[1].Write(GpioPinValue.High);
                    pins[2].Write(GpioPinValue.High);
                    pins[3].Write(GpioPinValue.High);
                }
            }
        }
    }
}
