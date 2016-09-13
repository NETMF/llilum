//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define SPIN_ON_SLEEP

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7
{
    using System;
    using System.Runtime.InteropServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition_ARM;
    using RT = Microsoft.Zelig.Runtime;
    using ARMv4;
    using TargetModel.ArmProcessor;

    public abstract partial class ProcessorARMv7MForLlvm : ProcessorARMv7M
    {
        public struct StandardFrame
        {
            public UIntPtr R0;
            public UIntPtr R1;
            public UIntPtr R2;
            public UIntPtr R3;
            public UIntPtr R12;
            public UIntPtr LR;
            public UIntPtr PC;
            public UIntPtr PSR;
        }

        //--//

        [TS.WellKnownType( "Microsoft_Zelig_ARMv7ForLlvm_MethodWrapper" )]
        public sealed class MethodWrapperLlvm : AbstractMethodWrapper
        {

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public override void Prologue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Prologue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  HardwareException he )
            {

            }

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public override void Epilogue( string typeFullName,
                                           string methodFullName,
                                           TS.MethodRepresentation.BuildTimeAttributes attribs )
            {

            }

            [Inline]
            [DisableNullChecks( ApplyRecursively = true )]
            public unsafe override void Epilogue( string typeFullName,
                                                  string methodFullName,
                                                  TS.MethodRepresentation.BuildTimeAttributes attribs,
                                                  HardwareException he )
            {

            }

        }

        //
        // Helper Methods
        //

        //--//

        [DllImport( "C" )]
        public static extern void Nop( );

        //--//

        [Inline]
        internal static unsafe UIntPtr GetMainStackPointerAtReset( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_MSP_ResetValue( ) );
        }

        internal static UIntPtr GetMainStackPointerBottom( )
        {
            UIntPtr stackBottom = AddressMath.Decrement(GetMainStackPointerAtReset(), GetMainStackSize());

            return stackBottom;
        }

        [Inline]
        internal static uint GetMainStackSize( )
        {
            return CMSIS_STUB_SCB__get_MSP_StackSize( );
        }

        [Inline]
        internal static UIntPtr GetMainStackPointer( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_MSP( ) );
        }

        [Inline]
        internal static void SetMainStackPointer( UIntPtr topOfMainStack )
        {
            CMSIS_STUB_SCB__set_MSP( topOfMainStack.ToUInt32( ) );
        }

        [Inline]
        internal static UIntPtr GetProcessStackPointer( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_PSP( ) );
        }

        [Inline]
        internal static void SetProcessStackPointer( UIntPtr topOfProcessStack )
        {
            CMSIS_STUB_SCB__set_PSP( topOfProcessStack.ToUInt32( ) );
        }

        protected static void SwitchToHandlerPrivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            control &= ~c_CONTROL__MASK;
            control |=  c_CONTROL__MODE__HNDLR;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }

        protected static void SwitchToThreadUnprivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            control &= ~c_CONTROL__MASK;
            control |= c_CONTROL__MODE__THRD_UNPRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }

        protected static void SwitchToThreadPrivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            control &= ~c_CONTROL__MASK;
            control |= c_CONTROL__MODE__THRD_PRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }

        protected static void SwitchToPrivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            control &= ~c_CONTROL__MASK_PRIVILEGE;
            control |=  c_CONTROL__nPRIV_PRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }

        protected static void SwitchToUnprivilegedMode( )
        {
            uint control = CMSIS_STUB_SCB__get_CONTROL( );

            control &= ~c_CONTROL__MASK_PRIVILEGE;
            control |=  c_CONTROL__nPRIV_UNPRIV;

            CMSIS_STUB_SCB__set_CONTROL( control & c_CONTROL__MASK );
        }

        internal static void SetExcReturn( uint ret )
        {
            CUSTOM_STUB_SetExcReturn( ret );
        }

        //--//

        #region Fault handlers helpers

        //
        // Fault diagnostic
        //

        protected static bool WasHardFaultForced( )
        {
            return (CUSTOM_STUB_SCB__get_HFSR( ) & c_SCB_HFSR_FORCED_FORCED) == c_SCB_HFSR_FORCED_FORCED;
        }

        protected static bool WasHardFaultOnVectorTableRead( )
        {
            return (CUSTOM_STUB_SCB__get_HFSR( ) & c_SCB_HFSR_VECTTBL_READ) == c_SCB_HFSR_VECTTBL_READ;
        }

        protected static bool IsMemFaultAddressValid( )
        {
            return (CUSTOM_STUB_SCB__get_CFSR( ) & c_SCB_CFSR_MEMFAULT_MMFARVALID) == c_SCB_CFSR_MEMFAULT_MMFARVALID;
        }

        protected static bool IsBusFaultAddressValid( )
        {
            return (CUSTOM_STUB_SCB__get_CFSR( ) & c_SCB_CFSR_BUSFAULT_BFARVALID) == c_SCB_CFSR_BUSFAULT_BFARVALID;
        }

        protected static bool IsBusFaultAddressPrecise( )
        {
            return (CUSTOM_STUB_SCB__get_CFSR( ) & c_SCB_CFSR_BUSFAULT_PRECISERR) == c_SCB_CFSR_BUSFAULT_PRECISERR;
        }

        protected static uint GetProgramCounter( )
        {
            return CUSTOM_STUB_GetProgramCounter( );
        }

        //--//

        [RT.Inline]
        protected static unsafe StandardFrame* PointerToStdFrame( UIntPtr SP )
        {
            return (StandardFrame*)SP.ToPointer( );
        }

        /// <summary>
        /// Hard Fault handler.
        /// </summary>
        protected static void HandleHardFault( )
        {
            if(IsDebuggerConnected)
            {
                if(WasHardFaultOnVectorTableRead( ))
                {
                    BugCheck.Raise( BugCheck.StopCode.Fault_Vectors );
                }

                if(WasHardFaultForced( ))
                {
                    BugCheck.Raise( BugCheck.StopCode.ForcedHardFault );
                }

                // TODO
                BugCheck.Raise( BugCheck.StopCode.Fault_Unknown );
            }
            else
            {
                while(true)
                {
                    Peripherals.Instance.WaitForInterrupt( );
                }
            }
        }

        /// <summary>
        /// Detects memory access violations to regions that are defined in the Memory Management Unit (MPU). 
        /// For example code execution from a memory region with read/write access only.
        /// </summary>
        protected static void HandleMemoryAccessFault( ref StandardFrame registers )
        {
            uint CFSR = CUSTOM_STUB_SCB__get_CFSR( );

            BugCheck.Log( "CFSR =0x%08x", (int)CFSR );
            BugCheck.Log( "MMFAR=0x%08x", (int)CUSTOM_STUB_SCB__get_MMFAR( ) );
            BugCheck.Log( "PC   =0x%08x", (int)registers.PC.ToUInt32( ) );

            if((CFSR & c_SCB_CFSR_MEMFAULT_MSTKERR) != 0)
            {
                BugCheck.Log( "Memory access fault on exception entry" );
            }
            if((CFSR & c_SCB_CFSR_MEMFAULT_MUNSTKERR) != 0)
            {
                BugCheck.Log( "Memory access fault on exception exit" );
            }
            if((CFSR & c_SCB_CFSR_MEMFAULT_DACCVIOL) != 0)
            {
                BugCheck.Log( "Data access violation" );
            }
            if((CFSR & c_SCB_CFSR_MEMFAULT_IACCVIOL) != 0)
            {
                BugCheck.Log( "Instruction access violation" );
            }

            if(IsMemFaultAddressValid( ))
            {
                BugCheck.Log( "Mem Fault Address=0x%08x", (int)CUSTOM_STUB_SCB__get_MMFAR( ) );
                Breakpoint( CUSTOM_STUB_SCB__get_MMFAR( ) );
            }
            else
            {
                BugCheck.Log( "Invalid Mem Fault Address" );
                Breakpoint( CFSR );
            }
        }

        /// <summary>
        /// Detects memory access errors on instruction fetch, data read/write, interrupt vector fetch, and 
        /// register stacking (save/restore) on interrupt (entry/exit).
        /// </summary>
        protected static void HandleBusFault( ref StandardFrame registers )
        {
            uint CFSR = CUSTOM_STUB_SCB__get_CFSR( );

            BugCheck.Log( "CFSR=0x%08x", (int)CFSR );
            BugCheck.Log( "BFAR=0x%08x", (int)CUSTOM_STUB_SCB__get_BFAR( ) );
            BugCheck.Log( "PC  =0x%08x", (int)registers.PC.ToUInt32( ) );

            if((CFSR & c_SCB_CFSR_BUSFAULT_STKERR) != 0)
            {
                BugCheck.Log( "Stacking error on entry" );
            }
            if((CFSR & c_SCB_CFSR_BUSFAULT_UNSTKERR) != 0)
            {
                BugCheck.Log( "Stacking error on exit" );
            }
            if((CFSR & c_SCB_CFSR_BUSFAULT_IBUSERR) != 0)
            {
                BugCheck.Log( "Prefetch abort" );
            }

            if(IsBusFaultAddressValid( ) && IsBusFaultAddressPrecise( ))
            {
                BugCheck.Log( "Bus Fault Address=0x%08x", (int)CUSTOM_STUB_SCB__get_BFAR( ) );
                Breakpoint( CUSTOM_STUB_SCB__get_BFAR( ) );
            }
            else
            {
                BugCheck.Log( "Invalid or imprecise Bus Fault Address" );
                Breakpoint( CFSR );
            }
        }

        /// <summary>
        /// Detects execution of undefined instructions, unaligned memory access for load/store multiple. 
        /// When enabled, divide-by-zero and other unaligned memory accesses are also detected.
        /// </summary>
        protected static void HandleUsageFault( ref StandardFrame registers )
        {
            uint CFSR = CUSTOM_STUB_SCB__get_CFSR( );

            BugCheck.Log( "CFSR=0x%08x", (int)CFSR );
            BugCheck.Log( "PC  =0x%08x", (int)registers.PC.ToUInt32( ) );

            if((CFSR & c_SCB_CFSR_USGFAULT_DIVBYZERO) != 0)
            {
                BugCheck.Log( "Divide by zero" );
            }
            if((CFSR & c_SCB_CFSR_USGFAULT_UNALIGNED) != 0)
            {
                BugCheck.Log( "Unaligned access" );
            }
            if((CFSR & c_SCB_CFSR_USGFAULT_INVPC) != 0)
            {
                BugCheck.Log( "Invalid PC load on EXC_RETURN" );
            }
            if((CFSR & c_SCB_CFSR_USGFAULT_NOPC) != 0)
            {
                BugCheck.Log( "No coprocessor" );
            }
            if((CFSR & c_SCB_CFSR_USGFAULT_INVSTATE) != 0)
            {
                BugCheck.Log( "Illegal use of the EPSR" );
            }
            if((CFSR & c_SCB_CFSR_USGFAULT_UNDEFINSTR) != 0)
            {
                BugCheck.Log( "Undefined instruction" );
            }
        }

        #endregion

        #region Fault handlers

        /// <summary>
        /// Hard Fault is caused by Bus Fault, Memory Management Fault, or Usage Fault if their handler 
        /// cannot be executed.
        /// </summary>
        [RT.CapabilitiesFilter( RequiredCapabilities = TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv7M )]
        [RT.HardwareExceptionHandler( RT.HardwareException.Fault )]
        [RT.ExportedMethod]
        private static void Zelig_Exception_HardFault_Handler( uint sp )
        {
            HandleHardFault( );
        }

        [RT.CapabilitiesFilter( RequiredCapabilities = TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv7M )]
        [RT.HardwareExceptionHandler( RT.HardwareException.Fault )]
        [RT.ExportedMethod]
        private static unsafe void Zelig_Exception_MemManage_Handler( uint sp )
        {
            StandardFrame* regs = PointerToStdFrame( new UIntPtr( sp ) );

            HandleMemoryAccessFault( ref *regs );
        }

        [RT.CapabilitiesFilter( RequiredCapabilities = TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv7M )]
        [RT.HardwareExceptionHandler( RT.HardwareException.Fault )]
        [RT.ExportedMethod]
        private static unsafe void Zelig_Exception_UsageFault_Handler( uint sp )
        {
            StandardFrame* regs = PointerToStdFrame( new UIntPtr( sp ) );

            HandleUsageFault( ref *regs );
        }

        [RT.CapabilitiesFilter( RequiredCapabilities = TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv7M )]
        [RT.HardwareExceptionHandler( RT.HardwareException.Fault )]
        [RT.ExportedMethod]
        private static unsafe void Zelig_Exception_BusFault_Handler( uint sp )
        {
            StandardFrame* regs = PointerToStdFrame( new UIntPtr( sp ) );

            HandleBusFault( ref *regs );
        }

        #endregion
    }
}

