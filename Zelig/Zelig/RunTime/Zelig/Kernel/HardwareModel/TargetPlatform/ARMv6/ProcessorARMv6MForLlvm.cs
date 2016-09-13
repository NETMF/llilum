//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define SPIN_ON_SLEEP

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv6
{
    using System;
    using System.Runtime.InteropServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;
    using RT = Microsoft.Zelig.Runtime;


    public abstract partial class ProcessorARMv6MForLlvm : ProcessorARMv6M
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

        [TS.WellKnownType( "Microsoft_Zelig_ARMv6ForLlvm_MethodWrapper" )]
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
        
        [Inline]
        protected static unsafe UIntPtr GetMainStackPointerAtReset()
        {
            return new UIntPtr(CMSIS_STUB_SCB__get_MSP_ResetValue());
        }

        internal static UIntPtr GetMainStackPointerBottom()
        {
            UIntPtr stackBottom = AddressMath.Decrement(GetMainStackPointerAtReset(), GetMainStackSize());

            return stackBottom;
        }

        [Inline]
        protected static uint GetMainStackSize()
        {
            return CMSIS_STUB_SCB__get_MSP_StackSize();
        }

        [Inline]
        protected static UIntPtr GetMainStackPointer( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_MSP() );
        }
            
        [Inline]
        protected static void SetMainStackPointer( UIntPtr topOfMainStack )
        {
            CMSIS_STUB_SCB__set_MSP( topOfMainStack.ToUInt32() );
        }
            
        [Inline]
        protected static UIntPtr GetProcessStackPointer( )
        {
            return new UIntPtr( CMSIS_STUB_SCB__get_PSP( ) );
        }
            
        [Inline]
        protected static void SetProcessStackPointer( UIntPtr topOfProcessStack )
        {
            CMSIS_STUB_SCB__set_PSP( topOfProcessStack.ToUInt32() );
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

        protected static void SetExcReturn( uint ret )
        {
            CUSTOM_STUB_SetExcReturn( ret );
        }
        
        //
        // Fault diagnostic
        //

        protected static uint GetProgramCounter()
        {
            return CUSTOM_STUB_GetProgramCounter( ); 
        }
        
        protected static bool DebuggerConnected()
        {
            return false; 
        }
        
        [RT.Inline]
        protected static unsafe StandardFrame* PointerToStdFrame( UIntPtr SP )
        {
            return (StandardFrame*)SP.ToPointer( );
        }

        //
        // All overridable exceptions
        //

        //////[RT.BottomOfCallStack( )]
        //////[RT.HardwareExceptionHandler( RT.HardwareException.NMI )]
        //////[RT.ExportedMethod]
        //////private static void NMI_Handler( )
        //////{
        //////    //
        //////    // The processor clears the FAULTMASK bit to 0 on exit from any exception handler except the NMI handler.
        //////    //

        //////    EnableFaults( );
        //////}

        /// <summary>
        /// Hard Fault is caused by Bus Fault, Memory Management Fault, or Usage Fault if their handler 
        /// cannot be executed.
        /// </summary>
        [RT.CapabilitiesFilter(RequiredCapabilities = TargetModel.ArmProcessor.InstructionSetVersion.Platform_Version__ARMv6M)]
        [RT.HardwareExceptionHandler( RT.HardwareException.Fault )]
        [RT.ExportedMethod]
        private static void HardFault_Handler_Zelig( uint sp )
        {
            while(true)
            {
                Peripherals.Instance.WaitForInterrupt();
            }
        }
        
        //--//

        [DllImport( "C" )]
        protected static extern unsafe uint* CUSTOM_STUB_FetchSoftwareFrameSnapshot( );

        //--//
        //--//
        //--//
        
        //
        // Utility methods 
        //
        
        [DllImport( "C" )]
        public static extern void Nop( );
    }
}
