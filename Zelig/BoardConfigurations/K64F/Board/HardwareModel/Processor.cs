//
// Copyright ((c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.K64F
{
    using System;
    using System.Runtime.InteropServices;

    using RT = Microsoft.Zelig.Runtime;
    using ARMv7 = Microsoft.Zelig.Runtime.TargetPlatform.ARMv7;
    using ChipsetModel = Microsoft.CortexM4OnMBED;


    [RT.ProductFilter( "Microsoft.Llilum.BoardConfigurations.K64FMBED" )]
    public sealed class Processor : Microsoft.CortexM4OnMBED.Processor
    {
        public new class Context : ChipsetModel.Processor.Context
        {
            public Context( RT.ThreadImpl owner ) : base( owner )
            {
            }

            public override unsafe void SwitchTo( )
            {
                //
                // BUGBUG: return to thread using VFP state as well 
                //
                base.SwitchTo( );
            }
        }

        internal static void RemapInterrupt( IRQn irqNumber )
        {
            var processor = (Microsoft.Llilum.K64F.Processor)Processor.Instance;

            using(RT.SmartHandles.InterruptState.Disable( ))
            {
                switch(irqNumber)
                {
                    ////////
                    //////// UART
                    //////// 
                    //////case IRQn.UART0_ERR_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.UART0_ERR_IRQn, Zelig_Exception_UART0_ERR_IRQHandler );
                    //////    break;
                    //////case IRQn.UART0_RX_TX_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.UART0_RX_TX_IRQn, Zelig_Exception_UART0_RX_TX_IRQHandler );
                    //////    break;
                    //////case IRQn.UART1_ERR_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.UART1_ERR_IRQn, Zelig_Exception_UART1_ERR_IRQHandler );
                    //////    break;
                    //////case IRQn.UART1_RX_TX_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.UART1_RX_TX_IRQn, Zelig_Exception_UART1_RX_TX_IRQHandler );
                    //////    break;
                    //////case IRQn.UART3_ERR_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.UART3_ERR_IRQn, Zelig_Exception_UART3_ERR_IRQHandler );
                    //////    break;
                    //////case IRQn.UART3_RX_TX_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.UART3_RX_TX_IRQn, Zelig_Exception_UART3_RX_TX_IRQHandler );
                    //////    break;
                        
                    ////////
                    //////// GPIO
                    //////// 
                    //////case IRQn.PORTA_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.PORTA_IRQn, Zelig_Exception_PORTA_IRQHandler );
                    //////    break;
                    //////case IRQn.PORTB_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.PORTB_IRQn, Zelig_Exception_PORTB_IRQHandler );
                    //////    break;
                    //////case IRQn.PORTC_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.PORTC_IRQn, Zelig_Exception_PORTC_IRQHandler );
                    //////    break;
                    //////case IRQn.PORTD_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.PORTD_IRQn, Zelig_Exception_PORTD_IRQHandler );
                    //////    break;
                    //////case IRQn.PORTE_IRQn:
                    //////    processor.RemapInterrupt( (IRQn_Type)IRQn.PORTE_IRQn, Zelig_Exception_PORTE_IRQHandler );
                    //////    break;
                        
                    //
                    // Ethernet
                    // 
                    case IRQn.ENET_Receive_IRQn:
                        processor.RemapInterrupt( (IRQn_Type)IRQn.ENET_Receive_IRQn, Zelig_Exception_ENET_Receive_IRQHandler );
                        break;

                    case IRQn.ENET_Transmit_IRQn:
                        processor.RemapInterrupt( (IRQn_Type)IRQn.ENET_Transmit_IRQn, Zelig_Exception_ENET_Transmit_IRQHandler );
                        break;

                    default:
                        RT.BugCheck.Assert( false, Zelig.Runtime.BugCheck.StopCode.IncorrectArgument );
                        break;
                }
            }
        }

        //--//

        //
        // State 
        //

        private static readonly Action s_ENET_Receive_IRQHandler;
        private static readonly Action s_ENET_Transmit_IRQHandler;
        //////private static readonly Action s_UART0_RX_TX_IRQHandler;
        //////private static readonly Action s_UART0_ERR_IRQHandler;
        //////private static readonly Action s_UART1_RX_TX_IRQHandler;
        //////private static readonly Action s_UART1_ERR_IRQHandler;
        //////private static readonly Action s_UART3_RX_TX_IRQHandler;
        //////private static readonly Action s_UART3_ERR_IRQHandler;
        //////private static readonly Action s_PORTA_IRQHandler;
        //////private static readonly Action s_PORTB_IRQHandler;
        //////private static readonly Action s_PORTC_IRQHandler;
        //////private static readonly Action s_PORTD_IRQHandler;
        //////private static readonly Action s_PORTE_IRQHandler;

        //--//

        //
        // Constructors
        //

        static Processor( )
        {
            s_ENET_Transmit_IRQHandler  = new Action( ENET_Transmit_IRQHandler_Wrapper );
            s_ENET_Receive_IRQHandler   = new Action( ENET_Receive_IRQHandler_Wrapper );
            //////s_UART0_RX_TX_IRQHandler    = new Action( UART0_RX_TX_IRQHandler );
            //////s_UART0_ERR_IRQHandler      = new Action( UART0_ERR_IRQHandler );
            //////s_UART1_RX_TX_IRQHandler    = new Action( UART1_RX_TX_IRQHandler );
            //////s_UART1_ERR_IRQHandler      = new Action( UART1_ERR_IRQHandler );
            //////s_UART3_RX_TX_IRQHandler    = new Action( UART3_RX_TX_IRQHandler );
            //////s_UART3_ERR_IRQHandler      = new Action( UART3_ERR_IRQHandler );
            //////s_PORTA_IRQHandler          = new Action( PORTA_IRQHandler );
            //////s_PORTB_IRQHandler          = new Action( PORTB_IRQHandler );
            //////s_PORTC_IRQHandler          = new Action( PORTC_IRQHandler );
            //////s_PORTD_IRQHandler          = new Action( PORTD_IRQHandler );
            //////s_PORTE_IRQHandler          = new Action( PORTE_IRQHandler );
        }

        //
        // Helper methods
        //

        [RT.Inline]
        public override void InitializeProcessor( )
        {
            base.InitializeProcessor( );

            DisableMPU( );
        }

        [RT.Inline]
        public override Microsoft.Zelig.Runtime.Processor.Context AllocateProcessorContext( RT.ThreadImpl owner )
        {
            return new Context( owner );
        }

        //--//

        private unsafe void DisableMPU( )
        {
            CUSTOM_STUB_K64F_DisableMPU( );
        }

        [DllImport( "C" )]
        private static extern void CUSTOM_STUB_K64F_DisableMPU( );

        //--//
        //--//
        //--//

        #region Warapper for exception handlers

        //
        // UART
        //  

        private static void UART0_RX_TX_IRQHandler_Wrapper( )
        {
            UART0_RX_TX_IRQHandler( );
        }

        private static void UART0_ERR_IRQHandler_Wrapper( )
        {
            UART0_ERR_IRQHandler( );
        }

        private static void UART1_RX_TX_IRQHandler_Wrapper( )
        {
            UART1_RX_TX_IRQHandler( );
        }

        private static void UART1_ERR_IRQHandler_Wrapper( )
        {
            UART1_ERR_IRQHandler( );
        }

        private static void UART3_RX_TX_IRQHandler_Wrapper( )
        {
            UART3_RX_TX_IRQHandler( );
        }

        private static void UART3_ERR_IRQHandler_Wrapper( )
        {
            UART3_ERR_IRQHandler( );
        }

        //
        // GPIO
        // 

        private static void PORTA_IRQHandler_Wrapper( )
        {
            PORTA_IRQHandler( );
        }

        private static void PORTB_IRQHandler_Wrapper( )
        {
            PORTB_IRQHandler( );
        }

        private static void PORTC_IRQHandler_Wrapper( )
        {
            PORTC_IRQHandler( );
        }

        private static void PORTD_IRQHandler_Wrapper( )
        {
            PORTD_IRQHandler( );
        }

        private static void PORTE_IRQHandler_Wrapper( )
        {
            PORTE_IRQHandler( );
        }

        //
        // Ethernet
        // 

        private static void ENET_Transmit_IRQHandler_Wrapper( )
        {
            ENET_Transmit_IRQHandler( );
        }

        private static void ENET_Receive_IRQHandler_Wrapper( )
        {
            ENET_Receive_IRQHandler( );
        }

        #endregion

        //--//
        //--//
        //--//

        #region Exported exception handlers

        //
        // Export all exception handler for native platform to call
        // 

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA2_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA2_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA3_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA3_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA4_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA4_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA5_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA5_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA6_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA6_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA7_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA7_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA8_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA8_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA9_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA9_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA10_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA10_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA11_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA11_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA12_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA12_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA13_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA13_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA14_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA14_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA15_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA15_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DMA_Error_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DMA_Error_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_MCM_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( MCM_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_FTFE_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( FTFE_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_Read_Collision_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( Read_Collision_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_LVD_LVW_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( LVD_LVW_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_LLW_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( LLW_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_Watchdog_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( Watchdog_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_RNG_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( RNG_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_I2C0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( I2C0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_I2C1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( I2C1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_SPI0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( SPI0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_SPI1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( SPI1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_I2S0_Tx_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( I2S0_Tx_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_I2S0_Rx_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( I2S0_Rx_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART0_LON_IRQHandler()
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler(UART0_LON_IRQHandler);
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART0_RX_TX_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_UART0_RX_TX_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART0_ERR_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_UART0_ERR_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART1_RX_TX_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_UART1_RX_TX_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////[RT.DisableNullChecks]
        //////private static void Zelig_Exception_UART1_ERR_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_UART1_ERR_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART2_RX_TX_IRQHandler()
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler(UART2_RX_TX_IRQHandler);
        //////}

        //////[RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART2_ERR_IRQHandler()
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler(UART2_ERR_IRQHandler);
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART3_RX_TX_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_UART3_RX_TX_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART3_ERR_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_UART3_ERR_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_ADC0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( ADC0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CMP0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CMP0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CMP1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CMP1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_FTM0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( FTM0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_FTM1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( FTM1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_FTM2_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( FTM2_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CMT_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CMT_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_RTC_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( RTC_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_RTC_Seconds_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( RTC_Seconds_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PIT0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( PIT0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PIT1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( PIT1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PIT2_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( PIT2_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PIT3_IRQHandler()
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler(PIT3_IRQHandler);
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PDB0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( PDB0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_USB0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( USB0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_USBDCD_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( USBDCD_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_Reserved71_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( Reserved71_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DAC0_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DAC0_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_MCG_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( MCG_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_LPTimer_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( LPTimer_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PORTA_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_PORTA_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PORTB_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_PORTB_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PORTC_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_PORTC_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PORTD_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_PORTD_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_PORTE_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( s_PORTE_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_SWI_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( SWI_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_SPI2_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( SPI2_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART4_RX_TX_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( UART4_RX_TX_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART4_ERR_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( UART4_ERR_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART5_RX_TX_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( UART5_RX_TX_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_UART5_ERR_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( UART5_ERR_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CMP2_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CMP2_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_FTM3_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( FTM3_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_DAC1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DAC1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_ADC1_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( ADC1_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_I2C2_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( I2C2_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CAN0_ORed_Message_buffer_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CAN0_ORed_Message_buffer_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CAN0_Bus_Off_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CAN0_Bus_Off_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CAN0_Error_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CAN0_Error_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CAN0_Tx_Warning_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CAN0_Tx_Warning_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CAN0_Rx_Warning_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CAN0_Rx_Warning_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_CAN0_Wake_Up_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( CAN0_Wake_Up_IRQHandler );
        //////}

        //////[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //////[RT.ExportedMethod]
        //////private static void Zelig_Exception_SDHC_IRQHandler( )
        //////{
        //////    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( SDHC_IRQHandler );
        //////}

        //[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //[RT.ExportedMethod]
        //private static void Zelig_Exception_ENET_1588_Timer_IRQHandler( )
        //{
        //    //ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( ENET_1588_Timer_IRQHandler );
        //    ENET_1588_Timer_IRQHandler( );
        //}

        [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        [RT.ExportedMethod]
        [RT.DisableNullChecks]
        private static void Zelig_Exception_ENET_Transmit_IRQHandler( )
        {
            ARMv7.ProcessorARMv7MForLlvm.ExclusiveAccessExceptionHandler( s_ENET_Transmit_IRQHandler );
        }

        [RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        [RT.ExportedMethod]
        [RT.DisableNullChecks]
        private static void Zelig_Exception_ENET_Receive_IRQHandler( )
        {
            ARMv7.ProcessorARMv7MForLlvm.ExclusiveAccessExceptionHandler( s_ENET_Receive_IRQHandler );
        }

        //[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //[RT.ExportedMethod]
        //private static void Zelig_Exception_ENET_Error_IRQHandler( )
        //{
        //    //ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( ENET_Error_IRQHandler );
        //    ENET_Error_IRQHandler( ); 
        //}

        //[RT.HardwareExceptionHandler( RT.HardwareException.Interrupt )]
        //[RT.ExportedMethod]
        //private static void Zelig_Exception_DefaultISR( )
        //{
        //    ARMv7.ProcessorARMv7M.ExclusiveAccessExceptionHandler( DefaultISR );
        //}

        #endregion

        //--//
        //--//
        //--//

        #region Imported exception handlers

        //
        // Import all exception handler from native platform
        // 

        [DllImport( "C" )]
        private static extern void NMI_Handler( );

        [DllImport( "C" )]
        private static extern void HardFault_Handler( );

        [DllImport( "C" )]
        private static extern void MemManage_Handler( );

        [DllImport( "C" )]
        private static extern void BusFault_Handler( );

        [DllImport( "C" )]
        private static extern void UsageFault_Handler( );

        [DllImport( "C" )]
        private static extern void SVC_Handler( );

        [DllImport( "C" )]
        private static extern void DebugMon_Handler( );

        [DllImport( "C" )]
        private static extern void PendSV_Handler( );

        [DllImport( "C" )]
        private static extern void SysTick_Handler( );

        [DllImport( "C" )]
        private static extern void DMA0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA2_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA3_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA4_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA5_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA6_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA7_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA8_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA9_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA10_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA11_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA12_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA13_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA14_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA15_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DMA_Error_IRQHandler( );

        [DllImport( "C" )]
        private static extern void MCM_IRQHandler( );

        [DllImport( "C" )]
        private static extern void FTFE_IRQHandler( );

        [DllImport( "C" )]
        private static extern void Read_Collision_IRQHandler( );

        [DllImport( "C" )]
        private static extern void LVD_LVW_IRQHandler( );

        [DllImport( "C" )]
        private static extern void LLW_IRQHandler( );

        [DllImport( "C" )]
        private static extern void Watchdog_IRQHandler( );

        [DllImport( "C" )]
        private static extern void RNG_IRQHandler( );

        [DllImport( "C" )]
        private static extern void I2C0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void I2C1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void SPI0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void SPI1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void I2S0_Tx_IRQHandler( );

        [DllImport( "C" )]
        private static extern void I2S0_Rx_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART0_LON_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART0_RX_TX_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART0_ERR_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART1_RX_TX_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART1_ERR_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART2_RX_TX_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART2_ERR_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART3_RX_TX_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART3_ERR_IRQHandler( );

        [DllImport( "C" )]
        private static extern void ADC0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CMP0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CMP1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void FTM0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void FTM1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void FTM2_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CMT_IRQHandler( );

        [DllImport( "C" )]
        private static extern void RTC_IRQHandler( );

        [DllImport( "C" )]
        private static extern void RTC_Seconds_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PIT0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PIT1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PIT2_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PIT3_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PDB0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void USB0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void USBDCD_IRQHandler( );

        [DllImport( "C" )]
        private static extern void Reserved71_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DAC0_IRQHandler( );

        [DllImport( "C" )]
        private static extern void MCG_IRQHandler( );

        [DllImport( "C" )]
        private static extern void LPTimer_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PORTA_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PORTB_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PORTC_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PORTD_IRQHandler( );

        [DllImport( "C" )]
        private static extern void PORTE_IRQHandler( );

        [DllImport( "C" )]
        private static extern void SWI_IRQHandler( );

        [DllImport( "C" )]
        private static extern void SPI2_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART4_RX_TX_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART4_ERR_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART5_RX_TX_IRQHandler( );

        [DllImport( "C" )]
        private static extern void UART5_ERR_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CMP2_IRQHandler( );

        [DllImport( "C" )]
        private static extern void FTM3_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DAC1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void ADC1_IRQHandler( );

        [DllImport( "C" )]
        private static extern void I2C2_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CAN0_ORed_Message_buffer_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CAN0_Bus_Off_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CAN0_Error_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CAN0_Tx_Warning_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CAN0_Rx_Warning_IRQHandler( );

        [DllImport( "C" )]
        private static extern void CAN0_Wake_Up_IRQHandler( );

        [DllImport( "C" )]
        private static extern void SDHC_IRQHandler( );

        [DllImport( "C" )]
        private static extern void ENET_1588_Timer_IRQHandler( );

        [DllImport( "C" )]
        private static extern void ENET_Transmit_IRQHandler( );

        [DllImport( "C" )]
        private static extern void ENET_Receive_IRQHandler( );

        [DllImport( "C" )]
        private static extern void ENET_Error_IRQHandler( );

        [DllImport( "C" )]
        private static extern void DefaultISR( );

        #endregion
    }
}
