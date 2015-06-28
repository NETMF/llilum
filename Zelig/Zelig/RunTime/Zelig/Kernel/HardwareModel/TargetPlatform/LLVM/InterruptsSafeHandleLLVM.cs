//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.LLVM.SmartHandles
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass( typeof(Runtime.SmartHandles.InterruptState) )]
    public struct InterruptStateLLVM : IDisposable
    {
        //
        // State
        //
        
        uint m_primask;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptStateLLVM( uint primask )
        {
            m_primask = primask;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            ProcessorLLVM.SetMode( m_primask );
        }

        [Inline]
        public void Toggle()
        {
            uint primask = ProcessorLLVM.GetPriMaskRegister( );

            ProcessorLLVM.SetMode( m_primask );
            ProcessorLLVM.Nop( );
            ProcessorLLVM.SetMode( primask );
        }

        //--//

        [Inline]
        public static InterruptStateLLVM Disable()
        {
            return new InterruptStateLLVM( ProcessorLLVM.DisableInterrupts( ) );
        }

        //[Inline]
        //public static InterruptStateLLVM DisableAll()
        //{
        //    return new InterruptStateLLVM( ProcessorLLVM.DisableAllInterrupts( ) );
        //}

        [Inline]
        public static InterruptStateLLVM Enable()
        {
            return new InterruptStateLLVM( ProcessorLLVM.EnableInterrupts( ) );
        }

        //[Inline]
        //public static InterruptStateLLVM EnableAll()
        //{
        //    return new InterruptStateLLVM( ProcessorLLVM.EnableAllInterrupts( ) );
        //}

        //
        // Access Methods
        //

        [Inline]
        public uint GetPreviousState()
        {
            return m_primask;
        }

        public HardwareException GetCurrentExceptionMode()
        {
            throw new NotImplementedException( "GetCurrentExceptionMode not NotImplementedException for LLVM" );
            //switch( m_cpsr & ProcessorLLVM.c_psr_mode )
            //{
            //    case ProcessorARMv4.c_psr_mode_FIQ  : return HardwareException.FastInterrupt;
            //    case ProcessorARMv4.c_psr_mode_IRQ  : return HardwareException.Interrupt;
            //    case ProcessorARMv4.c_psr_mode_SVC  : return HardwareException.SoftwareInterrupt;
            //    case ProcessorARMv4.c_psr_mode_ABORT: return HardwareException.DataAbort;
            //}

            // return HardwareException.None;
        }
    }
}
