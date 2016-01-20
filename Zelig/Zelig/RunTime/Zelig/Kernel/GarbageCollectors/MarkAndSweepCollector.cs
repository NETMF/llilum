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
        protected interface MarkAndSweepStackWalker
        {
            void Process( Processor.Context ctx );
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

        private OutOfMemoryException               m_outOfMemoryException;
        private MarkAndSweepStackWalker            m_stackWalker;

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

        protected virtual int MarkStackForObjectsSize
        {
            get { return 1024; }
        }
        protected virtual int MarkStackForArraysSize
        {
            get { return 128; }
        }

        //--//

        protected abstract MarkAndSweepStackWalker CreateStackWalker( );

        public unsafe override void InitializeGarbageCollectionManager()
        {
            m_outOfMemoryException = new OutOfMemoryException();
            m_stackWalker          = CreateStackWalker();
            m_maskStackForObjects  = new UIntPtr           [ MarkStackForObjectsSize ];
            m_markStackForArrays   = new MarkStackForArrays[ MarkStackForArraysSize  ];

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

        public override UIntPtr FindObject( UIntPtr interiorPtr )
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
                                    return UIntPtr.Zero;
                                }

                                address = nextAddress;
                            }
                            break;

                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.GapPlug              | ObjectHeader.GarbageCollectorFlags.Marked  :
                            address = AddressMath.Increment( address, sizeof(uint) );
                            break;

                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return UIntPtr.Zero;

                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return UIntPtr.Zero;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.SpecialHandlerObject | ObjectHeader.GarbageCollectorFlags.Marked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Marked:
                            {
                                UIntPtr nextAddress = oh.GetNextObjectPointer();

                                if(AddressMath.IsLessThan( interiorPtr, nextAddress ))
                                {
                                    return oh.Pack( ).ToPointer( );
                                }

                                address = nextAddress;
                            }
                            break;

                        default:
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return UIntPtr.Zero;
                    }
                }
            }

            return UIntPtr.Zero;
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

            // Take the memory manager lock so we make sure there's no in-process memory allocation as we
            // mark and sweep the memory.
            using(SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder( MemoryManager.Lock ))
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

        protected virtual bool IsThisAGoodPlaceToStopTheWorld()
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

        protected virtual void WalkStackFrames()
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

                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.ReadOnlyObject       | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.UnreclaimableObject  | ObjectHeader.GarbageCollectorFlags.Marked  :
                            BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                            return;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Unmarked:
                            if(Configuration.CollectPerformanceStatistics)
                            {
                                m_perf_deadCount++;
                            }

                            addressNext = oh.GetNextObjectPointer();
                            break;

                        case ObjectHeader.GarbageCollectorFlags.NormalObject         | ObjectHeader.GarbageCollectorFlags.Marked  :
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Marked  :
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
        protected void VisitInternalPointer( UIntPtr address )
        {
            VisitInternalPointerInline( address );
        }

        [NoInline]
        protected void VisitHeapObject( UIntPtr address )
        {
            VisitHeapObjectInline( address );
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void VisitInternalPointerInline( UIntPtr address )
        {
            UIntPtr objAddress = FindObject( address );
            if(objAddress != UIntPtr.Zero)
            {
                VisitHeapObject( objAddress );
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
                    oh.GarbageCollectorState = flags | ObjectHeader.GarbageCollectorFlags.Marked;
                    return;

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
                    {
                        BugCheck.Log  ( "Corruption! address=0x%08x, flags=0x%08x", (int)address.ToUInt32( ), (int)flags );
                        BugCheck.Raise( BugCheck.StopCode.HeapCorruptionDetected );
                    }
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
        private void VisitHeapObjectFields( UIntPtr   fieldAddress ,
                                            TS.VTable vTable       )
        {
            VisitHeapObjectFieldsInline( fieldAddress, vTable );
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private void VisitHeapObjectFieldsInline( UIntPtr   fieldAddress ,
                                                  TS.VTable vTable       )
        {
            TS.GCInfo.Pointer[] pointers      = vTable.GCInfo.Pointers;
            int                 numOfPointers = pointers.Length;

            for(int i = 0; i < numOfPointers; i++)
            {
                VisitHeapObjectField( fieldAddress, ref pointers[i] );
            }
        }

#if !GC_PRECISE_PROFILING
        [Inline]
#endif
        private unsafe void VisitHeapObjectField(     UIntPtr           fieldAddress ,
                                                  ref TS.GCInfo.Pointer pointer      )
        {
            UIntPtr* field            = (UIntPtr*)fieldAddress.ToPointer( );
            UIntPtr  referenceAddress = field[pointer.OffsetInWords];


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
                UIntPtr fieldAddress = ObjectImpl.FromPointer( address ).GetFieldPointer( );

                for(int i = 0; i < numOfPointers; i++)
                {
                    VisitHeapObjectField( fieldAddress, ref pointers[i] );
                }

                ProcessMarkStack();

                m_fFirstLevel = true;
            }
            else
            {
                BugCheck.Assert( m_maskStackForObjects_Pos < MarkStackForObjectsSize - 1, BugCheck.StopCode.NoMarkStack );

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

            BugCheck.Assert( m_markStackForArrays_Pos < MarkStackForArraysSize - 1, BugCheck.StopCode.NoMarkStack );

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

                    VisitHeapObjectFieldsInline( obj.GetFieldPointer(), vTable );
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
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr addressNext = oh.GetNextObjectPointer();

                                brickTable.MarkObject( address, AddressMath.RangeSize( address, addressNext ) );

                                address = addressNext;
                            }
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
                    FindObject( AddressMath.Increment( address, sizeof(uint) ) );

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
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Unmarked:
                        case ObjectHeader.GarbageCollectorFlags.AllocatedRawBytes    | ObjectHeader.GarbageCollectorFlags.Marked  :
                            {
                                UIntPtr addressNext = oh.GetNextObjectPointer();

                                address = addressNext;
                            }
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
