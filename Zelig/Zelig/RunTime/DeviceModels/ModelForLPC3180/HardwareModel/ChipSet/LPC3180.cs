//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;

/*

    0x2002 0000 SLC_DATA SLC NAND flash Data Register - R/W
    0x2002 0004 SLC_ADDR SLC NAND flash Address Register - W
    0x2002 0008 SLC_CMD SLC NAND flash Command Register - W
    0x2002 000C SLC_STOP SLC NAND flash STOP Register - W
    0x2002 0010 SLC_CTRL SLC NAND flash Control Register 0x00 R/W
    0x2002 0014 SLC_CFG SLC NAND flash Configuration Register 0x00 R/W
    0x2002 0018 SLC_STAT SLC NAND flash Status Register 00X binary R
    0x2002 001C SLC_INT_STAT SLC NAND flash Interrupt Status Register 0x00 R
    0x2002 0020 SLC_IEN SLC NAND flash Interrupt enable register 0x00 R/W
    0x2002 0024 SLC_ISR SLC NAND flash Interrupt set register 0x00 W
    0x2002 0028 SLC_ICR SLC NAND flash Interrupt clear register 0x00 W
    0x2002 002C SLC_TAC SLC NAND flash Read Timing Arcs Configuration Register 0x00 R/W
    0x2002 0030 SLC_TC SLC NAND flash Transfer Count Register 0x00 R/W
    0x2002 0034 SLC_ECC SLC NAND flash Parity bits 0x00 R
    0x2002 0038 SLC_DMA_DATA SLC NAND flash DMA DATA - R/W

    0x4002 8000 PIO_INP_STATE Input pin state register. Reads the state of input pins. - RO
    0x4002 8004 PIO_OUTP_SET Output pin set register. Allows setting output pin(s). - WO
    0x4002 8008 PIO_OUTP_CLR Output pin clear register. Allows clearing output pin(s). - WO
    0x4002 800C PIO_OUTP_STATE Output pin state register. Reads the state of output pins. - RO
    0x4002 8010 PIO_DIR_SET GPIO direction set register. Configures I/O pins as outputs. - WO
    0x4002 8014 PIO_DIR_CLR GPIO direction clear register. Configures I/O pins as inputs. - WO
    0x4002 8018 PIO_DIR_STATE GPIO direction state register. Reads back pin directions. 0 RO
    0x4002 801C PIO_SDINP_STATE Input pin state register for SDRAM pins. Reads the state of SDRAM input pins. - RO
    0x4002 8020 PIO_SDOUTP_SET Output pin set register for SDRAM pins. Allows setting SDRAM output pin(s). - WO
    0x4002 8024 PIO_SDOUTP_CLR Output pin clear register for SDRAM pins. Allows clearing SDRAM output pin(s). - WO
    0x4002 8028 PIO_MUX_SET PIO multiplexer control set register. Controls the selection of alternate functions on certain pins. - WO
    0x4002 802C PIO_MUX_CLR PIO multiplexer control clear register. Controls the selection of alternate functions on certain pins. - WO
    0x4002 8030 PIO_MUX_STATE PIO multiplexer state register. Reads back the selection of alternate functions on certain pins. 0x00000000 RO

    Device interrupt registers
    0x3102 0200 USBDevIntSt Device Interrupt Status R Interrupt status register for the device
    0x3102 0204 USBDevInt En Device Interrupt Enable R/W Enable external interrupt generation
    0x3102 0208 USBDevIntClr Device Interrupt Clear C Clears device interrupt status
    0x3102 020C USBDevIntSet Device Interrupt Set S Sets device interrupt status
    0x3102 022C USBDevIntPri Device Interrupt Priority W Interrupt priority register

    Endpoint interrupt registers
    0x3102 0230 USBEpIntSt Endpoint Interrupt Status R Interrupt status register for endpoints
    0x3102 0234 USBEpIntEn Endpoint Interrupt Enable R/W Enable endpoint interrupt generation
    0x3102 0238 USBEpIntClr Endpoint Interrupt Clear C Clears endpoint interrupt status
    0x3102 023C USBEpIntSet Endpoint Interrupt Set S Sets endpoint interrupt status
    0x3102 0240 USBEpIntPri Endpoint Interrupt Priority W Defines in which interrupt line the endpoint interrupt will be routed

    Endpoint realization registers
    0x3102 0244 USBReEp Realize Endpoint R/W Defines which endpoints are to be realized
    0x3102 0248 USBEpInd Endpoint Index W Pointer to the maxpacketsize register array
    0x3102 024C USBEpMaxPSize MaxPacket Size R/W Max packet size register array

    Data transfer registers
    0x3102 0218 USBRxData Receive Data R Register from which data corresponding to the OUT endpoint packet is to be read
    0x3102 0220 USBRxPLen Receive PacketLength R Register from which packet length corresponding to the OUT endpoint packet is to be read
    0x3102 021C USBTxData Transmit Data W Register to which data to the IN endpoint is to be written
    0x3102 0224 USBTxPLen Transmit PacketLength W Register to which packet length for IN endpoint is to be written
    0x3102 0228 USBCtrl USB Control R/W Controls read-write operation

    Command registers
    0x3102 0210 USBCmdCode Command Code W Register to which command has to be written
    0x3102 0214 USBCmdData Command Data R Register from which data resulting from the execution of command to be read

    DMA registers
    0x3102 0250 USBDMARSt DMA Request Status R The DMA request status register
    0x3102 0254 USBDMARClr DMA Request Clear C DMA request clear register
    0x3102 0258 USBDMARSet DMA Request Set S DMA Request set register
    0x3102 0280 USBUDCAH UDCA_Head R/W DD pointer address location
    0x3102 0284 USBEpDMASt EP DMA Status R DMA enable status for each endpoint
    0x3102 0288 USBEpDMAEn EP DMA Enable S Endpoint DMA enable register
    0x3102 028C USBEpDMADis EP DMA Disable C Endpoint DMA disable register
    0x3102 0290 USBDMAIntSt DMA Interrupt Status R DMA Interrupt status register
    0x3102 0294 USBDMAIntEn DMA Interrupt Enable R/W DMA Interrupt enable register
    0x3102 02A0 USBEoTIntSt End Of Transfer Interrupt Status R DMA transfer complete interrupt status register
    0x3102 02A4 USBEoTIntClr End Of Transfer Interrupt Clear C DMA transfer complete interrupt clear register
    0x3102 02A8 USBEoTIntSet End Of Transfer Interrupt Set S DMA transfer complete interrupt set register
    0x3102 02AC USBNDDRIntSt New DD Request Interrupt Status R New DD request interrupt status register
    0x3102 02B0 USBNDDRIntClr New DD Request Interrupt Clear C New DD request interrupt clear register
    0x3102 02B4 USBNDDRIntSet New DD Request Interrupt Set S New DD request interrupt set register
    0x3102 02B8 USBSysErrIntSt System Error Interrupt Status R System error interrupt status register
    0x3102 02BC USBSysErrIntClr System Error Interrupt Clear C System error interrupt clear register
    0x3102 02C0 USBSysErrIntSet System Error Interrupt Set S System error interrupt set register


    OTG registers
    0x3102 0100 OTG_int_status R This register holds the status of the OTG interrupts
    0x3102 0104 OTG_int_enable R/W This register is used for enabling the OTG interrupts
    0x3102 0108 OTG_int_set S This register is used for setting the interrupts
    0x3102 010C OTG_int_clear C This register is used for clearing the interrupts
    0x3102 0110 OTG_status R/W This register is used to monitor and control the operation of the OTG controller
    0x3102 0114 OTG_timer R/W Timer to be used for various OTG time-out activities

    I2C registers
    0x3102 0300 I2C_RX R Receive FIFO
    0x3102 0300 I2C_TX W Transmit FIFO
    0x3102 0304 I2C_STS R Status
    0x3102 0308 I2C_CTL R/W Control
    0x3102 030C I2C_CLKHI R/W Clock division high, set to run min frequency
    0x3102 0310 I2C_CLKLO W Clock division low, set to run min frequency

    Clock control registers
    0x3102 0FF4 OTG_clock_control R/W Controls clocking of the OTG controller
    0x3102 0FF8 OTG_clock_status R Clock availability status


    UART Base address
    1 0x4001 4000
    2 0x4001 8000
    7 0x4001 C000

    0x00 HSUn_RX High speed UARTn Receiver FIFO 0x1XX RO
    0x00 HSUn_TX High speed UARTn Transmitter FIFO - WO
    0x04 HSUn_LEVEL High speed UARTn FIFO Level Register 0 RO
    0x08 HSUn_IIR High speed UARTn Interrupt Identification Register 0 R/W
    0x0C HSUn_CTRL High speed UARTn Control Register 0x0000 2800 R/W
    0x10 HSUn_RATE High speed UARTn Rate Control Register 0 R/W



    0x2008 8000 SPI1_GLOBAL;
    0x2009 0000 SPI2_GLOBAL  SPIn Global Control Register. Controls resetting and enabling of SPI1 and SPI2. 0 R/W
    0x2008 8004 SPI1_CON;
    0x2009 0004 SPI2_CON SPIn Control Register. Controls many details of SPI operation. 0x0E08 R/W
    0x2008 8008 SPI1_FRM; 
    0x2009 0008 SPI2_FRM SPIn Frame Count Register. Selects the number of SPI frames to be transferred. 0 R/W
    0x2008 800C SPI1_IER; 
    0x2009 000C SPI2_IER SPIn Interrupt Enable Register. Enables or disables the 3 types of interrupts that may be generated by the SPI. 0 R/W
    0x2008 8010 SPI1_STAT; 
    0x2009 0010 SPI2_STAT SPIn Status Register. Provides information on conditions in the SPI interface. 0x01 R/W
    0x2008 8014 SPI1_DAT;
    0x2009 0014 SPI2_DAT SPIn Data Buffer Register. Provides access to the transmit and receive FIFO buffers. 0 R/W
    0x2008 8400 SPI1_TIM_CTRL;
    0x2009 0400 SPI2_TIM_CTRL SPIn Timer Control Register. Controls the generation of timed interrupts. 0x02 R/W
    0x2008 8404 SPI1_TIM_COUNT
    0x2009 0404 SPI2_TIM_COUNT SPIn Timer Counter Register. This is the counter for timed interrupts. 0 R/W
    0x2008 8408 SPI1_TIM_STAT
    0x2009 0408 SPI2_TIM_STAT SPIn Timer Status Register. Contains the timed interrupt pending flag. 0 R/W


    0x2009 8000 SD_Power Power Control Register 0x0000 0000 R/W
    0x2009 8004 SD_Clock Clock Control Register 0x0000 0000 R/W
    0x2009 8008 SD_Argument Argument register 0x0000 0000 R/W
    0x2009 800C SD_Command Command register 0x0000 0000 R/W
    0x2009 8010 SD_Respcmd Command response register 0x0000 0000 RO
    0x2009 8014 SD_Response0 Response register 0 0x0000 0000 RO
    0x2009 8018 SD_Response1 Response register 1 0x0000 0000 RO
    0x2009 800C SD_Response2 Response register 2 0x0000 0000 RO
    0x2009 8020 SD_Response3 Response register 3 0x0000 0000 RO
    0x2009 8024 SD_DataTimer Data Timer 0x0000 0000 R/W
    0x2009 8028 SD_DataLength Data Length register 0x0000 0000 R/W
    0x2009 802C SD_DataCtrl Data Control register 0x0000 0000 R/W
    0x2009 8030 SD_DataCnt Data counter 0x0000 0000 RO
    0x2009 8034 SD_Status Status register 0x0000 0000 RO
    0x2009 8038 SD_Clear Clear register 0x0000 0000 WO
    0x2009 803C SD_Mask0 Interrupt mask register 0 0x0000 0000 R/W
    0x2009 8040 SD_Mask1 Interrupt mask register 1 0x0000 0000 R/W
    0x2009 8048 SD_FIFOCnt FIFO counter 0x0000 0000 RO
    0x2009 8080 to
    0x2009 80BC SD_FIFO Data FIFO register 0x0000 0000 R/W


    I2C block Base address
    1 0x400A 0000
    2 0x400A 8000

    0x00 I2Cn_RX I2Cn RX Data FIFO RO
    0x00 I2Cn_TX I2Cn TX Data FIFO WO
    0x04 I2Cn_STS I2Cn Status Register RO
    0x08 I2Cn_CTRL I2Cn Control Register R/W
    0x0C I2Cn_CLK_HI I2Cn Clock Divider high R/W
    0x10 I2Cn_CLK_LO I2Cn Clock Divider low R/W


    0x4005 0000 KS_DEB Keypad de-bouncing duration register 0x05 R/W
    0x4005 0004 KS_STATE_COND Keypad state machine current state register 0x00 RO
    0x4005 0008 KS_IRQ Keypad interrupt register 0x01 R/W
    0x4005 000C KS_SCAN_CTL Keypad scan delay control register 0x05 R/W
    0x4005 0010 KS_FAST_TST Keypad scan clock control register 0x02 R/W
    0x4005 0014 KS_MATRIX_DIM Keypad Matrix Dimension select register 0x06 R/W
    0x4005 0040 KS_DATA0 Keypad data register 0 0x00 RO
    0x4005 0044 KS_DATA1 Keypad data register 1 0x00 RO
    0x4005 0048 KS_DATA2 Keypad data register 2 0x00 RO
    0x4005 004C KS_DATA3 Keypad data register 3 0x00 RO
    0x4005 0050 KS_DATA4 Keypad data register 4 0x00 RO
    0x4005 0054 KS_DATA5 Keypad data register 5 0x00 RO
    0x4005 0058 KS_DATA6 Keypad data register 6 0x00 RO
    0x4005 005C KS_DATA7 Keypad data register 7 0x00 RO


    0x4005 C000 PWM1_CTRL PWM1 Control register. 0x0 R/W
    0x4005 C004 PWM2_CTRL PWM2 Control register. 0x0 R/W 


    0x4002 4000 RTC_UCOUNT RTC up counter value register 0 R/W
    0x4002 4004 RTC_DCOUNT RTC down counter value register 0 R/W
    0x4002 4008 RTC_MATCH0 RTC match 0 register 0 R/W
    0x4002 400C RTC_MATCH1 RTC match 1 register 0 R/W
    0x4002 4010 RTC_CTRL RTC control register 0 R/W
    0x4002 4014 RTC_INTSTAT RTC Interrupt status register 0 R/W
    0x4002 4018 RTC_KEY RTC Key register 0 R/W
    0x4002 4080 to
    0x4002 40FF RTC_SRAM Battery RAM 0


    0x4003 C000 WDTIM_INT Watchdog timer interrupt status register 0 R/W
    0x4003 C004 WDTIM_CTRL Watchdog timer control register 0 R/W
    0x4003 C008 WDTIM_COUNTER Watchdog timer counter value register 0 R/W
    0x4003 C00C WDTIM_MCTRL Watchdog timer match control register 0 R/W
    0x4003 C010 WDTIM_MATCH0 Watchdog timer match 0 register 0 R/W
    0x4003 C014 WDTIM_EMR Watchdog timer external match control register 0 R/W
    0x4003 C018 WDTIM_PULSE Watchdog timer reset pulse length register 0 R/W
    0x4003 C01C WDTIM_RES Watchdog timer reset source register 0 RO

    0x4004 8004 ADSEL A/D Select Register 0x04 R/W
    0x4004 8008 ADCON A/D Control Register 0x0000 R/W
    0x4004 8048 ADDAT A/D Data Register 0x00000 R/- 

 */

}
