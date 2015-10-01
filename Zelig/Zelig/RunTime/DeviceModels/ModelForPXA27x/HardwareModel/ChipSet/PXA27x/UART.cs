//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40100000U, Length=0x00700000U)]
    public class UART
    {
        public enum Id
        {
            FFUART = 0, // Full-Function UART
            BTUART = 1, // Bluetooth UART
            STUART = 6, // Standard UART
        }

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct IER_bitfield
        {
            [BitFieldRegister(Position=7)] public bool DMAE;  // DMA Requests Enable
                                                              //
                                                              // 0: DMA requests are disabled.
                                                              // 1: DMA requests are enabled.
                                                              //
            [BitFieldRegister(Position=6)] public bool UUE;   // UART Unit Enable
                                                              //
                                                              // 0: The unit is disabled.
                                                              // 1: The unit is enabled.
                                                              //
            [BitFieldRegister(Position=5)] public bool NRZE;  // NRZ coding Enable
                                                              //
                                                              // NRZ encoding/decoding is only used in UART mode, not in infrared mode.
                                                              // If the slow infrared receiver or transmitteris enabled, NRZ coding is disabled.
                                                              //
                                                              // 0: NRZ coding disabled.
                                                              // 1: NRZ coding enabled.
                                                              //
            [BitFieldRegister(Position=4)] public bool RTOIE; // Receiver Time-Out Interrupt Enable (Source IIR[TOD])
                                                              //
                                                              // 0: Receiver data time-out interrupt disabled.
                                                              // 1: Receiver data time-out interrupt enabled.
                                                              //
            [BitFieldRegister(Position=3)] public bool MIE;   // Modem Interrupt Enable (Source IIR[IID])
                                                              //
                                                              // 0: Modem status interrupt disabled.
                                                              // 1: Modem status interrupt enabled.
                                                              //
            [BitFieldRegister(Position=2)] public bool RLSE;  // Receiver Line Status Interrupt Enable (Source IIR[IID])
                                                              //
                                                              // 0: Receiver line status interrupt disabled.
                                                              // 1: Receiver line status interrupt enabled.
                                                              //
            [BitFieldRegister(Position=1)] public bool TIE;   // Transmit Data reqeust Interrupt Enable (Source IIR[IID])
                                                              //
                                                              // 0: Transmit FIFO data request interrupt disabled.
                                                              // 1: Transmit FIFO data request interrupt enabled.
                                                              //
            [BitFieldRegister(Position=0)] public bool RAVIE; // Receiver Data Available Interrupt Enable (Source IIR[IID])
                                                              //
                                                              // 0: Receiver data available (trigger threshold reached) interrupt disabled.
                                                              // 1: Receiver data available (trigger threshold reached) interrupt enabled.
                                                              //
        }

        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct IIR_bitfield
        {
            public enum FIFOEnableStatus : uint
            {
                NonFIFO = 0x0, // Non-FIFO mode is selected.
                FIFO    = 0x3, // FIFO mode is selected.
            }

            public enum InterruptSource : uint
            {
                            // Priority | Type                                | Source                                                        | RESET Control
                            //          |                                     |                                                               |
                RLS  = 0x3, // Highest  | Receiver Line Status (RLS)          | Overrun Error (OE), Parity Error (PE), Framing Error (FE),    | Reading the LSR.
                            //          |                                     | Break Interrupt (BI).                                         |
                            //          |                                     |                                                               |
                RDA  = 0x2, //          | Received Data Available (RDA)       | Non-FIFO mode: receive buffer is full.                        | Reading the Receiver Buffer register.
                            //          |                                     | FIFO mode: trigger threshold was reached.                     | Reading bytes until the receive FIFO drops below trigger threshold or setting.
                            //          |                                     |                                                               |
                CTI  = 0x6, //          | Character Time-out Indication (CTI) | FIFO mode only: At least one character is left in the receive | Reading the receive FIFO or setting FCR[RESETRF].
                            //          |                                     | buffer indicating trailing bytes.                             |
                            //          |                                     |                                                               |
                TFDR = 0x1, //          | Transmit FIFO Data Request (TFDR)   | Non-FIFO mode: Transmit Holding register empty                | Reading the IIR register (if the source of the interrupt)
                            //          |                                     |                                                               | or writing into the Transmit Holding register.
                            //          |                                     | FIFO mode: transmit FIFO has half or less than half data.     | Reading the IIR register (if the source of the interrupt)
                            //          |                                     |                                                               | or writing to the transmit FIFO.
                            //          |                                     |                                                               |
                MS   = 0x0, // Lowest   | Modem Status (MS)                   | Clear to Send, Data Set REady, Ring Indicator, Received Line  | Reading the Modem Status register.
                            //          |                                     | Signal Detect.                                                |
            }

            [BitFieldRegister(Position=6, Size=2)] public FIFOEnableStatus FIFOES; // FIFO Mode Enable Status
                                                                                   //
            [BitFieldRegister(Position=5        )] public bool             EOC;    // DMA End of Descriptor Chain
                                                                                   //
                                                                                   // 0: DMA has not signaled the end of its programmed descriptor chain.
                                                                                   // 1: Dma has signaled the end of its programmed descriptor chain.
                                                                                   //
            [BitFieldRegister(Position=4        )] public bool             ABL;    // Auto-Baud Lock
                                                                                   //
                                                                                   // 0: Auto-baud circutry has not programmed Divisor Latch registers (DLR).
                                                                                   // 1: Divisor Latch registers (DLR) programmed by auto-baud circutry.
                                                                                   //
            [BitFieldRegister(Position=3        )] public bool TOD;                // Time-Out Detected
                                                                                   //
                                                                                   // 0: No time-out interrupt is pending.
                                                                                   // 1: Time-out interrupt is pending. (FIFO mode only)
                                                                                   //
            [BitFieldRegister(Position=1, Size=3)] public InterruptSource  IID;    // Interrupt Source Encoded
                                                                                   // 
            [BitFieldRegister(Position=0        )] public bool             nIP;    // Interrupt Pending
                                                                                   // 
                                                                                   // 0: Interrupt is pending (active low).
                                                                                   // 1: No interrupt is pending.
                                                                                   // 
        }

        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct FCR_bitfield
        {
            public enum InterruptTriggerLevel : uint
            {
                TriggerAt1  = 0, // 00: trigger level = 1
                TriggerAt8  = 1, // 01: trigger level = 8
                TriggerAt16 = 2, // 10: trigger level = 16
                TriggerAt32 = 3, // 11: trigger level = 32
            }

            [BitFieldRegister(Position=6, Size=2)] public InterruptTriggerLevel ITL;     // Interrupt Trigger-Level (Threshold)
                                                                                         //
                                                                                         // When the number of bytes in the receive FIFO equals the interrupt trigger threshold
                                                                                         // programmed into this field and the received-data-available interrupt is enabled using
                                                                                         // IER, an interrupt is generated and appropriate bits are st in the IIR. The receive
                                                                                         // DMA request is also generated when the trigger threshold is reached.
                                                                                         // 
            [BitFieldRegister(Position=5        )] public bool                  BUS;     // 32-bit Peripheral Bus
                                                                                         //
                                                                                         // 0: 8-bit peripheral bus
                                                                                         // 1: 32-bit peripheral bus
                                                                                         //
            [BitFieldRegister(Position=4        )] public bool                  TRAIL;   // Trailing Bytes
                                                                                         //
                                                                                         // 0: Trailing bytes are removed by the processor.
                                                                                         // 1: Trailing bytes are removed by the DMA controller.
                                                                                         //
            [BitFieldRegister(Position=3        )] public bool                  TIL;     // Transmitter Interrupt Level
                                                                                         //
                                                                                         // 0: Interrupt/DMA request when FIFO is half empty.
                                                                                         // 1: Interrput/DMA request when FIFO is empty.
                                                                                         //
            [BitFieldRegister(Position=2        )] public bool                  RESETTF; // Reset Transmit FIFO
                                                                                         //
                                                                                         // When RESETTF is set, all the bytes in the transmit FIFO are cleared. The TDRQ
                                                                                         // bit in the LSR is set and the IIR shows a transmitter requests data interrupt,
                                                                                         // if the TIE bit in the IER register is set. The transmit shift register is not
                                                                                         // cleared, and it completes the current transmission.
                                                                                         //
                                                                                         // 0: Writing 0 has no effect.
                                                                                         // 1: The transmit FIFO is cleared.
                                                                                         //
            [BitFieldRegister(Position=1        )] public bool                  RESETRF; // Reset Receive FIFO
                                                                                         //
                                                                                         // When RESETRF is set, all the bytes in the receive FIFO are cleared. The
                                                                                         // DR bit in the LSR is reset to 0. All the error bits in the FIFO and the
                                                                                         // FIFOE bit in the LSR are cleared. Any error bits, OE, PE, FE, BI, that
                                                                                         // had been set in the LSR are still set. The receive shift register is not
                                                                                         // cleared. IF the IIR had been set to Received Data Available, it is cleared.
                                                                                         //
                                                                                         // 0: Writing 0 has no effect.
                                                                                         // 1: The receive FIFO is cleared.
                                                                                         //
            [BitFieldRegister(Position=0        )] public bool                  TRFIFOE; // Transmit and Receive FIFO Enable
                                                                                         //
                                                                                         // TRFIFOE enables/disables the transmit and receive FIFOs. When TRFIFOE is set
                                                                                         // both FIFOs are enabled (FIFO mode). When TRFIFOE is clear, the FIFOs are both
                                                                                         // disabled (non-FIFO mode). Writing 0b0 to this bit clears all bytes in both
                                                                                         // FIFOs. When changing from FIFO mode to non-FIFO mode and vice versa, data
                                                                                         // is automatically cleared from the FIFOs. This bit must be set when other bits
                                                                                         // in this register are written or other bits are not programmed.
                                                                                         //
                                                                                         // 0: FIFOs are disabled.
                                                                                         // 1: FIFOs are enabled.
        }

        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct LCR_bitfield
        {
            public enum LengthSettings : uint
            {
                Use5bits = 0x0, // *00: 5 bit character length
                Use6bits = 0x1, //  01: 6 bit character length
                Use7bits = 0x2, //  10: 7 bit character length
                Use8bits = 0x3, //  11: 8 bit character length
            }

            [BitFieldRegister(Position=7        )] public bool           DLAB;  // Divisor Latch Access
                                                                                //
                                                                                // Must be set to access the Divisor Latch registers of the baud-rate generator
                                                                                // during a read or write operation. Must be clear to access the Receive Buffer,
                                                                                // the Transmit Holding Buffer, or the IER.
                                                                                //
                                                                                // 0: Access Transmit Holding register (THR), Receive Buffer register (RBR) and IER.
                                                                                // 1: Access Divisor Latch registers (DLL and DLH).
                                                                                //
            [BitFieldRegister(Position=6        )] public bool           SB;    // Set Break
                                                                                //
                                                                                // Causes a break condition to be transmitted to the receiving UART. Acts only on
                                                                                // the TXD pin and has no effect on the transmit logic. In FIFO mode, wait until
                                                                                // the transmitter is idle (LSR[TEMT] = 1) to set and clear SB.
                                                                                //
                                                                                // 0: No effect on the TXD output.
                                                                                // 1: Forces TXD output to 0 (space).
                                                                                //
            [BitFieldRegister(Position=5        )] public bool           STKYP; // Sticky Parity
                                                                                //
                                                                                // Forces the bit value at the parity bit location to be the opposite of EPS
                                                                                // bit rather than the parity valkue. This stops parity generation. If PEN = 0,
                                                                                // STKYP is ignored.
                                                                                //
                                                                                // 0: No effect on parity bit.
                                                                                // 1: Forces parity bit to be opposite of EPS bit value.
                                                                                //
            [BitFieldRegister(Position=4        )] public bool           EPS;   // Even Parity Select
                                                                                //
                                                                                // If PEN = 0, EPS is ignored.
                                                                                //
                                                                                // 0: Sends or checks for odd parity.
                                                                                // 1: Sends or checks for even parity.
                                                                                //
            [BitFieldRegister(Position=3        )] public bool           PEN;   // Parity Enable
                                                                                //
                                                                                // Enables a parity bit to be generated on trasmission or checked on reception.
                                                                                //
                                                                                // 0: No parity
                                                                                // 1: Parity
                                                                                //
            [BitFieldRegister(Position=2        )] public bool           STB;   // Stop Bits
                                                                                //
                                                                                // Specifies the number of stop bits transmitted in each character. When 
                                                                                // receiving, the receiver checks only the first stop bit.
                                                                                //
                                                                                // 0: 1 stop bit
                                                                                // 1: 2 stop bits, except for 5-bit character then 1-1/2 bits
                                                                                //
            [BitFieldRegister(Position=0, Size=2)] public LengthSettings WLS;   // Word Length Select
                                                                                // 
                                                                                // Specifies the number of data bits in each transmitted or received character.
                                                                                // 
        }

        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct MCR_bitfield
        {
            [BitFieldRegister(Position=5)] public bool AFE;   // Auto-Flow Control Enable
                                                              // 
                                                              //  0 = Auto-RTS and auto-CTS are disabled.
                                                              //  1 = Auto-CTS is enabled. If MCR[RTS] is also set, both auto-CTS and auto-RTS are enabled.
                                                              // 
            [BitFieldRegister(Position=4)] public bool LOOP;  // Loopback Mode
                                                              // 
                                                              // Provides a local loopback feature for diagnostic testing of the UART. When LOOP is set, the following occurs:
                                                              // 
                                                              //   The transmitter serial output is set to a logic 1 state.
                                                              //   The receiver serial input is disconnected from the pin.
                                                              //   The output of the transmit shift register is “looped back” into the receive shift register input.
                                                              //   The four modem control inputs (nCTS, nDSR, nDCD, and nRI) are disconnected from the pins
                                                              //   and the modem control output pins (nRTS and nDTR) are forced to their inactive state.
                                                              // 
                                                              // Coming out of the loopback mode may result in unpredictable activation of the delta bits (bits 3:0) in the Modem Status register.
                                                              // Read MSR once to clear the MSR delta bits.
                                                              // 
                                                              // Loopback mode must be configured before the UART is enabled.
                                                              // 
                                                              // The lower four bits of the MCR are connected to the upper four Modem Status register bits:
                                                              // 
                                                              //   DTR = 1 forces DSR to a 1
                                                              //   RTS = 1 forces CTS to a 1
                                                              //   OUT1 = 1 forces RI to a 1
                                                              //   OUT2 = 1 forces DCD to a 1
                                                              // 
                                                              // In loopback mode, data that is transmitted is immediately received.
                                                              // This feature allows the processor to verify the transmit and receive data paths of the UART.
                                                              // The transmit, receive, and modem-control interrupts are operational, except that the modem control interrupts are activated by MCR
                                                              // bits, not by the modem-control pins. A break signal can also be transferred from the transmitter section to the receiver section in loopback mode.
                                                              // 
                                                              //  0 = Normal UART operation
                                                              //  1 = Loopback-mode UART operation
                                                              // 
                                                              // 
            [BitFieldRegister(Position=3)] public bool OUT2;  // OUT2 Signal Control
                                                              // 
                                                              // OUT2 connects the UART’s interrupt output to the interrupt controller unit.
                                                              // 
                                                              // When LOOP is clear:
                                                              // 
                                                              //   0 = UART interrupt is disabled.
                                                              //   1 = UART interrupt is enabled.
                                                              // 
                                                              // When LOOP is set, interrupts always go to the processor:
                                                              // 
                                                              //   0 = MSR[DCD] forced to 0b0.
                                                              //   1 = MSR[DCD] forced to 0b1.
                                                              // 
            [BitFieldRegister(Position=2)] public bool OUT1;  // Test Bit
                                                              // 
                                                              // Used only in loopback mode. It is ignored otherwise.
                                                              //  0 = Force MSR[RI] to 0b0.
                                                              //  1 = Force MSR[RI] to 0b1.
                                                              // 
            [BitFieldRegister(Position=1)] public bool RTS;   // Request to Send
                                                              // 
                                                              //  0 = Non-auto-flow mode. nRTS pin is 1. Auto-RTS is disabled. Auto-flow works only with auto-CTS.
                                                              //  1 = Auto-flow mode. nRTS pin is 0. Auto-RTS is enabled. Auto-flow works with both auto-CTS and auto-RTS.
                                                              // 
            [BitFieldRegister(Position=0)] public bool DTR;   // Data Terminal Ready
                                                              // 
                                                              //  0 = nDTR pin is 1.
                                                              //  1 = nDTR pin is 0.
                                                              // 
        }

        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct LSR_bitfield
        {
            [BitFieldRegister(Position=7)] public bool FIFOE; // FIFO Error Status
                                                              // 
                                                              // In non-FIFO mode, this bit is clear. In FIFO mode, FIFOE is set when there is
                                                              // at least one parity error, framing error, or break indication for any of the 
                                                              // characters in the FIFO. A processor read of the LSR does not reset this bit.
                                                              // FIFOE is reset when all erroneous characters have been read from the FIFO.
                                                              // If DMA requests are enables (IER bit 7 set) and FIFOE is set, the error
                                                              // interrupt is generated, and no receive DMA request is generated even when the
                                                              // receive FIFO reaches the trigger threshold. Once the errors have been cleared
                                                              // by reading the FIFO, DMA request are re-enabled automatically. If DMA requests
                                                              // are not enabled (IER bit 7 clear), FIFOE set does not generate an error interrupt.
                                                              // 
                                                              // 0: No FIFO or no error in receive FIFO.
                                                              // 1: At least one character in receive FIFO has errors.
                                                              // 
            [BitFieldRegister(Position=6)] public bool TEMT;  // Transmitter Empty
                                                              // 
                                                              // Set when the Transmit Holding register and the transmit shift register are both
                                                              // empty. It is cleared when either the Transmit Holding register or the transmit
                                                              // shift register contains a data character. In FIFO mode, TEMT is set when the
                                                              // transmit FIFO and the transmit shift register are both empty.
                                                              // 
                                                              // 0: There is data in the transmit shift register, the Transmit Holding register or the FIFO.
                                                              // 1: All the data in the transmitter has been shifted out.
                                                              // 
            [BitFieldRegister(Position=5)] public bool TDRQ;  // Transmit Data Request
                                                              // 
                                                              // Indicates that the UART is ready to accept a new character for transmission.
                                                              // In addition, this bit causes the UART to issue an interrupt to the processor
                                                              // when the transmit data request interrupt enable is set and generates the DMA
                                                              // request to the DMA controller if DMA requests and FIFO mode are enabled.
                                                              // the TDRQ bit is set when a character is transferred from the Transmit Holding
                                                              // register into the transmit shift register. The bit is cleared with the loading
                                                              // of the Transmit Holding register. In FIFO mode, TDRQ is set when half of the
                                                              // characters in the FIFO have been loaded into the shift register or the 
                                                              // RESETTF bit in FCR has been set. It is cleared when the FIFO has more than
                                                              // half data. If more than 64 characters are loaded into the FIFO, the excess
                                                              // characters are lost.
                                                              // 
                                                              // 0: There is data in the holding register or FIFO waiting to be shifter out.
                                                              // 1: The transmit FIFO has half or less than half data.
                                                              // 
            [BitFieldRegister(Position=4)] public bool BI;    // Break Interrupt
                                                              // 
                                                              // BI is set when the received data input is held low for longer than a full-word
                                                              // transmission time (the total time of start bit + data bits + parity bit + stop
                                                              // bits). BI is cleared when the processor reads the LSR. In FIFO mode, only one
                                                              // character equal to 0x00, is loaded into the FIFO regardless of the length of
                                                              // the break condition. BI shows the break condition for the character at the 
                                                              // front of the FIFO, not the most recently received character.
                                                              //
                                                              // 0: No break signal has been received.
                                                              // 1: Break signal received.
                                                              // 
            [BitFieldRegister(Position=3)] public bool FE;    // Framing Error
                                                              // 
                                                              // Indicates that the received character did not have a valid stop bit. FE is set
                                                              // when the bit following the last data bit or parity bit is detected to be 0.
                                                              // If the LCR had been set for two stop bits, the receiver does not check for a
                                                              // valid second stop bit. FE is clared when the processor reads the LSR. The
                                                              // UART re-synchronizes after a framing error. To do this, it assumes that the
                                                              // framing error was due to the next start bit, so it samples this start bit
                                                              // twice and then reads in the data. In FIFO mode, FE shows a framing error for
                                                              // the character at the front of the FIFO, not for the most recently received
                                                              // character.
                                                              // 
                                                              // 0: No framing error.
                                                              // 1: Invalid stop bit has been detected.
                                                              // 
            [BitFieldRegister(Position=2)] public bool PE;    // Parity Error
                                                              // 
                                                              // Indicates that the received data character does not have the correct even or
                                                              // odd parity, as slected by the even parity select bit. PE is set upon detection
                                                              // of a parity error and is cleared when the processor reads the LSR. In FIFO
                                                              // mode, PE shows a parity error for the character at the front of the FIFO,
                                                              // not the most recently received character.
                                                              // 
                                                              // 0: No parity error.
                                                              // 1: Parity error has occured.
                                                              // 
            [BitFieldRegister(Position=1)] public bool OE;    // Overrun Error
                                                              // 
                                                              // In non-FIFO mode, indicates that data in the REceive Buffer register was not
                                                              // read by the processor before the next character was received. The new character
                                                              // is lost. In FIFO mode, OE indicates that all 64 bytes of the FIFO are full and
                                                              // the most recently received byte has been discarded. OE is set upon detection
                                                              // of an overrun condition and clared when the processor reads the LSR.
                                                              // 
                                                              // 0: No data has been lost.
                                                              // 1: Receive data has been lost.
                                                              // 
            [BitFieldRegister(Position=0)] public bool DR;    // Receiver Data Ready
                                                              // 
                                                              // Set when a complete incoming character has been received and transferred into
                                                              // the Receive Buffer register or the FIFO. In non-FIFO mode, DR is cleared when
                                                              // the receive buffer is read. In FIFO mode, DR is cleared if the FIFO is empty
                                                              // (last character has been read from RBR) or the FIFO is reset with FCR[RESETRF].
                                                              //
                                                              // 0: No data has been received.
                                                              // 1: Data is available in RBR or the FIFO.
                                                              // 
        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct ABR_bitfield
        {
            [BitFieldRegister(Position=3)] public bool ABT;   // Auto-Baud Calculation
                                                              //
                                                              // 0: Use a formula to calculate baud rates, allowing all possible baud rates to be chosen by UART.
                                                              // 1: Use a table to calculate baud rates, which limits UART to choosing commong baud rates.
                                                              //
            [BitFieldRegister(Position=2)] public bool ABUP;  // Auto-Baud Programmer
                                                              //
                                                              // 0: Process programs divisor latch registers.
                                                              // 1: UART programs divisor latch registers.
                                                              //
            [BitFieldRegister(Position=1)] public bool ABLIE; // Auto-Baud Lock Interrupt
                                                              //
                                                              // 0: Audo-baud-lock interrupt disabled (Source IIR[ABL]).
                                                              // 1: Auto-baud-lock interrupt enabled (Source IIR[ABL]).
                                                              //
            [BitFieldRegister(Position=0)] public bool ABE;   // Auto-Baud Enable
                                                              //
                                                              // 0: Auto-baud disabled.
                                                              // 1: Auto-baud enabled.
                                                              //
        }

        //--//

        [MemoryMappedPeripheral(Base=0x0000U, Length=0x100000U)]
        public class Port
        {
            [Register(Offset = 0x00U)] public byte          RBR;        // Receive Buffer Register - R
            [Register(Offset = 0x00U)] public byte          THR;        // Transmit Holding Register - W
            [Register(Offset = 0x04U)] public IER_bitfield  IER;        // Interrupt Enable Register
            [Register(Offset = 0x00U)] public byte          DLL;        // Divisor Latch Register - Low Byte
            [Register(Offset = 0x04U)] public byte          DLH;        // Divisor Latch Register - High Byte
            [Register(Offset = 0x08U)] public IIR_bitfield  IIR;        // Interrupt ID Register - R
            [Register(Offset = 0x08U)] public FCR_bitfield  FCR;        // FIFO Control Register - W
            [Register(Offset = 0x0CU)] public LCR_bitfield  LCR;        // Line Control Register
            [Register(Offset = 0x10U)] public MCR_bitfield  MCR;        // Modem Control Register
            [Register(Offset = 0x14U)] public LSR_bitfield  LSR;        // Line Status Register - R
            [Register(Offset = 0x18U)] public byte          MSR;        // Modem Status Register - R
            [Register(Offset = 0x1CU)] public byte          SPR;        // Scrath Pad Register
            [Register(Offset = 0x20U)] public byte          ISR;        // Infared Select Register
            [Register(Offset = 0x24U)] public byte          FOR;        // Receive FIFO Occupancy Register
            [Register(Offset = 0x28U)] public ABR_bitfield  ABR;        // Auto-baud Control Register
            [Register(Offset = 0x2CU)] public byte          ACR;        // Auto-baud Count Register

            //
            // Helper Methods
            //

            [Inline]
            public void EnableReceiveInterrupt()
            {
                // DMAE must not be set while RAVIE bit is set.
                this.IER.DMAE = false;

                this.IER.RAVIE = true;
                this.IER.RTOIE = true;
            }

            [Inline]
            public void DisableReceiveInterrupt()
            {
                this.IER.RAVIE = false;
                this.IER.RTOIE = false;
            }

            [Inline]
            public void EnableTransmitInterrupt()
            {
                // DMAE must not be set while RAVIE bit is set.
                this.IER.DMAE = false;

                this.IER.TIE = true;
            }

            [Inline]
            public void DisableTransmitInterrupt()
            {
                this.IER.TIE = false;
            }

            [Inline]
            public bool ReadByte(out byte rx)
            {
                if (this.CanReceive)
                {
                    rx = this.RBR;

                    return true;
                }
                else
                {
                    rx = 0;

                    return false;
                }
            }

            [Inline]
            public bool WriteByte(byte tx)
            {
                if (this.CanSend)
                {
                    this.THR = tx;

                    return true;
                }

                return false;
            }

            //
            // Access Methods
            //

            public bool CanSend
            {
                [Inline]
                get
                {
                    return this.LSR.TDRQ;
                }
            }

            public bool CanReceive
            {
                [Inline]
                get
                {
                    return this.LSR.DR;
                }
            }

            public bool IsTransmitInterruptEnabled
            {
                [Inline]
                get
                {
                    return this.IER.TIE;
                }
            }

            public bool IsReceiveInterruptEnabled
            {
                [Inline]
                get
                {
                    return this.IER.RAVIE;
                }
            }

            //
            // Debug Methods
            //

            public void DEBUG_WriteLine( string text  ,
                                         uint   value )
            {
                DEBUG_Write( text, value         );
                DEBUG_Write( Environment.NewLine );
            }

            public void DEBUG_WriteLine( string text )
            {
                DEBUG_Write( text                );
                DEBUG_Write( Environment.NewLine );
            }

            public void DEBUG_Write( string text  ,
                                     uint   value )
            {
                DEBUG_Write   ( text  );
                DEBUG_WriteHex( value );
            }

            [DisableBoundsChecks()]
            [DisableNullChecks]
            public void DEBUG_Write( string s )
            {
                if(s != null)
                {
                    for(int i = 0; i < 0; ++i )
                    {
                        DEBUG_Write( s[ i ] );
                    }
                }
            }

            public void DEBUG_WriteHex( uint value )
            {
                DEBUG_Write("0x");

                for(int pos = 32 - 4; pos >= 0; pos -= 4)
                {
                    uint digit = (value >> pos) & 0xF;

                    DEBUG_Write( digit >= 10 ? (char)('A' + (digit - 10)) : (char)('0' + digit) );
                }
            }

            public void DEBUG_Write( char c )
            {
                while(this.CanSend == false)
                {
                }

                this.THR = (byte)c;
            }
        }

        //--//

        [Register(Offset=0x00000000U, Instances=7)] public Port[] Ports;

        //--//

        //
        // Helper Methods
        //

        public Port Configure( Id  portNo   ,
                               int baudrate )
        {
            var cfg = new BaseSerialStream.Configuration(null)
            {
                BaudRate = baudrate,
                DataBits = 8,
                Parity = System.IO.Ports.Parity.None,
                StopBits = System.IO.Ports.StopBits.One,
            };

            return Configure( portNo, ref cfg );
        }

        public Port Configure(     Id                             portNo ,
                               ref BaseSerialStream.Configuration cfg    )
        {
            // Enable GPIO reads by clearing PSSR[RDH]
            PowerManager.Instance.PSSR.RDH = true;

            // Get a reference to the port
            var port = this.Ports[(int)portNo];

            //--//

            var gpio   = GPIO.Instance;
            var clkMgr = ClockManager.Instance;

            switch(portNo)
            {
                case Id.FFUART:
                    // FFRXD Pin 96 Alternate Function 3 (in)
                    gpio.EnableAsInputAlternateFunction( 96, 3 );

                    // FFTXD Pin 99 Alternate Function 3 (out)
                    gpio.EnableAsOutputAlternateFunction( 99, 3, true );
                
                    // enable clock to peripheral
                    clkMgr.CKEN.EnFFUART = true;
                    break;

                case Id.BTUART:
                    // BTRXD Pin 42 Alternate Function 1 (in)
                    gpio.EnableAsInputAlternateFunction( 42, 1 );

                    // BTTXD Pin 43 Alternate Fucntion 2 (out)
                    gpio.EnableAsOutputAlternateFunction( 43, 2, true );

                    // enable clock to peripheral
                    clkMgr.CKEN.EnBTUART = true;
                    break;

                case Id.STUART:
                    // STD_RXD Pin 46 Alternate Function 2 (in)
                    gpio.EnableAsInputAlternateFunction( 46, 2 );

                    // STD_TXD Pin 47 Alternate Function 1 (out)
                    gpio.EnableAsOutputAlternateFunction( 47, 1, true );

                    // enable clock to peripheral
                    clkMgr.CKEN.EnSTUART = true;
                    break;
            }

            //--//

            // turn on divisor latch access
            port.LCR.DLAB = true;

            switch(cfg.BaudRate)
            {
                case 300   : port.DLL =   0; port.DLH = 12; break; // 3072 Divisor
                case 1200  : port.DLL =   0; port.DLH =  3; break; // 768 Divisor
                case 2400  : port.DLL = 128; port.DLH =  1; break; // 384 Divisor
                case 4800  : port.DLL = 192; port.DLH =  0; break; // 192 Divisor
                case 9600  : port.DLL =  96; port.DLH =  0; break; // 96 Divisor
                case 19200 : port.DLL =  48; port.DLH =  0; break; // 48 Divisor
                case 38400 : port.DLL =  24; port.DLH =  0; break; // 24 Divisor
                case 57600 : port.DLL =  16; port.DLH =  0; break; // 16 Divisor
                case 115200: port.DLL =   8; port.DLH =  0; break; // 8 Divisor

                default:
                    return null;
            }

            // turn off divisor latch access
            port.LCR.DLAB = false;

            //--//

            var lcr = new LCR_bitfield();

            if(cfg.Parity !=  System.IO.Ports.Parity.None)
            {
                lcr.PEN = true;

                switch(cfg.Parity)
                {
                    case System.IO.Ports.Parity.Even:
                        lcr.EPS = true;
                        break;

                    case System.IO.Ports.Parity.Odd:
                        break;

                    default:
                        return null;
                }
            }

            switch(cfg.StopBits)
            {
                case System.IO.Ports.StopBits.One:
                    break;

                case System.IO.Ports.StopBits.Two:
                    lcr.STB = true;
                    break;

                default:
                    return null;
            }

            switch(cfg.DataBits)
            {
                case 5: lcr.WLS = LCR_bitfield.LengthSettings.Use5bits; break;
                case 6: lcr.WLS = LCR_bitfield.LengthSettings.Use6bits; break;
                case 7: lcr.WLS = LCR_bitfield.LengthSettings.Use7bits; break;
                case 8: lcr.WLS = LCR_bitfield.LengthSettings.Use8bits; break;

                default:
                    return null;
            }

            port.LCR = lcr;

            //--//

            var fcr = new FCR_bitfield()
                          {
                              ITL     = FCR_bitfield.InterruptTriggerLevel.TriggerAt16,
                              TIL     = true,
                              RESETTF = true,
                              RESETRF = true,
                              TRFIFOE = true,
                          };
            
            port.FCR = fcr;

            //--//

            // Sent interrupts to AITC
            port.MCR.OUT2 = true;

            // Enable uart
            port.IER.UUE = true;

            return port;
        }

        //
        // Access Methods
        //

        public static extern UART Instance
        {
            [SingletonFactory()]
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }
    }
}