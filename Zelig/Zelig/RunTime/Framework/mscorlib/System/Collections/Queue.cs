// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Queue
**
** Purpose: A circular-array implementation of a queue.
**
**
=============================================================================*/
namespace System.Collections
{
    using System;
////using System.Security.Permissions;
    using System.Diagnostics;

    // A simple Queue of objects.  Internally it is implemented as a circular
    // buffer, so Enqueue can be O(n).  Dequeue is O(1).
////[DebuggerTypeProxy( typeof( System.Collections.Queue.QueueDebugView ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    [Serializable]
    public class Queue : ICollection, ICloneable
    {
        private const int cMinimumGrow     = 4;
        private const int cShrinkThreshold = 32;

        private Object[] m_array;
        private int      m_head;       // First valid element in the queue
        private int      m_tail;       // Last valid element in the queue
        private int      m_size;       // Number of elements.
        private int      m_growFactor; // 100 == 1.0, 130 == 1.3, 200 == 2.0
        private int      m_version;

        [NonSerialized]
        private Object   m_syncRoot;

        // Creates a queue with room for capacity objects. The default initial
        // capacity and grow factor are used.
        public Queue() : this( 32, (float)2.0 )
        {
        }

        // Creates a queue with room for capacity objects. The default grow factor
        // is used.
        //
        public Queue( int capacity ) : this( capacity, (float)2.0 )
        {
        }

