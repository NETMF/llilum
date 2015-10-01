// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: SerialPinChange
**
** Purpose: Used to describe which pin has changed on a PinChanged event.
**
** Date:  August 2002
**
**
===========================================================*/

namespace System.IO.Ports
{
    public enum SerialPinChange
    {
        CtsChanged = 0x008, // NativeMethods.EV_CTS,
        DsrChanged = 0x010, // NativeMethods.EV_DSR,
        CDChanged  = 0x020, // NativeMethods.EV_RLSD,
        Ring       = 0x100, // NativeMethods.EV_RING,
        Break      = 0x040, // NativeMethods.EV_BREAK,
    }

    public class SerialPinChangedEventArgs : EventArgs
    {
        private SerialPinChange pinChanged;

        internal SerialPinChangedEventArgs( SerialPinChange eventCode )
        {
            pinChanged = eventCode;
        }

        public SerialPinChange EventType
        {
            get { return pinChanged; }
        }
    }

    public delegate void SerialPinChangedEventHandler( object sender, SerialPinChangedEventArgs e );
}

