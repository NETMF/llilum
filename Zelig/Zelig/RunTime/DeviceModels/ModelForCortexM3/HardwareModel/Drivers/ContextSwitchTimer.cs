//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define TIMERS_SELF_TEST

namespace Microsoft.DeviceModels.Chipset.CortexM3.Drivers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;

    using TS    = Microsoft.Zelig.Runtime.TypeSystem;
    using RT    = Microsoft.Zelig.Runtime;
    using CMSIS = Microsoft.DeviceModels.Chipset.CortexM3;

    public abstract class ContextSwitchTimer
    {
        public delegate void Callback( SysTickTimer sysTickTimer, ulong currentTime );

        //
        // The SysTick timer could be used as a general timer, although that is not an appropriate usage
        //
        public class SysTickTimer
        {

            //
            // State
            //

            private ContextSwitchTimer m_owner;
            private uint               m_timeout;
            private Callback           m_callback;

            //
            // Constructor Methods
            //

            internal SysTickTimer( ContextSwitchTimer owner ) : this( owner, null )
            {
            }

            internal SysTickTimer( ContextSwitchTimer owner,  Callback    callback )
            {
                m_owner    = owner;
                m_callback = callback;
            }

            //
            // Helper Methods
            //

            public Callback Expired
            {
                get
                {
                    return m_callback;
                }
                set
                {
                    m_callback = value;
                }
            }

            public void Cancel()
            {
                m_owner.Disable( this );
            }
        
            public bool Muted
            {
                set
                {
                    m_owner.Muted = value;
                }
            }

            //--//
            
            internal void Invoke( ulong currentTime )
            {
                m_callback( this, currentTime );
            }

            //
            // Access Methods
            //
            
            public uint RelativeTimeout
            {
                get
                {
                    return m_timeout;
                }

                set
                {
                    m_timeout = value;

                    m_owner.Enable( this );
                }
            }

            public void Schedule()
            {
                this.RelativeTimeout = m_owner.m_reload20ms;
            }
        }

        //
        // State
        //

        /// <summary>
        /// This constant is the representation of the overhead of invoking 
        /// the timer handler. It needs to be trimmed. 
        /// // TODO: automate and/or expose to system configuration
        /// </summary>
        const uint c_InvokeOverhead = 10; 
        /// <summary>
        /// This constant is the representation of the overhead of querying 
        /// the current time. It needs to be trimmed.
        /// // TODO: automate and/or expose to system configuration
        /// </summary>
        const uint c_QueryOverhead  = 10; 

        //--//
        
        private SysTick      m_sysTick;
        private SysTickTimer m_SysTickTimer;
        private ulong        m_accumulator;
        private uint         m_latestMatch;
        private uint         m_reload20ms;
        private bool         m_enabled; 

        //
        // Helper Methods
        //

        public void Initialize()
        {
            m_sysTick      = CMSIS.SysTick.Instance;
            m_SysTickTimer = new SysTickTimer( this );
            m_accumulator  = 0;
            m_latestMatch  = 0;

            //--//

            // Reset HW, stop all SysTick interrupts 
            Disable( m_SysTickTimer );

            //--//

            //
            // The calibration value is the match value for a 10ms interrupt at 100Mhz 
            // clock, so we have to adjust it accordingly to the system frequency
            //
            // The SysTick raises an interrupt on the tick following the reload value 
            // count and we need twice the factory match value. or simpy, we can just 
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

            m_enabled = true;
        }

        protected virtual uint GetTicksForQuantumValue( uint ms )
        {
            //
            // We use SysTick and handle wrap around for values larger than 24 bit precision
            // We will assume the device can be programmed with teh calibration value from factory settings
            // TODO: need to add logic to handle the case where we cannot count in the calibration value
            //
            RT.BugCheck.Assert( HasRef() && IsPrecise(), RT.BugCheck.StopCode.FailedBootstrap );

            //
            // match = (coreclock / 100) - 1 ) * 2
            //
            return ( ( ( ( GetCoreClockMhz( ) * GetFactoryCalibrationValue( ) ) / 100 ) - 1 ) * ms) / 10; 
        }

        public SysTickTimer CreateTimer( Callback callback )
        {
            m_SysTickTimer.Expired = callback;

            return m_SysTickTimer;
        }
        
        //
        // Access Methods
        //

        public static extern ContextSwitchTimer Instance
        {
            [RT.SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public uint CurrentTimeRaw
        {
            get
            {
                return this.CounterValue;
            }
        }

        public ulong CurrentTime
        {
            get
            {
                return m_accumulator + m_latestMatch - this.CounterValue + c_QueryOverhead;
            }
        }
        
        public bool Muted
        {
            set
            {
                m_enabled = !value;
            }
        }

        //--//

        private void ProcessTimeout( )
        {
            m_accumulator += m_latestMatch;

            //
            // TODO: measure and trim invoke overhead
            //
            //if(m_enabled)
            //{
                m_SysTickTimer.Invoke( m_accumulator + c_InvokeOverhead );
            //}
        }

        //--//

        //
        // Timer behavior
        //

        private void Enable( SysTickTimer sysTickTimer )
        {
            // Clear previous interrupts 
            ResetAndClear();

            // set match
            SetMatchAndStart( sysTickTimer.RelativeTimeout );
        }

        private void Disable( SysTickTimer sysTickTimer )
        {
            m_sysTick.Enabled = false;
        }
        
        //--//

        //
        // SysTick helpers
        //
        
        private uint CounterValue
        {
            [RT.Inline]
            get
            {
                return m_sysTick.Counter;
            }
        }

        [RT.Inline]
        private void SetMatchAndStart( uint match )
        {
            // sync state
            m_latestMatch = match;

            // 
            // Restarting causes the match value to be picked up
            // 
            m_sysTick.Match   = match;
            m_sysTick.Counter = 0;
            m_sysTick.Enabled = true; 

        }

        [RT.Inline]
        private void ResetAndClear( )
        {
            m_sysTick.ResetAndClear();
        }

        [RT.Inline]
        private uint GetCoreClockMhz( )
        {
            return (uint)( m_sysTick.SystemCoreClock / 1000000 ); 
        }

        [RT.Inline]
        private uint GetFactoryCalibrationValue( )
        {
            return m_sysTick.TenMillisecondsCalibrationValue;
        }
        
        private bool RanToZero( )
        {
            return m_sysTick.HasMatched;
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
        
        //[RT.BottomOfCallStack( )]
        //[RT.HardwareExceptionHandler( RT.HardwareException.SoftwareInterrupt )] // TODO: dfine PendSV instead?
        [RT.ExportedMethod]
        //[TS.WellKnownMethod("Hardware_InvokeSysTickHandler")]
        private void SysTick_Handler( )
        {
            using(RT.SmartHandles.InterruptState.Disable())
            {
                ContextSwitchTimer.Instance.ProcessTimeout( );
            }
        }
    }
}
