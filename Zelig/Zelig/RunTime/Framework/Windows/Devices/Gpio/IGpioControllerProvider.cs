namespace Windows.Devices.Gpio.Provider
{
    public interface IGpioControllerProvider
    {
        int PinCount
        {
            get;
        }

        bool AllocatePin(int pinNumber);
        void ReleasePin(int pinNumber);

        GpioPinValue Read(int pinNumber);
        void Write(int pinNumber, GpioPinValue Value);

        void SetPinDriveMode(int pinNumber, GpioPinDriveMode driveMode);
    }
}
