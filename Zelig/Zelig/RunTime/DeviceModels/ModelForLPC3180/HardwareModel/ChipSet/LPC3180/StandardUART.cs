//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40080000U,Length=0x00018020U)]
    public class StandardUART
    {
        public enum Id
        {
            UART3 = 0,
            UART4 = 1,
            UART5 = 2,
            UART6 = 3,
        }

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct UnIER_bitfield
        {
            [BitFieldRegister(Position=2)] public bool Rx;   // Rx Line Status Interrupt Enable
                                                             // 
                                                             // *0: Disable the Rx line status interrupts.
                                                             //  1: Enable the Rx line status interrupts.
                                                             // 
                                                             // This bit enables the UARTn Receiver Line Status interrupt.
                                                             // This interrupt reflects Overrun Error, Parity Error, Framing Error, and Break conditions.
                                                             // The status of this interrupt can be read from UnLSR[4:1].
                                                             // 
            [BitFieldRegister(Position=1)] public bool THRE; // THRE Interrupt Enable
                                                             // 
                                                             // *0: Disable the THRE interrupt.
                                                             //  1: Enable the THRE interrupt.
                                                             // 
                                                             // This bit enables the Transmit Holding Register Empty (THRE) interrupt for UARTn.
                                                             // The status of this interrupt can be read from UnLSR[5].
                                                             // 
                                                             // 
            [BitFieldRegister(Position=0)] public bool RDAE; // RDA Interrupt Enable
                                                             // 
                                                             // *0: Disable the RDA interrupt.
                                                             //  1: Enable the RDA interrupt.
                                                             // 
                                                             // This bit enables the Receive Data Available (RDA) interrupt for UARTn.
                                                             // 
        }
                                                      
        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct UnIIR_bitfield
        {
            public enum InterruptType : uint
            {
                            // 
                            // 
                            // Priority | Interrupt type                         | Interrupt source                                                             | Method of clearing interrupt
                            // ---------+----------------------------------------+------------------------------------------------------------------------------+-------------------------------------------
                RLS  = 0x3, // 1 (High) | Receiver Line Status (RLS)             | OE (Overrun Error), PE (Parity Error), FE (Framing Error), or                | Read of UnLSR.
                            //          |                                        | BI (Break Indication).                                                       |
                            //          |                                        | Note that an RLS interrupt is asserted immediately rather                    |
                            //          |                                        | than waiting for the corresponding character to reach the top of the FIFO.   |
                            //          |                                        |                                                                              |
                RDA  = 0x2, // 2        | Receiver Data Available (RDA)          | When the FIFO is turned off (UnFCR[0] = 0),                                  | Read of UnRBR when UnFCR[0] = 0,
                            //          |                                        | this interrupt is asserted when receive data is available.                   | or UARTn FIFO contents go below the trigger level when UnFCR[0] = 1.
                            //          |                                        |                                                                              |
                            //          |                                        | When the FIFO is turned on (UnFCR[0] = 1),                                   |
                            //          |                                        | this interrupt is asserted when the receive trigger level (as specified by   |
                            //          |                                        | UnFCR[7:6]) has been reached in the FIFO.                                    |
                            //          |                                        |                                                                              |
                CTI  = 0x6, // 2        | Character Time-out Indication (CTI)    | This case occurs when there is at least one character in the Rx FIFO and     | Read of UnRBR, or a Stop bit is received.
                            //          |                                        | no character has been received or removed from the FIFO                      |
                            //          |                                        | during the last 4 character times.                                           |
                            //          |                                        |                                                                              |
                THRE = 0x1, // 3        | Transmit Holding Register Empty (THRE) | When the FIFO is turned off (UnFCR[0] = 0),                                  | Read of UnIIR or write to THR.
                            //          |                                        | this interrupt is asserted when the transmit holding register is empty.      |
                            //          |                                        | When the FIFO is turned on (UnFCR[0] = 1),                                   |
                            //          |                                        | this interrupt is asserted when the transmit trigger level (as specified by  |
                            //          |                                        | UnFCR[5:4]) has been reached in the FIFO.                                    |
            }

            [BitFieldRegister(Position=1,Size=3)] public InterruptType IntId;        // Interrupt Identification
                                                                                     // 
            [BitFieldRegister(Position=0       )] public bool          NoIntPending; // Interrupt Pending
                                                                                     // 
                                                                                     // This flag indicates when there are no UARTn related interrupts pending.
                                                                                     // Note that this bit is active LOW. The pending interrupt can be determined by evaluating UnIIR[3:0].
                                                                                     // 
                                                                                     //  0: At least one interrupt is pending.
                                                                                     // *1: No pending interrupts.
                                                                                     // 
        }

        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct UnFCR_bitfield
        {
            public enum RxTriggerLevel : uint
            {
                TriggerAt16 = 0, // *00: trigger level = 16
                TriggerAt32 = 1, //  01: trigger level = 32
                TriggerAt48 = 2, //  10: trigger level = 48
                TriggerAt60 = 3, //  11: trigger level = 60
            }

            public enum TxTriggerLevel : uint
            {
                TriggerAt0  = 0, // *00: trigger level = 0
                TriggerAt4  = 1, //  01: trigger level = 4
                TriggerAt8  = 2, //  10: trigger level = 8
                TriggerAt16 = 3, //  11: trigger level = 16
            }

            [BitFieldRegister(Position=6,Size=2)] public RxTriggerLevel RxLvl;       // Receiver Trigger Level Select
                                                                                     // These two bits determine how many receiver UARTn FIFO characters must be present before an interrupt is activated.
                                                                                     // 
            [BitFieldRegister(Position=4,Size=2)] public TxTriggerLevel TxLvl;       // Transmitter Trigger Level Select
                                                                                     // These two bits determine the level of the UARTn transmitter FIFO causes an interrupt.
                                                                                     // 
            [BitFieldRegister(Position=3       )] public bool           FIFOControl; // FIFO Control.
                                                                                     // 
                                                                                     // Internal UARTn FIFO control. This bit must be set to 1 for proper FIFO operation (default off)
                                                                                     // 
            [BitFieldRegister(Position=2       )] public bool           ResetTxFIFO; // Transmitter FIFO Reset
                                                                                     // 
                                                                                     // Writing a logic 1 to UnFCR[2] will clear all bytes in UARTn Tx FIFO and reset the pointer logic.
                                                                                     // This bit is self-clearing.
                                                                                     // 
            [BitFieldRegister(Position=1       )] public bool           ResetRxFIFO; // Receiver FIFO Reset
                                                                                     // 
                                                                                     // Writing a logic 1 to UnFCR[1] will clear all bytes in UARTn Rx FIFO and reset the pointer logic.
                                                                                     // This bit is self-clearing.
                                                                                     // 
            [BitFieldRegister(Position=0       )] public bool           FIFOEnable;  // FIFO Enable
                                                                                     // 
                                                                                     // UARTn transmit and receive FIFO enable.
                                                                                     // Any transition on this bit will automatically clear the UARTn FIFOs.
                                                                                     // 
                                                                                     // *0: UARTn Rx and Tx FIFOs disabled.
                                                                                     //  1: UARTn Rx and Tx FIFOs enabled and other UnFCR bits activated.
                                                                                     // 
        }
                                                      
        //--//                                        
                                                      
        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct UnLCR_bitfield
        {
            public enum ParitySettings : uint
            {
                Odd     = 0x0, // *00: Odd parity
                Even    = 0x1, //  01: Even parity
                Forced1 = 0x2, //  10: Forced "1" stick parity
                Forced0 = 0x3, //  11: Forced "0" stick parity
            }

            public enum LengthSettings : uint
            {
                Use5bits = 0x0, // *00: 5 bit character length
                Use6bits = 0x1, //  01: 6 bit character length
                Use7bits = 0x2, //  10: 7 bit character length
                Use8bits = 0x3, //  11: 8 bit character length
            }

            [BitFieldRegister(Position=7       )] public bool           DLAB;         // Divisor Latch Access Bit
                                                                                      // 
                                                                                      // Allows access to the alternate registers at address offsets 0 and 4.
                                                                                      // 
                                                                                      // *0: Disable access to the baud rate Divisor Latches, enabling access to UnRBR, UnTHR, and UnIER.
                                                                                      //  1: Enable access to the baud rate Divisor Latches, disabling access to UnRBR, UnTHR, and UnIER.
                                                                                      // 
            [BitFieldRegister(Position=6       )] public bool           Break;        // Break Control
                                                                                      // 
                                                                                      // Allows forcing the Un_TX output low in order to generate a break condition.
                                                                                      // 
                                                                                      // *0: Disable break transmission
                                                                                      //  1: Enable break transmission.
                                                                                      // 
            [BitFieldRegister(Position=4,Size=2)] public ParitySettings Parity;       // If bit UnLCR[3] = 1, selects the type of parity used by the UART.
                                                                                      // 
                                                                                      // 
            [BitFieldRegister(Position=3       )] public bool           ParityEnable; // Parity Enable
                                                                                      // 
                                                                                      // Selects the whether or not the UART uses parity.
                                                                                      // 
                                                                                      // *0: Disable parity generation and checking
                                                                                      //  1: Enable parity generation and checking
                                                                                      // 
            [BitFieldRegister(Position=2       )] public bool           TwoStopBits;  // Stop Bit Select
                                                                                      // 
                                                                                      // Selects the number of stop bits used by the UART.
                                                                                      // 
                                                                                      // *0: 1 stop bit
                                                                                      //  1: 2 stop bits (1.5 if UnLCR[1:0] = 00)
                                                                                      // 
            [BitFieldRegister(Position=0,Size=2)] public LengthSettings WordLen;      // Word Length Select
                                                                                      // 
                                                                                      // Selects the character length (in bits) used by the UART
                                                                                      // 
        }
                                                      
        //--//                                        

        [BitFieldPeripheral(PhysicalType=typeof(byte))]
        public struct UnLSR_bitfield
        {
            [BitFieldRegister(Position=7)] public bool FIFO_Rx_Error; // FIFO Rx Error
                                                                      // 
                                                                      // This bit is set when a character with a receive error such as framing error, parity
                                                                      // error or break interrupt, is loaded into the UnRBR. This bit is cleared when the
                                                                      // UnLSR register is read and there are no subsequent errors in the UARTn FIFO.
                                                                      // 
                                                                      // *0: UnRBR contains no UARTn Rx errors or UnFCR[0] = 0.
                                                                      //  1: UARTn RBR contains at least one UARTn Rx error.
                                                                      // 
            [BitFieldRegister(Position=6)] public bool TEMT;          // Transmitter Empty
                                                                      // 
                                                                      // This bit is set when the last character has been transmitted from the Transmit
                                                                      // Shift Register. TEMT is cleared when another character is written to UnTHR.
                                                                      // 
                                                                      //  0: UnTHR and/or the UnTSR contains valid data.
                                                                      // *1: UnTHR and the UnTSR are empty.
                                                                      // 
            [BitFieldRegister(Position=5)] public bool THRE;          // Transmitter Holding Register Empty
                                                                      // 
                                                                      // This bit is set when the transmitter FIFO reaches the level selected in UnFCR.
                                                                      // THRE is cleared on a UnTHR write.
                                                                      // 
                                                                      //  0: UnTHR contains valid data.
                                                                      // *1: UnTHR is empty.
                                                                      // 
            [BitFieldRegister(Position=4)] public bool BI;            // Break Interrupt
                                                                      // 
                                                                      // When the Un_RX pin is held low for one full character transmission (start, data,
                                                                      // parity, stop), a break interrupt occurs. Once the break condition has been
                                                                      // detected, the receiver goes idle until the Un_RX pin goes high. A read of UnLSR
                                                                      // clears this status bit.
                                                                      // 
                                                                      // *0: Break interrupt status is inactive.
                                                                      //  1: Break interrupt status is active.
                                                                      // 
            [BitFieldRegister(Position=3)] public bool FE;            // Framing Error
                                                                      // 
                                                                      // When the stop bit of a received character is a logic 0, a framing error occurs. A
                                                                      // read of UnLSR clears this bit. A framing error is associated with the character at
                                                                      // the top of the UARTn RBR FIFO.
                                                                      // Upon detection of a framing error, the receiver will attempt to resynchronize to
                                                                      // the data and assume that the bad stop bit is actually an early start bit.
                                                                      // However, it cannot be assumed that the next received byte will be correct even if there is no Framing Error.
                                                                      // 
                                                                      // *0: Framing error status is inactive.
                                                                      //  1: Framing error status is active.
                                                                      // 
            [BitFieldRegister(Position=2)] public bool PE;            // Parity Error
                                                                      // 
                                                                      // When the parity bit of a received character is in the wrong state, a parity error occurs.
                                                                      // A read of UnLSR clears this bit. A parity error is associated with the character at the top of the UARTn RBR FIFO.
                                                                      // 
                                                                      // *0: Parity error status is inactive.
                                                                      //  1: Parity error status is active.
                                                                      // 
            [BitFieldRegister(Position=1)] public bool OE;            // Overrun Error
                                                                      // 
                                                                      // This bit is set when the UARTn RSR has a new character assembled and the UARTn RBR FIFO is full.
                                                                      // In this case, the UARTn RBR FIFO will not be overwritten and the character in the UARTn RSR will be lost.
                                                                      // The overrun error condition is set as soon as it occurs. A read of UnLSR clears the OE flag.
                                                                      // 
                                                                      // *0: Overrun error status is inactive.
                                                                      //  1: Overrun error status is active.
                                                                      // 
            [BitFieldRegister(Position=0)] public bool RDR;           // Receiver Data Ready
                                                                      // 
                                                                      // This bit is set when the UnRBR holds an unread character and is cleared when the UARTn RBR FIFO is empty.
                                                                      // 0: UnRBR is empty.
                                                                      // 1: UnRBR contains valid data.
                                                                      // 
        }

        //--//

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x8000U)]
        public class Port
        {
            [Register(Offset=0x00U)] public byte           UnRBR;   // Receiver Buffer Register      R
            [Register(Offset=0x00U)] public byte           UnTHR;   // Transmit Holding Register     W
            [Register(Offset=0x04U)] public UnIER_bitfield UnIER;   // Interrupt Enable Register     
            [Register(Offset=0x00U)] public byte           UnDLL;   // Divisor Latch Lower Byte      W
            [Register(Offset=0x04U)] public byte           UnDLM;   // Divisor Latch Upper Byte      W
            [Register(Offset=0x08U)] public UnIIR_bitfield UnIIR;   // Interrupt ID Register         R
            [Register(Offset=0x08U)] public UnFCR_bitfield UnFCR;   // FIFO Control Register         W
            [Register(Offset=0x0CU)] public UnLCR_bitfield UnLCR;   // Line Control Register
            [Register(Offset=0x14U)] public UnLSR_bitfield UnLSR;   // Line Status Register
            [Register(Offset=0x1CU)] public byte           UnRXLEV; // Receive FIFO Level Register

            //
            // Helper Methods
            //

            [Inline]
            public void EnableReceiveInterrupt()
            {
                this.UnIER.RDAE = true;
            }

            [Inline]
            public void DisableReceiveInterrupt()
            {
                this.UnIER.RDAE = false;
            }

            [Inline]
            public void EnableTransmitInterrupt()
            {
                this.UnIER.THRE = true;
            }

            [Inline]
            public void DisableTransmitInterrupt()
            {
                this.UnIER.THRE = false;
            }

            [Inline]
            public bool ReadByte( out byte rx )
            {
                if(this.CanReceive)
                {
                    rx = this.UnRBR;

                    return true;
                }
                else
                {
                    rx = 0;

                    return false;
                }
            }

            [Inline]
            public bool WriteByte( byte tx )
            {
                if(this.CanSend)
                {
                    this.UnTHR = tx;

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
                    return this.UnLSR.THRE;
                }
            }

            public bool CanReceive
            {
                [Inline]
                get
                {
                    return this.UnLSR.RDR;
                }
            }

            public bool IsTransmitInterruptEnabled
            {
                [Inline]
                get
                {
                    return this.UnIER.THRE;
                }
            }

            public bool IsReceiveInterruptEnabled
            {
                [Inline]
                get
                {
                    return this.UnIER.RDAE;
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
                    for( int i = 0; i < 0; ++i )
                    {
                        DEBUG_Write( s[ i ] );
                    }
                }
            }

            public void DEBUG_WriteHex( uint value )
            {
                DEBUG_Write( "0x" );

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

                this.UnTHR = (byte)c;
            }
        }

        //--//

        [Register(Offset=0x00000000U,Instances=4)] public Port[] Ports;

        //--//

        //
        // Helper Methods
        //

