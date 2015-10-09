//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal class SyncBlock
    {
        [TS.GarbageCollectionExtension(typeof(SyncBlock))]
        class Handler : GarbageCollectionExtensionHandler
        {
            [TS.SkipDuringGarbageCollection] SyncBlock m_head;

            public override void StartOfMarkPhase( GarbageCollectionManager gc )
            {
                m_head = null;
            }

            public override void Mark( GarbageCollectionManager gc     ,
                                       object                   target )
            {
                var obj = (SyncBlock)target;

                if(gc.IsMarked( obj.m_associatedObject ))
                {
                    return;
                }

                obj.m_next = m_head;

                m_head = obj;
            }

            public override void EndOfMarkPhase( GarbageCollectionManager gc )
            {
            }

            public override void StartOfSweepPhase( GarbageCollectionManager gc )
            {
                for(var ptr = m_head; ptr != null;)
                {
                    var ptrNext = ptr.m_next;

                    ptr.m_next = null;

                    if(!gc.IsMarked( ptr.m_associatedObject ))
                    {
                        ptr.AddToFreeList();
                    }

                    ptr = ptrNext;
                }
            }

            public override void Sweep( GarbageCollectionManager gc     ,
                                        object                   target )
            {
            }

            public override void EndOfSweepPhase( GarbageCollectionManager gc )
            {
            }
        }

        //
        // State
        //

        [TS.SkipDuringGarbageCollection]          object                          m_associatedObject;
                                                  SyncBlock                       m_next;
        /******************************/ readonly int                             m_index;
        /******************************/          int                             m_hashCode;
        /******************************/          Synchronization.CriticalSection m_lock;
        int m_counterUse;
        int m_counterFree;

        //
        // Helper Methods
        //

        internal SyncBlock( int index )
        {
            m_index = index;

            AddToFreeList();
        }

        internal void Prepare( object target   ,
                               int    hashCode )
        {
            m_associatedObject = target;
            m_hashCode         = hashCode;
        }

        internal void AddToFreeList()
        {
            var table = SyncBlockTable.Instance;

            m_associatedObject = null;
            m_lock             = null;

            m_counterFree++;

            while(true)
            {
                var first = table.m_freeList;

                m_next = first;

                if(System.Threading.Interlocked.CompareExchange( ref table.m_freeList, this, first ) == first)
                {
                    break;
                }
            }
        }

        internal static SyncBlock ExtractFromFreeList()
        {
            var table = SyncBlockTable.Instance;

            while(true)
            {
                var first = table.m_freeList;

                if(first == null)
                {
                    return null;
                }

                var next = first.m_next;

                if(System.Threading.Interlocked.CompareExchange( ref table.m_freeList, next, first ) == first)
                {
                    first.m_counterUse++;
                    first.m_next = null;
                    return first;
                }
            }
        }

        //
        // Access Methods
        //

        internal Synchronization.CriticalSection Lock
        {
            get
            {
                return TypeSystemManager.AtomicAllocator( ref m_lock );
            }
        }

        internal object AssociatedObject
        {
            get
            {
                return m_associatedObject;
            }
        }

        internal int HashCode
        {
            get
            {
                return m_hashCode;
            }

            set
            {
                m_hashCode = value;
            }
        }

        internal int Index
        {
            get
            {
                return m_index;
            }
        }
    }
}
