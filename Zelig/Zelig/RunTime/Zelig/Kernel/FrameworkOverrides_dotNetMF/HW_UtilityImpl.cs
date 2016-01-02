////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;


namespace Microsoft.Zelig.Runtime
{
    public static class Utility
    {
        static public uint ComputeCRC(byte[] buf, int offset, int length, uint crc)
        {
            // TODO TODO TODO: Implement
            throw new NotImplementedException( ); 
        }

        //--//
        
        static public void SetLocalTime(DateTime dt)
        {
            DateTimeImpl.SetUtcTime( dt ); 
        }
        
        static public TimeSpan GetMachineTime()
        {
            return new TimeSpan( (long)SchedulerTime.Now.Units ); 
        }
    }
}
