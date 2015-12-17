//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Threading.EventWaitHandle))]
    public class EventWaitHandleImpl : WaitHandleImpl
    {
        protected sealed class EventWaitableObject : Synchronization.WaitableObject
        {
            //
            // State
            //

            volatile EventWaitHandleImpl m_owner;

            //
            // Constructor Methods
            //

            internal EventWaitableObject(EventWaitHandleImpl owner)
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            public override bool Acquire(SchedulerTime timeout)
            {
                ThreadImpl thisThread = ThreadImpl.CurrentThread;

                BugCheck.Assert(thisThread != null, BugCheck.StopCode.NoCurrentThread);

                //
                // Let's try to shortcut the acquisition of the event.
                //
                if (m_owner.m_state)
                {
                    using (SmartHandles.InterruptState.Disable())
                    {
                        if (m_owner.m_state)
                        {
                            if (m_owner.m_mode == System.Threading.EventResetMode.AutoReset)
                            {
                                m_owner.m_state = false;
                            }

                            return true;
                        }
                    }
                }

                using (Synchronization.WaitingRecord.Holder holder = Synchronization.WaitingRecord.Holder.Get(thisThread, this, timeout))
                {
                    while (true)
                    {
                        using (SmartHandles.InterruptState.Disable())
                        {
                            if (holder.ShouldTryToAcquire)
                            {
                                if (m_owner.m_state)
                                {
                                    if (m_owner.m_mode == System.Threading.EventResetMode.AutoReset)
                                    {
                                        m_owner.m_state = false;
                                    }

                                    return true;
                                }
                            }
                        }

                        if (holder.RequestProcessed)
                        {
                            return holder.RequestFulfilled;
                        }
                    }
                }
            }

            public override void Release()
            {
                while (true)
                {
                    ThreadImpl wakeUpThread;

                    using (SmartHandles.InterruptState.Disable())
                    {
                        if (m_owner.m_state == false)
                        {
                            return;
                        }

                        Synchronization.WaitingRecord wr = m_listWaiting.FirstTarget();
                        if (wr == null)
                        {
                            return;
                        }

                        wakeUpThread = wr.Source;

                        wr.RequestFulfilled = true;

                        if (m_owner.m_mode == System.Threading.EventResetMode.AutoReset)
                        {
                            m_owner.m_state = false;
                        }
                    }

                    wakeUpThread.Wakeup();
                }
            }

            //
            // Access Methods
            //

        }

        //
        // State
        //

        internal bool m_state;
        internal System.Threading.EventResetMode m_mode;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation]
        public EventWaitHandleImpl(bool initialState,
                                    System.Threading.EventResetMode mode)
        {
            m_state = initialState;
            m_mode = mode;

            //--//

            m_handle = new EventWaitableObject(this);
        }

        //
        // Helper Methods
        //

        public bool Reset()
        {
            m_state = false;

            return true;
        }

        public bool Set()
        {
            m_state = true;

            m_handle.Release();

            return true;
        }
    }
}
