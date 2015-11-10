//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.SmartHandles
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public struct SwapCurrentThreadUnderInterrupt : IDisposable
    {
        //
        // State
        //

        ThreadImpl m_previousThread;

        internal ThreadImpl PreviousThread
        {
            get
            {
                return m_previousThread;
            }
        }

        //
        // Constructor Methods
        //

        internal SwapCurrentThreadUnderInterrupt( ThreadImpl newThread )
        {
            BugCheck.AssertInterruptsOff();

            m_previousThread = ThreadImpl.CurrentThread;

#if USE_THREAD_PERFORMANCE_COUNTER
            if(m_previousThread != null)
            {
                m_previousThread.ReleasedProcessor();
            }

            newThread.AcquiredProcessor();
#endif // USE_THREAD_PERFORMANCE_COUNTER

            ThreadImpl.CurrentThread = newThread;
        }

        //
        // Helper Methods
        //

        public void Dispose()
        {
            BugCheck.AssertInterruptsOff();

            ThreadImpl newThread = ThreadImpl.CurrentThread;

#if USE_THREAD_PERFORMANCE_COUNTER
            if(newThread != null)
            {
                newThread.ReleasedProcessor();
            }

            if(m_previousThread != null)
            {
                m_previousThread.AcquiredProcessor();
            }
#endif // USE_THREAD_PERFORMANCE_COUNTER

            ThreadImpl.CurrentThread = m_previousThread;
        }
    }
}