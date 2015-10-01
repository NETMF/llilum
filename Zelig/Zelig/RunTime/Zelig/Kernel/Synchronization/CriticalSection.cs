//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Synchronization
{
    using System;
    using System.Runtime.CompilerServices;


    public sealed class CriticalSection : WaitableObject
    {
        public static class Configuration
        {
            public static bool ImmediatelyTransferOwnership
            {
                [ConfigurationOption("CriticalSection__ImmediatelyTransferOwnership")]
                get
                {
                    return false;
                }
            }

            public static bool AvoidPriorityInversionOnRelease
            {
                [ConfigurationOption("CriticalSection__AvoidPriorityInversionOnRelease")]
                get
                {
                    return false;
                }
            }
        }

        //
        // State
        //

        volatile ThreadImpl m_ownerThread;
        volatile int        m_nestingCount;

        //
        // Constructor Methods
        //

        public CriticalSection()
        {
        }

        //
        // Helper Methods
        //

        public override bool Acquire( SchedulerTime timeout )
        {
            ThreadImpl thisThread = ThreadImpl.CurrentThread;

            if(thisThread == null)
            {
                //
                // Special case for boot code path: all locks are transparent.
                //
                return true;
            }

            //
            // Fast shortcut for non-contended case.
            //
            if(m_ownerThread == null)
            {
#if ENABLE_GENERICS_BUG
#pragma warning disable 420
                if(System.Threading.Interlocked.CompareExchange< ThreadImpl >( ref m_ownerThread, thisThread, null ) == null)
#pragma warning restore 420
                {
                    thisThread.AcquiredWaitableObject( this );
                    return true;
                }
#endif // ENABLE_GENERICS_BUG
            }

            //
            // Fast shortcut for nested calls.
            //
            if(m_ownerThread == thisThread)
            {
                m_nestingCount++;
                return true;
            }

            using(Synchronization.WaitingRecord.Holder holder = WaitingRecord.Holder.Get( thisThread, this, timeout ))
            {
                while(true)
                {
                    bool fNotify = false;
                    bool fResult = false;

                    using(SmartHandles.InterruptState.Disable())
                    {
                        if(holder.ShouldTryToAcquire)
                        {
                            if(m_ownerThread == null)
                            {
                                m_ownerThread = thisThread;

                                fNotify = true;
                                fResult = true;
                            }
                            else
                            {
                                if(m_ownerThread == thisThread)
                                {
                                    m_nestingCount++;

                                    fResult = true;
                                }
                            }
                        }
                    }

                    if(fNotify)
                    {
                        thisThread.AcquiredWaitableObject( this );
                    }

                    if(fResult)
                    {
                        return fResult;
                    }

                    if(holder.RequestProcessed)
                    {
                        return holder.RequestFulfilled;
                    }
                }
            }
        }

        public override void Release()
        {
            ThreadImpl thisThread = ThreadImpl.CurrentThread;

            if(thisThread == null)
            {
                //
                // Special case for boot code path: all locks are transparent.
                //
                return;
            }

            if(m_ownerThread != thisThread)
            {
#if EXCEPTION_STRINGS
                throw new Exception( "Releasing waitable object not owned by thread" );
#else
                throw new Exception();
#endif
            }

            if(m_nestingCount > 0)
            {
                m_nestingCount--;
                return;
            }

            thisThread.ReleasedWaitableObject( this );

            ThreadImpl ownerThread  = null;
            ThreadImpl wakeupThread = null;

            using(SmartHandles.InterruptState.Disable())
            {
                WaitingRecord wr = m_listWaiting.FirstTarget();

                if(wr != null)
                {
                    wakeupThread = wr.Source;

                    if( Configuration.ImmediatelyTransferOwnership                                                    ||
                       (Configuration.AvoidPriorityInversionOnRelease && thisThread.Priority < wakeupThread.Priority)  )
                    {
                        ownerThread = wakeupThread;

                        wr.RequestFulfilled = true;
                    }
                }

                m_ownerThread = ownerThread;
            }

            if(ownerThread != null)
            {
                ownerThread.AcquiredWaitableObject( this );
            }

            if(wakeupThread != null)
            {
                wakeupThread.Wakeup();
            }
        }

        //--//

        //
        // Access Methods
        //

    }
}
