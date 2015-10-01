//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;


    public sealed class KernelList< T >
    {
        //
        // State
        //

        private KernelNode< T > m_head;
        private KernelNode< T > m_tail;

        //
        // Constructor Methods
        //

        public KernelList()
        {
            m_head = new KernelNode< T >( default(T) );
            m_tail = new KernelNode< T >( default(T) );

            m_head.Next     = m_tail;
            m_tail.Previous = m_head;
        }

        //
        // Helper Methods
        //

        public void InsertAtTail( KernelNode< T > node )
        {
            node.InsertBefore( m_tail );
        }

        public KernelNode< T > FirstNode()
        {
            KernelNode< T > node = m_head.Next;

            if(node != m_tail)
            {
                return node;
            }

            return null;
        }

        public T FirstTarget()
        {
            KernelNode< T > node = m_head.Next;

            if(node != m_tail)
            {
                return node.Target;
            }

            return default(T);
        }

        public KernelNode< T > LastNode()
        {
            KernelNode< T > node = m_tail.Previous;

            if(node != m_head)
            {
                return node;
            }

            return null;
        }

        public T LastTarget()
        {
            KernelNode< T > node = m_tail.Previous;

            if(node != m_head)
            {
                return node.Target;
            }

            return default( T );
        }

        //--//

        public KernelNode< T > ExtractFirstNode()
        {
            KernelNode< T > node = m_head.Next;

            if(node != m_tail)
            {
                node.RemoveFromList();

                return node;
            }

            return null;
        }

        //
        // Access Methods
        //

        public KernelNode< T > StartOfForwardWalk
        {
            get
            {
                return m_head.Next;
            }
        }

        public KernelNode< T > StartOfForBackwardWalk
        {
            get
            {
                return m_tail.Previous;
            }
        }
    }
}
