// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ThreadState
**
**
** Purpose: Enum to represent the different thread states
**
**
=============================================================================*/

namespace System.Threading
{
    [Flags]
    [Serializable]
    public enum ThreadState
    {
        /*=========================================================================
        ** Constants for thread states.
        =========================================================================*/
        Running          = 0x000,
        StopRequested    = 0x001,
        SuspendRequested = 0x002,
        Background       = 0x004,
        Unstarted        = 0x008,
        Stopped          = 0x010,
        WaitSleepJoin    = 0x020,
        Suspended        = 0x040,
        AbortRequested   = 0x080,
        Aborted          = 0x100,
    }
}
