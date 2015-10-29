//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Synchronization
{
    using System;
    using System.Runtime.CompilerServices;


    public sealed class WaitingRecord
    {
        //
        // HACK: We have a bug in the liveness of multi-pointer structure. We have to use a class instead.
        //
////    public struct Holder : IDisposable
        public class Holder : IDisposable
        {
            //
            // State
            //

            ThreadImpl                     m_thread;
            Synchronization.WaitableObject m_waitableObject;
            SchedulerTime                  m_timeout;
            WaitingRecord                  m_wr;

            //
            // Constructor Methods
            //

            internal Holder()
            {
            }

////        public Holder( ThreadImpl                     thread         ,
////                       Synchronization.WaitableObject waitableObject ,
////                       SchedulerTime                  timeout        )
////        {
////            m_thread         = thread;
////            m_waitableObject = waitableObject;
////            m_timeout        = timeout;
////            m_wr             = null;
////        }

            //
            // Helper Methods
            //

            public void Dispose()
            {
                if(m_wr != null)
                {
                    using(SmartHandles.InterruptState.Disable())
                    {
                        m_wr.Recycle();

                        m_thread         = null;
                        m_waitableObject = null;
                        m_wr             = null;
                    }
                }
            }

            //
            // HACK: We have a bug in the liveness of multi-pointer structure. We have to use a class instead. Use this instead of the parametrized constructor.
            //
            public static Holder Get( ThreadImpl                     thread         ,
                                      Synchronization.WaitableObject waitableObject ,
                                      SchedulerTime                  timeout        )
            {
                Holder hld = thread.m_holder;

                hld.m_thread         = thread;
                hld.m_waitableObject = waitableObject;
                hld.m_timeout        = timeout;
                hld.m_wr             = null;

                return hld;
            }

            //
            // Access Methods
            //

            public bool ShouldTryToAcquire
            {
                get
                {
                    return m_wr == null || m_wr.Processed == false;
                }
            }

            public bool RequestProcessed
            {
                get
                {
                    //
                    // We do two passes through the acquire phase.
                    //
                    // On the first pass, we don't allocate a WaitingRecord, we just try to acquire the resource.
                    // If that fails, we allocate a WaitingRecord, connect it and 
                    //
                    // On the second pass, we retry to acquire the resource and if that fails, we simply wait.
                    //
                    if(m_wr == null)
                    {
                        m_wr = WaitingRecord.GetInstance( m_thread, m_waitableObject, m_timeout );

                        using(SmartHandles.InterruptState.Disable())
                        {
                            m_wr.Connect();
                        }

                        return false;
                    }
                    else
                    {
                        m_wr.Wait();

                        return m_wr.Processed;
                    }
                }
            }

            public bool RequestFulfilled
            {
                get
                {
                    return m_wr.RequestFulfilled;
                }
            }
        }

        //
        // State
        //

        const int RecycleLimit = 32;

        static KernelList< WaitingRecord > s_recycledList;
        static int                         s_recycledCount;

        KernelNode< WaitingRecord >        m_linkTowardSource;
        KernelNode< WaitingRecord >        m_linkTowardTarget;
        ThreadImpl                         m_source;
        WaitableObject                     m_target;
        SchedulerTime                      m_timeout;
        volatile bool                      m_processed;
        volatile bool                      m_fulfilled;

        //
        // Constructor Methods
        //

        static WaitingRecord()
        {
            s_recycledList = new KernelList< WaitingRecord >();

            while(s_recycledCount < RecycleLimit)
            {
                WaitingRecord wr = new WaitingRecord();

                wr.Recycle();
            }
        }

        private WaitingRecord()
        {
            m_linkTowardSource = new KernelNode< WaitingRecord >( this );
            m_linkTowardTarget = new KernelNode< WaitingRecord >( this );
        }

        //
        // Helper Methods
        //

        static WaitingRecord GetInstance( ThreadImpl     source  ,
                                          WaitableObject target  ,
                                          SchedulerTime  timeout )
        {
            BugCheck.AssertInterruptsOn();

            WaitingRecord wr = null;
            
            if(s_recycledCount > 0)
            {
                using(SmartHandles.InterruptState.Disable())
                {
                    KernelNode< WaitingRecord > node = s_recycledList.ExtractFirstNode();
                    if(node != null)
                    {
                        wr = node.Target;

                        s_recycledCount--;
                    }
                }
            }

            if(wr == null)
            {
                wr = new WaitingRecord();
            }

            wr.m_source  = source;
            wr.m_target  = target;
            wr.m_timeout = timeout;

            return wr;
        }

        void Connect()
        {
            BugCheck.AssertInterruptsOff();

            m_target.RegisterWait( m_linkTowardTarget );
            m_source.RegisterWait( m_linkTowardSource );
        }

        void Wait()
        {
            ThreadManager.Instance.SwitchToWait( this );
        }

        void Recycle()
        {
            BugCheck.AssertInterruptsOff();

            Disconnect();

            if(s_recycledCount < RecycleLimit)
            {
                m_processed = false;
                m_fulfilled = false;

                s_recycledCount++;
                s_recycledList.InsertAtTail( m_linkTowardTarget );
            }
        }

        void Disconnect()
        {
            BugCheck.AssertInterruptsOff();

            if(m_linkTowardSource.IsLinked)
            {
                m_source.UnregisterWait( m_linkTowardSource );
            }

            if(m_linkTowardTarget.IsLinked)
            {
                m_target.UnregisterWait( m_linkTowardTarget );
            }

            m_target = null;
            m_source = null;
        }

        //
        // Access Methods
        //

        public ThreadImpl Source
        {
            get
            {
                return m_source;
            }
        }

        public WaitableObject Target
        {
            get
            {
                return m_target;
            }
        }

        public SchedulerTime Timeout
        {
            get
            {
                return m_timeout;
            }
        }

        public bool Processed
        {
            get
            {
                return m_processed;
            }
        }

        public bool RequestFulfilled
        {
            get
            {
                return m_fulfilled;
            }

            set
            {
                if(m_processed == false)
                {
                    m_fulfilled = value;
                    m_processed = true;

                    Disconnect();
                }
            }
        }
    }    
}
