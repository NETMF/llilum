//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.SmartHandles
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public struct SwapCurrentThread : IDisposable
    {
        //
        // State
        //

        ThreadImpl m_previousThread;

        //
        // Constructor Methods
        //

        internal SwapCurrentThread( ThreadImpl newThread )
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                m_previousThread = ThreadImpl.CurrentThread;

                ThreadImpl.CurrentThread = newThread;
            }
        }

        //
        // Helper Methods
        //

        public void Dispose()
        {
            using(SmartHandles.InterruptState.DisableAll())
            {
                ThreadImpl.CurrentThread = m_previousThread;
            }
        }
    }
}
