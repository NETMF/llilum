//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [ImplicitInstance]
    [ForceDevirtualization]
    [TS.DisableAutomaticReferenceCounting]
    [TS.DisableReferenceCounting]
    public abstract unsafe class MemoryManager
    {
        public static class Configuration
        {
            public static bool TrashFreeMemory
            {
                [ConfigurationOption("MemoryManager__TrashFreeMemory")]
                get
                {
                    return true;
                }
            }
        }

        sealed class EmptyManager : MemoryManager
        {
            //
            // Helper Methods
            //

            public override void InitializeMemoryManager()
            {
            }

            //--//

            public override void ZeroFreeMemory()
            {
            }

            public override UIntPtr Allocate( uint size )
            {
                return new UIntPtr( 0 );
            }

            public override void Release(UIntPtr address)
            {
            }

            public override bool RefersToMemory( UIntPtr address )
            {
                return true;
            }
        }

        //
        // State
        //

        protected MemorySegment* m_heapHead;
        protected MemorySegment* m_heapTail;
        protected MemorySegment* m_active;

        //
        // Helper Methods
        //

        public virtual void InitializeMemoryManager()
        {
            m_heapHead = null;
            m_heapTail = null;
            m_active   = null;
        }

        public virtual void InitializationComplete()
        {
        }

        public virtual void ZeroFreeMemory()
        {
            MemorySegment* ptr = m_heapHead;

            while(ptr != null)
            {
                ptr->ZeroFreeMemory();

                ptr = ptr->Next;
            }
        }

        public virtual void DirtyFreeMemory()
        {
            MemorySegment* ptr = m_heapHead;

            while(ptr != null)
            {
                ptr->DirtyFreeMemory();

                ptr = ptr->Next;
            }
        }

        internal virtual void ConsistencyCheck()
        {
        }

        internal virtual bool IsObjectAlive( UIntPtr ptr )
        {
            throw new NotImplementedException( );
        }

        [TS.WellKnownMethod( "MemoryManager_Allocate" )]
        public abstract UIntPtr Allocate( uint size );

        [TS.WellKnownMethod("MemoryManager_Release")]
        public abstract void Release(UIntPtr address);

        public abstract bool RefersToMemory( UIntPtr address );

        [ExportedMethod]
        public static UIntPtr AllocateFromManagedHeap( uint size )
        {
            //
            // Force all heap allocations to be multiples of 4-bytes so that we guarantee 
            // 4-byte alignment for all allocations.
            //
            size = AddressMath.AlignToWordBoundary( size + ObjectHeader.HeaderSize );

            UIntPtr ptr = Instance.Allocate( size );

            if( ptr == UIntPtr.Zero )
            {
                GarbageCollectionManager.Instance.Collect();

                ptr = Instance.Allocate( size );

                if(ptr == UIntPtr.Zero)
                {
                    throw new OutOfMemoryException( );
                }
            }

            // MemoryManager.Allocate returns pointer with object header initialized as AllocatedRawBytes,
            // which is already what we want. So just shift the pointer by the size of object header
            // (since the callers are interop code that have no concept of object header) and we're done!
            return AddressMath.Increment( ptr, ObjectHeader.HeaderSize );

        }

        [ExportedMethod]
        public static void FreeFromManagedHeap( UIntPtr address )
        {
            // Since AllocateFromManagedHeap returns pointer that were offset by object header size,
            // we need to reverse it before handing it to Release()
            Instance.Release( AddressMath.Decrement( address, ObjectHeader.HeaderSize ) );
        }

        //--//

        protected void AddLinearSection( UIntPtr          beginning  ,
                                         UIntPtr          end        ,
                                         MemoryAttributes attributes )
        {
            uint size = AddressMath.RangeSize( beginning, end );

            if(size >= MemorySegment.MinimumSpaceRequired())
            {
                MemorySegment* seg = (MemorySegment*)beginning.ToPointer();

                seg->Next       = null;
                seg->Previous   = m_heapTail;
                seg->Beginning  = beginning;
                seg->End        = end;
                seg->Attributes = attributes;

                if(m_heapHead == null)
                {
                    m_heapHead = seg;
                }

                if(m_heapTail != null)
                {
                    m_heapTail->Next = seg;
                }

                m_heapTail = seg;

                seg->Initialize();
            }
        }

        //
        // Access Methods
        //

        public static extern MemoryManager Instance
        {
            [SingletonFactory(Fallback=typeof(EmptyManager))]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public static extern Synchronization.YieldLock Lock
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public MemorySegment* StartOfHeap
        {
            get
            {
                return m_heapHead;
            }
        }

        public uint AvailableMemory
        {
            get
            {
                uint total = 0;

                for(MemorySegment* heap = m_heapHead; heap != null; heap = heap->Next)
                {
                    total += heap->AvailableMemory;
                }

                return total;
            }
        }

        public uint AllocatedMemory
        {
            get
            {
                uint total = 0;

                for(MemorySegment* heap = m_heapHead; heap != null; heap = heap->Next)
                {
                    total += heap->AllocatedMemory;
                }

                return total;
            }
        }
    }
}
