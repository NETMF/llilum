//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.SmartHandles
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public struct CriticalSectionHolder : IDisposable
    {
        //
        // State
        //

        Synchronization.CriticalSection m_target;

        //
        // Constructor Methods
        //

        [Inline]
        public CriticalSectionHolder( Synchronization.CriticalSection target )
        {
            m_target = target;

            target.Acquire();
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            m_target.Release();
        }
    }
}
