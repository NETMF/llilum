// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: Stack
**
** Purpose: An array implementation of a stack.
**
**
=============================================================================*/
namespace System.Collections
{
    using System;
////using System.Security.Permissions;
    using System.Diagnostics;

    // A simple stack of objects.  Internally it is implemented as an array,
    // so Push can be O(n).  Pop is O(1).
////[DebuggerTypeProxy( typeof( System.Collections.Stack.StackDebugView ) )]
////[DebuggerDisplay( "Count = {Count}" )]
    [Serializable]
    public class Stack : ICollection, ICloneable
    {
        private const int cDefaultCapacity = 10;

        private Object[] m_array;     // Storage for stack elements
        private int      m_size;      // Number of items in the stack.
        private int      m_version;   // Used to keep enumerator in sync w/ collection.
        [NonSerialized]
        private Object   m_syncRoot;

        public Stack()
        {
            m_array   = new Object[cDefaultCapacity];
            m_size    = 0;
            m_version = 0;
        }

        // Create a stack with a specific initial capacity.  The initial capacity
        // must be a non-negative number.
        public Stack( int initialCapacity )
        {
            if(initialCapacity < 0)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentOutOfRangeException( "initialCapacity", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(initialCapacity < cDefaultCapacity)
            {
                initialCapacity = cDefaultCapacity;  // Simplify doubling logic in Push.
            }

            m_array   = new Object[initialCapacity];
            m_size    = 0;
            m_version = 0;
        }

        // Fills a Stack with the contents of a particular collection.  The items are
        // pushed onto the stack in the same order they are read by the enumerator.
        //
        public Stack( ICollection col ) : this( (col == null ? 32 : col.Count) )
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
                Push( en.Current );
            }
        }

        public virtual int Count
        {
            get
            {
                return m_size;
            }
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

        // Removes all Objects from the Stack.
        public virtual void Clear()
        {
            Array.Clear( m_array, 0, m_size ); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.

            m_size = 0;
            m_version++;
        }

        public virtual Object Clone()
        {
            Stack s = new Stack( m_size );

            s.m_size = m_size;

            Array.Copy( m_array, 0, s.m_array, 0, m_size );

            s.m_version = m_version;

            return s;
        }

        public virtual bool Contains( Object obj )
        {
            int count = m_size;

            while(count-- > 0)
            {
                object obj2 = m_array[count];

                if(obj == null)
                {
                    if(obj2 == null)
                    {
                        return true;
                    }
                }
                else if(obj2 != null && obj2.Equals( obj ))
                {
                    return true;
                }
            }

            return false;
        }

        // Copies the stack into an array.
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
                throw new ArgumentOutOfRangeException( "index", Environment.GetResourceString( "ArgumentOutOfRange_NeedNonNegNum" ) );
#else
                throw new ArgumentOutOfRangeException();
#endif
            }

            if(array.Length - index < m_size)
            {
#if EXCEPTION_STRINGS
                throw new ArgumentException( Environment.GetResourceString( "Argument_InvalidOffLen" ) );
#else
                throw new ArgumentException();
#endif
            }

            int i = 0;
            if(array is Object[])
            {
                Object[] objArray = (Object[])array;
                while(i < m_size)
                {
                    objArray[i + index] = m_array[m_size - i - 1];
                    i++;
                }
            }
            else
            {
                while(i < m_size)
                {
                    array.SetValue( m_array[m_size - i - 1], i + index );
                    i++;
                }
            }
        }

        // Returns an IEnumerator for this Stack.
        public virtual IEnumerator GetEnumerator()
        {
            return new StackEnumerator( this );
        }

        // Returns the top object on the stack without removing it.  If the stack
        // is empty, Peek throws an InvalidOperationException.
        public virtual Object Peek()
        {
            if(m_size == 0)
            {
#if EXCEPTION_STRINGS
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EmptyStack" ) );
#else
                throw new InvalidOperationException();
#endif
            }

            return m_array[m_size - 1];
        }

        // Pops an item from the top of the stack.  If the stack is empty, Pop
        // throws an InvalidOperationException.
        public virtual Object Pop()
        {
            if(m_size == 0)
            {
#if EXCEPTION_STRINGS
                throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EmptyStack" ) );
#else
                throw new InvalidOperationException();
#endif
            }

            m_version++;

            Object obj = m_array[--m_size];

            m_array[m_size] = null;     // Free memory quicker.

            return obj;
        }

        // Pushes an item to the top of the stack.
        //
        public virtual void Push( Object obj )
        {
            if(m_size == m_array.Length)
            {
                Object[] newArray = new Object[2 * m_array.Length];

                Array.Copy( m_array, 0, newArray, 0, m_size );

                m_array = newArray;
            }

            m_array[m_size++] = obj;
            m_version++;
        }

