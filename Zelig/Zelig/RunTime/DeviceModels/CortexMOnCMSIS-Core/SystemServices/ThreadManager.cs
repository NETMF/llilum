//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexMOnCMSISCore
{
    using System;

    using RT            = Microsoft.Zelig.Runtime;
    

    public sealed class ThreadManager : RT.ThreadManager
    {
        public override void CancelQuantumTimer( )
        {
            throw new NotImplementedException( );
        }

        public override void SetNextQuantumTimer( )
        {
            throw new NotImplementedException( );
        }

        public override void SetNextQuantumTimer( RT.SchedulerTime nextTimeout )
        {
            throw new NotImplementedException( );
        }

        public override void SetNextWaitTimer( RT.SchedulerTime nextTimeout )
        {
            throw new NotImplementedException( );
        }
    }
}
