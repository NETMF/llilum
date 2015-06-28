//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.DeviceModels.Chipset.PXA27x.Drivers
{
    using System;
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime;

    /// <summary>
    /// Driver for the PXA271 GPIO subsystem.
    /// </summary>
    public abstract class GPIO
    {
        #region Constructor/Destructor
        /// <summary>
        /// protected constructor for the GPIO class.
        /// use the singleton to retrieve the GPIO driver instance.
        /// </summary>
        /// <seealso cref="Instance"/>
        protected GPIO()
        {
        }

        /// <summary>
        /// Initializes the driver, connects interrupts, and enables the GPIO hardware
        /// in the PXA271.
        /// </summary>
        public void Initialize()
        {
            //reservation array for the ports available on the pxa271
            m_reservation = new Port[120]; //#Ports on the PXA27x

            var intcHi = InterruptController.Instance;

            //wire dedicated pin 0 interrupt to our handler
            m_interrupt0 = InterruptController.Handler.Create( PXA27x.InterruptController.IRQ_INDEX_GPIO0, InterruptPriority.Normal, InterruptSettings.RisingEdge, this.ProcessGPIOInterruptX );
            intcHi.RegisterAndEnable( m_interrupt0 );

            //wire dedicated pin 1 interrupt to our handler
            m_interrupt1 = InterruptController.Handler.Create( PXA27x.InterruptController.IRQ_INDEX_GPIO1, InterruptPriority.Normal, InterruptSettings.RisingEdge, this.ProcessGPIOInterruptX );
            intcHi.RegisterAndEnable( m_interrupt1 );

            //wire pin 2-120 interrupt to our handler
            m_interruptX = InterruptController.Handler.Create( PXA27x.InterruptController.IRQ_INDEX_GPIOx, InterruptPriority.Normal, InterruptSettings.RisingEdge, this.ProcessGPIOInterruptX );
            intcHi.RegisterAndEnable( this.m_interruptX );

            //enable GPIO pins
            PowerManager.Instance.PSSR.RDH = true;
        }
        #endregion

        #region Public API
        #region port reservation management
        /// <summary>
        /// Reserves a port.
        /// </summary>
        /// <remarks>
        /// If the hardware is already reserved by some other port,
        /// an ArgumentException will be thrown.
        /// </remarks>
        /// <param name="port">The port to reserve.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReservePort( Port port )
        {
            MustBeFree( port );

            m_reservation[port.Id] = port;
        }

        /// <summary>
        /// Releases a port from the GPIO driver.
        /// </summary>
        /// <param name="port">The port to release</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ReleasePort( Port port )
        {
            MustBeReserved( port );

            m_reservation[port.Id] = null;
        }
        #endregion

        private void MustBeFree( Port port )
        {
            if(port == null)
            {
                throw new ArgumentNullException( "port" );
            }

            var port2 = m_reservation[port.Id];
            if(port2 != null && port2 != port)
            {
                throw new ArgumentOutOfRangeException( "port" );
            }
        }

        private void MustBeReserved( Port port )
        {
            if(port == null)
            {
                throw new ArgumentNullException( "port" );
            }

            if(m_reservation[port.Id] != port)
            {
                throw new ArgumentOutOfRangeException( "port" );
            }
        }

        #region interrupt API
        /// <summary>
        /// Configures the interrupt associated with the port.
        /// </summary>
        /// <remarks>
        /// The PXA271 supports edge interrupts only.
        /// </remarks>
        /// <param name="port">The port for which to configure the interrupts</param>
        /// <param name="raisingEdge">if true, raising edges are detected as interrups and forwarded to the port.</param>
        /// <param name="fallingEdge">if true, falling edges are detected as interrups and forwarded to the port.</param>
        public void ConfigureInterrupt( Port port        ,
                                        bool raisingEdge ,
                                        bool fallingEdge )
        {
            ReservePort( port );

            var gpio = PXA27x.GPIO.Instance;

            gpio.ConfigureInterrupt( port.Id, raisingEdge, fallingEdge );
        }
        #endregion

        #region configuration API
        /// <summary>
        /// Configures the given port to one of its #ernate functions.
        /// </summary>
        /// <remarks>
        /// A port can either work as a standard GPIO port or run in one
        /// of several alternative configurations, depending on the pin.
        /// After configuring the port to the alternative function, standard GPIO
        /// operations should no longer be performed and result in unpredictable
        /// behavior of the GPIO pin.
        /// </remarks>
        /// <param name="port">The port to configure.</param>
        /// <param name="mode">The alternate function to assign to this port.</param>
        public void EnableAsInputAlternateFunction( Port port ,
                                                    int  mode )
        {
            ReservePort( port );

            var gpio = PXA27x.GPIO.Instance;

            gpio.ConfigureInterrupt            ( port.Id, false, false );
            gpio.EnableAsInputAlternateFunction( port.Id, mode         );
        }

        /// <summary>
        /// Configures the given port to one of its primary function, i.e. as
        /// a GPIO input pin.
        /// </summary>
        /// <param name="port">The port to configure.</param>
        public void EnableAsInputPin( Port port )
        {
            ReservePort( port );

            var gpio = PXA27x.GPIO.Instance;

            gpio.ConfigureInterrupt( port.Id, false, false );
            gpio.EnableAsInputPin  ( port.Id               );
        }

        /// <summary>
        /// Configures the given port to one of its alternate functions.
        /// </summary>
        /// <remarks>
        /// A port can either work as a standard GPIO port or run in one
        /// of several alternative configurations, depending on the pin.
        /// After configuring the port to the alternative function, standard GPIO
        /// operations should no longer be performed and result in unpredictable
        /// behavior of the GPIO pin.
        /// </remarks>
        /// <param name="port">The port to configure.</param>
        /// <param name="mode">The alternate function to assign to this port.</param>
        public void EnableAsOutputAlternateFunction( Port port ,
                                                     int  mode )
        {
            ReservePort( port );

            var gpio = PXA27x.GPIO.Instance;

            gpio.ConfigureInterrupt             ( port.Id, false, false );
            gpio.EnableAsOutputAlternateFunction( port.Id, mode , false );
        }

        /// <summary>
        /// Configures the given port to one of its primary function, i.e. as
        /// a GPIO output pin.
        /// </summary>
        /// <param name="port">The port to configure.</param>
        /// <param name="fSet">If true, the pin will signal High, If false, the pin will signal Low.</param>
        public void EnableAsOutputPin( Port port ,
                                       bool fSet )
        {
            ReservePort( port );

            var gpio = PXA27x.GPIO.Instance;

            gpio.ConfigureInterrupt( port.Id, false, false );
            gpio.EnableAsOutputPin ( port.Id, fSet         );
        }

        /// <summary>
        /// Allows direct access to the current
        /// state of any port.
        /// </summary>
        /// <param name="pin">The pin number to retrieve the current state for.</param>
        /// <returns>The current state of the input pin.</returns>
        public bool this[int pin]
        {
            get
            {
                return PXA27x.GPIO.Instance[pin];
            }

            set
            {
                PXA27x.GPIO.Instance[pin] = value;
            }
        }

        /// <summary>
        /// Allows direct access to the current
        /// state of any port.
        /// </summary>
        /// <param name="port">The port to retrieve the current state for.</param>
        /// <returns>The current state of the input pin.</returns>
        public bool this[Port port]
        {
            get
            {
                return this[port.Id];
            }

            set
            {
                this[port.Id] = value;
            }
        }
        #endregion
        #endregion

        #region Private API
        /// <summary>
        /// Port vector used to track active reservations
        /// and to wire up interrupt forwarding requests.
        /// </summary>
        private Port[] m_reservation;

        #region Private Interrupt dispatcher
        /// <summary>
        /// The epoch of all times reported by this module.
        /// The value 1.1.2004 has been chosen for compatibility reasons
        /// with the .Net Micro Framework.
        /// </summary>
        private static DateTime s_epoch = new DateTime( 2004, 1, 1, 0, 0, 0 );

        /// <summary>
        /// Interrupt handler for GPIO interrupt for port 0.
        /// </summary>
        private InterruptController.Handler m_interrupt0;
        
        /// <summary>
        /// Interrupt handler for GPIO interrupt for port 1.
        /// </summary>
        private InterruptController.Handler m_interrupt1;

        /// <summary>
        /// Interrupt handler for GPIO interrupts
        /// for ports 2..120.
        /// </summary>
        private InterruptController.Handler m_interruptX;

        /// <summary>
        /// Handler for all GPIO interrupts (ports 0..120).
        /// </summary>
        /// <remarks>
        /// Note: interrupts GPIO0, GPIO1, and GPIOx are all wired to this handler
        ///       in <see cref="Initialize"/>.
        /// </remarks>
        /// <param name="handler">The handler that caused the interrupt.</param>
        private void ProcessGPIOInterruptX( InterruptController.Handler handler )
        {
            var gpioHandler = PXA27x.GPIO.Instance;

            //the timespan is for compatibility reasons
            //with the .Net Micro Framework
            TimeSpan when = TimeSpan.FromTicks((long)Drivers.RealTimeClock.Instance.CurrentTime);

            for (int i = 0; i < m_reservation.Length; i++)
            {
                if(Port.IsReservedPin( i ) == false)
                {
                    if(gpioHandler.InterruptPending( i, true ))
                    {
                        Port ip = m_reservation[i];
                        if(ip != null)
                        {
                            ip.m_interrupts.EnqueueNonblocking(when);
                            //ip.FireInterrupt( when );
                        }
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Singleton
#if TESTMODE_DESKTOP
        private static GPIO s_instance;
        public static GPIO Instance
        {
            get
            {
                if (null != s_instance) return s_instance;

                s_instance = new GPIO();
                s_instance.Initialize();
                return s_instance;
            }
        }
#else
        /// <summary>
        /// Singleton for the GPIO driver.
        /// </summary>
        extern public static GPIO Instance
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            [SingletonFactory]
            get;
        }
#endif
        #endregion

        #region Nested classes
        /// <summary>
        /// Base class for the Port resources managed by the GPIO driver.
        /// </summary>
        public class Port : IDisposable
        {
            //
            // State
            //
            private readonly int m_pin;

            #region Constructor/Destructor
            /// <summary>
            /// Initializes a Port managed by the GPIO driver.
            /// </summary>
            /// <param name="pin">The port identifier.</param>
            /// <param name="isInterrupt">Starts an interrupt thread for this port, if true.</param>
            public Port( int pin, bool isInterrupt )
            {
                if(IsReservedPin( pin ))
                {
                    throw new ArgumentOutOfRangeException( "pin" );
                }

                m_pin = pin;

                if (isInterrupt)
                {
                    m_interrupts = new KernelCircularBuffer<TimeSpan>(8);
                    m_interruptThread = new System.Threading.Thread(DispatchInterrupts);
                    m_interruptThread.Priority = System.Threading.ThreadPriority.Highest;
                    m_interruptThread.Start();
                }
            }

            ~Port()
            {
                Dispose(false);
            }

            /// <summary>
            /// Releases resources associtated with this
            /// pin.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
            }

            /// <summary>
            /// Releases any custom resource associated
            /// with this pin.
            /// </summary>
            /// <param name="disposing">If true, called from Dispose()</param>
            protected virtual void Dispose(bool disposing)
            {
                if (m_interruptThread != null && m_interruptThread.IsAlive)
                {
                    try
                    {
                        m_interruptThread.Abort();
                    }
                    catch
                    {
                    }
                }

                m_interruptThread = null;

                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Allows callers to modify the priority of the interrupt
            /// thread associated with this pin.
            /// </summary>
            protected System.Threading.ThreadPriority InterruptPriority
            {
                get
                {
                    if (null == m_interruptThread) throw new NotSupportedException();

                    return m_interruptThread.Priority;
                }

                set
                {
                    if (null == m_interruptThread) throw new NotSupportedException();

                    m_interruptThread.Priority = value;
                }
            }

            /// <summary>
            /// Validates if the given pin is not Reserved
            /// </summary>
            /// <param name="pin">The pin to check.</param>
            [Inline]
            internal static bool IsReservedPin( int pin )
            {
                if(pin < 0 || pin >= 120)
                {
                    return true;
                }

                switch(pin)
                {
                    case 2:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        return true;
                }

                return false;
            }
            #endregion

            #region Public API

            /// <summary>
            /// Gets the indentifier (ID) for a port. 
            /// </summary>
            /// <remarks>
            /// The ID for the port.
            /// </remarks>
            public int Id
            {
                get
                {
                    return m_pin;
                }
            }
            #endregion

            #region Internal API
            private System.Threading.Thread m_interruptThread;
            internal KernelCircularBuffer<TimeSpan> m_interrupts;
            private void DispatchInterrupts()
            {
                while (true)
                {
                    TimeSpan ts = m_interrupts.DequeueBlocking();
                    FireInterrupt(ts);
                }
            }

            /// <summary>
            /// Called by the hardware when an interrupt for the 
            /// port has been detected.
            /// </summary>
            /// <param name="whenOccurred">The time when the interrupt occured.</param>
            public virtual void FireInterrupt( TimeSpan whenOccurred )
            {
            }
            #endregion

        }
        #endregion
    }
}
