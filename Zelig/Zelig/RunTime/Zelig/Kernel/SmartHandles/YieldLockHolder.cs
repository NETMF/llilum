//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.SmartHandles
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public struct YieldLockHolder : IDisposable
    {
        //
        // State
        //

        Synchronization.YieldLock m_target;

        ThreadImpl m_thisThread;

        System.Threading.ThreadPriority m_thisThreadPriority;

        //
        // Constructor Methods
        //

        [Inline]
        public YieldLockHolder( Synchronization.YieldLock target )
        {
            m_target = target;

            m_thisThread = ThreadImpl.CurrentThread;

            m_thisThreadPriority = (null != m_thisThread) ? m_thisThread.Priority : System.Threading.ThreadPriority.Normal;

            target.Acquire(m_thisThread);
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            m_target.Release(m_thisThread);

            //restore original priority in case
            //we were bumped during lock-execution by a higher priority thread.
            if ((null != m_thisThread) && (m_thisThread.Priority != m_thisThreadPriority))
            {
                m_thisThread.Priority = m_thisThreadPriority;
            }
        }
    }
}