////    // Returns a synchronized Stack.
////    //
////    [HostProtection( Synchronization = true )]
////    public static Stack Synchronized( Stack stack )
////    {
////        if(stack == null)
////        {
////            throw new ArgumentNullException( "stack" );
////        }
////
////        return new SyncStack( stack );
////    }


        // Copies the Stack to an array, in the same order Pop would return the items.
        public virtual Object[] ToArray()
        {
            Object[] objArray = new Object[m_size];

            int i = 0;

            while(i < m_size)
            {
                objArray[i] = m_array[m_size - i - 1];
                i++;
            }

            return objArray;
        }

////    [Serializable]
////    private class SyncStack : Stack
////    {
////        private Stack  m_s;
////        private Object m_root;
////
////        internal SyncStack( Stack stack )
////        {
////            m_s    = stack;
////            m_root = stack.SyncRoot;
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
////                    return m_s.Count;
////                }
////            }
////        }
////
////        public override bool Contains( Object obj )
////        {
////            lock(m_root)
////            {
////                return m_s.Contains( obj );
////            }
////        }
////
////        public override Object Clone()
////        {
////            lock(m_root)
////            {
////                return new SyncStack( (Stack)m_s.Clone() );
////            }
////        }
////
////        public override void Clear()
////        {
////            lock(m_root)
////            {
////                m_s.Clear();
////            }
////        }
////
////        public override void CopyTo( Array array, int arrayIndex )
////        {
////            lock(m_root)
////            {
////                m_s.CopyTo( array, arrayIndex );
////            }
////        }
////
////        public override void Push( Object value )
////        {
////            lock(m_root)
////            {
////                m_s.Push( value );
////            }
////        }
////
////        public override Object Pop()
////        {
////            lock(m_root)
////            {
////                return m_s.Pop();
////            }
////        }
////
////        public override IEnumerator GetEnumerator()
////        {
////            lock(m_root)
////            {
////                return m_s.GetEnumerator();
////            }
////        }
////
////        public override Object Peek()
////        {
////            lock(m_root)
////            {
////                return m_s.Peek();
////            }
////        }
////
////        public override Object[] ToArray()
////        {
////            lock(m_root)
////            {
////                return m_s.ToArray();
////            }
////        }
////    }


        [Serializable]
        private class StackEnumerator : IEnumerator, ICloneable
        {
            private Stack  m_stack;
            private int    m_index;
            private int    m_version;
            private Object m_currentElement;

            internal StackEnumerator( Stack stack )
            {
                m_stack          = stack;
                m_version        = stack.m_version;
                m_index          = -2;
                m_currentElement = null;
            }

            public Object Clone()
            {
                return MemberwiseClone();
            }

            public virtual bool MoveNext()
            {
                bool retval;

                if(m_version != m_stack.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                if(m_index == -2)
                {  // First call to enumerator.
                    m_index = m_stack.m_size - 1;
                    retval = (m_index >= 0);
                    if(retval)
                    {
                        m_currentElement = m_stack.m_array[m_index];
                    }

                    return retval;
                }

                if(m_index == -1)
                {  // End of enumeration.
                    return false;
                }

                retval = (--m_index >= 0);
                if(retval)
                {
                    m_currentElement = m_stack.m_array[m_index];
                }
                else
                {
                    m_currentElement = null;
                }

                return retval;
            }

            public virtual Object Current
            {
                get
                {
                    if(m_index == -2)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    if(m_index == -1)
                    {
#if EXCEPTION_STRINGS
                        throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                        throw new InvalidOperationException();
#endif
                    }

                    return m_currentElement;
                }
            }

            public virtual void Reset()
            {
                if(m_version != m_stack.m_version)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumFailedVersion" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                m_index          = -2;
                m_currentElement = null;
            }
        }

////    internal class StackDebugView
////    {
////        private Stack stack;
////
////        public StackDebugView( Stack stack )
////        {
////            if(stack == null)
////            {
////                throw new ArgumentNullException( "stack" );
////            }
////
////            this.stack = stack;
////        }
////
////        [DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
////        public Object[] Items
////        {
////            get
////            {
////                return stack.ToArray();
////            }
////        }
////    }
    }
}
