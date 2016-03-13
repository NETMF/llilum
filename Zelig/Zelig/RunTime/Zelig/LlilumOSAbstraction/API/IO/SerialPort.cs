//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Zelig.Runtime;


    public enum SerialPortParity : uint
    {
        None = 0,
        Odd,
        Even,
        Mark,
        Space,
    };

    public enum SerialPortStopBits : uint
    {
        None = 0,
        One,
        Two,
        OnePointFive
    };

    //
    // !!!WARNING!!! This structure MUST be identical to the C structure LLOS_SERIAL_Config in llos_serial.h
    //
    [StructLayout( LayoutKind.Sequential, Pack = 4 )]
    public struct SerialPortConfiguration
    {
        public uint BaudRate;
        public SerialPortParity Parity;
        public uint DataBits;
        public SerialPortStopBits StopBits;
        public uint SoftwareFlowControlValue;
    };

    public enum SerialPortEvent
    {
        Rx = 0,
        Tx,
    };

    public enum SerialPortIrq
    {
        IrqRx = 0,
        IrqTx,
        IrqBoth
    };

    public unsafe delegate void SerialPortInterruptHandler(SerialPortContext* port, UIntPtr callbackCtx, SerialPortEvent serialEvent);

    public static class SerialPort
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Open(int rxPin, int txPin, SerialPortConfiguration **ppConfiguration, SerialPortContext** pChannel);

        [DllImport( "C" )]
        public static unsafe extern void LLOS_SERIAL_Close(SerialPortContext* channel);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Enable(SerialPortContext* channel, SerialPortIrq irq);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Disable(SerialPortContext* channel, SerialPortIrq irq);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_SetFlowControl(SerialPortContext* channel, int rtsPin, int ctsPin);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Configure(SerialPortContext* channel, SerialPortConfiguration* pConfiguration);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Read(SerialPortContext* channel, byte* pBuffer, int offset, int* pLength);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Write(SerialPortContext* channel, byte* pBuffer, int offset, int length);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Flush(SerialPortContext* channel);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_Clear(SerialPortContext* channel);

        public static unsafe uint LLOS_SERIAL_SetCallback(SerialPortContext* channel, SerialPortInterruptHandler callback, UIntPtr callbackContext)
        {
            UIntPtr callbackPtr = UIntPtr.Zero;

            if(callback != null)
            {
                DelegateImpl dlg = (DelegateImpl)(object)callback;

                callbackPtr = new UIntPtr( dlg.InnerGetCodePointer( ).Target.ToPointer( ) );
            }

            return LLOS_SERIAL_SetCallback( channel, callbackPtr, callbackContext );
        }

        [DllImport( "C" )]
        private static unsafe extern uint LLOS_SERIAL_SetCallback(SerialPortContext* channel, UIntPtr callback, UIntPtr callbackContext);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_CanRead(SerialPortContext* channel, uint *pCanRead);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_SERIAL_CanWrite(SerialPortContext* channel, uint *pCanWrite);
    }

    public unsafe struct SerialPortContext
    {
    };
}
