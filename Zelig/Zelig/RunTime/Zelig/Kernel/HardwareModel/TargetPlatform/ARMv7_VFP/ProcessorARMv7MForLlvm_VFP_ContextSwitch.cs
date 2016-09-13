//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define DEBUG_CTX_SWITCH

using System.Runtime.InteropServices;

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;

    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using TS     = Microsoft.Zelig.Runtime.TypeSystem;
    using RT     = Microsoft.Zelig.Runtime;


    public abstract partial class ProcessorARMv7MForLlvm_VFP
    {
        //--//

        //
        // Part of Context may be defined in the model for the targeted sub-system, e.g. Mbed or CMSIS-Core for ARM processors
        //

        public abstract unsafe new class Context : Processor.Context
        {
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct SoftwareFrame
            {
                // SW stack frame: pushed by PendSV_Handler
                // SW stack frame 
                [TS.AssumeReferenced] public uint    EXC_RETURN;
                [TS.AssumeReferenced] public uint    CONTROL;
                [TS.AssumeReferenced] public UIntPtr R4;
                [TS.AssumeReferenced] public UIntPtr R5;
                [TS.AssumeReferenced] public UIntPtr R6;
                [TS.AssumeReferenced] public UIntPtr R7;
                [TS.AssumeReferenced] public UIntPtr R8;
                [TS.AssumeReferenced] public UIntPtr R9;
                [TS.AssumeReferenced] public UIntPtr R10;
                [TS.AssumeReferenced] public UIntPtr R11;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct SoftwareFloatingPointFrame
            {
                // SW stack frame for FP               
                [TS.AssumeReferenced] public float   S16;
                [TS.AssumeReferenced] public float   S17;
                [TS.AssumeReferenced] public float   S18;
                [TS.AssumeReferenced] public float   S19;
                [TS.AssumeReferenced] public float   S20;
                [TS.AssumeReferenced] public float   S21;
                [TS.AssumeReferenced] public float   S22;
                [TS.AssumeReferenced] public float   S23;
                [TS.AssumeReferenced] public float   S24;
                [TS.AssumeReferenced] public float   S25;
                [TS.AssumeReferenced] public float   S26;
                [TS.AssumeReferenced] public float   S27;
                [TS.AssumeReferenced] public float   S28;
                [TS.AssumeReferenced] public float   S29;
                [TS.AssumeReferenced] public float   S30;
                [TS.AssumeReferenced] public float   S31;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct HardwareFrame
            {
                // HW stack frame: pushed upon entering PendSV_Handler
                [TS.AssumeReferenced] public UIntPtr R0;
                [TS.AssumeReferenced] public UIntPtr R1;
                [TS.AssumeReferenced] public UIntPtr R2;
                [TS.AssumeReferenced] public UIntPtr R3;
                [TS.AssumeReferenced] public UIntPtr R12;
                [TS.AssumeReferenced] public UIntPtr LR;
                [TS.AssumeReferenced] public UIntPtr PC;
                [TS.AssumeReferenced] public UIntPtr PSR;

                // HW stack frame for FP
                [TS.AssumeReferenced] public float   S0;
                [TS.AssumeReferenced] public float   S1;
                [TS.AssumeReferenced] public float   S2;
                [TS.AssumeReferenced] public float   S3;
                [TS.AssumeReferenced] public float   S4;
                [TS.AssumeReferenced] public float   S5;
                [TS.AssumeReferenced] public float   S6;
                [TS.AssumeReferenced] public float   S7;
                [TS.AssumeReferenced] public float   S8;
                [TS.AssumeReferenced] public float   S9;
                [TS.AssumeReferenced] public float   S10;
                [TS.AssumeReferenced] public float   S11;
                [TS.AssumeReferenced] public float   S12;
                [TS.AssumeReferenced] public float   S13;
                [TS.AssumeReferenced] public float   S14;
                [TS.AssumeReferenced] public float   S15;
                [TS.AssumeReferenced] public UIntPtr FPSCR_1;
                [TS.AssumeReferenced] public UIntPtr FPSCR_2;
            }

            [TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv7ForLlvm_VFP_RegistersOnStack" )]
            [StructLayout(LayoutKind.Sequential)]
            public struct RegistersOnStackFullFPContext 
            {
                public const uint StackRegister          = EncDef.c_register_sp;
                public const uint LinkRegister           = EncDef.c_register_lr;
                public const uint ProgramCounterRegister = EncDef.c_register_pc;

                //
                // State
                //


                public SoftwareFrame              SoftwareFrameRegisters;
                public SoftwareFloatingPointFrame SoftwareFloatingPointFrameRegisters;
                public HardwareFrame              HardwareFrameRegisters;

                //
                // Helper Methods
                //

                public static unsafe uint TotalFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return HWFrameSize + SwitcherFrameSize;
                    }
                }

                public static unsafe uint HWFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return (uint)sizeof(HardwareFrame);
                    }
                }

                public static unsafe uint SwitcherFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return (uint)(sizeof(SoftwareFrame) + sizeof(SoftwareFloatingPointFrame));
                    }
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RegistersOnStackNoFPContext
            {
                public const uint StackRegister          = EncDef.c_register_sp;
                public const uint LinkRegister           = EncDef.c_register_lr;
                public const uint ProgramCounterRegister = EncDef.c_register_pc;

                //
                // State
                //

                public SoftwareFrame SoftwareFrameRegisters;
                public HardwareFrame HardwareFrameRegisters;

                //
                // Helper Methods
                //

                public static unsafe uint TotalFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return HWFrameSize + SwitcherFrameSize;
                    }
                }

                public static unsafe uint HWFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return (uint)sizeof(HardwareFrame);
                    }
                }

                public static unsafe uint SwitcherFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return (uint)sizeof(SoftwareFrame);
                    }
                }

                //--//

                internal unsafe UIntPtr* GetRegisterPointer( uint idx ) 
                {
                    switch(idx)
                    {
                        case  0: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.R0 ) { return ptr; };
                        case  1: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.R1 ) { return ptr; };
                        case  2: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.R2 ) { return ptr; };
                        case  3: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.R3 ) { return ptr; };
                        case  4: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R4 ) { return ptr; };
                        case  5: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R5 ) { return ptr; };
                        case  6: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R6 ) { return ptr; };
                        case  7: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R7 ) { return ptr; };
                        case  8: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R8 ) { return ptr; };
                        case  9: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R9 ) { return ptr; };
                        case 10: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R10) { return ptr; };
                        case 11: fixed(UIntPtr* ptr = &this.SoftwareFrameRegisters.R11) { return ptr; };
                        case 12: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.R12) { return ptr; };
                        case 13: throw new ArgumentException( "" );
                        case 14: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.LR ) { return ptr; };
                        case 15: fixed(UIntPtr* ptr = &this.HardwareFrameRegisters.PC ) { return ptr; };
                    }

                    return null;
                }
            }

            //--//
            
            //
            // This is the pointer to the base of the stack. Usefull for stack walking.
            //
            protected UIntPtr BaseSP;
            //
            // This is the pointer to the last known position of the stack pointer
            // For a long jump this points to the end of the SW context to install
            //
            protected UIntPtr SP;
            //
            // Return value for mode transitions
            //
            protected uint EXC_RETURN;
            //
            // Track VFP status
            //
            protected bool m_isFullContext;

            //--//

            public Context(RT.ThreadImpl owner) : base(owner)
            {
            }

            //
            // Overrides
            //

            public override void SwitchTo( )
            {
                // The long jump selects the current thread's context and sets its EXC_RETURN value
                ProcessorARMv7M.RaiseSupervisorCall( ProcessorARMv7M.SVC_Code.SupervisorCall__LongJump );
                
#if DEBUG_CTX_SWITCH
                BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!  ERROR  !!!!!!!!!!!!!!!!!!!!!!!" );
                BugCheck.Log( "!!! Back after Long Jump after, Ctx Switch Failed !!!" );
                BugCheck.Log( "!!!!!!!!!!!!!!!!!!!!!  ERROR  !!!!!!!!!!!!!!!!!!!!!!!" );
#endif

                RT.BugCheck.Assert( false, BugCheck.StopCode.IllegalMode );
            }
            
            public override void Populate( )
            {
                //
                // This woudl be called on the throw context, but in ARMv7M we do not have one
                //
                ThreadImpl        thisThread = ThreadImpl.CurrentThread;
                Processor.Context ctx        = thisThread.SwappedOutContext;

                this.BaseSP     = ctx.BaseStackPointer;
                this.SP         = ProcessorARMv7MForLlvm.GetProcessStackPointer( );
                this.EXC_RETURN = ctx.ExcReturn;
            }

            public override void Populate( Processor.Context context )
            {
                this.BaseSP     = context.BaseStackPointer;
                this.SP         = context.StackPointer;
                this.EXC_RETURN = context.ExcReturn;
            }

            public unsafe override void PopulateFromDelegate( Delegate dlg, uint[] stack )
            {
                DelegateImpl dlgImpl   = (DelegateImpl)(object)dlg;
                ArrayImpl    stackImpl = (ArrayImpl   )(object)stack;
                ObjectImpl   objImpl   = (ObjectImpl  )(object)dlg.Target;

                //
                // Save the initial stack pointer
                // In the general case the SP will be at the top of the current frame we are building
                // When we do a LongJump though, or we start the thread first, we will have to use the base stack pointer
                //
                
                this.BaseSP     = AddressMath.AlignToLowerBoundary( new UIntPtr( stackImpl.GetEndDataPointer( ) ), 8 );
                this.SP         = AddressMath.Decrement( this.BaseSP, RegistersOnStackNoFPContext.TotalFrameSize );
                this.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;

                //
                // Initial SP must still be within the stack array 
                //
                RT.BugCheck.Assert( stackImpl.GetDataPointer( ) <= this.SP.ToPointer( ), BugCheck.StopCode.StackCorruptionDetected ); 

                //
                // build the first stack frame
                //
                RegistersOnStackNoFPContext* firstFrame = GetSimpleFrame(this.SP);

                firstFrame->HardwareFrameRegisters.PC         = new UIntPtr( dlgImpl.InnerGetCodePointer().Target.ToPointer() );
                firstFrame->HardwareFrameRegisters.R0         = objImpl.ToPointer();
                firstFrame->HardwareFrameRegisters.PSR        = new UIntPtr(ProcessorARMv7M.c_psr_InitialValue);
                firstFrame->SoftwareFrameRegisters.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;   // !!! here we assume that no context starts with FP context active !!!
                firstFrame->SoftwareFrameRegisters.CONTROL    = c_CONTROL__MODE__THRD_PRIV;

#if DEBUG_CTX_SWITCH
                RT.BugCheck.Log( "[PFD-ctx] EXC=0x%08x, PSR=0x%08x, PC=0x%08x, R0=0x%08x, SP(aligned)=0x%08x",
                    (int)firstFrame->EXC_RETURN,
                    (int)firstFrame->PSR.ToUInt32( ),
                    (int)firstFrame->PC.ToUInt32( ),
                    (int)firstFrame->R0.ToUInt32( ),
                    (int)this.SP.ToUInt32( )
                    );

                RT.BugCheck.Log( "[PFD-stackImpl] SP(start)=0x%08x, SP(end)=0x%08x, SP(length)=0x%08x, SP(offset)=0x%08x",
                    (int)( stackImpl.GetDataPointer( ) ),
                    (int)( stackImpl.GetEndDataPointer( ) ),
                    (int)( stackImpl.GetEndDataPointer( ) - stackImpl.GetDataPointer( ) ),
                    (int)( (int)stackImpl.GetEndDataPointer( ) - this.SP.ToUInt32( ) )
                    );
#endif
            }

            public override void SetupForExceptionHandling(uint mode)
            {
                //
                // Stop any exception from happening
                //
                using (Runtime.SmartHandles.InterruptState.DisableAll())
                {
                    //
                    // Retrieve the MSP< which we will use to handle exceptions
                    //
                    UIntPtr stack = ProcessorARMv7MForLlvm.GetMainStackPointer();

                    ////
                    //// Enter target mode, with interrupts disabled.
                    ////                    
                    //SwitchToHandlerPrivilegedMode( );

                    //
                    // Set the stack pointer in the context to be the current MSP
                    //
                    this.BaseSP     = ProcessorARMv7MForLlvm.GetMainStackPointerAtReset( );
                    this.SP         = stack;
                    this.EXC_RETURN = c_MODE_RETURN__THREAD_MSP;

                    ////
                    //// Switch back to original mode
                    ////                    
                    //SwitchToThreadUnprivilegedMode( ); 
                }
            }

