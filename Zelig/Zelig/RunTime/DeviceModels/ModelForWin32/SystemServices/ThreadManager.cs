//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Runtime.Win32
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using RT = Microsoft.Zelig.Runtime;
    using LLOS = Zelig.LlilumOSAbstraction;
    using TS = Zelig.Runtime.TypeSystem;

    public sealed class ThreadManager : RT.ThreadManager
    {
        private static object                 s_deadThreadLock = new object();
        private unsafe LLOS.HAL.TimerContext* m_timer;
        private List<RT.Processor.Context>    m_deadThreads;
        private AutoResetEvent                m_evtThreadExit;
        private RT.ThreadImpl                 m_interruptThread;

        public override RT.ThreadImpl InterruptThread
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override RT.ThreadImpl FastInterruptThread
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override RT.ThreadImpl AbortThread
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //
        // State
        //

        //
        // Helper Methods
        //

        public override void InitializeBeforeStaticConstructors()
        {
            base.InitializeBeforeStaticConstructors();
        }

        public override void InitializeAfterStaticConstructors(uint[] systemStack)
        {
            base.InitializeAfterStaticConstructors(systemStack);

            m_deadThreads = new List<RT.Processor.Context>();
            m_evtThreadExit = new AutoResetEvent(false);

            m_interruptThread = new RT.ThreadImpl(null, new uint[ 128 ]);
        }

        //--//

        //
        // Extensibility 
        //

        private void WaitExpired( UIntPtr context, ulong time )
        {
            WaitExpired( RT.SchedulerTime.FromUnits( time ) ); 
        }


        public override void Activate()
        {
            base.Activate();

            unsafe
            {
                fixed (LLOS.HAL.TimerContext** ppTimer = &m_timer)
                {
                    LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_AllocateTimer(WaitExpired, UIntPtr.Zero, ulong.MaxValue, ppTimer), false);
                }
            }
        }

        public override void CancelQuantumTimer()
        {
        }

        public override void SetNextQuantumTimer()
        {
        }

        public override void SetNextQuantumTimer(RT.SchedulerTime nextTimeout)
        {
        }

        public override void TimeQuantumExpired()
        {
        }

        public unsafe override void SetNextWaitTimer(RT.SchedulerTime nextTimeout)
        {
            if(nextTimeout != RT.SchedulerTime.MaxValue)
            {
                ulong usecFromNow = LLOS.HAL.Clock.LLOS_CLOCK_GetClockTicks();

                if(usecFromNow > nextTimeout.Units)
                {
                    usecFromNow = 0;
                }
                else
                {
                    usecFromNow = nextTimeout.Units - usecFromNow;
                    usecFromNow *= 1000;
                }

                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer(m_timer, usecFromNow), false);
            }
            else
            {
                LLOS.LlilumErrors.ThrowOnError(LLOS.HAL.Timer.LLOS_SYSTEM_TIMER_ScheduleTimer(m_timer, ulong.MaxValue), false);
            }

        }

        protected override void IdleThread()
        {
            while(true)
            {
                m_evtThreadExit.WaitOne();

                lock (s_deadThreadLock)
                {
                    for(int i = 0; i<m_deadThreads.Count; i++)
                    {
                        ( (DeviceModels.Win32.Processor.Context)m_deadThreads[ i ] ).Retire();
                    }

                    m_deadThreads.Clear();
                }
            }
        }

        public override void SwitchToWait(RT.Synchronization.WaitingRecord wr)
        {
            RT.ThreadImpl thread = null;

            RT.BugCheck.AssertInterruptsOn();

            using(RT.SmartHandles.InterruptState hnd = RT.SmartHandles.InterruptState.Disable())
            {
                if(wr.Processed == false)
                {
                    thread = wr.Source;

                    m_waitingThreads.InsertAtTail(thread.SchedulingLink);

                    thread.State |= ThreadState.WaitSleepJoin;

                    InvalidateNextWaitTimer();
                }
            }

            if(thread != null)
            {
                var context = GetContextFromThread(thread);

                while(thread.IsWaiting)
                {
                    context.WaitForEvent(Timeout.Infinite);
                }
            }
        }

        public override void Wakeup(RT.ThreadImpl thread)
        {
            base.Wakeup(thread);

            GetContextFromThread(thread).SetEvent();
        }

        public override void Yield()
        {
            RT.BugCheck.AssertInterruptsOn();

            GetContextFromThread(ThreadManager.Instance.CurrentThread).Yield();
        }

        public override int DefaultStackSize
        {
            get
            {
                // Win32 stack size is determined by the OS
                return 4;
            }
        }

        public override void StartThreads()
        {
            RT.SmartHandles.InterruptState.Enable();

            GetContextFromThread(m_mainThread).Start();
            GetContextFromThread(m_idleThread).Start();

            base.StartThreads();
        }

        public override void AddThread(RT.ThreadImpl thread)
        {
            base.AddThread(thread);

            GetContextFromThread(thread).Start();
        }

        public override void RemoveThread(RT.ThreadImpl thread)
        {
            base.RemoveThread(thread);

            lock (s_deadThreadLock)
            {
                m_deadThreads.Add(GetContextFromThread(thread));
            }

            m_evtThreadExit.Set();
        }

        private DeviceModels.Win32.Processor.Context GetContextFromThread(RT.ThreadImpl thread)
        {
            return (DeviceModels.Win32.Processor.Context)thread.SwappedOutContext;
        }

        public override bool ShouldContextSwitch
        {
            get
            {
                return CurrentThread != m_nextThread;
            }
        }

        public override RT.ThreadImpl CurrentThread
        {
            get
            {
                RT.ThreadImpl currentThread = ( (DeviceModels.Win32.Processor)RT.Processor.Instance ).CurrentThread;

                if(currentThread == null && m_interruptThread != null)
                {
                    currentThread = m_interruptThread;
                }

                return currentThread;
            }
        }
    }
}
