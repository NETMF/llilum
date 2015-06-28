//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.LLVMHosted
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class ThreadManager : RT.ThreadManager
    {

        public override void InitializeBeforeStaticConstructors( )
        {
            //
            // Create the first active thread.
            //
            m_mainThread = new RT.ThreadImpl( MainThread, new uint[ 1024 ] );

            //
            // We need to have a current thread during initialization, in case some static constructors try to access it.
            //
            RT.ThreadImpl.CurrentThread = m_mainThread;
        }

        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {

        }

        public override void Activate( )
        {
        }

        [RT.NoReturn]
        public override void StartThreads( )
        {
            m_mainThread.Start( );
        }

        //--//

        //
        // Helper Methods
        //

        public override void AddThread( RT.ThreadImpl thread )
        {

        }

        public override void RemoveThread( RT.ThreadImpl thread )
        {

        }

        public override void RetireThread( RT.ThreadImpl thread )
        {

        }

        //--//

        public override void Yield( )
        {

        }

        public override void SwitchToWait( RT.Synchronization.WaitingRecord wr )
        {

        }

        public override void Wakeup( RT.ThreadImpl thread )
        {

        }

        public override void TimeQuantumExpired( )
        {

        }

        public override void SetNextQuantumTimerIfNeeded( )
        {

        }

        public override void Reschedule( )
        {

        }

        public override void SetNextWaitTimer( RT.SchedulerTime nextTimeout )
        {

        }

        public override void CancelQuantumTimer( )
        {

        }

        public override void SetNextQuantumTimer( )
        {

        }

        public override void SetNextQuantumTimer( RT.SchedulerTime nextTimeout )
        {

        }

        private void MainThread( )
        {
            // For now it only calls the App entry point

            while( true )
            {
                RT.Configuration.ExecuteApplication( );
            }
        }

    }
}