#region Tracking Collector and Exceptions  

            public override bool Unwind( )
            {
                throw new Exception( "Unwind not implemented" );
            }

            public override unsafe UIntPtr GetRegisterByIndex( uint idx )
            {
                RegistersOnStackNoFPContext* frame = GetSimpleFrame(this.SP);
                
                return *( frame->GetRegisterPointer( idx ) );
            }

            public override unsafe void SetRegisterByIndex( uint idx, UIntPtr value )
            {
                BugCheck.Assert( false, BugCheck.StopCode.InvalidOperation ); 
            }

#endregion

            private static UIntPtr ContextSwitch( ThreadManager tm, UIntPtr stackPointer, bool isFullFrame )
            {
                ThreadImpl currentThread = tm.CurrentThread;
                ThreadImpl nextThread    = tm.NextThread;
                Context    ctx;

                if(currentThread != null)
                {
                    ctx = (Context)currentThread.SwappedOutContext;

                    //
                    // update SP as well as the EXC_RETURN address
                    //     
                    ctx.IsFullContext = isFullFrame;
                    ctx.EXC_RETURN = isFullFrame 
                        ? GetFullFrame(stackPointer)  ->SoftwareFrameRegisters.EXC_RETURN
                        : GetSimpleFrame(stackPointer)->SoftwareFrameRegisters.EXC_RETURN;
                    ctx.StackPointer = stackPointer;
                }

                ctx = (Context)nextThread.SwappedOutContext;

                //
                // Pass EXC_RETURN down to the native portion of the 
                // PendSV handler we need to offset to the beginning of the frame
                //
                ProcessorARMv7MForLlvm.SetExcReturn( ctx.EXC_RETURN );

                //
                // Update thread manager state and Thread.CurrentThread static field
                //
                tm.CurrentThread = nextThread;

                ThreadImpl.CurrentThread = nextThread;

                return ctx.StackPointer;
            }

            //--//
            //--//
            //--//
            
            private static unsafe void FirstLongJump( )
            {
                LongJump( ); 
            }
            
            private static unsafe void LongJump( )
            {
                //
                // Retrieve next context from ThreadManager
                //
                Context currentThreadCtx = (ProcessorARMv7MForLlvm_VFP.Context)ThreadManager.Instance.CurrentThread.SwappedOutContext;

                //
                // Set the PSP at R0 so that returning from the SVC handler will complete the work
                //
                ProcessorARMv7MForLlvm.SetProcessStackPointer( AddressMath.Increment( 
                    currentThreadCtx.StackPointer, 
                    currentThreadCtx.IsFullContext 
                        ? RegistersOnStackFullFPContext.SwitcherFrameSize 
                        : RegistersOnStackNoFPContext.SwitcherFrameSize ) );

                ProcessorARMv7MForLlvm.SetExcReturn( currentThreadCtx.EXC_RETURN );

                //
                // SWitch to unprivileged mode before jumping to our thread 
                // This can only be enabled when we have a model for allowing tasks 
                // to enable/disable interrupts
                //
                //ProcessorARMv7M.SwitchToUnprivilegedMode( ); 
            }

            private static unsafe void LongJumpForRetireThread( )
            {
                LongJump( ); 
            }
            
            //
            // Access Methods
            //

            public override UIntPtr StackPointer
            {
                [RT.Inline]
                get { return this.SP; }
                [RT.Inline]
                set { this.SP = value; }
            }

            public override UIntPtr BaseStackPointer
            {
                [RT.Inline]
                get { return this.BaseSP; }
            }

            public override uint ExcReturn
            {
                [RT.Inline]
                get { return this.EXC_RETURN;  }
                [RT.Inline]
                set { this.EXC_RETURN = value; }
            }

            public bool IsFullContext
            {
                [RT.Inline]
                get { return this.m_isFullContext; }
                [RT.Inline]
                set { this.m_isFullContext = value; }
            }

            public override UIntPtr ProgramCounter
            {
                get
                {
                    //return Registers.PC;
                    return (UIntPtr)0;
                }
                set
                {
                    //Registers.PC = value;
                }
            }

            public override uint ScratchedIntegerRegisters
            {
                get { return 0; }
            }

            //--//
            
            [RT.Inline]
            internal static unsafe Context.RegistersOnStackFullFPContext* GetFullFrame( UIntPtr SP )
            {
                return (Context.RegistersOnStackFullFPContext*)SP.ToPointer( );
            }

            [RT.Inline]
            internal static unsafe Context.RegistersOnStackNoFPContext* GetSimpleFrame( UIntPtr SP )
            {
                return (Context.RegistersOnStackNoFPContext*)SP.ToPointer( );
            }
            
            private static unsafe void SnapshotActiveFrame( ref Context.RegistersOnStackNoFPContext frame )
            {
                ////
                //// Can run in handler mode only
                //// 
                //BugCheck.Assert( VerifyHandlerMode( ), BugCheck.StopCode.IllegalMode );

                ////
                //// Retrieve PSP and snapshot the frame that was pushed by the SVC handler
                ////
                //UIntPtr psp = GetProcessStackPointer( );

                //Context.RegistersOnStackNoFPContext* snapshot = GetSimpleFrame( psp );
                
                //frame.HardwareFrameRegisters = snapshot->HardwareFrameRegisters;
                //frame.SoftwareFrameRegisters = snapshot->SoftwareFrameRegisters;
            }

            //
            // Helpers 
            //

            [Inline]
            public static void InterruptHandlerWithContextSwitch( UIntPtr stackPtr )
            {
                Peripherals.Instance.ProcessInterrupt( );
                
                ThreadManager tm = ThreadManager.Instance;

                //
                // We keep looping until the current and next threads are the same,
                // because when swapping out a dead thread, we might wake up a different thread.
                //
                while(tm.ShouldContextSwitch)
                {
                    ContextSwitch( tm, stackPtr, false );
                }
            }

            [Inline]
            public static void InterruptHandlerWithoutContextSwitch( )
            {
                Peripherals.Instance.ProcessInterrupt( );
            }

            [Inline]
            public static void FastInterruptHandlerWithoutContextSwitch( )
            {
                Peripherals.Instance.ProcessFastInterrupt( );
            }

            [Inline]
            public static void GenericSoftwareInterruptHandler( ref RegistersOnStackNoFPContext registers )
            {
            }

            //--//

            [Inline]
            private static unsafe void PrepareStackForException( uint mode, RegistersOnStackNoFPContext* ptr )
            {

                //
                // EXC_RETURN to go back to previous mode.
                //
                // TODO: 

                //
                // Save unbanked R4 -R11.
                //


                // TODO: 

                //
                // Switch back to the exception handling mode.
                //


                // TODO: 

                //
                // R1 should point to the Register Context on the stack.
                //

                // TODO: 
                //SetRegister( EncDef.c_register_r1, new UIntPtr( ptr ) );
            }

            [Inline]
            private static unsafe void RestoreStackForException( uint mode, RegistersOnStackNoFPContext* ptr )
            {

                //
                // EXC_RETUNR to go back to previous mode.
                //
                // TODO: 

                //
                // Save unbanked R4 -R11.
                //


                // TODO: 

                //
                // Switch back to the exception handling mode.
                //


                // TODO: 

                //
                // R1 should point to the Register Context on the stack.
                //

                // TODO: 
                //SetRegister( EncDef.c_register_r1, new UIntPtr( ptr ) );
            }
            
            //
            // All overridable exceptions for Ctx Switch
            //

            [RT.CapabilitiesFilter( RequiredCapabilities=TargetModel.ArmProcessor.InstructionSetVersion.Platform_VFP__HardVFP )]
            [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            [RT.ExportedMethod]
            private static unsafe void SVC_Handler_Zelig_VFP_NoFPContext( uint* args )
            {
                SVC_Code svc_number = (SVC_Code)((byte*)args[6])[-2]; // svc number is at stacked PC offset - 2 bytes
                
                switch(svc_number)
                {
                    case SVC_Code.SupervisorCall__LongJump:
                        LongJump( );
                        break;
                    case SVC_Code.SupervisorCall__StartThreads:
                        FirstLongJump( );
                        break;
                    case SVC_Code.SupervisorCall__RetireThread:
                        LongJumpForRetireThread( );
                        break;
                    case SVC_Code.SupervisorCall__SnapshotProcessModeRegisters:
                        ProcessorARMv7MForLlvm.UpdateFrame( ref ProcessorARMv7MForLlvm.Snapshot, CUSTOM_STUB_FetchSoftwareFrameSnapshot( ) );
                        break;
                    default:
                        BugCheck.Assert( false, BugCheck.StopCode.Impossible );
                        break;
                }
            }
            
            
            [RT.CapabilitiesFilter( RequiredCapabilities=TargetModel.ArmProcessor.InstructionSetVersion.Platform_VFP__HardVFP )]
            [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            [RT.ExportedMethod]
            private static UIntPtr PendSV_Handler_Zelig_VFP( UIntPtr stackPointer, uint isParitalStack )
            {
                using(RT.SmartHandles.InterruptState.Disable( ))
                {
                    return ContextSwitch( ThreadManager.Instance, stackPointer, isParitalStack == 0 );
                }
            }
            
            [RT.CapabilitiesFilter( RequiredCapabilities=TargetModel.ArmProcessor.InstructionSetVersion.Platform_VFP__HardVFP )]
            [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            [RT.ExportedMethod]
            private static void AnyInterrupt( UIntPtr stackPtr )
            {
                using(RT.SmartHandles.InterruptState.Disable( ))
                {
                    InterruptHandlerWithContextSwitch( stackPtr );
                }
            }
        }

        //
        // State
        //

        internal static Context.RegistersOnStackNoFPContext Snapshot;
    }
}
