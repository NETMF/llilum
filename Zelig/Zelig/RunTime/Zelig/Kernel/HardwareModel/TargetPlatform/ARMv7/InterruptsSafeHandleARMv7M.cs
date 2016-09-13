//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv7.SmartHandles
{
    using System;

    using ISA = TargetModel.ArmProcessor.InstructionSetVersion;


    [ExtendClass( typeof(Runtime.SmartHandles.InterruptState), PlatformVersionFilter=ISA.Platform_Version__ARMv7M )]
    public struct InterruptStateARMv7M : IDisposable
    {
        //
        // State
        //
        
        uint m_basepri;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptStateARMv7M( uint basepri )
        {
            m_basepri = basepri;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            ProcessorARMv7M.DisableInterruptsWithPriorityLevelLowerOrEqualTo( m_basepri );
        }

        [Inline]
        public void Toggle()
        {
            uint basepri = ProcessorARMv7M.SetBasePriRegister( m_basepri );
            ProcessorARMv7MForLlvm.Nop();
            ProcessorARMv7M.SetBasePriRegister( basepri );
        }

        //--//

        [Inline]
        public static InterruptStateARMv7M Disable()
        {
            return new InterruptStateARMv7M( ProcessorARMv7M.DisableInterrupts( ) );
        }

        [Inline]
        public static InterruptStateARMv7M DisableAll( )
        {
            return new InterruptStateARMv7M( ProcessorARMv7M.DisableInterrupts( ) );
        }

        [Inline]
        public static InterruptStateARMv7M Enable()
        {
            return new InterruptStateARMv7M( ProcessorARMv7M.EnableInterrupts( ) );
        }

        [Inline]
        public static InterruptStateARMv7M EnableAll( )
        {
            return new InterruptStateARMv7M( ProcessorARMv7M.EnableInterrupts( ) );
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
            ProcessorARMv7M.ISR_NUMBER ex = GetMode();
            
            if(ex == ProcessorARMv7M.ISR_NUMBER.ThreadMode)
            {
                return HardwareException.None;
            }

            switch(ex)
            {
                case ProcessorARMv7M.ISR_NUMBER.NMI             : return HardwareException.NMI; 
                case ProcessorARMv7M.ISR_NUMBER.HardFault       : return HardwareException.Fault; 
                case ProcessorARMv7M.ISR_NUMBER.MemManage       : return HardwareException.Fault;
                case ProcessorARMv7M.ISR_NUMBER.BusFault        : return HardwareException.Fault;
                case ProcessorARMv7M.ISR_NUMBER.UsageFault      : return HardwareException.Fault;
                case ProcessorARMv7M.ISR_NUMBER.SVCall          : return HardwareException.Service;
                case ProcessorARMv7M.ISR_NUMBER.ReservedForDebug: return HardwareException.Debug;
                case ProcessorARMv7M.ISR_NUMBER.PendSV          : return HardwareException.PendSV;
                case ProcessorARMv7M.ISR_NUMBER.SysTick         : return HardwareException.SysTick;
                case ProcessorARMv7M.ISR_NUMBER.Reset           :
                case ProcessorARMv7M.ISR_NUMBER.Reserved7       :
                case ProcessorARMv7M.ISR_NUMBER.Reserved8       :
                case ProcessorARMv7M.ISR_NUMBER.Reserved9       :
                case ProcessorARMv7M.ISR_NUMBER.Reserved10      :
                case ProcessorARMv7M.ISR_NUMBER.Reserved13      : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                    
                default                                         : return HardwareException.Interrupt;
            }

            return HardwareException.Interrupt;
        }

        //--//

        private ProcessorARMv7M.ISR_NUMBER GetMode( )
        {
            return (ProcessorARMv7M.ISR_NUMBER)(ProcessorARMv7M.CMSIS_STUB_SCB__get_IPSR( ) & 0x1FF);
        }
    }
}
