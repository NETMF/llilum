//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x.Drivers
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    using Microsoft.Zelig.Runtime;


    #region SPI bus and device descriptors
    /// <summary>
    /// Describes a SPI bus
    /// </summary>
    public class SPI_Host
    {

        #region Constructor/Destructor
        /// <summary>
        /// One of the pre-defined SPI channels to which the device is connected.
        /// <p/>
        /// If you're not using a pre-defined channel, be sure to
        /// override <see cref="OnEnableClock"/> and <see cref="OnEnablePins"/>
        /// for proper hardware initialization.
        /// </summary>
        private uint m_spiChannelIndex; //SSP module INDEX (0 .. 2)

        /// <summary>
        /// Initializes a SPI HOST structure
        /// </summary>
        public SPI_Host()
        {
        }

        /// <summary>
        /// Initializes one of the pre-defined
        /// SPI HOST structures (0-2)
        /// </summary>
        /// <param name="spiChannelIndex">The SPI channel to initialize.
        /// 0-1: standard. 2: the CC2024 SPI modul.</param>
        internal SPI_Host(uint spiChannelIndex)
        {
            m_spiChannelIndex = spiChannelIndex;

            if (m_spiChannelIndex == 0)
            {
                RXD = new GPIO.Port(26, false);
                TXD = new GPIO.Port(25, false);
                CLK = new GPIO.Port(23, false);
                SSP = PXA27x.SSP.Instance.SSP1;
                return;
            }

            //configure SPI2 pins
            if (m_spiChannelIndex == 1)
            {
                RXD = new GPIO.Port(11, false);
                TXD = new GPIO.Port(38, false);
                CLK = new GPIO.Port(36, false);
                SSP = PXA27x.SSP.Instance.SSP2;
                return;
            };

            //configure SPI3 pins (connected to the
            //CC2420 zigbee chip)
            if (m_spiChannelIndex == 2)
            {
                RXD = new GPIO.Port(41, false);
                TXD = new GPIO.Port(35, false);
                CLK = new GPIO.Port(34, false);
                SSP = PXA27x.SSP.Instance.SSP3;
                return;
            };

            //not a known configuration
            throw new ArgumentOutOfRangeException("spiChannelIndex");
        }
        #endregion

        #region Enable/Disable HW resources
        private bool m_clockEnabled;
        /// <summary>
        /// Enables/Disables the clock of the SPI device.
        /// </summary>
        public bool ClockEnabled
        {
            get
            {
                return m_clockEnabled;
            }
            set
            {
                if (value == m_clockEnabled) return;

                m_clockEnabled = value;

                OnEnableClock(m_clockEnabled);
            }
        }

        /// <summary>
        /// Called when the clock for the SPI is enabled
        /// or disabled.
        /// </summary>
        /// <param name="enabled">The requested state for the SPI clock.</param>
        protected virtual void OnEnableClock(bool enabled)
        {
            if (enabled)
            {
                if (m_spiChannelIndex == 0)
                    ClockManager.Instance.CKEN.EnSSP1 = true;
                else if (m_spiChannelIndex == 1)
                    ClockManager.Instance.CKEN.EnSSP2 = true;
                else if (m_spiChannelIndex == 2)
                    ClockManager.Instance.CKEN.EnSSP3 = true;
                else
                    throw new ArgumentOutOfRangeException("enabled");
            }
            else
            {
                if (m_spiChannelIndex == 0)
                    ClockManager.Instance.CKEN.EnSSP1 = false;
                else if (m_spiChannelIndex == 1)
                    ClockManager.Instance.CKEN.EnSSP2 = false;
                else if (m_spiChannelIndex == 2)
                    ClockManager.Instance.CKEN.EnSSP3 = false;
                else
                    throw new ArgumentOutOfRangeException("enabled");
            }
        }

        private bool m_pinsEnabled;
        /// <summary>
        /// Enables/Disables the pins associated with the
        /// SPI device.
        /// </summary>
        public bool PinsEnabled
        {
            get
            {
                return m_pinsEnabled;
            }
            set
            {
                if (value == m_pinsEnabled) return;

                m_pinsEnabled = value;

                OnEnablePins(m_pinsEnabled);
            }
        }

        /// <summary>
        /// Called whenever the SPI bus pins are enabled/disabled.
        /// </summary>
        /// <param name="enabled">The requested state of the SPI bus pins.</param>
        protected virtual void OnEnablePins(bool enabled)
        {
            if (enabled)
            {

                if (m_spiChannelIndex == 0)
                {
                    //allow peripheral control of pins;
                    GPIO.Instance.EnableAsOutputAlternateFunction(CLK, 2 /*GAFR__ALT_2*/);
                    GPIO.Instance.EnableAsOutputAlternateFunction(TXD, 2 /*GAFR__ALT_2*/);
                    GPIO.Instance.EnableAsInputAlternateFunction(RXD, 1 /*GAFR__ALT_1*/);
                }
                else if (m_spiChannelIndex == 1)
                {
                    //allow peripheral control of pins;
                    GPIO.Instance.EnableAsOutputAlternateFunction(CLK, 2 /*GAFR__ALT_2*/);
                    GPIO.Instance.EnableAsOutputAlternateFunction(TXD, 2 /*GAFR__ALT_2*/);
                    GPIO.Instance.EnableAsInputAlternateFunction(RXD, 2 /*GAFR__ALT_2*/);
                }
                else if (m_spiChannelIndex == 2)
                {
                    //allow peripheral control of pins;
                    GPIO.Instance.EnableAsOutputAlternateFunction(CLK, 3 /*GAFR__ALT_3*/);
                    GPIO.Instance.EnableAsOutputAlternateFunction(TXD, 3 /*GAFR__ALT_3*/);
                    GPIO.Instance.EnableAsInputAlternateFunction(RXD, 3 /*GAFR__ALT_3*/);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("enabled");
                }
            }
            else
            {
                // put pins in output low state when not in use to avoid noise since clock stop and reset will cause these to become inputs
                if (m_spiChannelIndex == 0)
                {
                    //allow peripheral control of pins;
                    GPIO.Instance.EnableAsOutputAlternateFunction(CLK, 2 /*GAFR__ALT_2*/);
                    GPIO.Instance.EnableAsOutputAlternateFunction(TXD, 2 /*GAFR__ALT_2*/);

                    // avoid noise drawing excess power on a floating line by putting this in pulldown
                    GPIO.Instance.EnableAsInputAlternateFunction(RXD, 1 /*GAFR__ALT_1*/);
                }
                else if (m_spiChannelIndex == 1)
                {
                    //allow peripheral control of pins;
                    GPIO.Instance.EnableAsOutputAlternateFunction(CLK, 2 /*GAFR__ALT_2*/);
                    GPIO.Instance.EnableAsOutputAlternateFunction(TXD, 2 /*GAFR__ALT_2*/);

                    // avoid noise drawing excess power on a floating line by putting this in pulldown
                    GPIO.Instance.EnableAsInputAlternateFunction(RXD, 2 /*GAFR__ALT_2*/);
                }
                else if (m_spiChannelIndex == 2)
                {
                    //allow peripheral control of pins;
                    GPIO.Instance.EnableAsOutputAlternateFunction(CLK, 3 /*GAFR__ALT_3*/);
                    GPIO.Instance.EnableAsOutputAlternateFunction(TXD, 3 /*GAFR__ALT_3*/);

                    // avoid noise drawing excess power on a floating line by putting this in pulldown
                    GPIO.Instance.EnableAsInputAlternateFunction(RXD, 3 /*GAFR__ALT_3*/);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("enabled");
                }
            }
        }
        #endregion

        #region SPI Bus properties
        /// <summary>
        /// The clock pin for the SPI channel
        /// </summary>
        public GPIO.Port CLK { get; protected set; }

        /// <summary>
        /// The transmit data pin for the SPI channel
        /// </summary>
        public GPIO.Port TXD { get; protected set; }

        /// <summary>
        /// The receive data pin for the SPI channel
        /// </summary>
        public GPIO.Port RXD { get; protected set; }

        /// <summary>
        /// The SSP channel
        /// </summary>
        public SSP.SSP_Channel SSP { get; protected set; }
        #endregion

    }

    /// <summary>
    /// Represents a device connected to a SPI
    /// bus.
    /// </summary>
    public class SPI_Device
    {

        #region SPI Device configuration properties
        /// <summary>
        /// The GPIO through which the device is conntected to the SPI bus
        /// </summary>
        public GPIO.Port DeviceCS;

        /// <summary>
        /// Polarity of the chip select of the
        /// peer device.
        /// </summary>
        /// <remarks>
        /// False = LOW active,
        /// TRUE = HIGH active
        /// </remarks>
        public bool CS_Active;

        /// <summary>
        /// The polarity of the data lines when idle.
        /// </summary>
        /// <remarks>
        /// False = LOW during idle,
        /// TRUE = HIGH during idle
        /// </remarks>
        public bool MSK_IDLE;

        /// <summary>
        /// When to read data from the input
        /// line.
        /// </summary>
        /// <remarks>
        /// False = sample falling edge,
        /// TRUE = samples on rising
        /// </remarks>
        public bool MSK_SampleEdge;

        /// <summary>
        /// The size of an atomic transfer
        /// </summary>
        /// <remarks>
        /// true: 16bit transfers
        /// false: 8bit transfers
        /// </remarks>
        public bool MD_16bits;

        /// <summary>
        /// The clock rate to use when talking to the device
        /// </summary>
        public uint Clock_RateKHz;

        /// <summary>
        /// Time to wait after asserting the chip select
        /// before starting to send/receive data.
        /// </summary>
        public uint CS_Setup_uSecs;

        /// <summary>
        /// Time to wait after transfering the last
        /// element before de-asserting the chip select.
        /// </summary>
        public uint CS_Hold_uSecs;
        #endregion

    };
    #endregion

    /// <summary>
    /// SPI device channel driver for the PXA271 SSP hardware.
    /// </summary>
    public class SPI_Channel
    {

        #region Constructor/Destructor
        private SPI_Host m_host;

        /// <summary>
        /// Initializes a SPI channel instance
        /// </summary>
        /// <param name="host">The configuration data for the SPI channel.</param>
        public SPI_Channel(SPI_Host host)
        {
            m_host = host;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Performs a SPI bus transaction with 16bit wide elements
        /// </summary>
        /// <param name="configuration">The peer device to talk to</param>
        /// <param name="write16">The data to send to the peer</param>
        /// <param name="writeCount">The number of elements to send</param>
        /// <param name="read16">The buffer for data received from the peer</param>
        /// <param name="readCount">the number of elements received from the peer</param>
        /// <param name="readStartOffset">The offset to start storing read data in the read16 buffer</param>
        public void WriteRead(SPI_Device configuration, UInt16[] write16, int writeCount, UInt16[] read16, int readCount, int readStartOffset)
        {
            WriteRead_Start(m_host, configuration);

            WriteRead_16(m_host, write16, writeCount, read16, readCount, readStartOffset);

            WriteRead_End(m_host, configuration);
        }

        /// <summary>
        /// Performs a SPI bus transaction with 8bit wide elements
        /// </summary>
        /// <param name="configuration">The peer device to talk to</param>
        /// <param name="write8">The data to send to the peer</param>
        /// <param name="writeCount">The number of elements to send</param>
        /// <param name="read8">The buffer for data received from the peer</param>
        /// <param name="readCount">the number of elements received from the peer</param>
        /// <param name="readStartOffset">The offset to start storing read data in the Read8 buffer</param>
        public void WriteRead(SPI_Device configuration, byte[] write8, int writeCount, byte[] read8, int readCount, int readStartOffset)
        {
            WriteRead_Start(m_host, configuration);

            WriteRead_8(m_host, write8, writeCount, read8, readCount, readStartOffset);

            WriteRead_End(m_host, configuration);
        }
        #endregion

        #region Private SPI-BUS Operations
        /// <summary>
        /// read/writes a 16bit quantum
        /// </summary>
        /// <param name="host">The SPI bus on which to perform the transaction</param>
        /// <param name="write16">The data to send to the peer</param>
        /// <param name="writeCount">The number of elements to send</param>
        /// <param name="read16">The buffer for data received from the peer</param>
        /// <param name="readCount">the number of elements received from the peer</param>
        /// <param name="readStartOffset">The offset to start storing read data in the read16 buffer</param>
        private static void WriteRead_16(SPI_Host host, ushort[] write16, int writeCount, ushort[] read16, int readCount, int readStartOffset)
        {
            int i, d;
            UInt16 Data16;
            int readIndex = 0;
            int writeIndex = 0;

            if (!host.PinsEnabled || !host.ClockEnabled)
            {
                Console.WriteLine("\fSPI Xaction OFF\r\n");
                ASSERT(false);
                return;
            }

            // as master, we must always write something before reading or not   
            ASSERT(writeCount > 0);
            ASSERT(write16 != null);
            ASSERT(readCount == 0 || read16 != null);

            // send the first word without qualifications
            host.SSP.SSDR = write16[writeIndex];

            // repeat last write word for all subsequent reads, otherwise increment
            writeCount--; if (writeCount > 0) writeIndex++;

            if (readCount > 0)
            {
                i = readCount + readStartOffset - 1;    // minus 1 since last read happens outside of loop
                d = readCount - 2;    // 2 for extra reads at end of pipeline
            }
            else
            {
                i = writeCount;
                d = -1;
            }

            if (i-- > 0)
            {
                // wait while the transmit buffer is full
                while (TransmitFifoNotEmpty(host.SSP)) ;

                // writing sets the TBF bit again
                host.SSP.SSDR = write16[writeIndex];

                // repeat last write word for all subsequent reads
                writeCount--; if (writeCount > 0) writeIndex++;

                while (i-- > 0)
                {
                    //while(PXA271_SPI::SSP_SSSR__RNE != ((PXA271_SPI::SSP_SSSR__RNE | PXA271_SPI::SSP_SSSR__TFL) & SPI.SSP_SSSR));
                    while (!host.SSP.SSSR.RNE || (host.SSP.SSSR.TFL > 0))
                        ;

                    // writing sets the TBF bit again
                    host.SSP.SSDR = write16[writeIndex];
                    // reading clears the RBF bit and allows another transfer from the shift register
                    Data16 = (ushort)host.SSP.SSDR;

                    // repeat last write word for all subsequent reads
                    writeCount--; if (writeCount > 0) writeIndex++;

                    // only save data once we have reached readCount-1 portion of words
                    if (i < d)
                    {
                        read16[readIndex] = Data16;
                        readIndex++;
                    }
                }

                while (ReceiveFifoEmpty(host.SSP)) ;

                // reading clears the RBF bit and allows another transfer from the shift register
                Data16 = (ushort)host.SSP.SSDR;

                if (readCount > 1)
                {
                    read16[readIndex] = Data16;
                    readIndex++;
                }
            }

            // wait for the last character written to actually get into the shifter
            while (TransmitFifoNotEmpty(host.SSP)) ;

            // no write, since we want the state machine to stop here

            // wait for last word to read to be ready - this also ensures the write is finished in case of no reads
            while (ReceiveFifoEmpty(host.SSP)) ;

            // read the last word, and without a transmit nothing more will happen in FULL_DUPLEX mode
            Data16 = (ushort)host.SSP.SSDR;

            // save last word read, if we are saving them
            if (readCount > 0) read16[readIndex] = Data16;
        }

        /// <summary>
        /// read/writes a 8bit quantum
        /// </summary>
        /// <param name="host">The SPI bus on which to perform the transaction</param>
        /// <param name="write8">The data to send to the peer</param>
        /// <param name="writeCount">The number of elements to send</param>
        /// <param name="read8">The buffer for data received from the peer</param>
        /// <param name="readCount">the number of elements received from the peer</param>
        /// <param name="readStartOffset">The offset to start storing read data in the Read8 buffer</param>
        private static void WriteRead_8(SPI_Host host, byte[] write8, int writeCount, byte[] read8, int readCount, int readStartOffset)
        {
            int i, d;
            byte Data8;
            int readIndex = 0;
            int writeIndex = 0;

            if (!host.PinsEnabled || !host.ClockEnabled)
            {
                Console.WriteLine("\fSPI Xaction OFF\r\n");
                ASSERT(false);
                return;
            }

            // as master, we must always write something before reading or not
            ASSERT(writeCount > 0);
            ASSERT(write8 != null);
            ASSERT(readCount == 0 || read8 != null);

            // send the first word without qualifications
            host.SSP.SSDR = write8[writeIndex];

            // repeat last write word for all subsequent reads, otherwise increment
            writeCount--; if (writeCount > 0) writeIndex++;

            if (readCount > 0)
            {
                i = readCount + readStartOffset - 1;    // minus 1 since last read happens outside of loop
                d = readCount - 2;    // 2 for extra reads at end of pipeline
            }
            else
            {
                i = writeCount;
                d = -1;
            }

            if (i-- > 0)
            {
                // wait while the transmit buffer is full
                while (TransmitFifoNotEmpty(host.SSP))
                    ;

                // writing sets the TBF bit again
                host.SSP.SSDR = write8[writeIndex];

                // repeat last write word for all subsequent reads
                writeCount--; if (writeCount > 0) writeIndex++;

                while (i-- > 0)
                {
                    //while(PXA271_SPI::SSP_SSSR__RNE != ((PXA271_SPI::SSP_SSSR__RNE | PXA271_SPI::SSP_SSSR__TFL) & SPI.SSP_SSSR));
                    while (!host.SSP.SSSR.RNE || (host.SSP.SSSR.TFL > 0))
                        ;

                    // writing sets the TBF bit again
                    host.SSP.SSDR = write8[writeIndex];

                    // reading clears the RBF bit and allows another transfer from the shift register
                    Data8 = (byte)host.SSP.SSDR;

                    // repeat last write word for all subsequent reads
                    writeCount--; if (writeCount > 0) writeIndex++;

                    // only save data once we have reached readCount-1 portion of words
                    if (i < d)
                    {
                        read8[readIndex] = Data8;
                        readIndex++;
                    }
                }

                while (ReceiveFifoEmpty(host.SSP))
                    ;

                // reading clears the RBF bit and allows another transfer from the shift register
                Data8 = (byte)host.SSP.SSDR;

                if (readCount > 1)
                {
                    read8[readIndex] = Data8;
                    readIndex++;
                }
            }

            // wait for the last character written to actually get into the shifter
            while (TransmitFifoNotEmpty(host.SSP))
                ;

            // no write, since we want the state machine to stop here

            // wait for last word to read to be ready - this also ensures the write is finished in case of no reads
            while (ReceiveFifoEmpty(host.SSP))
                ;

            // read the last word, and without a transmit nothing more will happen in FULL_DUPLEX mode
            Data8 = (byte)host.SSP.SSDR;

            // save last word read, if we are saving them
            if (readCount > 0) read8[readIndex] = Data8;
        }

        /// <summary>
        /// Starts a SPI transaction
        /// </summary>
        /// <param name="host"></param>
        /// <param name="configuration"></param>
        private static void WriteRead_Start(SPI_Host host, SPI_Device configuration)
        {
            if (null == host) throw new ArgumentNullException("host");
            if (null == configuration) throw new ArgumentNullException("configuration");

            if (host.PinsEnabled || host.ClockEnabled)
            {
                Console.WriteLine("\fSPI Collision 3\r\n");
                ASSERT(false);
                return;
            }

            // enable the Periperal clock for this device
            host.ClockEnabled = true;

            //TODO: enable tests
            // make sure we didn't start one in the middle of an existing transaction
            //if((0 != (uint)SPI.SSCR0.)||(0 != (uint)SPI.SSCR1))
            //{
            //    throw new NotSupportedException();
            //    //return;
            //} 

            host.SSP.SSCR0 = new SSP.SSCR0_Bitfield
            {
                MOD = false, //polling mode
                FRF = 0, //Motorol SPI
                DSS = ((configuration.MD_16bits) ? 15u : 7u),
                SCR = (ConvertClockRateToDivisor(configuration.Clock_RateKHz) - 1)
            };

            host.SSP.SSCR1 = new SSP.SSCR1_Bitfield
            {
                SCLKDIR = false, //port is in master mode
                SFRMDIR = false, //port is in master mode
                TTE = true, //TXD tristate enable
                SPO = configuration.MSK_IDLE,
                SPH = !(configuration.MSK_IDLE ^ configuration.MSK_SampleEdge), //both are equal
            };

#if SPI_LOOP_BACK_PXA271
                SPI.SSCR1.LBM = true;
#endif
            host.SSP.SSCR0.SSE = true;

            //everything should be clean and idle
            ASSERT(!TransmitFifoNotEmpty(host.SSP));
            ASSERT(ReceiveFifoEmpty(host.SSP));
            ASSERT(ShiftBufferEmpty(host.SSP));

            //allow peripheral control of pins;
            host.PinsEnabled = true;

            // first set CS active as soon as clock and data pins are in proper initial state
            GPIO.Instance.EnableAsOutputPin(configuration.DeviceCS, configuration.CS_Active);

            // then setup the receiving pin
            //fmegen: was configuring RXD pins

            if (configuration.CS_Setup_uSecs > 0)
            {
                //TODO: sleep
                //Time_Sleep_MicroSeconds_InterruptEnabled( Configuration.CS_Setup_uSecs );
            }
        }

        /// <summary>
        /// ends a SPI transaction
        /// </summary>
        /// <param name="host"></param>
        /// <param name="configuration"></param>
        private static void WriteRead_End(SPI_Host host, SPI_Device configuration)
        {
            ASSERT(null != host);
            ASSERT(null != configuration);

            if (!host.PinsEnabled || !host.ClockEnabled)
            {
                Console.WriteLine("\fSPI Collision 4\r\n");
                ASSERT(false);
                return;
            }

            // we should have cleared the last TBF on the last RBF true setting
            // we should never bring CS inactive with the shifter busy
            ASSERT(!TransmitFifoNotEmpty(host.SSP));
            ASSERT(ReceiveFifoEmpty(host.SSP));
            ASSERT(ShiftBufferEmpty(host.SSP));

            if (configuration.CS_Hold_uSecs > 0)
            {
                //TODO: sleep
                //Time_Sleep_MicroSeconds_InterruptEnabled( Configuration.CS_Hold_uSecs );
            }

            // next, bring the CS to the proper inactive state
            GPIO.Instance.EnableAsOutputPin(configuration.DeviceCS, !configuration.CS_Active);

            // put pins in output low state when not in use to avoid noise since clock stop and reset will cause these to become inputs
            host.PinsEnabled = false;

            // disable spi bus so no new operations start
            host.SSP.SSCR0 = new SSP.SSCR0_Bitfield();
            host.SSP.SSCR1 = new SSP.SSCR1_Bitfield();

            //disable the peripheral clock for this device.
            host.ClockEnabled = false;

            //at this point, everything associated with the SPI bus
            //should be shut down.
            ASSERT(!host.PinsEnabled && !host.ClockEnabled);
        }

        [Inline]
        private static uint ConvertClockRateToDivisor(uint Clock_RateKHz)
        {
            uint SSP_CLOCK_KHZ = 13000;

            if (Clock_RateKHz >= SSP_CLOCK_KHZ)
                return 1;
            else if (Clock_RateKHz <= (SSP_CLOCK_KHZ / 4096))
                return 4096;
            else
                return (((SSP_CLOCK_KHZ + Clock_RateKHz - 1) / Clock_RateKHz));
        }

        [Inline]
        private static bool TransmitFifoNotEmpty(SSP.SSP_Channel channel)
        {
            return (channel.SSSR.TFL != 0);
        }

        [Inline]
        private static bool ReceiveFifoEmpty(SSP.SSP_Channel channel)
        {
            return (channel.SSSR.RNE == false);
        }

        [Inline]
        private static bool ShiftBufferEmpty(SSP.SSP_Channel channel)
        {
            return (channel.SSSR.BSY == false);
        }

        private static void ASSERT(bool p)
        {
            if (!p)
            {
                Console.WriteLine("did not expect");
                throw new NotSupportedException();
            }
        }
        #endregion

    }

    /// <summary>
    /// SPI device driver for the PXA271 SSP hardware.
    /// </summary>
    public abstract class SPI
    {

        #region Singleton API
#if TESTMODE_DESKTOP
        private static SPI s_instance;
        /// <summary>
        /// Retrieves the SPI driver singleton.
        /// </summary>
        public static SPI Instance
        {
            get
            {
                if (null != s_instance) return s_instance;
                s_instance = new SPI();
                s_instance.Initialize();
                return s_instance;
            }
        }
#else
        /// <summary>
        /// Retrieves the SPI driver singleton.
        /// </summary>
        extern public static SPI Instance
        {
            [MethodImpl(MethodImplOptions.InternalCall)]
            [SingletonFactory]
            get;
        }
#endif
        #endregion

        #region Available SPI Channels
        private SPI_Channel[] m_channels;

        /// <summary>
        /// SPI channel list
        /// </summary>
        public SPI_Channel[] Channels
        {
            get
            {
                if (null == m_channels)
                    throw new InvalidOperationException("m_channels");

                return m_channels;
            }
        }
        #endregion

        #region Constructor/Destructor
        /// <summary>
        /// Initialized the SPI driver.
        /// </summary>
        public void Initialize()
        {
            m_channels = new SPI_Channel[3];

            //configure SPI1 pins
            m_channels[0] = new SPI_Channel(new SPI_Host(0));

            //configure SPI2 pins
            m_channels[1] = new SPI_Channel(new SPI_Host(1));

            //configure SPI3 pins (connected to the
            //CC2420 zigbee chip)
            m_channels[2] = new SPI_Channel(new SPI_Host(2));
        }
        #endregion

    }

}
