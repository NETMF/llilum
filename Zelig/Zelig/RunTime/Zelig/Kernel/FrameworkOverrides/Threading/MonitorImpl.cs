//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Threading.Monitor))]
    public static class MonitorImpl
    {
        //
        // Helper Methods
        //

        [NoInline]
        public static void Enter( Object obj )
        {
            SyncBlockTable.GetLock( obj ).Acquire();
        }

        [NoInline]
        public static void Exit( Object obj )
        {
            SyncBlockTable.GetLock( obj ).Release();
        }

        [NoInline]
        private static bool TryEnterTimeout( Object obj                 ,
                                             int    millisecondsTimeout )
        {
            return SyncBlockTable.GetLock( obj ).Acquire( (SchedulerTime)millisecondsTimeout );
        }

        [NoInline]
        private static bool ObjWait( bool   exitContext         ,
                                     int    millisecondsTimeout ,
                                     Object obj                 )
        {
            throw new NotImplementedException();
        }

        [NoInline]
        private static void ObjPulse( Object obj )
        {
            throw new NotImplementedException();
        }

        [NoInline]
        private static void ObjPulseAll( Object obj )
        {
            throw new NotImplementedException();
        }
    }
}
