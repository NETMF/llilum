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

    public abstract partial class ProcessorARMv7M
    {
        //--//

        //
        // Part of Context may be defined in the model for the targeted sub-system, e.g. Mbed or CMSIS-Core for ARM processors
        //

        public abstract new class Context : Processor.Context
        {
            [StructLayout( LayoutKind.Sequential, Pack = 4 )]
            public struct SoftwareFrame
            {
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
            };

            [StructLayout( LayoutKind.Sequential, Pack = 4 )]
            public struct HardwareFrame
            {
                [TS.AssumeReferenced] public UIntPtr R0;
                [TS.AssumeReferenced] public UIntPtr R1;
                [TS.AssumeReferenced] public UIntPtr R2;
                [TS.AssumeReferenced] public UIntPtr R3;
                [TS.AssumeReferenced] public UIntPtr R12;
                [TS.AssumeReferenced] public UIntPtr LR;
                [TS.AssumeReferenced] public UIntPtr PC;
                [TS.AssumeReferenced] public UIntPtr PSR;
            };

            //[TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv7_RegistersOnStack" )]
            [StructLayout( LayoutKind.Sequential, Pack = 4 )]
            public struct RegistersOnStack
            {
                public const uint StackRegister          = EncDef.c_register_sp;
                public const uint LinkRegister           = EncDef.c_register_lr;
                public const uint ProgramCounterRegister = EncDef.c_register_pc;

                //
                // State
                //

                // SW stack frame: pushed by PendSV_Handler
                public SoftwareFrame SoftwareFrameRegisters;
                // HW stack frame: pushed upon entering PendSV_Handler
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
                        return (uint)sizeof( HardwareFrame );
                    }
                }

                public static unsafe uint SwitcherFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return (uint)sizeof( SoftwareFrame );
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

            //--//

            //
            // Overrides
            //

            public override unsafe void SwitchTo( )
            {
                //
                // The long jump selects the current thread's context and sets its EXC_RETURN value
                //
                ProcessorARMv7M.RaiseSupervisorCall( SVC_Code.SupervisorCall__LongJump );

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
                this.SP         = ctx.StackPointer;
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
                this.SP         = AddressMath.Decrement( this.BaseSP, RegistersOnStack.TotalFrameSize );
                this.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;

                //
                // Initial offset from start of stack storage must be at least as large as a frame
                //
                RT.BugCheck.Assert((((int)stackImpl.GetEndDataPointer() - this.SP.ToUInt32()) >= RegistersOnStack.TotalFrameSize),
                    BugCheck.StopCode.StackCorruptionDetected
                    );

                RegistersOnStack* firstFrame = GetFrame(this.SP);

                //
                // build the first stack frame
                //
                firstFrame->HardwareFrameRegisters.PC         = new UIntPtr( dlgImpl.InnerGetCodePointer( ).Target.ToPointer( ) );
                firstFrame->HardwareFrameRegisters.PSR        = new UIntPtr( c_psr_InitialValue );
                firstFrame->SoftwareFrameRegisters.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;
                firstFrame->SoftwareFrameRegisters.CONTROL    = c_CONTROL__MODE__THRD_PRIV;
                firstFrame->HardwareFrameRegisters.R0         = objImpl.ToPointer( );

#if DEBUG_CTX_SWITCH
                RT.BugCheck.Log( "[PFD-ctx] EXC=0x%08x, PSR=0x%08x, PC=0x%08x, R0=0x%08x, SP(aligned)=0x%08x",
                    (int)registers.EXC_RETURN,
                    (int)registers.PSR.ToUInt32( ),
                    (int)registers.PC.ToUInt32( ),
                    (int)registers.R0.ToUInt32( ),
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

            public override void SetupForExceptionHandling( uint mode )
            {
                //
                // Stop any exception from happening
                //
                using(Runtime.SmartHandles.InterruptState.DisableAll())
                {
                    //
                    // Retrieve the MSP< which we will use to handle exceptions
                    //
                    UIntPtr stack = GetMainStackPointer();
                    
                    ////
                    //// Enter target mode, with interrupts disabled.
                    ////                    
                    //SwitchToHandlerPrivilegedMode( );

                    //
                    // Set the stack pointer in the context to be the current MSP
                    //
                    this.StackPointer = stack;
                    
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
                RegistersOnStack* frame = GetFrame(this.SP);
                
                return *( frame->GetRegisterPointer( idx ) );
            }

            public override unsafe void SetRegisterByIndex( uint idx, UIntPtr value )
            {
                BugCheck.Assert( false, BugCheck.StopCode.InvalidOperation ); 
            }

#endregion


            private static UIntPtr ContextSwitch( ThreadManager tm, UIntPtr stackPointer )
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
                    unsafe
                    {
                        ctx.EXC_RETURN = GetFrame(stackPointer)->SoftwareFrameRegisters.EXC_RETURN;
                    }

                    ctx.StackPointer = stackPointer;
                }

                ctx = (Context)nextThread.SwappedOutContext;

                //
                // Pass EXC_RETURN down to the native portion of the 
                // PendSV handler we need to offset to the beginning of the frame
                //
                SetExcReturn( ctx.EXC_RETURN ); 
                    
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
                Context currentThreadCtx = (ProcessorARMv7M.Context)ThreadManager.Instance.CurrentThread.SwappedOutContext;
                                
                //
                // Set the PSP at R0 so that returning from the SVC handler will complete the work
                //
                SetProcessStackPointer(
                    AddressMath.Increment( currentThreadCtx.StackPointer, ProcessorARMv7M.Context.RegistersOnStack.SwitcherFrameSize )
                    );

                SetExcReturn( currentThreadCtx.EXC_RETURN ); 

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
                get { return this.SP; }
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
            internal static unsafe Context.RegistersOnStack* GetFrame( UIntPtr SP )
            {
                return (Context.RegistersOnStack*)SP.ToPointer( );
            }
            
            private static unsafe void SnapshotActiveFrame( ref Context.RegistersOnStack frame )
            {
                ////
                //// Can run in handler mode only
                //// 
                //BugCheck.Assert( VerifyHandlerMode( ), BugCheck.StopCode.IllegalMode );

                ////
                //// Retrieve PSP and snapshot the frame that was pushed by the SVC handler
                ////
                //UIntPtr psp = GetProcessStackPointer( );

                //Context.RegistersOnStack* snapshot = GetFrame( psp );
                
                //frame.HardwareFrameRegisters = snapshot->HardwareFrameRegisters;
                //frame.SoftwareFrameRegisters = snapshot->SoftwareFrameRegisters;
            }
            
            //
            // Helpers 
            //

            [Inline]
            public static void InterruptHandlerWithContextSwitch( ref RegistersOnStack registers )
            {
                Peripherals.Instance.ProcessInterrupt( );

                //ThreadManager tm = ThreadManager.Instance;

                //
                // We keep looping until the current and next threads are the same,
                // because when swapping out a dead thread, we might wake up a different thread.
                //
                //while(tm.ShouldContextSwitch)
                //{
                //    ContextSwitch(tm, ref registers);
                //}
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
            public static void GenericSoftwareInterruptHandler( ref RegistersOnStack registers )
            {
            }

            //--//

            [Inline]
            private static unsafe void PrepareStackForException( uint mode, Context.RegistersOnStack* ptr )
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
            private static unsafe void RestoreStackForException( uint mode, Context.RegistersOnStack* ptr )
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
            // All overridable exceptions for Ctx switch
            //
            
            [RT.HardwareExceptionHandler( RT.HardwareException.Service )]
            [RT.ExportedMethod]
            private static unsafe void SVC_Handler_Zelig( uint* args )
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
                        UpdateFrame( ref ProcessorARMv7M.Snapshot, CUSTOM_STUB_FetchSoftwareFrameSnapshot( ) ); 
                        break;
                    default:
                        BugCheck.Assert( false, BugCheck.StopCode.Impossible );
                        break;
                }
            }
            
            [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            [RT.ExportedMethod]
            private static UIntPtr PendSV_Handler_Zelig( UIntPtr stackPtr )
            {
                using(SmartHandles.InterruptState.Disable( ))
                {
                    unsafe
                    {
                        return ContextSwitch(ThreadManager.Instance, stackPtr );
                    }
                }
            }

            [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
            [RT.ExportedMethod]
            private static void AnyInterrupt( )
            {

            }        
        }

        //--//

        //
        // State
        //

        internal static Context.RegistersOnStack Snapshot;

        //
        // Helper methods
        //

        //--//
        
        [ExportedMethod]
        private static unsafe void CUSTOM_STUB_NotifySoftwareFrameSnapshot( void* frame, int size )
        {
            BugCheck.Assert( size * sizeof( uint ) == Context.RegistersOnStack.TotalFrameSize, BugCheck.StopCode.StackCorruptionDetected ); 

            uint* registers = (uint*)frame;

            UpdateFrame( ref Snapshot, registers ); 
        }

        protected static unsafe void UpdateFrame( ref Context.RegistersOnStack snapshot, uint* registers )
        {
            BugCheck.Log( "[Last Active Frame] EXC=0x%08x, PSR=0x%08x, PC=0x%08x",
                    (int)registers[ 0 ],
                    (int)registers[ 17 ],
                    (int)registers[ 16 ]
                    );

            BugCheck.Log( "[Last Active Frame] R0=0x%08x, R1=0x%08x, R2=0x%08x, R3=0x%08x, R12=0x%08x",
                    (int)registers[ 10 ],
                    (int)registers[ 11 ],
                    (int)registers[ 12 ],
                    (int)registers[ 13 ],
                    (int)registers[ 14 ]
                    );

            //snapshot.SoftwareFrameRegisters.EXC_RETURN = registers[ 0 ];
            //snapshot.SoftwareFrameRegisters.CONTROL = registers[ 1 ];
            //snapshot.SoftwareFrameRegisters.R4 = new UIntPtr( registers[ 2 ] );
            //snapshot.SoftwareFrameRegisters.R5 = new UIntPtr( registers[ 3 ] );
            //snapshot.SoftwareFrameRegisters.R6 = new UIntPtr( registers[ 4 ] );
            //snapshot.SoftwareFrameRegisters.R7 = new UIntPtr( registers[ 5 ] );
            //snapshot.SoftwareFrameRegisters.R8 = new UIntPtr( registers[ 6 ] );
            //snapshot.SoftwareFrameRegisters.R9 = new UIntPtr( registers[ 7 ] );
            //snapshot.SoftwareFrameRegisters.R10 = new UIntPtr( registers[ 8 ] );
            //snapshot.SoftwareFrameRegisters.R11 = new UIntPtr( registers[ 9 ] );
            ////--//
            //snapshot.HardwareFrameRegisters.R0 = new UIntPtr( registers[ 10 ] );
            //snapshot.HardwareFrameRegisters.R1 = new UIntPtr( registers[ 11 ] );
            //snapshot.HardwareFrameRegisters.R2 = new UIntPtr( registers[ 12 ] );
            //snapshot.HardwareFrameRegisters.R3 = new UIntPtr( registers[ 13 ] );
            //snapshot.HardwareFrameRegisters.R12 = new UIntPtr( registers[ 14 ] );
            //snapshot.HardwareFrameRegisters.LR = new UIntPtr( registers[ 15 ] );
            //snapshot.HardwareFrameRegisters.PC = new UIntPtr( registers[ 16 ] );
            //snapshot.HardwareFrameRegisters.PSR = new UIntPtr( registers[ 17 ] );
        }
    }
}
