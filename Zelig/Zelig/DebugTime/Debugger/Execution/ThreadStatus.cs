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

    using EncDef             = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using InstructionSet     = Microsoft.Zelig.TargetModel.ArmProcessor.InstructionSet;
    using IR                 = Microsoft.Zelig.CodeGeneration.IR;
    using RT                 = Microsoft.Zelig.Runtime;
    using TS                 = Microsoft.Zelig.Runtime.TypeSystem;


    public class ThreadStatus
    {
        public enum Kind
        {
            Bootstrap          ,
            ApplicationThread  ,
            IdleThread         ,
            InterruptThread    ,
            FastInterruptThread,
        }

        //
        // State
        //

        public uint                            ProgramCounter;
        public uint                            StackPointer;

        public List< StackFrame >              StackTrace = new List< StackFrame >();
        public StackFrame                      StackFrame;

        public int                             ManagedThreadId = -1;
        public ImageInformation.PointerContext ThreadObject;
        public Kind                            ThreadKind;
        public TS.MethodRepresentation         TopMethod;

        public PerformanceCounter              ActiveTime;

        //
        // Constructor Methods
        //

        private ThreadStatus( MemoryDelta                    memDelta    ,
                              Emulation.Hosting.Breakpoint[] breakpoints )
        {
            Emulation.Hosting.ProcessorStatus svc; memDelta.Host.GetHostingService( out svc );

            //
            // Get local copy of register state.
            //
            var regs = new RegisterContext( memDelta.ImageInformation.TypeSystem );

            foreach(var reg in regs.Keys)
            {
                regs.SetValue( reg, svc.GetRegister( reg ) );
            }

            //
            // Set the processor status property, so that register context updates are reflected in the actual processor state.
            //
            regs.ProcessorStatus = svc;

            FetchStackFrace( memDelta, breakpoints, regs );

            var sf = this.TopStackFrame;
            if(sf != null)
            {
                this.TopMethod = sf.Method;
            }
        }

        private ThreadStatus( MemoryDelta                     memDelta         ,
                              Emulation.Hosting.Breakpoint[]  breakpoints      ,
                              ImageInformation.PointerContext registersContext )
        {
            //
            // Get local copy of register state.
            //
            var regs = new RegisterContext( memDelta.ImageInformation.TypeSystem );

            foreach(var reg in regs.Keys)
            {
                string name;
                
                switch(reg.Encoding)
                {
                    case EncDef.c_register_r0  : name = "R0"  ; break;
                    case EncDef.c_register_r1  : name = "R1"  ; break;
                    case EncDef.c_register_r2  : name = "R2"  ; break;
                    case EncDef.c_register_r3  : name = "R3"  ; break;
                    case EncDef.c_register_r4  : name = "R4"  ; break;
                    case EncDef.c_register_r5  : name = "R5"  ; break;
                    case EncDef.c_register_r6  : name = "R6"  ; break;
                    case EncDef.c_register_r7  : name = "R7"  ; break;
                    case EncDef.c_register_r8  : name = "R8"  ; break;
                    case EncDef.c_register_r9  : name = "R9"  ; break;
                    case EncDef.c_register_r10 : name = "R10" ; break;
                    case EncDef.c_register_r11 : name = "R11" ; break;
                    case EncDef.c_register_r12 : name = "R12" ; break;
                    case EncDef.c_register_r13 : name = "SP"  ; break;
                    case EncDef.c_register_r14 : name = "LR"  ; break;
                    case EncDef.c_register_r15 : name = "PC"  ; break;
                    case EncDef.c_register_cpsr: name = "CPSR"; break;

                    default: name = null ; break;
                }

                if(name != null)
                {
                    ImageInformation.PointerContext pc = registersContext.AccessField( name );

                    if(pc != null && pc.Value is uint)
                    {
                        regs.SetLocationInMemory( reg, memDelta, pc.Address );
                    }
                }
            }

            FetchStackFrace( memDelta, breakpoints, regs );
        }

        //
        // Helper Methods
        //

        private void FetchStackFrace( MemoryDelta                    memDelta    ,
                                      Emulation.Hosting.Breakpoint[] breakpoints ,
                                      RegisterContext                regs        )
        {
            this.ProgramCounter = regs.ProgramCounter;
            this.StackPointer   = regs.StackPointer;

            memDelta.ImageInformation.FetchCurrentStackTrace( this.StackTrace, memDelta, breakpoints, regs );

            this.StackFrame = this.TopStackFrame;

            foreach(StackFrame sf in this.StackTrace)
            {
                sf.Thread = this;
            }
        }

        public static ThreadStatus GetCurrent( MemoryDelta memDelta )
        {
            return new ThreadStatus( memDelta, null );
        }

        public static ThreadStatus Analyze( List< ThreadStatus >           lst         ,
                                            MemoryDelta                    memDelta    ,
                                            Emulation.Hosting.Breakpoint[] breakpoints )
        {
            var imageInformation = memDelta.ImageInformation;
            var tsActive         = new ThreadStatus( memDelta, breakpoints );

            tsActive.ThreadKind = Kind.Bootstrap;

            lst.Clear();

            IR.TypeSystemForCodeTransformation typeSystem = imageInformation.TypeSystem;
            if(typeSystem != null)
            {
                ImageInformation.PointerContext pcThreadManager = imageInformation.ResolveSingleton( memDelta, typeSystem.WellKnownTypes.Microsoft_Zelig_Runtime_ThreadManager );

                if(pcThreadManager != null)
                {
                    ImageInformation.PointerContext pcAllThreads          = pcThreadManager.AccessField( "m_allThreads"          );
                    ImageInformation.PointerContext pcRunningThread       = pcThreadManager.AccessField( "m_runningThread"       );
                    ImageInformation.PointerContext pcIdleThread          = pcThreadManager.AccessField( "m_idleThread"          );
                    ImageInformation.PointerContext pcInterruptThread     = pcThreadManager.AccessField( "m_interruptThread"     );
                    ImageInformation.PointerContext pcFastInterruptThread = pcThreadManager.AccessField( "m_fastInterruptThread" );

                    PerformanceCounter deadTime = new PerformanceCounter(); deadTime.Fetch( pcThreadManager.AccessField( "m_deadThreadsTime" ) );

                    if(pcAllThreads != null)
                    {
                        ImageInformation.PointerContext pcHead = pcAllThreads.AccessField( "m_head" );
                        ImageInformation.PointerContext pcTail = pcAllThreads.AccessField( "m_tail" );

                        if(pcHead != null)
                        {
                            ImageInformation.PointerContext pcNode = pcHead.AccessField( "m_next" );

                            var regCtx = tsActive.TopStackFrame.RegisterContext;

                            uint cpsr = regCtx.GetValueAsUInt( regCtx.GetRegisterDescriptor( EncDef.c_register_cpsr ) );
                            switch(cpsr & EncDef.c_psr_mode)
                            {
                                case EncDef.c_psr_mode_IRQ:
                                    pcRunningThread = pcInterruptThread;
                                    break;

                                case EncDef.c_psr_mode_FIQ:
                                    pcRunningThread = pcFastInterruptThread;
                                    break;
                            }

                            while(pcNode != null && pcNode != pcTail)
                            {
                                ImageInformation.PointerContext pcThread = pcNode.AccessField( "m_target" );

                                if(pcThread != null)
                                {
                                    ThreadStatus ts = null;

                                    if(pcThread == pcRunningThread)
                                    {
                                        ts = tsActive;
                                    }
                                    else
                                    {
                                        ImageInformation.PointerContext pcContext = pcThread.AccessField( "m_swappedOutContext" );
                                        if(pcContext != null)
                                        {
                                            ImageInformation.PointerContext pcRegisters = pcContext.AccessField( "Registers" );
                                            if(pcRegisters != null)
                                            {
                                                ts = new ThreadStatus( memDelta, null, pcRegisters );
                                            }
                                        }
                                    }

                                    if(ts != null)
                                    {
                                        lst.Add( ts );

                                        ts.ThreadObject = pcThread;

                                        var pcManagedThreadId = pcThread.AccessField( "m_managedThreadId" );
                                        if(pcManagedThreadId != null)
                                        {
                                            var val = pcManagedThreadId.Value;

                                            if(val is int)
                                            {
                                                ts.ManagedThreadId = (int)val;
                                            }
                                        }

                                        ts.ActiveTime.Fetch( pcThread.AccessField( "m_activeTime" ) );

                                        TS.CodeMap codeMap = imageInformation.ResolveAddressToCodeMap( ts.ProgramCounter );
                                        if(codeMap != null)
                                        {
                                            ts.TopMethod = codeMap.Target;
                                        }

                                        if(pcThread == pcInterruptThread)
                                        {
                                            ts.ThreadKind = Kind.InterruptThread;
                                        }
                                        else if(pcThread == pcFastInterruptThread)
                                        {
                                            ts.ThreadKind = Kind.FastInterruptThread;
                                        }
                                        else if(pcThread == pcIdleThread)
                                        {
                                            ts.ThreadKind = Kind.IdleThread;
                                        }
                                        else
                                        {
                                            ts.ThreadKind = Kind.ApplicationThread;
                                        }
                                    }
                                }

                                pcNode = pcNode.AccessField( "m_next" );
                            }
                        }
                    }
                }
            }

            if(lst.Count == 0)
            {
                lst.Add( tsActive );
            }

            return tsActive;
        }

        //
        // Access Methods
        //

        public StackFrame TopStackFrame
        {
            get
            {
                if(this.StackTrace.Count > 0)
                {
                    return this.StackTrace[0];
                }

                return null;
            }
        }

        public StackFrame PreviousStackFrame
        {
            get
            {
                StackFrame currentStackFrame = this.StackFrame;

                if(currentStackFrame != null)
                {
                    foreach(StackFrame sf in this.StackTrace)
                    {
                        if(sf.Depth == currentStackFrame.Depth + 1)
                        {
                            return sf;
                        }
                    }
                }

                return null;
            }
        }

        //
        // Debug Methods
        //

        public override string ToString()
        {
            StringBuilder sb     = new StringBuilder();
            bool          fFirst = true;

            foreach(StackFrame sf in this.StackTrace)
            {
                if(fFirst)
                {
                    fFirst = false;
                }
                else
                {
                    sb.Append( Environment.NewLine );
                }

                sb.Append( sf );
            }

            return sb.ToString();
        }
    }
}