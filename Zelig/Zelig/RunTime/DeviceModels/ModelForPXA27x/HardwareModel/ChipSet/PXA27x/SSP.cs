//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;

    /// <summary>
    /// The Synchronous Serial Port (SSP) subsystem of the XScale PXA271
    /// </summary>
    /// <remarks>
    /// The SSP ports are a synchronous serial interfaces that connect to a variety of external
    /// devices that use serial protocols for data transfer. The SSP ports provide support for the
    /// following protocols:
    /// <list type="bullet">
    /// <item><description>Texas Instruments (TI) Synchronous Serial Protocol</description></item>
    /// <item><description>Motorola Serial Peripheral Interface (SPI) protocol</description></item>
    /// <item><description>National Semiconductor Microwire</description></item>
    /// <item><description>Programmable Serial Protocol (PSP)</description></item>
    /// </list>
    /// <p/>
    /// The SSP ports operate as full-duplex devices for the TI Synchronous Serial Protocol, SPI, and PSP
    /// protocols and as a half-duplex device for the Microwire protocol.
    /// The FIFOs can be loaded or emptied by the CPU using programmed I/O or by DMA burst
    /// transfers.
    /// <p/>
    /// <strong>Features</strong>
    /// <list type="bullet">
    /// <item><description>Supports the TI Synchronous Serial Protocol, the Motorola SPI protocol, National
    /// Semiconductor Microwire, and a Programmable Serial Protocol (PSP)</description></item>
    /// <item><description>One transmit FIFO and one receive FIFO, each 16 samples deep by 32-bits
    /// wide</description></item>
    /// <item><description>Sample sizes from four to 32-bits</description></item>
    /// <item><description>Bit-rates from 6.3 Kbps (minimum) to 13 Mbps (maximum)</description></item>
    /// <item><description>Master-mode and slave-mode operation</description></item>
    /// <item><description>Receive-without-transmit operation</description></item>
    /// <item><description>Network mode with up to eight time slots and independent transmit/receive
    /// in any/all/none of the time slots—available only with TI Synchronous Serial Protocol and
    /// Programmable Serial Protocol (PSP) formats</description></item>
    /// <item><description>Audio clock control to provide a 4x output clock and support for selection
    /// of most standard audio Codec frequencies</description></item>
    /// </list>
    /// </remarks>
    [MemoryMappedPeripheral(Base = 0x41000000U, Length = 0x00A00000U)]
    public class SSP
    {

        #region SSCR0 Bitfield
        /// <summary>
        /// SSCR0 controls various functions within the SSP port.
        /// Before enabling the SSP port by setting SSCR0[SSE],
        /// the desired values for this register must be programmed.
        /// </summary>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSCR0_Bitfield
        {
            /// <summary>
            /// Mode
            /// 0 = Normal SSP port mode
            /// 1 = Network mode
            /// </summary>
            /// <remarks>
            /// Mode
            /// 0 = Normal SSP port mode
            /// 1 = Network mode
            /// <p/>
            /// NOTE: When network mode is selected, only use DMA to move data
            /// to/from the SSP Data register (SSDR). Do not use interrupts or
            /// CPU polling.
            /// <p/>
            /// Set this bit only when using PSP and TI SSP formats. Setting this bit causes
            /// SSPSCLKx to run continuously (if the SSP port is a master of the clock—
            /// <see cref="SSCR1_Bitfield.SCLKDIR"/> = 0). During network mode, only one SSPSFRMx is
            /// sent (received) for the number of time slots programmed into the
            /// <see cref="SSCR0_Bitfield.FRDC"/> field.
            /// <p/>
            /// When using the PSP format in network mode, the parameters SFRMDLY,
            /// STRTDLY, DMYSTP, DMYSTRT must be cleared. Other parameters (such
            /// as FRMPOL, SCMODE, FSRT, SFRMDWDTH) are programmable.
            /// </remarks>
            [BitFieldRegister(Position = 31)]
            public bool MOD;

            /// <summary>
            /// audio clock select
            /// </summary>
            /// <remarks>
            /// 0 = SSPSCLKx selection is determined by the NCS and ECS bits.
            /// 1 = Audio Clock (and Audio Clock Divider) creates SSPSCLKx.
            /// <p/>
            /// If the ACS bit is set (and the GPIO are properly configured), SSPSYSCLKx
            /// is continually output (even if the SSP port is disabled). SSPSCLKx is output
            /// as previously defined (determined by format, SSPSCLKENx, ECRA/ECRB
            /// functions).
            /// </remarks>
            [BitFieldRegister(Position = 30)]
            public bool ACS;

            /// <summary>
            /// Frame rate divider control
            /// </summary>
            /// <remarks>
            /// Value 0-7 indicates the number of time slots per frame when in network
            /// mode (the actual number of time slots is FRDC + 1 for 1–8 time slots)
            /// </remarks>
            [BitFieldRegister(Position = 24, Size=3)]
            public uint FRDC;

            /// <summary>
            /// Transmit FIFO underrung interrupt mask
            /// </summary>
            /// <remarks>
            /// 0 = TUR events generate an SSP port interrupt.
            /// 1 = TUR events do not generate an SSP port interrupt.
            /// When set, this bit masks the TX FIFO Underrun (TUR) event from
            /// generating an SSP port interrupt. SSSR will still indicate that a TUR event
            /// has occurred. This bit can be written to at any time (before or after SSP port
            /// is enabled).
            /// </remarks>
            [BitFieldRegister(Position = 23)]
            public bool TIM;

            /// <summary>
            /// Receive FIFO overrung interrupt mask
            /// </summary>
            /// <remarks>
            /// Receive FIFO Overrun Interrupt Mask
            /// 0 = ROR events generate an SSP port interrupt.
            /// 1 = ROR events do not generate an SSP port interrupt.
            /// When set, this bit masks the RX FIFO Overrun (ROR) event from
            /// generating an SSP port interrupt. SSSR will still indicate that an ROR
            /// event has occurred.This bit can be written to at any time (before or after
            /// SSP port is enabled).
            /// </remarks>
            [BitFieldRegister(Position = 22)]
            public bool RIM;

            /// <summary>
            /// Network clock select
            /// </summary>
            /// <remarks>
            /// Used with ECS to select the network clock.
            /// 0 = The NCS bit determines clock selection.
            /// 1 = Network Clock creates the SSP port’s SSPSCLKx.
            /// Before setting the NCS bit, first disable the port. The NCS (and ECS) bits
            /// must be configured before or at the same time that the SSE bit is set.
            /// </remarks>
            [BitFieldRegister(Position = 21)]
            public bool NCS;

            /// <summary>
            /// Extended data size select
            /// </summary>
            /// <remarks>
            /// Selects the bit rate of the SSP port when in master mode with respect to
            /// SSPSCLKx (as defined by <see cref="SSCR1_Bitfield.SCLKDIR"/>). The maximum bit rate is
            /// 13 Mbps. The serial-clock generator uses clocks selected by ECS and NCS.
            /// The selected clock is divided by the value of SCR plus 1 (a range of 1 to
            /// 4096) to generate SSPSCLKx.
            /// <p/>
            /// NOTE: This field is ignored when the SSP port is a slave with respect to
            /// SSPSCLKx (defined by <see cref="SSCR1_Bitfield.SCLKDIR"/>) and transmission
            /// data rates are determined by an external device.
            /// <p/>
            /// NOTE: Software must not change SCR when SSPSCLKx is enabled
            /// (through use of the SSPSCLKEN pin or SSPCR1.ECRA or
            /// SPCR1.ECRB) because doing so causes the SSPSCLKx
            /// frequency to immediately change.
            /// Values (0 to 4095) generate the clock rate of the SSP port.
            /// Serial bit rate = SSP Port Clock / (SCR + 1), where SCR is a decimal integer
            /// </remarks>
            [BitFieldRegister(Position = 20)]
            public bool EDSS;

            /// <summary>
            /// Serial clock rate
            /// </summary>
            /// <remarks>
            /// Used with DSS to select the size of the data transmitted and received by the
            /// SSP port.
            /// 0 = Zero is pre-appended to the DSS value that sets the DSS range from
            /// 4-16- bits.
            /// 1 = One is pre-appended to the DSS value that sets the DSS range from
            /// 17-32-bits.
            /// </remarks>
            [BitFieldRegister(Position = 8, Size = 12)]
            public uint SCR;

            /// <summary>
            /// Synchronous serial enable.
            /// </summary>
            /// <remarks>
            /// Enables or disables all SSP port operations. When the port is disabled, all of
            /// its clocks can be stopped by programmers to minimize power consumption.
            /// When cleared during active operation, the SSP port is disabled immediately,
            /// terminating the current frame being transmitted or received. Clearing SSE
            /// resets the port FIFOs and the status bits; however, the SSP port control
            /// registers and the receive FIFO overrun status bit are not reset.
            /// NOTE: After reset or after clearing the SSE, ensure that the SSCR1,
            /// SSITR, SSTO, and SSPSP control registers are properly reconfigured
            /// and that the SSSR register is reset before re-enabling
            /// the SSP port by setting SSE. Also, SSE must be cleared before reconfiguring
            /// the SSCR0, SSCR1, or SSPSP registers; any or
            /// all control bits in SSCR0 can be written at the same time as the
            /// SSE.
            /// <p/>
            /// 0 = SSP port operation disabled.
            /// 1 = SSP port operation enabled.
            /// </remarks>
            [BitFieldRegister(Position = 7)]
            public bool SSE;

            /// <summary>
            /// Extended clock select
            /// </summary>
            /// <remarks>
            /// Used with NCS to select the clock source for the SSP port.
            /// ECS in conjunction with NCS selects whether the SSP port uses the on-chip
            /// 13-MHz clock or one of two off-chip clocks supplied by GPIO: the network
            /// clock (CLK_EXT, described in Chapter 24, “General-Purpose I/O
            /// Controller”) or the SSP port’s external clock (SSPEXTCLK) produces serial
            /// transmission rates ranging from 6.3 Kbps (minimum recommended bit rate)
            /// to a maximum of 13 Mbps. When NCS is cleared, ECS selects between the
            /// on-chip 13-MHz clock and external clock (SSPEXTCLK). When NCS is set,
            /// the network clock (CLK_EXT) is selected. The frequency of the off-chip
            /// clock can be any value up to 13 MHz.
            /// <p/>
            /// When the SSP port is a slave with respect to SSPSCLKx (defined by the
            /// <see cref="SSCR1_Bitfield.SCLKDIR"/>), this field is ignored and transmission data rates are
            /// determined by the external device.
            /// Before setting the ECS bit, first disable the port. The ECS (and NCS) bit
            /// must be configured before or at the same time that SSE is set.
            /// <p/>
            /// When ECS is cleared, SSPEXTCLKx is treated as SSPSCLKENx, a clock
            /// enable that gates the SSPSCLKx output. When
            /// the SSPSCLKENx changes, there is a 1–2 clock lag before SSPSCLKx is
            /// started or stopped because of internal synchronization delays.
            /// <p/>
            /// 0 = On-chip clock produces the SSP port’s SSPSCLKx.
            /// 1 = SSPEXTCLK/GPIO pin creates the SSP port’s SSPSCLKx.
            /// <p/>
            /// NOTE: The ECS bit for SSP3, SSCR0_3.ECS, should never be set to one
            /// because the SSP3 does not have an associated SSPEXTCLK.
            /// </remarks>
            [BitFieldRegister(Position = 6)]
            public bool ECS;

            /// <summary>
            /// Frame format
            /// </summary>
            /// <remarks>
            /// Selects which frame format to use.
            /// <list type="bullet">
            /// <item><description>0b00 = Motorola Serial Peripheral Interface</description></item>
            /// <item><description>0b01 = TI Synchronous Serial Protocol</description></item>
            /// <item><description>0b10 = Microwire</description></item>
            /// <item><description>0b11 = Programmable Serial Protocol</description></item>
            /// </list>
            /// </remarks>
            [BitFieldRegister(Position = 5, Size=2)]
            public uint FRF;

            /// <summary>
            /// Data size select
            /// </summary>
            /// <remarks>
            /// Used in conjunction with EDSS to select the size of the data transmitted and
            /// received by the SSP port. The concatenated 5-bit value of EDSS and DSS
            /// provides a data range from four to 32-bits in length.
            /// <p/>
            /// For the Microwire protocol, DSS and EDSS determine the receive data size.
            /// The size of the transmitted data is either eight or 16-bits (determined by
            /// <see cref="SSCR1_Bitfield.MWDS"/>) and the EDSS bit is ignored. For all modes (including
            /// Microwire), EDSS and DSS determine the receive data size.
            /// <p/>
            /// When data is programmed to be less than 32 bits, data written to the TX
            /// FIFO must be right-justified.
            /// <p/>
            /// <strong>Data sizes</strong>
            /// <list type="bullet">
            /// <item><description>EDSS == 0, DSS [0 .. 2] will reserved,undefined</description></item>
            /// <item><description>EDSS == 0, DSS [3 .. 15] will be [4bit .. 16bit]</description></item>
            /// <item><description>EDSS == 1, DSS [0 .. 15] data size will be [17bit .. 32bit]</description></item>
            /// </list>
            /// </remarks>
            [BitFieldRegister(Position = 0, Size=4)]
            public uint DSS;
        }
        #endregion

        #region SSCR1 Bitfield
        /// <summary>
        /// SSCR1, controls various SSP
        /// port functions. Before enabling the port (using
        /// <see cref="SSCR0_Bitfield.SSE"/>),
        /// the desired values for this register must be set.
        /// </summary>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSCR1_Bitfield
        {
            /// <summary>
            /// TXD tristate enable on last phase
            /// </summary>
            /// <remarks>
            /// TTELP is used in conjunction with TTE. When set, TTELP causes
            /// SSPTXDx to become high impedance 1/2 phase later than specified in
            /// the TTE bit description under either of the following 2 conditions: 1) the
            /// format is SSP; or 2) the format is PSP and the SSP port is a slave to
            /// frame (<see cref="SSCR1_Bitfield.SFRMDIR"/> is set). For example, when in SSP format,
            /// with TTELP and TTE set, SSPTXDx goes to high impedance one full
            /// clock after the clock edge that starts the LSB.
            /// Ensure if SCLKDIR is set (SSP port is a slave to SSPSCLKx) that the
            /// device driving SSPSCLKx provides another clock edge.
            /// NOTE: TTELP must not be set when using Microwire or SPI formats
            /// 1 = SSPTXDx becomes high impedance one full clock after the clock
            /// edge that starts the LSB.
            /// 0 = SSPTXDx becomes high impedance 1/2 clock after the clock
            /// edge that starts the LSB.
            /// </remarks>
            [BitFieldRegister(Position = 31)]
            public bool TTELP;

            /// <summary>
            /// TXD tristate enable
            /// </summary>
            /// <remarks>
            /// If TTE is cleared, SSPTXDx is always driven. When set, TTE causes
            /// SSPTXDx to become high impedance when the SSP port is not
            /// transmitting data. The timing for the high impedance enable/disable
            /// varies according to the different serial formats and frame direction.
            /// For Microwire format, SSPTXDx is driven on the same clock edge that
            /// the MSB is driven, and SSPTXDx becomes high impedance one full
            /// clock after the clock edge that starts the LSB.
            /// <p/>
            /// For SPI protocol, SSPTXDx becomes high impedance whenever
            /// SPSFRMx is deasserted (driven high).
            /// <p/>
            /// For TI Synchronous Serial Protocol, SSPTXDx is driven at the first
            /// rising edge of the SSPSCLKx after SSPSFRMx is asserted (the same
            /// time the MSB is driven) and continues to be driven until 1/2 clock
            /// (TTELP = 1) or 1 full clock (TTELP = 0) after the clock edge that starts
            /// the LSB.
            /// <p/>
            /// For PSP format when the SSP port is a slave to frame, SSPTXDx
            /// becomes high impedance 1/2 clock after the clock edge that starts the
            /// LSB. For PSP format when the SSP port is a master to frame,
            /// SSPTXDx becomes high impedance one full clock after the clock edge
            /// that starts the LSB (even if the SSP port is a master of clock and this
            /// clock edge does not appear on SSPSCLKx).
            /// <p/>
            /// NOTE: TTE must be set when in network mode.
            /// 0 = SSPTXDx does not become high impedance.
            /// 1 = SSPTXDx becomes high impedance when not transmitting data.
            /// </remarks>
            [BitFieldRegister(Position = 30)]
            public bool TTE;

            /// <summary>
            /// Enable bit count error interrupt
            /// </summary>
            /// <remarks>
            /// When set, EBCEI enables a bit count error interrupt. A bit count error
            /// occurs when the SSP port is a slave to SSPSCLKx and/or SSPSFRMx
            /// and the SSP port detects a new frame before the internal bit counter
            /// has reached 0 (before the LSB was driven).
            /// 0 = Interrupt due to a bit count error is disabled.
            /// 1 = Interrupt due to a bit count error is enabled.
            /// </remarks>
            [BitFieldRegister(Position = 29)]
            public bool EBCEI;

            /// <summary>
            /// Slave clock free running
            /// </summary>
            /// <remarks>
            /// In slave mode (<see cref="SSCR1_Bitfield.SCLKDIR"/> is set), SCFR must be cleared if
            /// the input clock from the external source is running continuously.
            /// In master mode (<see cref="SSCR1_Bitfield.SCLKDIR"/> is cleared), SCFR is ignored.
            /// Slave mode only:
            /// 0 = SSPSCLKx is continuously running.
            /// 1 = SSPSCLKx is active only during transfers.
            /// </remarks>
            [BitFieldRegister(Position = 28)]
            public bool SCFR;

            /// <summary>
            /// Enable clock request A
            /// </summary>
            /// <remarks>
            /// 0 = Clock request from another SSP port is disabled.
            /// 1 = Clock request from another SSP port is enabled.
            /// </remarks>
            [BitFieldRegister(Position = 27)]
            public bool ECRA;

            /// <summary>
            /// Enable clock request B
            /// </summary>
            /// <remarks>
            /// 0 = Clock request from another SSP port is disabled.
            /// 1 = Clock request from another SSP port is enabled.
            /// </remarks>
            [BitFieldRegister(Position = 26)]
            public bool ECRB;

            /// <summary>
            /// SSPSCLKx direction
            /// </summary>
            /// <remarks>
            /// SCLKDIR determines whether the port is the master or slave (with
            /// respect to driving SSPSCLKx).
            /// Depending on the frame format selected, each transmitted bit is driven
            /// on either the rising or falling edge of SSPSCLKx, and is sampled on the
            /// opposite clock edge. When the GPIO alternate function is selected for
            /// the SSP port, this bit has precedence over the GPIO direction bit.
            /// SCLKDIR must be written before the GPIO direction bit (to prevent any
            /// possible contention on SSPSCLKx).
            /// 0 = Master mode, the port generates SSPSCLKx internally, acts as
            /// the master, and drives SSPSCLKx.
            /// 1 = Slave mode, the port acts as a slave, receives SSPSCLKx from
            /// an external device and uses it to determine when to drive transmit
            /// data on SSPTXDx and when to sample receive data on
            /// SSPRXDx.
            /// NOTE: When SCLKDIR is set, the <see cref="SSCR0_Bitfield.NCS"/> and
            /// <see cref="SSCR0_Bitfield.ECS"/> bits must be cleared.
            /// </remarks>
            [BitFieldRegister(Position = 25)]
            public bool SCLKDIR;

            /// <summary>
            /// SSP frame direction
            /// </summary>
            /// <remarks>
            /// SFRMDIR determines whether the SSP port is the master or slave (with
            /// respect to driving SSPSFRMx).
            /// NOTE: When the port is configured as a slave to SSPSFRMx, the
            /// external device driving SSPSFRMx must wait until
            /// <see cref="SSSR_Bitfield.CSS"/> is cleared after enabling the port and before
            /// asserting SSPSFRMx (no external clock cycles are needed).
            /// When the GPIO alternate function is selected for the port,
            /// SFRMDIR has precedence over the GPIO direction bit.
            /// SFRMDIR must be written before the GPIO direction bit (to
            /// prevent any possible contention on SSPSFRMx).
            /// 0 = Master mode, the port generates SSPSFRMx internally, acts as
            /// the master and drives SSPSFRMx.
            /// 1 = Slave mode, the port acts as a slave, receives SSPSFRMx from
            /// an external device.
            /// </remarks>
            [BitFieldRegister(Position = 24)]
            public bool SFRMDIR;

            /// <summary>
            /// Receive without transmit
            /// </summary>
            /// <remarks>
            /// RWOT puts the SSP port into a mode similar to half duplex. This allows
            /// the port to receive data without transmitting data (half-duplex only).
            /// When the port is in master mode (SCLKDIR cleared) and RWOT is set,
            /// the port continues to clock in receive data, regardless of data existing in
            /// the transmit FIFO. Data is sent/received immediately after the port
            /// enable bit (<see cref="SSCR0_Bitfield.SSE"/>) is set. In this mode, if there
            /// is no data to send, the DMA service requests and interrupts for the transmit
            /// FIFO must be disabled (clear both <see cref="SSCR1_Bitfield"/>.[TSRE,TIE]). If the transmit FIFO
            /// is empty, SSPTXDx is driven low.
            /// The transmit FIFO underrun condition does not occur when RWOT is
            /// set. When RWOT is set, <see cref="SSSR_Bitfield.BSY"/> remains set until software
            /// clears the RWOT bit. After RWOT is cleared, and extra frame cycle may
            /// occur due to synchronization delays between the processor’s clock
            /// domains. RWOT must not be used when <see cref="SSCR0_Bitfield.MOD"/> is set.
            /// 0 = Transmit/receive mode.
            /// 1 = Receive without transmit mode.
            /// </remarks>
            [BitFieldRegister(Position = 23)]
            public bool RWOT;

            /// <summary>
            /// Trailing byte
            /// </summary>
            /// <remarks>
            /// TRAIL configures how trailing bytes are handled
            /// <p/>
            /// 0 = Processor based, Trailing bytes are handled by the CPU.
            /// 1 = DMA based, Trailing bytes are handled by DMA.
            /// </remarks>
            [BitFieldRegister(Position = 22)]
            public bool TRAIL;

            /// <summary>
            /// Transmit service request enable
            /// </summary>
            /// <remarks>
            /// TSRE enables the transmit FIFO DMA Service Request.
            /// NOTE: Clearing TSRE does not affect the current state of
            /// <see cref="SSSR_Bitfield.TFS"/> or the ability of the transmit FIFO logic to set and
            /// clear <see cref="SSSR_Bitfield.TFS"/>; it prevents the generation of the DMA
            /// Service Request. The state of TSRE does not effect the
            /// generation of the interrupt, which is asserted whenever the
            /// <see cref="SSSR_Bitfield.TFS"/> is set.
            /// 0 = DMA service request is disabled and the state of the transmit
            /// FIFO DMA service request is ignored.
            /// 1 = DMA service request is enabled.
            /// </remarks>
            [BitFieldRegister(Position = 21)]
            public bool TSRE;

            /// <summary>
            /// Receive service request enable
            /// </summary>
            /// <remarks>
            /// RSRE enables the Receive FIFO DMA Service Request.
            /// NOTE: Clearing RSRE does not affect the current state of
            /// <see cref="SSSR_Bitfield.RFS"/> or the ability of the receive FIFO logic to set and
            /// clear <see cref="SSSR_Bitfield.RFS"/>; it blocks only the generation of the DMA
            /// Service Request. The state of RFRS does not affect the
            /// generation of the interrupt, which is asserted whenever
            /// <see cref="SSSR_Bitfield.RFS"/> is set.
            /// 0 = DMA service request is disabled and the state of the
            /// <see cref="SSSR_Bitfield.RFS"/> is ignored.
            /// 1 = DMA service request is enabled.
            /// </remarks>
            [BitFieldRegister(Position = 20)]
            public bool RSRE;

            /// <summary>
            /// Receiver time-out interrupt enable
            /// </summary>
            /// <remarks>
            /// TINTE enables the receiver time-out interrupt.
            /// NOTE: Clearing TINTE does not affect the current state of
            /// <see cref="SSSR_Bitfield.TINT"/> or the ability of logic to set and clear
            /// <see cref="SSSR_Bitfield.TINT"/>; it prevents the generation of the interrupt
            /// request.
            /// 0 = Receiver time-out interrupts are disabled. The interrupt is masked
            /// and the state of <see cref="SSSR_Bitfield.TINT"/> is ignored by the interrupt
            /// controller.
            /// 1 = Receiver time-out interrupts are enabled.
            /// </remarks>
            [BitFieldRegister(Position = 19)]
            public bool TINTE;

            /// <summary>
            /// Peripheral trailing byte interrupt enable
            /// </summary>
            /// <remarks>
            /// PINTE enables the peripheral trailing byte interrupt.
            /// NOTE: Clearing PINTE does not affect the current state of
            /// <see cref="SSSR_Bitfield.PINT"/> or the ability of logic to set and clear
            /// <see cref="SSSR_Bitfield.PINT"/>; it prevents the generation of the interrupt
            /// request.
            /// 0 = Peripheral trailing byte interrupts are disabled. The interrupt is
            /// masked and the state of SSSRx.PINT is ignored by the interrupt
            /// controller.
            /// 1 = Peripheral trailing byte interrupt are enabled.
            /// </remarks>
            [BitFieldRegister(Position = 18)]
            public bool PINTE;

            /// <summary>
            /// Invert frame signal
            /// </summary>
            /// <remarks>
            /// 0 = SSPSFRMx polarity is determined by SSP format and PSP
            /// polarity bits.
            /// 1 = SSPSFRMx is inverted from the normal SSP frame signal (as
            /// defined by the SSP format and PSP polarity bits).
            /// </remarks>
            [BitFieldRegister(Position = 16)]
            public bool IFS;

            /// <summary>
            /// Select FIFO for EFWR (test mode bit only)
            /// </summary>
            /// <remarks>
            /// Only when <see cref="SSCR1_Bitfield.EFWR"/> is set, STRF selects whether the transmit
            /// or the receive FIFO is enabled for writes and reads.
            /// 0 = Transmit FIFO is selected for both writes and reads through
            /// SSDR
            /// 1 = Receive FIFO is selected for both writes and reads through
            /// SSDR
            /// </remarks>
            [BitFieldRegister(Position = 15)]
            public bool STRF;

            /// <summary>
            /// Enable FIFO write/read (test mode bit)
            /// </summary>
            /// <remarks>
            /// Enables test mode for the SSP port.
            /// When set, the SSP port enters a mode where whenever the CPU reads
            /// or writes to the SSP Data register, it reads and writes directly to either
            /// the transmit FIFO or the receive FIFO, depending on the programmed
            /// state of <see cref="SSCR1_Bitfield.STRF"/>.
            /// In EFWR test mode, data is not transmitted on SSPTXDx, data input on
            /// SSPRXDx is not stored, and the Busy and ROR bits have no effect.
            /// However, the Interrupt Test register is still functional. Using software,
            /// this mode can test whether or not the TX FIFO or the RX FIFO operates
            /// properly as a FIFO memory stack. Verify that the <see cref="SSSR_Bitfield.CSS"/> bit has
            /// gone from set to clear before reading the TX FIFO. This bit must be
            /// cleared for normal operation.
            /// When <see cref="SSCR1_Bitfield.STRF"/> is clear, writes to SSDR are performed on the
            /// Transmit FIFO, and reads from SSDR read back the data written to
            /// the TX FIFO in first-in-first-out order. When the STRF is set, writes to
            /// SSDR are performed on the RX FIFO, and reads from SSDR read
            /// back the data written to the RX FIFO in first-in-first-out order.
            /// 0 = FIFO write/read special function is disabled (normal SSP port
            /// operational mode)
            /// 1 = FIFO write/read special function is enabled.
            /// </remarks>
            [BitFieldRegister(Position = 14)]
            public bool EFWR;

            /// <summary>
            /// Receive FIFO Threshold
            /// </summary>
            /// <remarks>
            /// RFT sets the level at or above which the FIFO controller triggers a DMA
            /// service request (if enabled) and a CPU interrupt request (if enabled).
            /// This level must be set to the desired trigger threshold value minus 1.
            /// NOTE: Do not to set the value of RFT too high for the system;
            /// otherwise, the receive FIFO can overrun because of the bus
            /// latencies caused by other internal and external peripherals.
            /// This is especially important when using interrupts and polled
            /// modes that require a longer time to service.
            /// </remarks>
            [BitFieldRegister(Position = 10, Size=4)]
            public uint RFT;

            /// <summary>
            /// Transmit FIFO Threshold
            /// </summary>
            /// <remarks>
            /// TFT sets the level at or below which the FIFO controller triggers a DMA
            /// service request (if enabled) and a CPU interrupt request (if enabled).
            /// This level must be set to the desired trigger threshold value minus 1.
            /// NOTE: Do not set the value of TFT too low for the system; otherwise,
            /// the transmit FIFO can underrun because of the bus latencies
            /// caused by other internal and external peripherals. This is
            /// especially important when using interrupts and polled modes
            /// that require a longer time to service.
            /// </remarks>
            [BitFieldRegister(Position = 6, Size=4)]
            public uint TFT;

            /// <summary>
            /// Microwire transit data size
            /// </summary>
            /// <remarks>
            /// MWDS selects between an eight bit or 16-bit size for the command
            /// word transmitted using the Microwire protocol. MWDS is ignored for all
            /// other frame formats.
            /// 0 = 8-bit command word is transmitted.
            /// 1 = 16-bit command word is transmitted.
            /// </remarks>
            [BitFieldRegister(Position = 5)]
            public bool MWDS;

            /// <summary>
            /// Motorola SPI SSPSCLKx Phase
            /// </summary>
            /// <remarks>
            /// SPH determines the phase relationship between SSPSCLKx and the
            /// SSPSFRMx when the Motorola SPI format is selected. When SPH is
            /// clear, SSPSCLKx remains in its Inactive/Idle state (as determined by
            /// the <see cref="SSCR1_Bitfield.SPO"/> setting) for one full cycle after SSPSFRMx is
            /// asserted low at the beginning of a frame. SSPSCLKx continues to
            /// toggle for the rest of the frame and is then held in its Inactive state for
            /// one-half of an SSPSCLKx period before SSPSFRMx is deasserted high
            /// at the end of the frame.
            /// When SPH is set, SSPSCLKx remains in its Inactive/Idle state (as
            /// determined by the <see cref="SSCR1_Bitfield.SPO"/> bit’s value) for one-half cycle after
            /// SSPSFRMx is asserted low at the beginning of a frame. SSPSCLKx
            /// continues to toggle for the remainder of the frame, and is then held in its
            /// Inactive state for one full SSPSCLKx period before SSPSFRMx is
            /// deasserted high at the end of the frame. The combination of the
            /// <see cref="SSCR1_Bitfield.SPO"/> bit and <see cref="SSCR1_Bitfield.SPH"/> bit settings determines when
            /// SSPSCLKx is active during the assertion of SSPSFRMx, and which
            /// SSPSCLKx edge transmits and receives data on the SSPTXDx and
            /// SSPRXDx pins.
            /// When <see cref="SSCR1_Bitfield.SPO"/> and <see cref="SSCR1_Bitfield.SPH"/> are programmed to the
            /// same value (both clear or both set), SSPTXDx is driven on the falling
            /// edge of SSPSCLKx, and SSPRXDx is latched on the rising edge of
            /// SSPSCLKx. When <see cref="SSCR1_Bitfield.SPO"/> and <see cref="SSCR1_Bitfield.SPH"/> are
            /// programmed to opposite values (one clear and the other set),
            /// SSPTXDx is driven on the rising edge of SSPSCLKx and SSPRXDx is
            /// latched on the falling edge of SSPSCLKx.
            /// NOTE: SPH is ignored for all data frame formats except for the
            /// Motorola SPI format (<see cref="SSCR1_Bitfield.SPO"/> and <see cref="SSCR1_Bitfield.SPH"/>.
            /// <see cref="SSCR1_Bitfield.SPO"/>).
            /// inverts the polarity of SSPSCLKx, and <see cref="SSCR1_Bitfield.SPH"/> determines the
            /// phase relationship between SSPSCLKx and SSPSFRMx, shifting the
            /// SSPSCLKx one-half phase to the left or right during the assertion of
            /// SSPSFRMx.
            /// 0 = SSPSCLKx is inactive one cycle at the start of a frame and 1/2
            /// cycle at the end of a frame.
            /// 1 = SSPSCLKx is inactive 1/2 cycle at the start of a frame and one
            /// cycle at the end of a frame.
            /// </remarks>
            [BitFieldRegister(Position = 4)]
            public bool SPH;

            /// <summary>
            /// Motorol SPI SSPSCLKx Polarity
            /// </summary>
            /// <remarks>
            /// SPO selects the polarity of the inactive state of SSPSCLKx when the
            /// SPI protocol is selected.
            /// The programmed setting of SPO alone does not determine which
            /// SSPSCLKx edge transmits or receives data; SPO in combination with
            /// <see cref="SSCR1_Bitfield.SPH"/> does.
            /// NOTE: <see cref="SSCR1_Bitfield.SPO"/> is ignored for all data frame formats except for
            /// SPI protocol (<see cref="SSCR0_Bitfield.FRF"/> = 0b00).
            /// 0 = SSPSCLKx is held low in the inactive or idle state when the SSP
            /// port is not transmitting/receiving data.
            /// 1 = SSPSCLKx is held high during the inactive or idle state.
            /// </remarks>
            [BitFieldRegister(Position = 3)]
            public bool SPO;

            /// <summary>
            /// Loop-back mode (test-mode bit only)
            /// </summary>
            /// <remarks>
            /// LBM is a test mode bit that enables and disables the ability of the SSP
            /// port’s transmit and receive logic to communicate.
            /// NOTE: The loop-back mode cannot be used with the Microwire
            /// protocol since this protocol uses half-duplex master-slave
            /// message passing.
            /// 0 = Normal SSP port operation is enabled.
            /// 1 = Output of transmit serial shifter is internally connected to the input
            /// of the receive serial shifter. SSPTXDx continues to function
            /// normally.
            /// </remarks>
            [BitFieldRegister(Position = 2)]
            public bool LBM;

            /// <summary>
            /// Transmit FIFO interrupt enable
            /// </summary>
            /// <remarks>
            /// TIE enables the TX FIFO service request interrupt.
            /// NOTE: Clearing TIE does not affect the current state of <see cref="SSSR_Bitfield.TFS"/>
            /// or the ability of the transmit FIFO logic to set and clear
            /// <see cref="SSSR_Bitfield.TFS"/>—it blocks only the generation of the interrupt
            /// request. Also, the state of TIE does not effect the generation of
            /// the transmit FIFO DMA service request, which is asserted
            /// whenever <see cref="SSSR_Bitfield.TFS"/> is set.
            /// 0 = TX FIFO level interrupt is disabled. The interrupt is masked and
            /// the state of <see cref="SSSR_Bitfield.TFS"/> is ignored.
            /// 1 = TX FIFO level interrupt is enabled. Whenever <see cref="SSSR_Bitfield.TFS"/> is
            /// set, an interrupt request is made to the interrupt controller.
            /// </remarks>
            [BitFieldRegister(Position = 1)]
            public bool TIE;

            /// <summary>
            /// Receive FIFO interrupt enable
            /// </summary>
            /// <remarks>
            /// RIE enables the RX FIFO service request interrupt.
            /// NOTE: Clearing RIE does not affect the current state of <see cref="SSSR_Bitfield.RFS"/>
            /// or the ability of the RX FIFO logic to set and clear
            /// <see cref="SSSR_Bitfield.RFS"/>—it blocks only the generation of the interrupt
            /// request. The state of RIE does not affect the generation of the
            /// RX FIFO DMA service request, which is asserted whenever
            /// <see cref="SSSR_Bitfield.RFS"/> is set.
            /// 0 = RX FIFO level interrupt is disabled. The interrupt is masked and
            /// <see cref="SSSR_Bitfield.RFS"/> is ignored.
            /// 1 = RX FIFO level interrupt is enabled. Whenever <see cref="SSSR_Bitfield.RFS"/> is
            /// set, an interrupt request is made to the interrupt controller.
            /// </remarks>
            [BitFieldRegister(Position = 0)]
            public bool RIE;
        }
        #endregion

        #region SSSR Bitfield
        /// <summary>
        /// SSP status register
        /// </summary>
        /// <remarks>
        /// SSSR contains bit fields that signal overrun errors and the transmit and
        /// receive FIFO DMA service requests. Each of these hardware-detected events signal an interrupt
        /// request to the interrupt controller. The status register also contains flags that indicate:
        /// <list type="bullet">
        /// <item><description>When the SSP port is actively transmitting data</description></item>
        /// <item><description>When the TX FIFO is not full</description></item>
        /// <item><description>When the RX FIFO is not empty</description></item>
        /// </list>
        /// One interrupt signal is sent to the interrupt controller for each SSP port. These events can cause an
        /// interrupt:
        /// <list type="bullet">
        /// <item><description>End-of-chain</description></item>
        /// <item><description>Receiver time-out</description></item>
        /// <item><description>Peripheral trailing byte</description></item>
        /// <item><description>RX FIFO overrun</description></item>
        /// <item><description>RX FIFO request</description></item>
        /// <item><description>TX FIFO request</description></item>
        /// </list>
        /// An interrupt is signaled as long as the bits are set. The interrupt clears when the bits are cleared.
        /// Read and write bits are called status bits (status bits are referred to as sticky and once set by
        /// hardware, they must be cleared by software); read-only bits are called flags. Writing 0b1 to a sticky
        /// status bit clears it; writing 0b0 has no effect. Read-only flags are set and cleared by hardware;
        /// writes have no effect. The reset state of read-write bits is zero and all bits return to their reset state
        /// when <see cref="SSCR0_Bitfield.SSE"/> is cleared. Additionally, some bits that cause interrupts have
        /// corresponding mask bits in the control registers.
        /// </remarks>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSSR_Bitfield
        {
            /// <summary>
            /// Bit count error
            /// </summary>
            /// <remarks>
            /// BCE is a read-write status bit that indicates that the SSP port has
            /// detected that SSPSFRMx has been asserted at an incorrect time. This
            /// causes an interrupt (if enabled by <see cref="SSCR1_Bitfield.EBCEI"/>). When set, BCE
            /// indicates that an error occurred and that to get re-synchronized to the
            /// master device, the SSP port may disregard both the sample that had
            /// SSPDFRMx re-asserted in the middle of it AND the next sample.
            /// <p/>
            /// NOTE: BCE does not operate in Motorola SPI mode.
            /// <p/>
            /// NOTE: To clear BCE, write 0b1 to it.
            /// <p/>
            /// 0 = The SSP port has not experienced a bit count error
            /// 1 = SSPSFRMx has been asserted when the bit counter was not 0
            /// <p/>
            /// note: This bit is cleared on Write!
            /// </remarks>
            [BitFieldRegister(Position = 23)]
            public bool BCE;

            /// <summary>
            /// Clock synchronization status
            /// </summary>
            /// <remarks>
            /// CSS indicates that the SSP port is busy synchronizing the control
            /// signals into the SSPSCLKx domain. Software only needs to check CSS
            /// when the SSP port is a slave to SSPSFRMx. Software must wait until
            /// CSS is cleared before allowing an external device to assert
            /// SSPSFRMx.
            /// <p/>
            /// 0 = The SSP port is ready for slave clock operations
            /// 1 = The SSP port is currently busy synchronizing slave mode signals
            /// </remarks>
            [BitFieldRegister(Position = 22, Modifiers = BitFieldModifier.ReadOnly)]
            public bool CSS;

            /// <summary>
            /// Transmit FIFO underrung
            /// </summary>
            /// <remarks>
            /// TUR indicates that the transmitter tried to send data from the TX FIFO
            /// when the TX FIFO was empty. When set, an interrupt is generated to
            /// the CPU (that can be locally masked by the <see cref="SSCR0_Bitfield.TIM"/> bit).
            /// Setting TUR does not generate any DMA service request. TUR remains set
            /// until cleared by software writing 0b1 to it (which also resets its interrupt
            /// request). Writing 0b0 to TUR has no effect.
            /// TUR can be set when the SSP port is a slave to SSPSFRMx
            /// (<see cref="SSCR1_Bitfield.SFRMDIR"/> set), or if the SSP port is a master to SSPSFRMx
            /// and the SSP port is in network mode. TUR is not set if the SSP port is in
            /// receive-without-transmit mode (<see cref="SSCR1_Bitfield.RWOT"/> set).
            /// 0 = TX FIFO has not experienced an underrun
            /// 1 = Transmitter tried to send data from the TX FIFO when the FIFO
            /// was empty, an interrupt is signaled
            /// <p/>
            /// note: This bit is cleared on Write!
            /// </remarks>
            [BitFieldRegister(Position = 21)]
            public bool GCAD;

            /// <summary>
            /// Endo of chain
            /// </summary>
            /// <remarks>
            /// EOC indicates that the DMA has signaled an end of chain. The end- ofchain
            /// event indicates that the DMA descriptor for the RX FIFO is
            /// ending. This event requires software intervention if data remains in the
            /// RX FIFO.
            /// <p/>
            /// NOTE: To clear EOC, write 0b1 to it.
            /// 0 = DMA has not signaled an end of chain condition.
            /// 1 = DMA has signaled an end of chain condition. An EOC interrupt is
            /// only generated when either there are trailing bytes left (the PINT
            /// bit is set) or there are no trailing bytes (the TINT bit is set).
            /// EOC bit is always set, but does not generate an interrupt if neither
            /// of the these conditions are met.
            /// <p/>
            /// note: This bit is cleared on Write!
            /// </remarks>
            [BitFieldRegister(Position = 20)]
            public bool EOC;

            /// <summary>
            /// Time-out interrupt
            /// </summary>
            /// <remarks>
            /// TINT indicates that the RX FIFO has been idle (no samples received)
            /// for the period of time defined by the value programmed within SSTO.
            /// This interrupt can be masked by <see cref="SSCR1_Bitfield.TINTE"/>.
            /// NOTE: To clear TINT, write 0b1 to it.
            /// 0 = No Receiver Time-out has occurred
            /// 1 = Receiver Time-out has occurred
            /// <p/>
            /// note: This bit is cleared on Write!
            /// </remarks>
            [BitFieldRegister(Position = 19)]
            public bool TINT;

            /// <summary>
            /// Peripheral trailing byte interrupt
            /// </summary>
            /// <remarks>
            /// PINT indicates that a DMA end of chain event has occurred and there is
            /// data within the RX FIFO. This event requires the CPU or DMA to
            /// transfer the remaining bytes from the RX FIFO.
            /// <p/>
            /// This interrupt can be masked by <see cref="SSCR1_Bitfield.PINTE"/>.
            /// NOTE: To clear PINT, write 0b1 to it.
            /// 0 = No peripheral trailing byte interrupt is pending.
            /// 1 = Peripheral trailing byte interrupt is pending
            /// <p/>
            /// note: This bit is cleared on Write!
            /// </remarks>
            [BitFieldRegister(Position = 18)]
            public bool PINT;

            /// <summary>
            /// RX FIFO Level
            /// </summary>
            /// <remarks>
            /// RFL is the number of valid entries (minus 1) currently in the RX FIFO.
            /// NOTE: When the value of 0xF is read, the RX FIFO is either empty or
            /// full and programmers must refer to the RNE bit.
            /// </remarks>
            [BitFieldRegister(Position = 12, Size=4, Modifiers = BitFieldModifier.ReadOnly)]
            public uint RFL;

            /// <summary>
            /// TX FIFO Level
            /// </summary>
            /// <remarks>
            /// TFL is the number of valid entries currently in the TX FIFO.
            /// NOTE: When the value of 0x0 is read, the TX FIFO is either empty or
            /// full and programmers must refer to the TNF bit.
            /// </remarks>
            [BitFieldRegister(Position = 8, Size = 4, Modifiers = BitFieldModifier.ReadOnly)]
            public uint TFL;

            /// <summary>
            /// RX FIFO Overrun
            /// </summary>
            /// <remarks>
            /// ROR indicates that the Receive logic attempted to place data into the
            /// RX FIFO after it had been completely filled. When new data is received,
            /// ROR is asserted and the newly received data is discarded. This
            /// process is repeated for all new data received until at least one empty
            /// RX FIFO location exists.
            /// When set, an interrupt is generated to the CPU that can be locally
            /// masked by the <see cref="SSCR0_Bitfield.RIM"/> bit. Setting ROR does not
            /// generate any DMA service request. Clearing ROR resets its interrupt request.
            /// NOTE: To clear ROR, write 0b1 to it.
            /// 0 = RX FIFO has not experienced an overrun
            /// 1 = Attempted data write to a full RX FIFO, request an interrupt
            /// <p/>
            /// note: This bit is cleared on Write!
            /// </remarks>
            [BitFieldRegister(Position = 7)]
            public bool ROR;

            /// <summary>
            /// Receive FIFO Service
            /// </summary>
            /// <remarks>
            /// A RFS request indicates that the RX FIFO requires service to prevent
            /// an overrun. RFS is set when the number of valid entries in the RX FIFO
            /// is equal to or greater than the RX FIFO trigger threshold. RFS is cleared
            /// when the RX FIFO has fewer entries than the trigger threshold. When
            /// RFS is set, an interrupt is generated if <see cref="SSCR1_Bitfield.RIE"/> is set. When RFS
            /// is set, a DMA service request is generated if <see cref="SSCR1_Bitfield.RSRE"/> is set.
            /// After the CPU or DMA reads the RX FIFO such that it has fewer entries
            /// than the value of <see cref="SSCR1_Bitfield.RFT"/>, RFS (and the service request and/or
            /// interrupt) is automatically cleared. <see cref="SSCR1_Bitfield.RSRE"/> and
            /// <see cref="SSCR1_Bitfield.RIE"/> must not both be set.
            /// 0 = RX FIFO level is less than its trigger threshold or the SSP port is
            /// disabled
            /// 1 = RX FIFO level is equal to or above its trigger threshold, an
            /// interrupt or DMA service request is generated.
            /// </remarks>
            [BitFieldRegister(Position = 6, Modifiers = BitFieldModifier.ReadOnly)]
            public bool RFS;

            /// <summary>
            /// Transmit FIFO Service.
            /// </summary>
            /// <remarks>
            /// A TFS request indicates that the TX FIFO requires service to prevent an
            /// underrun. TFS is set when the number of valid entries in the TX FIFO is
            /// equal to or less than the TX FIFO trigger threshold. TFS is cleared
            /// when the TX FIFO has more entries than the trigger threshold. When
            /// TFS is set, an interrupt is generated if <see cref="SSCR1_Bitfield.TIE"/> is set. When TFS
            /// is set, a DMA service request is generated if <see cref="SSCR1_Bitfield.TSRE"/> is set.
            /// After the CPU or DMA fills the TX FIFO such that it has more entries
            /// than the value of <see cref="SSCR1_Bitfield.TFT"/>, TFS (and the service request and/or
            /// interrupt) is automatically cleared. <see cref="SSCR1_Bitfield.TSRE"/>
            /// and <see cref="SSCR1_Bitfield.TIE"/> must not both be set.
            /// 0 = TX FIFO level exceeds its threshold (TFT + 1) or the SSP port is
            /// disabled
            /// 1 = TX FIFO level is at or below its trigger threshold (TFT + 1), an
            /// interrupt or DMA service request is generated.
            /// </remarks>
            [BitFieldRegister(Position = 5, Modifiers = BitFieldModifier.ReadOnly)]
            public bool TFS;

            /// <summary>
            /// Busy
            /// </summary>
            /// <remarks>
            /// BSY is automatically set when the SSP port is actively transmitting
            /// and/or receiving data and BSY is automatically cleared when the SSP
            /// port is idle or disabled. BSY does not generate an interrupt.
            /// NOTE: When the SSP port is a master of a clock, software determines
            /// if the SSP port is active by monitoring <see cref="SSSR_Bitfield.TFL"/> and
            /// <see cref="SSSR_Bitfield.BSY"/>. If the SSP port is a slave to a clock, software
            /// determines if the SSP port is active by monitoring
            /// <see cref="SSSR_Bitfield.TFL"/>, <see cref="SSSR_Bitfield.RFL"/>
            /// and <see cref="SSSR_Bitfield.BSY"/> along with the
            /// SSTO register. Also, using the time-out feature (the SSTO
            /// register and <see cref="SSCR1_Bitfield.TRAIL"/>) to handle trailing bytes provides
            /// an indication of when the master has completed sending data.
            /// 0 = SSP port is idle or disabled
            /// 1 = SSP port is actively transmitting or receiving data
            /// </remarks>
            [BitFieldRegister(Position = 4, Modifiers = BitFieldModifier.ReadOnly)]
            public bool BSY;

            /// <summary>
            /// RX FIFO not empty
            /// </summary>
            /// <remarks>
            /// RNE indicates that the RX FIFO contains one or more entries of valid
            /// data. RNE is automatically cleared when the RX FIFO no longer
            /// contains any valid data. This bit does not generate an interrupt.
            /// When using programmed I/O, RNE can be polled to remove remaining
            /// bytes of data from the RX FIFO since CPU interrupt requests are made
            /// only when the RX FIFO trigger threshold has been met or exceeded.
            /// 0 = RX FIFO is empty.
            /// 1 = RX FIFO is not empty.
            /// </remarks>
            [BitFieldRegister(Position = 3, Modifiers = BitFieldModifier.ReadOnly)]
            public bool RNE;

            /// <summary>
            /// TX FIFO not full
            /// </summary>
            /// <remarks>
            /// TNF indicates that the TX FIFO contains one or more entries that do not
            /// contain valid data. TNF is automatically cleared when the TX FIFO is
            /// completely full. TNF does not generate an interrupt.
            /// When using programmed I/O, TNF can be polled to fill the TX FIFO
            /// beyond its trigger threshold.
            /// 0 = TX FIFO is full
            /// 1 = TX FIFO is not full
            /// </remarks>
            [BitFieldRegister(Position = 2, Modifiers = BitFieldModifier.ReadOnly)]
            public bool TNF;
        }
        #endregion

        #region SSITR Bitfield
        /// <summary>
        /// SSITR, contains bit fields used for testing purposes only.
        /// </summary>
        /// <remarks>
        /// Setting bits in this register causes the SSP port controller to generate interrupts and DMA requests
        /// if they are enabled. This is useful in testing the port’s functionality.
        /// Setting any of these bits also causes corresponding status bits to be set in <see cref="SSSR_Bitfield"/>.
        /// The interrupt or DMA service request, caused by the setting of one of these bits remains active until
        /// the bit is cleared. This register must be 0 for normal operation.
        /// </remarks>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSITR1_Bitfield
        {
            /// <summary>
            /// Test RX FIFO overrun
            /// </summary>
            /// <remarks>
            /// 0 = No receive FIFO overrun DMA service request is generated.
            /// 1 = Generates a non-maskable interrupt to the CPU. No DMA request
            /// is generated. Write 0b0 to clear.
            /// </remarks>
            [BitFieldRegister(Position = 7)]
            public bool TROR;

            /// <summary>
            /// Test RX FIFO Service Request
            /// </summary>
            /// <remarks>
            /// 0 = No receive FIFO DMA service request is generated.
            /// 1 = Generates a non-maskable interrupt to the CPU and a DMA
            /// request for the RX FIFO. Write 0b0 to clear.
            /// </remarks>
            [BitFieldRegister(Position = 6)]
            public bool TRFS;

            /// <summary>
            /// Test TX Service Request
            /// </summary>
            /// <remarks>
            /// 0 = No transmit FIFO DMA service request is generated.
            /// 1 = Generates a non-maskable interrupt to the CPU and a DMA
            /// request for the TX FIFO. Write 0b0 to clear.
            /// </remarks>
            [BitFieldRegister(Position = 5)]
            public bool TTFS;
        }
        #endregion

        #region SSPSP Bitfield
        /// <summary>
        /// SSITR contains bit fields used for testing purposes only.
        /// Setting bits in this register causes the SSP port controller to generate interrupts and DMA requests
        /// if they are enabled. This is useful in testing the port’s functionality.
        /// Setting any of these bits also causes corresponding status bits to be set in <see cref="SSSR_Bitfield"/>.
        /// The interrupt or DMA service request, caused by the setting of one of these bits remains active until the
        /// bit is cleared. This register must be 0 for normal operation.
        /// </summary>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSPSP1_Bitfield
        {
            /// <summary>
            /// Frame sync relative timing
            /// </summary>
            /// <remarks>
            /// 0 = Next frame is asserted after the end of the T4 timing
            /// 1 = Next frame is asserted with the LSB of the previous frame
            /// NOTE: When FSRT is set, SSPSFRMx corresponding to the next
            /// sample is asserted during the transmission of the LSB from the
            /// current sample
            /// </remarks>
            [BitFieldRegister(Position = 25)]
            public bool FSRT;

            /// <summary>
            /// Dummy stop
            /// </summary>
            /// <remarks>
            /// DMYSTOP determines the number of cycles that SSPSCLKx is active
            /// following the last bit (bit 0) of transmitted data (SSPTXDx) or received
            /// data (SSPRXDx). The value must be from 0 to 3. DMYSTOP must be
            /// cleared when PSP format is used in network mode and/or when FSRT
            /// is set.
            /// </remarks>
            [BitFieldRegister(Position = 23, Size=2)]
            public uint DMYSTOP;

            /// <summary>
            /// Serial Frame Width
            /// </summary>
            /// <remarks>
            /// SFRMWDTH determines the number of SSPSCLKx cycles that
            /// SSPSFRMx is active.
            /// The programmed value must not be asserted past the end of
            /// DMYSTOP. The value must be from 1 to 44.
            /// In PSP slave mode (<see cref="SSCR1_Bitfield.SFRMDIR"/> is set), SFRMWDTH is
            /// ignored. The incoming SSPSFRMx must be asserted for a duration of at
            /// least one SSPSCLKx cycle for each sample. The incoming SSPSFRMx
            /// and first data bit of the sample can be asserted at the same time.
            /// Between samples, the incoming SSPSFRMx must be deasserted for a
            /// duration of at least one SSPSCLKx cycle.
            /// </remarks>
            [BitFieldRegister(Position = 16, Size=6)]
            public uint SFRMWDTH;

            /// <summary>
            /// Serial Frame Delay
            /// </summary>
            /// <remarks>
            /// SFRMDLY determines the number of half SSPSCLKx cycles that
            /// SSPSFRMx is delayed from the start of the transfer to the time
            /// SSPSFRMx is asserted. The value must be from 0 to 88.
            /// </remarks>
            [BitFieldRegister(Position = 9, Size=7)]
            public uint SFRMDLY;

            /// <summary>
            /// Dummy Start
            /// </summary>
            /// <remarks>
            /// DMYSTRT determines the number of SSPSCLKx cycles after
            /// STRTDLY and before transmitted data (SSPTXDx) or received data
            /// (SSPRXDx).
            /// </remarks>
            [BitFieldRegister(Position = 7, Size=2)]
            public uint DMYSTRT;

            /// <summary>
            /// Start Delay
            /// </summary>
            /// <remarks>
            /// STRTDLY determines the number of cycles that SSPSCLKx remains in
            /// its Idle state between data transfers. The STRTDLY field must be
            /// cleared if the SSPSCLKENx, <see cref="SSCR1_Bitfield.ECRA"/>,
            /// or <see cref="SSCR1_Bitfield.ECRB"/>
            /// clock enables are used. The STRTDLY field must be cleared whenever
            /// SSPSCLKx or SSPSFRMx is configured as an input
            /// (<see cref="SSCR1_Bitfield.SCLKDIR"/> or <see cref="SSCR1_Bitfield.SFRMDIR"/>
            /// are set). The value must be from 0 to 7.
            /// </remarks>
            [BitFieldRegister(Position = 4, Size=3)]
            public uint STRTDLY;

            /// <summary>
            /// End-of-Transfer Data State
            /// </summary>
            /// <remarks>
            /// ETDS determines the state of SSPTXDx at the end of a transfer. When
            /// cleared, the state of SSPTXDx is forced low after the LSB of the frame
            /// is sent and remains low through the next idle period. When set, the
            /// state of SSPTXDx retains the value of the LSB through the next idle
            /// period.
            /// NOTE: ETDS has no effect if <see cref="SSCR1_Bitfield.TTE"/> is set.
            /// NOTE: ETDS bit has no effect when configured in TI Synchronous
            /// Serial Protocol.
            /// 0 = Low
            /// 1 = Last Value &lt;Bit 0&gt;
            /// </remarks>
            [BitFieldRegister(Position = 3)]
            public bool ETDS;

            /// <summary>
            /// Serial Frame Polarity
            /// </summary>
            /// <remarks>
            /// SFRMP determines the active state of SSPSFRMx.
            /// In idle mode or when the SSP port is disabled, SSPSFRMx is in its
            /// inactive state. In slave mode (<see cref="SSCR1_Bitfield.SFRMDIR"/> is set), SFRMP
            /// indicates the polarity of the incoming SSPSFRMx.
            /// 0 = SSPSFRMx is active low.
            /// 1 = SSPSFRMx is active high.
            /// </remarks>
            [BitFieldRegister(Position = 2)]
            public bool SFRMP;

            /// <summary>
            /// Serial Bit-Rate clock mode
            /// </summary>
            /// <remarks>
            /// SCMODE selects one of four serial clock modes when PSP format is
            /// used (<see cref="SSCR0_Bitfield.FRF"/> = 0b11).
            /// Its operation is similar to how <see cref="SSCR1_Bitfield.SPO"/>
            /// and <see cref="SSCR1_Bitfield.SPH"/>
            /// together determine the idle state of SSPSCLKx and on which edges
            /// data is driven and sampled.
            /// <list type="bullet">
            /// <item><description>0b00 = Data Driven (Falling), Data Sampled (Rising), Idle State (Low)</description></item>
            /// <item><description>0b01 = Data Driven (Rising), Data Sampled (Falling), Idle State (Low)</description></item>
            /// <item><description>0b10 = Data Driven (Rising), Data Sampled (Falling), Idle State (High)</description></item>
            /// <item><description>0b11 = Data Driven (Falling), Data Sampled (Rising), Idle State (High)</description></item>
            /// </list>
            /// NOTE: For all selections of SCMODE, the Idle State is high impedance
            /// when <see cref="SSCR1_Bitfield.TIE"/> is set.
            /// </remarks>
            [BitFieldRegister(Position = 0, Size=2)]
            public uint SCMODE;
        }
        #endregion

        #region SSTSS Bitfield
        /// <summary>
        /// These registers indicate which time slot the SSP port is currently in. SSTSS are ignored when the
        /// SSP port is not in network mode.
        /// </summary>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSTSS_Bitfield
        {
            /// <summary>
            /// Network mode busy
            /// </summary>
            /// <remarks>
            /// Only if the SSP port is in network mode, NMBSY indicates when the SSP
            /// port is in the middle of a frame. NMBSY can be used by software when a
            /// clean shutdown of the SSP port is needed. Software must ensure that the
            /// TX FIFO is either empty (or will be empty at the end of the next frame),
            /// deactivate the TX DMA requests, clear the <see cref="SSCR0_Bitfield.MOD"/> bit;
            /// then software must poll NMBSY until it is 0 before disabling the SSP port (by
            /// clearing the <see cref="SSCR0_Bitfield.SSE"/> bit).
            /// When the SSP port is a master of SSPSFRMx, NMBSY is set. If the SSP
            /// port is a slave to SSPSFRMx, NMBSY is set only if the current frame
            /// (number of bits per sample number of time slots per frame) has not expired
            /// since SSPSFRMx was asserted.
            /// 0 = No SSPSFRMx is currently asserted (network mode only)
            /// 1 = SSPSFRMx is currently asserted (network mode only)
            /// </remarks>
            [BitFieldRegister(Position = 31, Modifiers= BitFieldModifier.ReadOnly)]
            public bool NMBSY;

            /// <summary>
            /// Time slot status
            /// </summary>
            /// <remarks>
            /// Only if the SSP port is in network mode, the 3-bit TSS value indicates which
            /// time slot the SSP port is in. Due to synchronization delays between clock
            /// domains, the TSS value is a delayed version of the actual time slot.
            /// </remarks>
            [BitFieldRegister(Position = 0, Size=3, Modifiers = BitFieldModifier.ReadOnly)]
            public uint TSS;
        }
        #endregion

        #region SSACD Bitfield
        /// <summary>
        /// SSACD select which clock frequency is sent to the SSP port and then to SSPSYSCLKx and
        /// SSPSCLKx. If <see cref="SSCR0_Bitfield.SCR"/> is not 0, there is no guaranteed phase relationship between
        /// SSPSYSCLKx and SSPSCLKx. SSPSYSCLKx’s frequency is calculated by
        /// dividing the chosen PLL output clock frequency (<see cref="SSACD_Bitfield.ACPS"/>) by the chosen divider
        /// (<see cref="SSACD_Bitfield.ACDS"/>). SSPSYSCLKx is then divided by 4 (or by 1) to get SSPSCLKx
        /// SSPSFRMx’s frequency is calculated by dividing SSPSCLKx by the multiplyproduct
        /// of data size (<see cref="SSCR0_Bitfield"/>.[EDSS, DSS] values) times the number of time slots being used
        /// (<see cref="SSCR0_Bitfield.FRDC"/> value).
        /// </summary>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct SSACD_Bitfield
        {
            /// <summary>
            /// Audio Clock PLL select
            /// </summary>
            /// <remarks>
            /// The ACPS value indicates which PLL output clock is sent to the clock
            /// divider.
            /// Some combinations of ACPS and ACDS are not valid.
            /// <list type="bullet">
            /// <item><description>ACPS 0b000 results in PLL output frequency 5.622MHz</description></item>
            /// <item><description>ACPS 0b001 results in PLL output frequency 11.345MHz</description></item>
            /// <item><description>ACPS 0b010 results in PLL output frequency 12.235MHz</description></item>
            /// <item><description>ACPS 0b011 results in PLL output frequency 14.857Hz</description></item>
            /// <item><description>ACPS 0b100 results in PLL output frequency 32.842MHz</description></item>
            /// <item><description>ACPS 0b101 results in PLL output frequency 48.000MHz</description></item>
            /// <item><description>ACPS 0b110 results in PLL output frequency RESERVED</description></item>
            /// <item><description>ACPS 0b111 results in PLL output frequency RESERVED</description></item>
            /// </list>
            /// </remarks>
            [BitFieldRegister(Position = 4, Size=3)]
            public uint ACPS;

            /// <summary>
            /// SSPSYSCLK divider bypass
            /// </summary>
            /// <remarks>
            /// If SCDB is set and <see cref="SSCR0_Bitfield.ACS"/> is set, SSPSYSCLKx is divided by 1 to
            /// become SSPSCLKx. If SCDB is cleared and <see cref="SSCR0_Bitfield.ACS"/> is set),
            /// SSPSYSCLKx is divided by 4 to become SSPSCLKx. If <see cref="SSCR0_Bitfield.ACS"/> is
            /// cleared, SCDB has no effect.
            /// <p/>
            /// 0 = SSPSYSCLKx is divided by 4 to become SSPSCLKx
            /// 1 = SSPSYSCLKx is divided by 1 to become SSPSCLKx
            /// </remarks>
            [BitFieldRegister(Position = 3)]
            public bool SCDB;

            /// <summary>
            /// Audio clock divider select
            /// </summary>
            /// <remarks>
            /// The ACDS value indicates which divider creates SSPSYSCLKx.
            /// Some combinations of ACPS and ACDS are not valid.
            /// <list type="bullet">
            /// <item><description>ACDS 0b000 results in clock divider value 1</description></item>
            /// <item><description>ACDS 0b001 results in clock divider value 2</description></item>
            /// <item><description>ACDS 0b010 results in clock divider value 4</description></item>
            /// <item><description>ACDS 0b011 results in clock divider value 8</description></item>
            /// <item><description>ACDS 0b100 results in clock divider value 16</description></item>
            /// <item><description>ACDS 0b101 results in clock divider value 32</description></item>
            /// <item><description>ACDS 0b110 results in clock divider value RESERVED</description></item>
            /// <item><description>ACDS 0b111 results in clock divider value RESERVED</description></item>
            /// </list>
            /// </remarks>
            [BitFieldRegister(Position = 0, Size=3)]
            public uint ACDS;
        }
        #endregion

        #region SSP channel definition
        /// <summary>
        /// Aggregation of all registers of a SSP channel.
        /// </summary>
        [MemoryMappedPeripheral(Base=0, Length = 0x40)]
        public class SSP_Channel
        {
            #region Register Bank
            /// <summary>
            /// Control register 0
            /// </summary>
            /// <remarks>
            /// SSCR0 controls various functions within the SSP port.
            /// Before enabling the SSP port by setting <see cref="SSCR0_Bitfield.SSE"/>,
            /// the desired values for this register must be programmed.
            /// </remarks>
            [Register(Offset = 0x00000000U, Instances = 1)]
            public SSCR0_Bitfield SSCR0;// = new uint[1];

            /// <summary>
            /// Control register 1
            /// </summary>
            /// <remarks>
            /// SSCR1 controls various SSP
            /// port functions. Before enabling the port (using
            /// <see cref="SSCR0_Bitfield.SSE"/>), the desired values for this register must be set.
            /// </remarks>
            [Register(Offset = 0x00000004U, Instances = 1)]
            public SSCR1_Bitfield SSCR1;// = new uint[1];

            /// <summary>
            /// Status register
            /// </summary>
            /// <remarks>
            /// SSSR contains bit fields that signal overrun errors and the transmit and
            /// receive FIFO DMA service requests. Each of these hardware-detected events signal an interrupt
            /// request to the interrupt controller. The status register also contains flags that indicate:
            /// <list type="bullet">
            /// <item><description>When the SSP port is actively transmitting data</description></item>
            /// <item><description>When the TX FIFO is not full</description></item>
            /// <item><description>When the RX FIFO is not empty</description></item>
            /// </list>
            /// One interrupt signal is sent to the interrupt controller for each SSP port. These events can cause an
            /// interrupt:
            /// <list type="bullet">
            /// <item><description>End-of-chain</description></item>
            /// <item><description>Receiver time-out</description></item>
            /// <item><description>Peripheral trailing byte</description></item>
            /// <item><description>RX FIFO overrun</description></item>
            /// <item><description>RX FIFO request</description></item>
            /// <item><description>TX FIFO request</description></item>
            /// </list>
            /// An interrupt is signaled as long as the bits are set. The interrupt clears when the bits are cleared.
            /// Read and write bits are called status bits (status bits are referred to as sticky and once set by
            /// hardware, they must be cleared by software); read-only bits are called flags. Writing 0b1 to a sticky
            /// status bit clears it; writing 0b0 has no effect. Read-only flags are set and cleared by hardware;
            /// writes have no effect. The reset state of read-write bits is zero and all bits return to their reset state
            /// when <see cref="SSCR0_Bitfield.SSE"/> is cleared. Additionally, some bits that cause interrupts have corresponding
            /// mask bits in the control registers.
            /// </remarks>
            [Register(Offset = 0x00000008U, Instances = 1)]
            public SSSR_Bitfield SSSR;// = new uint[1];

            /// <summary>
            /// Interrupt test register
            /// </summary>
            /// <remarks>
            /// Setting bits in this register causes the SSP port controller to generate interrupts and DMA requests
            /// if they are enabled. This is useful in testing the port’s functionality.
            /// Setting any of these bits also causes corresponding status bits to be set in <see cref="SSSR_Bitfield"/>.
            /// The interrupt or DMA service request, caused by the setting of one of these bits remains active until the bit is
            /// cleared. This register must be 0 for normal operation.
            /// </remarks>
            [Register(Offset = 0x0000000CU, Instances = 1)]
            public SSITR1_Bitfield SSITR;// = new uint[1];

            /// <summary>
            /// Data write register/data read register
            /// </summary>
            /// <remarks>
            /// SSDR is a single address location that is accessed by both read and write
            /// data transfers. Each SSDR register represents two physical registers: the first register provides
            /// temporary storage for data on its way to the TX FIFO and the 2nd register provides temporary
            /// storage for data coming from the RX FIFO.
            /// <p/>
            /// As the CPU or DMA accesses the SSDR registers, FIFO control logic transfers data
            /// automatically between the registers and FIFOs as fast as the CPU or DMA moves it. Data in the
            /// FIFOs shift up or down to accommodate new word(s), unless attempting a write to a full transmit
            /// FIFO. Status bits (such as <see cref="SSSR_Bitfield"/>.[TFL, RFL, TNF, RNE]) show if the FIFO is full, above the
            /// programmable trigger threshold, below the programmable trigger threshold, or empty.
            /// For transmit data, SSDR can be loaded (written) by the processor (using programmed I/O or
            /// DMA) anytime the TX FIFO falls below its trigger threshold.
            /// <p/>
            /// When a data size of less than 32-bits is selected, do not left-justify transmit data that is written to
            /// SSDR. Transmit logic left-justifies the data and ignores any unused bits. Received data of less
            /// than 32-bits is automatically right-justified in the RX FIFO.
            /// <p/>
            /// When the SSP port is programmed for the Microwire protocol and the size of the transmit data is
            /// eight bits (<see cref="SSCR1_Bitfield.MWDS"/> cleared), the most significant 24-bits are ignored. Similarly, if the
            /// size for the Transmit data is 16-bits (<see cref="SSCR1_Bitfield.MWDS"/> set), the most significant 16-bits are
            /// ignored. <see cref="SSCR0_Bitfield.DSS"/> controls the Receive data size.
            /// <p/>
            /// Both the TX and RX FIFOs are cleared when the SSP port is reset, or by clearing <see cref="SSCR0_Bitfield.SSE"/>.
            /// </remarks>
            [Register(Offset = 0x00000010U, Instances = 1)]
            public uint SSDR;// = new uint[1];

            /// <summary>
            /// Time-out register
            /// </summary>
            /// <remarks>
            /// SSTO specifies the time-out value to signal a period of inactivity within the
            /// receive FIFO.
            /// <p/>
            /// The size of this register is 24bits.
            /// </remarks>
            [Register(Offset = 0x00000028U, Instances = 1)]
            public uint SSTO;// = new uint[1];

            /// <summary>
            /// Programmable serial protocol
            /// </summary>
            /// <remarks>
            /// SSITR contains bit fields used for testing purposes only.
            /// Setting bits in this register causes the SSP port controller to generate interrupts and DMA requests
            /// if they are enabled. This is useful in testing the port’s functionality.
            /// Setting any of these bits also causes corresponding status bits to be set in <see cref="SSSR_Bitfield"/>.
            /// The interrupt or DMA service request, caused by the setting of one of these bits remains active until the bit is
            /// cleared. This register must be 0 for normal operation.
            /// </remarks>
            [Register(Offset = 0x0000002CU, Instances = 1)]
            public SSPSP1_Bitfield SSPSP;// = new uint[1];

            /// <summary>
            /// TX timeslot active register
            /// </summary>
            /// <remarks>
            /// SSTSA are read-write registers that indicate in which time slot the SSP port transmits data.
            /// SSTSA are ignored if the SSP port is not in network mode (<see cref="SSCR0_Bitfield.MOD"/> = 1).
            /// <p/>
            /// Only if the SSP port is in network mode, the 8 TTSA bits indicate in which of
            /// 8 associated time slots the SSP port transmits. Each TTSA bit selects one
            /// time slot, respectively. Time slot bits beyond the <see cref="SSCR0_Bitfield.FRDC"/> value are
            /// ignored (if <see cref="SSCR0_Bitfield.FRDC"/> = 0b011 to select 4 time slots, then TTSA bits
            /// 7:4 are ignored). If <see cref="SSCR1_Bitfield.TTE"/> is set, then the SSP port causes
            /// SSPTXD to be high impedance during time slots where associated TTSA
            /// bits are programmed to 0.
            /// 0 = SSP port does not transmit data in this time slot
            /// 1 = SSP port transmits data in this time slot
            /// <p/>
            /// note: valid are the lowest 8 bits.
            /// </remarks>
            [Register(Offset = 0x00000030U, Instances = 1)]
            public uint SSTSA;// = new uint[1];

            /// <summary>
            /// RX timeslot active register
            /// </summary>
            /// <remarks>
            /// SSRSA are read-write registers that indicate in which time slots the SSP port receives data.
            /// SSRSA are ignored if the SSP port is not in network mode.
            /// <p/>
            /// Only if the SSP port is in network mode, the 8 RTSA bits indicate in which of
            /// 8 associated time slots the SSP port receives data. Each RTSA bit selects
            /// one time slot, respectively. Time slot bits beyond the <see cref="SSCR0_Bitfield.FRDC"/>
            /// value is ignored (if <see cref="SSCR0_Bitfield.FRDC"/> = 0b011 to select 4 time slots, then
            /// RTSA bits 7:4 are ignored).
            /// 0 = SP port does not receive data in this time slot
            /// 1 = SSP port receives data in this time slot
            /// <p/>
            /// note: valid are the lowest 8 bits.
            /// </remarks>
            [Register(Offset = 0x00000034U, Instances = 1)]
            public uint SSRSA;// = new uint[1];

            /// <summary>
            /// Timeslot status register
            /// </summary>
            /// <remarks>
            /// These registers indicate which time slot the SSP port is currently in. SSTSS are ignored when the
            /// SSP port is not in network mode.
            /// </remarks>
            [Register(Offset = 0x00000038U, Instances = 1)]
            public SSTSS_Bitfield SSTSS;// = new uint[1];

            /// <summary>
            /// Audio clock divider register
            /// </summary>
            /// <remarks>
            /// SSACD select which clock frequency is sent to the SSP port and then to SSPSYSCLKx and
            /// SSPSCLKx. If <see cref="SSCR0_Bitfield.SCR"/> is not 0, there is no guaranteed phase relationship between
            /// SSPSYSCLKx and SSPSCLKx. SSPSYSCLKx’s frequency is calculated by
            /// dividing the chosen PLL output clock frequency (<see cref="SSACD_Bitfield.ACPS"/>) by the chosen divider
            /// (<see cref="SSACD_Bitfield.ACDS"/>). SSPSYSCLKx is then divided by 4 (or by 1) to get SSPSCLKx. SSPSFRMx’s
            /// frequency is calculated by dividing SSPSCLKx by the multiplyproduct
            /// of data size (<see cref="SSCR0_Bitfield"/>.[EDSS, DSS] values) times the number of time slots being used
            /// (<see cref="SSCR0_Bitfield.FRDC"/> value).
            /// </remarks>
            [Register(Offset = 0x0000003CU, Instances = 1)]
            public SSACD_Bitfield SSACD;// = new uint[1];
            #endregion
        }
        #endregion

        #region Available SSP channels (SSP1 - SSP3)
        /// <summary>
        /// SSP channel 1
        /// </summary>
        [Register(Offset = 0)]
        public SSP_Channel SSP1;

        /// <summary>
        /// SSP channel 2
        /// </summary>
        [Register(Offset = 0x700000)]
        public SSP_Channel SSP2;

        /// <summary>
        /// SSP channel 3
        /// </summary>
        [Register(Offset = 0x900000)]
        public SSP_Channel SSP3;
        #endregion

        #region Singleton data
#if TESTMODE_DESKTOP
        private static SSP s_instance;
        /// <summary>
        /// The singleton for the SSP subsystem.
        /// </summary>
        public static SSP Instance
        {
            get
            {
                if (null != s_instance) return s_instance;

                s_instance = new SSP();
                s_instance.SSP1 = new SSP_Channel();
                s_instance.SSP1.SSACD = new SSACD_Bitfield();
                s_instance.SSP1.SSCR0 = new SSCR0_Bitfield();
                s_instance.SSP1.SSCR1 = new SSCR1_Bitfield();
                s_instance.SSP1.SSITR = new SSITR1_Bitfield();
                s_instance.SSP1.SSPSP = new SSPSP1_Bitfield();
                s_instance.SSP1.SSSR = new SSSR_Bitfield();
                s_instance.SSP1.SSTSS = new SSTSS_Bitfield();

                s_instance.SSP2 = new SSP_Channel();
                s_instance.SSP2.SSACD = new SSACD_Bitfield();
                s_instance.SSP2.SSCR0 = new SSCR0_Bitfield();
                s_instance.SSP2.SSCR1 = new SSCR1_Bitfield();
                s_instance.SSP2.SSITR = new SSITR1_Bitfield();
                s_instance.SSP2.SSPSP = new SSPSP1_Bitfield();
                s_instance.SSP2.SSSR = new SSSR_Bitfield();
                s_instance.SSP2.SSTSS = new SSTSS_Bitfield();

                s_instance.SSP3 = new SSP_Channel();
                s_instance.SSP3.SSACD = new SSACD_Bitfield();
                s_instance.SSP3.SSCR0 = new SSCR0_Bitfield();
                s_instance.SSP3.SSCR1 = new SSCR1_Bitfield();
                s_instance.SSP3.SSITR = new SSITR1_Bitfield();
                s_instance.SSP3.SSPSP = new SSPSP1_Bitfield();
                s_instance.SSP3.SSSR = new SSSR_Bitfield();
                s_instance.SSP3.SSTSS = new SSTSS_Bitfield();
                return s_instance;
            }
        }
#else
        /// <summary>
        /// The singleton for the SSP subsystem.
        /// </summary>
        public static extern SSP Instance
        {
            [SingletonFactory()]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
#endif
        #endregion

    }

}
