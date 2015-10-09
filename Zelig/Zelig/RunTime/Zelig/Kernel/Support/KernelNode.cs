//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;


    public sealed class KernelNode< T >
    {
        //
        // State
        //

        private KernelNode< T > m_next;
        private KernelNode< T > m_previous;
        private T               m_target;

        //
        // Constructor Methods
        //

        public KernelNode( T target )
        {
            m_target = target;
        }

        //
        // Helper Methods
        //

        public void InsertBefore( KernelNode< T > place )
        {
            if(m_next != null)
            {
                RemoveFromList_NoClear();
            }

            KernelNode< T > prev = place.m_previous;

            prev.ConnectToNext( this  );
            this.ConnectToNext( place );
        }

        public void RemoveFromList()
        {
            if(m_next != null)
            {
                RemoveFromList_NoClear();
            }

            m_next     = null;
            m_previous = null;
        }

        public KernelNode< T > MoveToNext()
        {
            KernelNode< T > node = this.Next;

            if(node.Next != null)
            {
                return node;
            }

            return null;
        }

        public KernelNode< T > MoveToPrevious()
        {
            KernelNode< T > node = this.Previous;

            if(node.Previous != null)
            {
                return node;
            }

            return null;
        }

        //--//

        [Inline]
        private void RemoveFromList_NoClear()
        {
            KernelNode< T > next = m_next;
            KernelNode< T > prev = m_previous;

            next.m_previous = prev;
            prev.m_next     = next;
        }

        [Inline]
        private void ConnectToNext( KernelNode< T > next )
        {
            this.m_next     = next;
            next.m_previous = this;
        }

        public bool VerifyUnlinked()
        {
            return m_next == null && m_previous == null;
        }

        //
        // Access Methods
        //

        public bool IsLinked
        {
            get
            {
                return m_next != null;
            }
        }

        public bool IsValidForForwardMove
        {
            get
            {
                return m_next != null;
            }
        }

        public bool IsValidForBackwardMove
        {
            get
            {
                return m_previous == null;
            }
        }

        public KernelNode< T > Next
        {
            get
            {
                return m_next;
            }

            internal set
            {
                m_next = value;
            }
        }

        public KernelNode< T > Previous
        {
            get
            {
                return m_previous;
            }

            internal set
            {
                m_previous = value;
            }
        }

        public T Target
        {
            get
            {
                return m_target;
            }
        }
    }
}
