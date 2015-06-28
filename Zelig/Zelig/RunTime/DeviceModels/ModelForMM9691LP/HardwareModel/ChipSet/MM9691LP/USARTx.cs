//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral]
    public abstract class USARTx
    {
        public class ComputeBaudRates
        {
            static void ComputeBestFit( int clockFrequency, int baudRate, out double bestBaudRate, out int bestO, out int bestN, out int bestP )
            {
                double bestError = baudRate;

                bestBaudRate = 0;
                bestO        = 0;
                bestN        = 0;
                bestP        = 0;

                for(int O = 16; O >= 7; O--)
                {
                    for(int P = 2; P <= 32; P++)
                    {
                        for(int N = 1; N < 1 << 11; N++)
                        {
                            double newBaudRate = clockFrequency * 2.0 / (O * N * P);

                            double error = Math.Abs( newBaudRate - baudRate );

                            if(error < bestError)
                            {
                                bestBaudRate = newBaudRate;
                                bestError    = error;

                                bestO = O;
                                bestN = N;
                                bestP = P;
                            }
                        }
                    }
                }
            }

            public static void BuildTable( int clockFrequency )
            {
                foreach(var baudRate in new int[] { 300, 600, 1200, 1800, 2000, 2400, 3600, 4800, 7200, 9600, 14400, 19200, 38400, 56000, 115200, 128000, 230400, 345600, 460800 })
                {
                    double bestBaudRate;
                    int    bestO;
                    int    bestN;
                    int    bestP;

                    ComputeBestFit( clockFrequency, baudRate, out bestBaudRate, out bestO, out bestN, out bestP );

                    Console.WriteLine( "BaudRate={0}  => O={1} N={2} P={3}   error = {4,4:F2}%", baudRate, bestO, bestN, bestP / 2.0, Math.Abs( bestBaudRate - baudRate ) * 100.0 / baudRate );
                }
            }
        }

        //--//

        const uint SER1_CLKX = GPIO.c_Pin_00;
        const uint SER1_TDX  = GPIO.c_Pin_01;
        const uint SER1_RDX  = GPIO.c_Pin_02;
        const uint SER1_RTS  = GPIO.c_Pin_03;
        const uint SER1_CTS  = GPIO.c_Pin_04;
        const uint SER2_CLKX = GPIO.c_Pin_08;
        const uint SER2_TDX  = GPIO.c_Pin_09;
        const uint SER2_RDX  = GPIO.c_Pin_10;
        const uint SER2_RTS  = GPIO.c_Pin_11;
        const uint SER2_CTS  = GPIO.c_Pin_12;

        //--//

        public const byte UnICTRL__TBE           = 0x01;
        public const byte UnICTRL__RBF           = 0x02;
        public const byte UnICTRL__DCTS          = 0x04;
        public const byte UnICTRL__CTS           = 0x08;
        public const byte UnICTRL__EFCI          = 0x10;
        public const byte UnICTRL__ETI           = 0x20;
        public const byte UnICTRL__ERI           = 0x40;
        public const byte UnICTRL__EEI           = 0x80;
        public const byte UnICTRL__mask          = (UnICTRL__EFCI | UnICTRL__ETI | UnICTRL__ERI | UnICTRL__EEI);

        public const byte UnSTAT__PE             = 0x01;
        public const byte UnSTAT__FE             = 0x02;
        public const byte UnSTAT__DOE            = 0x04;
        public const byte UnSTAT__ERR            = 0x08;
        public const byte UnSTAT__BKD            = 0x10;
        public const byte UnSTAT__RB9            = 0x20;
        public const byte UnSTAT__XMIP           = 0x40;

        public const byte UnFRS__CHAR_8          = 0x00;
        public const byte UnFRS__CHAR_7          = 0x01;
        public const byte UnFRS__CHAR_9          = 0x02;
        public const byte UnFRS__CHAR_9_LOOPBACK = 0x03;
        public const byte UnFRS__STP_1           = 0x00;
        public const byte UnFRS__STP_2           = 0x04;
        public const byte UnFRS__XB9_0           = 0x00;
        public const byte UnFRS__XB9_1           = 0x08;
        public const byte UnFRS__PSEL_ODD        = 0x00;
        public const byte UnFRS__PSEL_EVEN       = 0x10;
        public const byte UnFRS__PSEL_MARK       = 0x20;
        public const byte UnFRS__PSEL_SPACE      = 0x30;
        public const byte UnFRS__PEN_DISABLED    = 0x00;
        public const byte UnFRS__PEN_ENABLED     = 0x40;

        public const byte UnMDSL1__MOD           = 0x01;
        public const byte UnMDSL1__ATN           = 0x02;
        public const byte UnMDSL1__BRK           = 0x04;
        public const byte UnMDSL1__CKS           = 0x08;
        public const byte UnMDSL1__ETD           = 0x10;
        public const byte UnMDSL1__ERD           = 0x20;
        public const byte UnMDSL1__FCE           = 0x40;
        public const byte UnMDSL1__RTS           = 0x80;

        public static byte UnBAUD__set    ( uint a ) { return (byte) ((a-1) & 0x00FF)      ; }
        public static byte UnPSR__DIV__set( uint a ) { return (byte)(((a-1) & 0x0700) >> 8); }
        public static byte UnPSR__PSC__set( uint a ) { return (byte)(( a    & 0x001F) << 3); }

        public const byte UnPSR__1              =  1;
        public const byte UnPSR__1p5            =  2;
        public const byte UnPSR__2              =  3;
        public const byte UnPSR__2p5            =  4;
        public const byte UnPSR__3              =  5;
        public const byte UnPSR__3p5            =  6;
        public const byte UnPSR__4              =  7;
        public const byte UnPSR__4p5            =  8;
        public const byte UnPSR__5              =  9;
        public const byte UnPSR__5p5            = 10;
        public const byte UnPSR__6              = 11;
        public const byte UnPSR__6p5            = 12;
        public const byte UnPSR__7              = 13;
        public const byte UnPSR__7p5            = 14;
        public const byte UnPSR__8              = 15;
        public const byte UnPSR__8p5            = 16;
        public const byte UnPSR__9              = 17;
        public const byte UnPSR__9p5            = 18;
        public const byte UnPSR__10             = 19;
        public const byte UnPSR__10p5           = 20;
        public const byte UnPSR__11             = 21;
        public const byte UnPSR__11p5           = 22;
        public const byte UnPSR__12             = 23;
        public const byte UnPSR__12p5           = 24;
        public const byte UnPSR__13             = 25;
        public const byte UnPSR__13p5           = 26;
        public const byte UnPSR__14             = 27;
        public const byte UnPSR__14p5           = 28;
        public const byte UnPSR__15             = 29;
        public const byte UnPSR__15p5           = 30;
        public const byte UnPSR__16             = 31;

        public const byte UnOVSR__7              = 0x07;
        public const byte UnOVSR__8              = 0x08;
        public const byte UnOVSR__9              = 0x09;
        public const byte UnOVSR__10             = 0x0A;
        public const byte UnOVSR__11             = 0x0B;
        public const byte UnOVSR__12             = 0x0C;
        public const byte UnOVSR__13             = 0x0D;
        public const byte UnOVSR__14             = 0x0E;
        public const byte UnOVSR__15             = 0x0F;
        public const byte UnOVSR__16             = 0x00;

        public static byte UnOVSR__set( uint a ) { return (byte)(a & 0x0F); }

        public const byte UnMDSL2__SMD           = 0x01;

        //--//

        [Register(Offset=0x00000000U)] public byte UnTBUF;                  // Read/Write
        [Register(Offset=0x00000004U)] public byte UnRBUF;                  // Read Only
        [Register(Offset=0x00000008U)] public byte UnICTRL = UnICTRL__TBE;
        [Register(Offset=0x0000000CU)] public byte UnSTAT;
        [Register(Offset=0x00000010U)] public byte UnFRS;
        [Register(Offset=0x00000014U)] public byte UnMDSL1;
        [Register(Offset=0x00000018U)] public byte UnBAUD;
        [Register(Offset=0x0000001CU)] public byte UnPSR;
        [Register(Offset=0x00000020U)] public byte UnOVSR;
        [Register(Offset=0x00000024U)] public byte UnMDSL2;
        [Register(Offset=0x00000028U)] public byte UnSPOS;

        //
        // Helper Methods
        //

        protected bool Initialize( int usartNum ,
                                   int baudrate )
        {
            var cfg = new BaseSerialStream.Configuration( null )
                      {
                              BaudRate = baudrate,
                              DataBits = 8,
                              Parity   = System.IO.Ports.Parity.None,
                              StopBits = System.IO.Ports.StopBits.One,
                      };

            return Initialize( usartNum, ref cfg );
        }

        protected bool Initialize(     int                            usartNum ,
                                   ref BaseSerialStream.Configuration cfg      )
        {
            uint OversampleRate;
            uint Divisor;
            uint Prescaler;

            if(Configuration.CoreClockFrequency == 26000000)
            {
                //    BaudRate=300     => O=16 N=471 P=11.5   error = 0.00%
                //    BaudRate=600     => O=12 N=314 P=11.5   error = 0.00%
                //    BaudRate=1200    => O=12 N=157 P=11.5   error = 0.00%
                //    BaudRate=1800    => O=8  N=157 P=11.5   error = 0.00%
                //    BaudRate=2000    => O=16 N=325 P=2.5   error = 0.00%
                //    BaudRate=2400    => O=7  N=619 P=2.5   error = 0.01%
                //    BaudRate=3600    => O=15 N=321 P=1.5   error = 0.00%
                //    BaudRate=4800    => O=8  N=677 P=1   error = 0.01%
                //    BaudRate=7200    => O=14 N=258 P=1   error = 0.02%
                //    BaudRate=9600    => O=14 N=129 P=1.5   error = 0.02%
                //    BaudRate=14400   => O=14 N=129 P=1   error = 0.02%
                //    BaudRate=19200   => O=9  N=43  P=3.5   error = 0.02%
                //    BaudRate=38400   => O=11 N=41  P=1.5   error = 0.09%
                //    BaudRate=56000   => O=16 N=29  P=1   error = 0.06%
                //    BaudRate=115200  => O=15 N=15  P=1   error = 0.31%
                //    BaudRate=128000  => O=14 N=1   P=14.5   error = 0.06%
                //    BaudRate=230400  => O=15 N=5   P=1.5   error = 0.31%
                //    BaudRate=345600  => O=15 N=5   P=1   error = 0.31%
                //    BaudRate=460800  => O=16 N=1   P=3.5   error = 0.76%

                switch(cfg.BaudRate)
                {
                    case   1200: OversampleRate = UnOVSR__12; Divisor = 157; Prescaler = UnPSR__11p5; break;
                    case   2400: OversampleRate = UnOVSR__7 ; Divisor = 619; Prescaler = UnPSR__2p5 ; break;
                    case   4800: OversampleRate = UnOVSR__8 ; Divisor = 677; Prescaler = UnPSR__1   ; break;
                    case   9600: OversampleRate = UnOVSR__14; Divisor = 129; Prescaler = UnPSR__1p5 ; break;
                    case  19200: OversampleRate = UnOVSR__9 ; Divisor = 43 ; Prescaler = UnPSR__3p5 ; break;
                    case  38400: OversampleRate = UnOVSR__11; Divisor = 41 ; Prescaler = UnPSR__1p5 ; break;
                    case  57600: OversampleRate = UnOVSR__16; Divisor = 29 ; Prescaler = UnPSR__1   ; break;
                    case 115200: OversampleRate = UnOVSR__15; Divisor = 15 ; Prescaler = UnPSR__1   ; break;
                    case 230400: OversampleRate = UnOVSR__15; Divisor = 5  ; Prescaler = UnPSR__1p5 ; break;
                    case 460800: OversampleRate = UnOVSR__16; Divisor = 1  ; Prescaler = UnPSR__3p5 ; break;

                    default:
                        return false;
                }
            }
            else if (Configuration.CoreClockFrequency == 30000000)
            {
                //BaudRate=300  => O=16 N=1250 P=5   error = 0.00%
                //BaudRate=600  => O=16 N=1250 P=2.5   error = 0.00%
                //*BaudRate=1200  => O=16 N=625 P=2.5   error = 0.00%
                //BaudRate=1800  => O=13 N=1282 P=1   error = 0.00%
                //BaudRate=2000  => O=16 N=625 P=1.5   error = 0.00%
                //*BaudRate=2400  => O=10 N=1250 P=1   error = 0.00%
                //BaudRate=3600  => O=13 N=641 P=1   error = 0.00%
                //*BaudRate=4800  => O=10 N=625 P=1   error = 0.00%
                //BaudRate=7200  => O=9 N=463 P=1   error = 0.01%
                //*BaudRate=9600  => O=10 N=125 P=2.5   error = 0.00%
                //BaudRate=14400  => O=7 N=119 P=2.5   error = 0.04%
                //*BaudRate=19200  => O=11 N=142 P=1   error = 0.03%
                //*BaudRate=38400  => O=11 N=71 P=1   error = 0.03%
                //*BaudRate=56000  => O=9 N=17 P=3.5   error = 0.04%
                //**BaudRate=57600  => O=16 N=13 P=2.5   error = 0.16%
                //*BaudRate=115200  => O=13 N=20 P=1   error = 0.16%
                //BaudRate=128000  => O=13 N=18 P=1   error = 0.16%
                //*BaudRate=230400  => O=13 N=10 P=1   error = 0.16%
                //BaudRate=345600  => O=7 N=5 P=2.5   error = 0.79%
                //*BaudRate=460800  => O=13 N=5 P=1   error = 0.16%


                switch (cfg.BaudRate)
                {
                    case 1200: OversampleRate = UnOVSR__16; Divisor = 625; Prescaler = UnPSR__2p5; break;
                    case 2400: OversampleRate = UnOVSR__10; Divisor = 1250; Prescaler = UnPSR__1; break;
                    case 4800: OversampleRate = UnOVSR__10; Divisor = 625; Prescaler = UnPSR__1; break;
                    case 9600: OversampleRate = UnOVSR__10; Divisor = 125; Prescaler = UnPSR__2p5; break;
                    case 19200: OversampleRate = UnOVSR__11; Divisor = 142; Prescaler = UnPSR__1; break;
                    case 38400: OversampleRate = UnOVSR__11; Divisor = 71; Prescaler = UnPSR__1; break;
                    //case 57600: OversampleRate = UnOVSR__9; Divisor = 17; Prescaler = UnPSR__3p5; break;
                    case 57600: OversampleRate = UnOVSR__16; Divisor = 13; Prescaler = UnPSR__2p5; break;
                    case 115200: OversampleRate = UnOVSR__13; Divisor = 20; Prescaler = UnPSR__1; break;
                    case 230400: OversampleRate = UnOVSR__13; Divisor = 10; Prescaler = UnPSR__1; break;
                    case 460800: OversampleRate = UnOVSR__13; Divisor = 5; Prescaler = UnPSR__1; break;

                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }

            //--//

            byte frs = 0;

            switch(cfg.DataBits)
            {
                case 7:
                    frs |= UnFRS__CHAR_7;
                    break;

                case 8:
                    frs |= UnFRS__CHAR_8;
                    break;

                default:
                    return false;
            }

            switch(cfg.StopBits)
            {
                case System.IO.Ports.StopBits.One:
                    frs |= UnFRS__STP_1;
                    break;

                case System.IO.Ports.StopBits.Two:
                    frs |= UnFRS__STP_2;
                    break;

                default:
                    return false;
            }

            if(cfg.Parity != System.IO.Ports.Parity.None)
            {
                frs |= UnFRS__PEN_ENABLED;

                switch(cfg.Parity)
                {
                    case System.IO.Ports.Parity.Even:
                        frs |= UnFRS__PSEL_EVEN;
                        break;

                    case System.IO.Ports.Parity.Odd:
                        frs |= UnFRS__PSEL_ODD;
                        break;

                    default:
                        return false;
                }
            }
            else
            {
                frs |= UnFRS__PEN_DISABLED;
            }

            GPIO gpio = GPIO.Instance;
    
            CMU.Instance.EnableClock( usartNum == 0 ? CMU.MCLK_EN__USART0 : CMU.MCLK_EN__USART1 );

            gpio.Control[usartNum == 0 ? SER1_TDX : SER2_TDX].Data = GPIO.CW.MODE_ALTA | GPIO.CW.RES_DIS;
            gpio.Control[usartNum == 0 ? SER1_RDX : SER2_RDX].Data = GPIO.CW.MODE_ALTA | GPIO.CW.RES_DIS;
    
            this.UnFRS   = frs;
            this.UnMDSL1 = 0;
            this.UnMDSL2 = 0;

            this.UnOVSR =        UnOVSR__set    ( OversampleRate );
            this.UnPSR  = (byte)(UnPSR__DIV__set( Divisor ) | UnPSR__PSC__set( Prescaler ));
            this.UnBAUD =        UnBAUD__set    ( Divisor );

            this.UnICTRL = (byte)((this.UnICTRL & UnICTRL__mask) | UnICTRL__ETI | UnICTRL__ERI);

            return true;
        }
    }
}