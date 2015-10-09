//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;


    public sealed class KernelCircularBuffer< T >
    {
        //
        // State
        //

        private readonly int              m_size;
        private readonly T[]              m_array;
        private readonly ManualResetEvent m_writerEvent;
        private readonly ManualResetEvent m_readerEvent;

        private          int              m_count;
        private          int              m_writerPos;
        private          int              m_readerPos;

        //
        // Constructor Methods
        //

        public KernelCircularBuffer( int size )
        {
            m_size        = size;
            m_array       = new T[size];
            m_writerEvent = new ManualResetEvent( true  );
            m_readerEvent = new ManualResetEvent( false );

            m_count       = 0;
            m_writerPos   = 0;
            m_readerPos   = 0;
        }

        //
        // Helper Methods
        //

        public void Clear()
        {
            BugCheck.AssertInterruptsOff();

            m_writerEvent.Set  ();
            m_readerEvent.Reset();

            m_count     = 0;
            m_writerPos = 0;
            m_readerPos = 0;
        }

        public bool Peek( out T val )
        {
            BugCheck.AssertInterruptsOff();

            if(this.IsEmpty)
            {
                val = default(T);

                return false;
            }
            else
            {
                val = m_array[m_readerPos];

                return true;
            }
        }

        //--//

        public bool EnqueueNonblocking( T val )
        {
            BugCheck.AssertInterruptsOff();

            if(this.IsFull)
            {
                return false;
            }

            if(this.IsEmpty)
            {
                m_readerEvent.Set();
            }

            int pos = m_writerPos;

            m_array[pos] = val;

            m_writerPos = NextPosition( pos );
            m_count++;

            if(this.IsFull)
            {
                m_writerEvent.Reset();
            }

            return true;
        }

        public bool DequeueNonblocking( out T val )
        {
            BugCheck.AssertInterruptsOff();

            if(this.IsEmpty)
            {
                val = default(T);

                return false;
            }

            if(this.IsFull)
            {
                m_writerEvent.Set();
            }

            int pos = m_readerPos;

            val = m_array[pos];

            m_readerPos = NextPosition( pos );
            m_count--;

            if(this.IsEmpty)
            {
                m_readerEvent.Reset();
            }

            return true;
        }

        //--//

        public void EnqueueBlocking( T val )
        {
            BugCheck.AssertInterruptsOn();

            while(true)
            {
                bool fSent;

                using(SmartHandles.InterruptState.Disable())
                {
                    fSent = EnqueueNonblocking( val );
                }

                if(fSent)
                {
                    return;
                }

                m_writerEvent.WaitOne();
            }
        }

        public T DequeueBlocking()
        {
            BugCheck.AssertInterruptsOn();

            while(true)
            {
                bool fReceived;
                T    val;

                using(SmartHandles.InterruptState.Disable())
                {
                    fReceived = DequeueNonblocking( out val );
                }

                if(fReceived)
                {
                    return val;
                }

                m_readerEvent.WaitOne();
            }
        }

        //--//

        public bool EnqueueBlocking( int timeout ,
                                     T   val     )
        {
            BugCheck.AssertInterruptsOn();

            while(true)
            {
                bool fSent;

                using(SmartHandles.InterruptState.Disable())
                {
                    fSent = EnqueueNonblocking( val );
                }

                if(fSent)
                {
                    return true;
                }

                if(m_writerEvent.WaitOne( timeout, false ) == false)
                {
                    return false;
                }
            }
        }

        public bool DequeueBlocking(     int timeout ,
                                     out T   val     )
        {
            BugCheck.AssertInterruptsOn();

            while(true)
            {
                bool fReceived;

                using(SmartHandles.InterruptState.Disable())
                {
                    fReceived = DequeueNonblocking( out val );
                }

                if(fReceived)
                {
                    return true;
                }

                if(m_readerEvent.WaitOne( timeout, false ) == false)
                {
                    return false;
                }
            }
        }

        //--//

        [Inline]
        private int NextPosition( int val )
        {
            val = val + 1;

            if(val == m_size)
            {
                return 0;
            }

            return val;
        }

        [Inline]
        private int PreviousPosition( int val )
        {
            if(val == 0)
            {
                val = m_size;
            }

            return val - 1;
        }

        //
        // Access Methods
        //

        public int Count
        {
            get
            {
                return m_count;
            }
        }

        public int RemainingCapacity
        {
            get
            {
                return m_size - m_count;
            }
        }

        public bool IsEmpty
        {
            [Inline]
            get
            {
                return m_count == 0;
            }
        }

        public bool IsFull
        {
            [Inline]
            get
            {
                return m_count == m_size;
            }
        }
    }
}
