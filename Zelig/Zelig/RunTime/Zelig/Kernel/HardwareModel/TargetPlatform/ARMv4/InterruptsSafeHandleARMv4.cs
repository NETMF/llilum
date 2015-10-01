//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.SmartHandles
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    // Removed class extension as it interferes with InterruptState.
    // BUGBUG: ColinA-MSFT: We should detect and report at compile-time when two extended methods collide.
    //[ExtendClass( typeof(Runtime.SmartHandles.InterruptState) )]
    public struct InterruptStateARMv4 : IDisposable
    {
        //
        // State
        //

        uint m_cpsr;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptStateARMv4( uint cpsr )
        {
            m_cpsr = cpsr;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            ProcessorARMv4.SetMode( m_cpsr );
        }

        [Inline]
        public void Toggle()
        {
            uint cpsr = ProcessorARMv4.GetStatusRegister();

            ProcessorARMv4.SetMode( m_cpsr );
            ProcessorARMv4.Nop    (        );
            ProcessorARMv4.SetMode(   cpsr );
        }

        //--//

        [Inline]
        public static InterruptStateARMv4 Disable()
        {
            return new InterruptStateARMv4( ProcessorARMv4.DisableInterrupts() );
        }

        [Inline]
        public static InterruptStateARMv4 DisableAll()
        {
            return new InterruptStateARMv4( ProcessorARMv4.DisableAllInterrupts() );
        }

        [Inline]
        public static InterruptStateARMv4 Enable()
        {
            return new InterruptStateARMv4( ProcessorARMv4.EnableInterrupts() );
        }

        [Inline]
        public static InterruptStateARMv4 EnableAll()
        {
            return new InterruptStateARMv4( ProcessorARMv4.EnableAllInterrupts() );
        }

        //
        // Access Methods
        //

        [Inline]
        public uint GetPreviousState()
        {
            return m_cpsr;
        }

        public HardwareException GetCurrentExceptionMode()
        {
            switch(m_cpsr & ProcessorARMv4.c_psr_mode)
            {
                case ProcessorARMv4.c_psr_mode_FIQ  : return HardwareException.FastInterrupt;
                case ProcessorARMv4.c_psr_mode_IRQ  : return HardwareException.Interrupt;
                case ProcessorARMv4.c_psr_mode_SVC  : return HardwareException.SoftwareInterrupt;
                case ProcessorARMv4.c_psr_mode_ABORT: return HardwareException.DataAbort;
            }

            return HardwareException.None;
        }
    }
}
