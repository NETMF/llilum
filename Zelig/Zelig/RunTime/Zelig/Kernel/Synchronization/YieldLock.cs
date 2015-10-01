//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Synchronization
{
    using System;
    using System.Runtime.CompilerServices;


    public sealed class YieldLock
    {
        //
        // State
        //

        volatile ThreadImpl m_ownerThread;
        volatile int        m_nestingCount;

        //
        // Constructor Methods
        //

        public YieldLock()
        {
        }

        //
        // Helper Methods
        //

        public void Acquire(ThreadImpl thisThread)
        {
            if(thisThread == null)
            {
                //
                // Special case for boot code path: all locks are transparent.
                //
                return;
            }

            while(true)
            {
                using(SmartHandles.InterruptState.Disable())
                {
                    if(m_ownerThread == null)
                    {
                        m_ownerThread = thisThread;
                        return;
                    }

                    if(m_ownerThread == thisThread)
                    {
                        m_nestingCount++;
                        return;
                    }

                    //bump priority to prevent priority inversion problems
                    if ((int)m_ownerThread.Priority < (int)thisThread.Priority)
                    {
                        m_ownerThread.Priority = thisThread.Priority;
                    }
                }

                thisThread.Yield();
            }
        }

        public void Release(ThreadImpl thisThread)
        {
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

            m_ownerThread = null;
        }

        //
        // Access Methods
        //

    }
}
