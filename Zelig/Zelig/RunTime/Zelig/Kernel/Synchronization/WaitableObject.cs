//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Synchronization
{
    using System;
    using System.Runtime.CompilerServices;


    public abstract class WaitableObject
    {
        //
        // State
        //

        protected KernelNode< WaitableObject > m_ownershipLink;
        protected KernelList< WaitingRecord  > m_listWaiting;

        //
        // Constructor Methods
        //

        protected WaitableObject()
        {
            m_ownershipLink = new KernelNode< WaitableObject >( this );
            m_listWaiting   = new KernelList< WaitingRecord  >();
        }

        //
        // Helper Methods
        //

        public abstract bool Acquire( SchedulerTime timeout );

        public abstract void Release();

        //--//

        public bool Acquire()
        {
            return Acquire( SchedulerTime.MaxValue );
        }

        public void Dispose()
        {
            using(SmartHandles.InterruptState.Disable())
            {
                WaitingRecord wr;

                while((wr = m_listWaiting.FirstTarget()) != null)
                {
                    wr.RequestFulfilled = false;
                }
            }
        }

        //--//

        public void RegisterWait( KernelNode< WaitingRecord > node )
        {
            m_listWaiting.InsertAtTail( node );
        }

        public void UnregisterWait( KernelNode< WaitingRecord > node )
        {
            node.RemoveFromList();
        }

        //
        // Access Methods
        //

        public KernelNode< WaitableObject > OwnershipLink
        {
            get
            {
                return m_ownershipLink;
            }
        }
    }
}
