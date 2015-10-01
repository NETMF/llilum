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

            if(m_previousThread != null)
            {
                m_previousThread.ReleasedProcessor();
            }

            newThread.AcquiredProcessor();

            ThreadImpl.CurrentThread = newThread;
        }

        //
        // Helper Methods
        //

        public void Dispose()
        {
            BugCheck.AssertInterruptsOff();

            ThreadImpl newThread = ThreadImpl.CurrentThread;

            if(newThread != null)
            {
                newThread.ReleasedProcessor();
            }

            if(m_previousThread != null)
            {
                m_previousThread.AcquiredProcessor();
            }

            ThreadImpl.CurrentThread = m_previousThread;
        }
    }
}