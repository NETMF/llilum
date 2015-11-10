//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.Devices.SerialCommunication
{
    public sealed class PinChangedEventArgs
    {
        public PinChangedEventArgs(SerialPinChange pinChange)
        {
            PinChange = pinChange;
        }

        public SerialPinChange PinChange
        {
            get;
        }
    }

    public sealed class ErrorReceivedEventArgs
    {
        public ErrorReceivedEventArgs(SerialError error)
        {
            Error = error;
        }

        public SerialError Error
        {
            get;
        }
    }
}
