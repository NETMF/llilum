//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Windows.Devices.SerialCommunication
{
    public enum SerialError
    {
        Frame = 0,
        BufferOverrun,
        ReceiveFull,
        ReceiveParity,
        TransmitFull,
    }

    public enum SerialHandshake
    {
        None = 0,
        RequestToSend,
        XOnXOff,
        RequestToSendXOnXOff,
    }

    public enum SerialParity
    {
        None = 0,
        Odd,
        Even,
        Mark,
        Space,
    }

    public enum SerialPinChange
    {
        BreakSignal = 0,
        CarrierDetect,
        ClearToSend,
        DataSetReady,
        RingIndicator,
    }

    public enum SerialStopBitCount
    {
        One = 1,
        Two,
        OnePointFive,
    }
}
