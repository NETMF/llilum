//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv5
{
    using System;
    using System.Runtime.InteropServices;

    using              Microsoft.Zelig.TargetModel.ArmProcessor;
    using TS         = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef     = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using EncDef_VFP = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_VFP_ARM;


    public abstract partial class ProcessorARMv5_VFP
    {
        public abstract new class Context : Processor.Context
        {
            //
            // WARNING: Don't assume the actual layout of the structure is sequential!!!
            // WARNING: The code generator rearranges the fields to minimize the cost of a context switch!!!
            //
            [TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv5_VFP_RegistersOnStack" )]
            [StructLayout( LayoutKind.Sequential, Pack = 4 )]
            public struct RegistersOnStack
            {
                public const uint StackRegister          = EncDef.c_register_sp;
                public const uint LinkRegister           = EncDef.c_register_lr;
                public const uint ProgramCounterRegister = EncDef.c_register_pc;

                //
                // State
                //

                [TS.AssumeReferenced] public uint    FPSCR;
                [TS.AssumeReferenced] public uint    FPEXC;

                [TS.AssumeReferenced] public uint    CPSR;
                [TS.AssumeReferenced] public UIntPtr SP;
                [TS.AssumeReferenced] public UIntPtr LR;

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

                [TS.AssumeReferenced] public UIntPtr R0;
                [TS.AssumeReferenced] public UIntPtr R1;
                [TS.AssumeReferenced] public UIntPtr R2;
                [TS.AssumeReferenced] public UIntPtr R3;
                [TS.AssumeReferenced] public UIntPtr R4;
                [TS.AssumeReferenced] public UIntPtr R5;
                [TS.AssumeReferenced] public UIntPtr R6;
                [TS.AssumeReferenced] public UIntPtr R7;
                [TS.AssumeReferenced] public UIntPtr R8;
                [TS.AssumeReferenced] public UIntPtr R9;
                [TS.AssumeReferenced] public UIntPtr R10;
                [TS.AssumeReferenced] public UIntPtr R11;
                [TS.AssumeReferenced] public UIntPtr R12;
                [TS.AssumeReferenced] public UIntPtr PC;

                //
                // Helper Methods
                //

                internal unsafe UIntPtr* GetRegisterPointer( uint idx ) 
                {
                    switch(idx)
                    {
                        case  0: fixed(UIntPtr* ptr = &this.R0 ) { return ptr; };
                        case  1: fixed(UIntPtr* ptr = &this.R1 ) { return ptr; };
                        case  2: fixed(UIntPtr* ptr = &this.R2 ) { return ptr; };
                        case  3: fixed(UIntPtr* ptr = &this.R3 ) { return ptr; };
                        case  4: fixed(UIntPtr* ptr = &this.R4 ) { return ptr; };
                        case  5: fixed(UIntPtr* ptr = &this.R5 ) { return ptr; };
                        case  6: fixed(UIntPtr* ptr = &this.R6 ) { return ptr; };
                        case  7: fixed(UIntPtr* ptr = &this.R7 ) { return ptr; };
                        case  8: fixed(UIntPtr* ptr = &this.R8 ) { return ptr; };
                        case  9: fixed(UIntPtr* ptr = &this.R9 ) { return ptr; };
                        case 10: fixed(UIntPtr* ptr = &this.R10) { return ptr; };
                        case 11: fixed(UIntPtr* ptr = &this.R11) { return ptr; };
                        case 12: fixed(UIntPtr* ptr = &this.R12) { return ptr; };
                        case 13: fixed(UIntPtr* ptr = &this.SP ) { return ptr; };
                        case 14: fixed(UIntPtr* ptr = &this.LR ) { return ptr; };
                        case 15: fixed(UIntPtr* ptr = &this.PC ) { return ptr; };
                    }

                    return null;
                }

                internal unsafe float* GetFPRegisterPointer( uint idx ) 
                {
                    switch(idx)
                    {
                        case  0: fixed(float* ptr = &this.S0 ) { return ptr; };
                        case  1: fixed(float* ptr = &this.S1 ) { return ptr; };
                        case  2: fixed(float* ptr = &this.S2 ) { return ptr; };
                        case  3: fixed(float* ptr = &this.S3 ) { return ptr; };
                        case  4: fixed(float* ptr = &this.S4 ) { return ptr; };
                        case  5: fixed(float* ptr = &this.S5 ) { return ptr; };
                        case  6: fixed(float* ptr = &this.S6 ) { return ptr; };
                        case  7: fixed(float* ptr = &this.S7 ) { return ptr; };
                        case  8: fixed(float* ptr = &this.S8 ) { return ptr; };
                        case  9: fixed(float* ptr = &this.S9 ) { return ptr; };
                        case 10: fixed(float* ptr = &this.S10) { return ptr; };
                        case 11: fixed(float* ptr = &this.S11) { return ptr; };
                        case 12: fixed(float* ptr = &this.S12) { return ptr; };
                        case 13: fixed(float* ptr = &this.S13) { return ptr; };
                        case 14: fixed(float* ptr = &this.S14) { return ptr; };
                        case 15: fixed(float* ptr = &this.S15) { return ptr; };
                        case 16: fixed(float* ptr = &this.S16) { return ptr; };
                        case 17: fixed(float* ptr = &this.S17) { return ptr; };
                        case 18: fixed(float* ptr = &this.S18) { return ptr; };
                        case 19: fixed(float* ptr = &this.S19) { return ptr; };
                        case 20: fixed(float* ptr = &this.S20) { return ptr; };
                        case 21: fixed(float* ptr = &this.S21) { return ptr; };
                        case 22: fixed(float* ptr = &this.S22) { return ptr; };
                        case 23: fixed(float* ptr = &this.S23) { return ptr; };
                        case 24: fixed(float* ptr = &this.S24) { return ptr; };
                        case 25: fixed(float* ptr = &this.S25) { return ptr; };
                        case 26: fixed(float* ptr = &this.S26) { return ptr; };
                        case 27: fixed(float* ptr = &this.S27) { return ptr; };
                        case 28: fixed(float* ptr = &this.S28) { return ptr; };
                        case 29: fixed(float* ptr = &this.S29) { return ptr; };
                        case 30: fixed(float* ptr = &this.S30) { return ptr; };
                        case 31: fixed(float* ptr = &this.S31) { return ptr; };
                    }

                    return null;
                }

                public void Assign( ref RegistersOnStack other )
                {
                    this.R0    = other.R0;
                    this.R1    = other.R1;
                    this.R2    = other.R2;
                    this.R3    = other.R3;
                    this.R4    = other.R4;
                    this.R5    = other.R5;
                    this.R6    = other.R6;
                    this.R7    = other.R7;
                    this.R8    = other.R8;
                    this.R9    = other.R9;
                    this.R10   = other.R10;
                    this.R11   = other.R11;
                    this.R12   = other.R12;
                    this.SP    = other.SP;
                    this.LR    = other.LR;
                    this.PC    = other.PC;
                    this.CPSR  = other.CPSR;

                    this.S0    = other.S0;
                    this.S1    = other.S1;
                    this.S2    = other.S2;
                    this.S3    = other.S3;
                    this.S4    = other.S4;
                    this.S5    = other.S5;
                    this.S6    = other.S6;
                    this.S7    = other.S7;
                    this.S8    = other.S8;
                    this.S9    = other.S9;
                    this.S10   = other.S10;
                    this.S11   = other.S11;
                    this.S12   = other.S12;
                    this.S13   = other.S13;
                    this.S14   = other.S14;
                    this.S15   = other.S15;
                    this.S16   = other.S16;
                    this.S17   = other.S17;
                    this.S18   = other.S18;
                    this.S19   = other.S19;
                    this.S20   = other.S20;
                    this.S21   = other.S21;
                    this.S22   = other.S22;
                    this.S23   = other.S23;
                    this.S24   = other.S24;
                    this.S25   = other.S25;
                    this.S26   = other.S26;
                    this.S27   = other.S27;
                    this.S28   = other.S28;
                    this.S29   = other.S29;
                    this.S30   = other.S30;
                    this.S31   = other.S31;
                    this.FPSCR = other.FPSCR;
                }
            }

            //
            // State
            //

            const uint c_STMFD_Mask     = 0xFFFF0000;
            const uint c_STMFD_Opcode   = 0xE92D0000;

            const uint c_FSTMFDD_Mask   = 0xFFFF0F00;
            const uint c_FSTMFDD_Opcode = 0xED2D0B00;

            const uint c_SUBSP_Mask     = 0xFFFFF000;
            const uint c_SUBSP_Opcode   = 0xE24DD000;

            //--//

            const uint c_FLDMFDD_Mask   = 0xFFFF0F00;
            const uint c_FLDMFDD_Opcode = 0xECBD0B00;

            const uint c_LDMFD_Mask     = 0xFFFF0000;
            const uint c_LDMFD_Opcode   = 0xE8BD0000;

            //--//

            public RegistersOnStack Registers;

            //
            // Constructor Methods
            //

            public Context()
            {
            }

            //
            // Helper Methods
            //

            [NoInline]
            public override void Populate()
            {
                GetAllTheRegisters();

                //
                // Now we have all the registers at this method boundary, unwind one more time and we have the state at the caller's site.
                //
                Unwind();
            }

            [NoInline]
            public override void Populate( Processor.Context context )
            {
                Context ctx = (Context)context;

                this.Registers.Assign( ref ctx.Registers );
            }

            [NoInline]
            public unsafe override void PopulateFromDelegate( Delegate dlg   ,
                                                              uint[]   stack )
            {
                DelegateImpl dlgImpl   = (DelegateImpl)(object)dlg;
                ArrayImpl    stackImpl = (ArrayImpl   )(object)stack;
                ObjectImpl   objImpl   = (ObjectImpl  )(object)dlg.Target;
    
                this.Registers.CPSR  = c_psr_mode_SYS;
                this.Registers.PC    = new UIntPtr( dlgImpl.InnerGetCodePointer().Target.ToPointer() );
                this.Registers.SP    = new UIntPtr( stackImpl.GetEndDataPointer()                    );
                this.Registers.FPSCR = EncDef_VFP.c_fpscr_DN | EncDef_VFP.c_fpscr_FZ;
                this.Registers.R0    = objImpl.ToPointer();
            }

            [NoInline]
            public override void SetupForExceptionHandling( uint mode )
            {
                using(Runtime.SmartHandles.InterruptState.DisableAll())
                {
                    UIntPtr stack   = this.Registers.SP;
                    uint    oldMode = GetStatusRegister();

                    //
                    // Enter target mode, with interrupts disabled.
                    //
                    SetStatusRegister( c_psr_field_c, c_psr_I | c_psr_F | mode );

                    SetRegister( Context.RegistersOnStack.StackRegister, stack );

                    //
                    // Switch back to original mode.
                    //
                    SetStatusRegister( c_psr_field_c, oldMode );
                }
            }

            [NoInline]
            public unsafe override bool Unwind()
            {
                UIntPtr    pc = this.ProgramCounter;
                TS.CodeMap cm = TS.CodeMap.ResolveAddressToCodeMap( pc );

                this.InPrologue = false;
                this.InEpilogue = false;

                if(cm != null)
                {
                    for(int i = 0; i < cm.Ranges.Length; i++)
                    {
                        TS.CodeMap.Range rng = cm.Ranges[i];

                        if((rng.Flags & TS.CodeMap.Flags.EntryPoint) != 0)
                        {
                            if((rng.Flags & TS.CodeMap.Flags.BottomOfCallStack) != 0)
                            {
                                return false;
                            }

                            UIntPtr sp                 = this.StackPointer;
                            UIntPtr address            = rng.Start;
                            uint    regRestoreMap      = 0;
                            uint    regFpRestore       = 0;
                            uint    regFpRestoreCount  = 0;
                            uint    stackAdjustment    = 0;
                            bool    fReturnAddressinLR = false;
                            bool    fDone              = false;

                            if(pc == address)
                            {
                                //
                                // We are at the beginning of a method, the return address is in LR for sure.
                                //
                                fReturnAddressinLR = true;

                                //
                                // The PC has not executed the next prologue instruction, stop processing.
                                //
                                fDone = true;

                                this.InPrologue = true;
                            }

                            if(fDone == false)
                            {
                                if((rng.Flags & TS.CodeMap.Flags.HasIntRegisterSave) != 0)
                                {
                                    uint* ptr          = (uint*)address.ToPointer();
                                    uint  opcode_STMFD = DebuggerAwareRead( ptr++ );

                                    address = new UIntPtr( ptr );

                                    if((opcode_STMFD & c_STMFD_Mask) == c_STMFD_Opcode)
                                    {
                                        regRestoreMap = opcode_STMFD & 0xFFFF;
                                    }
                                    else
                                    {
                                        //CHECKS.ASSERT( false, "Expecting a STMFD opcode, got 0x{0:X8}", opcode_STMFD );
                                        return false;
                                    }
                                }
                                else
                                {
                                    //
                                    // No register push, the return address is in LR for sure.
                                    //
                                    fReturnAddressinLR = true;
                                }
                            }

                            if(pc == address)
                            {
                                //
                                // The PC has not executed the next prologue instruction, stop processing.
                                //
                                fDone = true;

                                this.InPrologue = true;
                            }

                            if(fDone == false)
                            {
                                if((rng.Flags & TS.CodeMap.Flags.HasFpRegisterSave) != 0)
                                {
                                    uint* ptr            = (uint*)address.ToPointer();
                                    uint  opcode_FSTMFDD = DebuggerAwareRead( ptr++ );

                                    address = new UIntPtr( ptr );

                                    if((opcode_FSTMFDD & c_FSTMFDD_Mask) == c_FSTMFDD_Opcode)
                                    {
                                        regFpRestore      = ((opcode_FSTMFDD & 0x0000F000) >> 12) * 2;
                                        regFpRestoreCount =  (opcode_FSTMFDD & 0x000000FF);
                                    }
                                    else
                                    {
                                        //CHECKS.ASSERT( false, "Expecting a FSTMFDD opcode, got 0x{0:X8}", opcode_FSTMFDD );
                                        return false;
                                    }
                                }
                            }

                            if(pc == address)
                            {
                                //
                                // The PC has not executed the next prologue instruction, stop processing.
                                //
                                fDone = true;

                                this.InPrologue = true;
                            }

                            if(fDone == false)
                            {
                                if((rng.Flags & TS.CodeMap.Flags.HasStackAdjustment) != 0)
                                {
                                    uint* ptr          = (uint*)address.ToPointer();
                                    uint  opcode_SUBSP = DebuggerAwareRead( ptr );

                                    if((opcode_SUBSP & c_SUBSP_Mask) == c_SUBSP_Opcode)
                                    {
                                        stackAdjustment = s_Encoding.get_DataProcessing_ImmediateValue( opcode_SUBSP );
                                    }
                                    else
                                    {
                                        //CHECKS.ASSERT( false, "Expecting a STMFD opcode, got 0x{0:X8}", opcode_SUBSP );
                                        return false;
                                    }
                                }
                            }

                            //
                            // Deal with method epilogue: if we are on one of the return instructions, we need to restore less state.
                            //
                            uint opcode = *(uint*)pc.ToPointer();

                            if((opcode & c_FLDMFDD_Mask) == c_FLDMFDD_Opcode)
                            {
                                stackAdjustment = 0;
                            }

                            if((opcode & c_LDMFD_Mask) == c_LDMFD_Opcode)
                            {
                                stackAdjustment   = 0;
                                regFpRestoreCount = 0;
                            }

                            //--//

                            sp = AddressMath.Increment( sp, stackAdjustment );

                            if(fReturnAddressinLR)
                            {
                                this.Registers.PC = this.Registers.LR;
                            }
                            else
                            {
                                UIntPtr* src = (UIntPtr*)sp.ToPointer();

                                while(regFpRestoreCount > 0)
                                {
                                    UIntPtr* dst = (UIntPtr*)this.Registers.GetFPRegisterPointer( regFpRestore );

                                    *dst = *src++;

                                    regFpRestore++;
                                    regFpRestoreCount--;
                                }

                                for(uint idx = 0; idx < 16; idx++)
                                {
                                    if((regRestoreMap & (1u << (int)idx)) != 0)
                                    {
                                        //
                                        // Prologue saves LR, but we need to restore it as PC.
                                        //
                                        uint regIdx = (idx == RegistersOnStack.LinkRegister) ? RegistersOnStack.ProgramCounterRegister : idx;

                                        UIntPtr* dst = this.Registers.GetRegisterPointer( regIdx );

                                        *dst = *src++;
                                    }
                                }

                                sp = new UIntPtr( src );
                            }

                            this.StackPointer = sp;

                            return true;
                        }
                    }
                }

                return false;
            }

            [NoInline]
            public override void SwitchTo()
            {
                LongJump( ref this.Registers );
            }

            public override unsafe UIntPtr GetRegisterByIndex( uint idx )
            {
                return *(this.Registers.GetRegisterPointer( idx ));
            }

            public override unsafe void SetRegisterByIndex( uint    idx   ,
                                                            UIntPtr value )
            {
                *(this.Registers.GetRegisterPointer( idx )) = value;
            }

            //--//

            [NoInline]
            [SaveFullProcessorContext]
            private unsafe void GetAllTheRegisters()
            {
                this.Registers.SP = GetRegister( RegistersOnStack.StackRegister          );
                this.Registers.PC = GetRegister( RegistersOnStack.ProgramCounterRegister );

                Unwind();
            }

            //--//

            [NoInline]
            [NoReturn()]
            [HardwareExceptionHandler(HardwareException.LongJump)]
            static void LongJump( ref RegistersOnStack registers )
            {
                //
                // WARNING! 
                // WARNING! Keep this method empty!!!!
                // WARNING! 
                //
                // We need a way to make a long jump as part of the exception handling code.
                //
                // The code responsible for emitting the prologue of the method will detect that
                // this method is decorated with the ContextSwitch flag and it will generate the proper code.
                //
                // WARNING! 
                // WARNING! Keep this method empty!!!!
                // WARNING! 
                //
            }

            //--//

            static uint fault_DFSR;
            static uint fault_IFSR;
            static uint fault_FAR;

            [NoInline]
            [NoReturn()]
            [HardwareExceptionHandler(HardwareException.UndefinedInstruction)]
            [MemoryUsage(MemoryUsage.Bootstrap)]
            static void UndefinedInstruction()
            {
                fault_DFSR = MoveFromCoprocessor( 15, 0, 5, 0, 0 );
                fault_IFSR = MoveFromCoprocessor( 15, 0, 5, 0, 1 );
                fault_FAR  = MoveFromCoprocessor( 15, 0, 6, 0, 0 );

                Processor.Instance.Breakpoint();
            }

            [NoInline]
            [NoReturn()]
            [HardwareExceptionHandler(HardwareException.PrefetchAbort)]
            [MemoryUsage(MemoryUsage.Bootstrap)]
            static void PrefetchAbort()
            {
                fault_DFSR = MoveFromCoprocessor( 15, 0, 5, 0, 0 );
                fault_IFSR = MoveFromCoprocessor( 15, 0, 5, 0, 1 );
                fault_FAR  = MoveFromCoprocessor( 15, 0, 6, 0, 0 );

                Processor.Instance.Breakpoint();
            }

            [NoInline]
            [NoReturn()]
            [HardwareExceptionHandler(HardwareException.DataAbort)]
            [MemoryUsage(MemoryUsage.Bootstrap)]
            static void DataAbort()
            {
                fault_DFSR = MoveFromCoprocessor( 15, 0, 5, 0, 0 );
                fault_IFSR = MoveFromCoprocessor( 15, 0, 5, 0, 1 );
                fault_FAR  = MoveFromCoprocessor( 15, 0, 6, 0, 0 );

                Processor.Instance.Breakpoint();
            }

            //--//

            [Inline]
            public static void InterruptHandlerWithContextSwitch( ref RegistersOnStack registers )
            {
                Peripherals.Instance.ProcessInterrupt();

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
            public static void InterruptHandlerWithoutContextSwitch()
            {
                Peripherals.Instance.ProcessInterrupt();
            }

            [Inline]
            public static void FastInterruptHandlerWithoutContextSwitch()
            {
                Peripherals.Instance.ProcessFastInterrupt();
            }

            [Inline]
            public static void GenericSoftwareInterruptHandler( ref RegistersOnStack registers )
            {
            }

            //--//

            private static void ContextSwitch(     ThreadManager    tm        ,
                                               ref RegistersOnStack registers )
            {
                ThreadImpl currentThread = tm.CurrentThread;
                ThreadImpl nextThread    = tm.NextThread;
                Context    ctx;

                if(currentThread != null)
                {
                    ctx = (Context)currentThread.SwappedOutContext;

                    ctx.Registers.Assign( ref registers );
                }

                ctx = (Context)nextThread.SwappedOutContext;

                registers.Assign( ref ctx.Registers );

                tm.CurrentThread = nextThread;

                ThreadImpl.CurrentThread = nextThread;
            }

            //
            // Access Methods
            //

            public override UIntPtr StackPointer
            {
                get
                {
                    return this.Registers.SP;
                }

                set
                {
                    this.Registers.SP = value;
                }
            }

            public override UIntPtr BaseStackPointer
            {
                get
                {
                    return (UIntPtr)0;
                }
            }

            public override UIntPtr ProgramCounter
            {
                get
                {
                    return this.Registers.PC;
                }

                set
                {
                    this.Registers.PC = value;
                }
            }

            public override uint ScratchedIntegerRegisters
            {
                get
                {
                    return MethodWrapperHelpers.ScratchedRegisters();
                }
            }
        }
    }
}
