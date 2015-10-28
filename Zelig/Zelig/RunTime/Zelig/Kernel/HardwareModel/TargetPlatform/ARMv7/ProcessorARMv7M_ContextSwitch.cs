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
            //
            // WARNING: Don't assume the actual layout of the structure is sequential!!!
            // WARNING: The code generator rearranges the fields to minimize the cost of a context switch!!!
            //
            //[TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv4_RegistersOnStack" )]
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
                // HW stack frame: pushed upon entering PendSV_Handler
                [TS.AssumeReferenced] public UIntPtr R0;
                [TS.AssumeReferenced] public UIntPtr R1;
                [TS.AssumeReferenced] public UIntPtr R2;
                [TS.AssumeReferenced] public UIntPtr R3;
                [TS.AssumeReferenced] public UIntPtr R12;
                [TS.AssumeReferenced] public UIntPtr LR;
                [TS.AssumeReferenced] public UIntPtr PC;
                [TS.AssumeReferenced] public UIntPtr PSR;

                //
                // Helper Methods
                //

                internal unsafe UIntPtr* GetRegisterPointer( uint idx )
                {
                    switch(idx)
                    {
                        // SW stack frame
                        case 4: fixed (UIntPtr* ptr = &this.R4) { return ptr; };
                        case 5: fixed (UIntPtr* ptr = &this.R5) { return ptr; };
                        case 6: fixed (UIntPtr* ptr = &this.R6) { return ptr; };
                        case 7: fixed (UIntPtr* ptr = &this.R7) { return ptr; };
                        case 8: fixed (UIntPtr* ptr = &this.R8) { return ptr; };
                        case 9: fixed (UIntPtr* ptr = &this.R9) { return ptr; };
                        case 10: fixed (UIntPtr* ptr = &this.R10) { return ptr; };
                        case 11: fixed (UIntPtr* ptr = &this.R11) { return ptr; };
                        // HW stack frame
                        case 0: fixed (UIntPtr* ptr = &this.R0) { return ptr; };
                        case 1: fixed (UIntPtr* ptr = &this.R1) { return ptr; };
                        case 2: fixed (UIntPtr* ptr = &this.R2) { return ptr; };
                        case 3: fixed (UIntPtr* ptr = &this.R3) { return ptr; };
                        case 12: fixed (UIntPtr* ptr = &this.R12) { return ptr; };
                        case 13: fixed (UIntPtr* ptr = &this.LR) { return ptr; };
                        case 15: fixed (UIntPtr* ptr = &this.PC) { return ptr; };
                        case 16: fixed (UIntPtr* ptr = &this.PSR) { return ptr; };
                    }

                    return null;
                }

                public void Assign( ref RegistersOnStack other )
                {
                    // SW frame
                    this.EXC_RETURN = other.EXC_RETURN;
                    this.CONTROL    = other.CONTROL;
                    this.R4         = other.R4;
                    this.R5         = other.R5;
                    this.R6         = other.R6;
                    this.R7         = other.R7;
                    this.R8         = other.R8;
                    this.R9         = other.R9;
                    this.R10        = other.R10;
                    this.R11        = other.R11;
                    //--//
                    this.R0         = other.R0;
                    this.R1         = other.R1;
                    this.R2         = other.R2;
                    this.R3         = other.R3;
                    this.R12        = other.R12;
                    this.LR         = other.LR;
                    this.PC         = other.PC;
                    this.PSR        = other.PSR;
                }

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
                        return (uint)sizeof( UIntPtr ) * 8;
                    }
                }

                public static unsafe uint SwitcherFrameSize
                {
                    [RT.Inline]
                    get
                    {
                        return (uint)sizeof( UIntPtr ) * 10;
                    }
                }

                //--//

                //public unsafe UIntPtr StackFrame
                //{
                //    [RT.Inline]
                //    get
                //    {
                //        return HWStackFrame;
                //    }
                //}

                //public unsafe UIntPtr HWStackFrame
                //{
                //    [RT.Inline]
                //    get
                //    {
                //        ArrayImpl registersImpl = (ArrayImpl)(object)this;
                //        return new UIntPtr( registersImpl.GetEndDataPointer() );
                //    }
                //}

                //public unsafe UIntPtr SWStackFrame
                //{
                //    [RT.Inline]
                //    get
                //    {
                //        ArrayImpl registersImpl = (ArrayImpl)(object)this;
                //        return new UIntPtr( registersImpl.GetEndDataPointer() + HWStackFrameSize );
                //    }
                //}
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
                ProcessorARMv7M.SetExcReturn( ProcessorARMv7M.c_MODE_RETURN__THREAD_PSP );

                //
                // Load new task and fill frame
                //
                ProcessorARMv7M.SetProcessStackPointer( 
                    AddressMath.Increment( this.StackPointer, RegistersOnStack.SwitcherFrameSize ) 
                    );
                
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
                // build the first stack frame
                //
                RegistersOnStack registers = new RegistersOnStack();
                registers.PC            = new UIntPtr( dlgImpl.InnerGetCodePointer( ).Target.ToPointer( ) );
                registers.PSR           = new UIntPtr( c_psr_InitialValue );
                registers.EXC_RETURN    = c_MODE_RETURN__THREAD_PSP;
                registers.CONTROL       = c_CONTROL__MODE__THRD_PRIV;
                registers.R0            = objImpl.ToPointer( );

