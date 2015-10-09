//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using SPI_FRAMEWORK     = Windows.Devices.Spi.Provider;
    using GPIO_FRAMEWORK    = Windows.Devices.Gpio.Provider;
    using I2C_FRAMEWORK     = Windows.Devices.I2c.Provider;
    using Chipset           = Microsoft.CortexM3OnCMSISCore;
    using ChipsetAbstration = Microsoft.DeviceModels.Chipset.CortexM3;


    public class HardwareProvider : Chipset.HardwareProvider
    {
        //
        // Gpio Discovery 
        //
        public override int PinCount => Board.Instance.PinCount;

        public override int PinToIndex(int pin)
        {
            return Board.Instance.PinToIndex(pin);
        }

        public override GPIO_FRAMEWORK.GpioPinProvider CreateGpioPin()
        {
            return new GpioPinMbed();
        }

        public override int InvalidPin
        {
            get
            {
                return Board.Instance.NCPin;
            }
        }

        //
        // Spi
        //

        public override int GetSpiChannelIndexFromString(string busId)
        {
            return Board.Instance.GetSpiChannelIndexFromString(busId);
        }

        public override bool GetSpiPinsFromBusId(int id, out int mosi, out int miso, out int sclk, out int chipSelect)
        {
            ChipsetAbstration.Board.SpiChannelInfo channelInfo = Board.Instance.GetSpiChannelInfo(id);
            if (channelInfo == null)
            {
                // The spi bus was not found
                int invalidPin = this.InvalidPin;
                mosi = invalidPin;
                miso = invalidPin;
                sclk = invalidPin;
                chipSelect = invalidPin;

                return false;
            }

            mosi = channelInfo.Mosi;
            miso = channelInfo.Miso;
            sclk = channelInfo.Sclk;
            chipSelect = channelInfo.ChipSelect;

            return true;
        }

        public override bool GetSpiChannelInfo(int id, out int csLineCount, out int maxFreq, out int minFreq, out bool supports16)
        {
            ChipsetAbstration.Board.SpiChannelInfo channelInfo = Board.Instance.GetSpiChannelInfo(id);
            if (channelInfo == null)
            {
                // The bus was not found
                csLineCount = -1;
                maxFreq = -1;
                minFreq = -1;
                supports16 = false;

                return false;
            }

            csLineCount = channelInfo.ChipSelectLines;
            maxFreq = channelInfo.MaxFreq;
            minFreq = channelInfo.MinFreq;
            supports16 = channelInfo.Supports16;

            return true;
        }

        public override bool GetSpiChannelActiveLow(int id, out bool activeLow)
        {
            ChipsetAbstration.Board.SpiChannelInfo channelInfo = Board.Instance.GetSpiChannelInfo(id);
            if (channelInfo == null)
            {
                activeLow = true;
                return false;
            }

            activeLow = channelInfo.ActiveLow;
            return true;
        }

        public override string[] GetSpiChannels()
        {
            return Board.Instance.GetSpiChannels();
        }

        public override SPI_FRAMEWORK.SpiChannel CreateSpiChannel()
        {
            return new SpiChannel();
        }

        public override bool GetSpiChannelTimingInfo(int id, out int setupTime, out int holdTime)
        {
            ChipsetAbstration.Board.SpiChannelInfo channelInfo = Board.Instance.GetSpiChannelInfo(id);
            if (channelInfo == null)
            {
                // Fail. Returning 0 because time should not be negative
                setupTime = 0;
                holdTime = 0;
                return false;
            }

            setupTime = channelInfo.SetupTime;
            holdTime = channelInfo.HoldTime;
            return true;
        }

        //
        // I2C Discovery
        //

        public override string[] GetI2CChannels()
        {
            return Board.Instance.GetI2cChannels();
        }

        public override int GetI2cChannelIndexFromString(string busId)
        {
            return Board.Instance.GetI2cChannelIndexFromString(busId);
        }

        public override bool GetI2CPinsFromChannelIndex(int index, out int sdaPin, out int sclPin)
        {
            ChipsetAbstration.Board.I2cChannelInfo channelInfo = Board.Instance.GetI2cChannelInfo(index);
            if (channelInfo == null)
            {
                // Channel does not exist
                int invalidPin = this.InvalidPin;
                sdaPin = invalidPin;
                sclPin = invalidPin;
                return false;
            }

            sdaPin = channelInfo.SdaPin;
            sclPin = channelInfo.SclPin;
            return true;
        }

        //
        // I2C Creation
        //

        public override I2C_FRAMEWORK.I2cChannel CreateI2cChannel()
        {
            return new I2cChannelMbed();
        }

        //
        // Serial Discovery
        //

        public override string[] GetSerialPorts()
        {
            return Board.Instance.GetSerialPorts();
        }

        public override bool GetSerialPinsFromPortName(string portName, out int txPin, out int rxPin, out int rtsPin, out int ctsPin)
        {
            ChipsetAbstration.Board.SerialPortInfo portInfo = Board.Instance.GetSerialPortInfo(portName);
            if (portInfo == null)
            {
                int invalidPin = this.InvalidPin;
                txPin = invalidPin;
                rxPin = invalidPin;
                ctsPin = invalidPin;
                rtsPin = invalidPin;
                return false;
            }

            txPin =  portInfo.TxPin;
            rxPin =  portInfo.RxPin;
            ctsPin = portInfo.CtsPin;
            rtsPin = portInfo.RtsPin;
            return true;
        }
    }
}
