//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Threading;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public class Finalizer
    {
        internal class Tracker
        {
            [TS.GarbageCollectionExtension(typeof(Tracker))]
            class Handler : GarbageCollectionExtensionHandler
            {
                //
                // State
                //

                KernelList< Tracker > m_workList;
                Thread                m_worker;
                bool                  m_fNewItems;

                //
                // Helper Methods
                //

                public override void Initialize()
                {
                    m_workList = new KernelList< Tracker >();
                    m_worker   = new Thread( Worker );
                }

                public override void StartOfMarkPhase( GarbageCollectionManager gc )
                {
                }

                public override void Mark( GarbageCollectionManager gc     ,
                                           object                   target )
                {
                }

                public override void EndOfMarkPhase( GarbageCollectionManager gc )
                {
                }

                public override void StartOfSweepPhase( GarbageCollectionManager gc )
                {
                    var node = s_list.StartOfForwardWalk;
                    while(node.IsValidForForwardMove)
                    {
                        var nextNode = node.Next;

                        var ptr    = node.Target;
                        var target = ptr.m_target;

                        if(!gc.IsMarked( target ))
                        {
                            if(ptr.m_fFinalized)
                            {
                                //
                                // Already finalized, drop the tracker.
                                //
                                node.RemoveFromList();
                            }
                            else
                            {
                                //
                                // Keep target alive and move the tracker to the work list.
                                //
                                gc.ExtendMarking( target );

                                ptr.m_targetKeepAlive = target;
                                ptr.m_fFinalized      = true;

                                m_workList.InsertAtTail( node );
                                m_fNewItems = true;
                            }
                        }

                        node = nextNode;
                    }
                }

                public override void Sweep( GarbageCollectionManager gc     ,
                                            object                   target )
                {
                }

                public override void EndOfSweepPhase( GarbageCollectionManager gc )
                {
                }

                public override void RestartExecution()
                {
                    if(m_fNewItems)
                    {
                        m_fNewItems = false;

                        s_notifyStop .Reset();
                        s_notifyStart.Set  ();

                        if(m_worker.IsAlive == false)
                        {
                            m_worker.Start();
                        }
                    }
                }

                //--//

                void Worker()
                {
                    while(true)
                    {
                        s_notifyStart.WaitOne();

                        var node = m_workList.StartOfForwardWalk;
                        while(node.IsValidForForwardMove)
                        {
                            var nextNode = node.Next;

                            //
                            // Add the tracker back to the normal list, it will be reclaimed on the next GC cycle.
                            //
                            lock(s_list)
                            {
                                s_list.InsertAtTail( node );
                            }

                            var ptr = node.Target;
                            var obj = (ObjectImpl)ptr.m_targetKeepAlive;

                            ptr.m_targetKeepAlive = null;

                            obj.FinalizeImpl();

                            node = nextNode;
                        }

                        s_notifyStop.Set();
                    }
                }
            }

            //
            // State
            //

                                             readonly KernelNode< Tracker > m_node;

            [TS.SkipDuringGarbageCollection] readonly object                m_target;
                                                      object                m_targetKeepAlive;
                                                      bool                  m_fFinalized;

            //
            // Constructor Methods
            //

            internal Tracker( object obj )
            {
                m_node   = new KernelNode< Tracker >( this );

                m_target = obj;
            }

            //
            // Helper Methods
            //

            internal static void Allocate( object obj )
            {
                var tracker = new Tracker( obj );

                lock(s_list)
                {
                    s_list.InsertAtTail( tracker.m_node );
                }
            }

            internal static void SetFinalizationFlag( object obj ,
                                                      bool   val )
            {
                lock(s_list)
                {
                    for(var node = s_list.StartOfForwardWalk; node.IsValidForForwardMove; node = node.Next)
                    {
                        var ptr = node.Target;

                        if(ptr.m_target == obj)
                        {
                            ptr.m_fFinalized = val;
                            return;
                        }
                    }
                }
            }
        }

        //
        // State
        //

        static readonly KernelList< Tracker> s_list        = new KernelList< Tracker >();
        static readonly AutoResetEvent       s_notifyStart = new AutoResetEvent  ( false );
        static readonly ManualResetEvent     s_notifyStop  = new ManualResetEvent( true  );

        //
        // Helper Methods
        //

        [TS.WellKnownMethod( "Finalizer_Allocate" )]
        internal static void Allocate( object obj  )
        {
            Tracker.Allocate( obj );
        }

        internal static void WaitForPendingFinalizers()
        {
            s_notifyStop.WaitOne();
        }

        internal static void SuppressFinalize( object obj )
        {
            Tracker.SetFinalizationFlag( obj, true );
        }

        internal static void ReRegisterForFinalize( object obj )
        {
            Tracker.SetFinalizationFlag( obj, false );
        }
    }
}
