//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public unsafe struct MemorySegment
    {
        //
        // State
        //

        public MemorySegment*   Next;
        public MemorySegment*   Previous;

        public UIntPtr          Beginning;
        public UIntPtr          End;
        public MemoryAttributes Attributes;

        public MemoryFreeBlock* FirstFreeBlock;
        public MemoryFreeBlock* LastFreeBlock;

        //
        // Helper Methods
        //

        [TS.WellKnownMethod("DebugGC_MemorySegment_Initialize")]
        public void Initialize()
        {
            fixed(MemorySegment* seg = &this)
            {
                byte* firstBlockPtr =        (byte*)&seg[1];
                uint  size          = (uint)((byte*) seg->End.ToPointer() - firstBlockPtr);

                MemoryFreeBlock* firstBlock = MemoryFreeBlock.InitializeFromRawMemory( new UIntPtr( firstBlockPtr ), size );

                seg->FirstFreeBlock  = firstBlock;
                seg->LastFreeBlock   = firstBlock;

                firstBlock->Next     = null;
                firstBlock->Previous = null;
            }
        }

        public void ZeroFreeMemory()
        {
            for(MemoryFreeBlock* ptr = this.FirstFreeBlock; ptr != null; ptr = ptr->Next)
            {
                ptr->ZeroFreeMemory();
            }
        }

        public void DirtyFreeMemory()
        {
            for(MemoryFreeBlock* ptr = this.FirstFreeBlock; ptr != null; ptr = ptr->Next)
            {
                ptr->DirtyFreeMemory();
            }
        }

        public UIntPtr Allocate( uint size )
        {
            for(MemoryFreeBlock* ptr = this.FirstFreeBlock; ptr != null; ptr = ptr->Next)
            {
                UIntPtr res = ptr->Allocate( ref this, size );

                if(res != UIntPtr.Zero)
                {
                    return res;
                }
            }

            return UIntPtr.Zero;
        }

        [TS.WellKnownMethod("DebugGC_MemorySegment_LinkNewFreeBlock")]
        public MemoryFreeBlock* LinkNewFreeBlock( UIntPtr start ,
                                                  UIntPtr end   )
        {
            uint size = AddressMath.RangeSize( start, end );

            if(size >= MemoryFreeBlock.MinimumSpaceRequired())
            {
                MemoryFreeBlock* ptr = MemoryFreeBlock.InitializeFromRawMemory( start, size );

                if(MemoryManager.Configuration.TrashFreeMemory)
                {
                    ptr->DirtyFreeMemory();
                }
                else
                {
                    ptr->ZeroFreeMemory();
                }

                if(this.FirstFreeBlock == null)
                {
                    this.FirstFreeBlock = ptr;
                }

                MemoryFreeBlock* ptrLast = this.LastFreeBlock;
                if(ptrLast != null)
                {
                    ptrLast->Next = ptr;
                }

                this.LastFreeBlock = ptr;
                ptr->Previous      = ptrLast;
                ptr->Next          = null;

                return ptr;
            }
            else
            {
                ObjectHeader oh = ObjectHeader.CastAsObjectHeader( start );

                oh.InsertPlug( size );

                return null;
            }
        }

        [TS.WellKnownMethod("DebugGC_MemorySegment_RemoveFreeBlock")]
        public void RemoveFreeBlock( MemoryFreeBlock* ptr )
        {
            MemoryFreeBlock* ptrNext = ptr->Next;
            MemoryFreeBlock* ptrPrev = ptr->Previous;

            if(ptrNext != null               ) ptrNext->Previous   = ptrPrev;
            if(ptrPrev != null               ) ptrPrev->Next       = ptrNext;
            if(ptr     == this.FirstFreeBlock) this.FirstFreeBlock = ptrNext;
            if(ptr     == this.LastFreeBlock ) this.LastFreeBlock  = ptrPrev;
        }


        [Inline]
        public static uint MinimumSpaceRequired()
        {
            int size;

            size  = System.Runtime.InteropServices.Marshal.SizeOf( typeof(MemorySegment) );
            size += (int)MemoryFreeBlock.MinimumSpaceRequired();

            return (uint)size;
        }

        //
        // Access Methods
        //

        public UIntPtr FirstBlock
        {
            get
            {
                fixed(MemorySegment* seg = &this)
                {
                    return new UIntPtr( &seg[1] );
                }
            }
        }

        public uint AvailableMemory
        {
            get
            {
                uint total = 0;

                for(MemoryFreeBlock* ptr = this.FirstFreeBlock; ptr != null; ptr = ptr->Next)
                {
                    total += ptr->AvailableMemory;
                }

                return total;
            }
        }

        public uint AllocatedMemory
        {
            get
            {
                return AddressMath.RangeSize( this.FirstBlock, this.End ) - this.AvailableMemory;
            }
        }
    }
}