#if DEBUG_CTX_SWITCH
                //registers.R1  = new UIntPtr( 0x11111111 ); //UIntPtr.Zero;
                //registers.R2  = new UIntPtr( 0x22222222 ); //UIntPtr.Zero;
                //registers.R3  = new UIntPtr( 0x33333333 ); //UIntPtr.Zero;
                ////--//
                //registers.R4  = new UIntPtr( 0x44444444 ); //UIntPtr.Zero; // start of SW frame
                //registers.R5  = new UIntPtr( 0x55555555 ); //UIntPtr.Zero;
                //registers.R6  = new UIntPtr( 0x66666666 ); //UIntPtr.Zero;
                //registers.R7  = new UIntPtr( 0x77777777 ); //UIntPtr.Zero;
                //registers.R8  = new UIntPtr( 0x88888888 ); //UIntPtr.Zero;
                //registers.R9  = new UIntPtr( 0x99999999 ); //UIntPtr.Zero;
                //registers.R10 = new UIntPtr( 0xaaaaaaaa ); //UIntPtr.Zero;
                //registers.R11 = new UIntPtr( 0xbbbbbbbb ); //UIntPtr.Zero;
                //registers.R12 = new UIntPtr( 0xcccccccc ); //UIntPtr.Zero;
#endif
                //
                // Save the initial stack pointer
                // In the general case the SP will be at teh top of the current frame we are building
                // When we do a LongJump thoug, or we start the thread first, we will have to use the base stack pointer
                //
                this.SP         = GetFirstStackPointerFromPhysicalStack( stackImpl );
                this.EXC_RETURN = c_MODE_RETURN__THREAD_PSP;

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

                //
                // Initial offset from start of stack storage must be at least as large as a frame
                //
                RT.BugCheck.Assert( (((int)stackImpl.GetEndDataPointer( ) - this.SP.ToUInt32( )) >= RegistersOnStack.TotalFrameSize), 
                    BugCheck.StopCode.StackCorruptionDetected 
                    ); 

                //
                // set the initial stack values in the physical stack
                // this used to be done with ARM specific operators
                // now we need to push manually
                //

                RegistersOnStack* firstFrame = PointerToFrame( this.SP );

                firstFrame->Assign( ref registers );

                //--//
                
                RT.BugCheck.Assert( firstFrame->R0          == registers.R0        , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->R1          == registers.R1        , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->R2          == registers.R2        , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->R3          == registers.R3        , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->R12         == registers.R12       , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->LR          == registers.LR        , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->PC          == registers.PC        , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->PSR         == registers.PSR       , BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->EXC_RETURN  == registers.EXC_RETURN, BugCheck.StopCode.StackCorruptionDetected );
                RT.BugCheck.Assert( firstFrame->CONTROL     == registers.CONTROL   , BugCheck.StopCode.StackCorruptionDetected );
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
                    // Set the stack pointer in the context to be teh current MSP
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


            private static void ContextSwitch( ThreadManager tm, ref RegistersOnStack registers )
            {
                ThreadImpl currentThread = tm.CurrentThread;
                ThreadImpl nextThread    = tm.NextThread;
                Context    ctx;

                if(currentThread != null)
                {
                    ctx = (Context)currentThread.SwappedOutContext;

                    //ctx.Registers.Assign( ref registers );

                    //
                    // update SP as well as the EXC_RETURN address
                    //
                    ctx.EXC_RETURN   = registers.EXC_RETURN;    
                    ctx.StackPointer = AddressMath.Decrement( GetProcessStackPointer( ), RegistersOnStack.SwitcherFrameSize );                    
                }

                ctx = (Context)nextThread.SwappedOutContext;

                //registers.Assign( ref ctx.Registers );

                //
                // Pass PSP and EXC_RETURN down to the native portion of the 
                // PendSV handler we need to offset to the beginning of the frame
                //
                SetProcessStackPointer( ctx.StackPointer );
                SetExcReturn          ( ctx.EXC_RETURN ); 
                    
                //
                // Update thread manager state and Thread.CurrentThread static field
                //
                tm.CurrentThread = nextThread;

                ThreadImpl.CurrentThread = nextThread;
            }

            private static unsafe void ContextSwitch( UIntPtr currentStackPointer )
            {
                //
                // This exception executes on the Main Processor stack (MSP), our threads execute on 
                // the Process Stack (PSP). 
                //

                //
                //  Precondition?
                //

                ContextSwitch( ThreadManager.Instance, ref *PointerToFrame( currentStackPointer ) );

                //RT.BugCheck.Log( "[ContextSwitch] PSP=0x%08x", (int)GetProcessStackPointer( ) );

                //BugCheck.Assert( ThreadManager.Instance.CurrentThread.SwappedOutContext.StackPointer == GetProcessStackPointer( ), BugCheck.StopCode.ContextSwitchFailed );
            }

            private static unsafe void ContextSwitch( )
            {
                ContextSwitch( GetProcessStackPointer() );
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

                ThreadManager tm = ThreadManager.Instance;

                //
                // We keep looping until the current and next threads are the same,
                // because when swapping out a dead thread, we might wake up a different thread.
                //
                while(tm.ShouldContextSwitch)
                {
                    ContextSwitch( tm, ref registers );
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
            public static void GenericSoftwareInterruptHandler( ref RegistersOnStack registers )
            {
            }

            //--//

            [Inline]
            private static unsafe void PrepareStackForException( uint mode,
                                                                 Context.RegistersOnStack* ptr )
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
            private static unsafe void RestoreStackForException( uint mode,
                                                                 Context.RegistersOnStack* ptr )
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
            private static void PendSV_Handler_Zelig( )
            {
                using(SmartHandles.InterruptState.Disable( ))
                {
                    ContextSwitch( );
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
