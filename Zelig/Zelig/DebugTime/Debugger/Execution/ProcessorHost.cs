//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Debugger.ArmProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading;
    using System.Windows.Forms;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.TargetModel.ArmProcessor;
    using Microsoft.Zelig.CodeGeneration.IR.Abstractions;

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using Cfg                = Microsoft.Zelig.Configuration.Environment;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;


    public class ProcessorHost : Emulation.Hosting.AbstractHost
    {
        class MemoryDeltaHolder : IDisposable
        {
            //
            // State
            //

            ProcessorHost m_host;

            //
            // Constructor Methods
            //

            internal MemoryDeltaHolder( ProcessorHost host )
            {
                m_host = host;

                lock(m_host)
                {
                    foreach(var memDelta in m_host.m_notifyOnEnteringExecuting.ToArray())
                    {
                        memDelta.EnteringExecuting();
                    }
                }
            }

            //
            // Helper Methods
            //

            public void Dispose()
            {
                lock(m_host)
                {
                    foreach(var memDelta in m_host.m_notifyOnExitingRunning.ToArray())
                    {
                        memDelta.ExitingRunning();
                    }
                }
            }
        }

        class SimulatorControlImpl : Emulation.Hosting.SimulatorControl
        {
            //
            // State
            //

            ProcessorHost m_host;

            //
            // Constructor Methods
            //

            internal SimulatorControlImpl( ProcessorHost host )
            {
                m_host = host;

                m_host.RegisterService( typeof(Emulation.Hosting.SimulatorControl), this );
            }

            //
            // Helper Methods
            //

            public override void Wait( TimeSpan tm )
            {
                double span = tm.TotalMilliseconds;
                int    val  = (span > 0 && span < int.MaxValue) ? (int)span : int.MaxValue;
               
                if(val > 100)
                {
                    val = 100;
                }

                m_host.m_implHalEvents.m_systemEvent.WaitOne( val, false );
            }
        }

        //--//

        class HalButtonsImpl : Emulation.Hosting.HalButtons
        {
            struct ButtonRecord
            {
                //
                // State
                //

                internal uint m_buttonsPressed;
                internal uint m_buttonsReleased;
            }

            //
            // State
            //

            ProcessorHost       m_host;
                            
            Queue<ButtonRecord> m_buttonQueue = new Queue<ButtonRecord>();

            //
            // Constructor Methods
            //

            internal HalButtonsImpl( ProcessorHost host )
            {
                m_host = host;

                m_host.RegisterService( typeof(Emulation.Hosting.HalButtons), this );
            }

            //
            // Helper Methods
            //

            public override bool GetNextStateChange( out uint buttonsPressed  ,
                                                     out uint buttonsReleased )
            {
                lock(this)
                {
                    if(m_buttonQueue.Count > 0)
                    {
                        ButtonRecord br = m_buttonQueue.Dequeue();

                        buttonsPressed  = br.m_buttonsPressed;
                        buttonsReleased = br.m_buttonsReleased;

                        return true;
                    }

                    buttonsPressed  = 0;
                    buttonsReleased = 0;

                    return false;
                }
            }

            public override void QueueNextStateChange( uint buttonsPressed  ,
                                                       uint buttonsReleased )
            {
                lock(this)
                {
                    ButtonRecord br;

                    br.m_buttonsPressed  = buttonsPressed;
                    br.m_buttonsReleased = buttonsReleased;

                    m_buttonQueue.Enqueue( br );

                    m_host.m_implHalEvents.Set( Emulation.Hosting.HalEvents.SYSTEM_EVENT_FLAG_BUTTON );
                }
            }
        }

        //--//

        class HalEventsImpl : Emulation.Hosting.HalEvents
        {
            //
            // State
            //

            private  ProcessorHost  m_host;

            internal AutoResetEvent m_systemEvent = new AutoResetEvent( false );
            internal uint           m_systemFlags = 0;

            //
            // Constructor Methods
            //

            internal HalEventsImpl( ProcessorHost host )
            {
                m_host = host;

                m_host.RegisterService( typeof(Emulation.Hosting.HalEvents), this );
            }

            //
            // Helper Methods
            //

            public override void Clear( uint mask )
            {
                lock(this)
                {
                    m_systemFlags &= ~mask;
                }
            }

            public override void Set( uint mask )
            {
                lock(this)
                {
                    m_systemFlags |= mask;

                    m_systemEvent.Set();
                }
            }

            public override uint Get( uint mask )
            {
                lock(this)
                {
                    uint res = m_systemFlags & mask;

                    m_systemFlags &= ~mask;

                    return res;
                }
            }

            public override uint MaskedRead( uint mask )
            {
                lock(this)
                {
                    return m_systemFlags & mask;
                }
            }
        }

        //--//

        //
        // State
        //

        DebuggerMainForm                                   m_owner;
                                            
        Emulation.Hosting.AbstractEngine                   m_activeEngine;
        Dictionary<Type, Emulation.Hosting.AbstractEngine> m_engines;

        List< Emulation.Hosting.Breakpoint >               m_breakpoints;
                                            
        SimulatorControlImpl                               m_implSimulatorControl;
        HalButtonsImpl                                     m_implHalButtons;
        HalEventsImpl                                      m_implHalEvents;

        MemoryDelta                                        m_memoryDelta;
        List< MemoryDelta >                                m_notifyOnEnteringExecuting;
        List< MemoryDelta >                                m_notifyOnExitingRunning;

        uint                                               m_softBreakpointTableAddress;

        //
        // Constructor Methods
        //

        public ProcessorHost( DebuggerMainForm owner )
        {
            this.RegisterService( typeof(Emulation.Hosting.AbstractHost), this );
            this.RegisterService( typeof(ProcessorHost                 ), this );

            //--//

            m_owner                     = owner;

            m_engines                   = new Dictionary< Type, Emulation.Hosting.AbstractEngine >();

            m_breakpoints               = new List< Emulation.Hosting.Breakpoint >();

            m_implSimulatorControl      = new SimulatorControlImpl( this );
            m_implHalButtons            = new HalButtonsImpl      ( this );
            m_implHalEvents             = new HalEventsImpl       ( this );

            m_notifyOnEnteringExecuting = new List< MemoryDelta >();
            m_notifyOnExitingRunning    = new List< MemoryDelta >();
        }

        public void SelectEngine(Cfg.EngineCategory category, InstructionSet iset)
        {
            m_softBreakpointTableAddress = 0;

            this.Unlink( m_activeEngine );

            bool match = false;
            if(m_engines.TryGetValue(category.GetType(), out m_activeEngine) == true)
            {
                if(m_activeEngine.InstructionSet == iset)
                {
                    match = true;
                }
                else
                {
                    m_engines.Remove(category.GetType());
                }
            }

            if(!match)
            {
                m_activeEngine = category.Instantiate(iset) as Emulation.Hosting.AbstractEngine;
                if(m_activeEngine == null)
                {
                    throw TypeConsistencyErrorException.Create("Unrecognized engine: {0}", category);
                }

                m_engines[category.GetType()] = m_activeEngine;
            }

            this.Link( m_activeEngine );

            //--//

            m_breakpoints.Clear();
        }

        //
        // Helper Methods
        //

        internal bool GetAbsoluteTime( out ulong clockTicks  ,
                                       out ulong nanoseconds )
        {
            Emulation.Hosting.DeviceClockTicksTracking svc; GetHostingService( out svc );

            if(svc == null)
            {
                clockTicks  = 0;
                nanoseconds = 0;
                return false;
            }

            return svc.GetAbsoluteTime( out clockTicks, out nanoseconds );
        }

        public void Execute( ImageInformation    imageInformation ,
                             Cfg.ProductCategory product          )
        {
            ExecuteInner( imageInformation, product, false );
        }

        public void ExecuteStep( ImageInformation    imageInformation ,
                                 Cfg.ProductCategory product          )
        {
            ExecuteInner( imageInformation, product, true );
        }

        void ExecuteInner( ImageInformation    imageInformation ,
                           Cfg.ProductCategory product          ,
                           bool                fSingleStep      )
        {
            Emulation.Hosting.ProcessorStatus  svcPS;   this.GetHostingService( out svcPS   );
            Emulation.Hosting.ProcessorControl svcPC;   this.GetHostingService( out svcPC   );
            Emulation.Hosting.MemoryProvider   svcMP;   this.GetHostingService( out svcMP   );
            Emulation.Hosting.JTagConnector    svcJTAG; this.GetHostingService( out svcJTAG );

            var lst = new List< Emulation.Hosting.Breakpoint >();

            object softBreakpointOpcode;
            int    maxHardBreakpoints;

            svcPC.GetBreakpointCapabilities( out softBreakpointOpcode, out maxHardBreakpoints );

            var cleanup = HashTableFactory.New< uint, uint >();

            while(true)
            {
                lst.Clear();

                uint pc    = svcPS.ProgramCounter;
                bool fStep = fSingleStep;

                foreach(var bp in m_breakpoints)
                {
                    fStep |= bp.ShouldStopOverStatement( pc );

                    bp.SetAs = Emulation.Hosting.Breakpoint.Status.NotSet;
                }

                cleanup.Clear();

                for(int pass = 0; pass < 3; pass++)
                {
                    bool fForceHardware   = (pass == 0);
                    bool fIncludeOptional = (pass == 2);
        
                    foreach(var bp in m_breakpoints)
                    {
                        if(bp.IsActive                                      &&
                           bp.ShouldIgnoreOnce          == false            &&
                           bp.ShouldImplementInHardware == fForceHardware   &&
                           bp.IsOptional                == fIncludeOptional  )
                        {
                            uint address = bp.Address;
        
                            if(bp.ShouldImplementInHardware == false)
                            {
                                if(product != null && softBreakpointOpcode is uint)
                                {
                                    var mem = product.FindMemory( address );
                                    if(mem != null && mem.IsRAM)
                                    {
                                        if(cleanup.ContainsKey( address ) == false)
                                        {
                                            uint val;

                                            if(svcMP.GetUInt32( address, out val ))
                                            {
                                                uint expectedVal;

                                                if(imageInformation.TryReadUInt32FromPhysicalImage( address, out expectedVal ) && expectedVal == val)
                                                {
                                                    cleanup[address] = val;

                                                    svcMP.SetUInt32( address, (uint)softBreakpointOpcode );

                                                    bp.SetAs = Emulation.Hosting.Breakpoint.Status.SoftBreakpoint;
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if(lst.Count < maxHardBreakpoints)
                            {
                                lst.Add( bp );

                                bp.SetAs = Emulation.Hosting.Breakpoint.Status.HardBreakpoint;
                            }
                        }
                    }
                }

                if(cleanup.Count > 0 && svcJTAG != null)
                {
                    PublishSoftBreakpoints( imageInformation, product, svcMP, svcJTAG, cleanup );

                    FlushCache( imageInformation, product, svcMP, svcJTAG );
                }

                //--//

                svcPC.StopExecution = false;

                if(fStep)
                {
                    svcPC.ExecuteStep( lst );
                }
                else
                {
                    svcPC.Execute( lst );
                }

                if(cleanup.Count > 0)
                {
                    FlushSoftBreakpoints( svcMP );

                    foreach(uint address in cleanup.Keys)
                    {
                        svcMP.SetUInt32( address, cleanup[address] );
                    }
                }

                pc = svcPS.ProgramCounter;

                Emulation.Hosting.Breakpoint.Response res = Emulation.Hosting.Breakpoint.Response.DoNothing;
                
                foreach(var bp in m_breakpoints.ToArray())
                {
                    if(bp.IsActive                  &&
                       bp.ShouldIgnoreOnce == false  )
                    {
                        if(bp.Address == pc)
                        {
                            res |= bp.Hit();
                        }
                    }

                    bp.ClearIgnoreFlag();
                }

                if(fSingleStep)
                {
                    break;
                }

                if((res & Emulation.Hosting.Breakpoint.Response.StopExecution) != 0)
                {
                    break;
                }

                if((res & Emulation.Hosting.Breakpoint.Response.NextInstruction) != 0)
                {
                    continue;
                }

                if(fStep)
                {
                    continue;
                }

                break;
            }

            RemoveTemporaryBreakpoints();
        }

        private void PublishSoftBreakpoints( ImageInformation                 imageInformation ,
                                             Cfg.ProductCategory              product          ,
                                             Emulation.Hosting.MemoryProvider svcMP            ,
                                             Emulation.Hosting.JTagConnector  svcJTAG          ,
                                             GrowOnlyHashTable< uint, uint >  cleanup          )
        {
            if(m_softBreakpointTableAddress == 0)
            {
                var md = imageInformation.TypeSystem.TryGetHandler( Runtime.DebuggerHook.GetSoftBreakpointTable );
                if(md != null)
                {
                    var reg = imageInformation.ResolveMethodToRegion( md );
                    if(reg != null)
                    {
                        var mem = product.FindAnyBootstrapRAM();
                        if(mem != null)
                        {
                            uint   blockEnd   = mem.EndAddress;
                            uint   blockStart = blockEnd - 128;
                            byte[] oldState;
        
                            svcMP.GetBlock( blockStart, 128, 4, out oldState );
        
                            var input = new []
                            {
                                new Emulation.Hosting.JTagConnector.RegisterSet { Name = "CPSR"   , Value = EncDef.c_psr_I | EncDef.c_psr_F | EncDef.c_psr_mode_SVC },
                                new Emulation.Hosting.JTagConnector.RegisterSet { Name = "PC"     , Value = reg.ExternalAddress                                                                         },
                                new Emulation.Hosting.JTagConnector.RegisterSet { Name = "Svc_R13", Value = blockEnd                                                                                    },
                            };

                            var output = new []
                            {
                                new Emulation.Hosting.JTagConnector.RegisterSet { Name = "R0" },
                            };

                            svcJTAG.ExecuteCode( 1000, input, output );
        
                            svcMP.SetBlock( blockStart, oldState, 4 );

                            if(output[0].Value is uint)
                            {
                                m_softBreakpointTableAddress = (uint)output[0].Value;
                            }
                        }                        
                    }
                }
            }

            if(m_softBreakpointTableAddress != 0)
            {
                uint len;

                if(svcMP.GetUInt32( m_softBreakpointTableAddress, out len ))
                {
                    uint pos = 0;

                    foreach(var address in cleanup.Keys)
                    {
                        if(pos < len)
                        {
                            svcMP.SetUInt32( m_softBreakpointTableAddress + sizeof(uint) + pos * 8    , address          );
                            svcMP.SetUInt32( m_softBreakpointTableAddress + sizeof(uint) + pos * 8 + 4, cleanup[address] );
                        }

                        pos++;
                    }
                }
            }
        }

        private void FlushSoftBreakpoints( Emulation.Hosting.MemoryProvider svcMP )
        {
            if(m_softBreakpointTableAddress != 0)
            {
                uint len;

                if(svcMP.GetUInt32( m_softBreakpointTableAddress, out len ))
                {
                    if(len > 0)
                    {
                        svcMP.SetUInt32( m_softBreakpointTableAddress + sizeof(uint), 0 );
                    }
                }
            }
        }

        private static void FlushCache( ImageInformation                 imageInformation ,
                                        Cfg.ProductCategory              product          ,
                                        Emulation.Hosting.MemoryProvider svcMP            ,
                                        Emulation.Hosting.JTagConnector  svcJTAG          )
        {
            var md = imageInformation.TypeSystem.TryGetHandler( Runtime.DebuggerHook.FlushInstructionCache );
            if(md != null)
            {
                var reg = imageInformation.ResolveMethodToRegion( md );
                if(reg != null)
                {
                    var mem = product.FindAnyBootstrapRAM();
                    if(mem != null)
                    {
                        uint   blockEnd   = mem.EndAddress;
                        uint   blockStart = blockEnd - 128;
                        byte[] oldState;
    
                        svcMP.GetBlock( blockStart, 128, 4, out oldState );
    
                        var input = new []
                        {
                            new Emulation.Hosting.JTagConnector.RegisterSet { Name = "CPSR"   , Value = EncDef.c_psr_I | EncDef.c_psr_F | EncDef.c_psr_mode_SVC },
                            new Emulation.Hosting.JTagConnector.RegisterSet { Name = "PC"     , Value = reg.ExternalAddress                                                                         },
                            new Emulation.Hosting.JTagConnector.RegisterSet { Name = "Svc_R13", Value = blockEnd                                                                                    },
                        };

                        svcJTAG.ExecuteCode( 1000, input, null );
    
                        svcMP.SetBlock( blockStart, oldState, 4 );
                    }                        
                }
            }
        }

        //--//

        internal void RegisterForNotification( MemoryDelta memDelta  ,
                                               bool        fEntering ,
                                               bool        fExiting  )
        {
            if(fEntering)
            {
                if(m_notifyOnEnteringExecuting.Contains( memDelta ) == false)
                {
                    m_notifyOnEnteringExecuting.Add( memDelta );
                }
            }

            if(fExiting)
            {
                if(m_notifyOnExitingRunning.Contains( memDelta ) == false)
                {
                    m_notifyOnExitingRunning.Add( memDelta );
                }
            }
        }

        internal void UnregisterForNotification( MemoryDelta memDelta  ,
                                                 bool        fEntering ,
                                                 bool        fExiting  )
        {
            if(fEntering)
            {
                m_notifyOnEnteringExecuting.Remove( memDelta );
            }

            if(fExiting)
            {
                m_notifyOnExitingRunning.Remove( memDelta );
            }
        }

        public IDisposable SuspendMemoryDeltaUpdates()
        {
            return new MemoryDeltaHolder( this );
        }

        //--//

        private void RemoveTemporaryBreakpoints()
        {
            for(int pos = m_breakpoints.Count; --pos >= 0; )
            {
                Emulation.Hosting.Breakpoint bp = m_breakpoints[pos];

                if(bp.IsTemporary)
                {
                    m_breakpoints.RemoveAt( pos );
                }
            }
        }

        public Emulation.Hosting.Breakpoint CreateBreakpoint( uint                                  address ,
                                                              Debugging.DebugInfo                   di      ,
                                                              Emulation.Hosting.Breakpoint.Callback target  )
        {
            foreach(var bp in m_breakpoints)
            {
                if(bp.Address   == address &&
                   bp.DebugInfo == di      &&
                   bp.Target    == target   )
                {
                    return bp;
                }
            }

            var bpNew = new Emulation.Hosting.Breakpoint( address, di, target );

            m_breakpoints.Add( bpNew );

            return bpNew;
        }

        public void RestoreBreakpoint( Emulation.Hosting.Breakpoint bp )
        {
            m_breakpoints.Remove( bp );
            m_breakpoints.Add   ( bp );
        }

        public void RemoveBreakpoint( Emulation.Hosting.Breakpoint bp )
        {
            m_breakpoints.Remove( bp );
        }

        //
        // Access Methods
        //

        public Emulation.Hosting.Breakpoint[] Breakpoints
        {
            get
            {
                return m_breakpoints.ToArray();
            }
        }

        public MemoryDelta MemoryDelta
        {
            get
            {
                lock(this)
                {
                    if(m_memoryDelta == null)
                    {
                        m_memoryDelta = new MemoryDelta( m_owner.ImageInformation, this );

                        RegisterForNotification( m_memoryDelta, true, true );
                    }
                    else
                    {
                        m_memoryDelta.Synchronize( m_owner.ImageInformation, this );
                    }
                }

                return m_memoryDelta;
            }
        }
    }
}
