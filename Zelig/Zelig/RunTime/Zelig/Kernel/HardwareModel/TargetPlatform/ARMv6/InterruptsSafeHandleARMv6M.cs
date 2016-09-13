//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv6.SmartHandles
{
    using System;

    using ISA = TargetModel.ArmProcessor.InstructionSetVersion;


    [ExtendClass( typeof(Runtime.SmartHandles.InterruptState), PlatformVersionFilter=ISA.Platform_Version__ARMv6M )]
    public struct InterruptStateARMv6M : IDisposable
    {
        //
        // State
        //

        private static ProcessorARMv6M.ISR_NUMBER s_softwareExceptionMode = ProcessorARMv6M.ISR_NUMBER.SVCall;
        //--//
        private uint m_basepri;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptStateARMv6M( uint basepri )
        {
            m_basepri = basepri;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            ProcessorARMv6M.SetPriMaskRegister( m_basepri );
        }

        [Inline]
        public void Toggle()
        {
            uint basepri = ProcessorARMv6M.SetPriMaskRegister( m_basepri );
            ProcessorARMv6MForLlvm.Nop();
            ProcessorARMv6M.SetPriMaskRegister( basepri );
        }

        //--//

        [Inline]
        public static InterruptStateARMv6M Disable()
        {
            return new InterruptStateARMv6M( ProcessorARMv6M.DisableInterrupts( ) );
        }

        [Inline]
        public static InterruptStateARMv6M DisableAll( )
        {
            return new InterruptStateARMv6M( ProcessorARMv6M.DisableInterrupts( ) );
        }

        [Inline]
        public static InterruptStateARMv6M Enable()
        {
            return new InterruptStateARMv6M( ProcessorARMv6M.EnableInterrupts( ) );
        }

        [Inline]
        public static InterruptStateARMv6M EnableAll( )
        {
            return new InterruptStateARMv6M( ProcessorARMv6M.EnableInterrupts( ) );
        }

        //
        // Access Methods
        //

        [Inline]
        public uint GetPreviousState()
        {
            return m_basepri;
        }

        public HardwareException GetCurrentExceptionMode()
        {
            ProcessorARMv6M.ISR_NUMBER ex = GetMode();
            
            if(ex == ProcessorARMv6M.ISR_NUMBER.ThreadMode)
            {
                return HardwareException.None;
            }

            switch(ex)
            {
                case ProcessorARMv6M.ISR_NUMBER.NMI             : return HardwareException.NMI; 
                case ProcessorARMv6M.ISR_NUMBER.HardFault       : return HardwareException.Fault; 
                case ProcessorARMv6M.ISR_NUMBER.SVCall          : return HardwareException.Service;
                case ProcessorARMv6M.ISR_NUMBER.ReservedForDebug: return HardwareException.Debug;
                case ProcessorARMv6M.ISR_NUMBER.PendSV          : return HardwareException.PendSV;
                case ProcessorARMv6M.ISR_NUMBER.SysTick         : return HardwareException.SysTick;
                case ProcessorARMv6M.ISR_NUMBER.Reset           :
                case ProcessorARMv6M.ISR_NUMBER.Reserved4       :
                case ProcessorARMv6M.ISR_NUMBER.Reserved5       :
                case ProcessorARMv6M.ISR_NUMBER.Reserved6       :
                case ProcessorARMv6M.ISR_NUMBER.Reserved7       :
                case ProcessorARMv6M.ISR_NUMBER.Reserved8       :
                case ProcessorARMv6M.ISR_NUMBER.Reserved9       :
                case ProcessorARMv6M.ISR_NUMBER.Reserved10      :
                case ProcessorARMv6M.ISR_NUMBER.Reserved13      : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                    
                default                                         : return HardwareException.Interrupt;
            }

            return HardwareException.Interrupt;
        }

        //--//

        private ProcessorARMv6M.ISR_NUMBER GetMode( )
        {
            return s_softwareExceptionMode | (ProcessorARMv6M.ISR_NUMBER)(ProcessorARMv6M.CMSIS_STUB_SCB__get_IPSR( ) & 0x1FF);
        }

        [Inline]
        internal static void SetSoftwareExceptionMode( )
        {
            s_softwareExceptionMode = ProcessorARMv6M.ISR_NUMBER.SVCall;
        }

        [Inline]
        internal static void ResetSoftwareExceptionMode( )
        {
            s_softwareExceptionMode = ProcessorARMv6M.ISR_NUMBER.ThreadMode;
        }
    }
}
