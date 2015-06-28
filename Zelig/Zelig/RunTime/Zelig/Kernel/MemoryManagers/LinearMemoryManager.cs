//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class LinearMemoryManager : MemoryManager
    {
        //
        // Helper Methods
        //

        public override void InitializeMemoryManager()
        {
            base.InitializeMemoryManager();

            Memory.Range[] ranges = Memory.Instance.AvailableMemory;

            foreach(Memory.Range rng in ranges)
            {
                if((rng.Usage      & MemoryUsage     .Heap     ) != 0 &&
                   (rng.Attributes & MemoryAttributes.Allocated) == 0  )
                {
                    AddLinearSection( rng.Start, rng.End, rng.Attributes );
                }
            }
        }

        //
        // Miguel: Added to allow base.base.InitializeMemoryManager() call
        // from LLVM manager
        //
        public void CallBaseInitializeMemoryManager( )
        {
            base.InitializeMemoryManager( );
        }

        public override void InitializationComplete()
        {
            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                DirtyFreeMemory();
            }
            else
            {
                ZeroFreeMemory();
            }
        }

        //--//

        public override unsafe UIntPtr Allocate( uint size )
        {
            //using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
            {
                MemorySegment* ptr = m_active;

                if(ptr != null)
                {
                    UIntPtr res = ptr->Allocate( size );

                    if(res != UIntPtr.Zero)
                    {
                        return res;
                    }
                }

                ptr = m_heapHead;
                while(ptr != null)
                {
                    UIntPtr res = ptr->Allocate( size );

                    if(res != UIntPtr.Zero)
                    {
                        m_active = ptr;

                        return res;
                    }

                    ptr = ptr->Next;
                }

                return UIntPtr.Zero;
            }
        }

        public override void Release( UIntPtr address ,
                                      uint    size    )
        {
        }

        public override bool RefersToMemory( UIntPtr address )
        {
            foreach(Memory.Range rng in Memory.Instance.AvailableMemory)
            {
                if(AddressMath.IsInRange( address, rng.Start, rng.End ))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
