//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public class SyncBlockTable
    {
        internal const int BlocksInACluster = 32;

        //
        // State
        //

        private  Synchronization.YieldLock m_lock;
        private  SyncBlock[][]             m_clusters;
        internal SyncBlock                 m_freeList;
        private  int                       m_uniqueHashCode;

        //
        // Helper Methods
        //

        public static int GetHashCode( object target )
        {
            ObjectHeader oh = ObjectHeader.Unpack( target );
            int          hashCode;

            switch(oh.ExtensionKind)
            {
                case ObjectHeader.ExtensionKinds.HashCode:
                    return oh.Payload;

                case ObjectHeader.ExtensionKinds.Empty:
                    using(var hnd = new SmartHandles.YieldLockHolder( Instance.Lock ))
                    {
                        if(oh.IsImmutable == false)
                        {
                            //
                            // Check again, under lock, in case we had a race condition.
                            //
                            if(oh.ExtensionKind == ObjectHeader.ExtensionKinds.Empty)
                            {
                                hashCode = Instance.m_uniqueHashCode++;

                                oh.UpdateExtension( ObjectHeader.ExtensionKinds.HashCode, hashCode );

                                return hashCode;
                            }
                        }
                    }
                    break;
            }

            //--//

            int idx = AssignSyncBlock( target );

            hashCode = Instance.GetHashCode( idx );

            GC.KeepAlive( target );

            return hashCode;
        }

        public static Synchronization.CriticalSection GetLock( object target )
        {
            BugCheck.Assert(null != target, BugCheck.StopCode.SyncBlockCorruption );

            int idx = AssignSyncBlock( target );

            Synchronization.CriticalSection res = Instance.GetLock( idx );

            GC.KeepAlive( target );

            return res;
        }

        //--//

        [Inline]
        private static int AssignSyncBlock( object obj )
        {
            BugCheck.Assert(null != obj, BugCheck.StopCode.SyncBlockCorruption);

            ObjectHeader oh = ObjectHeader.Unpack( obj );

            if(oh.ExtensionKind == ObjectHeader.ExtensionKinds.SyncBlock)
            {
                return oh.Payload;
            }

            return Instance.AssignSyncBlockSlow( obj );
        }

        private int AssignSyncBlockSlow( object obj )
        {
            ObjectHeader oh = ObjectHeader.Unpack( obj );

            using(new SmartHandles.YieldLockHolder( this.Lock ))
            {
                //
                // Check again, under lock, in case we had a race condition.
                //
                if(oh.ExtensionKind == ObjectHeader.ExtensionKinds.SyncBlock)
                {
                    return oh.Payload;
                }

                int idx = -1;

                if(oh.IsImmutable)
                {
                    if(m_clusters != null)
                    {
                        foreach(var blocks in m_clusters)
                        {
                            for(int pos = 0; pos < BlocksInACluster; pos++)
                            {
                                var sb = blocks[pos];

                                if(sb.AssociatedObject == obj)
                                {
                                    idx = sb.Index;
                                    break;
                                }
                            }

                            if(idx >= 0)
                            {
                                break;
                            }
                        }
                    }
                }

                if(idx < 0)
                {
                    while(true)
                    {
                        var sb = SyncBlock.ExtractFromFreeList();
                        if(sb != null)
                        {
                            sb.Prepare( obj, m_uniqueHashCode++ );

                            idx = sb.Index;
                            break;
                        }

                        ExpandClusters();
                    }
                }

                switch(oh.ExtensionKind)
                {
                    case ObjectHeader.ExtensionKinds.Empty:
                        //
                        // Hash code automatically assigned.
                        //
                        break;

                    case ObjectHeader.ExtensionKinds.HashCode:
                        //
                        // Copy hash code from header.
                        //
                        SetHashCode( idx, oh.Payload );
                        break;

                    case ObjectHeader.ExtensionKinds.SyncBlock:
                        BugCheck.Raise( BugCheck.StopCode.SyncBlockCorruption );
                        break;

                    default:
                        //
                        // Not implemented yet, so it has to be a corruption.
                        //
                        BugCheck.Raise( BugCheck.StopCode.SyncBlockCorruption );
                        break;
                }

                if(oh.IsImmutable == false)
                {
                    oh.UpdateExtension( ObjectHeader.ExtensionKinds.SyncBlock, idx );
                }

                return idx;
            }
        }

        //--//

        public int GetHashCode( int idx )
        {
            int clusterIndex = idx / BlocksInACluster;
            int blockIndex   = idx % BlocksInACluster;

            return m_clusters[clusterIndex][blockIndex].HashCode;
        }

        public void SetHashCode( int idx      ,
                                 int hashCode )
        {
            int clusterIndex = idx / BlocksInACluster;
            int blockIndex   = idx % BlocksInACluster;

            m_clusters[clusterIndex][blockIndex].HashCode = hashCode;
        }

        public Synchronization.CriticalSection GetLock( int idx )
        {
            int clusterIndex = idx / BlocksInACluster;
            int blockIndex   = idx % BlocksInACluster;

            return m_clusters[clusterIndex][blockIndex].Lock;
        }

        //--//

        private void ExpandClusters()
        {
            int clusterIndex = (m_clusters == null) ? 0 : m_clusters.Length;

            var blocks = new SyncBlock[BlocksInACluster];

            //
            // Link each block to the next one, except for the last one.
            //
            int index = clusterIndex * BlocksInACluster;

            for(int i = 0; i < BlocksInACluster; i++)
            {
                blocks[i] = new SyncBlock( index++ );
            }

            m_clusters = ArrayUtility.AppendToArray( m_clusters, blocks );
        }

        //
        // Access Methods
        //

        public static extern SyncBlockTable Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public Synchronization.YieldLock Lock
        {
            get
            {
                return TypeSystemManager.AtomicAllocator( ref m_lock );
            }
        }
    }
}
