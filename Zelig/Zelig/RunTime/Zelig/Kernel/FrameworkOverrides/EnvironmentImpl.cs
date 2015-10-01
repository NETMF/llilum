//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(Environment))]
    internal static class EnvironmentImpl
    {
        public static int TickCount
        {
            get
            {
                return (int)SchedulerTime.Now.Units;
            }
        }
    }
}
