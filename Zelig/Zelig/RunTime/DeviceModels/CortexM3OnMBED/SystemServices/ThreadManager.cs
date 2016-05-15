//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM3OnMBED
{
    using System;

    using RT            = Microsoft.Zelig.Runtime;
    using HAL           = Microsoft.Zelig.LlilumOSAbstraction.HAL;
    using ARMv7         = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;
    using ChipsetModel  = Microsoft.CortexM3OnCMSISCore;


    public abstract class ThreadManager : ChipsetModel.ThreadManager
    {
        //
        // State
        //

        //
        // BUGBUG: we need to Dispose this object on shutdown !!!
        //
        protected Drivers.SystemTimer.Timer m_timerForWaits;
        protected RT.ThreadImpl             m_exceptionThread;

        //--//

        //
        // Helper methods
        //

        public override void InitializeAfterStaticConstructors( uint[] systemStack )
        {
            base.InitializeAfterStaticConstructors( systemStack );
            
            //
            // The exception thread wraps the main stack pointer
            //
            m_exceptionThread = new RT.ThreadImpl( RT.Bootstrap.Initialization, GetMainStack( ) );

            //
            // The msp thread is never started, so we have to manually register them, to enable the debugger to see them.
            //
            RegisterThread(m_exceptionThread);

            //--//

            m_exceptionThread.SetupForExceptionHandling( unchecked((uint)ARMv7.ProcessorARMv7M.IRQn_Type.Reset_IRQn) );
        }

        public override void Activate()
        {
            base.Activate();

            m_timerForWaits = Drivers.SystemTimer.Instance.CreateTimer(WaitExpired);

            DeviceModels.Chipset.CortexM0.Drivers.InterruptController.Instance.Activate();
        }

        public override void SetNextWaitTimer(RT.SchedulerTime nextTimeout)
        {
            if (nextTimeout != RT.SchedulerTime.MaxValue)
            {
                m_timerForWaits.Timeout = nextTimeout.Units;
            }
            else
            {
                m_timerForWaits.Cancel();
            }
        }

        //
        // Access methods
        //

        public override RT.ThreadImpl InterruptThread
        {
            get
            {
                return m_exceptionThread;
            }
        }

        public override RT.ThreadImpl FastInterruptThread
        {
            get
            {
                return m_exceptionThread;
            }
        }

        public override RT.ThreadImpl AbortThread
        {
            get
            {
                return m_exceptionThread;
            }
        }

        //--//

        protected void WaitExpired(Drivers.SystemTimer.Timer sysTickTimer, ulong currentTime)
        {
            WaitExpired(RT.SchedulerTime.FromUnits(currentTime));
        }

        protected uint[] GetMainStack()
        {
            //
            // The main stack address will have to have at least additional 12 bytes 
            // to inject the ObjectHeader and the ArrayImpl members. 
            // TODO: find a better way to keep this in sync with ArrayImpl. 
            //
            uint correction = RT.MemoryFreeBlock.FixedSize();
            RT.BugCheck.Assert( correction == 12, RT.BugCheck.StopCode.StackCorruptionDetected ); 

            uint stackAddress = HAL.Thread.LLOS_THREAD_GetMainStackAddress( ) - correction;
            uint stackSize    = HAL.Thread.LLOS_THREAD_GetMainStackSize( ); 

            return RT.ArrayImpl.InitializeFromRawMemory( new UIntPtr( stackAddress ), stackSize ); 
        }
    }
}
