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
                public SoftwareFrame SoftwareStack;
                // HW stack frame: pushed upon entering PendSV_Handler
                public HardwareFrame HardwareStack;

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
            }

            //--//
            
            //
            // This is the pointer to the last known position of the stack pointer
            // For a long jump this points to the end of the SW context to install
            //
            protected UIntPtr SP;
            protected uint    EXC_RETURN;

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
                BugCheck.Raise( BugCheck.StopCode.InvalidOperation );
            }

            public override void Populate( Processor.Context context )
            {
                BugCheck.Raise( BugCheck.StopCode.InvalidOperation );
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
                this.SP         = GetFirstStackPointerFromPhysicalStack( stackImpl );
                this.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;

                //
                // Initial offset from start of stack storage must be at least as large as a frame
                //
                RT.BugCheck.Assert((((int)stackImpl.GetEndDataPointer() - this.SP.ToUInt32()) >= RegistersOnStack.TotalFrameSize),
                    BugCheck.StopCode.StackCorruptionDetected
                    );

                RegistersOnStack* firstFrame = PointerToFrame(this.SP);

                //
                // build the first stack frame
                //
                firstFrame->HardwareStack.PC         = new UIntPtr( dlgImpl.InnerGetCodePointer( ).Target.ToPointer( ) );
                firstFrame->HardwareStack.PSR        = new UIntPtr( c_psr_InitialValue );
                firstFrame->SoftwareStack.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;
                firstFrame->SoftwareStack.CONTROL    = c_CONTROL__MODE__THRD_PRIV;
                firstFrame->HardwareStack.R0         = objImpl.ToPointer( );

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
                //return *( this.Registers.GetRegisterPointer( idx ) );
                return (UIntPtr)0;
            }

            public override unsafe void SetRegisterByIndex( uint idx, UIntPtr value )
            {
                //*( this.Registers.GetRegisterPointer( idx ) ) = value;
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
                        ctx.EXC_RETURN = PointerToFrame(stackPointer)->SoftwareStack.EXC_RETURN;
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
            
            private unsafe UIntPtr GetFirstStackPointerFromPhysicalStack( ArrayImpl stackImpl )
            {
                return AddressMath.AlignToLowerBoundary(
                    AddressMath.Decrement( new UIntPtr( stackImpl.GetEndDataPointer( ) ), RegistersOnStack.TotalFrameSize ), 8 );
            }

            //
            // Access Methods
            //

            public override UIntPtr StackPointer
            {
                get { return this.SP; }
                set { this.SP = value; }
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
            internal static unsafe Context.RegistersOnStack* PointerToFrame( UIntPtr SP )
            {
                return (Context.RegistersOnStack*)SP.ToPointer( );
            }

            [RT.Inline]
            private static unsafe Context.RegistersOnStack* GetFrameFromPhysicalStack( )
            {
                return (Context.RegistersOnStack*)GetProcessStackPointer( ).ToPointer( );
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
    }
}
