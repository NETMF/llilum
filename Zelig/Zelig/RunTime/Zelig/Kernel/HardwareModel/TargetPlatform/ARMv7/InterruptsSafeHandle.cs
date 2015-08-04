//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7.SmartHandles
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass( typeof(Runtime.SmartHandles.InterruptState) )]
    public struct InterruptState : IDisposable
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
        public InterruptState( uint primask )
        {
            m_primask = primask;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            ProcessorARMv7M.SetMode( m_primask );
        }

        [Inline]
        public void Toggle()
        {
            uint primask = ProcessorARMv7M.GetPriMaskRegister( );

            ProcessorARMv7M.SetMode( m_primask );
            ProcessorARMv7M.Nop( );
            ProcessorARMv7M.SetMode( primask );
        }

        //--//

        [Inline]
        public static InterruptState Disable()
        {
            return new InterruptState( ProcessorARMv7M.DisableInterrupts( ) );
        }

        [Inline]
        public static InterruptState DisableAll( )
        {
            return new InterruptState( ProcessorARMv7M.DisableAllInterrupts( ) );
        }

        [Inline]
        public static InterruptState Enable()
        {
            return new InterruptState( ProcessorARMv7M.EnableInterrupts( ) );
        }

        [Inline]
        public static InterruptState EnableAll( )
        {
            return new InterruptState( ProcessorARMv7M.EnableAllInterrupts( ) );
        }

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
            return (HardwareException)GetPreviousState( );

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
