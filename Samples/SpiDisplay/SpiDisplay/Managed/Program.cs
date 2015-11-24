//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Test.mbed.Simple
{
    using Windows.Devices.Adc;
    using Windows.Devices.Gpio;
    using Windows.Devices.Spi;
    using Windows.Devices.Enumeration;

    // This is our board-specific assembly
    using LPC1768;

    /// <summary>
    /// This is a sample program to show the functionality of the SPI controller implementation. This demonstrates the power of
    /// writing managed drivers, and the ability to interact with C functions through managed code. This demo is intended to
    /// be run on a LPC1768 board on the mBed application board (https://developer.mbed.org/cookbook/mbed-application-board)
    /// </summary>
    unsafe class Program
    {
        static AdcChannel adcChannel;
        static SpiDevice spiDevice;
        static GpioPin _A0, _reset, _CS;

        // This is the figure we will be drawing
        static byte[] stickFigure = new byte[]
        {0x00, 0x00, 0xF0, 0x9C, 0x06, 0xC2, 0x93, 0x01, 0x01, 0x93, 0xC2, 0x06, 0x9C, 0xF0, 0x00, 0x00,
        0x10, 0x10, 0x20, 0x23, 0x46, 0x44, 0x8C, 0xF9, 0xF9, 0x8C, 0x44, 0x46, 0x23, 0x20, 0x10, 0x10,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x7F, 0x7F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x80, 0x60, 0x18, 0x06, 0x01, 0x00, 0x00, 0x01, 0x06, 0x18, 0x60, 0x80, 0x00, 0x00};

        static byte[] writeHelper;

        static void Main()
        {
            var controller = GpioController.GetDefault();

            // Used to pass data to SPI device
            writeHelper = new byte[1];

            _A0 = controller.OpenPin((int)PinName.p8);
            _A0.SetDriveMode(GpioPinDriveMode.Output);

            _reset = controller.OpenPin((int)PinName.p6);

            // Start reset high
            _reset.Write(GpioPinValue.High);
            _reset.SetDriveMode(GpioPinDriveMode.Output);

            _CS = controller.OpenPin((int)PinName.p11);
            _CS.Write(GpioPinValue.High);
            _CS.SetDriveMode(GpioPinDriveMode.Output);

            // Analog pin init. This maps to LPC1768 p19
            adcChannel = AdcController.GetDefaultAsync().OpenChannel(4);

            // Note: The LCD display uses a non-standard SPI configuration. This is why we
            // have added a copy of the LPC1768 project, and changed the configuration for SPI0!

            // Get the device selector by friendly name
            string deviceSelector = SpiDevice.GetDeviceSelector("SPI0");
            var acqs = DeviceInformation.FindAllAsync(deviceSelector);
            string busId = acqs[0].Id;

            // Set up a non-default frequency and pins
            int chipSelect = unchecked((int)PinName.NC);

            SpiConnectionSettings settings = new SpiConnectionSettings(chipSelect)
            {
                ClockFrequency = 20000000,
                Mode = SpiMode.Mode3
            };

            // Get a reference to the SPI device
            spiDevice = SpiDevice.FromIdAsync(busId, settings);

            // Initialize the LCD display
            lcd_reset();

            int width = 16;
            while (true)
            {
                int analogVal = (int)(adcChannel.ReadRatio() * 100);
                if (analogVal > 112)
                {
                    analogVal = 112;
                }
                else if (analogVal < 0)
                {
                    analogVal = 0;
                }

                copy_to_lcd(analogVal, analogVal + width);
            }
        }

        /// <summary>
        /// LCD Initializer
        /// </summary>
        private static void lcd_reset()
        {
            _A0.Write(GpioPinValue.Low);
            _CS.Write(GpioPinValue.High);
            _reset.Write(GpioPinValue.Low);

            for (int i = 0; i < 500; i++)
            {
                // Wait 50ms
            }

            // Try to force low...
            _reset.Write(GpioPinValue.High);

            for (int i = 0; i < 50; i++)
            {
                // Wait 5ms
            }

            wr_cmd(0xAE);   //  display off
            wr_cmd(0xA2);   //  bias voltage

            wr_cmd(0xA0);
            wr_cmd(0xC8);   //  column normal

            wr_cmd(0x22);   //  voltage resistor ratio
            wr_cmd(0x2F);   //  power on
                            //wr_cmd(0xA4);   //  LCD display ram
            wr_cmd(0x40);   // start line = 0
            wr_cmd(0xAF);     // display ON

            wr_cmd(0x81);   //  set contrast
            wr_cmd(0x17);   //  set contrast

            wr_cmd(0xA6);     // display normal

            copy_to_lcd(-1, -1);
        }

        /// <summary>
        /// Write a single byte command to the LCD display
        /// </summary>
        /// <param name="a"></param>
        private static void wr_cmd(byte a)
        {
            writeHelper[0] = a;
            _A0.Write(GpioPinValue.Low);
            _CS.Write(GpioPinValue.Low);
            spiDevice.Write(writeHelper);
            _CS.Write(GpioPinValue.High);
        }

        /// <summary>
        /// Write a single byte data to the LCD display
        /// </summary>
        /// <param name="a"></param>
        private static void wr_dat(byte a)
        {
            writeHelper[0] = a;
            _A0.Write(GpioPinValue.High);
            _CS.Write(GpioPinValue.Low);
            spiDevice.Write(writeHelper);
        }

        /// <summary>
        /// Flush the LCD display. Draw a figure from startIndex to stopIndex.
        /// </summary>
        /// <param name="startIndex">Begin drawing</param>
        /// <param name="stopIndex">End drawing</param>
        private static void copy_to_lcd(int startIndex, int stopIndex)
        {
            int i = 0;
            int j = 0;

            //page 0
            wr_cmd(0x00);      // set column low nibble 0
            wr_cmd(0x10);      // set column hi  nibble 0
            wr_cmd(0xB0);      // set page address  0
            _A0.Write(GpioPinValue.High);

            for (i = 0; i < 128; i++)
            {
                if (i >= startIndex && i < stopIndex)
                {
                    wr_dat(stickFigure[j]);
                    j++;
                }
                else
                {
                    wr_dat(0x00);
                }
            }

            // page 1
            wr_cmd(0x00);      // set column low nibble 0
            wr_cmd(0x10);      // set column hi  nibble 0
            wr_cmd(0xB1);      // set page address  1
            _A0.Write(GpioPinValue.High);
            for (i = 0; i < 128; i++)
            {
                if (i >= startIndex && i < stopIndex)
                {
                    wr_dat(stickFigure[j]);
                    j++;
                }
                else
                {
                    wr_dat(0x00);
                }
            }

            //page 2
            wr_cmd(0x00);      // set column low nibble 0
            wr_cmd(0x10);      // set column hi  nibble 0
            wr_cmd(0xB2);      // set page address  2
            _A0.Write(GpioPinValue.High);
            for (i = 0; i < 128; i++)
            {
                if (i >= startIndex && i < stopIndex)
                {
                    wr_dat(stickFigure[j]);
                    j++;
                }
                else
                {
                    wr_dat(0x00);
                }
            }

            //page 3
            wr_cmd(0x00);      // set column low nibble 0
            wr_cmd(0x10);      // set column hi  nibble 0
            wr_cmd(0xB3);      // set page address  3
            _A0.Write(GpioPinValue.High);

            // Why is this here?
            //_CS.Write(GpioPinValue.Low);

            for (i = 0; i < 128; i++)
            {
                if (i >= startIndex && i < stopIndex)
                {
                    wr_dat(stickFigure[j]);
                    j++;
                }
                else
                {
                    wr_dat(0x00);
                }
            }
        }

    }
}
