//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define GC_PRECISE_PROFILING


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class MarkAndSweepCollector : GarbageCollectionManager
    {
        class StackWalker : TS.CodeMapDecoderCallback
        {
            //
            // State
            //

            const int c_StackMarkSize = 8 * sizeof(uint);

            MarkAndSweepCollector m_owner;
            UIntPtr               m_pc;

            uint                  m_registerMask_Scratched;
            uint                  m_registerMask_Heap;
            uint                  m_registerMask_Internal;
            uint                  m_registerMask_Potential;
            uint                  m_stackMask_Heap;
            uint                  m_stackMask_Internal;
            uint                  m_stackMask_Potential;
            uint                  m_stackLow;
            uint                  m_stackHigh;
            bool                  m_done;

            //
            // Constructor Methods
            //

            internal StackWalker( MarkAndSweepCollector owner )
            {
                m_owner = owner;
            }

            //
            // Helper Methods
            //

            public override bool RegisterEnter( UIntPtr     address ,
                                                uint        num     ,
                                                PointerKind kind    )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                uint mask = 1u << (int)num;

                switch(kind)
                {
                    case PointerKind.Heap     : m_registerMask_Heap      |= mask; break;
                    case PointerKind.Internal : m_registerMask_Internal  |= mask; break;
                    case PointerKind.Potential: m_registerMask_Potential |= mask; break;
                }

                return true;
            }

            public override bool RegisterLeave( UIntPtr address ,
                                                uint    num     )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                uint mask = ~(1u << (int)num);

                m_registerMask_Heap      &= mask;
                m_registerMask_Internal  &= mask;
                m_registerMask_Potential &= mask;

                return true;
            }

            public override bool StackEnter( UIntPtr     address ,
                                             uint        offset  ,
                                             PointerKind kind    )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                if(m_stackLow <= offset && offset < m_stackHigh)
                {
                    uint mask = (1u << (int)(offset - m_stackLow));

                    switch(kind)
                    {
                        case PointerKind.Heap     : m_stackMask_Heap      |= mask; break;
                        case PointerKind.Internal : m_stackMask_Internal  |= mask; break;
                        case PointerKind.Potential: m_stackMask_Potential |= mask; break;
                    }
                }

                if(offset >= m_stackHigh)
                {
                    m_done = false;
                }

                return true;
            }

            public override bool StackLeave( UIntPtr address ,
                                             uint    offset  )
            {
                if(AddressMath.IsGreaterThan( address, m_pc ))
                {
                    return false;
                }

                if(m_stackLow <= offset && offset < m_stackHigh)
                {
                    uint mask = ~(1u << (int)(offset - m_stackLow));

                    m_stackMask_Heap      &= mask;
                    m_stackMask_Internal  &= mask;
                    m_stackMask_Potential &= mask;
                }

                if(offset >= m_stackHigh)
                {
                    m_done = false;
                }

                return true;
            }

            //--//

            internal unsafe void Process( Processor.Context ctx )
            {
                //
                // All registers should be considered.
                //
                m_registerMask_Scratched = 0;

                while(true)
                {
                    m_pc        = ctx.ProgramCounter;
                    m_stackLow  = 0;
                    m_stackHigh = c_StackMarkSize;

                    //--//

                    TS.CodeMap cm = TS.CodeMap.ResolveAddressToCodeMap( m_pc );

                    BugCheck.Assert( cm != null, BugCheck.StopCode.UnwindFailure );

                    int idx = cm.FindRange( m_pc );

                    BugCheck.Assert( idx >= 0, BugCheck.StopCode.UnwindFailure );

                    while(true)
                    {
                        m_registerMask_Heap      = 0;
                        m_registerMask_Internal  = 0;
                        m_registerMask_Potential = 0;
                        m_stackMask_Heap         = 0;
                        m_stackMask_Internal     = 0;
                        m_stackMask_Potential    = 0;
                        m_done                   = true;

                        cm.Ranges[idx].Decode( this );

                        //
                        // On the first pass, mark the objects pointed to by live registers.
                        //
                        if(m_stackLow == 0)
                        {
                            uint set = m_registerMask_Heap | m_registerMask_Internal | m_registerMask_Potential;

                            set &= ~m_registerMask_Scratched;

                            if(set != 0)
                            {
                                uint mask = 1u;

                                for(int regNum = 0; regNum < 16; regNum++, mask <<= 1)
                                {
                                    if((set & mask) != 0)
                                    {
                                        UIntPtr ptr = ctx.GetRegisterByIndex( (uint)regNum );

                                        if(ptr != UIntPtr.Zero)
                                        {
                                            if((m_registerMask_Heap & mask) != 0)
                                            {
                                                m_owner.VisitHeapObject( ptr );
                                            }
                                            else
                                            {
                                                m_owner.VisitInternalPointer( ptr );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            uint set = m_stackMask_Heap | m_stackMask_Internal | m_stackMask_Potential;

                            if(set != 0)
                            {
                                uint mask = 1u;

                                for(int offset = 0; offset < c_StackMarkSize; offset++, mask <<= 1)
                                {
                                    if((set & mask) != 0)
                                    {
                                        UIntPtr* stack = (UIntPtr*)ctx.StackPointer.ToPointer();
                                        UIntPtr  ptr   = stack[m_stackLow + offset];

                                        if(ptr != UIntPtr.Zero)
                                        {
                                            if((m_stackMask_Heap & mask) != 0)
                                            {
                                                m_owner.VisitHeapObject( ptr );
                                            }
                                            else
                                            {
                                                m_owner.VisitInternalPointer( ptr );
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(m_done)
                        {
                            break;
                        }

                        m_stackLow  += c_StackMarkSize;
                        m_stackHigh += c_StackMarkSize;
                    }

                    //
                    // Exclude scratched registers from the set of live registers.
                    //
                    m_registerMask_Scratched = ctx.ScratchedIntegerRegisters;

                    if(ctx.Unwind() == false)
                    {
                        break;
                    }
                }
            }

        }

        //--//

        struct MarkStackForArrays
        {
            //
            // State
            //

            private UIntPtr   m_address;
            private uint      m_elementSize;
            private int       m_numOfElements;
            private TS.VTable m_vTableElement;

            //
            // Helper Methods
            //

#if !GC_PRECISE_PROFILING
            [Inline]
#endif
            internal unsafe void Push( ArrayImpl array         ,
                                       uint      elementSize   ,
                                       int       numOfElements ,
                                       TS.VTable vTableElement )
            {
                m_address       = new UIntPtr( array.GetDataPointer() );
                m_elementSize   = elementSize;
                m_numOfElements = numOfElements;
                m_vTableElement = vTableElement;
            }

#if !GC_PRECISE_PROFILING
            [Inline]
#endif
            internal unsafe void Visit( MarkAndSweepCollector owner )
            {
                UIntPtr   address = m_address;
                TS.VTable vTable  = m_vTableElement;

                if(--m_numOfElements == 0)
                {
                    //
                    // Pop entry.
                    //
                    m_address       = new UIntPtr();
                    m_vTableElement = null;

                    owner.m_markStackForArrays_Pos--;
                }
                else
                {
                    //
                    // Move to next element.
                    //
                    m_address = AddressMath.Increment( m_address, m_elementSize );
                }

                if(vTable != null)
                {
                    owner.VisitHeapObjectFields( address, vTable );
                }
                else
                {
                    UIntPtr* ptr = (UIntPtr*)address.ToPointer();
                    UIntPtr  obj = ptr[0];

                    if(obj != UIntPtr.Zero)
                    {
                        owner.VisitHeapObject( obj );
                    }
                }
            }
        }

        //
        // State
        //

        const int c_MarkStackForObjectsSize = 1024;
        const int c_MarkStackForArraysSize  = 128;

        //--//

        private Synchronization.YieldLock          m_lock;
        private OutOfMemoryException               m_outOfMemoryException;
        private StackWalker                        m_stackWalker;

        private bool                               m_fFirstLevel;

        private UIntPtr[]                          m_maskStackForObjects;
        private int                                m_maskStackForObjects_Pos;

        private MarkStackForArrays[]               m_markStackForArrays;
        private int                                m_markStackForArrays_Pos;

        private ObjectHeader.GarbageCollectorFlags m_markForNonHeap;
        private uint[]                             m_trackFreeBlocks;

        private int                                m_perf_gapCount;
        private int                                m_perf_freeCount;
        private int                                m_perf_deadCount;
        private int                                m_perf_objectCount;

        private int                                m_perf_stat_calls;
        private uint                               m_perf_stat_freeMem;
        private long                               m_perf_time_baseline;
        private long                               m_perf_time_start;
        private long                               m_perf_time_walk;
        private long                               m_perf_time_global;
        private long                               m_perf_time_sweep;
        private long                               m_perf_time_ret;

        //
        // Helper Methods
        //

        public unsafe override void InitializeGarbageCollectionManager()
        {
            m_lock                 = new Synchronization.YieldLock();
            m_outOfMemoryException = new OutOfMemoryException();
            m_stackWalker          = new StackWalker( this );
            m_maskStackForObjects  = new UIntPtr           [c_MarkStackForObjectsSize];
            m_markStackForArrays   = new MarkStackForArrays[c_MarkStackForArraysSize ];

            if(Configuration.TraceFreeBlocks)
            {
                m_trackFreeBlocks = new uint[32];
            }

            //--//

            foreach(var handler in this.ExtensionHandlers)
            {
                handler.Initialize();
            }

            //--//

            MemorySegment* heapLow  = null;
            MemorySegment* heapHigh = null;

            for(MemorySegment* heap = MemoryManager.Instance.StartOfHeap; heap != null; heap = heap->Next)
            {
                if(heapLow == null || AddressMath.IsGreaterThan( heapLow->Beginning, heap->Beginning ))
                {
                    heapLow = heap;
                }

                if(heapHigh == null || AddressMath.IsLessThan( heapHigh->End, heap->End ))
                {
                    heapHigh = heap;
                }
            }

            BugCheck.Assert( heapLow != null && heapHigh != null, BugCheck.StopCode.NoMemory );

            BrickTable.Instance.Initialize( heapLow->Beginning.ToUInt32(), heapHigh->End.ToUInt32() );

            RebuildBrickTable();

            if(Configuration.ValidateHeap)
            {
                VerifyBrickTable();
            }
        }

        [Inline]
        public override void NotifyNewObject( UIntPtr ptr  ,
                                              uint    size )
        {
            BrickTable.Instance.MarkObject( ptr, size );
        }

        public override ObjectImpl FindObject( UIntPtr interiorPtr )
        {
            UIntPtr address = BrickTable.Instance.FindLowerBoundForObjectPointer( interiorPtr );

            if(address != UIntPtr.Zero)
            {
                while(true)
                {
                    ObjectHeader                       oh    = ObjectHeader.CastAsObjectHeader( address );
                    ObjectHeader.GarbageCollectorFlags flags = oh.GarbageCollectorState;

                    switch(flags)
                    {
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr nextAddress = oh.GetNextObjectPointer();

                                if(AddressMath.IsLessThan( interiorPtr, nextAddress ))
                                {
                                    //
                                    // Don't return the address of a heap free block.
                                    //
                                    return null;
                                }

                                address = nextAddress;
                            }
                            break;

                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Marked  :
                            address = AddressMath.Increment( address, sizeof(uint) );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Marked:
                            address = AddressMath.Increment( address, oh.AllocatedRawBytesSize );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return null;

                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return null;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr nextAddress = oh.GetNextObjectPointer();

                                if(AddressMath.IsLessThan( interiorPtr, nextAddress ))
                                {
                                    return oh.Pack();
                                }

                                address = nextAddress;
                            }
                            break;

                        default:
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return null;
                    }
                }
            }

            return null;
        }

        public override uint Collect()
        {
            if(Configuration.CollectPerformanceStatistics)
            {
                m_perf_stat_calls++;
                m_perf_time_baseline = System.Diagnostics.Stopwatch.GetTimestamp();
            }

            uint mem;
            long gcTime;

            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( m_lock ))
            {
                while(true)
                {
                    using(SmartHandles.InterruptState hnd2 = SmartHandles.InterruptState.Disable())
                    {
                        if(IsThisAGoodPlaceToStopTheWorld())
                        {
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_time_start = System.Diagnostics.Stopwatch.GetTimestamp() - m_perf_time_baseline;
                            }

                            long gc_start = 0;
                            long gc_stop  = 0;

                            if(Configuration.CollectMinimalPerformanceStatistics)
                            {
                                gc_start = System.Diagnostics.Stopwatch.GetTimestamp();
                            }

                            StartCollection();

                            if(Configuration.CollectMinimalPerformanceStatistics)
                            {
                                gc_stop = System.Diagnostics.Stopwatch.GetTimestamp();
                            }

                            gcTime = gc_stop - gc_start;

                            mem = MemoryManager.Instance.AvailableMemory;

                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_time_ret     = System.Diagnostics.Stopwatch.GetTimestamp() - m_perf_time_baseline;
                                m_perf_stat_freeMem = mem;
                            }

                            break;
                        }
                    }

                    ThreadImpl.CurrentThread.Yield();
                }
            }

            foreach(var handler in this.ExtensionHandlers)
            {
                handler.RestartExecution();
            }

            DumpFreeBlockTracking();

            if(Configuration.CollectMinimalPerformanceStatistics)
            {
                BugCheck.WriteLineFormat( "GC: Free mem: {0} Time: {1}msec", mem, ToMilliseconds( gcTime ) );
            }

            if(Configuration.CollectPerformanceStatistics)
            {
                BugCheck.WriteLineFormat( "m_perf_gapCount   : {0,9}", m_perf_gapCount    );
                BugCheck.WriteLineFormat( "m_perf_freeCount  : {0,9}", m_perf_freeCount   );
                BugCheck.WriteLineFormat( "m_perf_deadCount  : {0,9}", m_perf_deadCount   );
                BugCheck.WriteLineFormat( "m_perf_objectCount: {0,9}", m_perf_objectCount );

                BugCheck.WriteLineFormat( "m_perf_time_start : {0}msec", ToMilliseconds( m_perf_time_start  ) );
                BugCheck.WriteLineFormat( "m_perf_time_walk  : {0}msec", ToMilliseconds( m_perf_time_walk   ) );
                BugCheck.WriteLineFormat( "m_perf_time_global: {0}msec", ToMilliseconds( m_perf_time_global ) );
                BugCheck.WriteLineFormat( "m_perf_time_sweep : {0}msec", ToMilliseconds( m_perf_time_sweep  ) );
                BugCheck.WriteLineFormat( "m_perf_time_ret   : {0}msec", ToMilliseconds( m_perf_time_ret    ) );
            }

            return mem;
        }

        private static int ToMilliseconds( long ticks )
        {
            return (int)(1000.0 * ticks / System.Diagnostics.Stopwatch.Frequency);
        }

        public override long GetTotalMemory()
        {
            return MemoryManager.Instance.AllocatedMemory;
        }

        public override void ThrowOutOfMemory( TS.VTable vTable )
        {
            throw m_outOfMemoryException;
        }

        public override bool IsMarked( object obj )
        {
            if(obj == null)
            {
                return true;
            }

            ObjectHeader                       oh    = ObjectHeader.Unpack( obj );
            ObjectHeader.GarbageCollectorFlags flags = oh.GarbageCollectorState;

            switch(flags)
            {
                case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return true;

                case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                    return false;

                case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return true;

                default:
                    BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                    return false;
            }
        }

        public override void ExtendMarking( object obj )
        {
            var objImpl = (ObjectImpl)obj;
            if(objImpl != null)
            {
                VisitHeapObject( objImpl.ToPointer() );
            }
        }

        //--//

        private bool IsThisAGoodPlaceToStopTheWorld()
        {
            ThreadImpl        thisThread = ThreadImpl.CurrentThread;
            Processor.Context ctx        = thisThread.ThrowContext; // Reuse the throw context for the current thread to unwind the stack.

            //
            // TODO: LT72: Only the RT.Threadmanager can implement this method correctly at this time
            //
            ThreadManager     tm         = ThreadManager.Instance;

            for(KernelNode< ThreadImpl > node = tm.StartOfForwardWalkThroughAllThreads; node.IsValidForForwardMove; node = node.Next)
            {
                ThreadImpl thread = node.Target;

                if(thread == thisThread)
                {
                    continue;
                }

                if(thread.IsAtSafePoint( ctx ) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void StartCollection()
        {
            m_fFirstLevel              = true;
            m_maskStackForObjects_Pos  = -1;
            m_markStackForArrays_Pos   = -1;
            m_markForNonHeap          ^= ObjectHeader.GarbageCollectorFlags.Marked;

            foreach(var handler in this.ExtensionHandlers)
            {
                handler.StartOfMarkPhase( this );
            }

            WalkStackFrames();

            if(Configuration.CollectPerformanceStatistics)
            {
                m_perf_time_walk = System.Diagnostics.Stopwatch.GetTimestamp() - m_perf_time_baseline;
            }

            MarkGlobalRoot();

            ProcessMarkStack();

            foreach(var handler in this.ExtensionHandlers)
            {
                handler.EndOfMarkPhase( this );
            }

            if(Configuration.CollectPerformanceStatistics)
            {
                m_perf_time_global = System.Diagnostics.Stopwatch.GetTimestamp() - m_perf_time_baseline;
            }

            foreach(var handler in this.ExtensionHandlers)
            {
                handler.StartOfSweepPhase( this );
            }

            Sweep();

            foreach(var handler in this.ExtensionHandlers)
            {
                handler.EndOfSweepPhase( this );
            }

            if(Configuration.CollectPerformanceStatistics)
            {
                m_perf_time_sweep = System.Diagnostics.Stopwatch.GetTimestamp() - m_perf_time_baseline;
            }

            if(Configuration.ValidateHeap)
            {
                VerifyBrickTable();
            }
        }

        private void WalkStackFrames()
        {
            ThreadImpl        thisThread = ThreadImpl.CurrentThread;
            Processor.Context ctx        = thisThread.ThrowContext; // Reuse the throw context for the current thread to unwind the stack.
            
            //
            // TODO: LT72: Only the RT.Threadmanager can implement this method correctly at this time
            //
            ThreadManager  tm = ThreadManager.Instance;

            for(KernelNode< ThreadImpl > node = tm.StartOfForwardWalkThroughAllThreads; node.IsValidForForwardMove; node = node.Next)
            {
                ThreadImpl thread = node.Target;

                if(thread == thisThread)
                {
                    ctx.Populate();
                }
                else
                {
                    ctx.Populate( thread.SwappedOutContext );
                }

                m_stackWalker.Process( ctx );
            }
        }

        private void MarkGlobalRoot()
        {
            object  root    = TS.GlobalRoot.Instance;
            UIntPtr address = ((ObjectImpl)root).ToPointer();

            VisitHeapObject( address );
        }

        private unsafe void Sweep()
        {
            ResetFreeBlockTracking();

            var brickTable = BrickTable.Instance;
            
            brickTable.Reset();

            if(Configuration.CollectPerformanceStatistics)
            {
                m_perf_gapCount    = 0;
                m_perf_freeCount   = 0;
                m_perf_deadCount   = 0;
                m_perf_objectCount = 0;
            }

            for(MemorySegment* heap = MemoryManager.Instance.StartOfHeap; heap != null; heap = heap->Next)
            {
                UIntPtr address      = heap->FirstBlock;
                UIntPtr end          = heap->End;
                bool    fLastWasFree = false;
                UIntPtr freeStart    = UIntPtr.Zero;

                heap->FirstFreeBlock = null;
                heap->LastFreeBlock  = null;

                while(AddressMath.IsLessThan( address, end ))
                {
                    ObjectHeader                       oh    = ObjectHeader.CastAsObjectHeader( address );
                    ObjectHeader.GarbageCollectorFlags flags = oh.GarbageCollectorState;
                    bool                               fFree = true;
                    UIntPtr                            addressNext;

                    switch(flags)
                    {
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Marked  :
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_freeCount++;
                            }

                            addressNext = oh.GetNextObjectPointer();
                            break;

                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Marked  :
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_gapCount++;
                            }

                            addressNext = AddressMath.Increment( address, sizeof(uint) );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Marked:
                            addressNext = AddressMath.Increment( address, oh.AllocatedRawBytesSize );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_deadCount++;
                            }

                            addressNext = oh.GetNextObjectPointer();
                            break;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_objectCount++;
                            }

                            addressNext = oh.GetNextObjectPointer();

                            oh.GarbageCollectorState = (flags & ~ObjectHeader.GarbageCollectorFlags.Marked);

                            brickTable.MarkObject( address, AddressMath.RangeSize( address, addressNext ) );

                            fFree = false;
                            break;

                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_deadCount++;
                            }

                            addressNext = oh.GetNextObjectPointer();
                            break;

                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                if(Configuration.CollectPerformanceStatistics)
                                {
                                    m_perf_objectCount++;
                                }

                                addressNext = oh.GetNextObjectPointer();

                                var ext = FindExtensionHandler( oh.VirtualTable );
                                if(ext == null)
                                {
                                    BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                                }
                                else
                                {
                                    ext.Sweep( this, oh.Pack() );
                                }

                                oh.GarbageCollectorState = (flags & ~ObjectHeader.GarbageCollectorFlags.Marked);

                                brickTable.MarkObject( address, AddressMath.RangeSize( address, addressNext ) );

                                fFree = false;
                                break;
                            }

                        default:
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;
                    }

                    if(fLastWasFree != fFree)
                    {
                        if(fFree)
                        {
                            freeStart = address;
                        }
                        else
                        {
                            TrackFreeBlock( freeStart, address );

                            heap->LinkNewFreeBlock( freeStart, address );
                        }

                        fLastWasFree = fFree;
                    }

                    address = addressNext;
                }

                if(fLastWasFree)
                {
                    TrackFreeBlock( freeStart, end );

                    heap->LinkNewFreeBlock( freeStart, end );
                }
            }
        }

        //--//

        private void ResetFreeBlockTracking()
        {
            if(Configuration.TraceFreeBlocks)
            {
                Array.Clear( m_trackFreeBlocks, 0, m_trackFreeBlocks.Length );
            }
        }

        private void TrackFreeBlock( UIntPtr start ,
                                     UIntPtr end   )
        {
            if(Configuration.TraceFreeBlocks)
            {
                uint size = AddressMath.RangeSize( start, end );

                for(int i = 0; i < 32; i++)
                {
                    if(size < (1u << i))
                    {
                        m_trackFreeBlocks[i]++;
                        break;
                    }
                }
            }
        }

        private void DumpFreeBlockTracking()
        {
            if(Configuration.TraceFreeBlocks)
            {
                for(int i = 0; i < 32; i++)
                {
                    if(m_trackFreeBlocks[i] != 0)
                    {
                        BugCheck.WriteLineFormat( "Size: {0,9} = {1}", 1u << i, m_trackFreeBlocks[i] );
                    }
                }
            }
        }

        //--//

        [NoInline]
        private void VisitInternalPointer( UIntPtr address )
        {
            VisitInternalPointerInline( address );
        }

        [NoInline]
        private void VisitHeapObject( UIntPtr address )
        {
            VisitHeapObjectInline( address );
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void VisitInternalPointerInline( UIntPtr address )
        {
            ObjectImpl obj = FindObject( address );
            if(obj != null)
            {
                VisitHeapObject( obj.ToPointer() );
            }
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void VisitHeapObjectInline( UIntPtr address )
        {
            if(Configuration.ValidateHeap)
            {
                BugCheck.Assert( MemoryManager.Instance.RefersToMemory( address ), BugCheck.StopCode.NotAMemoryReference );
            }

            ObjectHeader                       oh    = ObjectHeader.Unpack( ObjectImpl.FromPointer( address ) );
            ObjectHeader.GarbageCollectorFlags flags = oh.GarbageCollectorState;

            switch(flags)
            {
                case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return;

                case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return;

                case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return;

                case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Unmarked:
                case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Marked:
                    return;

                case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                    if(m_markForNonHeap == ObjectHeader.GarbageCollectorFlags.Unmarked)
                    {
                        return;
                    }

                    flags ^= ObjectHeader.GarbageCollectorFlags.Marked;
                    break;

                case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                    if(m_markForNonHeap == ObjectHeader.GarbageCollectorFlags.Marked)
                    {
                        return;
                    }

                    flags ^= ObjectHeader.GarbageCollectorFlags.Marked;
                    break;

                case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                    flags |= ObjectHeader.GarbageCollectorFlags.Marked;
                    break;

                case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return;

                case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                    {

                        var ext = FindExtensionHandler( oh.VirtualTable );
                        if(ext == null)
                        {
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                        }
                        else
                        {
                            ext.Mark( this, oh.Pack() );
                        }

                        flags |= ObjectHeader.GarbageCollectorFlags.Marked;
                        break;
                    }
                
                case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked  :
                    return;

                default:
                    BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                    return;
            }

            oh.GarbageCollectorState = flags;

            TS.VTable vTable = oh.VirtualTable;

            if(vTable.IsArray)
            {
                PushArrayReference( address, vTable );
            }
            else
            {
                PushObjectReference( address, vTable );
            }
        }

        [NoInline]
        private void VisitHeapObjectFields( UIntPtr   address ,
                                            TS.VTable vTable  )
        {
            VisitHeapObjectFieldsInline( address, vTable );
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void VisitHeapObjectFieldsInline( UIntPtr   address ,
                                                  TS.VTable vTable  )
        {
            TS.GCInfo.Pointer[] pointers      = vTable.GCInfo.Pointers;
            int                 numOfPointers = pointers.Length;

            for(int i = 0; i < numOfPointers; i++)
            {
                VisitHeapObjectField( address, ref pointers[i] );
            }
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private unsafe void VisitHeapObjectField(     UIntPtr           address ,
                                                  ref TS.GCInfo.Pointer pointer )
        {
            UIntPtr* fieldAddress     = (UIntPtr*)address.ToPointer();
            UIntPtr  referenceAddress = fieldAddress[pointer.OffsetInWords];

            if(referenceAddress != UIntPtr.Zero)
            {
                switch(pointer.Kind)
                {
                    case TS.GCInfo.Kind.Heap:
                        VisitHeapObjectInline( referenceAddress );
                        break;

                    case TS.GCInfo.Kind.Internal:
                    case TS.GCInfo.Kind.Potential:
                        VisitInternalPointerInline( referenceAddress );
                        break;
                }
            }
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void PushObjectReference( UIntPtr   address ,
                                          TS.VTable vTable  )
        {
            TS.GCInfo.Pointer[] pointers = vTable.GCInfo.Pointers;

            if(pointers == null)
            {
                //
                // No pointers, nothing to do.
                //
                return;
            }

            if(m_fFirstLevel)
            {
                m_fFirstLevel = false;

                int numOfPointers = pointers.Length;

                for(int i = 0; i < numOfPointers; i++)
                {
                    VisitHeapObjectField( address, ref pointers[i] );
                }

                ProcessMarkStack();

                m_fFirstLevel = true;
            }
            else
            {
                BugCheck.Assert( m_maskStackForObjects_Pos < c_MarkStackForObjectsSize - 1, BugCheck.StopCode.NoMarkStack );

                m_maskStackForObjects[++m_maskStackForObjects_Pos] = address;
            }
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void PushArrayReference( UIntPtr   address ,
                                         TS.VTable vTable  )
        {
            ArrayImpl array         = ArrayImpl.CastAsArray( ObjectImpl.FromPointer( address ) );
            int       numOfElements = array.Length;

            if(numOfElements == 0)
            {
                //
                // Empty array, nothing to do.
                //
                return;
            }

            TS.VTable vTableElement = vTable.TypeInfo.ContainedType.VirtualTable;

            if(vTableElement.IsValueType)
            {
                if(vTableElement.GCInfo.Pointers == null)
                {
                    //
                    // It's an array of value types with no pointers, no need to push it.
                    //
                    return;
                }

                //
                // The address is for an array with embedded structures.
                //
            }
            else
            {
                //
                // The address is for an object reference.
                //
                vTableElement = null;
            }

            BugCheck.Assert( m_markStackForArrays_Pos < c_MarkStackForArraysSize - 1, BugCheck.StopCode.NoMarkStack );

            m_markStackForArrays[++m_markStackForArrays_Pos].Push( array, vTable.ElementSize, numOfElements, vTableElement );

            if(m_fFirstLevel)
            {
                m_fFirstLevel = false;

                ProcessMarkStack();

                m_fFirstLevel = true;
            }
        }

        private void ProcessMarkStack()
        {
            while(true)
            {
                int pos;

                pos = m_maskStackForObjects_Pos;
                if(pos >= 0)
                {
                    UIntPtr address = m_maskStackForObjects[pos];

                    m_maskStackForObjects_Pos = pos - 1;

                    ObjectImpl obj    = ObjectImpl.FromPointer( address );
                    TS.VTable  vTable = TS.VTable.Get( obj );

                    VisitHeapObjectFieldsInline( address, vTable );
                    continue;
                }

                pos = m_markStackForArrays_Pos;
                if(pos >= 0)
                {
                    m_markStackForArrays[pos].Visit( this );
                    continue;
                }

                break;
            }
        }

        //--//

        private unsafe void RebuildBrickTable()
        {
            var brickTable = BrickTable.Instance;

            brickTable.Reset();

            for(MemorySegment* heap = MemoryManager.Instance.StartOfHeap; heap != null; heap = heap->Next)
            {
                UIntPtr address = heap->FirstBlock;
                UIntPtr end     = heap->End;

                while(AddressMath.IsLessThan( address, end ))
                {
                    ObjectHeader                       oh    = ObjectHeader.CastAsObjectHeader( address );
                    ObjectHeader.GarbageCollectorFlags flags = oh.GarbageCollectorState;

                    switch(flags)
                    {
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr addressNext = oh.GetNextObjectPointer();

                                //
                                // The arrays used to wrap the free blocks are marked as outside the heap.
                                // We should not add them to the brick table, because otherwise the whole brick table will look allocated
                                // and we don't care for pointers into the free list.
                                //
                                address = addressNext;
                            }
                            break;

                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Marked  :
                            address = AddressMath.Increment( address, sizeof(uint) );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr addressNext = oh.GetNextObjectPointer();

                                brickTable.MarkObject( address, AddressMath.RangeSize( address, addressNext ) );

                                address = addressNext;
                            }
                            break;

                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Marked:
                            address = AddressMath.Increment( address, oh.AllocatedRawBytesSize );
                            break;

                        default:
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;
                    }
                }
            }
        }

        //--//

        [TS.WellKnownMethod( "DebugBrickTable_VerifyBrickTable" )]
        private unsafe void VerifyBrickTable()
        {
            for(MemorySegment* heap = MemoryManager.Instance.StartOfHeap; heap != null; heap = heap->Next)
            {
                UIntPtr address = heap->FirstBlock;
                UIntPtr end     = heap->End;

                while(AddressMath.IsLessThan( address, end ))
                {
                    object obj = FindObject( AddressMath.Increment( address, sizeof(uint) ) );

                    ObjectHeader                       oh    = ObjectHeader.CastAsObjectHeader( address );
                    ObjectHeader.GarbageCollectorFlags flags = oh.GarbageCollectorState;

                    switch(flags)
                    {
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.FreeBlock            | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr addressNext = oh.GetNextObjectPointer();

                                address = addressNext;
                            }
                            break;

                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Marked  :
                            address = AddressMath.Increment( address, sizeof(uint) );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr addressNext = oh.GetNextObjectPointer();

                                address = addressNext;
                            }
                            break;

                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes | ObjectHeader.GarbageCollectorFlags.Marked:
                            address = AddressMath.Increment( address, oh.AllocatedRawBytesSize );
                            break;

                        default:
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;
                    }
                }
            }
        }

        //
        // Access Methods
        //
    }
}
