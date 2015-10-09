//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv4
{
    using System;

    using TS     = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;


    public abstract partial class ProcessorARMv4 : Processor
    {
        public const int c_psr_bit_T       = EncDef.c_psr_bit_T;
        public const int c_psr_bit_F       = EncDef.c_psr_bit_F;
        public const int c_psr_bit_I       = EncDef.c_psr_bit_I;
        public const int c_psr_bit_V       = EncDef.c_psr_bit_V;
        public const int c_psr_bit_C       = EncDef.c_psr_bit_C;
        public const int c_psr_bit_Z       = EncDef.c_psr_bit_Z;
        public const int c_psr_bit_N       = EncDef.c_psr_bit_N;


        public const uint c_psr_T          = EncDef.c_psr_T;
        public const uint c_psr_F          = EncDef.c_psr_F;
        public const uint c_psr_I          = EncDef.c_psr_I;
        public const uint c_psr_V          = EncDef.c_psr_V;
        public const uint c_psr_C          = EncDef.c_psr_C;
        public const uint c_psr_Z          = EncDef.c_psr_Z;
        public const uint c_psr_N          = EncDef.c_psr_N;

        //--//

        public const uint c_psr_mode       = EncDef.c_psr_mode;
        public const uint c_psr_mode_USER  = EncDef.c_psr_mode_USER;
        public const uint c_psr_mode_FIQ   = EncDef.c_psr_mode_FIQ;
        public const uint c_psr_mode_IRQ   = EncDef.c_psr_mode_IRQ;
        public const uint c_psr_mode_SVC   = EncDef.c_psr_mode_SVC;
        public const uint c_psr_mode_ABORT = EncDef.c_psr_mode_ABORT;
        public const uint c_psr_mode_UNDEF = EncDef.c_psr_mode_UNDEF;
        public const uint c_psr_mode_SYS   = EncDef.c_psr_mode_SYS;

        public const uint c_psr_field_c    = EncDef.c_psr_field_c;
        public const uint c_psr_field_x    = EncDef.c_psr_field_x;
        public const uint c_psr_field_s    = EncDef.c_psr_field_s;
        public const uint c_psr_field_f    = EncDef.c_psr_field_f;
        public const uint c_psr_field_ALL  = EncDef.c_psr_field_ALL;

        //--//

        public const uint c_InterruptsOff    = EncDef.c_psr_I;
        public const uint c_AllInterruptsOff = EncDef.c_psr_I | EncDef.c_psr_F;

        public static class MethodWrapperHelpers
        {
            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_ScratchedRegisters" )]
            public static extern uint ScratchedRegisters();

            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PushRegisters" )]
            public static extern void PushRegisters( uint indexRegister           ,
                                                     bool fWriteBackIndexRegister ,
                                                     bool fAddComputedRegisters   ,
                                                     uint registerMask            );

            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PopRegisters" )]
            public static extern void PopRegisters( uint indexRegister           ,
                                                    bool fWriteBackIndexRegister ,
                                                    bool fAddComputedRegisters   ,
                                                    bool fRestoreSPSR            ,
                                                    uint registerMask            );

            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PushStackFrame" )]
            public static extern void PushStackFrame();

            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv4_MethodWrapperHelpers_PopStackFrame" )]
            public static extern void PopStackFrame();

            [Inline]
            public static void AdjustLinkAddress( uint offset )
            {
                ProcessorARMv4.SetRegister( EncDef.c_register_lr, AddressMath.Decrement( ProcessorARMv4.GetRegister( EncDef.c_register_lr ), offset ) );
            }

            [Inline]
            public static void PushStackPointer( uint offset )
            {
                ProcessorARMv4.SetRegister( EncDef.c_register_sp, AddressMath.Decrement( ProcessorARMv4.GetRegister( EncDef.c_register_sp ), offset ) );
            }

            [Inline]
            public static void PopStackPointer( uint offset )
            {
                ProcessorARMv4.SetRegister( EncDef.c_register_sp, AddressMath.Increment( ProcessorARMv4.GetRegister( EncDef.c_register_sp ), offset ) );
            }
        }

        [TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv4_MethodWrapper" )]
        public sealed class MethodWrapper : AbstractMethodWrapper
        {
            public const uint DisableInterruptsMask = EncDef.c_psr_I | EncDef.c_psr_F;

            private const uint ExtraRegistersSize = 3 * sizeof(uint);


            [Inline]
            [DisableNullChecks(ApplyRecursively=true)]
            public override void Prologue( string                                      typeFullName   ,
                                           string                                      methodFullName ,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs        )
            {
                AddActivationRecordEvent( ActivationRecordEvents.Constructing ); 

                if((attribs & TS.MethodRepresentation.BuildTimeAttributes.StackNotAvailable) == 0)
                {
                    MethodWrapperHelpers.PushRegisters( EncDef.c_register_sp, true, true, EncDef.c_register_lst_lr );

                    MethodWrapperHelpers.PushStackFrame();
                }

                AddActivationRecordEvent( ActivationRecordEvents.ReadyForUse ); 
            }

            [Inline]
            [DisableNullChecks(ApplyRecursively=true)]
            public unsafe override void Prologue( string                                      typeFullName   ,
                                                  string                                      methodFullName ,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs        ,
                                                  HardwareException                           he             )
            {
                AddActivationRecordEvent( ActivationRecordEvents.EnteringException );

                AddActivationRecordEvent( ActivationRecordEvents.Constructing ); 

                switch(he)
                {
                    case HardwareException.UndefinedInstruction: MethodWrapperHelpers.AdjustLinkAddress( 4 ); break; // Point to the undefined instruction.
                    case HardwareException.PrefetchAbort       : MethodWrapperHelpers.AdjustLinkAddress( 4 ); break;
                    case HardwareException.DataAbort           : MethodWrapperHelpers.AdjustLinkAddress( 8 ); break;
    
                    case HardwareException.Interrupt           : MethodWrapperHelpers.AdjustLinkAddress( 4 ); break;
                    case HardwareException.FastInterrupt       : MethodWrapperHelpers.AdjustLinkAddress( 4 ); break;
                    case HardwareException.SoftwareInterrupt   :                                              break; // Skip the SWI instruction.
                }
    
                switch(he)
                {
                    case HardwareException.VectorTable:
                    case HardwareException.Bootstrap:
                    case HardwareException.LongJump:
                    case HardwareException.Reset:
                        //
                        // No prologue for these.
                        //
                        AddActivationRecordEvent( ActivationRecordEvents.ReadyForUse ); 
                        return;
                }
    
                MethodWrapperHelpers.PushRegisters( EncDef.c_register_sp, true, true, EncDef.c_register_lst_lr );
    
                if((attribs & TS.MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext) != 0)
                {
                    MethodWrapperHelpers.PushStackPointer( ExtraRegistersSize );
                }
    
                var ptr = (Context.RegistersOnStack*)GetRegister( EncDef.c_register_sp ).ToPointer();
    
                MethodWrapperHelpers.PushStackFrame();
    
                if((attribs & TS.MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext) != 0)
                {
                    switch(he)
                    {
                        case HardwareException.UndefinedInstruction: PrepareStackForException( EncDef.c_psr_mode_UNDEF, ptr ); break;
                        case HardwareException.PrefetchAbort       : PrepareStackForException( EncDef.c_psr_mode_ABORT, ptr ); break;
                        case HardwareException.DataAbort           : PrepareStackForException( EncDef.c_psr_mode_ABORT, ptr ); break;
        
                        case HardwareException.Interrupt           : PrepareStackForException( EncDef.c_psr_mode_IRQ  , ptr ); break;
                        case HardwareException.FastInterrupt       : PrepareStackForException( EncDef.c_psr_mode_FIQ  , ptr ); break;
                        case HardwareException.SoftwareInterrupt   : PrepareStackForException( EncDef.c_psr_mode_SYS  , ptr ); break;
                    }
                }
    
                AddActivationRecordEvent( ActivationRecordEvents.ReadyForUse ); 
            }

            [Inline]
            [DisableNullChecks(ApplyRecursively=true)]
            public override void Epilogue( string                                      typeFullName   ,
                                           string                                      methodFullName ,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs        )
            {
                AddActivationRecordEvent( ActivationRecordEvents.ReadyForTearDown ); 

                if((attribs & TS.MethodRepresentation.BuildTimeAttributes.NoReturn) == 0)
                {
                    MethodWrapperHelpers.PopStackFrame();

                    AddActivationRecordEvent( ActivationRecordEvents.ReturnToCaller ); 

                    MethodWrapperHelpers.PopRegisters( EncDef.c_register_sp, true, true, false, EncDef.c_register_lst_pc );

                    AddActivationRecordEvent( ActivationRecordEvents.NonReachable ); 
                }
                else
                {
                    //
                    // The method is not supposed to return, but we have to put something here,
                    // because the stack crawling code will look for this spot during stack unwinding...
                    //
                    Breakpoint( 0x1 );

                    AddActivationRecordEvent( ActivationRecordEvents.NonReachable ); 
                }
            }

            [Inline]
            [DisableNullChecks(ApplyRecursively=true)]
            public unsafe override void Epilogue( string                                      typeFullName   ,
                                                  string                                      methodFullName ,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs        ,
                                                  HardwareException                           he             )
            {
                AddActivationRecordEvent( ActivationRecordEvents.ReadyForTearDown ); 

                switch(he)
                {
                    case HardwareException.VectorTable:
                    case HardwareException.Bootstrap:
                    case HardwareException.Reset:
                        return;

                    case HardwareException.LongJump:
                        {
                            var ptr = (Context.RegistersOnStack*)GetRegister( EncDef.c_register_r1 ).ToPointer();

                            SetRegister( EncDef.c_register_sp, ptr->SP                 );
                            SetRegister( EncDef.c_register_lr, ptr->LR                 );
                            SetRegister( EncDef.c_register_r1, new UIntPtr( &ptr->R0 ) );

                            AddActivationRecordEvent( ActivationRecordEvents.LongJump );

                            MethodWrapperHelpers.PopRegisters( EncDef.c_register_r1, false, true, false, EncDef.c_register_lst_pc );

                            AddActivationRecordEvent( ActivationRecordEvents.NonReachable );
                        }
                        return;

                    default:
                        {
                            MethodWrapperHelpers.PopStackFrame();

                            if((attribs & TS.MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext) != 0)
                            {
                                var ptr = (Context.RegistersOnStack*)GetRegister( EncDef.c_register_sp ).ToPointer();
                    
                                switch(he)
                                {
                                    case HardwareException.UndefinedInstruction: RestoreStackForException( EncDef.c_psr_mode_UNDEF, ptr ); break;
                                    case HardwareException.PrefetchAbort       : RestoreStackForException( EncDef.c_psr_mode_ABORT, ptr ); break;
                                    case HardwareException.DataAbort           : RestoreStackForException( EncDef.c_psr_mode_ABORT, ptr ); break;
                    
                                    case HardwareException.Interrupt           : RestoreStackForException( EncDef.c_psr_mode_IRQ  , ptr ); break;
                                    case HardwareException.FastInterrupt       : RestoreStackForException( EncDef.c_psr_mode_FIQ  , ptr ); break;
                                    case HardwareException.SoftwareInterrupt   : RestoreStackForException( EncDef.c_psr_mode_SYS  , ptr ); break;
                                }

                                MethodWrapperHelpers.PopStackPointer( ExtraRegistersSize );
                            }

                            AddActivationRecordEvent( ActivationRecordEvents.ReturnFromException );

                            MethodWrapperHelpers.PopRegisters( EncDef.c_register_sp, true, true, true, EncDef.c_register_lst_pc );

                            AddActivationRecordEvent( ActivationRecordEvents.NonReachable ); 
                        }
                        return;
                }
            }

            [Inline]
            private static unsafe void PrepareStackForException( uint                      mode ,
                                                                 Context.RegistersOnStack* ptr  )
            {
                //
                // Keep interrupts disabled.
                //
                mode |= DisableInterruptsMask;

                //
                // Get CSPR of the calling context.
                //
                var CPSR = GetSavedStatusRegister();

                //
                // Save it.
                //
                ptr->CPSR = CPSR;

                //
                // Disable interrupts.
                //
                CPSR |= DisableInterruptsMask;

                //
                // Go back to the previous mode.
                //
                SetStatusRegister( EncDef.c_psr_field_c, CPSR );

                //
                // Save unbanked R13 and R14.
                //
                ptr->LR = GetRegister( EncDef.c_register_lr );
                ptr->SP = GetRegister( EncDef.c_register_sp );

                //
                // Switch back to the exception handling mode.
                //
                SetStatusRegister( EncDef.c_psr_field_c, mode );

                //
                // R1 should point to the Register Context on the stack.
                //
                SetRegister( EncDef.c_register_r1, new UIntPtr( ptr ) );
            }

            [Inline]
            private static unsafe void RestoreStackForException( uint                      mode ,
                                                                 Context.RegistersOnStack* ptr  )
            {
                //
                // Keep interrupts disabled.
                //
                mode |= DisableInterruptsMask;

                //
                // Get CSPR of the calling context.
                //
                var CPSR = ptr->CPSR;

                //
                // Restore the SPSR
                //
                SetSavedStatusRegister( EncDef.c_psr_field_ALL, CPSR );

                //
                // Disable interrupts.
                //
                CPSR |= DisableInterruptsMask;

                //
                // Go back to the previous mode.
                //
                SetStatusRegister( EncDef.c_psr_field_c, CPSR );

                //
                // Save unbanked R13 and R14.
                //
                SetRegister( EncDef.c_register_lr, ptr->LR );
                SetRegister( EncDef.c_register_sp, ptr->SP );

                //
                // Switch back to the exception handling mode.
                //
                SetStatusRegister( EncDef.c_psr_field_c, mode );
            }
        }

        //--//

        //
        // Helper Methods
        //

        [TS.WellKnownMethod( "ProcessorARM_SetRegister" )]
        public static extern void SetRegister( uint    reg   ,
                                               UIntPtr value );

        [TS.WellKnownMethod( "ProcessorARM_GetRegister" )]
        public static extern UIntPtr GetRegister( uint reg );

        [TS.WellKnownMethod( "ProcessorARM_SetSystemRegister" )]
        public static extern void SetSystemRegister( uint reg   ,
                                                     uint value );

        [TS.WellKnownMethod( "ProcessorARM_GetSystemRegister" )]
        public static extern uint GetSystemRegister( uint reg );

        //--//

        [TS.WellKnownMethod( "ProcessorARM_SetStatusRegister" )]
        public static extern void SetStatusRegister( uint fields ,
                                                     uint value  );

        [TS.WellKnownMethod( "ProcessorARM_GetStatusRegister" )]
        public static extern uint GetStatusRegister();

        [TS.WellKnownMethod( "ProcessorARM_SetSavedStatusRegister" )]
        public static extern void SetSavedStatusRegister( uint fields ,
                                                          uint value  );

        [TS.WellKnownMethod( "ProcessorARM_GetSavedStatusRegister" )]
        public static extern uint GetSavedStatusRegister();

        //--//

        [TS.WellKnownMethod( "ProcessorARM_MoveToCoprocessor" )]
        public static extern void MoveToCoprocessor( uint CpNum ,
                                                     uint Op1   ,
                                                     uint CRn   ,
                                                     uint CRm   ,
                                                     uint Op2   ,
                                                     uint value );


        [TS.WellKnownMethod( "ProcessorARM_MoveFromCoprocessor" )]
        public static extern uint MoveFromCoprocessor( uint CpNum ,
                                                       uint Op1   ,
                                                       uint CRn   ,
                                                       uint CRm   ,
                                                       uint Op2   );

        [TS.WellKnownMethod( "ProcessorARM_Breakpoint" )]
        public static extern void Breakpoint( uint value );

        //--//

        public override void FlushCacheLine( UIntPtr target )
        {
        }

        //--//--//

        [Inline]
        public override bool AreInterruptsDisabled()
        {
            uint cpsr = GetStatusRegister();

            return (cpsr & c_InterruptsOff) == c_InterruptsOff;
        }

        public override bool AreAllInterruptsDisabled()
        {
            uint cpsr = GetStatusRegister();

            return (cpsr & c_AllInterruptsOff) == c_AllInterruptsOff;
        }

        //--//--//

        [Inline]
        public static uint DisableInterrupts()
        {
            uint cpsr = GetStatusRegister();

            SetStatusRegister( c_psr_field_c, cpsr | c_InterruptsOff );

            return cpsr;
        }

        [Inline]
        public static uint DisableAllInterrupts()
        {
            uint cpsr = GetStatusRegister();

            SetStatusRegister( c_psr_field_c, cpsr | c_AllInterruptsOff );

            return cpsr;
        }

        //--//

        [Inline]
        public static uint EnableInterrupts()
        {
            uint cpsr = GetStatusRegister();

            SetStatusRegister( c_psr_field_c, cpsr & ~c_InterruptsOff );

            return cpsr;
        }

        [Inline]
        public static uint EnableAllInterrupts()
        {
            uint cpsr = GetStatusRegister();

            SetStatusRegister( c_psr_field_c, cpsr & ~c_AllInterruptsOff );

            return cpsr;
        }

        //--//

        [Inline]
        public override void Breakpoint()
        {
            Breakpoint( 0 );
        }

        //--//

        [Inline]
        public static void SetMode( uint cpsr )
        {
            SetStatusRegister( c_psr_field_c, cpsr );
        }

        [NoInline]
        [MemoryUsage(MemoryUsage.Bootstrap)]
        public static void Nop()
        {
            //
            // Do-nothing method just to flush the pipeline.
            //
        }

        //--//

        [NoReturn()]
        [BottomOfCallStack()]
        [HardwareExceptionHandler(HardwareException.Bootstrap)]
        private static void Bootstrap()
        {
            //
            // WARNING! 
            // WARNING! Keep this method empty!!!!
            // WARNING! 
            //
            // We need a way to bootstrap code generation for the entrypoint into the system, which is not a regular piece of code. 
            // It's only composed of a single absolute branch to the entrypoint.
            //
            // The code responsible for emitting the opcode for the return operator will detect that
            // this method is decorated with the Bootstrap flag and it will generate the proper code.
            //
            // WARNING! 
            // WARNING! Keep this method empty!!!!
            // WARNING! 
            //
        }

        [NoInline]
        [NoReturn()]
        [BottomOfCallStack()]
        [HardwareExceptionHandler(HardwareException.VectorTable)]
        private static void VectorsTable()
        {
            //
            // WARNING! 
            // WARNING! Keep this method empty!!!!
            // WARNING! 
            //
            // We need a way to bootstrap code generation for the vector table, which is not a regular piece of code. 
            // It's only composed of branches and the location of the branches is mandated by the hardware.
            //
            // The code responsible for emitting the opcode for the return operator will detect that
            // this method is decorated with the VectorTable flag and it will generate the proper code.
            //
            // WARNING! 
            // WARNING! Keep this method empty!!!!
            // WARNING! 
            //
        }

        //--//

        static readonly SoftBreakpointDescriptor[] s_softBreakpointTable = new SoftBreakpointDescriptor[128];

        [NoInline]
        [BottomOfCallStack]
        [MemoryUsage( MemoryUsage.Bootstrap )]
        [DebuggerHookHandler( DebuggerHook.GetSoftBreakpointTable )]
        static SoftBreakpointDescriptor[] GetSoftBreakpointTable()
        {
            SoftBreakpointDescriptor[] res = s_softBreakpointTable;

            Processor.Instance.Breakpoint();

            return res;
        }

        public static unsafe uint DebuggerAwareRead( uint* ptr )
        {
            uint val = *ptr;

            if(val == 0xE1200070) // BKPT opcode
            {
                UIntPtr address = new UIntPtr( ptr );

                for(int i = 0; i < s_softBreakpointTable.Length; i++)
                {
                    var address2 = s_softBreakpointTable[i].Address;

                    if(address2 == new UIntPtr( 0 ))
                    {
                        break;
                    }

                    if(address2 == address)
                    {
                        return s_softBreakpointTable[i].Value;
                    }
                }
            }

            return val;
        }
    }
}
