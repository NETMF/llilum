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
        
        uint m_basepri;

        //
        // Constructor Methods
        //

        [DiscardTargetImplementation()]
        [Inline]
        public InterruptState( uint basepri )
        {
            m_basepri = basepri;
        }

        //
        // Helper Methods
        //

        [Inline]
        public void Dispose()
        {
            ProcessorARMv7M.DisableInterruptsWithPriorityLowerOrEqualTo( m_basepri );
        }

        [Inline]
        public void Toggle()
        {
            uint basepri = ProcessorARMv7M.GetBasePriRegister();

            ProcessorARMv7M.SetBasePriRegister( m_basepri );
            ProcessorARMv7M.Nop               (           );
            ProcessorARMv7M.SetBasePriRegister(   basepri );
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
            return new InterruptState( ProcessorARMv7M.DisableInterrupts( ) );
        }

        [Inline]
        public static InterruptState Enable()
        {
            return new InterruptState( ProcessorARMv7M.EnableInterrupts( ) );
        }

        [Inline]
        public static InterruptState EnableAll( )
        {
            return new InterruptState( ProcessorARMv7M.EnableInterrupts( ) );
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
                case ProcessorARMv7M.ISR_NUMBER.Reset           : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                case ProcessorARMv7M.ISR_NUMBER.NMI             : return HardwareException.NMI; 
                case ProcessorARMv7M.ISR_NUMBER.HardFault       : return HardwareException.Fault; 
                case ProcessorARMv7M.ISR_NUMBER.MemManage       : return HardwareException.Fault;
                case ProcessorARMv7M.ISR_NUMBER.BusFault        : return HardwareException.Fault;
                case ProcessorARMv7M.ISR_NUMBER.UsageFault      : return HardwareException.Fault;
                case ProcessorARMv7M.ISR_NUMBER.Reserved7       : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                case ProcessorARMv7M.ISR_NUMBER.Reserved8       : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                case ProcessorARMv7M.ISR_NUMBER.Reserved9       : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                case ProcessorARMv7M.ISR_NUMBER.Reserved10      : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                case ProcessorARMv7M.ISR_NUMBER.SVCall          : return HardwareException.Service;
                case ProcessorARMv7M.ISR_NUMBER.ReservedForDebug: return HardwareException.Debug;
                case ProcessorARMv7M.ISR_NUMBER.Reserved13      : BugCheck.Assert( false, BugCheck.StopCode.IllegalMode ); break;
                case ProcessorARMv7M.ISR_NUMBER.PendSV          : return HardwareException.SoftwareInterrupt;
                case ProcessorARMv7M.ISR_NUMBER.SysTick         : return HardwareException.SoftwareInterrupt;
                    
                default                                         : return HardwareException.Interrupt;
            }

            return HardwareException.Interrupt;
        }

        //--//

        private ProcessorARMv7M.ISR_NUMBER GetMode( )
        {
            return (ProcessorARMv7M.ISR_NUMBER)(ProcessorARMv7M.CMSIS_STUB_SCB__get_IPSR( ) & 0xFF);
        }
    }
}
