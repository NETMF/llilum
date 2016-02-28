//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TIMERS_SELF_TEST

namespace Microsoft.DeviceModels.Chipset.CortexM.Drivers
{
    using System.Runtime.CompilerServices;

    using RT    = Microsoft.Zelig.Runtime;
    using CMSIS = Microsoft.DeviceModels.Chipset.CortexM;
    using LLOS  = Zelig.LlilumOSAbstraction.HAL;

    public abstract class ContextSwitchTimer
    {        
        public delegate void Callback();
   
        /// <summary>
        /// Max value that can be assigned for a one shot timer with no wrap around
        /// </summary>
        public const uint c_MaxCounterValue = 0x00FFFFFF;

        //--//

        //
        // State
        //
        
        //--//
        
        private SysTick      m_sysTick;
        private uint         m_reload20ms;
        private uint         m_timeout;
        private bool         m_enabled; 

        //
        // Helper Methods
        //

        //
        // Access Methods
        //

        public static extern ContextSwitchTimer Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
        
        //--//

        public void Initialize()
        {
            m_sysTick = CMSIS.SysTick.Instance;

            //--//

            // Reset HW, stop all SysTick interrupts 
            Cancel();

            //--//

            //
            // The calibration value is the match value for a 10ms interrupt at 100Mhz 
            // clock, so we have to adjust it accordingly to the system frequency
            //
            // The SysTick raises an interrupt on the tick following the reload value 
            // count and we need twice the factory match value. or simply, we can just 
            // count the ticks in a 20 ms interval given the processor core frequency 

            m_reload20ms = GetTicksForQuantumValue( RT.ARMv7ThreadManager.c_TimeQuantumMsec );

#if TIMERS_SELF_TEST
            ulong now = this.CurrentTime;
            ulong last = now;

            int testCount = 20;
            var testTimer = this.CreateTimer(
                    delegate ( SysTickTimer timer, ulong currentTime )
                    {   
                        testCount--;

                        RT.BugCheck.Log( "[handler] ct=0x%08x%08x, delta=0x%08x%08x",
                            (int)( (currentTime         >> 32) & 0xFFFFFFFF),
                            (int)(  currentTime                & 0xFFFFFFFF),
                            (int)(((currentTime - last) >> 32) & 0xFFFFFFFF),
                            (int)(((currentTime - last) >>  0) & 0xFFFFFFFF)
                            );

                        last = currentTime;
                    } );
                
            testTimer.RelativeTimeout = m_reload20ms; 
                
            while( testCount > 0 )
            {
                //
                // Enable primask in the debugger to fire the exception
                //
            }
#endif
        }

        public void Schedule( uint timeout_ms )
        {
            SetMatchAndStart( GetTicksForQuantumValue( timeout_ms ) );
        }

        public void Cancel( )
        {
            m_sysTick.Enabled = false;
            m_enabled = false;
        }

        public void Reset( )
        {
            if(m_enabled)
            {
                // If the timer is already enabled, then only the counter needs to be
                // reset.
                m_sysTick.ResetAndClear();
            }
            else
            {
                SetMatchAndStart( m_reload20ms );
            }
        }

        //--//

        protected virtual uint GetTicksForQuantumValue( uint ms )
        {
            //
            // We use SysTick and handle wrap around for values larger than 24 bit precision
            // We will assume the device can be programmed with the calibration value from factory settings
            // TODO: need to add logic to handle the case where we cannot count in the calibration value
            //
            RT.BugCheck.Assert( HasRef() && IsPrecise(), RT.BugCheck.StopCode.FailedBootstrap );

            //
            // match = ( (timerClockMhz * calibration_x10 / 100) - 1 ) * ms ) / 10
            //
            return ( ( ( ( GetTimerClockMhz( ) * GetFactoryCalibrationValue( ) ) / 100 ) - 1 ) * ms) / 10;
        }
        
        //--//

        //
        // SysTick helpers
        //
        
        [RT.Inline]
        private void SetMatchAndStart( uint match )
        {
            // 
            // Restarting causes the match value to be picked up
            // 
            m_sysTick.Match   = match;
            m_sysTick.Counter = 0;
            m_sysTick.Enabled = true; 
            m_enabled = true;
        }

        [RT.Inline]
        private unsafe uint GetTimerClockMhz( )
        {
            return (uint)(LLOS.Timer.LLOS_SYSTEM_TIMER_GetTimerFrequency( null ) / 1000000); 
        }

        [RT.Inline]
        private uint GetFactoryCalibrationValue( )
        {
            return m_sysTick.TenMillisecondsCalibrationValue;
        }
        
        [RT.Inline]
        private bool IsPrecise( )
        {
            return m_sysTick.IsPrecise;
        }
        
        [RT.Inline]
        private bool HasRef( )
        {
            return m_sysTick.HasRef;
        }

        //--//
        
        [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        [RT.ExportedMethod]
        private static void SysTick_Handler_Zelig( )
        {
            using(RT.SmartHandles.InterruptState.Disable())
            {
                RT.ThreadManager.Instance.TimeQuantumExpired( );
            }
        }
    }
}
