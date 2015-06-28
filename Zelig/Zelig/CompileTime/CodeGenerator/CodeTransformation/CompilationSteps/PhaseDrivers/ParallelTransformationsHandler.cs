//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//
#define USE_THREAD_POOL

namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;
    using System.Threading;


    public class ParallelTransformationsHandler : IDisposable
    {
        public delegate void NotificationCallback( Operation phase, MethodRepresentation md, ref object state );

        public enum Operation
        {
            Initialize,
            Execute,
            Shutdown,
        }

        internal class Worker
        {
            //
            // State
            //

            private          ParallelTransformationsHandler  m_owner;
            private volatile MethodRepresentation            m_md;

            private          List< Exception >               m_errors;

            private          object                          m_state;
#if !USE_THREAD_POOL
            private          System.Threading.Thread         m_thread;
#endif
            private          System.Threading.AutoResetEvent m_waitForRequest;

            //
            // Constructor Methods
            //

            internal Worker( ParallelTransformationsHandler owner )
            {
                m_owner          = owner;

                m_errors         = new List< Exception >();

                m_state          = null;
#if !USE_THREAD_POOL
                m_thread         = new System.Threading.Thread( Execute );
#endif
                m_waitForRequest = new System.Threading.AutoResetEvent( false );

                //--//

#if USE_THREAD_POOL
                ThreadPool.QueueUserWorkItem( Execute );
#else
                m_thread.Priority     = System.Threading.ThreadPriority.BelowNormal;
                m_thread.IsBackground = true;
                m_thread.Start();
#endif
            }

            //
            // Helper Methods
            // 

            internal void Post( MethodRepresentation md )
            {
                m_md = md;
                m_waitForRequest.Set();
            }

            internal void Stop()
            {
                m_md = null;
                m_waitForRequest.Set();

#if !USE_THREAD_POOL
                m_thread.Join();
#endif

                if(m_errors.Count > 0)
                {
                    throw new Exception( "Delivering exception from worker thread", m_errors[0] );
                }
            }

            //--//

            private void Execute(object state)
            {
                lock(m_owner.m_availableWorkers)
                {
                    Notify( Operation.Initialize, null );
                }

                while(true)
                {
                    lock(m_owner.m_availableWorkers)
                    {
                        m_owner.m_availableWorkers.Enqueue( this );
                    }

                    m_owner.m_availableSemaphore.Release();

                    m_waitForRequest.WaitOne();

                    MethodRepresentation md = m_md;

                    if(md == null)
                    {
                        break;
                    }

                    try
                    {
                        using(ControlFlowGraphState.LockThreadToMethod( md ))
                        {
                            Notify( Operation.Execute, md );

                            if(m_owner.m_mdCallback != null)
                            {
                                m_owner.m_mdCallback( md );
                            }

                            if(m_owner.m_cfgCallback != null)
                            {
                                ControlFlowGraphStateForCodeTransformation cfg = TypeSystemForCodeTransformation.GetCodeForMethod( md );
                                if(cfg != null)
                                {
                                    m_owner.m_cfgCallback( cfg );
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        m_errors.Add( ex );
                    }
                }

                lock(m_owner.m_availableWorkers)
                {
                    Notify( Operation.Shutdown, null );
                }
            }

            private void Notify( Operation            phase ,
                                 MethodRepresentation md    )
            {
                if(m_owner.m_notificationCallback != null)
                {
                    m_owner.m_notificationCallback( phase, md, ref m_state );
                }
            }
        }

        //
        // State
        //

        static int s_maximumNumberOfProcessorsToUse = int.MaxValue;

        //--//

        MethodEnumerationCallback           m_mdCallback;
        ControlFlowGraphEnumerationCallback m_cfgCallback;
        NotificationCallback                m_notificationCallback;

        int                                 m_procs;
        Worker[]                            m_workers;
        System.Threading.Semaphore          m_availableSemaphore;
        Queue< Worker >                     m_availableWorkers;

        //
        // Constructor Methods
        //

        public ParallelTransformationsHandler( MethodEnumerationCallback callback ) : this( callback, null, null )
        {
        }

        public ParallelTransformationsHandler( ControlFlowGraphEnumerationCallback callback ) : this( null, callback, null )
        {
        }

        public ParallelTransformationsHandler( MethodEnumerationCallback           mdCallback           ,
                                               ControlFlowGraphEnumerationCallback cfgCallback          ,
                                               NotificationCallback                notificationCallback )
        {
            int procs = (int)BitVector.CountBits( (uint)(System.Diagnostics.Process.GetCurrentProcess().ProcessorAffinity.ToInt32()) ) + 1;

            if(procs > s_maximumNumberOfProcessorsToUse) procs = s_maximumNumberOfProcessorsToUse;
            if(procs < 1                               ) procs = 1;

            //--//

            m_mdCallback           = mdCallback;
            m_cfgCallback          = cfgCallback;
            m_notificationCallback = notificationCallback;

            m_procs              = procs;
            m_workers            = new Worker[procs];
            m_availableSemaphore = new System.Threading.Semaphore( 0, (int)procs );
            m_availableWorkers   = new Queue< Worker >();

            for(int idx = 0; idx < procs; idx++)
            {
                m_workers[idx] = new Worker( this );
            }
        }

        //
        // Helper Methods
        //

        public void Queue( MethodRepresentation md )
        {
            CHECKS.ASSERT( md != null, "Expecting method argument" );

            m_availableSemaphore.WaitOne();

            Worker worker;

            lock(m_availableWorkers)
            {
                worker = m_availableWorkers.Dequeue();
            }

            worker.Post( md );
        }

        public void Synchronize()
        {
            for(int idx = 0; idx < m_procs; idx++)
            {
                m_availableSemaphore.WaitOne();
            }

            m_availableSemaphore.Release( m_procs );
        }

        public void Shutdown()
        {
            for(int idx = 0; idx < m_procs; idx++)
            {
                m_availableSemaphore.WaitOne();

                Worker worker;

                lock(m_availableWorkers)
                {
                    worker = m_availableWorkers.Dequeue();
                }

                worker.Stop();
            }
        }

        //--//

        void IDisposable.Dispose()
        {
            Shutdown();
        }

        //--//

        public static void EnumerateMethods( TypeSystemForCodeTransformation typeSystem ,
                                             NotificationCallback            callback   )
        {
            using(ParallelTransformationsHandler handler = new ParallelTransformationsHandler( null, null, callback ))
            {
                typeSystem.EnumerateMethods( handler.EnumerationCallback );
            }
        }

        public static void EnumerateMethods( TypeSystemForCodeTransformation typeSystem ,
                                             MethodEnumerationCallback       callback   )
        {
            using(ParallelTransformationsHandler handler = new ParallelTransformationsHandler( callback ))
            {
                typeSystem.EnumerateMethods( handler.EnumerationCallback );
            }
        }

        public static void EnumerateFlowGraphs( TypeSystemForCodeTransformation     typeSystem ,
                                                ControlFlowGraphEnumerationCallback callback   )
        {
            using(ParallelTransformationsHandler handler = new ParallelTransformationsHandler( callback ))
            {
                typeSystem.EnumerateMethods( handler.EnumerationCallback );
            }
        }

        //--//

        private void EnumerationCallback( MethodRepresentation md )
        {
            this.Queue( md );
        }

        //
        // Access Methods
        //

        public static int MaximumNumberOfProcessorsToUse
        {
            get
            {
                return s_maximumNumberOfProcessorsToUse;
            }

            set
            {
                s_maximumNumberOfProcessorsToUse = value;
            }
        }
    }
}
