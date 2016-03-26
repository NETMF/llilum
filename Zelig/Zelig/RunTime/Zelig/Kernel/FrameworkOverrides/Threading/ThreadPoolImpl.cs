//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Threading.ThreadPool))]
    public static class ThreadPoolImpl
    {
        const int c_RecycleLimit = 32;

        internal class WorkItem
        {
            //
            // State
            //

            internal WaitCallback m_callBack;
            internal Object       m_state;
        }

        internal class Engine
        {
            Queue< WorkItem > m_queue;
            Queue< WorkItem > m_queueFree;
            AutoResetEvent    m_wakeup;
            int               m_maxThreads;
            int               m_activeThreads;
            int               m_busyThreads;

            //
            // Helper Methods
            //
            
            internal Engine( )
            {
                m_queue        = new Queue< WorkItem >();
                m_queueFree    = new Queue< WorkItem >();
                m_wakeup       = new AutoResetEvent( false );
                m_maxThreads   = Configuration.DefaultThreadPoolThreads;
            }

            internal Engine( int maxThreads ) : this()
            {
                m_maxThreads = maxThreads;
            }


            //
            // Helper Methods
            //

            internal void Queue( WaitCallback callBack ,
                                 Object       state    )
            {
                WorkItem item = null;

                if(m_queueFree.Count > 0)
                {
                    lock(m_queueFree)
                    {
                        if(m_queueFree.Count > 0)
                        {
                            item = m_queueFree.Dequeue();
                        }
                    }
                }

                if(item == null)
                {
                    item = new WorkItem();
                }

                item.m_callBack = callBack;
                item.m_state    = state;

                int count;

                lock(m_queue)
                {
                    m_queue.Enqueue( item );

                    count = m_queue.Count;
                }

                if(count == 1)
                {
                    m_wakeup.Set();
                }

                int active = m_activeThreads;

                if(active == m_busyThreads)
                {
                    if(active < m_maxThreads)
                    {
                        if(Interlocked.CompareExchange( ref m_activeThreads, active + 1, active ) == active)
                        {
                            Thread worker = new Thread( Worker );

                            worker.IsBackground = true;
                            worker.Start();
                        }
                    }
                }
            }

            internal void SetMaxThreads( int workerThreads )
            {
                m_maxThreads = workerThreads;

                m_wakeup.Set();
            }

            private void Worker()
            {
                while(m_activeThreads <= m_maxThreads)
                {
                    m_wakeup.WaitOne();

                    while(m_queue.Count > 0)
                    {
                        WorkItem item;
                        int      count;

                        lock(m_queue)
                        {
                            count = m_queue.Count;

                            if(count > 0)
                            {
                                item = m_queue.Dequeue();
                                count--;
                            }
                            else
                            {
                                item = null;
                            }
                        }

                        if(item != null)
                        {
                            if(count != 0)
                            {
                                m_wakeup.Set();
                            }

                            WaitCallback callBack = item.m_callBack;
                            object       state    = item.m_state;

                            if(m_queueFree.Count < c_RecycleLimit)
                            {
                                item.m_callBack = null;
                                item.m_state    = null;

                                lock(m_queueFree)
                                {
                                    m_queueFree.Enqueue( item );
                                }
                            }

                            Interlocked.Increment( ref m_busyThreads );

                            try
                            {
                                callBack( state );
                            }
                            catch
                            {
                            }

                            Interlocked.Decrement( ref m_busyThreads );
                        }
                    }
                }

                Interlocked.Decrement( ref m_activeThreads );
            }
        }

        //
        // State
        //

        private static Engine s_engine = new Engine();

        //--//

        //
        // Helper Methods
        //

        public static bool QueueUserWorkItem( WaitCallback callBack ,
                                              Object       state    )
        {
            s_engine.Queue( callBack, state );
            return true;
        }

        [Inline]
        public static bool QueueUserWorkItem( WaitCallback callBack )
        {
            return QueueUserWorkItem( callBack, null );
        }

        public static bool SetMaxThreads( int workerThreads         ,
                                          int completionPortThreads )
        {
            s_engine.SetMaxThreads( workerThreads );

            return true;
        }

        //--//

        //
        // Access Methods
        //

    }
}
