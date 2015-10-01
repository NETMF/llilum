// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: SerialError
**
** Purpose: Describes the types of serial port errors.
**
** Date:  August 2002
**
**
===========================================================*/

namespace System.IO.Ports
{
    public enum SerialError
    {
        TXFull   = 0x100, // NativeMethods.CE_TXFULL,
        RXOver   = 0x001, // NativeMethods.CE_RXOVER,
        Overrun  = 0x002, // NativeMethods.CE_OVERRUN,
        RXParity = 0x004, // NativeMethods.CE_PARITY,
        Frame    = 0x008, // NativeMethods.CE_FRAME,
    }

    public class SerialErrorReceivedEventArgs : EventArgs
    {
        private SerialError errorType;

        internal SerialErrorReceivedEventArgs( SerialError eventCode )
        {
            errorType = eventCode;
        }

        public SerialError EventType
        {
            get { return errorType; }
        }
    }

    public delegate void SerialErrorReceivedEventHandler( object sender, SerialErrorReceivedEventArgs e );
}

