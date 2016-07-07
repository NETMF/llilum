// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: SerialStream
**
** Purpose: Class for enabling low-level sync and async control over a serial
**        : communications resource.
**
** Date: August, 2002
**
=============================================================================*/

// Notes about the SerialStream:
//  * The stream is always opened via the SerialStream constructor.
//  * The handleProtector guarantees ownership of the file handle, so that it may not be
//  * unnaturally closed by another process or thread.  Thus, since all properties are available
//  * only when the object exists, the object's properties can be queried only when the SerialStream
//  * object is instantiated (i.e. "open").
//  * Handles to serial communications resources here always:
//  * 1) own the handle
//  * 2) are opened for asynchronous operation
//  * 3) set access at the level of FileAccess.ReadWrite
//  * 4) Allow for reading AND writing
//  * 5) Disallow seeking, since they encapsulate a file of type FILE_TYPE_CHAR

namespace System.IO.Ports
{
    using System;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Resources;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Collections;
////using Microsoft.Win32;
////using Microsoft.Win32.SafeHandles;
    using System.Threading;
////using System.Runtime.Remoting.Messaging;
    using System.Runtime.CompilerServices;
    using System.Globalization;
////using System.Runtime.Versioning;

    public abstract class SerialStream : Stream
    {
////    internal static class NativeMethods
////    {
////        internal const int MAXDWORD = -1;   // note this is 0xfffffff, or UInt32.MaxValue, here used as an int
////
////        internal const byte DEFAULTXONCHAR  = 17;
////        internal const byte DEFAULTXOFFCHAR = 19;
////        internal const byte EOFCHAR         = 26;
////
////        // The following are unique to the SerialPort/SerialStream classes
////        internal const byte ONESTOPBIT = 0;
////        internal const byte ONE5STOPBITS = 1;
////        internal const byte TWOSTOPBITS = 2;
////
////        internal const int  MS_CTS_ON = 0x10;
////        internal const int  MS_DSR_ON = 0x20;
////        internal const int  MS_RING_ON = 0x40;
////        internal const int  MS_RLSD_ON  = 0x80;
////
////        internal const int EV_RXCHAR = 0x01;
////        internal const int EV_RXFLAG = 0x02;
////        internal const int EV_CTS = 0x08;
////        internal const int EV_DSR = 0x10;
////        internal const int EV_RLSD = 0x20;
////        internal const int EV_BREAK = 0x40;
////        internal const int EV_ERR = 0x80;
////        internal const int EV_RING = 0x100;
////        internal const int ALL_EVENTS = 0x1fb;  // don't use EV_TXEMPTY
////
////        internal const int PURGE_TXABORT     =  0x0001;  // Kill the pending/current writes to the comm port.
////        internal const int PURGE_RXABORT     =  0x0002;  // Kill the pending/current reads to the comm port.
////        internal const int PURGE_TXCLEAR     =  0x0004;  // Kill the transmit queue if there.
////        internal const int PURGE_RXCLEAR     =  0x0008;  // Kill the typeahead buffer if there.
////    }
////
////    internal static class UnsafeNativeMethods
////    {
////        //////////////////// Serial Port structs ////////////////////
////        // Declaration for C# representation of Win32 Device Control Block (DCB)
////        // structure.  Note that all flag properties are encapsulated in the Flags field here,
////        // and accessed/set through SerialStream's GetDcbFlag(...) and SetDcbFlag(...) methods.
////        internal struct DCB
////        {
////            public uint   BaudRate;
////
////            public bool   Flags_BINARY;
////            public bool   Flags_PARITY;
////            public bool   Flags_OUTXCTSFLOW;
////            public bool   Flags_OUTXDSRFLOW;
////            public bool   Flags_DTRCONTROL;
////            public bool   Flags_DSRSENSITIVITY;
////            public bool   Flags_TXCONTINUEONXOFF;
////            public bool   Flags_OUTX;
////            public bool   Flags_INX;
////            public bool   Flags_ERRORCHAR;
////            public bool   Flags_NULL;
////            public bool   Flags_RTSCONTROL;
////            public bool   Flags_ABORTONOERROR;
////
////            public ushort XonLim;
////            public ushort XoffLim;
////            public byte   ByteSize;
////            public byte   Parity;
////            public byte   StopBits;
////            public byte   XonChar;
////            public byte   XoffChar;
////            public byte   ErrorChar;
////            public byte   EofChar;
////            public byte   EvtChar;
////        }
////
////        // Declaration for C# representation of Win32 COMSTAT structure associated with
////        // a file handle to a serial communications resource.  SerialStream's
////        // InBufferBytes and OutBufferBytes directly expose cbInQue and cbOutQue to reading, respectively.
////        internal struct COMSTAT
////        {
////            public uint Flags;
////            public uint cbInQue;
////            public uint cbOutQue;
////        }
////
////        // Declaration for C# representation of Win32 COMMTIMEOUTS
////        // structure associated with a file handle to a serial communications resource.
////        ///Currently the only set fields are ReadTotalTimeoutConstant
////        // and WriteTotalTimeoutConstant.
////        internal struct COMMTIMEOUTS
////        {
////            public int ReadIntervalTimeout;
////            public int ReadTotalTimeoutMultiplier;
////            public int ReadTotalTimeoutConstant;
////            public int WriteTotalTimeoutMultiplier;
////            public int WriteTotalTimeoutConstant;
////        }
////
////        // Declaration for C# representation of Win32 COMMPROP
////        // structure associated with a file handle to a serial communications resource.
////        // Currently the only fields used are dwMaxTxQueue, dwMaxRxQueue, and dwMaxBaud
////        // to ensure that users provide appropriate settings to the SerialStream constructor.
////        internal struct COMMPROP
////        {
////            public ushort wPacketLength;
////            public ushort wPacketVersion;
////            public int    dwServiceMask;
////            public int    dwReserved1;
////            public int    dwMaxTxQueue;
////            public int    dwMaxRxQueue;
////            public int    dwMaxBaud;
////            public int    dwProvSubType;
////            public int    dwProvCapabilities;
////            public int    dwSettableParams;
////            public int    dwSettableBaud;
////            public ushort wSettableData;
////            public ushort wSettableStopParity;
////            public int    dwCurrentTxQueue;
////            public int    dwCurrentRxQueue;
////            public int    dwProvSpec1;
////            public int    dwProvSpec2;
////            public char   wcProvChar;
////        }
////        //////////////////// end Serial Port structs ////////////////////
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool SetCommState( object _handle, ref DCB dcb );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool SetCommBreak( object _handle );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool ClearCommBreak( object _handle );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool SetCommTimeouts( object _handle, ref COMMTIMEOUTS commTimeouts );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool GetCommModemStatus( object _handle, ref int pinStatus );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool ClearCommError( object _handle, ref int errorCode, ref COMSTAT comStat );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool GetCommProperties( object _handle, ref COMMPROP commProp );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern void SetCommMask( object _handle, int p );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool PurgeComm( object _handle, int p );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern void FlushFileBuffers( object _handle );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool SetupComm( object _handle, int readBufferSize, int writeBufferSize );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool GetCommState( object _handle, ref DCB dcb );
////
////        [MethodImpl( MethodImplOptions.InternalCall )]
////        internal static extern bool ClearCommError( object handle, ref int errors, IntPtr intPtr );
////    }
////
////    const int errorEvents      = (int)SerialError.Frame    |
////                                 (int)SerialError.Overrun  |
////                                 (int)SerialError.RXOver   |
////                                 (int)SerialError.RXParity |
////                                 (int)SerialError.TXFull;
////
////    const int receivedEvents   = (int)SerialData.Chars | 
////                                 (int)SerialData.Eof;
////
////    const int pinChangedEvents = (int)SerialPinChange.Break      |
////                                 (int)SerialPinChange.CDChanged  |
////                                 (int)SerialPinChange.CtsChanged |
////                                 (int)SerialPinChange.Ring       |
////                                 (int)SerialPinChange.DsrChanged;
////
////    const int infiniteTimeoutConst = -2;

////    // members supporting properties exposed to SerialPort
////    private string    portName;
////    private byte      parityReplace = (byte)'?';
////    private bool      inBreak       = false;               // port is initially in non-break state
////    private Handshake handshake;
////    private bool      rtsEnable     = false;
////
////    // The internal C# representations of Win32 structures necessary for communication
////    // hold most of the internal "fields" maintaining information about the port.
////    private UnsafeNativeMethods.DCB          dcb;
////    private UnsafeNativeMethods.COMMTIMEOUTS commTimeouts;
////    private UnsafeNativeMethods.COMSTAT      comStat;
////    private UnsafeNativeMethods.COMMPROP     commProp;
////
////    // internal-use members
////    // private const long dsrTimeout = 0L; -- Not used anymore.
////    private const int maxDataBits = 8;
////    private const int minDataBits = 5;
////    internal EventLoopRunner eventRunner;
////
////    private byte[] tempBuf;                 // used to avoid multiple array allocations in ReadByte()
////
////    // called whenever any async i/o operation completes.
////    private unsafe static readonly IOCompletionCallback IOCallback = new IOCompletionCallback( SerialStream.AsyncFSCallback );

