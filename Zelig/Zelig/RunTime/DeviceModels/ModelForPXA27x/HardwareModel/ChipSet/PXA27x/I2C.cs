//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System.Runtime.CompilerServices;

    using Microsoft.Zelig.Runtime;


    /// <summary>
    /// The I2C subsystem of the XScale271
    /// </summary>
    /// <remarks>
    /// The serial I2C bus has a two-pin interface. The serial data and address (SDA) data pin serves I/O
    /// functions, and the serial clock line (SCL) clock pin controls and references the I2C bus. The I2C
    /// interface allows the PXA27x processor to serve as a master and slave device on the I2C bus.
    /// The I2C interface enables the PXA27x processor to communicate with I2C peripherals and
    /// microcontrollers for system management functions. The I2C bus requires a minimum of hardware
    /// to relay status, reliability, and control information between devices.
    /// <p/>
    /// The I2C unit includes the following features:
    /// <list type="bullet">
    /// <item><description>I2C compliant (see the I2C-Bus Specification, Version 2.0)</description></item>
    /// <item><description>Multi-master and arbitration support</description></item>
    /// <item><description>Standard-speed operation at 100 kbps.</description></item>
    /// <item><description>Fast-mode operation at 400 kbps.</description></item>
    /// </list>
    /// <p/>
    /// I2C Modes of Operation:
    /// While the I2C interface is idle, it defaults to slave-receive mode. This allows the interface to
    /// monitor the bus and receive any slave addresses intended for the processor I2C interface.
    /// When the I2C interface receives an address that matches the seven-bit address in the I2C Slave
    /// Address register (ISAR) or the general call address (127), the interface either
    /// remains in slave-receive mode or switches to slave-transmit mode. The Read/Write bit (R/nW)
    /// determines which mode the interface enters. The R/nW bit is the least significant bit of the byte
    /// containing the slave address. If R/nW is clear, the master that initiated the transaction intends to
    /// write data, and the I2C interface remains in slave-receive mode. If the R/nW bit is set, the master
    /// that initiated the transaction intends to read data, and the I2C interface switches to slave-transmit
    /// mode.
    /// <p/>
    /// When the PXA27x processor initiates a read or write on the I2C bus, it switches the interface from
    /// the default slave-receive mode to the master-transmit mode. If the transaction is a write, the I2C
    /// interface remains in master-transmit mode after the address transfer is completed. If the transaction
    /// is a read, the I2C interface transmits the slave address, then switches to master-receive mode.
    /// </remarks>
    [MemoryMappedPeripheral(Base = 0x40301680U, Length = 0x00002000U)]
    public class I2C
    {

        #region IBMR Bitfield
        /// <summary>
        /// This register contains the current (physical) levels
        /// of the SDA and SCL pins.
        /// </summary>
        /// <remarks>
        /// The I2C Bus Monitor register (IBMR) tracks the status of the SCL and SDA pins. The values of
        /// these pins are recorded in this read-only IBMR so software can determine when the I2C bus is hung
        /// and the I2C unit must be reset.
        /// </remarks>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct IBMR_Bitfield
        {
            /// <summary>
            /// SCL pin value
            /// </summary>
            [BitFieldRegister(Position = 1, Modifiers = BitFieldModifier.ReadOnly)]
            public bool SCL;

            /// <summary>
            /// SDA pin value
            /// </summary>
            [BitFieldRegister(Position = 0, Modifiers = BitFieldModifier.ReadOnly)]
            public bool SDA;
        }
        #endregion

        #region ICR Bitfield
        /// <summary>
        /// This is the I2C configuration register.
        /// use it to configure, enable/disable communication speed,
        /// interrupts, and device state.
        /// </summary>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct ICR_Bitfield
        {
            /// <summary>
            /// Fast Mode.
            /// </summary>
            /// <remarks>
            /// ICR (Standard I2C):
            /// 0 = 100 kbps operation
            /// 1 = 400 kbps operation
            /// </remarks>
            [BitFieldRegister(Position = 15)]
            public bool FM;

            /// <summary>
            /// unit reset.
            /// </summary>
            /// <remarks>
            /// 0 = No reset.
            /// 1 = Reset the I2C interface only.
            /// </remarks>
            [BitFieldRegister(Position = 14)]
            public bool UR;

            /// <summary>
            /// Slave address detected interupt enable
            /// </summary>
            /// <remarks>
            /// 0 = Disable interrupt.
            /// 1 = Enables the I2C interface to interrupt the processor upon detecting a
            /// slave address match or a general call address.
            /// </remarks>
            /// <seealso cref="ISAR"/>
            [BitFieldRegister(Position = 13)]
            public bool SADIE;

            /// <summary>
            /// Arbitration loss detected interrupt enable
            /// </summary>
            /// <remarks>
            /// 0 = Disable interrupt.
            /// 1 = Enables the I2C interface to interrupt the processor upon losing
            /// arbitration while in master mode.
            /// note: setting this to false will enable the automatic
            ///       arbitration logic in the I2C controller. In case
            ///       of a lost arbitration, it will re-try the operation
            ///       again after the bus has been given up by the winning
            ///       I2C master.
            /// </remarks>
            [BitFieldRegister(Position = 12)]
            public bool ALDIE;

            /// <summary>
            /// Slave stop detected interrupt enable
            /// </summary>
            /// <remarks>
            /// 0 = Disable interrupt.
            /// 1 = Enables the I2C interface to interrupt the processor when it detects a
            /// STOP condition while in slave mode.
            /// </remarks>
            [BitFieldRegister(Position = 11)]
            public bool SSDIE;

            /// <summary>
            /// Bus error interrupt enable
            /// </summary>
            /// <remarks>
            /// 0 = Disable interrupt.
            /// 1 = Enables the I2C interface to interrupt the processor for the following
            /// I2C bus errors:
            /// <list type="bullet">
            /// <item><description>As a master transmitter, no Ack was detected after a byte was sent.</description></item>
            /// <item><description>As a slave receiver, the I2C interface generated a NAK pulse.</description></item>
            /// </list>
            /// NOTE: Software is responsible for guaranteeing that misplaced START
            /// and STOP conditions do not occur.
            /// </remarks>
            [BitFieldRegister(Position = 10)]
            public bool BEIE;

            /// <summary>
            /// Data Buffer Register (IDBR) receive interrupt enable
            /// </summary>
            /// <remarks>
            /// 0 = Disable interrupt.
            /// 1 = Enables the I2C interface to interrupt the processor when the IDBR
            /// has received a data byte from the I2C bus.
            /// </remarks>
            [BitFieldRegister(Position = 9)]
            public bool DRFIE;

            /// <summary>
            /// IDBR transit empty interrupt enable
            /// </summary>
            /// <remarks>
            /// 0 = Disable interrupt.
            /// 1 = Enables the I2C interface to interrupt the processor after transmitting
            /// a byte onto the I2C bus. 
            /// </remarks>
            [BitFieldRegister(Position = 8)]
            public bool ITEIE;

            /// <summary>
            /// General call disable.
            /// </summary>
            /// <remarks>
            /// 0 = Enable the I2C interface to respond to general call messages (127).
            /// 1 = Disable I2C interface response to general call messages as a slave.
            /// This bit must be set when sending a master mode general call message
            /// from the I2C interface.
            /// note: appliations should usually set this bit.
            /// </remarks>
            [BitFieldRegister(Position = 7)]
            public bool GCD;

            /// <summary>
            /// I2C unit enable
            /// </summary>
            /// <remarks>
            /// 0 = Disables the unit and does not master any transactions or respond to
            /// any slave transactions.
            /// 1 = Enables the I2C interface (defaults to slave-receive mode).
            /// Software must guarantee the I2C bus is idle before setting this bit.
            /// Software must guarantee that the internal clock to the Power I2C unit is
            /// enabled (CKEN[15] must be set) before setting or clearing this bit.
            /// </remarks>
            [BitFieldRegister(Position = 6)]
            public bool IUE;

            /// <summary>
            /// SCL enable
            /// </summary>
            /// <remarks>
            /// 0 = Disables the I2C interface from driving the SCL line.
            /// 1 = Enables the I2C clock output for master-mode operation.
            /// </remarks>
            [BitFieldRegister(Position = 5)]
            public bool SCLE;

            /// <summary>
            /// MASTER ABORT
            /// </summary>
            /// <remarks>
            /// Used by the I2C interface in master mode to generate a STOP without
            /// transmitting another data byte:
            /// 0 = The I2C interface transmits STOP using the ICR[STOP] bit only.
            /// 1 = The I2C interface sends STOP without data transmission.
            /// When in master-transmit mode, after transmitting a data byte, the ICR
            /// transfer byte bit is cleared and IDBR transmit empty bit is set. When no
            /// more data bytes need to be sent, setting master abort bit sends the STOP.
            /// The transfer byte bit (03) must remain clear.
            /// In master-receive mode, when a Nack is sent without a STOP (stopICR bit
            /// was not set) and the processor does not send a repeated START, setting
            /// this bit sends the STOP. Once again, the transfer byte bit (03) must remain
            /// clear.</remarks>
            [BitFieldRegister(Position = 4)]
            public bool MA;

            /// <summary>
            /// Transfer Byte command
            /// </summary>
            /// <remarks>
            /// Sends or receives a byte on the I2C bus:
            /// 0 = Cleared by I2C interface when the byte is sent/received.
            /// 1 = Send/receive a byte.
            /// The processor can monitor this bit to determine when the byte transfer has
            /// completed. In master or slave mode, after each byte transfer including
            /// ACKNOWLEDGE pulse, the I2C interface holds the SCL line low (inserting
            /// wait states) until TB is set.</remarks>
            [BitFieldRegister(Position = 3)]
            public bool TB;

            /// <summary>
            /// POS/NEG ACK
            /// ACK, if true
            /// NACK, if false
            /// </summary>
            /// <remarks>
            /// Defines the type of ACKNOWLEDGE pulse sent by the I2C interface when
            /// in master receive mode:
            /// 0 = Send a positive acknowledge (ACK) pulse after receiving a data byte.
            /// 1 = Send a negative acknowledge (NAK) pulse after receiving a data
            /// byte.
            /// The I2C interface automatically sends an ACK pulse when responding to its
            /// slave address or when responding in slave-receive mode, regardless of the
            /// ACKNAK control-bit setting.</remarks>
            [BitFieldRegister(Position = 2)]
            public bool ACKNAK;

            /// <summary>
            /// Sends STOP indication after next byte in master mode.
            /// </summary>
            /// <remarks>
            /// Initiates a STOP condition after transferring the next data byte on the I2C
            /// bus when in master mode. In master-receive mode, the ACKNAK control bit
            /// must be set in conjunction with the STOP bit. See Section 9.4.3.3 for details
            /// of the STOP state.
            /// 0 = Do not send a STOP.
            /// 1 = Send a STOP</remarks>
            [BitFieldRegister(Position = 1)]
            public bool STOP;

            /// <summary>
            /// Sends START indication in master mode before the next
            /// byte sent on the I2C bus.
            /// </summary>
            /// <remarks>
            /// Initiates a START condition I2C when in master mode.
            /// 0 = Do not send a START pulse.
            /// 1 = Send a START pulse.
            /// </remarks>
            [BitFieldRegister(Position = 0)]
            public bool START;
        }
        #endregion

        #region ISR Bitfield
        /// <summary>
        /// This is the I2C status register.
        /// Use it to retrieve bus error conditions,
        /// status changes, or clear interrupt conditions.
        /// </summary>
        /// <remarks>
        /// The ISR signals I2C interrupts to the PXA27x processor interrupt controller. Software can use the
        /// ISR bits to check the status of the I2C unit and bus. ISR bits [9:5] are updated after the ACK/NAK
        /// bit is completed on the I2C bus.
        /// <p/>
        /// The ISR also clears the following interrupts signaled from the I2C interface:
        /// <list type="bullet">
        /// <item><description>IDBR receive full</description></item>
        /// <item><description>IDBR transmit empty</description></item>
        /// <item><description>Slave-address detected</description></item>
        /// <item><description>Bus error detected</description></item>
        /// <item><description>STOP condition detected</description></item>
        /// <item><description>Arbitration lost</description></item>
        /// </list>
        /// </remarks>
        [BitFieldPeripheral(PhysicalType = typeof(uint))]
        public struct ISR_Bitfield
        {
            /// <summary>
            /// BUS ERROR detected
            /// </summary>
            /// <remarks>
            /// The I2C interface sets this bit when it detects one of the following error conditions:
            /// <list type="bullet">
            /// <item><description>As a master transmitter, no Ack was detected on the interface after a
            ///   byte was sent.</description></item>
            /// <item><description>As a slave receiver, the I2C interface generates a Nack pulse.</description></item>  
            /// </list>
            /// <p/>
            /// note: When an error occurs, I2C bus transactions continue. Software
            ///       must guarantee that misplaced START and STOP conditions do
            ///       not occur.
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 10, Modifiers = BitFieldModifier.ReadOnly)]
            public bool BED;

            /// <summary>
            /// Slave address detected
            /// </summary>
            /// <remarks>
            /// The I2C interface detected a seven-bit address that matches the
            /// general call address or ISAR. An interrupt is signaled when enabled in
            /// the ICR.
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 9, Modifiers = BitFieldModifier.ReadOnly)]
            public bool SAD;

            /// <summary>
            /// General call address detected
            /// </summary>
            /// <remarks>
            /// 0 = No general call address received.
            /// 1 = I2C interface received a general call address (127).
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 8, Modifiers = BitFieldModifier.ReadOnly)]
            public bool GCAD;

            /// <summary>
            /// <see cref="IDBR"/> receive full indication.
            /// </summary>
            /// <remarks>
            /// 0 = The IDBR has not received a new data byte or the I2C interface is idle.
            /// 1 = The IDBR register received a new data byte from the I2C bus. An
            ///     interrupt is signaled when enabled in the ICR.
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 7, Modifiers = BitFieldModifier.ReadOnly)]
            public bool IRF;

            /// <summary>
            /// <see cref="IDBR"/> transmit empty indication.
            /// </summary>
            /// <remarks>
            /// 0 = The data byte is still being transmitted.
            /// 1 = The I2C interface has finished transmitting a data byte on the I2C bus.
            ///     An interrupt is signaled when enabled in the ICR.
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 6, Modifiers = BitFieldModifier.ReadOnly)]
            public bool ITE;

            /// <summary>
            /// Arbitration loss detected indication.
            /// </summary>
            /// <remarks>
            /// Used during multi-master operation:
            /// 0 = Cleared when arbitration is won or never took place.
            /// 1 = Set when the I2C interface loses arbitration.
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 5, Modifiers = BitFieldModifier.ReadOnly)]
            public bool ALD;

            /// <summary>
            /// Slave stop detected indication.
            /// </summary>
            /// <remarks>
            /// 0 = No STOP detected.
            /// 1 = Set when the I2C interface detects a STOP while in slave-receive or
            ///     slave-transmit mode.
            /// <p/>
            /// note: This bit is cleared on Read!
            /// </remarks>
            [BitFieldRegister(Position = 4, Modifiers = BitFieldModifier.ReadOnly)]
            public bool SSD;

            /// <summary>
            /// I2C bus busy
            /// </summary>
            /// <remarks>
            /// 0 = I2C bus is idle or the I2C interface is using the bus (unit busy).
            /// 1 = Set when the I2C bus is busy but the processor’s I2C interface is not
            ///     involved in the transaction.
            /// </remarks>
            [BitFieldRegister(Position = 3, Modifiers = BitFieldModifier.ReadOnly)]
            public bool IBB;

            /// <summary>
            /// Unit busy
            /// </summary>
            /// <remarks>
            /// 0 = I2C interface not busy.
            /// 1 = Set when the processor’s I2C interface is busy. This is defined as the
            ///     time between the first START and STOP.
            /// </remarks>
            [BitFieldRegister(Position = 2, Modifiers = BitFieldModifier.ReadOnly)]
            public bool UB;

            /// <summary>
            /// ACK/NAK status indication.
            /// </summary>
            /// <remarks>
            /// 0 = The I2C interface received or sent an Ack on the bus.
            /// 1 = The I2C interface received or sent a Nack.
            /// <p/>
            /// Used in slave-transmit mode to determine when the byte transferred is the
            /// last one. This bit is updated after each byte and Ack/Nack information is
            /// received.
            /// </remarks>
            [BitFieldRegister(Position = 1, Modifiers = BitFieldModifier.ReadOnly)]
            public bool ACKNAK;

            /// <summary>
            /// read/write mode
            /// </summary>
            /// <remarks>
            /// 0 = The I2C interface is in master-transmit or slave-receive mode.
            /// 1 = The I2C interface is in master-receive or slave-transmit mode.
            /// <p/>
            /// This is the R/nW bit of the slave address. It is automatically cleared by
            /// hardware after a STOP state.
            /// </remarks>
            [BitFieldRegister(Position = 0, Modifiers = BitFieldModifier.ReadOnly)]
            public bool RWM;
        }
        #endregion

        #region Register Bank
        /// <summary>
        /// I2C Bus Monitor Register
        /// </summary>
        /// <seealso cref="IBMR_Bitfield"/>
        [Register(Offset = 0x00000000U, Instances = 1)]
        public IBMR_Bitfield IBMR;

        /// <summary>
        /// Data buffer register.
        /// valid data is in (IDBR and 0x000000FF)
        /// </summary>
        /// <remarks>
        /// The PXA27x processor uses the I2C Data Buffer register to transmit and receive data from the I2C
        /// bus. The IDBR is accessed by the programmed I/O on one side and by the I2C shift register on the
        /// other. The IDBR receives data coming into the I2C unit after a full byte is received and
        /// acknowledged. The processor core writes data going out of the I2C interface to the IDBR and sends
        /// it to the serial bus.
        /// When the I2C interface is in transmit mode (master or slave), the processor writes data to the IDBR
        /// over the internal bus. The processor writes data to the IDBR when a master transaction is initiated
        /// or when the IDBR transmit-empty interrupt is signaled. Data moves from the IDBR to the shift
        /// register when the Transfer Byte bit is set. The IDBR transmit-empty interrupt is signaled (if
        /// enabled) when a byte is transferred on the I2C bus and the acknowledge cycle is complete. If the
        /// IDBR is not written by the processor and a STOP condition is not in place before the I2C bus is
        /// ready to transfer the next byte packet, the I2C unit inserts wait states until the processor writes the
        /// IDBR and sets the Transfer Byte bit.
        /// When the I2C interface is in receive mode (master or slave), the processor reads IDBR data over
        /// the internal bus. The processor reads data from the IDBR when the IDBR receive-full interrupt is
        /// signaled. The data moves from the shift register to the IDBR when the ACKNOWLEDGE cycle is
        /// complete. The I2C interface inserts wait states until the IDBR is read. See SlaveMode for more
        /// information on the ACKNOWLEDGE pulse in receive mode. After the processor reads the IDBR,
        /// ICR[ACKNAK] and ICR[ACKNAK] are written, allowing the next byte transfer to proceed to the I2C bus.
        /// </remarks>
        [Register(Offset = 0x00000008U, Instances = 1)]
        public uint IDBR;

        /// <summary>
        /// The PXA27x processor uses the bits in the I2C Control register (ICR) to control the I2C unit.
        /// </summary>
        [Register(Offset = 0x00000010U, Instances = 1)]
        public ICR_Bitfield ICR;

        /// <summary>
        /// The ISR signals I2C interrupts to the PXA27x processor interrupt controller. Software can use the
        /// ISR bits to check the status of the I2C unit and bus. ISR bits [9:5] are updated after the ACK/NAK
        /// bit is completed on the I2C bus.
        /// </summary>
        [Register(Offset = 0x00000018U, Instances = 1)]
        public ISR_Bitfield ISR;

        /// <summary>
        /// Slave address  register.
        /// valid address data is in the lowest 7 bits.
        /// </summary>
        /// <remarks>
        /// The ISAR defines the I2C interface’s seven-bit slave address. In slave-receive
        /// mode, the PXA27x processor responds when the seven-bit address matches the value in this
        /// register. The processor writes this register before it enables I2C operations. The ISAR is fully
        /// programmable (no address is assigned to the I2C interface) so it can be set to a value other than
        /// those of hard-wired I2C slave peripherals in the system. If the processor is reset, the ISAR is not
        /// affected. The ISAR register default value is 00000002.
        /// </remarks>
        [Register(Offset = 0x00000020U, Instances = 1)]
        public uint ISAR;
        #endregion

        #region Singleton data
        /// <summary>
        /// The GPIO pin for the I2C SCL line.
        /// </summary>
        public const int PIN_I2C_SCL = 117;

        /// <summary>
        /// The GPIO pin for the SDA SCL line.
        /// </summary>
        public const int PIN_I2C_SDA = 118;

#if TESTMODE_DESKTOP
        private static I2C s_instance;
        public static I2C Instance
        {
            get
            {
                if (null != s_instance) return s_instance;

                s_instance = new I2C();
                s_instance.IBMR = new IBMR_Bitfield();
                s_instance.ICR = new ICR_Bitfield();
                s_instance.IDBR=0;
                s_instance.ISAR =2;
                s_instance.ISR = new ISR_Bitfield();
                return s_instance;
            }
        }
#else
        /// <summary>
        /// The singleton for the I2C subsystem.
        /// </summary>
        public static extern I2C Instance
        {
            [SingletonFactory()]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
#endif
        #endregion

    }

}
