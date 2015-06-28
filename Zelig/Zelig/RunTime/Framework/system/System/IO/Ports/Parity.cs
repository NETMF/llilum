// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Type: Parity
**
** Purpose: Parity enum type defined here.
**
** Date:  August 2002
**
===========================================================*/

namespace System.IO.Ports
{
    public enum Parity
    {
        None  = 0, // NativeMethods.NOPARITY,
        Odd   = 1, // NativeMethods.ODDPARITY,
        Even  = 2, // NativeMethods.EVENPARITY,
        Mark  = 3, // NativeMethods.MARKPARITY,
        Space = 4, // NativeMethods.SPACEPARITY
    }
}

