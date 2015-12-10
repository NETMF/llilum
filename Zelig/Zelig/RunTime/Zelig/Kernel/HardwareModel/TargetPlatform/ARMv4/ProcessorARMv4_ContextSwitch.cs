//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv4
{
    using System;
    using System.Runtime.InteropServices;

    using          Microsoft.Zelig.TargetModel.ArmProcessor;
    using TS     = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;


    public abstract partial class ProcessorARMv4 : Processor
    {
        protected static EncDef s_Encoding = (EncodingDefinition_ARM)CurrentInstructionSetEncoding.GetEncoding();

        //--//

        public abstract new class Context : Processor.Context
        {
            //
            // WARNING: Don't assume the actual layout of the structure is sequential!!!
            // WARNING: The code generator rearranges the fields to minimize the cost of a context switch!!!
            //
            [TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv4_RegistersOnStack" )]
            [StructLayout( LayoutKind.Sequential, Pack = 4 )]
            public struct RegistersOnStack
            {
                public const uint StackRegister          = EncDef.c_register_sp;
                public const uint LinkRegister           = EncDef.c_register_lr;
                public const uint ProgramCounterRegister = EncDef.c_register_pc;

                //
                // State
                //

                [TS.AssumeReferenced] public uint    CPSR;
                [TS.AssumeReferenced] public UIntPtr SP;
                [TS.AssumeReferenced] public UIntPtr LR;

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

                public void Assign( ref RegistersOnStack other )
                {
////                fixed(RegistersOnStack* dst = &this)
////                {
////                    fixed(RegistersOnStack* src = &other)
////                    {
////                        UIntPtr dstStart = new UIntPtr( dst );
////                        UIntPtr srcStart = new UIntPtr( src );
////                        UIntPtr srcEnd   = AddressMath.Increment( srcStart, (uint)Marshal.SizeOf( typeof(RegistersOnStack) ) );
////
////                        Memory.CopyNonOverlapping( srcStart, srcEnd, dstStart );
////                    }
////                }
                    this.R0   = other.R0;
                    this.R1   = other.R1;
                    this.R2   = other.R2;
                    this.R3   = other.R3;
                    this.R4   = other.R4;
                    this.R5   = other.R5;
                    this.R6   = other.R6;
                    this.R7   = other.R7;
                    this.R8   = other.R8;
                    this.R9   = other.R9;
                    this.R10  = other.R10;
                    this.R11  = other.R11;
                    this.R12  = other.R12;
                    this.SP   = other.SP;
                    this.LR   = other.LR;
                    this.PC   = other.PC;
                    this.CPSR = other.CPSR;
                }
            }

            //
            // State
            //

            const uint c_STMFD_Mask     = 0xFFFF0000;
            const uint c_STMFD_Opcode   = 0xE92D0000;

            const uint c_SUBSP_Mask     = 0xFFFFF000;
            const uint c_SUBSP_Opcode   = 0xE24DD000;

            //--//

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
    
                this.Registers.CPSR = c_psr_mode_SYS;
                this.Registers.PC   = new UIntPtr( dlgImpl.InnerGetCodePointer().Target.ToPointer() );
                this.Registers.SP   = new UIntPtr( stackImpl.GetEndDataPointer()                    );
                this.Registers.R0   = objImpl.ToPointer();
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

                            //--//

                            //
                            // Deal with method epilogue: if we are on one of the return instructions, we need to restore less state.
                            //
                            uint opcode = *(uint*)pc.ToPointer();

                            if((opcode & c_LDMFD_Mask) == c_LDMFD_Opcode)
                            {
                                stackAdjustment = 0;
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