////    [Inline]
        public Port Configure( StandardUART.Id portNo     ,
                               bool            fAutoClock ,
                               int             baudrate   )
        {
            var cfg = new BaseSerialStream.Configuration( null )
                      {
                              BaudRate = baudrate,
                              DataBits = 8,
                              Parity   = System.IO.Ports.Parity.None,
                              StopBits = System.IO.Ports.StopBits.One,
                      };

            return Configure( portNo, fAutoClock, ref cfg );
        }

////    [Inline]
        public Port Configure(     StandardUART.Id                portNo     ,
                                   bool                           fAutoClock ,
                               ref BaseSerialStream.Configuration cfg        )
        {
            uint preDivX;
            uint preDivY;
            int  divisor;


            switch(cfg.BaudRate)
            {
                case   2400: preDivX =  1; preDivY = 169; divisor = 2; break;
                case   4800: preDivX =  1; preDivY = 169; divisor = 1; break;
                case   9600: preDivX =  3; preDivY = 254; divisor = 1; break;
                case  19200: preDivX =  3; preDivY = 127; divisor = 1; break;
                case  38400: preDivX =  6; preDivY = 127; divisor = 1; break;
                case  57600: preDivX =  9; preDivY = 127; divisor = 1; break;
                case 115200: preDivX = 19; preDivY = 134; divisor = 1; break;
                case 230400: preDivX = 19; preDivY =  67; divisor = 1; break;
                case 460800: preDivX = 38; preDivY =  67; divisor = 1; break;

                default:
                    return null;
            }

            var sysCtrl                    = SystemControl.Instance;
            var valUART_CLKMODE__UARTx_CLK = fAutoClock ? SystemControl.UART_CLKMODE_bitfield.Mode.AutoClock : SystemControl.UART_CLKMODE_bitfield.Mode.ClockOn;
            var valUxCLK                   = new SystemControl.UxCLK_bitfield();

            valUxCLK.UseHCLK = false;
            valUxCLK.X       = preDivX;
            valUxCLK.Y       = preDivY;

            switch(portNo)
            {
                case Id.UART3:
                    sysCtrl.UARTCLK_CTRL.Uart3_Enable = true;
                    sysCtrl.UART_CLKMODE.UART3_CLK    = valUART_CLKMODE__UARTx_CLK;
                    sysCtrl.U3CLK                     = valUxCLK;
                    break;

                case Id.UART4:
                    sysCtrl.UARTCLK_CTRL.Uart4_Enable = true;
                    sysCtrl.UART_CLKMODE.UART4_CLK    = valUART_CLKMODE__UARTx_CLK;
                    sysCtrl.U4CLK                     = valUxCLK;
                    break;

                case Id.UART5:
                    sysCtrl.UARTCLK_CTRL.Uart5_Enable = true;
                    sysCtrl.UART_CLKMODE.UART5_CLK    = valUART_CLKMODE__UARTx_CLK;
                    sysCtrl.U5CLK                     = valUxCLK;
                    break;

                case Id.UART6:
                    sysCtrl.UARTCLK_CTRL.Uart6_Enable = true;
                    sysCtrl.UART_CLKMODE.UART6_CLK    = valUART_CLKMODE__UARTx_CLK;
                    sysCtrl.U6CLK                     = valUxCLK;
                    break;

                default:
                    return null;
            }

            //--//

            var lcr = new UnLCR_bitfield();

            if(cfg.Parity != System.IO.Ports.Parity.None)
            {
                lcr.ParityEnable = true;

                switch(cfg.Parity)
                {
                    case System.IO.Ports.Parity.Even:
                        lcr.Parity = UnLCR_bitfield.ParitySettings.Even;
                        break;

                    case System.IO.Ports.Parity.Odd:
                        lcr.Parity = UnLCR_bitfield.ParitySettings.Odd;
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
                    lcr.TwoStopBits = true;
                    break;

                default:
                    return null;
            }

            switch(cfg.DataBits)
            {
                case 5: lcr.WordLen = UnLCR_bitfield.LengthSettings.Use5bits; break;
                case 6: lcr.WordLen = UnLCR_bitfield.LengthSettings.Use6bits; break;
                case 7: lcr.WordLen = UnLCR_bitfield.LengthSettings.Use7bits; break;
                case 8: lcr.WordLen = UnLCR_bitfield.LengthSettings.Use8bits; break;

                default:
                    return null;
            }

            //--//

            Port uart = this.Ports[(int)portNo];

            {
                var val = new UnIER_bitfield();

                uart.UnIER = val; // Disable both Rx and Tx interrupts
            }

            //--//

            var fcr = new UnFCR_bitfield();

            fcr.RxLvl       = UnFCR_bitfield.RxTriggerLevel.TriggerAt16;
            fcr.TxLvl       = UnFCR_bitfield.TxTriggerLevel.TriggerAt0;
            fcr.ResetRxFIFO = true;
            fcr.ResetTxFIFO = true;
            fcr.FIFOEnable  = true;

            uart.UnFCR = fcr;

            //--//

            {
                var lcr2 = new UnLCR_bitfield();

                lcr2.DLAB = true;

                uart.UnLCR = lcr2;
            }

            uart.UnDLL = (byte) divisor;
            uart.UnDLM = (byte)(divisor >> 8);

            uart.UnLCR = lcr;

            //--//

            return uart;
        }

        //
        // Access Methods
        //

        public static extern StandardUART Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}