//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;


    public struct KernelPerformanceCounter
    {
        //
        // State
        //

        uint  m_hits;
        uint  m_start;
        ulong m_total;

        //
        // Helper Methods
        //

        public void Start()
        {
            m_start = Peripherals.Instance.ReadPerformanceCounter();
        }

        public void Stop()
        {
            m_hits  += 1;
            m_total += (Peripherals.Instance.ReadPerformanceCounter() - m_start);
        }

        public void Merge( KernelPerformanceCounter other )
        {
            m_hits  += other.m_hits;
            m_total += other.m_total;
        }
    }
}
