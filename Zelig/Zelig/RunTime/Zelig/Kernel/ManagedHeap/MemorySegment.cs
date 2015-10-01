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
                    Log("Allocated %d bytes from free block 0x%x at 0x%x", (int)size, (int)ptr, (int)res.ToUInt32());

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

        public void Release(UIntPtr address)
        {
            UIntPtr newFreeBlockBaseAddress = address;
            uint newFreeBlockSize = ObjectHeader.CastAsObjectHeader(address).Size;

            Log("MemorySegment(0x%x).Release - address 0x%x, size: %d", (int)this.Beginning.ToUInt32(), (int)address.ToUInt32(), (int)newFreeBlockSize);

            // Find the position in the FreeBlock linked list where it should go
            Log("Locating insertion point...");
            MemoryFreeBlock* insertAfter = null;
            for (MemoryFreeBlock* current = this.FirstFreeBlock; current != null; current = current->Next)
            {
                UIntPtr startAddress = new UIntPtr(current);

                Log("   looking at FreeBlock 0x%x", (int)startAddress.ToUInt32());

                if (AddressMath.IsGreaterThan(address, startAddress))
                {
                    insertAfter = current;
                }
                else
                {
                    break;
                }
            }

            // We now have the place to insert the new free block!
            Log("InsertAfter FreeBlock 0x%x", (int)insertAfter);

            // But first, we have to see if we should merge with adjacent free blocks 
            // instead of creating a brand new one.

            bool mergeWithPrevious = false;
            bool mergeWithNext = false;

            if (insertAfter != null)
            {
                ObjectHeader previousBlockObjectHeader = insertAfter->ToObjectHeader();
                UIntPtr previousBlockEndAddress = previousBlockObjectHeader.GetNextObjectPointer();

                Log("Comparing with previous FreeBlock baseAddress 0x%x, end 0x%x",
                    (int)previousBlockObjectHeader.ToPointer().ToUInt32(), (int)previousBlockEndAddress.ToUInt32());

                if (previousBlockEndAddress == address)
                {
                    // We need to merge with the previous block
                    newFreeBlockBaseAddress = previousBlockObjectHeader.ToPointer();
                    newFreeBlockSize += previousBlockObjectHeader.Size;
                    mergeWithPrevious = true;

                    Log("Need to merge with previous block! New address 0x%x, size:%d",
                        (int)newFreeBlockBaseAddress.ToUInt32(), (int)newFreeBlockSize);
                }
            }

            MemoryFreeBlock* nextBlock = (insertAfter != null) ? insertAfter->Next : this.FirstFreeBlock;
            if (nextBlock != null)
            {
                UIntPtr nextBlockBaseAddress = nextBlock->ToObjectHeaderPointer();

                Log("Comparing with next FreeBlock 0x%x (base address 0x%x)", 
                    (int)nextBlock, (int)nextBlockBaseAddress.ToUInt32());

                if (AddressMath.Increment(newFreeBlockBaseAddress, newFreeBlockSize) == nextBlockBaseAddress)
                {
                    // We need to merge with the next block
                    newFreeBlockSize += ObjectHeader.CastAsObjectHeader(nextBlockBaseAddress).Size;
                    mergeWithNext = true;

                    Log("Need to merge with next block! New address 0x%x, size:%d",
                        (int)newFreeBlockBaseAddress.ToUInt32(), (int)newFreeBlockSize);
                }
            }

            if (!mergeWithPrevious)
            {
                // Unless previous block was a free block, look for gap before the object to be deleted
                uint* target = (uint*)newFreeBlockBaseAddress.ToPointer();
                uint* limit = (uint*)this.Beginning.ToPointer();

                Log("Checking for plug gap before starting from 0x%x...", (int)target);

                while ((target > limit) && (*(target - 1) == (uint)ObjectHeader.GarbageCollectorFlags.GapPlug))
                {
                    target--;

                    Log("    Found potential plug gap at 0x%x", (int)target);
                }

                // If we detect any gap plugs before the object we are freeing, we need to
                // walk the ObjectHeader chain to ensure that they are indeed gap plugs, rather 
                // than valid data that just happen to look like GarbageCollectorFlags.GapPlug 
                if (AddressMath.IsLessThan(new UIntPtr(target), newFreeBlockBaseAddress))
                {
                    UIntPtr objectPointer = (insertAfter != null) ? insertAfter->ToObjectHeader().GetNextObjectPointer() : this.FirstBlock;
                    UIntPtr gapStart = new UIntPtr(target);

                    Log("Verifying plug gap starting at object header address 0x%x", (int)objectPointer.ToUInt32());

                    while (AddressMath.IsLessThan(objectPointer, gapStart))
                    {
                        ObjectHeader oh = ObjectHeader.CastAsObjectHeader(objectPointer);
                        if (oh.GarbageCollectorStateWithoutMutableBits == ObjectHeader.GarbageCollectorFlags.GapPlug)
                        {
                            objectPointer = AddressMath.Increment(objectPointer, sizeof(uint));
                        }
                        else
                        {
                            objectPointer = oh.GetNextObjectPointer();
                        }

                        Log("    Next object header address 0x%x", (int)objectPointer.ToUInt32());
                    }

                    // At this point, oh >= gapStart

                    if (AddressMath.IsInRange(objectPointer, gapStart, newFreeBlockBaseAddress)) // Note, IsInRange instead of == since it's possible that some of the GapPlug markings are real data
                    {
                        uint gapSize = AddressMath.RangeSize(objectPointer, newFreeBlockBaseAddress);
                        newFreeBlockBaseAddress = objectPointer;
                        newFreeBlockSize += gapSize;

                        Log("    Plug gap of size %d found! New address 0x%x, size:%d",
                            (int)gapSize, (int)newFreeBlockBaseAddress.ToUInt32(), (int)newFreeBlockSize);
                    }
                }
            }

            if (!mergeWithNext)
            {
                // Unless next block was a free block, look for gap after the object to be deleted
                uint* target = (uint*)AddressMath.Increment(newFreeBlockBaseAddress, newFreeBlockSize).ToPointer();
                uint* limit = (uint*)this.End.ToPointer();

                Log("Checking for plug gap after starting from 0x%x...", (int)target);

                while ((target < limit) && (*target == (uint)ObjectHeader.GarbageCollectorFlags.GapPlug))
                {
                    newFreeBlockSize += sizeof(uint);

                    Log("    Found plug gap at 0x%x, new size: %d", (int)target, (int)newFreeBlockSize);

                    target++;
                }
            }

            // Once we get here, we have gather all the information and have the correct newFreeBlockAddress and 
            // newFreeBlockSize. It's now time to manipulate the free list.
            if (mergeWithPrevious || mergeWithNext)
            {
                // Note that in the case where mergeWithPrevious and mergeWithNext is both true
                // we will do both and return early.
                if (mergeWithNext)
                {
                    // Remove the next block from the linked list, so we can add a new (combined) one later
                    RemoveFreeBlock(nextBlock);

                    Log("Removing next block 0x%x", (int)nextBlock);
                }

                if (mergeWithPrevious)
                {
                    // Merging with previous block can be done easily by increasing the size of the previous block
                    ArrayImpl array = ArrayImpl.CastAsArray(insertAfter->Pack());
                    array.SetLength(newFreeBlockSize - MemoryFreeBlock.FixedSize());

                    Log("Merging with previous block, new array length: %d", array.Length);

                    return;
                }
            }

            if (newFreeBlockSize >= MemoryFreeBlock.MinimumSpaceRequired())
            {
                // If the block is big enough for a MemoryFreeBlock, build one and insert it into the free list
                MemoryFreeBlock* newFreeBlock = MemoryFreeBlock.InitializeFromRawMemory(newFreeBlockBaseAddress, newFreeBlockSize);
                InsertFreeBlock(newFreeBlock, insertAfter);

                Log("New free block: address 0x%x (0x%x), size %d", (int)newFreeBlock, (int)newFreeBlockBaseAddress.ToUInt32(), (int)newFreeBlockSize);
            }
            else
            {
                // Otherwise, insert gap plugs instead.
                ObjectHeader.CastAsObjectHeader(newFreeBlockBaseAddress).InsertPlug(newFreeBlockSize);

                Log("Not big enough for a new free block! Insert %d bytes of gap plugs starting at address 0x%x",
                    (int)newFreeBlockSize, (int)newFreeBlockBaseAddress.ToUInt32());
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

        private void InsertFreeBlock(MemoryFreeBlock* ptr, MemoryFreeBlock* insertAfter)
        {
            if (insertAfter == null)
            {
                // Insert at head
                ptr->Next = this.FirstFreeBlock;
                this.FirstFreeBlock = ptr;
            }
            else
            {
                ptr->Next = insertAfter->Next;
            }

            if (ptr->Next != null)
            {
                ptr->Next->Previous = ptr;
            }
            else
            {
                // The new block is at the end of the list
                this.LastFreeBlock = ptr;
            }

            ptr->Previous = insertAfter;
            if (ptr->Previous != null)
            {
                ptr->Previous->Next = ptr;
            }
        }

        [Inline]
        public static uint MinimumSpaceRequired()
        {
            int size;

            size  = System.Runtime.InteropServices.Marshal.SizeOf( typeof(MemorySegment) );
            size += (int)MemoryFreeBlock.MinimumSpaceRequired();

            return (uint)size;
        }
        
        internal void ConsistencyCheck()
        {
            Log("MemorySegment 0x%x -> 0x%x", (int)this.Beginning.ToUInt32(), (int)this.End.ToUInt32());
            Log("FirstFreeBlock: 0x%x / LastFreeBlock: 0x%x", (int)this.FirstFreeBlock, (int)this.LastFreeBlock);

            if (this.FirstFreeBlock == null)
            {
                BugCheck.Assert(this.LastFreeBlock == null, BugCheck.StopCode.HeapCorruptionDetected);
                BugCheck.Assert(this.AvailableMemory == 0, BugCheck.StopCode.HeapCorruptionDetected);
            }
            else
            {
                BugCheck.Assert(this.FirstFreeBlock->Previous == null, BugCheck.StopCode.HeapCorruptionDetected);
                BugCheck.Assert(this.LastFreeBlock != null, BugCheck.StopCode.HeapCorruptionDetected);
                BugCheck.Assert(this.LastFreeBlock->Next == null, BugCheck.StopCode.HeapCorruptionDetected);

                // A generous estimation of how many free blocks are possible given the size of this memory segment
                uint maxFreeBlocksPossible = (AddressMath.RangeSize(this.FirstBlock, this.End) / MemoryFreeBlock.MinimumSpaceRequired()) + 1;

                Log("Walking MemoryFreeBlock...");

                uint freeBlockCount = 0;
                for (MemoryFreeBlock* current = this.FirstFreeBlock; current != null; current = current->Next)
                {
                    Log("FreeBlock: 0x%x, prev:0x%x, next: 0x%x, size:%d", (int)current, (int)current->Previous, (int)current->Next, (int)current->Size());
                    BugCheck.Assert(AddressMath.IsInRange(current->ToObjectHeaderPointer(), this.Beginning, this.End), BugCheck.StopCode.HeapCorruptionDetected);

                    if (current->Next == null)
                    {
                        BugCheck.Assert(current == this.LastFreeBlock, BugCheck.StopCode.HeapCorruptionDetected);
                    }
                    else
                    {
                        BugCheck.Assert(current->Next->Previous == current, BugCheck.StopCode.HeapCorruptionDetected);
                        BugCheck.Assert(AddressMath.IsLessThan(current->ToObjectHeaderPointer(), current->Next->ToObjectHeaderPointer()), BugCheck.StopCode.HeapCorruptionDetected);
                    }

                    freeBlockCount++;
                    if (freeBlockCount == maxFreeBlocksPossible)
                    {
                        // We've looped too long, either the linked list is looping or something else is horribly wrong.
                        BugCheck.Raise(BugCheck.StopCode.HeapCorruptionDetected);
                        break;
                    }
                }
            }

            Log("Walking object pointers...");

            UIntPtr objectPointer = this.FirstBlock;
            MemoryFreeBlock* nextExpectedFreeBlock = this.FirstFreeBlock;
            bool wasFreeBlock = false;
            bool wasGapPlug = false;
            while (AddressMath.IsLessThan(objectPointer, this.End))
            {
                BugCheck.Assert(AddressMath.IsAlignedTo32bits(objectPointer), BugCheck.StopCode.HeapCorruptionDetected);

                ObjectHeader oh = ObjectHeader.CastAsObjectHeader(objectPointer);
                ObjectHeader.GarbageCollectorFlags gcFlags = oh.GarbageCollectorStateWithoutMutableBits;
                bool isFreeBlock = gcFlags == ObjectHeader.GarbageCollectorFlags.FreeBlock;
                bool isGapPlug = gcFlags == ObjectHeader.GarbageCollectorFlags.GapPlug;

                Log("oh:0x%x, gcFlags:%x(fb:%d, gp:%d), size %d",
                    (int)objectPointer.ToUInt32(), (int)gcFlags, isFreeBlock ? 1 : 0, isGapPlug ? 1 : 0, isGapPlug ? sizeof(uint) : (int)oh.Size);

                if (isFreeBlock)
                {
                    // Ensure the current free block is in the free block list
                    BugCheck.Assert(nextExpectedFreeBlock != null && nextExpectedFreeBlock->ToObjectHeader() == oh, BugCheck.StopCode.HeapCorruptionDetected);
                    nextExpectedFreeBlock = nextExpectedFreeBlock->Next;
                }

                if (wasFreeBlock || wasGapPlug)
                {
                    // Adjacent free blocks or free block next to gap plugs are not allowed
                    BugCheck.Assert(!isFreeBlock, BugCheck.StopCode.HeapCorruptionDetected);
                }

                if (wasFreeBlock)
                {
                    // A free block next to gap plugs is not allowed
                    BugCheck.Assert(!isGapPlug, BugCheck.StopCode.HeapCorruptionDetected);
                }

                // Move to next object pointer
                if (isGapPlug)
                {
                    objectPointer = AddressMath.Increment(objectPointer, sizeof(uint));
                }
                else
                {
                    objectPointer = oh.GetNextObjectPointer();
                }

                wasFreeBlock = isFreeBlock;
                wasGapPlug = isGapPlug;
            }

            // Make sure we are exactly at the end of the memory segment and 
            // we visited every free block that was in the free block list
            BugCheck.Assert(objectPointer == this.End, BugCheck.StopCode.HeapCorruptionDetected);
            BugCheck.Assert(nextExpectedFreeBlock == null, BugCheck.StopCode.HeapCorruptionDetected);

            Log("MemorySegment Done");
        }

        internal bool IsObjectAlive( UIntPtr ptr )
        {
            UIntPtr objectPointer = this.FirstBlock;
            while(AddressMath.IsInRange( ptr, objectPointer, this.End ))
            {
                ObjectHeader oh = ObjectHeader.CastAsObjectHeader(objectPointer);
                ObjectHeader.GarbageCollectorFlags gcFlags = oh.GarbageCollectorStateWithoutMutableBits;

                if(ptr == objectPointer)
                {
                    return ( gcFlags != ObjectHeader.GarbageCollectorFlags.FreeBlock ) &&
                           ( gcFlags != ObjectHeader.GarbageCollectorFlags.GapPlug );
                }

                // Move to next object pointer
                if(gcFlags == ObjectHeader.GarbageCollectorFlags.GapPlug)
                {
                    objectPointer = AddressMath.Increment( objectPointer, sizeof( uint ) );
                }
                else
                {
                    objectPointer = oh.GetNextObjectPointer( );
                }
            }

            return false;
        }

#if MEMORYSEGMENT_LOG
        private static void Log(string format)
        {
            BugCheck.Log(format);
        }

        private static void Log(string format, int p1)
        {
            BugCheck.Log(format, p1);
        }

        private static void Log(string format, int p1, int p2)
        {
            BugCheck.Log(format, p1, p2);
        }

        private static void Log(string format, int p1, int p2, int p3)
        {
            BugCheck.Log(format, p1, p2, p3);
        }

        private static void Log(string format, int p1, int p2, int p3, int p4)
        {
            BugCheck.Log(format, p1, p2, p3, p4);
        }

        private static void Log(string format, int p1, int p2, int p3, int p4, int p5)
        {
            BugCheck.Log(format, p1, p2, p3, p4, p5);
        }
#else
        private static void Log(string format)
        {
        }

        private static void Log(string format, int p1)
        {
        }

        private static void Log(string format, int p1, int p2)
        {
        }

        private static void Log(string format, int p1, int p2, int p3)
        {
        }

        private static void Log(string format, int p1, int p2, int p3, int p4)
        {
        }

        private static void Log(string format, int p1, int p2, int p3, int p4, int p5)
        {
        }
#endif

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