        // three different events, also wrapped by SerialPort.
        internal event SerialDataReceivedEventHandler  DataReceived;    // called when one character is received.
        internal event SerialPinChangedEventHandler    PinChanged;      // called when any of the pin/ring-related triggers occurs
        internal event SerialErrorReceivedEventHandler ErrorReceived;   // called when any runtime error occurs on the port (frame, overrun, parity, etc.)


        // ----SECTION: inherited properties from Stream class ------------*

        // These six properites are required for SerialStream to inherit from the abstract Stream class.
        // Note four of them are always true or false, and two of them throw exceptions, so these
        // are not usefully queried by applications which know they have a SerialStream, etc...
        public override bool CanRead
        {
            get
            {
                return this.IsOpen;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return this.IsOpen;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.IsOpen;
            }
        }

        public override long Length
        {
            get
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( "NotSupported_UnseekableStream" );
#else
                throw new NotSupportedException();
#endif
////            throw new NotSupportedException( SR.GetString( SR.NotSupported_UnseekableStream ) );
            }
        }


        public override long Position
        {
            get
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( "NotSupported_UnseekableStream" );
#else
                throw new NotSupportedException();
#endif
////            throw new NotSupportedException( SR.GetString( SR.NotSupported_UnseekableStream ) );
            }

            set
            {
#if EXCEPTION_STRINGS
                throw new NotSupportedException( "NotSupported_UnseekableStream" );
#else
                throw new NotSupportedException();
#endif
////            throw new NotSupportedException( SR.GetString( SR.NotSupported_UnseekableStream ) );
            }
        }

        // ----- new get-set properties -----------------*

        // Standard port properties, also called from SerialPort
        // BaudRate may not be settable to an arbitrary integer between dwMinBaud and dwMaxBaud,
        // and is limited only by the serial driver.  Typically about twelve values such
        // as Winbase.h's CBR_110 through CBR_256000 are used.
        public abstract int BaudRate
        {
            //get { return (int) dcb.BaudRate; }

            set;
////        {
////            if(value <= 0 || (value > commProp.dwMaxBaud && commProp.dwMaxBaud > 0))
////            {
////                // if no upper bound on baud rate imposed by serial driver, note that argument must be positive
////                if(commProp.dwMaxBaud == 0)
////                {
////                    throw new ArgumentOutOfRangeException( "baudRate", SR.GetString( SR.ArgumentOutOfRange_NeedPosNum ) );
////                }
////                else
////                {
////                    // otherwise, we can present the bounds on the baud rate for this driver
////                    throw new ArgumentOutOfRangeException( "baudRate", SR.GetString( SR.ArgumentOutOfRange_Bounds_Lower_Upper, 0, commProp.dwMaxBaud ) );
////                }
////            }
////
////            // Set only if it's different.  Rollback to previous values if setting fails.
////            //  This pattern occurs through most of the other properties in this class.
////            if(value != dcb.BaudRate)
////            {
////                int baudRateOld = (int)dcb.BaudRate;
////                dcb.BaudRate = (uint)value;
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.BaudRate = (uint)baudRateOld;
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        public abstract bool BreakState
        {
            get;
////        {
////            return inBreak;
////        }

            set;
////        {
////            if(value)
////            {
////                if(UnsafeNativeMethods.SetCommBreak( _handle ) == false)
////                {
////                    InternalResources.WinIOError();
////                }
////                inBreak = true;
////            }
////            else
////            {
////                if(UnsafeNativeMethods.ClearCommBreak( _handle ) == false)
////                {
////                    InternalResources.WinIOError();
////                }
////
////                inBreak = false;
////            }
////        }
        }

        public abstract int DataBits
        {
            //get  { return (int) dcb.ByteSize; }
            set;
////        {
////            //Debug.Assert( !(value < minDataBits || value > maxDataBits), "An invalid value was passed to DataBits" );
////
////            if(value != dcb.ByteSize)
////            {
////                byte byteSizeOld = dcb.ByteSize;
////                dcb.ByteSize = (byte)value;
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.ByteSize = byteSizeOld;
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        public abstract bool DiscardNull
        {
            set;
////        {
////            bool fNullOld = dcb.Flags_NULL;
////            if(value != fNullOld)
////            {
////                dcb.Flags_NULL = value;
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.Flags_NULL = fNullOld;
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        public abstract bool DtrEnable
        {
            get;
////        {
////            return dcb.Flags_DTRCONTROL;
////        }

            set;
////        {
////            bool fDtrControlOld = dcb.Flags_DTRCONTROL;
////
////            if(value != fDtrControlOld)
////            {
////                dcb.Flags_DTRCONTROL = value;
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.Flags_DTRCONTROL = fDtrControlOld;
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        public abstract Handshake Handshake
        {
            set;
////        {
////
////            //Debug.Assert( !(value < System.IO.Ports.Handshake.None || value > System.IO.Ports.Handshake.RequestToSendXOnXOff), "An invalid value was passed to Handshake" );
////
////            if(value != handshake)
////            {
////                // in the DCB, handshake affects the fRtsControl, fOutxCtsFlow, and fInX, fOutX fields,
////                // so we must save everything in that closure before making any changes.
////                Handshake handshakeOld = handshake;
////
////                bool fInOutXOld      = dcb.Flags_INX;
////                bool fOutxCtsFlowOld = dcb.Flags_OUTXCTSFLOW;
////                bool fRtsControlOld  = dcb.Flags_RTSCONTROL;
////
////                handshake = value;
////
////                bool fInXOutXFlag = (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff);
////
////                dcb.Flags_INX         = fInXOutXFlag;
////                dcb.Flags_OUTX        = fInXOutXFlag;
////                dcb.Flags_OUTXCTSFLOW = (handshake == Handshake.RequestToSend || handshake == Handshake.RequestToSendXOnXOff);
////
////                if((handshake == Handshake.RequestToSend ||
////                    handshake == Handshake.RequestToSendXOnXOff))
////                {
////                    dcb.Flags_RTSCONTROL = true;
////                }
////                else
////                {
////                    dcb.Flags_RTSCONTROL = false;
////                }
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    handshake = handshakeOld;
////
////                    dcb.Flags_INX         = fInOutXOld;
////                    dcb.Flags_OUTX        = fInOutXOld;
////                    dcb.Flags_OUTXCTSFLOW = fOutxCtsFlowOld;
////                    dcb.Flags_RTSCONTROL  = fRtsControlOld;
////
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        public abstract bool IsOpen
        {
            get;
////        {
////            return _handle != null && !eventRunner.ShutdownLoop;
////        }
        }

        public abstract Parity Parity
        {
            set;
////        {
////            //Debug.Assert( !(value < Parity.None || value > Parity.Space), "An invalid value was passed to Parity" );
////
////            if((byte)value != dcb.Parity)
////            {
////                byte parityOld = dcb.Parity;
////
////                // in the DCB structure, the parity setting also potentially effects:
////                // fParity, fErrorChar, ErrorChar
////                // so these must be saved as well.
////                bool fParityOld    = dcb.Flags_PARITY;
////                byte ErrorCharOld  = dcb.ErrorChar;
////                bool fErrorCharOld = dcb.Flags_ERRORCHAR;
////
////                dcb.Parity = (byte)value;
////
////                bool parityFlag = (dcb.Parity != (byte)Parity.None);
////                dcb.Flags_PARITY = parityFlag;
////
////                if(parityFlag)
////                {
////                    dcb.Flags_ERRORCHAR = (parityReplace != '\0');
////                    dcb.ErrorChar       = parityReplace;
////                }
////                else
////                {
////                    dcb.Flags_ERRORCHAR = false;
////                    dcb.ErrorChar       = (byte)'\0';
////                }
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.Parity          = parityOld;
////                    dcb.ErrorChar       = ErrorCharOld;
////                    dcb.Flags_PARITY    = fParityOld;
////                    dcb.Flags_ERRORCHAR = fErrorCharOld;
////
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        // ParityReplace is the eight-bit character which replaces any bytes which
        // ParityReplace affects the equivalent field in the DCB structure: ErrorChar, and
        // the DCB flag fErrorChar.
        public abstract byte ParityReplace
        {
            set;
////        {
////            if(value != parityReplace)
////            {
////                byte parityReplaceOld = parityReplace;
////                byte errorCharOld = dcb.ErrorChar;
////                bool fErrorCharOld = dcb.Flags_ERRORCHAR;
////
////                parityReplace = value;
////                if(dcb.Flags_PARITY)
////                {
////                    dcb.Flags_ERRORCHAR = (parityReplace != '\0');
////                    dcb.ErrorChar       = parityReplace;
////                }
////                else
////                {
////                    dcb.Flags_ERRORCHAR = false;
////                    dcb.ErrorChar       = (byte)'\0';
////                }
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    parityReplace       = parityReplaceOld;
////                    dcb.Flags_ERRORCHAR = fErrorCharOld;
////                    dcb.ErrorChar       = errorCharOld;
////
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        // Timeouts are considered to be TOTAL time for the Read/Write operation and to be in milliseconds.
        // Timeouts are translated into DCB structure as follows:
        // Desired timeout      =>  ReadTotalTimeoutConstant    ReadTotalTimeoutMultiplier  ReadIntervalTimeout
        //  0                                   0                           0               MAXDWORD
        //  0 < n < infinity                    n                       MAXDWORD            MAXDWORD
        // infinity                             infiniteTimeoutConst    MAXDWORD            MAXDWORD
        //
        // rationale for "infinity": There does not exist in the COMMTIMEOUTS structure a way to
        // *wait indefinitely for any byte, return when found*.  Instead, if we set ReadTimeout
        // to infinity, SerialStream's EndRead loops if infiniteTimeoutConst mills have elapsed
        // without a byte received.  Note that this is approximately 24 days, so essentially
        // most practical purposes effectively equate 24 days with an infinite amount of time
        // on a serial port connection.
////    public override int ReadTimeout
////    {
////        get
////        {
////            int constant = commTimeouts.ReadTotalTimeoutConstant;
////
////            return (constant == infiniteTimeoutConst) ? SerialPort.InfiniteTimeout : constant;
////        }
////
////        set
////        {
////            if(value < 0 && value != SerialPort.InfiniteTimeout)
////            {
////                throw new ArgumentOutOfRangeException( "ReadTimeout", SR.GetString( SR.ArgumentOutOfRange_Timeout ) );
////            }
////
////            if(_handle == null)
////            {
////                InternalResources.FileNotOpen();
////            }
////
////            int oldReadConstant  = commTimeouts.ReadTotalTimeoutConstant;
////            int oldReadInterval  = commTimeouts.ReadIntervalTimeout;
////            int oldReadMultipler = commTimeouts.ReadTotalTimeoutMultiplier;
////
////            // NOTE: this logic should match what is in the constructor
////            if(value == 0)
////            {
////                commTimeouts.ReadTotalTimeoutConstant   = 0;
////                commTimeouts.ReadTotalTimeoutMultiplier = 0;
////                commTimeouts.ReadIntervalTimeout        = NativeMethods.MAXDWORD;
////            }
////            else if(value == SerialPort.InfiniteTimeout)
////            {
////                // SetCommTimeouts doesn't like a value of -1 for some reason, so
////                // we'll use -2(infiniteTimeoutConst) to represent infinite. 
////                commTimeouts.ReadTotalTimeoutConstant   = infiniteTimeoutConst;
////                commTimeouts.ReadTotalTimeoutMultiplier = NativeMethods.MAXDWORD;
////                commTimeouts.ReadIntervalTimeout        = NativeMethods.MAXDWORD;
////            }
////            else
////            {
////                commTimeouts.ReadTotalTimeoutConstant   = value;
////                commTimeouts.ReadTotalTimeoutMultiplier = NativeMethods.MAXDWORD;
////                commTimeouts.ReadIntervalTimeout        = NativeMethods.MAXDWORD;
////            }
////
////            if(UnsafeNativeMethods.SetCommTimeouts( _handle, ref commTimeouts ) == false)
////            {
////                commTimeouts.ReadTotalTimeoutConstant   = oldReadConstant;
////                commTimeouts.ReadTotalTimeoutMultiplier = oldReadMultipler;
////                commTimeouts.ReadIntervalTimeout        = oldReadInterval;
////
////                InternalResources.WinIOError();
////            }
////        }
////    }

        public abstract bool RtsEnable
        {
            get;
////        {
////            return dcb.Flags_RTSCONTROL;
////        }

            set;
////        {
////            if((handshake == Handshake.RequestToSend || handshake == Handshake.RequestToSendXOnXOff))
////            {
////                throw new InvalidOperationException( SR.GetString( SR.CantSetRtsWithHandshaking ) );
////            }
////
////            if(value != rtsEnable)
////            {
////                bool fRtsControlOld = dcb.Flags_RTSCONTROL;
////
////                rtsEnable            = value;
////                dcb.Flags_RTSCONTROL = value;
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.Flags_RTSCONTROL = fRtsControlOld;
////
////                    // set it back to the old value on a failure
////                    rtsEnable = !rtsEnable;
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        // StopBits represented in C# as StopBits enum type and in Win32 as an integer 1, 2, or 3.
        public abstract StopBits StopBits
        {
            set;
////        {
////            //Debug.Assert( !(value < StopBits.One || value > StopBits.OnePointFive), "An invalid value was passed to StopBits" );
////
////            byte nativeValue;
////
////            if     (value == StopBits.One         ) nativeValue = (byte)NativeMethods.ONESTOPBIT;
////            else if(value == StopBits.OnePointFive) nativeValue = (byte)NativeMethods.ONE5STOPBITS;
////            else                                    nativeValue = (byte)NativeMethods.TWOSTOPBITS;
////
////            if(nativeValue != dcb.StopBits)
////            {
////                byte stopBitsOld = dcb.StopBits;
////
////                dcb.StopBits = nativeValue;
////
////                if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////                {
////                    dcb.StopBits = stopBitsOld;
////
////                    InternalResources.WinIOError();
////                }
////            }
////        }
        }

        // note: WriteTimeout must be either SerialPort.InfiniteTimeout or POSITIVE.
        // a timeout of zero implies that every Write call throws an exception.
////    public override int WriteTimeout
////    {
////        get
////        {
////            int timeout = commTimeouts.WriteTotalTimeoutConstant;
////
////            return (timeout == 0) ? SerialPort.InfiniteTimeout : timeout;
////        }
////
////        set
////        {
////            if(value <= 0 && value != SerialPort.InfiniteTimeout)
////            {
////                throw new ArgumentOutOfRangeException( "WriteTimeout", SR.GetString( SR.ArgumentOutOfRange_WriteTimeout ) );
////            }
////
////            if(_handle == null)
////            {
////                InternalResources.FileNotOpen();
////            }
////
////            int oldWriteConstant = commTimeouts.WriteTotalTimeoutConstant;
////
////            commTimeouts.WriteTotalTimeoutConstant = ((value == SerialPort.InfiniteTimeout) ? 0 : value);
////
////            if(UnsafeNativeMethods.SetCommTimeouts( _handle, ref commTimeouts ) == false)
////            {
////                commTimeouts.WriteTotalTimeoutConstant = oldWriteConstant;
////
////                InternalResources.WinIOError();
////            }
////        }
////    }


        // CDHolding, CtsHolding, DsrHolding query the current state of each of the carrier, the CTS pin,
        // and the DSR pin, respectively. Read-only.
        // All will throw exceptions if the port is not open.
        public abstract bool CDHolding
        {
            get;
////        {
////            int pinStatus = 0;
////
////            if(UnsafeNativeMethods.GetCommModemStatus( _handle, ref pinStatus ) == false)
////            {
////                InternalResources.WinIOError();
////            }
////
////            return (NativeMethods.MS_RLSD_ON & pinStatus) != 0;
////        }
        }


        public abstract bool CtsHolding
        {
            get;
////        {
////            int pinStatus = 0;
////
////            if(UnsafeNativeMethods.GetCommModemStatus( _handle, ref pinStatus ) == false)
////            {
////                InternalResources.WinIOError();
////            }
////
////            return (NativeMethods.MS_CTS_ON & pinStatus) != 0;
////        }
        }

        public abstract bool DsrHolding
        {
            get;
////        {
////            int pinStatus = 0;
////
////            if(UnsafeNativeMethods.GetCommModemStatus( _handle, ref pinStatus ) == false)
////            {
////                InternalResources.WinIOError();
////            }
////
////            return (NativeMethods.MS_DSR_ON & pinStatus) != 0;
////        }
        }


        // Fills comStat structure from an unmanaged function
        // to determine the number of bytes waiting in the serial driver's internal receive buffer.
        public abstract int BytesToRead
        {
            get;
////        {
////            int errorCode = 0; // "ref" arguments need to have values, as opposed to "out" arguments
////
////            if(UnsafeNativeMethods.ClearCommError( _handle, ref errorCode, ref comStat ) == false)
////            {
////                InternalResources.WinIOError();
////            }
////
////            return (int)comStat.cbInQue;
////        }
        }

        // Fills comStat structure from an unmanaged function
        // to determine the number of bytes waiting in the serial driver's internal transmit buffer.
        public abstract int BytesToWrite
        {
            get;
////        {
////            int errorCode = 0; // "ref" arguments need to be set before method invocation, as opposed to "out" arguments
////
////            if(UnsafeNativeMethods.ClearCommError( _handle, ref errorCode, ref comStat ) == false)
////            {
////                InternalResources.WinIOError();
////            }
////
////            return (int)comStat.cbOutQue;
////        }
        }

        // -----------SECTION: constructor --------------------------*

        [MethodImpl( MethodImplOptions.InternalCall )]
        internal static extern SerialStream Create( string    portName        ,
                                                    int       baudRate        ,
                                                    Parity    parity          ,
                                                    int       dataBits        ,
                                                    StopBits  stopBits        ,
                                                    int       readBufferSize  ,
                                                    int       writeBufferSize ,
                                                    int       readTimeout     ,
                                                    int       writeTimeout    ,
                                                    Handshake handshake       ,
                                                    bool      dtrEnable       ,
                                                    bool      rtsEnable       ,
                                                    bool      discardNull     ,
                                                    byte      parityReplace   );

        // this method is used by SerialPort upon SerialStream's creation
////    [ResourceExposure( ResourceScope.Machine )]
////    [ResourceConsumption( ResourceScope.Machine )]
////    internal SerialStream( string    portName      ,
////                           int       baudRate      ,
////                           Parity    parity        ,
////                           int       dataBits      ,
////                           StopBits  stopBits      ,
////                           int       readTimeout   ,
////                           int       writeTimeout  ,
////                           Handshake handshake     ,
////                           bool      dtrEnable     ,
////                           bool      rtsEnable     ,
////                           bool      discardNull   ,
////                           byte      parityReplace )
////    {
////        if((portName == null) || !portName.StartsWith( "COM", StringComparison.OrdinalIgnoreCase ))
////            throw new ArgumentException( "Arg_InvalidSerialPort", "portName" );
////            throw new ArgumentException( SR.GetString( SR.Arg_InvalidSerialPort ), "portName" );
////
////        //Error checking done in SerialPort.
////
////        SafeFileHandle tempHandle = UnsafeNativeMethods.CreateFile( "\\\\.\\" + portName,
////            NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE,
////            0,    // comm devices must be opened w/exclusive-access
////            IntPtr.Zero, // no security attributes
////            UnsafeNativeMethods.OPEN_EXISTING, // comm devices must use OPEN_EXISTING
////            flags,
////            IntPtr.Zero  // hTemplate must be NULL for comm devices
////            );
////
////        if(tempHandle.IsInvalid)
////        {
////            InternalResources.WinIOError( portName );
////        }
////
////        try
////        {
////            int fileType = UnsafeNativeMethods.GetFileType( tempHandle );
////
////            // Allowing FILE_TYPE_UNKNOWN for legitimate serial device such as USB to serial adapter device 
////            if((fileType != UnsafeNativeMethods.FILE_TYPE_CHAR) && (fileType != UnsafeNativeMethods.FILE_TYPE_UNKNOWN))
////                throw new ArgumentException( "Arg_InvalidSerialPort", "portName" );
////                throw new ArgumentException( SR.GetString( SR.Arg_InvalidSerialPort ), "portName" );
////
////            _handle = tempHandle;
////
////            // set properties of the stream that exist as members in SerialStream
////            this.portName      = portName;
////            this.handshake     = handshake;
////            this.parityReplace = parityReplace;
////
////            tempBuf = new byte[1];          // used in ReadByte()
////
////            // Fill COMMPROPERTIES struct, which has our maximum allowed baud rate.
////            // Call a serial specific API such as GetCommModemStatus which would fail
////            // in case the device is not a legitimate serial device. For instance, 
////            // some illegal FILE_TYPE_UNKNOWN device (or) "LPT1" on Win9x 
////            // trying to pass for serial will be caught here. GetCommProperties works
////            // fine for "LPT1" on Win9x, so that alone can't be relied here to
////            // detect non serial devices.
////
////            commProp = new UnsafeNativeMethods.COMMPROP();
////            int pinStatus = 0;
////
////            if(!UnsafeNativeMethods.GetCommProperties ( _handle, ref commProp  ) ||
////               !UnsafeNativeMethods.GetCommModemStatus( _handle, ref pinStatus )  )
////            {
////                // If the portName they have passed in is a FILE_TYPE_CHAR but not a serial port,
////                // for example "LPT1", this API will fail.  For this reason we handle the error message specially. 
////                    int errorCode = Marshal.GetLastWin32Error();
////                    if((errorCode == NativeMethods.ERROR_INVALID_PARAMETER) || (errorCode == NativeMethods.ERROR_INVALID_HANDLE))
////                    {
////                        throw new ArgumentException( SR.GetString( SR.Arg_InvalidSerialPortExtended ), "portName" );
////                    }
////                    else
////                    {
////                        InternalResources.WinIOError( errorCode, string.Empty );
////                    }
////
////                InternalResources.WinIOError();
////            }
////
////            if(commProp.dwMaxBaud != 0 && baudRate > commProp.dwMaxBaud)
////            {
////                throw new ArgumentOutOfRangeException( "baudRate", SR.GetString( SR.Max_Baud, commProp.dwMaxBaud ) );
////            }
////
////
////            comStat = new UnsafeNativeMethods.COMSTAT();
////            // create internal DCB structure, initialize according to Platform SDK
////            // standard: ms-help://MS.MSNDNQTR.2002APR.1003/hardware/commun_965u.htm
////            dcb = new UnsafeNativeMethods.DCB();
////
////            // set constant properties of the DCB
////            InitializeDCB( baudRate, parity, dataBits, stopBits, discardNull );
////
////            this.DtrEnable = dtrEnable;
////
////            // query and cache the initial RtsEnable value 
////            // so that set_RtsEnable can do the (value != rtsEnable) optimization
////            this.rtsEnable = dcb.Flags_RTSCONTROL;
////
////            // now set this.RtsEnable to the specified value.
////            // Handshake takes precedence, this will be a nop if 
////            // handshake is either RequestToSend or RequestToSendXOnXOff 
////            if((handshake != Handshake.RequestToSend && handshake != Handshake.RequestToSendXOnXOff))
////            {
////                this.RtsEnable = rtsEnable;
////            }
////
////            // NOTE: this logic should match what is in the ReadTimeout property
////            if(readTimeout == 0)
////            {
////                commTimeouts.ReadTotalTimeoutConstant   = 0;
////                commTimeouts.ReadTotalTimeoutMultiplier = 0;
////                commTimeouts.ReadIntervalTimeout        = NativeMethods.MAXDWORD;
////            }
////            else if(readTimeout == SerialPort.InfiniteTimeout)
////            {
////                // SetCommTimeouts doesn't like a value of -1 for some reason, so
////                // we'll use -2(infiniteTimeoutConst) to represent infinite. 
////                commTimeouts.ReadTotalTimeoutConstant   = infiniteTimeoutConst;
////                commTimeouts.ReadTotalTimeoutMultiplier = NativeMethods.MAXDWORD;
////                commTimeouts.ReadIntervalTimeout        = NativeMethods.MAXDWORD;
////            }
////            else
////            {
////                commTimeouts.ReadTotalTimeoutConstant   = readTimeout;
////                commTimeouts.ReadTotalTimeoutMultiplier = NativeMethods.MAXDWORD;
////                commTimeouts.ReadIntervalTimeout        = NativeMethods.MAXDWORD;
////            }
////
////            commTimeouts.WriteTotalTimeoutMultiplier = 0;
////            commTimeouts.WriteTotalTimeoutConstant   = ((writeTimeout == SerialPort.InfiniteTimeout) ? 0 : writeTimeout);
////
////            // set unmanaged timeout structure
////            if(UnsafeNativeMethods.SetCommTimeouts( _handle, ref commTimeouts ) == false)
////            {
////                InternalResources.WinIOError();
////            }
////
////            // monitor all events except TXEMPTY
////            UnsafeNativeMethods.SetCommMask( _handle, NativeMethods.ALL_EVENTS );
////
////            // prep. for starting event cycle.
////            eventRunner = new EventLoopRunner( this );
////            Thread eventLoopThread = new Thread( new ThreadStart( eventRunner.WaitForCommEvent ) );
////            eventLoopThread.IsBackground = true;
////            eventLoopThread.Start();
////        }
////        catch
////        {
////            // if there are any exceptions after the call to CreateFile, we need to be sure to close the
////            // handle before we let them continue up.
////            tempHandle.Close();
////            _handle = null;
////            throw;
////        }
////    }

        ~SerialStream()
        {
            Dispose( false );
        }

////    protected override void Dispose( bool disposing )
////    {
////        // Signal the other side that we're closing.  Should do regardless of whether we've called
////        // Close() or not Dispose() 
////        if(_handle != null && !_handle.IsInvalid)
////        {
////            try
////            {
////                eventRunner.endEventLoop = true;
////                Thread.MemoryBarrier();
////
////                // turn off all events and signal WaitCommEvent
////                UnsafeNativeMethods.SetCommMask( _handle, 0 );
////                if(!UnsafeNativeMethods.EscapeCommFunction( _handle, NativeMethods.CLRDTR ))
////                {
////                    // should not happen
////                    InternalResources.WinIOError();
////                }
////
////                if(!_handle.IsClosed)
////                    Flush();
////
////                eventRunner.waitCommEventWaitHandle.Set();
////                DiscardInBuffer();
////                DiscardOutBuffer();
////
////                if(disposing && eventRunner != null)
////                {
////                    // now we need to wait for the event loop to tell us it's done.  Without this we could get into a race where the
////                    // event loop kept the port open even after Dispose ended.
////                    eventRunner.eventLoopEndedSignal.WaitOne();
////                    eventRunner.eventLoopEndedSignal.Close();
////                    eventRunner.waitCommEventWaitHandle.Close();
////                }
////            }
////            finally
////            {
////                // If we are disposing synchronize closing with raising SerialPort events
////                if(disposing)
////                {
////                    lock(this)
////                    {
////                        _handle.Close();
////                        _handle = null;
////                    }
////                }
////                else
////                {
////                    _handle.Close();
////                    _handle = null;
////                }
////                base.Dispose( disposing );
////            }
////        }
////    }

        // -----SECTION: all public methods ------------------*

////    // User-accessible async read method.  Returns SerialStreamAsyncResult : IAsyncResult
////    [HostProtection( ExternalThreading = true )]
////    public override IAsyncResult BeginRead( byte[]        array        ,
////                                            int           offset       ,
////                                            int           numBytes     ,
////                                            AsyncCallback userCallback ,
////                                            object        stateObject  )
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(offset < 0)
////        {
////            throw new ArgumentOutOfRangeException( "offset", SR.GetString( SR.ArgumentOutOfRange_NeedNonNegNumRequired ) );
////        }
////
////        if(numBytes < 0)
////        {
////            throw new ArgumentOutOfRangeException( "numBytes", SR.GetString( SR.ArgumentOutOfRange_NeedNonNegNumRequired ) );
////        }
////
////        if(array.Length - offset < numBytes)
////        {
////            throw new ArgumentException( SR.GetString( SR.Argument_InvalidOffLen ) );
////        }
////
////        if(_handle == null)
////        {
////            InternalResources.FileNotOpen();
////        }
////
////        int oldtimeout = ReadTimeout;
////        this.ReadTimeout = SerialPort.InfiniteTimeout;
////        IAsyncResult result;
////        try
////        {
////            result = base.BeginRead( array, offset, numBytes, userCallback, stateObject );
////        }
////        finally
////        {
////            ReadTimeout = oldtimeout;
////        }
////        return result;
////    }

////    // User-accessible async write method.  Returns SerialStreamAsyncResult : IAsyncResult
////    // Throws an exception if port is in break state.
////    [HostProtection( ExternalThreading = true )]
////    public override IAsyncResult BeginWrite( byte[]        array        ,
////                                             int           offset       ,
////                                             int           numBytes     ,
////                                             AsyncCallback userCallback ,
////                                             object        stateObject  )
////    {
////        if(inBreak)
////        {
////            throw new InvalidOperationException( SR.GetString( SR.In_Break_State ) );
////        }
////
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array" );
////        }
////
////        if(offset < 0)
////        {
////            throw new ArgumentOutOfRangeException( "offset", SR.GetString( SR.ArgumentOutOfRange_NeedNonNegNumRequired ) );
////        }
////
////        if(numBytes < 0)
////        {
////            throw new ArgumentOutOfRangeException( "numBytes", SR.GetString( SR.ArgumentOutOfRange_NeedNonNegNumRequired ) );
////        }
////
////        if(array.Length - offset < numBytes)
////        {
////            throw new ArgumentException( SR.GetString( SR.Argument_InvalidOffLen ) );
////        }
////
////        if(_handle == null)
////        {
////            InternalResources.FileNotOpen();
////        }
////
////        int oldtimeout = WriteTimeout;
////        WriteTimeout = SerialPort.InfiniteTimeout;
////        IAsyncResult result;
////        try
////        {
////            result = base.BeginWrite( array, offset, numBytes, userCallback, stateObject );
////        }
////        finally
////        {
////            WriteTimeout = oldtimeout;
////        }
////        return result;
////    }

        // Uses Win32 method to dump out the receive buffer; analagous to MSComm's "InBufferCount = 0"
        public abstract void DiscardInBuffer();
////    {
////        if(UnsafeNativeMethods.PurgeComm( _handle, NativeMethods.PURGE_RXCLEAR | NativeMethods.PURGE_RXABORT ) == false)
////        {
////            InternalResources.WinIOError();
////        }
////    }

        // Uses Win32 method to dump out the xmit buffer; analagous to MSComm's "OutBufferCount = 0"
        public abstract void DiscardOutBuffer();
////    {
////        if(UnsafeNativeMethods.PurgeComm( _handle, NativeMethods.PURGE_TXCLEAR | NativeMethods.PURGE_TXABORT ) == false)
////        {
////            InternalResources.WinIOError();
////        }
////    }

////    // Async companion to BeginRead.
////    // Note, assumed IAsyncResult argument is of derived type SerialStreamAsyncResult,
////    // and throws an exception if untrue.
////    public override int EndRead( IAsyncResult asyncResult )
////    {
////        return base.EndRead( asyncResult );
////    }

////    // Async companion to BeginWrite.
////    // Note, assumed IAsyncResult argument is of derived type SerialStreamAsyncResult,
////    // and throws an exception if untrue.
////    // Also fails if called in port's break state.
////    public unsafe override void EndWrite( IAsyncResult asyncResult )
////    {
////        base.EndWrite( asyncResult );
////    }

////    // Flush dumps the contents of the serial driver's internal read and write buffers.
////    // We actually expose the functionality for each, but fulfilling Stream's contract
////    // requires a Flush() method.  Fails if handle closed.
////    // Note: Serial driver's write buffer is *already* attempting to write it, so we can only wait until it finishes.
////    public override void Flush()
////    {
////        if(_handle == null)
////        {
////            throw new ObjectDisposedException( SR.GetString( SR.Port_not_open ) );
////        }
////
////        UnsafeNativeMethods.FlushFileBuffers( _handle );
////    }

        // Blocking read operation, returning the number of bytes read from the stream.

        public override int Read( [In, Out] byte[] array  ,
                                            int    offset ,
                                            int    count  )
        {
            return Read( array, offset, count, ReadTimeout );
        }

        public abstract int Read( [In, Out] byte[] array   ,
                                            int    offset  ,
                                            int    count   ,
                                            int    timeout );
////    {
////        if(array == null)
////        {
////            throw new ArgumentNullException( "array", SR.GetString( SR.ArgumentNull_Buffer ) );
////        }
////
////        if(offset < 0)
////        {
////            throw new ArgumentOutOfRangeException( "offset", SR.GetString( SR.ArgumentOutOfRange_NeedNonNegNumRequired ) );
////        }
////
////        if(count < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", SR.GetString( SR.ArgumentOutOfRange_NeedNonNegNumRequired ) );
////        }
////
////        if(array.Length - offset < count)
////        {
////            throw new ArgumentException( SR.GetString( SR.Argument_InvalidOffLen ) );
////        }
////
////        if(count == 0) 
////        {
////            return 0; // return immediately if no bytes requested; no need for overhead.
////        }
////
////        //Debug.Assert( timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Read - called with timeout " + timeout );
////
////        // Check to see we have no handle-related error, since the port's always supposed to be open.
////        if(_handle == null) InternalResources.FileNotOpen();
////
////        int numBytes = 0;
////        int hr;
////
////        numBytes = ReadFileNative( array, offset, count, null, out hr );
////        if(numBytes == -1)
////        {
////            InternalResources.WinIOError();
////        }
////
////        if(numBytes == 0)
////        {
////            throw new TimeoutException();
////        }
////
////        return numBytes;
////    }

        public override int ReadByte()
        {
            return ReadByte( ReadTimeout );
        }

        public abstract int ReadByte( int timeout );
////    {
////        if(_handle == null)
////        {
////            InternalResources.FileNotOpen();
////        }
////
////        int numBytes = 0;
////        int hr;
////
////        numBytes = ReadFileNative( tempBuf, 0, 1, null, out hr );
////        if(numBytes == -1)
////        {
////            InternalResources.WinIOError();
////        }
////
////        if(numBytes == 0)
////        {
////            throw new TimeoutException();
////        }
////
////        return tempBuf[0];
////    }

        public override long Seek( long offset, SeekOrigin origin )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( "NotSupported_UnseekableStream" );
#else
            throw new NotSupportedException();
#endif
////        throw new NotSupportedException( SR.GetString( SR.NotSupported_UnseekableStream ) );
        }

        public override void SetLength( long value )
        {
#if EXCEPTION_STRINGS
            throw new NotSupportedException( "NotSupported_UnseekableStream" );
#else
            throw new NotSupportedException();
#endif
////        throw new NotSupportedException( SR.GetString( SR.NotSupported_UnseekableStream ) );
        }

        public override void Write( byte[] array  ,
                                    int    offset ,
                                    int    count  )
        {
            Write( array, offset, count, WriteTimeout );
        }

        public abstract void Write( byte[] array   ,
                                    int    offset  ,
                                    int    count   ,
                                    int    timeout );
////    {
////        if(inBreak)
////        {
////            throw new InvalidOperationException( SR.GetString( SR.In_Break_State ) );
////        }
////
////        if(array == null)
////        {
////            throw new ArgumentNullException( "buffer", SR.GetString( SR.ArgumentNull_Array ) );
////        }
////
////        if(offset < 0)
////        {
////            throw new ArgumentOutOfRangeException( "offset", SR.GetString( SR.ArgumentOutOfRange_NeedPosNum ) );
////        }
////
////        if(count < 0)
////        {
////            throw new ArgumentOutOfRangeException( "count", SR.GetString( SR.ArgumentOutOfRange_NeedPosNum ) );
////        }
////
////        if(count == 0)
////        {
////            return; // no need to expend overhead in creating asyncResult, etc.
////        }
////
////        if(array.Length - offset < count)
////        {
////            throw new ArgumentException( "count", SR.GetString( SR.ArgumentOutOfRange_OffsetOut ) );
////        }
////
////        //Debug.Assert( timeout == SerialPort.InfiniteTimeout || timeout >= 0, "Serial Stream Write - write timeout is " + timeout );
////
////        // check for open handle, though the port is always supposed to be open
////        if(_handle == null) InternalResources.FileNotOpen();
////
////        int numBytes;
////        int hr;
////
////        numBytes = WriteFileNative( array, offset, count, null, out hr );
////        if(numBytes == -1)
////        {
////            InternalResources.WinIOError();
////        }
////
////        if(numBytes == 0)
////        {
////            throw new TimeoutException( SR.GetString( SR.Write_timed_out ) );
////        }
////    }

        // use default timeout as argument to WriteByte override with timeout arg
        public override void WriteByte( byte value )
        {
            WriteByte( value, WriteTimeout );
        }

        public abstract void WriteByte( byte value   ,
                                        int  timeout );
////    {
////        if(inBreak)
////        {
////            throw new InvalidOperationException( SR.GetString( SR.In_Break_State ) );
////        }
////
////        if(_handle == null)
////        {
////            InternalResources.FileNotOpen();
////        }
////
////        tempBuf[0] = value;
////
////
////        int numBytes;
////        int hr;
////
////        numBytes = WriteFileNative( tempBuf, 0, 1, null, out hr );
////        if(numBytes == -1)
////        {
////            InternalResources.WinIOError();
////        }
////
////        if(numBytes == 0)
////        {
////            throw new TimeoutException( SR.GetString( SR.Write_timed_out ) );
////        }
////
////        return;
////    }


        // --------SUBSECTION: internal-use methods ----------------------*
        // ------ internal DCB-supporting methods ------- *

////    // Initializes unmananged DCB struct, to be called after opening communications resource.
////    // assumes we have already: baudRate, parity, dataBits, stopBits
////    // should only be called in SerialStream(...)
////    private void InitializeDCB( int      baudRate    ,
////                                Parity   parity      ,
////                                int      dataBits    ,
////                                StopBits stopBits    ,
////                                bool     discardNull )
////    {
////        // first get the current dcb structure setup
////        if(UnsafeNativeMethods.GetCommState( _handle, ref dcb ) == false)
////        {
////            InternalResources.WinIOError();
////        }
////
////        // set parameterized properties
////        dcb.BaudRate = (uint)baudRate;
////        dcb.ByteSize = (byte)dataBits;
////
////
////        switch(stopBits)
////        {
////            case StopBits.One:
////                dcb.StopBits = NativeMethods.ONESTOPBIT;
////                break;
////            case StopBits.OnePointFive:
////                dcb.StopBits = NativeMethods.ONE5STOPBITS;
////                break;
////            case StopBits.Two:
////                dcb.StopBits = NativeMethods.TWOSTOPBITS;
////                break;
////            default:
////                //Debug.Assert( false, "Invalid value for stopBits" );
////                break;
////        }
////
////        dcb.Parity = (byte)parity;
////        // SetDcbFlag, GetDcbFlag expose access to each of the relevant bits of the 32-bit integer
////        // storing all flags of the DCB.  C# provides no direct means of manipulating bit fields, so
////        // this is the solution.
////        dcb.Flags_PARITY =  (parity != Parity.None);
////
////        dcb.Flags_BINARY = true; // always true for communications resources
////
////        // set DCB fields implied by default and the arguments given.
////        // Boolean fields in C# must become 1, 0 to properly set the bit flags in the unmanaged DCB struct
////
////        dcb.Flags_OUTXCTSFLOW    = (handshake == Handshake.RequestToSend || handshake == Handshake.RequestToSendXOnXOff);
////        dcb.Flags_OUTXDSRFLOW    = false; // dsrTimeout is always set to 0.
////        dcb.Flags_DTRCONTROL     = false;
////        dcb.Flags_DSRSENSITIVITY = false;
////        dcb.Flags_INX            = (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff);
////        dcb.Flags_OUTX           = (handshake == Handshake.XOnXOff || handshake == Handshake.RequestToSendXOnXOff);
////
////        // if no parity, we have no error character (i.e. ErrorChar = '\0' or null character)
////        if(parity != Parity.None)
////        {
////            dcb.Flags_ERRORCHAR = (parityReplace != '\0');
////            dcb.ErrorChar       = parityReplace;
////        }
////        else
////        {
////            dcb.Flags_ERRORCHAR = false;
////            dcb.ErrorChar       = (byte)'\0';
////        }
////
////        // this method only runs once in the constructor, so we only have the default value to use.
////        // Later the user may change this via the NullDiscard property.
////        dcb.Flags_NULL = discardNull;
////
////
////        // Setting RTS control, which is RTS_CONTROL_HANDSHAKE if RTS / RTS-XOnXOff handshaking
////        // used, RTS_ENABLE (RTS pin used during operation) if rtsEnable true but XOnXoff / No handshaking
////        // used, and disabled otherwise.
////        if(handshake == Handshake.RequestToSend        ||
////           handshake == Handshake.RequestToSendXOnXOff  )
////        {
////            dcb.Flags_RTSCONTROL = true;
////        }
////        else
////        {
////            dcb.Flags_RTSCONTROL = false;
////        }
////
////        dcb.XonChar  = NativeMethods.DEFAULTXONCHAR;             // may be exposed later but for now, constant
////        dcb.XoffChar = NativeMethods.DEFAULTXOFFCHAR;
////
////        // minimum number of bytes allowed in each buffer before flow control activated
////        // heuristically, this has been set at 1/4 of the buffer size
////        dcb.XonLim = dcb.XoffLim = (ushort)(commProp.dwCurrentRxQueue / 4);
////
////        dcb.EofChar = NativeMethods.EOFCHAR;
////
////        //OLD MSCOMM: dcb.EvtChar = (byte) 0;
////        // now changed to make use of RXFlag WaitCommEvent event => Eof WaitForCommEvent event
////        dcb.EvtChar = NativeMethods.EOFCHAR;
////
////        // set DCB structure
////        if(UnsafeNativeMethods.SetCommState( _handle, ref dcb ) == false)
////        {
////            InternalResources.WinIOError();
////        }
////    }

        // ----SUBSECTION: internal methods supporting public read/write methods-------*

        // ----SECTION: internal classes --------*

////    internal sealed class EventLoopRunner
////    {
////        private  WeakReference    streamWeakReference;
////        internal ManualResetEvent eventLoopEndedSignal    = new ManualResetEvent( false );
////        internal ManualResetEvent waitCommEventWaitHandle = new ManualResetEvent( false );
////        private  object           handle;
////        internal bool             endEventLoop;
////
////        WaitCallback              callErrorEvents;
////        WaitCallback              callReceiveEvents;
////        WaitCallback              callPinEvents;
////
////        internal EventLoopRunner( SerialStream stream )
////        {
////            handle = stream._handle;
////            streamWeakReference = new WeakReference( stream );
////
////            callErrorEvents   = new WaitCallback( CallErrorEvents );
////            callReceiveEvents = new WaitCallback( CallReceiveEvents );
////            callPinEvents     = new WaitCallback( CallPinEvents );
////        }
////
////        internal bool ShutdownLoop
////        {
////            get
////            {
////                return endEventLoop;
////            }
////        }
////
////        // This is the blocking method that waits for an event to occur.  It wraps the SDK's WaitCommEvent function.
////        [ResourceExposure( ResourceScope.None )]
////        [ResourceConsumption( ResourceScope.Machine, ResourceScope.Machine )]
////        internal unsafe void WaitForCommEvent()
////        {
////            int unused = 0;
////            bool doCleanup = false;
////            NativeOverlapped* intOverlapped = null;
////            while(!ShutdownLoop)
////            {
////                SerialStreamAsyncResult asyncResult = null;
////                if(isAsync)
////                {
////                    asyncResult = new SerialStreamAsyncResult();
////                    asyncResult._userCallback = null;
////                    asyncResult._userStateObject = null;
////                    asyncResult._isWrite = false;
////
////                    // we're going to use _numBytes for something different in this loop.  In this case, both 
////                    // freeNativeOverlappedCallback and this thread will decrement that value.  Whichever one decrements it
////                    // to zero will be the one to free the native overlapped.  This guarantees the overlapped gets freed
////                    // after both the callback and GetOverlappedResult have had a chance to use it. 
////                    asyncResult._numBytes = 2;
////                    asyncResult._waitHandle = waitCommEventWaitHandle;
////
////                    waitCommEventWaitHandle.Reset();
////                    Overlapped overlapped = new Overlapped( 0, 0, waitCommEventWaitHandle.SafeWaitHandle.DangerousGetHandle(), asyncResult );
////                    // Pack the Overlapped class, and store it in the async result
////                    intOverlapped = overlapped.Pack( freeNativeOverlappedCallback, null );
////                }
////
////                int eventsOccurred = 0;
////
////                if(UnsafeNativeMethods.WaitCommEvent( handle, ref eventsOccurred, intOverlapped ) == false)
////                {
////                    int hr = Marshal.GetLastWin32Error();
////                    if(hr == NativeMethods.ERROR_ACCESS_DENIED)
////                    {
////                        doCleanup = true;
////                        break;
////                    }
////                    if(hr == NativeMethods.ERROR_IO_PENDING)
////                    {
////                        //Debug.Assert( isAsync, "The port is not open for async, so we should not get ERROR_IO_PENDING from WaitCommEvent" );
////                        int error;
////
////                        // if we get IO pending, MSDN says we should wait on the WaitHandle, then call GetOverlappedResult
////                        // to get the results of WaitCommEvent. 
////                        bool success = waitCommEventWaitHandle.WaitOne();
////                        //Debug.Assert( success, "waitCommEventWaitHandle.WaitOne() returned error " + Marshal.GetLastWin32Error() );
////
////                        do
////                        {
////                            // NOTE: GetOverlappedResult will modify the original pointer passed into WaitCommEvent.
////                            success = UnsafeNativeMethods.GetOverlappedResult( handle, intOverlapped, ref unused, false );
////                            error = Marshal.GetLastWin32Error();
////                        }
////                        while(error == NativeMethods.ERROR_IO_INCOMPLETE && !ShutdownLoop && !success);
////
////                        if(!success)
////                        {
////                            // Ignore ERROR_IO_INCOMPLETE and ERROR_INVALID_PARAMETER, because there's a chance we'll get
////                            // one of those while shutting down 
////                            if(!((error == NativeMethods.ERROR_IO_INCOMPLETE || error == NativeMethods.ERROR_INVALID_PARAMETER) && ShutdownLoop))
////                                //Debug.Assert( false, "GetOverlappedResult returned error, we might leak intOverlapped memory" + error.ToString( CultureInfo.InvariantCulture ) );
////                        }
////                    }
////                    else if(hr != NativeMethods.ERROR_INVALID_PARAMETER)
////                    {
////                        // ignore ERROR_INVALID_PARAMETER errors.  WaitCommError seems to return this
////                        // when SetCommMask is changed while it's blocking (like we do in Dispose())
////                        //Debug.Assert( false, "WaitCommEvent returned error " + hr );
////                    }
////                }
////
////                if(!ShutdownLoop)
////                    CallEvents( eventsOccurred );
////
////                if(isAsync)
////                {
////
////                    if(Interlocked.Decrement( ref asyncResult._numBytes ) == 0)
////                        Overlapped.Free( intOverlapped );
////                }
////            }
////            if(doCleanup)
////            {
////                // the rest will be handled in Dispose()
////                endEventLoop = true;
////                Overlapped.Free( intOverlapped );
////            }
////            eventLoopEndedSignal.Set();
////        }
////
////        private unsafe void FreeNativeOverlappedCallback( uint errorCode, uint numBytes, NativeOverlapped* pOverlapped )
////        {
////            // Unpack overlapped
////            Overlapped overlapped = Overlapped.Unpack( pOverlapped );
////
////            // Extract the async result from overlapped structure
////            SerialStreamAsyncResult asyncResult =
////                (SerialStreamAsyncResult)overlapped.AsyncResult;
////
////            if(Interlocked.Decrement( ref asyncResult._numBytes ) == 0)
////                Overlapped.Free( pOverlapped );
////        }
////
////        private void CallEvents( int nativeEvents )
////        {
////            // EV_ERR includes only CE_FRAME, CE_OVERRUN, and CE_RXPARITY
////            // To catch errors such as CE_RXOVER, we need to call CleanCommErrors bit more regularly. 
////            // EV_RXCHAR is perhaps too loose an event to look for overflow errors but a safe side to err...
////            if((nativeEvents & (NativeMethods.EV_ERR | NativeMethods.EV_RXCHAR)) != 0)
////            {
////                int errors = 0;
////                if(UnsafeNativeMethods.ClearCommError( handle, ref errors, IntPtr.Zero ) == false)
////                {
////
////                    //InternalResources.WinIOError();
////
////                    // We don't want to throw an exception from the background thread which is un-catchable and hence tear down the process.
////                    // At present we don't have a first class event that we can raise for this class of fatal errors. One possibility is 
////                    // to overload SeralErrors event to include another enum (perhaps CE_IOE) that we can use for this purpose. 
////                    // In the absene of that, it is better to eat this error silently than tearing down the process (lesser of the evil). 
////                    // This uncleared comm error will most likely blow up when the device is accessed by other APIs (such as Read) on the 
////                    // main thread and hence become known. It is bit roundabout but acceptable.  
////                    //  
////                    // Shutdown the event runner loop (probably bit drastic but we did come across a fatal error). 
////                    // Defer actual dispose chores until finalization though. 
////                    endEventLoop = true;
////                    Thread.MemoryBarrier();
////                    return;
////                }
////
////                errors = errors & errorEvents;
////                // TODO: what about CE_BREAK?  Is this the same as EV_BREAK?  EV_BREAK happens as one of the pin events,
////                //       but CE_BREAK is returned from ClreaCommError.
////                // TODO: what about other error conditions not covered by the enum?  Should those produce some other error?
////
////                if(errors != 0)
////                {
////                    ThreadPool.QueueUserWorkItem( callErrorEvents, errors );
////                }
////            }
////
////            // now look for pin changed and received events.
////            if((nativeEvents & pinChangedEvents) != 0)
////            {
////                ThreadPool.QueueUserWorkItem( callPinEvents, nativeEvents );
////            }
////
////            if((nativeEvents & receivedEvents) != 0)
////            {
////                ThreadPool.QueueUserWorkItem( callReceiveEvents, nativeEvents );
////            }
////        }
////
////
////        private void CallErrorEvents( object state )
////        {
////            int errors = (int)state;
////            SerialStream stream = (SerialStream)streamWeakReference.Target;
////            if(stream == null)
////                return;
////
////            if(stream.ErrorReceived != null)
////            {
////                if((errors & (int)SerialError.TXFull) != 0)
////                    stream.ErrorReceived( stream, new SerialErrorReceivedEventArgs( SerialError.TXFull ) );
////
////                if((errors & (int)SerialError.RXOver) != 0)
////                    stream.ErrorReceived( stream, new SerialErrorReceivedEventArgs( SerialError.RXOver ) );
////
////                if((errors & (int)SerialError.Overrun) != 0)
////                    stream.ErrorReceived( stream, new SerialErrorReceivedEventArgs( SerialError.Overrun ) );
////
////                if((errors & (int)SerialError.RXParity) != 0)
////                    stream.ErrorReceived( stream, new SerialErrorReceivedEventArgs( SerialError.RXParity ) );
////
////                if((errors & (int)SerialError.Frame) != 0)
////                    stream.ErrorReceived( stream, new SerialErrorReceivedEventArgs( SerialError.Frame ) );
////            }
////
////            stream = null;
////        }
////
////        private void CallReceiveEvents( object state )
////        {
////            int nativeEvents = (int)state;
////            SerialStream stream = (SerialStream)streamWeakReference.Target;
////            if(stream == null)
////                return;
////
////            if(stream.DataReceived != null)
////            {
////                if((nativeEvents & (int)SerialData.Chars) != 0)
////                    stream.DataReceived( stream, new SerialDataReceivedEventArgs( SerialData.Chars ) );
////                if((nativeEvents & (int)SerialData.Eof) != 0)
////                    stream.DataReceived( stream, new SerialDataReceivedEventArgs( SerialData.Eof ) );
////            }
////
////            stream = null;
////        }
////
////        private void CallPinEvents( object state )
////        {
////            int nativeEvents = (int)state;
////
////            SerialStream stream = (SerialStream)streamWeakReference.Target;
////            if(stream == null)
////                return;
////
////            if(stream.PinChanged != null)
////            {
////                if((nativeEvents & (int)SerialPinChange.CtsChanged) != 0)
////                    stream.PinChanged( stream, new SerialPinChangedEventArgs( SerialPinChange.CtsChanged ) );
////
////                if((nativeEvents & (int)SerialPinChange.DsrChanged) != 0)
////                    stream.PinChanged( stream, new SerialPinChangedEventArgs( SerialPinChange.DsrChanged ) );
////
////                if((nativeEvents & (int)SerialPinChange.CDChanged) != 0)
////                    stream.PinChanged( stream, new SerialPinChangedEventArgs( SerialPinChange.CDChanged ) );
////
////                if((nativeEvents & (int)SerialPinChange.Ring) != 0)
////                    stream.PinChanged( stream, new SerialPinChangedEventArgs( SerialPinChange.Ring ) );
////
////                if((nativeEvents & (int)SerialPinChange.Break) != 0)
////                    stream.PinChanged( stream, new SerialPinChangedEventArgs( SerialPinChange.Break ) );
////            }
////
////            stream = null;
////        }
////
////    }


////    // This is an internal object implementing IAsyncResult with fields
////    // for all of the relevant data necessary to complete the IO operation.
////    // This is used by AsyncFSCallback and all async methods.
////    unsafe internal sealed class SerialStreamAsyncResult : IAsyncResult
////    {
////        // User code callback
////        internal AsyncCallback _userCallback;
////
////        internal Object _userStateObject;
////
////        internal bool _isWrite;     // Whether this is a read or a write
////        internal bool _isComplete;
////        internal bool _completedSynchronously;  // Which thread called callback
////
////        internal ManualResetEvent _waitHandle;
////        internal int _EndXxxCalled;   // Whether we've called EndXxx already.
////        internal int _numBytes;     // number of bytes read OR written
////        internal int _errorCode;
////        internal NativeOverlapped* _overlapped;
////
////        public Object AsyncState
////        {
////            get { return _userStateObject; }
////        }
////
////        public bool IsCompleted
////        {
////            get { return _isComplete; }
////        }
////
////        public WaitHandle AsyncWaitHandle
////        {
////            get
////            {
////                /*
////                  // Consider uncommenting this someday soon - the EventHandle 
////                  // in the Overlapped struct is really useless half of the 
////                  // time today since the OS doesn't signal it.  If users call
////                  // EndXxx after the OS call happened to complete, there's no
////                  // reason to create a synchronization primitive here.  Fixing
////                  // this will save us some perf, assuming we can correctly
////                  // initialize the ManualResetEvent. 
////                if (_waitHandle == null) {
////                    ManualResetEvent mre = new ManualResetEvent(false);
////                    if (_overlapped != null && _overlapped->EventHandle != IntPtr.Zero)
////                        mre.Handle = _overlapped->EventHandle;
////                    if (_isComplete)
////                        mre.Set();
////                    _waitHandle = mre;
////                }
////                */
////                return _waitHandle;
////            }
////        }
////
////        // Returns true iff the user callback was called by the thread that
////        // called BeginRead or BeginWrite.  If we use an async delegate or
////        // threadpool thread internally, this will be false.  This is used
////        // by code to determine whether a successive call to BeginRead needs
////        // to be done on their main thread or in their callback to avoid a
////        // stack overflow on many reads or writes.
////        public bool CompletedSynchronously
////        {
////            get { return _completedSynchronously; }
////        }
////    }
    }
}
