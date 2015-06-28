//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass("System_CurrentSystemTimeZone", NoConstructors=true)]
    public class CurrentSystemTimeZoneImpl
    {
        //
        // Helper Methods
        //

        internal static int nativeGetTimeZoneMinuteOffset()
        {
            return -9 * 60;
        }

        internal static String nativeGetDaylightName()
        {
            return "";
        }

        internal static String nativeGetStandardName()
        {
            return "";
        }

        internal static short[] nativeGetDaylightChanges()
        {
            return new short[] { 0,  3, 0, 2, 2, 0, 0, 0,
                                 0, 11, 0, 1, 2, 0, 0, 0, 
                                 60 };
        }
    }
}
