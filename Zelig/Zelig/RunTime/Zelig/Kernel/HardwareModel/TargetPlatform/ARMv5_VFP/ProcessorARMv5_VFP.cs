//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv5
{
    using System;
    
    using              Microsoft.Zelig.TargetModel.ArmProcessor;
    using TS         = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef     = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using EncDef_VFP = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_VFP_ARM;


    public abstract partial class ProcessorARMv5_VFP : ARMv4.ProcessorARMv4
    {
        public static class MethodWrapperHelpers_VFP
        {
            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv5_VFP_MethodWrapperHelpers_PushFpRegisters" )]
            public static extern void PushFpRegisters( uint indexRegister           ,
                                                       bool fWriteBackIndexRegister ,
                                                       bool fAddComputedRegisters   ,
                                                       uint registerLow             ,
                                                       uint registerHigh            );

            [TS.WellKnownMethod( "Microsoft_Zelig_ProcessorARMv5_VFP_MethodWrapperHelpers_PopFpRegisters" )]
            public static extern void PopFpRegisters( uint indexRegister           ,
                                                      bool fWriteBackIndexRegister ,
                                                      bool fAddComputedRegisters   ,
                                                      uint registerLow             ,
                                                      uint registerHigh            );
        }

        [TS.WellKnownType( "Microsoft_Zelig_ProcessorARMv5_VFP_MethodWrapper" )]
        public sealed class MethodWrapper_VFP : AbstractMethodWrapper
        {
            public const uint DisableInterruptsMask = EncDef.c_psr_I | EncDef.c_psr_F;

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

                    MethodWrapperHelpers_VFP.PushFpRegisters( EncDef.c_register_sp, true, true, uint.MaxValue, uint.MinValue );

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

                MethodWrapperHelpers_VFP.PushFpRegisters( EncDef.c_register_sp, true, true, uint.MaxValue, uint.MinValue );
    
                if((attribs & TS.MethodRepresentation.BuildTimeAttributes.SaveFullProcessorContext) != 0)
                {
                    MethodWrapperHelpers.PushStackPointer( 5 * sizeof(uint) );
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

                    MethodWrapperHelpers_VFP.PopFpRegisters( EncDef.c_register_sp, true, true, uint.MaxValue, uint.MinValue );

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

                            SetSystemRegister( EncDef_VFP.c_register_FPSCR, ptr->FPSCR              );
                            SetRegister      ( EncDef    .c_register_sp   , ptr->SP                 );
                            SetRegister      ( EncDef    .c_register_lr   , ptr->LR                 );
                            SetRegister      ( EncDef    .c_register_r1   , new UIntPtr( &ptr->S0 ) );

                            AddActivationRecordEvent( ActivationRecordEvents.LongJump );

                            MethodWrapperHelpers_VFP.PopFpRegisters( EncDef.c_register_r1, true , true       , uint.MaxValue, uint.MinValue );

                            MethodWrapperHelpers    .PopRegisters  ( EncDef.c_register_r1, false, true, false, EncDef.c_register_lst_pc     );

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

                                MethodWrapperHelpers.PopStackPointer( 5 * sizeof(uint) );
                            }

                            AddActivationRecordEvent( ActivationRecordEvents.ReturnFromException );

                            MethodWrapperHelpers_VFP.PopFpRegisters( EncDef.c_register_sp, true, true, uint.MaxValue, uint.MinValue );

                            MethodWrapperHelpers.PopRegisters( EncDef.c_register_sp, true, true, true, EncDef.c_register_lst_pc     );

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
                // Save unbanked R13 and R14 plus FP status.
                //
                ptr->FPSCR = GetSystemRegister( EncDef_VFP.c_register_FPSCR );
                ptr->FPEXC = GetSystemRegister( EncDef_VFP.c_register_FPEXC );
                ptr->LR    = GetRegister      ( EncDef    .c_register_lr    );
                ptr->SP    = GetRegister      ( EncDef    .c_register_sp    );

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
                // Save unbanked R13 and R14 plus FP status.
                //
                SetSystemRegister( EncDef_VFP.c_register_FPSCR, ptr->FPSCR );
                SetSystemRegister( EncDef_VFP.c_register_FPEXC, ptr->FPEXC );
                SetRegister      ( EncDef    .c_register_lr   , ptr->LR    );
                SetRegister      ( EncDef    .c_register_sp   , ptr->SP    );

                //
                // Switch back to the exception handling mode.
                //
                SetStatusRegister( EncDef.c_psr_field_c, mode );
            }
        }

        //
        // Helper Methods
        //

        [TS.WellKnownMethod( "ProcessorARM_SetRegisterFP32" )]
        public static extern void SetRegisterFP32( uint  reg   ,
                                                   float value );

        [TS.WellKnownMethod( "ProcessorARM_SetRegisterFP64" )]
        public static extern void SetRegisterFP64( uint   reg   ,
                                                   double value );

        [TS.WellKnownMethod( "ProcessorARM_GetRegisterFP32" )]
        public static extern float GetRegisterFP32( uint reg );

        [TS.WellKnownMethod( "ProcessorARM_GetRegisterFP64" )]
        public static extern double GetRegisterFP64( uint reg );

        //--//

        public override unsafe void FlushCacheLine( UIntPtr target )
        {
            ARMv4.Coprocessor15.CleanDCache     ( target );
            ARMv4.Coprocessor15.DrainWriteBuffer(        );
            ARMv4.Coprocessor15.InvalidateICache( target );
        }

        //--//

        [NoInline]
        public static void EnableRunFastMode()
        {
            SetSystemRegister( EncDef_VFP.c_register_FPEXC, EncDef_VFP.c_fpexc_EN );

            uint fpscr = GetSystemRegister( EncDef_VFP.c_register_FPSCR );

            fpscr &= ~(EncDef_VFP.c_fpscr_IOE |
                       EncDef_VFP.c_fpscr_DZE |
                       EncDef_VFP.c_fpscr_OFE |
                       EncDef_VFP.c_fpscr_UFE |
                       EncDef_VFP.c_fpscr_IXE |
                       EncDef_VFP.c_fpscr_IDE );

            fpscr |= EncDef_VFP.c_fpscr_DN | EncDef_VFP.c_fpscr_FZ;

            SetSystemRegister( EncDef_VFP.c_register_FPSCR, fpscr );
        }

        //--//

        protected static void InitializeCache()
        {
            ARMv4.Coprocessor15.InvalidateICache();

            ARMv4.Coprocessor15.InvalidateDCache();

            //
            // Enable ICache
            //
            ARMv4.Coprocessor15.SetControlRegisterBits( ARMv4.Coprocessor15.c_ControlRegister__ICache );
        }
    }
}
