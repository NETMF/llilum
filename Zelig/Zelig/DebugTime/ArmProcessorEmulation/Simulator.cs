//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TRACK_EMULATOR_PERFORMANCE

using System.Reflection;

namespace Microsoft.Zelig.Emulation.ArmProcessor
{
    using                      System;
    using                      System.Collections;
    using                      System.Collections.Generic;
    using Cfg                = Microsoft.Zelig.Configuration.Environment;
    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;


    public class Simulator : SimulatorCore
    {
        sealed class ProcessorControlImpl : Emulation.Hosting.ProcessorControl
        {
            //
            // State
            //

            Simulator m_owner;

            //
            // Constructor Methods
            //

            internal ProcessorControlImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Emulation.Hosting.ProcessorControl), this );
            }
            
            //
            // Helper Methods
            //

            public override void GetBreakpointCapabilities( out object softBreakpointOpcode ,
                                                            out int    maxHardBreakpoints   )
            {
                softBreakpointOpcode = null;
                maxHardBreakpoints   = int.MaxValue;
            }

            public override void Shutdown()
            {
                base.Shutdown();

                m_owner.Reset();
            }

            public override void ResetState( Configuration.Environment.ProductCategory product )
            {
                base.ResetState( product );

                m_owner.Reset();
            }

            public override void PrepareHardwareModels( Configuration.Environment.ProductCategory product )
            {
                m_owner.PrepareHardwareModels( product );
            }

            public override void DeployImage( List< Configuration.Environment.ImageSection > image    ,
                                              ProgressCallback                               callback )
            {
                m_owner.DeployImage( image, callback );
            }

            public override void StartPlugIns( List< Type > extraHandlers )
            {
                m_owner.StartPlugIns( m_owner.m_product, extraHandlers );
            }

            public override void StopPlugIns()
            {
                m_owner.StopPlugIns();
            }

            public override void Execute( List< Hosting.Breakpoint > breakpoints )
            {
                m_owner.Execute( breakpoints );
            }

            public override void ExecuteStep( List< Hosting.Breakpoint > breakpoints )
            {
                m_owner.ExecuteStep( breakpoints );
            }

            //
            // Access Methods
            //

            public override bool StopExecution
            {
                get
                {
                    return m_owner.m_fStopExecution;
                }

                set
                {
                    m_owner.m_fStopExecution = value;
                }
            }
        }

        sealed class ProcessorStatusImpl : Emulation.Hosting.ProcessorStatus
        {
            //
            // State
            //

            Simulator m_owner;

            //
            // Constructor Methods
            //

            internal ProcessorStatusImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Emulation.Hosting.ProcessorStatus), this );
            }

            //
            // Helper Methods
            //

            internal void ResetState()
            {
                m_eventExternalProgramFlowChange = null;
            }

            public override Emulation.Hosting.BinaryBlob GetRegister( IR.Abstractions.RegisterDescriptor reg )
            {
                uint idx = reg.Encoding;

                switch(idx)
                {
                    case EncDef.c_register_pc:
                        return Emulation.Hosting.BinaryBlob.Wrap( m_owner.m_pc );

                    case EncDef.c_register_cpsr:
                        return Emulation.Hosting.BinaryBlob.Wrap( m_owner.m_cpsr );

                    default:
                        return Emulation.Hosting.BinaryBlob.Wrap( m_owner.GetRegister( idx ) );
                }
            }

            public override bool SetRegister( IR.Abstractions.RegisterDescriptor reg ,
                                              Emulation.Hosting.BinaryBlob       bb  )
            {
                uint val = bb.ReadUInt32( 0 );
                uint idx = reg.Encoding;

                switch(idx)
                {
                    case EncDef.c_register_pc:
                        m_owner.m_pc = val;
                        break;

                    case EncDef.c_register_cpsr:
                        m_owner.m_cpsr = val;
                        break;

                    default:
                        m_owner.SetRegister( idx, val );
                        break;
                }

                return true;
            }
            
            //--//

            //
            // Access Methods
            //

            public override uint ProgramCounter
            {
                get
                {
                    return m_owner.m_pc;
                }

                set
                {
                    m_owner.m_pc = value;
                }
            }

            public override uint StackPointer
            {
                get
                {
                    return m_owner.GetRegister( EncDef.c_register_sp );
                }

                set
                {
                    m_owner.SetRegister( EncDef.c_register_sp, value );
                }
            }
        }

        sealed class ProcessorPerformanceImpl : Emulation.Hosting.ProcessorPerformance
        {
            //
            // State
            //

            Simulator m_owner;
            TimingState m_timingState;

            //
            // Constructor Methods
            //

            internal ProcessorPerformanceImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Emulation.Hosting.ProcessorPerformance), this );
            }

            //
            // Helper Methods
            //

            public override void SuspendTimingUpdates()
            {
                m_owner.SuspendTimingUpdates( ref m_timingState );
            }

            public override void ResumeTimingUpdates()
            {
                m_owner.ResumeTimingUpdates( ref m_timingState );
            }

            //
            // Access Methods
            //

            public override ulong ClockCycles
            {
                get
                {
                    return m_owner.m_clockTicks;
                }
            }

            public override ulong WaitStates
            {
                get
                {
                    return m_owner.m_busAccess_WaitStates;
                }
            }
        }

        sealed class MonitorExecutionImpl : Emulation.Hosting.MonitorExecution
        {
            //
            // State
            //

            Simulator m_owner;

            //
            // Constructor Methods
            //

            internal MonitorExecutionImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Emulation.Hosting.MonitorExecution), this );
            }

            //
            // Access Methods
            //

            public override bool MonitorMemory
            {
                get
                {
                    return m_owner.m_fMonitorMemory;
                }

                set
                {
                    m_owner.m_fMonitorMemory = value;
                }
            }

            public override bool MonitorRegisters
            {
                get
                {
                    return m_owner.m_fMonitorRegisters;
                }

                set
                {
                    m_owner.m_fMonitorRegisters = value;
                }
            }

            public override bool MonitorOpcodes
            {
                get
                {
                    return m_owner.m_fMonitorOpcodes;
                }

                set
                {
                    m_owner.m_fMonitorOpcodes = value;
                }
            }

            public override bool MonitorCalls
            {
                get
                {
                    return m_owner.m_fMonitorCalls;
                }

                set
                {
                    m_owner.m_fMonitorCalls = value;

                    if(m_owner.m_fMonitorCalls == false)
                    {
                        m_owner.m_registers_Mode_USER .m_callQueue.Clear();
                        m_owner.m_registers_Mode_FIQ  .m_callQueue.Clear();
                        m_owner.m_registers_Mode_IRQ  .m_callQueue.Clear();
                        m_owner.m_registers_Mode_SVC  .m_callQueue.Clear();
                        m_owner.m_registers_Mode_ABORT.m_callQueue.Clear();
                        m_owner.m_registers_Mode_UNDEF.m_callQueue.Clear();
                    }
                }
            }

            public override bool MonitorInterrupts
            {
                get
                {
                    return m_owner.m_fMonitorInterrupts;
                }

                set
                {
                    m_owner.m_fMonitorInterrupts = value;
                }
            }

            public override bool MonitorInterruptDisabling
            {
                get
                {
                    return m_owner.m_fMonitorInterruptDisabling;
                }

                set
                {
                    m_owner.m_fMonitorInterruptDisabling = value;
                }
            }

            public override bool NoSleep
            {
                get
                {
                    return m_owner.m_fNoSleep;
                }

                set
                {
                    m_owner.m_fNoSleep = value;
                }
            }
        }

        sealed class CodeCoverageImpl : Hosting.CodeCoverage
        {
            //
            // State
            //

            Simulator m_owner;

            //
            // Constructor Methods
            //

            internal CodeCoverageImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Hosting.CodeCoverage), this );
            }

            //
            // Helper Methods
            //

            public override void Reset()
            {
                m_owner.m_codeCoverageClusters.Reset();
            }

            //--//

            public override void Dump()
            {
                Hosting.OutputSink sink;

                if(m_owner.GetHostingService( out sink ))
                {
                    Dictionary< uint, ulong > blocks        = new Dictionary< uint, ulong >();
                    Dictionary< uint, ulong > blocksHits    = new Dictionary< uint, ulong >();
                    Dictionary< uint, ulong > blocksWS      = new Dictionary< uint, ulong >();
                    Dictionary< uint, ulong > blocksOpcodes = new Dictionary< uint, ulong >();

                    m_owner.m_codeCoverageClusters.Enumerate( delegate( uint address, uint hits, uint cycles, uint waitStates )
                    {
                        uint context;

                        if(cycles != 0 && m_owner.GetContext( address, out context ))
                        {
                            if(context == address)
                            {
                                blocksHits[context] = hits;
                            }

                            AddToBlocks( blocks       , context, cycles     );
                            AddToBlocks( blocksWS     , context, waitStates );
                            AddToBlocks( blocksOpcodes, context, hits       );
                        }
                    } );

                    sink.OutputLine( "Address\tClock_Cycles\tWait_States\tNumber_of_Opcodes\tNumber_of_Invocations\tClass\tFunction\tModifier" );

                    foreach(uint address in blocks.Keys)
                    {
                        string strClass;
                        string strFunction;
                        string strModifier;

                        SymDef.Unmangle( m_owner.m_symdef_Inverse[address], out strClass, out strFunction, out strModifier );

                        ulong clockCycles = blocks       [address];
                        ulong waitStates  = blocksWS     [address];
                        ulong opcodes     = blocksOpcodes[address];
                        ulong hits;

                        if(blocksHits.TryGetValue( address, out hits ) == false)
                        {
                            hits = 1;
                        }

                        sink.OutputLine( "0x{0:X8}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", address, clockCycles, waitStates, opcodes, hits, strClass, strFunction, strModifier );
                    }
                }
            }

            private void AddToBlocks( Dictionary< uint, ulong > block   ,
                                      uint                      context ,
                                      ulong                     amount  )
            {
                ulong total;

                if(block.TryGetValue( context, out total ))
                {
                    total += amount;
                }
                else
                {
                    total = amount;
                }

                block[context] = total;
            }

            public override void Enumerate( EnumerationCallback dlg )
            {
                m_owner.m_codeCoverageClusters.Enumerate( delegate( uint address, uint hits, uint cycles, uint waitStates )
                {
                    dlg( address, hits, cycles, waitStates );
                } );
            }

            public override void SetSymbols( SymDef.SymbolToAddressMap symdef         ,
                                             SymDef.AddressToSymbolMap symdef_Inverse )
            {
                m_owner.SetSymbols( symdef, symdef_Inverse );
            }


            //
            // Access Methods
            //

            public override bool Enable
            {
                get
                {
                    return m_owner.m_fMonitorCoverage;
                }

                set
                {
                    m_owner.m_fMonitorCoverage = value;
                }
            }
        }

        sealed class InteropImpl : Hosting.Interop
        {
            //
            // State
            //

            Simulator m_owner;

            //
            // Constructor Methods
            //

            internal InteropImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Hosting.Interop), this );
            }

            //
            // Helper Methods
            //

            public override Registration SetInterop( uint                     pc              ,
                                                     bool                     fHead           ,
                                                     bool                     fPostProcessing ,
                                                     Hosting.Interop.Callback ftn             )
            {
                return m_owner.SetInterop( pc, ftn, fHead, fPostProcessing );
            }

            public override void RemoveInterop( Registration reg )
            {
                m_owner.RemoveInterop( reg );
            }
        }

        sealed class DeviceClockTicksTrackingImpl : Hosting.DeviceClockTicksTracking
        {
            //
            // State
            //

            Simulator m_owner;

            //
            // Constructor Methods
            //

            internal DeviceClockTicksTrackingImpl( Simulator owner )
            {
                m_owner = owner;

                m_owner.RegisterService( typeof(Hosting.DeviceClockTicksTracking), this );
            }

            //
            // Helper Methods
            //

            public override bool GetAbsoluteTime( out ulong clockTicks  ,
                                                  out ulong nanoseconds )
            {
                clockTicks  =         m_owner.m_executionTimingState.m_clockTicks;
                nanoseconds = (ulong)(m_owner.m_executionTimingState.m_clockTicks * 1E9 / this.ClockFrequency);

                return true;
            }

            public override IDisposable SuspendTiming()
            {
                return new TimingStateSmartHandler( m_owner );
            }

            protected override void NotifyActivity()
            {
            }

            //
            // Access Methods
            //

            public override ulong ClockTicks
            {
                get
                {
                    return m_owner.m_clockTicks;
                }
            }

            protected override object LockRoot
            {
                get
                {
                    return m_owner;
                }
            }
        }

        //--//

        [AttributeUsageAttribute(AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
        public class InteropAttribute : Attribute
        {
            //
            // State
            //

            public string Function;
        }

        public abstract class InteropHandler
        {
            public static readonly InteropHandler[] SharedEmptyArray = new InteropHandler[0];

            //
            // State
            //

            protected Simulator m_owner;
            private   TimingState m_backup;

            //--//

            //
            // Helper Methods
            //

            internal void CreateInterop( Simulator owner )
            {
                m_owner = owner;

                foreach(InteropAttribute ia in ReflectionHelper.GetAttributes< InteropAttribute >( this, true ))
                {
                    m_owner.SetInterop( ia.Function, PerformInterop, false );
                }
            }

            protected virtual Hosting.Interop.CallbackResponse PerformInterop()
            {
                return m_owner.Interop_GenericSkipCall();
            }

            protected void SaveState()
            {
                m_owner.SuspendTimingUpdates( ref m_backup );
            }

            protected void RestoreState()
            {
                m_owner.ResumeTimingUpdates( ref m_backup );
            }
        }

        //--//

        public class AddressSpaceMapping
        {
            public static readonly AddressSpaceMapping[] SharedEmptyArray = new AddressSpaceMapping[0];

            //
            // State
            //

            public AddressSpaceHandler Target;
            public uint                RangeBase;
            public ulong               RangeLength;

            //
            // Helper Methods
            //

            //
            // Debug Methods
            //

            public override string ToString()
            {
                return this.Target.ToString();
            }
        }

        public abstract class AddressSpaceHandler
        {
            //
            // State
            //

            protected Simulator              m_owner;
            protected AddressSpaceBusHandler m_parent;
            protected ulong                  m_rangeLength;
            protected uint                   m_rangeWidth;
            protected uint                   m_readLatency;
            protected uint                   m_writeLatency;

            //
            // Constructor Methods
            //

            protected AddressSpaceHandler()
            {
            }

            //
            // Helper Methods
            //

            public virtual void Initialize( Simulator owner        ,
                                            ulong     rangeLength  ,
                                            uint      rangeWidth   ,
                                            uint      readLatency  ,
                                            uint      writeLatency )
            {
                if(rangeWidth == 0) rangeWidth = 32;

                m_owner        = owner;
                m_rangeLength  = rangeLength;
                m_rangeWidth   = rangeWidth;
                m_readLatency  = readLatency;
                m_writeLatency = writeLatency;
            }

            internal virtual void LinkToBus( AddressSpaceBusHandler parent )
            {
                CHECKS.ASSERT( m_parent == null || m_parent == parent, "Cannot move an address space handler '{0}' from one bus to another!", this );

                m_parent = parent;
            }

            internal virtual void UnlinkFromBus()
            {
                m_parent = null;
            }

            public void LinkAtAddress( uint address )
            {
                m_parent.AttachHandlerToBus( this, address );
            }

            //--//

            public abstract bool CanAccess( uint                                           address         ,
                                            uint                                           relativeAddress ,
                                            TargetAdapterAbstractionLayer.MemoryAccessType kind            );

            //--//

            public ulong Read64bit( uint address         ,
                                    uint relativeAddress )
            {
                return (ulong)Read( address               , relativeAddress               , TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 )       |
                       (ulong)Read( address + sizeof(uint), relativeAddress + sizeof(uint), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 ) << 32 ;
            }

            public abstract uint Read( uint                                           address         ,
                                       uint                                           relativeAddress ,
                                       TargetAdapterAbstractionLayer.MemoryAccessType kind            );

            public void Write64bit( uint  address         ,
                                    uint  relativeAddress ,
                                    ulong value           )
            {
                Write( address               , relativeAddress               , (uint) value       , TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
                Write( address + sizeof(uint), relativeAddress + sizeof(uint), (uint)(value >> 32), TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );
            }

            public abstract void Write( uint                                           address         ,
                                        uint                                           relativeAddress ,
                                        uint                                           value           ,
                                        TargetAdapterAbstractionLayer.MemoryAccessType kind            );

            public abstract uint GetPhysicalAddress( uint address );

            //--//

            public virtual void UpdateClockTicksForLoad( uint                                           address ,
                                                         TargetAdapterAbstractionLayer.MemoryAccessType kind    )
            {
                UpdateClocks( m_readLatency, kind );
            }

            public virtual void UpdateClockTicksForStore( uint                                           address ,
                                                          TargetAdapterAbstractionLayer.MemoryAccessType kind    )
            {
                UpdateClocks( m_writeLatency, kind );
            }

            //--//

            protected void UpdateClocks( uint                                           clockTicks ,
                                         TargetAdapterAbstractionLayer.MemoryAccessType kind       )
            {
                if(m_owner.AreTimingUpdatesEnabled)
                {
                    uint width;

                    switch(kind)
                    {
                        case TargetAdapterAbstractionLayer.MemoryAccessType.UINT8:
                        case TargetAdapterAbstractionLayer.MemoryAccessType.SINT8:
                            width = 8;
                            break;

                        case TargetAdapterAbstractionLayer.MemoryAccessType.UINT16:
                        case TargetAdapterAbstractionLayer.MemoryAccessType.SINT16:
                            width = 16;
                            break;

                        case TargetAdapterAbstractionLayer.MemoryAccessType.UINT32:
                        case TargetAdapterAbstractionLayer.MemoryAccessType.SINT32:
                        case TargetAdapterAbstractionLayer.MemoryAccessType.FETCH:
                            width = 32;
                            break;

                        default:
                            return;
                    }

                    while(true)
                    {
                        m_owner.m_clockTicks           += clockTicks;
                        m_owner.m_busAccess_WaitStates += clockTicks - 1;

                        if(m_rangeWidth >= width)
                        {
                            break;
                        }

                        width -= m_rangeWidth;
                    }
                }
            }

            //--//

            //
            // Access Methods
            //

            public ulong RangeLength
            {
                get
                {
                    return m_rangeLength;
                }
            }

            public uint ReadLatency
            {
                get
                {
                    return m_readLatency;
                }
            }

            public uint WriteLatency
            {
                get
                {
                    return m_writeLatency;
                }
            }
        }

        public abstract class AddressSpaceBusHandler : AddressSpaceHandler
        {
            //
            // State
            //

            protected AddressSpaceMapping[] m_children;

            //
            // Constructor Methods
            //

            protected AddressSpaceBusHandler()
            {
                m_children = AddressSpaceMapping.SharedEmptyArray;
            }

            //
            // Helper Methods
            //

            public AddressSpaceHandler FindHandlerAtAddress( uint address )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    AddressSpaceHandler hnd = mapper.Target;

                    if(mapper.RangeBase == address)
                    {
                        return hnd;
                    }

                    AddressSpaceBusHandler hndBus = hnd as AddressSpaceBusHandler;
                    if(hndBus != null)
                    {
                        AddressSpaceHandler res = hndBus.FindHandlerAtAddress( address - mapper.RangeBase );
                        if(res != null)
                        {
                            return res;
                        }
                    }
                }

                return null;
            }

            public AddressSpaceHandler FindHandler( Type cls )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    AddressSpaceHandler hnd = mapper.Target;

                    if(hnd.GetType() == cls)
                    {
                        return hnd;
                    }

                    AddressSpaceBusHandler hndBus = hnd as AddressSpaceBusHandler;
                    if(hndBus != null)
                    {
                        AddressSpaceHandler res = hndBus.FindHandler( cls );
                        if(res != null)
                        {
                            return res;
                        }
                    }
                }

                return null;
            }

            public object FindInterface( Type itf )
            {
                foreach(var res in FindInterfaces( itf ))
                {
                    return res;
                }

                return null;
            }

            public IEnumerable FindInterfaces( Type itf )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    AddressSpaceHandler hnd = mapper.Target;

                    if(itf.IsInstanceOfType( hnd ))
                    {
                        yield return hnd;
                    }

                    AddressSpaceBusHandler hndBus = hnd as AddressSpaceBusHandler;
                    if(hndBus != null)
                    {
                        foreach(var res in hndBus.FindInterfaces( itf ))
                        {
                            yield return res;
                        }
                    }
                }
            }

            //--//

            internal void AttachHandlerToBus( AddressSpaceHandler memory    ,
                                              uint                rangeBase )
            {
                m_owner.SuspendInterops();

                foreach(AddressSpaceMapping mapper in m_children)
                {
                    if(mapper.RangeBase == rangeBase)
                    {
                        m_children = ArrayUtility.RemoveUniqueFromNotNullArray( m_children, mapper );
                    }
                }

                AddressSpaceMapping newMapper = new AddressSpaceMapping();

                newMapper.Target      = memory;
                newMapper.RangeBase   = rangeBase;
                newMapper.RangeLength = memory.RangeLength;

                m_children = ArrayUtility.AppendToNotNullArray( m_children, newMapper );

                memory.LinkToBus( this );

                m_owner.ApplyInterops();
            }

            internal override void UnlinkFromBus()
            {
                base.UnlinkFromBus();

                foreach(AddressSpaceMapping mapper in m_children)
                {
                    mapper.Target.UnlinkFromBus();
                }

                m_children = AddressSpaceMapping.SharedEmptyArray;
            }

            //--//

            public override bool CanAccess( uint                                           address         ,
                                            uint                                           relativeAddress ,
                                            TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    uint relativeSubAddress = relativeAddress - mapper.RangeBase;

                    if(relativeSubAddress < mapper.RangeLength)
                    {
                        AddressSpaceHandler hnd = mapper.Target;

                        return hnd.CanAccess( address, relativeSubAddress, kind );
                    }
                }

                return false;
            }

            public override uint Read( uint                                           address         ,
                                       uint                                           relativeAddress ,
                                       TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    uint relativeSubAddress = relativeAddress - mapper.RangeBase;

                    if(relativeSubAddress < mapper.RangeLength)
                    {
                        AddressSpaceHandler hnd = mapper.Target;

                        return hnd.Read( address, relativeSubAddress, kind );
                    }
                }
                
                throw new TargetAdapterAbstractionLayer.BusErrorException( address, kind );
            }

            public override void Write( uint                                           address         ,
                                        uint                                           relativeAddress ,
                                        uint                                           value           ,
                                        TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    uint relativeSubAddress = relativeAddress - mapper.RangeBase;

                    if(relativeSubAddress < mapper.RangeLength)
                    {
                        AddressSpaceHandler hnd = mapper.Target;

                        hnd.Write( address, relativeSubAddress, value, kind );

                        return;
                    }
                }
               
                throw new TargetAdapterAbstractionLayer.BusErrorException( address, kind );
            }

            public override uint GetPhysicalAddress( uint address )
            {
                foreach(AddressSpaceMapping mapper in m_children)
                {
                    uint relativeAddress = address - mapper.RangeBase;

                    if(relativeAddress < mapper.RangeLength)
                    {
                        AddressSpaceHandler hnd = mapper.Target;

                        return hnd.GetPhysicalAddress( address );
                    }
                }

                return address;
            }

            //--//

            //
            // Access Methods
            //

            public AddressSpaceMapping[] AttachedHandlers
            {
                get
                {
                    return m_children;
                }
            }
        }

        //--//

        public class MemoryHandler : AddressSpaceHandler
        {
            //
            // State
            //

            protected uint[] m_target;
            protected bool   m_fMonitorAccesses;

            //
            // Constructor Methods
            //

            public MemoryHandler()
            {
                m_fMonitorAccesses = true;
            }

            //
            // Helper Methods
            //

            public override void Initialize( Simulator owner        ,
                                             ulong     rangeLength  ,
                                             uint      rangeWidth   ,
                                             uint      readLatency  ,
                                             uint      writeLatency )
            {
                base.Initialize( owner, rangeLength, rangeWidth, readLatency, writeLatency );

                m_target = new uint[rangeLength / sizeof(uint)];
            }

            //--//

            public override bool CanAccess( uint                                           address         ,
                                            uint                                           relativeAddress ,
                                            TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                return relativeAddress < m_rangeLength;
            }

            public override uint Read( uint                                           address         ,
                                       uint                                           relativeAddress ,
                                       TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                uint value = m_target[relativeAddress / sizeof(uint)];

                //
                // If there's a detour installed at this spot, read the redirected value!!
                //
                if(value == TrackDetour.c_DetourOpcode && kind != TargetAdapterAbstractionLayer.MemoryAccessType.FETCH)
                {
                    value = m_owner.HandleDetour( address, value );
                }

                UpdateClockTicksForLoad( address, kind );

                switch(kind)
                {
                    case TargetAdapterAbstractionLayer.MemoryAccessType.UINT8:
                        {
                            uint shift = (address % 4) * 8;

                            return (uint)((byte)(value >> (int)shift));
                        }

                    case TargetAdapterAbstractionLayer.MemoryAccessType.UINT16:
                        {
                            uint shift = (address % 4) * 8;

                            return (uint)((ushort)(value >> (int)shift));
                        }

                    case TargetAdapterAbstractionLayer.MemoryAccessType.UINT32:
                    case TargetAdapterAbstractionLayer.MemoryAccessType.SINT32:
                        {
                            return value;
                        }

                    case TargetAdapterAbstractionLayer.MemoryAccessType.SINT8:
                        {
                            uint shift = (address % 4) * 8;

                            return (uint)(int)((sbyte)(value >> (int)shift));
                        }

                    case TargetAdapterAbstractionLayer.MemoryAccessType.SINT16:
                        {
                            uint shift = (address % 4) * 8;

                            return (uint)(int)((short)(value >> (int)shift));
                        }

                    case TargetAdapterAbstractionLayer.MemoryAccessType.FETCH:
                        {
                            return value;
                        }

                    default:
                        throw new NotSupportedException();
                }
            }

            public override void Write( uint                                           address         ,
                                        uint                                           relativeAddress ,
                                        uint                                           value           ,
                                        TargetAdapterAbstractionLayer.MemoryAccessType kind            )
            {
                WriteInner( address, relativeAddress, value, kind, ref m_target[relativeAddress / sizeof(uint)] );
            }

            public override uint GetPhysicalAddress( uint address )
            {
                return address;
            }

            //--//

            private void WriteInner(     uint                                           address         ,
                                         uint                                           relativeAddress ,
                                         uint                                           value           ,
                                         TargetAdapterAbstractionLayer.MemoryAccessType kind            ,
                                     ref uint                                           target          )
            {
                UpdateClockTicksForStore( address, kind );

                uint oldValue = target;
                uint newValue;

                switch(kind)
                {
                    case TargetAdapterAbstractionLayer.MemoryAccessType.UINT8:
                    case TargetAdapterAbstractionLayer.MemoryAccessType.SINT8:
                        newValue = Insert( target, value, 0x000000FF, (int)(address % 4) * 8 );
                        break;

                    case TargetAdapterAbstractionLayer.MemoryAccessType.UINT16:
                    case TargetAdapterAbstractionLayer.MemoryAccessType.SINT16:
                        newValue = Insert( target, value, 0x0000FFFF, (int)(address % 4) * 8 );
                        break;

                    case TargetAdapterAbstractionLayer.MemoryAccessType.UINT32:
                    case TargetAdapterAbstractionLayer.MemoryAccessType.SINT32:
                    case TargetAdapterAbstractionLayer.MemoryAccessType.FETCH:
                        newValue = value;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                //
                // If there's a detour installed at this spot, don't overwrite it!!
                //
                if(oldValue == TrackDetour.c_DetourOpcode)
                {
                    if(m_owner.UpdateDetour( address, newValue ))
                    {
                        return;
                    }
                }

                target = newValue;

                if(m_fMonitorAccesses && m_owner.m_fMonitorMemory)
                {
                    if(oldValue != newValue)
                    {
                        int  shift;
                        uint mask;

                        switch(kind)
                        {
                            case TargetAdapterAbstractionLayer.MemoryAccessType.UINT8:
                            case TargetAdapterAbstractionLayer.MemoryAccessType.SINT8:
                                mask  = 0x000000FF;
                                shift = (int)(address % 4) * 8;
                                break;

                            case TargetAdapterAbstractionLayer.MemoryAccessType.UINT16:
                            case TargetAdapterAbstractionLayer.MemoryAccessType.SINT16:
                                mask  = 0x0000FFFF;
                                shift = (int)(address % 2) * 8;
                                break;

                            default:
                                mask  = 0xFFFFFFFF;
                                shift = 0;
                                break;
                        }

                        oldValue = (oldValue >> shift) & mask;
                        newValue = (newValue >> shift) & mask;

                        Hosting.OutputSink sink;

                        if(m_owner.GetHostingService( out sink ))
                        {
                            sink.OutputLine( " {0} MEM  : 0x{1:X8} -> 0x{2:X8} [{3:X8}]", new string( ' ', 80 ), oldValue, newValue, address );
                        }
                    }
                }
            }

            private static uint Insert( uint oldValue ,
                                        uint newValue ,
                                        uint mask     ,
                                        int  shift    )
            {
                mask     <<= shift;
                newValue <<= shift;

                return (oldValue & ~mask) | (newValue & mask);
            }
        }

        //--//

        [AttributeUsageAttribute(AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
        public class PeripheralRangeAttribute : Attribute
        {
            //
            // State
            //

            public uint Base;
            public uint Length;
            public uint WordSize;
            public uint Latency;
            public uint ReadLatency;
            public uint WriteLatency;
        }

        [AttributeUsageAttribute(AttributeTargets.Field | AttributeTargets.Property, Inherited=true, AllowMultiple=false)]
        public class RegisterAttribute : Attribute
        {
            //
            // State
            //

            public uint Offset;
            public uint Size;
            public int  Instances;
        }

        [AttributeUsageAttribute(AttributeTargets.Field, Inherited=false, AllowMultiple=false)]
        public class LinkToPeripheralAttribute : Attribute
        {
        }

        [AttributeUsageAttribute(AttributeTargets.Field, Inherited=false, AllowMultiple=false)]
        public class LinkToContainerAttribute : Attribute
        {
        }

        [AttributeUsageAttribute(AttributeTargets.Method, Inherited=false, AllowMultiple=false)]
        public class NotifyOnConnectionToPeripheralAttribute : Attribute
        {
            //
            // State
            //

            public string RegisterName;
        }

        public abstract class Peripheral : AddressSpaceBusHandler
        {
            internal class FieldRegisterDescriptor : AddressSpaceHandler
            {
                delegate uint Reader(          );
                delegate void Writer( uint val );

                //
                // State
                //
    
                private object                       m_target;
                private System.Reflection.MemberInfo m_mi;
                private Reader                       m_reader;
                private Writer                       m_writer;

                //
                // Helper Methods
                //

                public override bool CanAccess( uint                                           address         ,
                                                uint                                           relativeAddress ,
                                                TargetAdapterAbstractionLayer.MemoryAccessType kind            )
                {
                    return true;
                }

                public override uint Read( uint                                           address         ,
                                           uint                                           relativeAddress ,
                                           TargetAdapterAbstractionLayer.MemoryAccessType kind            )
                {
                    ulong clockTicks = m_owner.ClockTicks;
                    uint  value      = m_reader();

                    //
                    // If not handled, update the clock ticks here.
                    //
                    if(clockTicks == m_owner.ClockTicks)
                    {
                        UpdateClockTicksForLoad( address, kind );
                    }

                    return value;
                }

                public override void Write( uint                                           address         ,
                                            uint                                           relativeAddress ,
                                            uint                                           value           ,
                                            TargetAdapterAbstractionLayer.MemoryAccessType kind            )
                {
                    ulong clockTicks = m_owner.ClockTicks;

                    m_writer( value );

                    //
                    // If not handled, update the clock ticks here.
                    //
                    if(clockTicks == m_owner.ClockTicks)
                    {
                        UpdateClockTicksForStore( address, kind );
                    }
                }

                public override uint GetPhysicalAddress( uint address )
                {
                    return address;
                }

                //--//

                internal void Connect( Peripheral                   target    ,
                                       System.Reflection.MemberInfo mi        ,
                                       Type                         fieldType )
                {
                    m_target = target;
                    m_mi     = mi;

                    //--//

                    var fi = mi as System.Reflection.FieldInfo;
                    if(fi != null)
                    {
                        if(fieldType == typeof(byte))
                        {
                            m_reader = delegate(          ) { return (uint)(byte)fi.GetValue( m_target            ); };
                            m_writer = delegate( uint val ) {                    fi.SetValue( m_target, (byte)val ); };
                        }
                        else if(fieldType == typeof(ushort))
                        {
                            m_reader = delegate(          ) { return (uint)(ushort)fi.GetValue( m_target              ); };
                            m_writer = delegate( uint val ) {                      fi.SetValue( m_target, (ushort)val ); };
                        }
                        else if(fieldType == typeof(uint))
                        {
                            m_reader = delegate(          ) { return (uint)        fi.GetValue( m_target              ); };
                            m_writer = delegate( uint val ) {                      fi.SetValue( m_target,         val ); };
                        }
                    }

                    var pi = mi as System.Reflection.PropertyInfo;
                    if(pi != null)
                    {
                        if(fieldType == typeof(byte))
                        {
                            m_reader = delegate(          ) { return pi.CanRead  ?  (uint)(byte  )pi.GetValue( m_target             , null ) : 0; };
                            m_writer = delegate( uint val ) { if(    pi.CanWrite )                pi.SetValue( m_target, (byte  )val, null )    ; };
                        }
                        else if(fieldType == typeof(ushort))
                        {
                            m_reader = delegate(          ) { return pi.CanRead  ?  (uint)(ushort)pi.GetValue( m_target             , null ) : 0; };
                            m_writer = delegate( uint val ) { if(    pi.CanWrite )                pi.SetValue( m_target, (ushort)val, null )    ; };
                        }
                        else if(fieldType == typeof(uint))
                        {
                            m_reader = delegate(          ) { return pi.CanRead  ?  (uint)        pi.GetValue( m_target             , null ) : 0; };
                            m_writer = delegate( uint val ) { if(    pi.CanWrite )                pi.SetValue( m_target,         val, null )    ; };
                        }
                    }
                }
            }

            //
            // Constructor Methods
            //

            protected Peripheral()
            {
            }

            //--//

            //
            // Helper Methods
            //

            public virtual void OnConnected()
            {
            }

            public virtual void OnDisconnected()
            {
            }

            internal override void LinkToBus( AddressSpaceBusHandler bus )
            {
                base.LinkToBus( bus );

                foreach(var fi in ReflectionHelper.GetAllInstanceFields( this.GetType() ))
                {
                    ProcessMember( bus, fi, fi.FieldType );
                }

                foreach(var pi in ReflectionHelper.GetAllInstanceProperties( this.GetType() ))
                {
                    ProcessMember( bus, pi, pi.PropertyType );
                }

                m_owner.QueueLinkToPeripheral( null, null, this );
            }

            internal override void UnlinkFromBus()
            {
                base.UnlinkFromBus();

                OnDisconnected();
            }

            private void ProcessMember( AddressSpaceBusHandler       bus       ,
                                        System.Reflection.MemberInfo mi        ,
                                        Type                         fieldType )
            {
                foreach(RegisterAttribute attrib in ReflectionHelper.GetAttributes< RegisterAttribute >( mi, false ))
                {
                    uint offset = attrib.Offset;

                    if(fieldType == typeof(byte))
                    {
                        AddRegisterHandler( mi, fieldType, offset, sizeof(byte) );
                    }
                    else if(fieldType == typeof(ushort))
                    {
                        AddRegisterHandler( mi, fieldType, offset, sizeof(ushort) );
                    }
                    else if(fieldType == typeof(uint))
                    {
                        AddRegisterHandler( mi, fieldType, offset, sizeof(uint) );
                    }
                    else if(fieldType.IsArray)
                    {
                        Type subType = fieldType.GetElementType();

                        if(subType.IsSubclassOf( typeof(Peripheral) ))
                        {
                            Array array = Array.CreateInstance( subType, attrib.Instances );

                            SetRegisterValue( mi, array );

                            for(int i = 0; i < attrib.Instances; i++)
                            {
                                Peripheral subPeripheral = AddSubHandler( subType, offset, attrib.Size, m_rangeWidth );

                                array.SetValue( subPeripheral, i );

                                offset += attrib.Size;
                            }
                        }
                    }
                    else if(fieldType.IsSubclassOf( typeof(Peripheral) ))
                    {
                        Peripheral subPeripheral = AddSubHandler( fieldType, offset, attrib.Size, m_rangeWidth );

                        SetRegisterValue( mi, subPeripheral );
                    }
                }

                if(ReflectionHelper.HasAttribute< LinkToContainerAttribute >( mi, false ))
                {
                    Type typeParent = m_parent.GetType();

                    if(fieldType.IsAssignableFrom( typeParent ))
                    {
                        SetRegisterValue( mi, m_parent );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Field {0} has a type incompatible with the one of its container {1}", mi, m_parent.GetType() );
                    }
                }

                if(ReflectionHelper.HasAttribute< LinkToPeripheralAttribute >( mi, false ))
                {
                    m_owner.QueueLinkToPeripheral( mi, fieldType, this );
                }
            }

            internal void SetRegisterValue( MemberInfo mi    ,
                                            object     value )
            {
                var fi = mi as System.Reflection.FieldInfo;
                if(fi != null)
                {
                    fi.SetValue( this, value );
                    return;
                }

                var pi = mi as System.Reflection.PropertyInfo;
                if(pi != null)
                {
                    pi.SetValue( this, value, null );
                    return;
                }


                throw new NotSupportedException( string.Format( "No valid setter for '{0}'", mi ) );
            }

            private object GetRegisterValue( MemberInfo mi )
            {
                var fi = mi as System.Reflection.FieldInfo;
                if(fi != null)
                {
                    return fi.GetValue( this );
                }

                var pi = mi as System.Reflection.PropertyInfo;
                if(pi != null)
                {
                    return pi.GetValue( this, null );
                }

                throw new NotSupportedException( string.Format( "No valid getter for '{0}'", mi ) );
            }

            private void AddRegisterHandler( System.Reflection.MemberInfo mi        ,
                                             Type                         fieldType ,
                                             uint                         offset    ,
                                             uint                         size      )
            {
                FieldRegisterDescriptor rd = new FieldRegisterDescriptor();

                rd.Initialize( m_owner, size, 0, m_readLatency, m_writeLatency );

                rd.Connect( this, mi, fieldType );

                AttachHandlerToBus( rd, offset );
            }

            private Peripheral AddSubHandler( Type subType ,
                                              uint offset  ,
                                              uint size    ,
                                              uint width   )
            {
                Peripheral sub = (Peripheral)Activator.CreateInstance( subType );

                sub.Initialize( m_owner, size, width, m_readLatency, m_writeLatency );

                AttachHandlerToBus( sub, offset );

                return sub;
            }

            //--//

            internal void LinkedTo( System.Reflection.MemberInfo mi )
            {
                foreach(var mi2 in ReflectionHelper.GetAllInstanceMethods( this.GetType() ))
                {
                    foreach(var attrib in ReflectionHelper.GetAttributes< NotifyOnConnectionToPeripheralAttribute >( mi2, false ))
                    {
                        if(attrib.RegisterName == mi.Name)
                        {
                            mi2.Invoke( this, new object[0] );
                        }
                    }
                }
            }

            //--//

            protected ulong Get64BitValue( uint low  ,
                                           uint high )
            {
                return (ulong)low | ((ulong)high << 32);
            }

            protected void Set64BitValue(     ulong value ,
                                          out uint  low   ,
                                          out uint  high  )
            {
                low  = (uint)(value      );
                high = (uint)(value >> 32);
            }

            //--//

            public static bool TestBitField( ushort val      ,
                                             ushort bitField )
            {
                return (val & bitField) != 0;
            }

            public static bool TestBitField( uint val      ,
                                             uint bitField )
            {
                return (val & bitField) != 0;
            }

            //--//

            public static void SetBitField( ref byte val      ,
                                                byte bitField )
            {
                val |= bitField;
            }

            public static void SetBitField( ref ushort val      ,
                                                ushort bitField )
            {
                val |= bitField;
            }

            public static void SetBitField( ref uint val      ,
                                                uint bitField )
            {
                val |= bitField;
            }

            //--//

            public static void ClearBitField( ref byte val      ,
                                                  byte bitField )
            {
                val &= (byte)~bitField;
            }

            public static void ClearBitField( ref ushort val      ,
                                                  ushort bitField )
            {
                val &= (ushort)~bitField;
            }

            public static void ClearBitField( ref uint val      ,
                                                  uint bitField )
            {
                val &= ~bitField;
            }

            //--//

            public static void MaskedUpdateBitField( ref byte val    ,
                                                         byte newVal ,
                                                         byte mask   )
            {
                val = (byte)((val & ~mask) | (newVal & mask));
            }

            public static void MaskedUpdateBitField( ref ushort val    ,
                                                         ushort newVal ,
                                                         ushort mask   )
            {
                val = (ushort)((val & ~mask) | (newVal & mask));
            }

            public static void MaskedUpdateBitField( ref uint val    ,
                                                         uint newVal ,
                                                         uint mask   )
            {
                val = (val & ~mask) | (newVal & mask);
            }

            //--//

            public static void UpdateBitField( ref byte val      ,
                                                   byte bitField ,
                                                   bool fSet     )
            {
                if(fSet)
                {
                    SetBitField( ref val, bitField );
                }
                else
                {
                    ClearBitField( ref val, bitField );
                }
            }

            public static void UpdateBitField( ref ushort val      ,
                                                   ushort bitField ,
                                                   bool   fSet     )
            {
                if(fSet)
                {
                    SetBitField( ref val, bitField );
                }
                else
                {
                    ClearBitField( ref val, bitField );
                }
            }

            public static void UpdateBitField( ref uint val      ,
                                                   uint bitField ,
                                                   bool fSet     )
            {
                if(fSet)
                {
                    SetBitField( ref val, bitField );
                }
                else
                {
                    ClearBitField( ref val, bitField );
                }
            }
        }

        class TopAddressSpaceBusHandler : AddressSpaceBusHandler
        {
            //
            // Constructor Methods
            //

            internal TopAddressSpaceBusHandler( Simulator owner )
            {
                m_owner = owner;
            }
        }

        //--//

        public delegate void TrackLoad ( uint address,             TargetAdapterAbstractionLayer.MemoryAccessType kind );
        public delegate void TrackStore( uint address, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind );

        public class TrackDetour
        {
            public const uint c_DetourOpcode = (EncDef.c_cond_AL << 28) | EncDef.op_Undefined;

            public static readonly Hosting.Interop.Callback[] SharedEmtpyArray = new Hosting.Interop.Callback[0];

            //
            // State
            //

            public uint                       m_pc;
            public uint                       m_op;
            public bool                       m_active;
            public Hosting.Interop.Callback[] m_callsPre;
            public Hosting.Interop.Callback[] m_callsPost;

            //
            // Constructor Methods
            //

            public TrackDetour( uint pc )
            {
                m_pc        = pc;
                m_op        = c_DetourOpcode;
                m_callsPre  = SharedEmtpyArray;
                m_callsPost = SharedEmtpyArray;
            }
        }

        class TrackBreakpoint
        {
            //
            // State
            //

            internal readonly uint                         m_address;
            internal readonly Hosting.Interop.Callback     m_call;
            private           Hosting.Interop.Registration m_reg;

            //
            // Constructor Methods
            //

            internal TrackBreakpoint( uint address )
            {
                m_address = address;
                m_call    = this.Callback;
            }

            //
            // Helper Methods
            //

            private Hosting.Interop.CallbackResponse Callback()
            {
                return Hosting.Interop.CallbackResponse.StopExecution;
            }

            internal void Enable( Simulator owner )
            {
                if(m_reg == null)
                {
                    m_reg = owner.SetInterop( m_address, m_call, true, false );
                }
            }

            internal void Disable( Simulator owner )
            {
                if(m_reg != null)
                {
                    owner.RemoveInterop( m_reg );

                    m_reg = null;
                }
            }
        }

        class InterruptHistory
        {
            class Context
            {
                //
                // State
                //

                internal uint                            m_programCounter;

                internal uint                            m_hits;
                internal GrowOnlyHashTable< uint, uint > m_ticksSpread;

                internal uint                            m_min;
                internal uint                            m_avg;
                internal uint                            m_max;

                //
                // Constructor Methods
                //

                internal Context( uint programCounter )
                {
                    m_programCounter = programCounter;
                    m_ticksSpread    = HashTableFactory.New< uint, uint >();
                }

                //
                // Helper Methods
                //

                internal void Update( uint ticks )
                {
                    m_hits++;

                    if(m_ticksSpread.ContainsKey( ticks ))
                    {
                        m_ticksSpread[ticks] += 1;
                    }
                    else
                    {
                        m_ticksSpread[ticks] = 1;
                    }
                }

                internal void ComputeStatistics()
                {
                    m_min = uint.MaxValue;
                    m_max = uint.MinValue;

                    uint sum = 0;
                    uint tot = 0;

                    foreach(uint ticks in m_ticksSpread.Keys)
                    {
                        uint count = m_ticksSpread[ticks];

                        m_min = Math.Min( ticks, m_min );
                        m_max = Math.Max( ticks, m_max );

                        sum += ticks * count;
                        tot +=         count;
                    }

                    m_avg = sum / tot;
                }
            }

            //
            // State
            //

            uint                               m_mask;
            string                             m_id;
            ulong                              m_startClockTicks;
            uint                               m_startProgramCounter;
            GrowOnlyHashTable< uint, Context > m_locations;

            //
            // Constructor Methods
            //

            internal InterruptHistory( uint   mask ,
                                       string id   )
            {
                m_mask      = mask;
                m_id        = id;
                m_locations = HashTableFactory.New< uint, Context >();
            }

            //
            // Helper Methods
            //

            internal void Process( Simulator owner ,
                                   uint      diff  ,
                                   uint      cpsr  )
            {
                if((diff & m_mask) != 0)
                {
                    if((cpsr & m_mask) != 0)
                    {
                        m_startClockTicks     = owner.m_clockTicks;
                        m_startProgramCounter = owner.m_pc - 4;
                    }
                    else if(m_startClockTicks != 0)
                    {
                        Context ctx;

                        if(m_locations.TryGetValue( m_startProgramCounter, out ctx ) == false)
                        {
                            ctx = new Context( m_startProgramCounter );

                            m_locations[m_startProgramCounter] = ctx;
                        }

                        uint ticks = (uint)(owner.m_clockTicks - m_startClockTicks);
    
                        ctx.Update( ticks );
                    }
                }
            }

            internal void Dump( Simulator owner )
            {
                Hosting.OutputSink sink;

                if(owner.GetHostingService( out sink ))
                {
                    if(m_locations.Count > 0)
                    {
                        Context[] locations = m_locations.ValuesToArray();

                        foreach(Context ctx in locations)
                        {
                            ctx.ComputeStatistics();
                        }

                        sink.OutputLine( "Sites for {0} disabling:", m_id );
                        sink.OutputLine( " <address> <min>/<avg>/<max> <method name>" );
                        sink.OutputLine( "" );

                        Array.Sort( locations, delegate( Context left, Context right )
                        {
                            return - left.m_max.CompareTo( right.m_max );
                        } );

                        foreach(Context ctx in locations)
                        {
                            uint    context;
                            string  fmt;
                            string  name;

                            if(owner.GetContext( ctx.m_programCounter, out context ))
                            {
                                fmt  = "  0x{0:X8} {1,4}/{2,4}/{3,4} {4}";
                                name = owner.m_symdef_Inverse[context];
                            }
                            else
                            {
                                fmt  = "  0x{0:X8} {1,4}/{2,4}/{3,4}";
                                name = "";
                            }

                            sink.OutputLine( fmt, ctx.m_programCounter, ctx.m_min, ctx.m_avg, ctx.m_max, name );
                        }

                        sink.OutputLine( "" );
                    }
                }
            }
        }

        //--//

        public delegate void CodeCoverageEnumeration( uint address    ,
                                                      uint hits       ,
                                                      uint cycles     ,
                                                      uint waitStates );

        private class CodeCoverageCluster
        {
            internal struct Entry
            {
                //
                // State
                //

                internal uint m_hits;
                internal uint m_cycles;
                internal uint m_waitStates;

                //
                // Helper Methods
                //

                internal void Update( uint cycles     ,
                                      uint waitStates )
                {
                    m_hits       += 1;
                    m_cycles     += cycles;
                    m_waitStates += waitStates;
                }

                internal void Enumerate( uint                    address  ,
                                         CodeCoverageEnumeration callback )
                {
                    if(m_hits != 0)
                    {
                        callback( address, m_hits, m_cycles, m_waitStates );
                    }
                }
            }

            //
            // State
            //
            
            const int c_ClusterSize = 512;

            internal SortedDictionary< uint, Entry[] > m_clusters;
            internal uint                              m_lastClusterIndex;
            internal Entry[]                           m_lastCluster;

            //
            // Constructor Methods
            //

            internal CodeCoverageCluster()
            {
                m_clusters = new SortedDictionary< uint, Entry[] >();

                Reset();
            }

            //
            // Helper Methods
            //

            internal void Reset()
            {
                m_clusters.Clear();

                //
                // Initialize at least one cluster, so lastCluster is always valid.
                //
                m_lastClusterIndex = 0;
                m_lastCluster      = new Entry[c_ClusterSize];

                m_clusters.Add( m_lastClusterIndex, m_lastCluster );
            }

            internal void Update( uint address    ,
                                  uint cycles     ,
                                  uint waitStates )
            {
                Entry[] cluster;

                uint word   = address / sizeof(uint);
                uint index  = word / c_ClusterSize;
                uint offset = word % c_ClusterSize;

                if(m_lastClusterIndex == index)
                {
                    cluster = m_lastCluster;
                }
                else
                {
                    if(m_clusters.TryGetValue( index, out cluster ) == false)
                    {
                        cluster = new Entry[c_ClusterSize];

                        m_clusters.Add( index, cluster );
                    }

                    m_lastClusterIndex = index;
                    m_lastCluster      = cluster;
                }

                cluster[offset].Update( cycles, waitStates );
            }

            internal void Enumerate( CodeCoverageEnumeration callback )
            {
                foreach(KeyValuePair< uint, Entry[] > pair in m_clusters)
                {
                    uint    address = pair.Key * c_ClusterSize * sizeof(uint);
                    Entry[] cluster = pair.Value;

                    for(uint offset = 0; offset < c_ClusterSize; offset++, address += sizeof(uint))
                    {
                        cluster[offset].Enumerate( address, callback );
                    }
                }
            }
        }

        private class PendingLinkToPeripheral
        {
            //
            // State
            //

            internal System.Reflection.MemberInfo m_mi;
            internal Type                         m_fieldType;
            internal Peripheral                   m_peripheral;

            //
            // Constructor Methods
            //

            internal PendingLinkToPeripheral( System.Reflection.MemberInfo mi         ,
                                              Type                         fieldType  ,
                                              Peripheral                   peripheral )
            {
                m_mi         = mi;
                m_fieldType  = fieldType;
                m_peripheral = peripheral;
            }
        }

        private class PendingLinkToBus
        {
            //
            // State
            //

            internal Cfg.BusAttachedCategory m_category;
            internal AddressSpaceHandler     m_handler;
            internal uint                    m_address;

            //
            // Constructor Methods
            //

            internal PendingLinkToBus( Cfg.BusAttachedCategory category ,
                                       AddressSpaceHandler     handler  ,
                                       uint                    address  )
            {
                m_category = category;
                m_handler  = handler;
                m_address  = address;
            }
        }

        //
        // State
        //
                                                           
        protected          bool                             m_fMonitorMemory;
        protected          bool                             m_fMonitorRegisters;
        protected          bool                             m_fMonitorOpcodes;
        protected          bool                             m_fMonitorCalls;
        protected          bool                             m_fMonitorInterrupts;
        protected          bool                             m_fMonitorInterruptDisabling;
        protected          bool                             m_fMonitorCoverage;
        protected          bool                             m_fNoSleep;
                                                           
        protected volatile bool                             m_fStopExecution;
        protected          bool                             m_fRollbackFetch;
                                                           
        protected          AddressSpaceBusHandler           m_topAddressSpaceHandler;
        protected          InteropHandler[]                 m_interopHandlers;
        private            List < PendingLinkToBus        > m_pendingLinkToBus;
        private            Queue< PendingLinkToPeripheral > m_pendingLinkToPeripheral;

        private            ProcessorControlImpl             m_implProcessorControl;
        private            ProcessorStatusImpl              m_implProcessorStatus;
        private            ProcessorPerformanceImpl         m_implProcessorPerformance;
        private            MonitorExecutionImpl             m_implMonitorExecution;

        private            DeviceClockTicksTrackingImpl     m_implDeviceClockTicksTracking;
        protected          TimingState                      m_executionTimingState;
        private            Hosting.OutputSink               m_execution_OutputSink;

        //
        // Code coverage fields.
        //
        private            CodeCoverageImpl                 m_implCodeCoverage;
        private            CodeCoverageCluster              m_codeCoverageClusters;
                                                            
        //                                                  
        // Register monitoring fields.                      
        //                                                  
        private            uint[]                           m_registersBefore = new uint[16];

        //
        // Interrupt disabling history fields.
        //
        private            InterruptHistory                 m_IRQ_history;
        private            InterruptHistory                 m_FIQ_history;
                                                            
        //--//                                              
                                                            
        private            InteropImpl                      m_implInterop;
        protected          Dictionary< uint, TrackDetour >  m_detours;

        protected          TrackLoad                        m_eventTrackLoad;
        protected          TrackStore                       m_eventTrackStore;

        //--//                                              
                                                            
        protected          SymDef.SymbolToAddressMap        m_symdef;
        protected          SymDef.AddressToSymbolMap        m_symdef_Inverse;
        protected          SymDef.AddressToSymbolMap        m_symdef_Inverse_PhysicalAddress;

        //
        // Constructor Methods
        //

        public Simulator(InstructionSet iset) : base(iset)
        {
            m_fMonitorMemory          = false;
            m_fMonitorRegisters       = false;
            m_fMonitorOpcodes         = false;
            m_fMonitorCalls           = false;
                                  
            m_fStopExecution          = false;
            
            m_topAddressSpaceHandler  = new TopAddressSpaceBusHandler( this );
            m_interopHandlers         = InteropHandler.SharedEmptyArray;
            m_pendingLinkToBus        = new List < PendingLinkToBus        >();
            m_pendingLinkToPeripheral = new Queue< PendingLinkToPeripheral >();

            m_codeCoverageClusters    = new CodeCoverageCluster();

            m_detours                 = new Dictionary< uint, TrackDetour >();

            //--//

            m_implProcessorControl         = new ProcessorControlImpl        ( this );
            m_implProcessorPerformance     = new ProcessorPerformanceImpl    ( this );
            m_implProcessorStatus          = new ProcessorStatusImpl         ( this );
            m_implMonitorExecution         = new MonitorExecutionImpl        ( this );
            m_implDeviceClockTicksTracking = new DeviceClockTicksTrackingImpl( this );
            m_implCodeCoverage             = new CodeCoverageImpl            ( this );
            m_implInterop                  = new InteropImpl                 ( this );

            this.RegisterService( typeof(Simulator), this );
        }

        //
        // Helper Methods
        //

        void PrepareHardwareModels( Cfg.ProductCategory product )
        {
            SetHardwareModel( product );
        }

        void DeployImage( List< Configuration.Environment.ImageSection >      image    ,
                          Emulation.Hosting.ProcessorControl.ProgressCallback callback )
        {
            foreach(var section in image)
            {
                if(section.NeedsRelocation == false)
                {
                    if(CanAccess( section.Address, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 ))
                    {
                        LoadMemoryBlock( section.Address, section.Payload );
                    }
                }
            }
        }

        //--//

        public override void Reset()
        {
            m_topAddressSpaceHandler.UnlinkFromBus();

            base.Reset();

            m_fStopExecution                 = false;
                                    
            m_topAddressSpaceHandler         = new TopAddressSpaceBusHandler( this );
            m_interopHandlers                = InteropHandler.SharedEmptyArray;
                                    
            m_detours                        .Clear();

            m_eventTrackLoad                 = null;
            m_eventTrackStore                = null;

            m_implProcessorStatus         .ResetState();
            m_implDeviceClockTicksTracking.ResetState();
                                    
            m_symdef                         = null;
            m_symdef_Inverse                 = null;
            m_symdef_Inverse_PhysicalAddress = null;

            m_executionTimingState           = new TimingState();
            m_execution_OutputSink           = null;

            m_IRQ_history                    = new InterruptHistory( EncDef.c_psr_I, "IRQ" );
            m_FIQ_history                    = new InterruptHistory( EncDef.c_psr_F, "FIQ" );

            SuspendTimingUpdates( ref m_executionTimingState );
        }

        //--//

        public virtual void SetSymbols( SymDef.SymbolToAddressMap symdef         ,
                                        SymDef.AddressToSymbolMap symdef_Inverse )
        {
            m_symdef                         = symdef;
            m_symdef_Inverse                 = symdef_Inverse;
            m_symdef_Inverse_PhysicalAddress = null;
        }

        public void LoadMemoryBlock( uint   address ,
                                     byte[] data    )
        {
            for(int i = 0; i < data.Length; i++)
            {
                if((address % 4) == 0 && i + 4 <= data.Length)
                {
                    uint value = (uint)(data[i+0]      ) |
                                 (uint)(data[i+1] <<  8) |
                                 (uint)(data[i+2] << 16) |
                                 (uint)(data[i+3] << 24) ;

                    Store( address, value, TargetAdapterAbstractionLayer.MemoryAccessType.UINT32 );

                    address += 4;
                    i       += 3;
                }
                else
                {
                    Store( address, data[i], TargetAdapterAbstractionLayer.MemoryAccessType.UINT8 );

                    address += 1;
                }
            }
        }

        //--//

        public virtual void SpinUntilInterrupts()
        {
            while(m_fStopExecution == false)
            {
                while(m_implDeviceClockTicksTracking.ShouldProcessClockTicksCallback( m_clockTicks ))
                {
                    m_implDeviceClockTicksTracking.ProcessClockTicksCallback();

                    if(m_fStopExecution)
                    {
                        return;
                    }
                }

                if(m_interruptStatus != 0)
                {
                    break;
                }

                long     sleepClockTicks = m_implDeviceClockTicksTracking.TimeToNextCallback( m_clockTicks );
                TimeSpan tm              = sleepClockTicks < 0 ? TimeSpan.FromMilliseconds( 1000 ) : ClockTicksToTime( sleepClockTicks );
                DateTime start           = DateTime.Now;

                if(m_fNoSleep || tm.TotalMilliseconds < 0.5)
                {
                    //
                    // Skip the sleep.
                    //
                }
                else
                {
                    Hosting.SimulatorControl sink;
                    
                    if(this.GetHostingService( out sink ))
                    {
                        sink.Wait( tm );

                        TimeSpan slept = DateTime.Now - start;

                        if(slept < tm)
                        {
                            sleepClockTicks = TimeToClockTicks( slept );
                        }
                    }
                }

                UpdateSleepTicks( sleepClockTicks );
            }
        }

        //--//

        protected override void SetHardwareModel( Cfg.ProductCategory product )
        {
            base.SetHardwareModel( product );

            m_implDeviceClockTicksTracking.ClockFrequency = m_clockFrequency;

            foreach(Cfg.AbstractCategory.ValueContext ctx in product.SearchValues( typeof(Cfg.AbstractCategory) ))
            {
                var category = ctx.Value as Cfg.AbstractCategory;

                if(category != null)
                {
                    Type t = category.Model;

                    if(t != null)
                    {
                        if(category is Cfg.MemoryCategory)
                        {
                            CreateMemory( (Cfg.MemoryCategory)category );
                        }

                        if(category is Cfg.PeripheralCategory)
                        {
                            CreatePeripheral( (Cfg.PeripheralCategory)category );
                        }

                        if(category is Cfg.InteropCategory)
                        {
                            CreateInterop( (Cfg.InteropCategory)category );
                        }
                    }
                }
            }

            do
            {
                ProcessPendingLinkToBus();

                ProcessPendingLinkToPeripheral();
            } while(m_pendingLinkToBus.Count > 0);

            if(m_symdef_Inverse != null && m_symdef_Inverse_PhysicalAddress == null)
            {
                m_symdef_Inverse_PhysicalAddress = new SymDef.AddressToSymbolMap();

                foreach(uint address in m_symdef_Inverse.Keys)
                {
                    m_symdef_Inverse_PhysicalAddress[GetPhysicalAddress(address)] = m_symdef_Inverse[address];
                }
            }
        }

        private void QueueLinkToBus( Cfg.BusAttachedCategory category ,
                                     AddressSpaceHandler     handler  ,
                                     uint                    address  )
        {
            if(category.ConnectedToBus != null)
            {
                m_pendingLinkToBus.Add( new PendingLinkToBus( category, handler, address ) );
            }
        }

        private void ProcessPendingLinkToBus()
        {
            for(int i = 0; i < m_pendingLinkToBus.Count; )
            {
                PendingLinkToBus       link = m_pendingLinkToBus[i];
                Type                   t    = link.m_category.ConnectedToBus;
                AddressSpaceBusHandler bus  = null;

                //
                // The 'ConnectedToBus' field refers to the definition type, we need to find the actual model.
                //
                foreach(Cfg.HardwareModelAttribute attrib in ReflectionHelper.GetAttributes< Cfg.HardwareModelAttribute >( t, true ))
                {
                    bus = m_topAddressSpaceHandler.FindHandler( attrib.Target ) as AddressSpaceBusHandler;
                    if(bus != null)
                    {
                        break;
                    }
                }

                if(bus == null)
                {
                    if(t.IsSubclassOf( typeof(Cfg.ProcessorCategory) ))
                    {
                        bus = m_topAddressSpaceHandler;
                    }
                    else
                    {
                        bus = m_topAddressSpaceHandler.FindHandler( t ) as AddressSpaceBusHandler;
                    }
                }

                if(bus != null)
                {
                    bus.AttachHandlerToBus( link.m_handler, link.m_address );

                    m_pendingLinkToBus.RemoveAt( i );

                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            if(m_pendingLinkToBus.Count != 0)
            {
                //
                // More than one model might be broken, just report the first one.
                //
                PendingLinkToBus link = m_pendingLinkToBus[0];

                throw new NotSupportedException( string.Format( "Failed to initialize model '{0}': bus '{1}' does not exist", link.m_handler, link.m_category.ConnectedToBus ) );
            }
        }

        //--//

        public void FindMemory<T>( out T res ) where T : AddressSpaceHandler
        {
            res = (T)m_topAddressSpaceHandler.FindHandler( typeof(T) );
        }

        public AddressSpaceHandler FindMemoryAtAddress( uint address )
        {
            return m_topAddressSpaceHandler.FindHandlerAtAddress( address );
        }

        private void CreateMemory( Cfg.MemoryCategory mem )
        {
            Type t = mem.Model;
            if(t != null)
            {
                if(t.IsSubclassOf( typeof(AddressSpaceHandler) ))
                {
                    var handler = (AddressSpaceHandler)Activator.CreateInstance( t );

                    uint latency = mem.WaitStates + 1;

                    handler.Initialize( this, mem.SizeInBytes, mem.WordSize, latency, latency );

                    QueueLinkToBus( mem, handler, mem.BaseAddress );
                    return;
                }
            }

            throw TypeConsistencyErrorException.Create( "Unrecognized handler for {0}", mem );
        }

        //--//

        public T FindInterface<T>() where T : class
        {
            return (T)m_topAddressSpaceHandler.FindInterface( typeof(T) );
        }

        public IEnumerable<T> FindInterfaces<T>() where T : class
        {
            foreach(var res in m_topAddressSpaceHandler.FindInterfaces( typeof(T) ))
            {
                yield return (T)res;
            }
        }

        //--//

        public Peripheral FindPeripheral( Type cls )
        {
            return (Peripheral)m_topAddressSpaceHandler.FindHandler( cls );
        }

        public void FindPeripheral<T>( out T res ) where T : Peripheral
        {
            res = (T)FindPeripheral( typeof(T) );
        }

        private void CreatePeripheral( Cfg.PeripheralCategory category )
        {
            Peripheral peripheral = null;

            foreach(PeripheralRangeAttribute attrib in ReflectionHelper.GetAttributes< PeripheralRangeAttribute >( category.Model, true ))
            {
                if(peripheral == null)
                {
                    peripheral = (Peripheral)Activator.CreateInstance( category.Model );

                    uint readLatency  = 1;
                    uint writeLatency = 1;

                    if(attrib.Latency != 0)
                    {
                        readLatency  = attrib.Latency;
                        writeLatency = attrib.Latency;
                    }

                    if(attrib.ReadLatency != 0)
                    {
                        readLatency  = attrib.ReadLatency;
                    }

                    if(attrib.WriteLatency != 0)
                    {
                        writeLatency = attrib.WriteLatency;
                    }

                    peripheral.Initialize( this, attrib.Length, attrib.WordSize, readLatency, writeLatency );
                }

                QueueLinkToBus( category, peripheral, attrib.Base );
            }

            if(peripheral == null)
            {
                throw new NotSupportedException( string.Format( "{0} does not have any PeripheralRange attribute", category.Model ) );
            }
        }

        public void QueueLinkToPeripheral( System.Reflection.MemberInfo mi         ,
                                           Type                         fieldType  ,
                                           Peripheral                   peripheral )
        {
            m_pendingLinkToPeripheral.Enqueue( new PendingLinkToPeripheral( mi, fieldType, peripheral ) );
        }

        protected void ProcessPendingLinkToPeripheral()
        {
            while(m_pendingLinkToPeripheral.Count > 0)
            {
                PendingLinkToPeripheral      link       = m_pendingLinkToPeripheral.Dequeue();
                System.Reflection.MemberInfo mi         = link.m_mi;
                Type                         fieldType  = link.m_fieldType;
                Peripheral                   peripheral = link.m_peripheral;

                if(mi == null)
                {
                    peripheral.OnConnected();
                }
                else
                {
                    Peripheral target = FindPeripheral( fieldType );
                    if(target != null)
                    {
                        peripheral.SetRegisterValue( mi, target );

                        peripheral.LinkedTo( mi );
                    }
                    else
                    {
                        throw TypeConsistencyErrorException.Create( "Cannot resolve link to peripheral {0} from {1}::{2}", fieldType, peripheral.GetType(), mi.Name );
                    }
                }
            }
        }

        //--//

        private void CreateInterop( Cfg.InteropCategory category )
        {
            var interop = (InteropHandler)Activator.CreateInstance( category.Model );

            interop.CreateInterop( this );

            m_interopHandlers = ArrayUtility.AppendToNotNullArray( m_interopHandlers, interop );
        }

        //--//

        public override bool CanAccess( uint                                           address ,
                                        TargetAdapterAbstractionLayer.MemoryAccessType kind    )
        {
            return m_topAddressSpaceHandler.CanAccess( address, address, kind );
        }

        public override uint Load( uint                                           address ,
                                   TargetAdapterAbstractionLayer.MemoryAccessType kind    )
        {
            m_busAccess_Read++;

            if(m_eventTrackLoad != null)
            {
                m_eventTrackLoad( address, kind );
            }

////        if((address & 0xFFFFFF00) == 0xDEADBE00)
////        {
////            throw new DataAbortException( address, kind );
////        }

            return m_topAddressSpaceHandler.Read( address, address, kind );
        }

        public override void Store( uint                                           address ,
                                    uint                                           value   ,
                                    TargetAdapterAbstractionLayer.MemoryAccessType kind    )
        {
            m_busAccess_Write++;

            if(m_eventTrackStore != null)
            {
                m_eventTrackStore( address, value, kind );
            }

////        if((address & 0xFFFFFF00) == 0xDEADBE00)
////        {
////            throw new DataAbortException( address, kind );
////        }

            m_topAddressSpaceHandler.Write( address, address, value, kind );
        }

        public override uint TimeMemoryAccess( uint                                           address ,
                                               TargetAdapterAbstractionLayer.MemoryAccessType kind    )
        {
            var backup = new TimingState();

            SuspendTimingUpdates( ref backup );

            Load( address, kind );

            uint res = (uint)(m_clockTicks - backup.m_clockTicks);

            ResumeTimingUpdates( ref backup );

            return res;
        }

        public override uint GetPhysicalAddress( uint address )
        {
            return m_topAddressSpaceHandler.GetPhysicalAddress( address );
        }

        //--//

        public virtual void Execute( List< Hosting.Breakpoint > breakpoints )
        {
            List< TrackBreakpoint > res = PrepareToExecute( breakpoints );

#if TRACK_EMULATOR_PERFORMANCE
            DateTime start      = DateTime.Now;
            ulong    clockTicks = m_clockTicks;
            ulong    count      = 1;

            Hosting.OutputSink sink; this.GetHostingService( out sink );
#endif

            ResumeTimingUpdates( ref m_executionTimingState );

            this.GetHostingService( out m_execution_OutputSink );

            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Stopped , Hosting.ProcessorControl.State.Starting  );
            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Starting, Hosting.ProcessorControl.State.Executing );

            //--//

            while(m_fStopExecution == false)
            {
#if TRACK_EMULATOR_PERFORMANCE
                const uint stepMonitor = 1024 * 1024;

                if((count % stepMonitor) == 0)
                {
                    if(sink != null)
                    {
                        TimeSpan diff      = DateTime.Now - start;
                        double   seconds   = diff.TotalSeconds;
                        double   realTicks = TimeToClockTicks( diff );

                        sink.OutputLine( "#### {0} ops, {1:F1} slowdown ratio", (int)(stepMonitor / seconds), realTicks / (m_clockTicks - clockTicks) );

                        start      = DateTime.Now;
                        clockTicks = m_clockTicks;
                    }
                }

                count++;
#endif

                ExecuteStepInner();
            }

            //--//

            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Executing, Hosting.ProcessorControl.State.Stopping );

            SuspendTimingUpdates( ref m_executionTimingState );

            CleanupAfterExecution( res );

            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Stopping, Hosting.ProcessorControl.State.Stopped );
        }

        public virtual void ExecuteStep( List< Hosting.Breakpoint > breakpoints )
        {
            List< TrackBreakpoint > res = PrepareToExecute( breakpoints );

            ResumeTimingUpdates( ref m_executionTimingState );

            this.GetHostingService( out m_execution_OutputSink );

            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Stopped , Hosting.ProcessorControl.State.Starting  );
            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Starting, Hosting.ProcessorControl.State.Executing );

            //--//

            ExecuteStepInner();

            //--//

            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Executing, Hosting.ProcessorControl.State.Stopping );

            SuspendTimingUpdates( ref m_executionTimingState );

            CleanupAfterExecution( res );

            m_implProcessorControl.Notify( Hosting.ProcessorControl.State.Stopping, Hosting.ProcessorControl.State.Stopped );
        }

        private List< TrackBreakpoint > PrepareToExecute( List< Hosting.Breakpoint > breakpoints )
        {
            var res = new List< TrackBreakpoint >();

            foreach(var bp in breakpoints)
            {
                var track = new TrackBreakpoint( bp.Address );

                track.Enable( this );

                res.Add( track );
            }

            //
            // Prepare Interop detours.
            //
            ApplyInterops();

            Hosting.OutputSink sink;

            if(this.GetHostingService( out sink ))
            {
                sink.StartOutput();
            }

            return res;
        }

        private void CleanupAfterExecution( List< TrackBreakpoint > res )
        {
            foreach(TrackBreakpoint track in res)
            {
                track.Disable( this );
            }

            if(m_fMonitorInterruptDisabling)
            {
                m_IRQ_history.Dump( this );
                m_FIQ_history.Dump( this );
            }
        }

        private void ExecuteStepInner()
        {
            while(m_implDeviceClockTicksTracking.ShouldProcessClockTicksCallback( m_clockTicks ))
            {
                m_implDeviceClockTicksTracking.ProcessClockTicksCallback();

                if(m_fStopExecution)
                {
                    return;
                }
            }

            uint interruptMask = (m_interruptStatus & ~m_cpsr);

            if(interruptMask != 0)
            {
                uint cpsrPost = m_cpsr & ~EncDef.c_psr_mode;
                uint targetPC;

                if((interruptMask & EncDef.c_psr_F) != 0)
                {
                    if(m_fMonitorInterrupts)
                    {
                        if(m_execution_OutputSink != null)
                        {
                            m_execution_OutputSink.OutputLine( "#### Dispatching FIQ interrupt" );
                        }
                    }

                    cpsrPost |= EncDef.c_psr_mode_FIQ;
                    cpsrPost |= EncDef.c_psr_F;
                    cpsrPost |= EncDef.c_psr_I;

                    targetPC = 0x0000001C;
                }
                else
                {
                    if(m_fMonitorInterrupts)
                    {
                        if(m_execution_OutputSink != null)
                        {
                            m_execution_OutputSink.OutputLine( "#### Dispatching IRQ interrupt" );
                        }
                    }

                    cpsrPost |= EncDef.c_psr_mode_IRQ;
                    cpsrPost |= EncDef.c_psr_I;

                    targetPC = 0x00000018;
                }

                uint spsr = m_cpsr;

                SwitchMode( cpsrPost );

                SetRegister( RegisterLookup.SPSR , spsr     );
                SetRegister( EncDef.c_register_lr, m_pc + 4 );

                m_pc = targetPC;
            }

            //--//

            uint pc                                    =       m_pc;
            uint instructionStart_clockTicks           = (uint)m_clockTicks;
            uint instructionStart_busAccess_WaitStates = (uint)m_busAccess_WaitStates;
            uint instruction;

            try
            {
                instruction = Load( pc, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );
            }
            catch(Exception e)
            {
                if(m_execution_OutputSink != null)
                {
                    m_execution_OutputSink.OutputLine( "FETCH ABORT at 0x{0:x8} for {1}", pc, e );
                }

                m_fStopExecution = true;
                throw;
            }

            if(m_fMonitorCalls)
            {
                SniffForCallPop( m_currentBank.m_mode, m_currentBank.m_mode );
                SniffForCallPush();
            }

            //--//--//--//--//--//--//--//--//--//--//--//

#if ARMEMULATOR_VERIFY_ENCODING
            try
            {
                uint                  instruction2 = Load( pc, MemoryAccessType.UINT32 );
                InstructionSet.Opcode op           = m_instructionSet.Decode( instruction2 );
                uint                  instruction3 = op.Encode();

                if(instruction != instruction2)
                {
                    uint target;
                    bool targetIsCode;

                    string res = m_instructionSet.DecodeAndPrint( pc, instruction, out target, out targetIsCode );

                    if(m_execution_OutputSink != null)
                    {
                        m_execution_OutputSink.OutputLine( "MISMATCH: {0:X8} != {1:X8} 0x{2:X8}:  {3}", instruction2, instruction3, pc, res );
                    }
                }
            }
            catch
            {
            }
#endif

            if(m_fMonitorOpcodes)
            {
                uint opcode = instruction;
                
                if(opcode == TrackDetour.c_DetourOpcode)
                {
                    opcode = m_detours[ pc ].m_op;
                }

                try
                {
                    InstructionSet.Opcode op = m_instructionSet.Decode( opcode );

                    bool fExecuted = CheckConditions( op.ConditionCodes );
                    uint target;
                    bool targetIsCode;

                    string res = m_instructionSet.DecodeAndPrint( pc, opcode, out target, out targetIsCode );

                    if(m_execution_OutputSink != null)
                    {
                        m_execution_OutputSink.OutputLine( "{0,-9} 0x{1:X8}:  {2:X8}  {3}{4}{5}{6} {7} {8}", instructionStart_clockTicks, pc, opcode,
                                                           Negative() != 0 ? 'N' : '-',
                                                           Zero()     != 0 ? 'Z' : '-',
                                                           Carry()    != 0 ? 'C' : '-',
                                                           Overflow() != 0 ? 'V' : '-',
                                                           fExecuted       ? ' ' : '*', res );
                    }
                }
                catch(Exception e)
                {
                    if(m_execution_OutputSink != null)
                    {
                        m_execution_OutputSink.OutputLine( "DECODE ABORT at 0x{0:X8} for {1}", pc, e );
                    }

                    throw;
                }
            }

            try
            {
                if(m_fMonitorRegisters)
                {
                    for(uint reg = EncDef.c_register_r0; reg < EncDef.c_register_r15; reg++)
                    {
                        m_registersBefore[reg] = GetRegister( reg );
                    }

                    Execute( instruction );

                    for(uint reg = EncDef.c_register_r0; reg < EncDef.c_register_r15; reg++)
                    {
                        if(m_registersBefore[reg] != GetRegister( reg ))
                        {
                            if(m_execution_OutputSink != null)
                            {
                                m_execution_OutputSink.OutputLine( " {0} {1,-4} : 0x{2:X8} -> 0x{3:X8}", new string( ' ', 80 ), InstructionSet.Opcode.DumpRegister( reg ), m_registersBefore[reg], GetRegister( reg ) );
                            }
                        }
                    }
                }
                else
                {
                    Execute( instruction );
                }

                if(m_fRollbackFetch)
                {
                    m_clockTicks           = instructionStart_clockTicks;
                    m_busAccess_WaitStates = instructionStart_busAccess_WaitStates;
                    m_fRollbackFetch       = false;
                }
            }
            catch(Exception e)
            {
                if(m_execution_OutputSink != null)
                {
                    m_execution_OutputSink.OutputLine( "EXECUTION ABORT at 0x{0:X8} for {1}", pc, e );
                }

                throw;
            }

            if(m_fMonitorCoverage)
            {
                uint cycles     = (uint)m_clockTicks           - instructionStart_clockTicks;
                uint waitStates = (uint)m_busAccess_WaitStates - instructionStart_busAccess_WaitStates;

                m_codeCoverageClusters.Update( pc, cycles, waitStates );
            }
        }

        public override void SwitchMode( uint mode )
        {
            BankedRegisters from = GetShadowRegisters( m_cpsr );
            BankedRegisters to   = GetShadowRegisters( mode   );

            if(m_fMonitorInterrupts && from != to)
            {
                if(m_execution_OutputSink != null)
                {
                    m_execution_OutputSink.OutputLine( "#### Leaving mode {0}, PC={1:X8}, LR={2:X8}" , InstructionSet.DumpMode( m_cpsr ), m_pc, GetRegister( EncDef.c_register_lr ) );
                }
            }

            if(m_fMonitorCalls)
            {
                SniffForCallPop( from.m_mode, to.m_mode );
            }

            base.SwitchMode( mode );

            if(m_fMonitorInterrupts && from != to)
            {
                if(m_execution_OutputSink != null)
                {
                    m_execution_OutputSink.OutputLine( "#### Entering mode {0}, PC={1:X8}, LR={2:X8}" , InstructionSet.DumpMode( m_cpsr ), m_pc, GetRegister( EncDef.c_register_lr ) );
                }
            }
        }

        //--//

        protected override void ProcessInterruptDisabling( uint cpsrPre  ,
                                                           uint cpsrPost )
        {
            if(m_fMonitorInterruptDisabling)
            {
                uint diff = (cpsrPre ^ cpsrPost);

                if((diff & (EncDef.c_psr_I | EncDef.c_psr_F)) != 0)
                {
                    m_IRQ_history.Process( this, diff, cpsrPost );
                    m_FIQ_history.Process( this, diff, cpsrPost );
                }
            }
        }

        protected override bool ProcessUnsupportedOperation( uint op )
        {
            if(op == TrackDetour.c_DetourOpcode)
            {
                m_pc -= 4;

                //
                // Copy, not reference, the entry can be removed from the map by the interop code.
                //
                TrackDetour detour = m_detours[ m_pc ];

                var res = ProcessDetour( detour, detour.m_callsPre, false );

                if((res & Hosting.Interop.CallbackResponse.StopExecution) != 0)
                {
                    m_fStopExecution = true;
                    m_fRollbackFetch = true; // We need to rollback the fetch, to get accurate timing.
                    return true;
                }

                if((res & Hosting.Interop.CallbackResponse.NextInstruction) != 0)
                {
                    return true;
                }

                Execute( detour.m_op );

                res = ProcessDetour( detour, detour.m_callsPost, true );
    
                if((res & Hosting.Interop.CallbackResponse.StopExecution) != 0)
                {
                    m_fStopExecution = true;
                    return true;
                }

                return true;
            }

            return false;
        }

        //--//

        internal void SuspendInterops()
        {
            foreach(TrackDetour detour in m_detours.Values)
            {
                SuspendInterop( detour );
            }
        }

        internal void SuspendInterop( TrackDetour detour )
        {
            if(detour.m_active)
            {
                detour.m_active = false;

                TimingState state = new TimingState();

                SuspendTimingUpdates( ref state );

                Store( detour.m_pc, detour.m_op, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );

                ResumeTimingUpdates( ref state );
            }
        }

        private void ApplyInterops()
        {
            foreach(TrackDetour detour in m_detours.Values)
            {
                ApplyInterop( detour );
            }
        }

        private void ApplyInterop( TrackDetour detour )
        {
            if(detour.m_active == false)
            {
                if(CanAccess( detour.m_pc, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH ))
                {
                    TimingState state = new TimingState();

                    SuspendTimingUpdates( ref state );

                    uint mem = Load( detour.m_pc, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );

                    if(mem != TrackDetour.c_DetourOpcode)
                    {
                        detour.m_op = mem;

                        Store( detour.m_pc, TrackDetour.c_DetourOpcode, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );

                        detour.m_active = true;
                    }

                    ResumeTimingUpdates( ref state );
                }
            }
        }

        private Hosting.Interop.CallbackResponse ProcessDetour( TrackDetour                detour          ,
                                                                Hosting.Interop.Callback[] calls           ,
                                                                bool                       fPostProcessing )
        {
            foreach(Hosting.Interop.Callback ftn in calls)
            {
                var res = ftn();

                if((res & Hosting.Interop.CallbackResponse.RemoveDetour) != 0)
                {
                    res &= ~Hosting.Interop.CallbackResponse.RemoveDetour;

                    RemoveInterop( new Hosting.Interop.Registration( detour.m_pc, fPostProcessing, ftn ) );
                }

                if(res != Hosting.Interop.CallbackResponse.DoNothing)
                {
                    return res;
                }
            }

            return Hosting.Interop.CallbackResponse.DoNothing;
        }

        private void RemoveDetour( TrackDetour detour )
        {
            m_detours.Remove( detour.m_pc );

            if(detour.m_op != TrackDetour.c_DetourOpcode)
            {
                Store( detour.m_pc, detour.m_op, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );
            }
        }

        public uint HandleDetour( uint address ,
                                  uint value   )
        {
            TrackDetour detour;

            if(m_detours.TryGetValue( address, out detour ))
            {
                if(detour.m_active)
                {
                    return detour.m_op;
                }
            }

            return value;
        }

        public bool UpdateDetour( uint address  ,
                                  uint newValue )
        {
            TrackDetour detour;

            if(m_detours.TryGetValue( address, out detour ))
            {
                if(detour.m_active)
                {
                    detour.m_op = newValue;
                    return true;
                }
            }

            return false;
        }

        //--//

        public void RemoveInterop( Hosting.Interop.Registration reg )
        {
            TrackDetour detour;

            if(m_detours.TryGetValue( reg.Address, out detour ))
            {
                Hosting.Interop.Callback[] calls = reg.IsPostProcessing ? detour.m_callsPost : detour.m_callsPre;
                     
                calls = ArrayUtility.RemoveUniqueFromNotNullArray( calls, reg.Target );

                if(reg.IsPostProcessing)
                {
                    detour.m_callsPost = calls;
                }
                else
                {
                    detour.m_callsPre = calls;
                }

                if(detour.m_callsPre .Length == 0 &&
                   detour.m_callsPost.Length == 0  )
                {
                    RemoveDetour( detour );
                }
            }
        }

        public void SetInterop( string                   name            ,
                                Hosting.Interop.Callback ftn             ,
                                bool                     fPostProcessing )
        {
            uint pc;

            if(m_symdef.TryGetValue( name, out pc ))
            {
                SetInterop( pc, ftn, false, fPostProcessing );
            }
        }

        public Hosting.Interop.Registration SetInterop( uint                     pc              ,
                                                        Hosting.Interop.Callback ftn             ,
                                                        bool                     fHead           ,
                                                        bool                     fPostProcessing )
        {
            TrackDetour detour;

            if(m_detours.TryGetValue( pc, out detour ) == false)
            {
                detour = new TrackDetour( pc );

                m_detours[pc] = detour;

                ApplyInterop( detour );
            }

            Hosting.Interop.Callback[] calls = fPostProcessing ? detour.m_callsPost : detour.m_callsPre;

            if(ArrayUtility.FindInNotNullArray( calls, ftn ) < 0)
            {
                if(fHead)
                {
                    calls = ArrayUtility.InsertAtHeadOfNotNullArray( calls, ftn );
                }
                else
                {
                    calls = ArrayUtility.AppendToNotNullArray( calls, ftn );
                }

                if(fPostProcessing)
                {
                    detour.m_callsPost = calls;
                }
                else
                {
                    detour.m_callsPre = calls;
                }
            }

            return new Hosting.Interop.Registration( pc, fPostProcessing, ftn );
        }

        public string GetContext()
        {
            uint context;

            if(GetContext( m_pc, out context ))
            {
                return m_symdef_Inverse[context];
            }
            else
            {
                return "??";
            }
        }

        public bool GetContext(     uint address ,
                                out uint context )
        {
            SymDef.AddressToSymbolMap map;
            uint                      closestAddress;

            context = 0;

            if(m_symdef_Inverse_PhysicalAddress != null)
            {
                map     = m_symdef_Inverse_PhysicalAddress;
                address = GetPhysicalAddress( address );
            }
            else
            {
                map = m_symdef_Inverse;
            }

            if(map.FindClosestAddress( address, out closestAddress ))
            {
                string symbol = map[closestAddress];

                return m_symdef.TryGetValue( symbol, out context );
            }

            return false;
        }

        //--//--//

        private void SniffForCallPush()
        {
            uint   pc = m_pc;
            uint   context;

            if(GetContext( pc, out context ) && pc == context)
            {
                List< TrackCall > callQueue = m_currentBank.m_callQueue;
                int               pos       = callQueue.Count;
                uint              sp        = GetRegister( EncDef.c_register_sp );
                string            name      = m_symdef_Inverse[ context ];

                while(true)
                {
                    if(name == "NoClearZI_ER_RAM") break;
                    if(name == "ClearZI_ER_RAM"  ) break;
                    if(name == "ARM_Vectors"     ) break;
                    if(name == "IRQ_VECTOR"      ) break;

                    TrackCall tc = new TrackCall();

                    tc.m_pc    = pc;
                    tc.m_lr    = GetRegister( EncDef.c_register_lr );
                    tc.m_sp    = sp;
                    tc.m_name  = name;

                    tc.m_te.Start( this );

                    callQueue.Add( tc );

                    if(m_execution_OutputSink != null)
                    {
                        m_execution_OutputSink.OutputLine( "{0}>>>> CALL {1} {2}", new string( ' ', pos ), tc.m_name, m_clockTicks );
                    }
                    break;
                }
            }
        }

        private void SniffForCallPop( uint psrFrom ,
                                      uint psrTo   )
        {
            List< TrackCall > callQueue = m_currentBank.m_callQueue;
            int               pos       = callQueue.Count;
            uint              pc        = m_pc;
            uint              sp        = GetRegister( EncDef.c_register_sp );

            for(int pos2 = pos; pos2-- > 0; )
            {
                TrackCall tc = callQueue[pos2];

                if(tc.m_lr == pc && tc.m_sp == sp)
                {
                }
                else if(tc.m_sp != 0 && tc.m_sp < sp)
                {
                }
                else if(tc.m_sp == sp && psrFrom != psrTo    && 
                        (psrFrom == EncDef.c_psr_mode_IRQ ||
                         psrFrom == EncDef.c_psr_mode_FIQ  )  )
                {
                }
                else
                {
                    return;
                }

                while(pos-- > pos2)
                {
                    TrackCall tc2 = callQueue[pos];

                    tc2.m_te.End( this );

                    if(m_execution_OutputSink != null)
                    {
                        m_execution_OutputSink.OutputLine( "{0}<<<< CALL {1} ({2}) {3}", new string( ' ', pos ), tc2.m_name, tc2.m_te.ToString(), m_clockTicks );
                    }

                    callQueue.RemoveAt( pos );
                }

                pos++;
            }
        }

        //--//--//

        public Hosting.Interop.CallbackResponse Interop_GenericSkipCall()
        {
            //
            // Force return from function.
            //
            m_pc = GetRegister( EncDef.c_register_lr );

            m_implProcessorStatus.RaiseExternalProgramFlowChange();

            return Hosting.Interop.CallbackResponse.NextInstruction;
        }

        //--//

        //
        // Access Methods
        //

        public event TrackLoad NotifyOnLoad
        {
            add
            {
                m_eventTrackLoad += value;
            }

            remove
            {
                m_eventTrackLoad -= value;
            }
        }

        public event TrackStore NotifyOnStore
        {
            add
            {
                m_eventTrackStore += value;
            }

            remove
            {
                m_eventTrackStore -= value;
            }
        }
    }
}
