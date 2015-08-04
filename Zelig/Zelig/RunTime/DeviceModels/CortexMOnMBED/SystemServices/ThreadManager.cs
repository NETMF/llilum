//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexMOnMBED
{
    using System;
    using Zelig.Runtime.Synchronization;
    using RT = Microsoft.Zelig.Runtime;

    
    public sealed class ThreadManager : RT.ThreadManager
    {
        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {
        }

        public override void InitializeBeforeStaticConstructors( )
        {
        }

        public override void Activate( )
        {
        }

        //--//

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

        public override void SetNextQuantumTimerIfNeeded( )
        {
            throw new NotImplementedException( );
        }

        public override void SetNextWaitTimer( RT.SchedulerTime nextTimeout )
        {
            throw new NotImplementedException( );
        }

        //--//

        public override void StartThreads( )
        {
            while( true )
            {
                RT.Configuration.ExecuteApplication( );
            }
        }

        public override void Yield( )
        {
            throw new NotImplementedException( );
        }
        
        public override void AddThread( RT.ThreadImpl thread )
        {
            throw new NotImplementedException( );
        }

        public override void RemoveThread( RT.ThreadImpl thread )
        {
            throw new NotImplementedException( );
        }

        public override void Reschedule( )
        {
            throw new NotImplementedException( );
        }

        public override void RetireThread( RT.ThreadImpl thread )
        {
            throw new NotImplementedException( );
        }

        public override void SwitchToWait( WaitingRecord wr )
        {
            throw new NotImplementedException( );
        }

        public override void TimeQuantumExpired( )
        {
            throw new NotImplementedException( );
        }

        public override void Wakeup( RT.ThreadImpl thread )
        {
            throw new NotImplementedException( );
        }
    }
}
