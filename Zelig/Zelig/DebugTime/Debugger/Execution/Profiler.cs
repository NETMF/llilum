//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.IO;
    using System.Windows.Forms;
    using System.Threading;

    using EncDef                 = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using EncDef_VFP             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_VFP;
    using InstructionSet         = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                     = Microsoft.Zelig.CodeGeneration.IR;
    using RT                     = Microsoft.Zelig.Runtime;
    using TS                     = Microsoft.Zelig.Runtime.TypeSystem;
    using Cfg                    = Microsoft.Zelig.Configuration.Environment;


    public class Profiler
    {
        public class AllocationEntry
        {
            //
            // State
            //

            public readonly CallEntry             Context;
            public readonly ulong                 AbsoluteClockTicks;
            public readonly TS.TypeRepresentation Type;
            public readonly int                   ArraySize;

            //
            // Constructor Methods
            //

            internal AllocationEntry( CallEntry             context            ,
                                      ulong                 absoluteClockTicks ,
                                      TS.TypeRepresentation type               ,
                                      int                   arraySize          )
            {
                this.Context            = context;
                this.Type               = type;
                this.AbsoluteClockTicks = absoluteClockTicks;
                this.ArraySize          = arraySize;
            }

            //
            // Helper Methods
            //

            //--//

            public int TotalByteSize
            {
                get
                {
                    var vtbl = this.Type.VirtualTable;

                    return (int)(vtbl.BaseSize + vtbl.ElementSize * this.ArraySize);
                }
            }
        }

        public class CallEntry
        {
            //
            // State
            //

            public readonly ThreadContext           Owner;
            public readonly TS.MethodRepresentation Method;
            public readonly CallEntry               Parent;
            public          CallList                Children;
            public          AllocationList          MemoryAllocations;
            public          ulong                   AbsoluteClockCycles;
            public          long                    InclusiveClockCycles;
            public          long                    InclusiveWaitStates;
            internal        ulong                   m_startCycles;
            internal        ulong                   m_startWaitStates;
            internal        bool                    m_visited;

            //
            // Constructor Methods
            //

            public CallEntry( ThreadContext           tc     ,
                              CallEntry               parent ,
                              TS.MethodRepresentation method )
            {
                this.Owner  = tc;
                this.Method = method;
                this.Parent = parent;

                if(parent != null)
                {
                    tc.Push( this );
                }
            }

            //
            // Helper Methods
            //

            internal void AddSubCall( CallEntry en )
            {
                var lst = this.Children;
                
                if(lst == null)
                {
                    lst = new CallList();

                    this.Children = lst;
                }

                lst.Add( en );
            }

            internal IEnumerable< CallEntry > EnumerateChildren()
            {
                var lst = this.Children;
                
                if(lst != null)
                {
                    foreach(var en in lst)
                    {
                        yield return en;

                        foreach(var enSub in en.EnumerateChildren())
                        {
                            yield return enSub;
                        }
                    }
                }
            }

            internal void ResetVisitedFlag()
            {
                m_visited = false;

                var lst = this.Children;
                
                if(lst != null)
                {
                    foreach(var en in lst)
                    {
                        en.ResetVisitedFlag();
                    }
                }
            }

            internal long ComputeExactInclusiveClockCycles()
            {
                if(m_visited)
                {
                    return 0;
                }

                m_visited = true;

                long res = this.ExclusiveClockCycles;

                if(this.Children != null)
                {
                    foreach(var sub in this.Children)
                    {
                        res += sub.ComputeExactInclusiveClockCycles();
                    }
                }

                return res;
            }

            internal long ComputeExactAllocatedBytes()
            {
                if(m_visited)
                {
                    return 0;
                }

                m_visited = true;

                long res = this.ExclusiveAllocatedBytes;

                if(this.Children != null)
                {
                    foreach(var sub in this.Children)
                    {
                        res += sub.ComputeExactAllocatedBytes();
                    }
                }

                return res;
            }

            //
            // Access Methods
            //

            public int Depth
            {
                get
                {
                    int res = 0;
                    var en  = this;
                    
                    while((en = en.Parent) != null)
                    {
                        res++;
                    }

                    return res;
                }
            }

            public long ExclusiveClockCycles
            {
                get
                {
                    long res = this.InclusiveClockCycles;

                    if(this.Children != null)
                    {
                        foreach(var sub in this.Children)
                        {
                            res -= sub.InclusiveClockCycles;
                        }
                    }

                    return res;
                }
            }

            public long ExclusiveWaitStates
            {
                get
                {
                    long res = this.InclusiveWaitStates;

                    if(this.Children != null)
                    {
                        foreach(var sub in this.Children)
                        {
                            res -= sub.InclusiveWaitStates;
                        }
                    }

                    return res;
                }
            }

            public long InclusiveAllocatedBytes
            {
                get
                {
                    long total = this.ExclusiveAllocatedBytes;

                    if(this.Children != null)
                    {
                        foreach(var sub in this.Children)
                        {
                            total += sub.InclusiveAllocatedBytes;
                        }
                    }

                    return total;
                }
            }

            public long ExclusiveAllocatedBytes
            {
                get
                {
                    long total      = 0;
                    int  headerSize = this.Owner.m_owner.m_objectHeaderSize;

                    if(this.MemoryAllocations != null)
                    {
                        foreach(var memAlloc in this.MemoryAllocations)
                        {
                            total += memAlloc.TotalByteSize + headerSize;
                        }
                    }

                    return total;
                }
            }

            //
            // Debug Methods
            //

            public override string ToString()
            {
                return string.Format( "[Thread:{0}] {1}{2}: {3} clock cycles [{4} exclusive] - {5} bytes allocated", this.Owner.ManagedThreadId, new string( ' ', this.Depth + 1 ), this.Method.ToShortString(), this.InclusiveClockCycles, this.ExclusiveClockCycles, this.InclusiveAllocatedBytes );
            }
        }

        public class ThreadContext
        {
            //
            // State
            //

            internal readonly Profiler          m_owner;
            private           CallEntry         m_activeEntry;

            public   readonly int               ManagedThreadId;
            public   readonly ThreadStatus.Kind ThreadKind;
            public   readonly CallEntry         TopLevel;

            //
            // Constructor Methods
            //

            internal ThreadContext( Profiler          owner ,
                                    int               id    ,
                                    ThreadStatus.Kind kind  )
            {
                m_owner = owner;

                this.ManagedThreadId = id;
                this.ThreadKind      = kind;
                this.TopLevel        = new CallEntry( this, null, null );

                m_activeEntry = this.TopLevel;
            }

            //
            // Helper Methods
            //

            internal void Activate()
            {
                var perf = m_owner.m_svcPerf;
                if(perf != null)
                {
                    ulong cycles     = perf.ClockCycles;
                    ulong waitStates = perf.WaitStates;

                    for(var en = this.ActiveEntry; en != null; en = en.Parent)
                    {
                        en.m_startCycles     = cycles;
                        en.m_startWaitStates = waitStates;
                    }
                }
            }

            internal void Deactivate()
            {
                var perf = m_owner.m_svcPerf;
                if(perf != null)
                {
                    ulong cycles     = perf.ClockCycles;
                    ulong waitStates = perf.WaitStates;

                    for(var en = this.ActiveEntry; en != null; en = en.Parent)
                    {
                        en.InclusiveClockCycles += (long)(cycles     - en.m_startCycles    );
                        en.InclusiveWaitStates  += (long)(waitStates - en.m_startWaitStates);
                    }
                }
            }

            internal void Push( CallEntry en )
            {
                m_activeEntry.AddSubCall( en );
                m_activeEntry = en;

                var perf = m_owner.m_svcPerf;
                if(perf != null)
                {
                    ulong cycles     = perf.ClockCycles;
                    ulong waitStates = perf.WaitStates;

                    en.AbsoluteClockCycles = cycles;
                    en.m_startCycles       = cycles;
                    en.m_startWaitStates   = waitStates;
                }
            }

            internal void Pop()
            {
                if(this.IsActive)
                {
                    var en = m_activeEntry;

                    m_activeEntry = en.Parent;

                    var perf = m_owner.m_svcPerf;
                    if(perf != null)
                    {
                        ulong cycles     = perf.ClockCycles;
                        ulong waitStates = perf.WaitStates;

                        en.InclusiveClockCycles += (long)(cycles     - en.m_startCycles    );
                        en.InclusiveWaitStates  += (long)(waitStates - en.m_startWaitStates);
                    }
                }
            }

            internal void AddAllocation( TS.TypeRepresentation td        ,
                                         int                   arraySize )
            {
                var   perf   = m_owner.m_svcPerf;
                ulong cycles = (perf != null) ? perf.ClockCycles : 0;

                //--//

                var en = this.ActiveEntry;

                var mat = en.MemoryAllocations;

                if(mat == null)
                {
                    mat = new AllocationList();

                    en.MemoryAllocations = mat;
                }

                mat.Add( new AllocationEntry( en, cycles, td, arraySize ) );
            }

            //
            // Access Methods
            //

            public CallEntry ActiveEntry
            {
                get
                {
                    return m_activeEntry;
                }
            }

            public bool IsActive
            {
                get
                {
                    return this.ActiveEntry != this.TopLevel;
                }
            }
        }

        //--//

        public class CallList : GrowOnlyList< CallEntry >
        {
            //
            // Helper Methods
            //

            public long ComputeExactInclusiveClockCycles()
            {
                foreach(var en in this)
                {
                    en.ResetVisitedFlag();
                }

                long res = 0;

                foreach(var en in this)
                {
                    res += en.ComputeExactInclusiveClockCycles();
                }

                return res;
            }

            public long ComputeExactAllocatedBytes()
            {
                foreach(var en in this)
                {
                    en.ResetVisitedFlag();
                }

                long res = 0;

                foreach(var en in this)
                {
                    res += en.ComputeExactAllocatedBytes();
                }

                return res;
            }

            //
            // Access Methods
            //

            public long ExclusiveClockCycles
            {
                get
                {
                    long res = 0;

                    foreach(var en in this)
                    {
                        res += en.ExclusiveClockCycles;
                    }

                    return res;
                }
            }

            public long InclusiveClockCycles
            {
                get
                {
                    long res = 0;

                    foreach(var en in this)
                    {
                        res += en.InclusiveClockCycles;
                    }

                    return res;
                }
            }

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var en in this)
                    {
                        res += en.ExclusiveAllocatedBytes;
                    }

                    return res;
                }
            }
        }

        public class CallsByType : GrowOnlyHashTable< TS.TypeRepresentation, CallList >
        {
            //
            // Helper Method
            //

            internal void AddCall( CallEntry en )
            {
                var      td = en.Method.OwnerType;
                CallList coll;

                if(this.TryGetValue( td, out coll ) == false)
                {
                    coll = new CallList();

                    this[td] = coll;
                }

                coll.Add( en );
            }

            //
            // Access Methods
            //

            public long ExclusiveClockCycles
            {
                get
                {
                    long res = 0;

                    foreach(var md in this.Keys)
                    {
                        res += this[md].ExclusiveClockCycles;
                    }

                    return res;
                }
            }

            public long InclusiveClockCycles
            {
                get
                {
                    long res = 0;

                    foreach(var md in this.Keys)
                    {
                        res += this[md].InclusiveClockCycles;
                    }

                    return res;
                }
            }

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var md in this.Keys)
                    {
                        res += this[md].AllocatedBytes;
                    }

                    return res;
                }
            }
        }

        public class CallsByMethod : GrowOnlyHashTable< TS.MethodRepresentation, CallList >
        {
            //
            // Helper Method
            //

            internal void AddCall( CallEntry en )
            {
                var      md = en.Method;
                CallList coll;

                if(this.TryGetValue( md, out coll ) == false)
                {
                    coll = new CallList();

                    this[md] = coll;
                }

                coll.Add( en );
            }

            //
            // Access Methods
            //

            public long TotalCalls
            {
                get
                {
                    long res = 0;

                    foreach(var key in this.Keys)
                    {
                        res += this[key].Count;
                    }

                    return res;
                }
            }

            public long ExclusiveClockCycles
            {
                get
                {
                    long res = 0;

                    foreach(var key in this.Keys)
                    {
                        res += this[key].ExclusiveClockCycles;
                    }

                    return res;
                }
            }

            public long InclusiveClockCycles
            {
                get
                {
                    long res = 0;

                    foreach(var key in this.Keys)
                    {
                        res += this[key].InclusiveClockCycles;
                    }

                    return res;
                }
            }

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var td in this.Keys)
                    {
                        res += this[td].AllocatedBytes;
                    }

                    return res;
                }
            }
        }

        public class CallsByTypeAndMethod : GrowOnlyHashTable< TS.TypeRepresentation, CallsByMethod >
        {
            //
            // Helper Method
            //

            internal void AddCall( CallEntry en )
            {
                var           td = en.Method.OwnerType;
                CallsByMethod coll;

                if(this.TryGetValue( td, out coll ) == false)
                {
                    coll = new CallsByMethod();

                    this[td] = coll;
                }

                coll.AddCall( en );
            }
        }

        public class Callers : GrowOnlyHashTable< TS.MethodRepresentation, CallsByMethod >
        {
            //
            // Helper Method
            //

            internal void AddCall( CallEntry en )
            {
                var md = en.Method;
                if(md != null)
                {
                    var enParent = en.Parent;

                    if(enParent != null && enParent.Method != null)
                    {
                        CallsByMethod coll;

                        if(this.TryGetValue( md, out coll ) == false)
                        {
                            coll = new CallsByMethod();

                            this[md] = coll;
                        }

                        coll.AddCall( enParent );
                    }
                }
            }
        }

        public class Callees : GrowOnlyHashTable< TS.MethodRepresentation, CallsByMethod >
        {
            //
            // Helper Method
            //

            internal void AddCall( CallEntry en )
            {
                if(en.Children != null)
                {
                    var md = en.Method;
                    if(md != null)
                    {
                        CallsByMethod coll;

                        if(this.TryGetValue( md, out coll ) == false)
                        {
                            coll = new CallsByMethod();

                            this[md] = coll;
                        }

                        foreach(var child in en.Children)
                        {
                            coll.AddCall( child );
                        }
                    }
                }
            }
        }

        public class CallsByMethodAndType : GrowOnlyHashTable< TS.MethodRepresentation, CallsByType >
        {
            //
            // Helper Method
            //

            internal void AddCall( CallEntry en )
            {
                var         md = en.Method;
                CallsByType coll;

                if(this.TryGetValue( md, out coll ) == false)
                {
                    coll = new CallsByType();

                    this[md] = coll;
                }

                coll.AddCall( en );
            }
        }

        //--//

        public class AllocationList : GrowOnlyList< AllocationEntry >
        {
            //
            // Access Methods
            //

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var mem in this)
                    {
                        res += mem.TotalByteSize;
                    }

                    return res;
                }
            }

            public long AllocatedInstances
            {
                get
                {
                    return this.Count;
                }
            }
        }

        public class AllocationsByType : GrowOnlyHashTable< TS.TypeRepresentation, AllocationList >
        {
            //
            // Helper Method
            //

            internal void AddAllocation( AllocationEntry en )
            {
                var            td = en.Type;
                AllocationList coll;

                if(this.TryGetValue( td, out coll ) == false)
                {
                    coll = new AllocationList();

                    this[td] = coll;
                }

                coll.Add( en );
            }

            //
            // Access Methods
            //

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var td in this.Keys)
                    {
                        res += this[td].AllocatedBytes;
                    }

                    return res;
                }
            }
        }

        public class AllocationsByMethod : GrowOnlyHashTable< TS.MethodRepresentation, AllocationList >
        {
            //
            // Helper Method
            //

            internal void AddAllocation( AllocationEntry en )
            {
                var            md = en.Context.Method;
                AllocationList coll;

                if(this.TryGetValue( md, out coll ) == false)
                {
                    coll = new AllocationList();

                    this[md] = coll;
                }

                coll.Add( en );
            }

            //
            // Access Methods
            //

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var md in this.Keys)
                    {
                        res += this[md].AllocatedBytes;
                    }

                    return res;
                }
            }

            public long AllocatedInstances
            {
                get
                {
                    long res = 0;

                    foreach(var md in this.Keys)
                    {
                        res += this[md].AllocatedInstances;
                    }

                    return res;
                }
            }
        }

        public class AllocationsByTypeAndMethod : GrowOnlyHashTable< TS.TypeRepresentation, AllocationsByMethod >
        {
            //
            // Helper Method
            //

            internal void AddAllocation( AllocationEntry en )
            {
                var                 td = en.Type;
                AllocationsByMethod coll;

                if(this.TryGetValue( td, out coll ) == false)
                {
                    coll = new AllocationsByMethod();

                    this[td] = coll;
                }

                coll.AddAllocation( en );
            }

            //
            // Access Methods
            //

            public long AllocatedBytes
            {
                get
                {
                    long res = 0;

                    foreach(var td in this.Keys)
                    {
                        res += this[td].AllocatedBytes;
                    }

                    return res;
                }
            }

            public long AllocatedInstances
            {
                get
                {
                    long res = 0;

                    foreach(var td in this.Keys)
                    {
                        res += this[td].AllocatedInstances;
                    }

                    return res;
                }
            }
        }

        //--//

        //
        // State
        //

        private MemoryDelta                                    m_memDelta;

        private Emulation.Hosting.Interop                      m_svcInterop;
        private Emulation.Hosting.ProcessorPerformance         m_svcPerf;
        private Emulation.Hosting.ProcessorStatus              m_svcStatus;

        private Dictionary< int, ThreadContext >               m_threads;
        private ThreadContext                                  m_activeContext;

        private List< Emulation.Hosting.Interop.Registration > m_interopsForCalls;
        private List< Emulation.Hosting.Interop.Registration > m_interopsForAllocations;

        private bool                                           m_fCollectAllocationData;

        private bool                                           m_fAttachedForCalls;
        private bool                                           m_fAttachedForAllocations;
 
        private int                                            m_objectHeaderSize;

        //
        // Constructor Methods
        //

        public Profiler( MemoryDelta memDelta )
        {
            m_memDelta               = memDelta;

            m_threads                = new Dictionary< int, ThreadContext >();

            m_interopsForCalls       = new List< Emulation.Hosting.Interop.Registration >();
            m_interopsForAllocations = new List< Emulation.Hosting.Interop.Registration >();

            m_objectHeaderSize       = (int)memDelta.ImageInformation.TypeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ObjectHeader.Size;
        }

        //
        // Helper Methods
        //

        internal IEnumerable< CallEntry > EnumerateCalls()
        {
            foreach(var tc in m_threads.Values)
            {
                foreach(var en in tc.TopLevel.EnumerateChildren())
                {
                    yield return en;
                }
            }
        }

        public CallsByTypeAndMethod GetCallsByType()
        {
            var res = new CallsByTypeAndMethod();

            foreach(var en in this.EnumerateCalls())
            {
                res.AddCall( en );
            }

            return res;
        }

        public CallsByMethodAndType GetCallsByMethod()
        {
            var res = new CallsByMethodAndType();

            foreach(var en in this.EnumerateCalls())
            {
                res.AddCall( en );
            }

            return res;
        }

        public void GetCallersAndCallees( out Callers callers ,
                                          out Callees callees )
        {
            callers = new Callers();
            callees = new Callees();

            foreach(var en in this.EnumerateCalls())
            {
                callers.AddCall( en );
                callees.AddCall( en );
            }
        }

        public AllocationsByType GetAllocationsByType()
        {
            var res = new AllocationsByType();

            foreach(var en in this.EnumerateCalls())
            {
                if(en.MemoryAllocations != null)
                {
                    foreach(var mem in en.MemoryAllocations)
                    {
                        res.AddAllocation( mem );
                    }
                }
            }

            return res;
        }

        public AllocationsByMethod GetAllocationsByMethod()
        {
            var res = new AllocationsByMethod();

            foreach(var en in this.EnumerateCalls())
            {
                if(en.MemoryAllocations != null)
                {
                    foreach(var mem in en.MemoryAllocations)
                    {
                        res.AddAllocation( mem );
                    }
                }
            }

            return res;
        }

        public AllocationsByTypeAndMethod GetAllocationsByTypeAndMethod()
        {
            var res = new AllocationsByTypeAndMethod();

            foreach(var en in this.EnumerateCalls())
            {
                if(en.MemoryAllocations != null)
                {
                    foreach(var mem in en.MemoryAllocations)
                    {
                        res.AddAllocation( mem );
                    }
                }
            }

            return res;
        }

        //--//

        public void Attach()
        {
            if(m_fAttachedForCalls == false)
            {
                var host = m_memDelta.Host;

                if(host.GetHostingService( out m_svcInterop ) &&
                   host.GetHostingService( out m_svcStatus  )  )
                {
                    host.GetHostingService( out m_svcPerf );

                    Attach_Calls();

                    if(m_fCollectAllocationData)
                    {
                        Attach_Allocations();
                    }

                    SwitchToNewThread();
                }

                m_fAttachedForCalls = true;
            }
        }

        private void Attach_Calls()
        {
            m_svcStatus.NotifyOnExternalProgramFlowChange += Unwind;

            m_memDelta.ImageInformation.ImageBuilder.EnumerateImageAnnotations( an =>
            {
                if(an.Target is Runtime.ActivationRecordEvents)
                {
                    switch((Runtime.ActivationRecordEvents)an.Target)
                    {
                        case Runtime.ActivationRecordEvents.Constructing:
                            AttachProfiler_Enter( an );
                            break;

                        case Runtime.ActivationRecordEvents.ReturnToCaller:
                            AttachProfiler_Exit( an );
                            break;

                        case Runtime.ActivationRecordEvents.ReturnFromException:
                            AttachProfiler_ExitFromException( an );
                            break;

                        case Runtime.ActivationRecordEvents.LongJump:
                            AttachProfiler_LongJump( an );
                            break;
                    }
                }

                return true;
            } );
        }

        private void Attach_Allocations()
        {
            if(m_fAttachedForAllocations == false)
            {
                var ts = m_memDelta.ImageInformation.TypeSystem;
                if(ts != null)
                {
                    var wkm = ts.WellKnownMethods;

                    SetInteropForVirtualMethod( ts, wkm.TypeSystemManager_AllocateObject, delegate()
                    {
                        RecordAllocation_Object( EncDef.c_register_r1 );

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );

                    SetInteropForVirtualMethod( ts, wkm.TypeSystemManager_AllocateArray, delegate()
                    {
                        RecordAllocation_Array( EncDef.c_register_r1, EncDef.c_register_r2 );

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );

                    SetInteropForVirtualMethod( ts, wkm.TypeSystemManager_AllocateArrayNoClear, delegate()
                    {
                        RecordAllocation_Array( EncDef.c_register_r1, EncDef.c_register_r2 );

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );

                    SetInteropForVirtualMethod( ts, wkm.TypeSystemManager_AllocateString, delegate()
                    {
                        RecordAllocation_Array( EncDef.c_register_r1, EncDef.c_register_r2 );

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );
                }

                m_fAttachedForAllocations =  true;
            }
        }

        private void RecordAllocation_Object( uint encodingVTable )
        {
            var td = ExtractType( encodingVTable );
            if(td != null)
            {
                m_activeContext.AddAllocation( td, 0 );
            }
        }

        private void RecordAllocation_Array( uint encodingVTable ,
                                             uint encodingLength )
        {
            var td = ExtractType( encodingVTable );
            if(td != null)
            {
                var bb = ReadRegister( encodingLength );
                if(bb != null)
                {
                    m_activeContext.AddAllocation( td, (int)bb.ReadUInt32( 0 ) );
                }
            }
        }

        //--//

        public void Detach()
        {
            Detach_Allocations();
            Detach_Calls();

            if(m_activeContext != null)
            {
                m_activeContext.Deactivate();

                m_activeContext = null;
            }
        }

        private void Detach_Calls()
        {
            if(m_fAttachedForCalls)
            {
                m_svcStatus.NotifyOnExternalProgramFlowChange -= Unwind;

                foreach(var reg in m_interopsForCalls)
                {
                    m_svcInterop.RemoveInterop( reg );
                }

                m_interopsForCalls.Clear();

                m_fAttachedForCalls = false;
            }
        }

        private void Detach_Allocations()
        {
            if(m_fAttachedForAllocations)
            {
                foreach(var reg in m_interopsForAllocations)
                {
                    m_svcInterop.RemoveInterop( reg );
                }

                m_interopsForAllocations.Clear();

                m_fAttachedForAllocations = false;
            }
        }

        //--//

        private TS.TypeRepresentation ExtractType( uint encoding )
        {
            var bb = ReadRegister( EncDef.c_register_r1 );
            if(bb != null)
            {
                uint vTablePtr = bb.ReadUInt32( 0 );

                return m_memDelta.ImageInformation.GetTypeFromVirtualTable( vTablePtr );
            }

            return null;
        }

        private Emulation.Hosting.BinaryBlob ReadRegister( uint encoding )
        {
            return m_svcStatus.GetRegister( m_memDelta.ImageInformation.TypeSystem.PlatformAbstraction.GetRegisterForEncoding( encoding ) );
        }

        private void SetInteropForCall( uint                               pc              ,
                                        bool                               fPostProcessing ,
                                        Emulation.Hosting.Interop.Callback ftn             )
        {
            m_interopsForCalls.Add( m_svcInterop.SetInterop( pc, true, fPostProcessing, ftn ) );
        }

        private void SetInteropForVirtualMethod( IR.TypeSystemForCodeTransformation ts  ,
                                                 TS.MethodRepresentation            md  ,
                                                 Emulation.Hosting.Interop.Callback ftn )
        {
            if(md is TS.VirtualMethodRepresentation)
            {
                for(TS.TypeRepresentation td = ts.FindSingleConcreteImplementation( md.OwnerType ); td != null; td = td.Extends)
                {
                    TS.MethodRepresentation md2 = td.FindMatch( md, null );

                    if(md2 != null)
                    {
                        md = md2;
                        break;
                    }
                }
            }

            if(md != null)
            {
                IR.ImageBuilders.SequentialRegion reg = m_memDelta.ImageInformation.ResolveMethodToRegion( md );
                if(reg != null)
                {
                    m_interopsForAllocations.Add( m_svcInterop.SetInterop( reg.ExternalAddress, true, false, ftn ) );
                }
            }
        }

        private CallEntry CreateNewEntry( TS.MethodRepresentation md )
        {
            var en = new CallEntry( m_activeContext, m_activeContext.ActiveEntry, md );

            return en;
        }

        private TS.MethodRepresentation GetMethod( IR.ImageBuilders.ImageAnnotation an )
        {
            var bb = (IR.BasicBlock)an.Region.Context;

            return bb.Owner.Method;
        }

        private ThreadStatus AnalyzeStackFrame( out List< ThreadStatus > lst )
        {
            StopTiming();

            lst = new List< ThreadStatus >();

            m_memDelta.FlushCache();

            var ts = ThreadStatus.Analyze( lst, m_memDelta, null );

            RestartTiming();

            return ts;
        }

        private void SwitchToNewThread()
        {
            List< ThreadStatus > lst;

            var ts = AnalyzeStackFrame( out lst );

            //--//

            if(m_activeContext != null)
            {
                m_activeContext.Deactivate();
            }

            var tc = FindThread( ts );
            if(tc == null)
            {
                int id = ts.ManagedThreadId;

                tc = new ThreadContext( this, id, ts.ThreadKind );

                m_threads[id] = tc;
            }

            m_activeContext = tc;

            tc.Activate();

            if(tc.IsActive == false)
            {
                var md = ts.TopMethod;

                //
                // If we switch to the beginning of a method, we don't need to create an entry, it will be created as part of the normal interop sequence.
                //
                if(ts.ProgramCounter != m_memDelta.ImageInformation.ResolveMethodToRegion( md ).ExternalAddress)
                {
                    CreateNewEntry( ts.TopMethod );
                }
            }
        }

        private void Unwind()
        {
            List< ThreadStatus > lst;

            var ts = AnalyzeStackFrame( out lst );
            var md = ts.TopMethod;

            while(true)
            {
                var en = m_activeContext.ActiveEntry;

                if(en == m_activeContext.TopLevel)
                {
                    break;
                }

                if(en.Method == md)
                {
                    break;
                }

                m_activeContext.Pop();
            }

            if(m_activeContext.IsActive == false)
            {
                CreateNewEntry( md );
            }
        }

        private ThreadContext FindThread( ThreadStatus ts )
        {
            ThreadContext res;

            m_threads.TryGetValue( ts.ManagedThreadId, out res );

            return res;
        }

        private void StopTiming()
        {
            if(m_svcPerf != null)
            {
                m_svcPerf.SuspendTimingUpdates();
            }
        }

        private void RestartTiming()
        {
            if(m_svcPerf != null)
            {
                m_svcPerf.ResumeTimingUpdates();
            }
        }

        //--//

        private void AttachProfiler_Enter( IR.ImageBuilders.ImageAnnotation an )
        {
            var md = GetMethod( an );

            switch(m_memDelta.ImageInformation.TypeSystem.ExtractHardwareExceptionSettingsForMethod( md ))
            {
                case Runtime.HardwareException.UndefinedInstruction:
                case Runtime.HardwareException.PrefetchAbort       :
                case Runtime.HardwareException.DataAbort           :

                case Runtime.HardwareException.Interrupt           :
                case Runtime.HardwareException.FastInterrupt       :
                case Runtime.HardwareException.SoftwareInterrupt   :
                    SetInteropForCall( an.InsertionAddress, false, delegate()
                    {
                        SwitchToNewThread();

                        CreateNewEntry( md );

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );
                    break;

                default:
                    SetInteropForCall( an.InsertionAddress, false, delegate()
                    {
                        CreateNewEntry( md );

                        return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
                    } );
                    break;
            }
        }

        private void AttachProfiler_Exit( IR.ImageBuilders.ImageAnnotation an )
        {
            SetInteropForCall( an.InsertionAddress, true, delegate()
            {
                m_activeContext.Pop();

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );
        }

        private void AttachProfiler_ExitFromException( IR.ImageBuilders.ImageAnnotation an )
        {
            SetInteropForCall( an.InsertionAddress, true, delegate()
            {
                m_activeContext.Pop();

                SwitchToNewThread();

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );
        }

        private void AttachProfiler_LongJump( IR.ImageBuilders.ImageAnnotation an )
        {
            SetInteropForCall( an.InsertionAddress, true, delegate()
            {
                m_activeContext.Pop();

                Unwind();

                return Emulation.Hosting.Interop.CallbackResponse.DoNothing;
            } );
        }

        //
        // Access Methods
        //

        public bool IsActive
        {
            get
            {
               return m_fAttachedForCalls;
            }
        }

        public bool CollectAllocationData
        {
            get
            {
                return m_fCollectAllocationData;
            }

            set
            {
                if(m_fCollectAllocationData != value)
                {
                    m_fCollectAllocationData = value;

                    if(m_fAttachedForCalls)
                    {
                        if(value)
                        {
                            Attach_Allocations();
                        }
                        else
                        {
                            Detach_Allocations();
                        }
                    }
                }
            }
        }

        public ThreadContext[] Threads
        {
            get
            {
                var values = m_threads.Values;

                var res = new ThreadContext[values.Count];

                m_threads.Values.CopyTo( res, 0 );

                return res;
            }
        }
    }
}