        // Creates a queue with room for capacity objects. When full, the new
        // capacity is set to the old capacity * growFactor.
        //
        public Queue( int capacity, float growFactor )
        {
            if(capacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "capacity", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(!(growFactor >= 1.0 && growFactor <= 10.0))
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "growFactor", Environment.GetResourceString( "ArgumentOutOfRange_QueueGrowFactor", 1, 10 ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            m_array      = new Object[capacity];
            m_head       = 0;
            m_tail       = 0;
            m_size       = 0;
            m_growFactor = (int)(growFactor * 100);
        }

        // Fills a Queue with the elements of an ICollection.  Uses the enumerator
        // to get each of the elements.
        //
        public Queue( ICollection col ) : this( (col == null ? 32 : col.Count) )
        {
            if(col == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "col" );
#else
                throw new ArgumentNullException();
#endif
            }

            IEnumerator en = col.GetEnumerator();
            while(en.MoveNext())
            {
                Enqueue( en.Current );
            }
        }


        public virtual int Count
        {
            get
            {
                return m_size;
            }
        }

        public virtual Object Clone()
        {
            Queue q = new Queue( m_size );

            q.m_size = m_size;

            int numToCopy = m_size;
            int firstPart = Math.Min( m_array.Length - m_head, numToCopy );

            Array.Copy( m_array, m_head, q.m_array, 0, firstPart );

            numToCopy -= firstPart;

            if(numToCopy > 0)
            {
                Array.Copy( m_array, 0, q.m_array, m_array.Length - m_head, numToCopy );
            }

            q.m_version = m_version;

            return q;
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual Object SyncRoot
        {
            get
            {
                if(m_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange( ref m_syncRoot, new Object(), null );
                }

                return m_syncRoot;
            }
        }

        // Removes all Objects from the queue.
        public virtual void Clear()
        {
            if(m_head < m_tail)
            {
                Array.Clear( m_array, m_head, m_size );
            }
            else
            {
                Array.Clear( m_array, m_head, m_array.Length - m_head );
                Array.Clear( m_array, 0     , m_tail                  );
            }

            m_head = 0;
            m_tail = 0;
            m_size = 0;
            m_version++;
        }

        // CopyTo copies a collection into an Array, starting at a particular
        // index into the array.
        //
        public virtual void CopyTo( Array array, int index )
        {
            if(array == null)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentNullException( "array" );
#else
                throw new ArgumentNullException();
#endif
            }

            if(array.Rank != 1)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Arg_RankMultiDimNotSupported" ) );
#else
                throw new ArgumentException();
#endif
            }

            if(index < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_Index" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            int arrayLen = array.Length;
            if(arrayLen - index < m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            int numToCopy = m_size;
            if(numToCopy == 0)
            {
                return;
            }

            int firstPart = Math.Min( m_array.Length - m_head, numToCopy );

            Array.Copy( m_array, m_head, array, index, firstPart );

            numToCopy -= firstPart;
            if(numToCopy > 0)
            {
                Array.Copy( m_array, 0, array, index + m_array.Length - m_head, numToCopy );
            }
        }

        // Adds obj to the tail of the queue.
        //
        public virtual void Enqueue( Object obj )
        {
            if(m_size == m_array.Length)
            {
                int newcapacity = (int)((long)m_array.Length * (long)m_growFactor / 100);

                if(newcapacity < m_array.Length + cMinimumGrow)
                {
                    newcapacity = m_array.Length + cMinimumGrow;
                }

                SetCapacity( newcapacity );
            }

            m_array[m_tail] = obj;

////        m_tail = (m_tail + 1) % m_array.Length;
            int tailNext = m_tail + 1;
            m_tail = (tailNext == m_array.Length) ? 0 : tailNext;
            m_size++;
            m_version++;
        }

        // GetEnumerator returns an IEnumerator over this Queue.  This
        // Enumerator will support removing.
        //
        public virtual IEnumerator GetEnumerator()
        {
            return new QueueEnumerator( this );
        }

        // Removes the object at the head of the queue and returns it. If the queue
        // is empty, this method simply returns null.
        public virtual Object Dequeue()
        {
            if(m_size == 0)
            {
#if EXCEPTION_STRINGS
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EmptyQueue" ) );
#else
                throw new InvalidOperationException();
#endif
            }

            Object removed = m_array[m_head];
            m_array[m_head] = null;

////        m_head = (m_head + 1) % m_array.Length;
            int headNext = m_head + 1;
            m_head = (headNext == m_array.Length) ? 0 : headNext;
            m_size--;
            m_version++;

            return removed;
        }

        // Returns the object at the head of the queue. The object remains in the
        // queue. If the queue is empty, this method throws an
        // InvalidOperationException.
        public virtual Object Peek()
        {
            if(m_size == 0)
            {
#if EXCEPTION_STRINGS
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EmptyQueue" ) );
#else
                throw new InvalidOperationException();
#endif
            }

            return m_array[m_head];
        }

////    // Returns a synchronized Queue.  Returns a synchronized wrapper
////    // class around the queue - the caller must not use references to the
////    // original queue.
////    //
////    [HostProtection( Synchronization = true )]
////    public static Queue Synchronized( Queue queue )
////    {
////        if(queue == null)
////        {
////            throw new ArgumentNullException( "queue" );
////        }
////
////        return new SynchronizedQueue( queue );
////    }

        // Returns true if the queue contains at least one object equal to obj.
        // Equality is determined using obj.Equals().
        //
        // Exceptions: ArgumentNullException if obj == null.
        public virtual bool Contains( Object obj )
        {
            int index = m_head;
            int count = m_size;

            while(count-- > 0)
            {
                if(obj == null)
                {
                    if(m_array[index] == null)
                    {
                        return true;
                    }
                }
                else if(m_array[index] != null && m_array[index].Equals( obj ))
                {
                    return true;
                }

////            index = (index + 1) % m_array.Length;
                index = (index + 1);
                if(index == m_array.Length)
                {
                    index = 0;
                }
            }

            return false;
        }

        internal Object GetElement( int i )
        {
            return m_array[(m_head + i) % m_array.Length];
        }

        // Iterates over the objects in the queue, returning an array of the
        // objects in the Queue, or an empty array if the queue is empty.
        // The order of elements in the array is first in to last in, the same
        // order produced by successive calls to Dequeue.
        public virtual Object[] ToArray()
        {
            Object[] arr = new Object[m_size];
            if(m_size == 0)
            {
                return arr;
            }

            if(m_head < m_tail)
            {
                Array.Copy( m_array, m_head, arr, 0, m_size );
            }
            else
            {
                Array.Copy( m_array, m_head, arr, 0                      , m_array.Length - m_head );
                Array.Copy( m_array, 0     , arr, m_array.Length - m_head, m_tail                  );
            }

            return arr;
        }


        // PRIVATE Grows or shrinks the buffer to hold capacity objects. Capacity
        // must be >= _size.
        private void SetCapacity( int capacity )
        {
            Object[] newarray = new Object[capacity];
            if(m_size > 0)
            {
                if(m_head < m_tail)
                {
                    Array.Copy( m_array, m_head, newarray, 0, m_size );
                }
                else
                {
                    Array.Copy( m_array, m_head, newarray, 0                      , m_array.Length - m_head );
                    Array.Copy( m_array, 0     , newarray, m_array.Length - m_head, m_tail                  );
                }
            }

            m_array = newarray;
            m_head  = 0;
            m_tail  = (m_size == capacity) ? 0 : m_size;
            m_version++;
        }

        public virtual void TrimToSize()
        {
            SetCapacity( m_size );
        }


////    // Implements a synchronization wrapper around a queue.
////    [Serializable]
////    private class SynchronizedQueue : Queue
////    {
////        private Queue  m_q;
////        private Object m_root;
////
////        internal SynchronizedQueue( Queue q )
////        {
////            m_q    = q;
////            m_root = q.SyncRoot;
////        }
////
////        public override bool IsSynchronized
////        {
////            get
////            {
////                return true;
////            }
////        }
////
////        public override Object SyncRoot
////        {
////            get
////            {
////                return m_root;
////            }
////        }
////
////        public override int Count
////        {
////            get
////            {
////                lock(m_root)
////                {
////                    return m_q.Count;
////                }
////            }
////        }
////
////        public override void Clear()
////        {
////            lock(m_root)
////            {
////                m_q.Clear();
////            }
////        }
////
////        public override Object Clone()
////        {
////            lock(m_root)
////            {
////                return new SynchronizedQueue( (Queue)m_q.Clone() );
////            }
////        }
////
////        public override bool Contains( Object obj )
////        {
////            lock(m_root)
////            {
////                return m_q.Contains( obj );
////            }
////        }
////
////        public override void CopyTo( Array array, int arrayIndex )
////        {
////            lock(m_root)
////            {
////                m_q.CopyTo( array, arrayIndex );
////            }
////        }
////
////        public override void Enqueue( Object value )
////        {
////            lock(m_root)
////            {
////                m_q.Enqueue( value );
////            }
////        }
////
////        public override Object Dequeue()
////        {
////            lock(m_root)
////            {
////                return m_q.Dequeue();
////            }
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            lock(m_root)
////            {
////                return m_q.GetEnumerator();
////            }
////        }
////
////        public override Object Peek()
////        {
////            lock(m_root)
////            {
////                return m_q.Peek();
////            }
////        }
////
////        public override Object[] ToArray()
////        {
////            lock(m_root)
////            {
////                return m_q.ToArray();
////            }
////        }
////
////        public override void TrimToSize()
////        {
////            lock(m_root)
////            {
////                m_q.TrimToSize();
////            }
////        }
////    }


        // Implements an enumerator for a Queue.  The enumerator uses the
        // internal version number of the list to ensure that no modifications are
        // made to the list while an enumeration is in progress.
        [Serializable]
        private class QueueEnumerator : IEnumerator, ICloneable
        {
            private Queue  m_q;
            private int    m_index;
            private int    m_version;
            private Object m_currentElement;

            internal QueueEnumerator( Queue q )
            {
                m_q              = q;
                m_version        = q.m_version;
                m_index          = 0;
                m_currentElement = q.m_array;

                if(q.m_size == 0)
                {
                    m_index = -1;
                }
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                if(m_version != m_q.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_index < 0)
                {
                    m_currentElement = m_q.m_array;
                    return false;
                }

                m_currentElement = m_q.GetElement( m_index );
                m_index++;

                if(m_index == m_q.m_size)
                {
                    m_index = -1;
                }

                return true;
            }

            public virtual Object Current
            {
                get
                {
                    if(m_currentElement == m_q.m_array)
                    {
                        if(m_index == 0)
                        {
#if EXCEPTION_STRINGS
                            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                            throw new InvalidOperationException();
#endif
                        }
                        else
                        {
#if EXCEPTION_STRINGS
                            throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                            throw new InvalidOperationException();
#endif
                        }
                    }

                    return m_currentElement;
                }
            }

            public virtual void Reset()
            {
                if(m_version != m_q.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_q.m_size == 0)
                {
                    m_index = -1;
                }
                else
                {
                    m_index = 0;
                }

                m_currentElement = m_q.m_array;
            }
        }

////    internal class QueueDebugView
////    {
////        private Queue queue;
////
////        public QueueDebugView( Queue queue )
////        {
////            if(queue == null)
////            {
////                throw new ArgumentNullException( "queue" );
////            }
////
////            this.queue = queue;
////        }
////
////        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
////        public Object[] Items
////        {
////            get
////            {
////                return queue.ToArray();
////            }
////        }
////    }
    }
}
