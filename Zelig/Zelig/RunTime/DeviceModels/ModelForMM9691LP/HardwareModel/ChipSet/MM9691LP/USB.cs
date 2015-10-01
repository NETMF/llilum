//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38060000U,Length=0x00000100U)]
    public class USB
    {
////        struct ENDPNT
////        {
////            /****/ volatile UINT8 EPCT;
////            // for EP0
////            static const    UINT8 EPCT__EPC0_MASK               = 0xDF;
////            static const    UINT8 EPCT__EPC0_EP                 = 0x0F;
////            static const    UINT8 USB_SETUP_FIX_DIS             = 0x10;
////
////            static const    UINT8 EPCT__EPC0_DEF                = 0x40;
////            static const    UINT8 EPCT__EPC0_STALL              = 0x80;
////            // for EP1-3
////            static const    UINT8 EPCT__EPC_MASK                = 0xBF;
////            static const    UINT8 EPCT__EPC_EP                  = 0x0F;
////            static const    UINT8 EPCT__EPC_EP_EN               = 0x10;
////            static const    UINT8 EPCT__EPC_ISO                 = 0x20;
////            static const    UINT8 EPCT__EPC_STALL               = 0x80;
////            /*************/ UINT8 padding1[3];
////
////            /****/ volatile UINT8 TXD;
////            static const    UINT8 TXD__mask                     = 0xFF;
////            /*************/ UINT8 padding2[3];
////
////            /****/ volatile UINT8 TXS;
////            // for EP0
////            static const    UINT8 TXS__TXS0_MASK                = 0x6F;
////            static const    UINT8 TXS__TXS0_TCOUNT              = 0x0F;
////            static const    UINT8 TXS__TXS0_TX_DONE             = 0x20;
////            static const    UINT8 TXS__TXS0_ACK_STAT            = 0x40;
////            // for EP1-3
////            static const    UINT8 TXS__TXS_MASK                 = 0xFF;
////            static const    UINT8 TXS__TXS_TCOUNT               = 0x1F;
////            static const    UINT8 TXS__TXS_TX_DONE              = 0x20;
////            static const    UINT8 TXS__TXS_ACK_STAT             = 0x40;
////            static const    UINT8 TXS__TXS_TX_URUN              = 0x80;
////            /*************/ UINT8 padding3[3];
////
////            /****/ volatile UINT8 TXC;
////            // for EP0
////            static const    UINT8 TXC__TXC0_MASK                = 0x14;
////            static const    UINT8 TXC__TXC0_TX_EN               = 0x01;
////            static const    UINT8 TXC__TXC0_TOGGLE              = 0x04;
////            static const    UINT8 TXC__TXC0_FLUSH               = 0x08;
////            static const    UINT8 TXC__TXC0_IGN_IN              = 0x10;
////            // for EP1-3
////            static const    UINT8 TXC__TXC_MASK                 = 0xE4;
////            static const    UINT8 TXC__TXC_TX_EN                = 0x01;
////            static const    UINT8 TXC__TXC_LAST                 = 0x02;
////            static const    UINT8 TXC__TXC_TOGGLE               = 0x04;
////            static const    UINT8 TXC__TXC_FLUSH                = 0x08;
////            static const    UINT8 TXC__TXC_RFF                  = 0x10;
////            static const    UINT8 TXC__TXC_TFWL                 = 0x60;
////            static const    UINT8 TXC__TXC_IGN_ISOMSK           = 0x80;
////            /*************/ UINT8 padding4[3];
////
////            /****/ volatile UINT8 EPCR; // EPCR use EPCT macro
////            /*************/ UINT8 padding5[3];
////
////            /****/ volatile UINT8 RXD;
////            static const    UINT8 RXD__mask                     = 0xFF;
////            /*************/ UINT8 padding6[3];
////
////            /****/ volatile UINT8 RXS;
////            // for EP0
////            static const    UINT8 RXS__RXS0_MASK                = 0x7F;
////            static const    UINT8 RXS__RXS0_RCOUNT              = 0x0F;
////            static const    UINT8 RXS__RXS0_RX_LAST             = 0x10;
////            static const    UINT8 RXS__RXS0_TOGGLE              = 0x20;
////            static const    UINT8 RXS__RXS0_SETUP               = 0x40;
////            // for EP1-3
////            static const    UINT8 RXS__RXS_MASK                 = 0xFF;
////            static const    UINT8 RXS__RXS_RCOUNT               = 0x0F;
////            static const    UINT8 RXS__RXS_RX_LAST              = 0x10;
////            static const    UINT8 RXS__RXS_TOGGLE               = 0x20;
////            static const    UINT8 RXS__RXS_SETUP                = 0x40;
////            static const    UINT8 RXS__RXS_RX_ERR               = 0x80;
////            /*************/ UINT8 padding7[3];
////
////            /****/ volatile UINT8 RXC;
////            // for EP0
////            static const    UINT8 RXC__RXC0_MASK                = 0x06;
////            static const    UINT8 RXC__RXC0_RX_EN               = 0x01;
////            static const    UINT8 RXC__RXC0_IGN_OUT             = 0x02;
////            static const    UINT8 RXC__RXC0_IGN_SETUP           = 0x04;
////            static const    UINT8 RXC__RXC0_FLUSH               = 0x08;
////            // for EP1-3
////            static const    UINT8 RXC__RXC_MASK                 = 0x64;
////            static const    UINT8 RXC__RXC_RX_EN                = 0x01;
////            static const    UINT8 RXC__RXC_IGN_SETUP            = 0x04;
////            static const    UINT8 RXC__RXC_FLUSH                = 0x08;
////            static const    UINT8 RXC__RXC_RFWL                 = 0x60;
////            /*************/ UINT8 padding8[3];
////        };
////
////        //--//
////
////        static const UINT32 c_Base = 0xB8060000;
////
////        //--//
////
////        /****/ volatile UINT8  MCNTRL;
////        static const    UINT8  MCNTRL__MASK             = 0x09;
////        static const    UINT8  MCNTRL__USBEN            = 0x01;
////        static const    UINT8  MCNTRL__DBG              = 0x02;
////        static const    UINT8  MCNTRL__NAT              = 0x08;
////        /*************/ UINT8  Padding1[3];
////
////        /****/ volatile UINT8  XCVRDIAG;
////        /*************/ UINT8  Padding2[3];
////
////        /****/ volatile UINT8  TCR;
////        /*************/ UINT8  Padding3[3];
////
////        /****/ volatile UINT8  UTR;
////        /*************/ UINT8  Padding4[3];
////
////        /****/ volatile UINT8  FAR_;
////        static const    UINT8  FAR__FAR_MASK            = 0xFF;
////        static const    UINT8  FAR__FAR_AD              = 0x7F;
////        static const    UINT8  FAR__FAR_AD_EN           = 0x80;
////        /*************/ UINT8  Padding5[3];
////
////        /****/ volatile UINT8  NFSR;
////        static const    UINT8  NFSR__STATE_NODE_MASK         = 0x03;
////        static const    UINT8  NFSR__STATE_NODE_RESET        = 0x00;
////        static const    UINT8  NFSR__STATE_NODE_RESUME       = 0x01;
////        static const    UINT8  NFSR__STATE_NODE_OPERATIONAL  = 0x02;
////        static const    UINT8  NFSR__STATE_NODE_SUSPEND      = 0x03;
////        /*************/ UINT8  Padding6[3];
////
////        /****/ volatile UINT8  MAEV;
////        static const    UINT8  MAEV__MASK                    = 0xFF;
////        static const    UINT8  MAEV__WARN                    = 0x01;
////        static const    UINT8  MAEV__ALT                     = 0x02;
////        static const    UINT8  MAEV__TX_EV                   = 0x04;
////        static const    UINT8  MAEV__FRAME                   = 0x08;
////        static const    UINT8  MAEV__NAK                     = 0x10;
////        static const    UINT8  MAEV__ULD                     = 0x20;
////        static const    UINT8  MAEV__RX_EV                   = 0x40;
////        static const    UINT8  MAEV__INTR                    = 0x80;
////        /*************/ UINT8  Padding7[3];
////
////        /****/ volatile UINT8  MAMSK;
////        static const    UINT8  MAMSK__MASK                   = 0xFF;
////        static const    UINT8  MAMSK__WARN                   = 0x01;
////        static const    UINT8  MAMSK__ALT                    = 0x02;
////        static const    UINT8  MAMSK__TX_EV                  = 0x04;
////        static const    UINT8  MAMSK__FRAME                  = 0x08;
////        static const    UINT8  MAMSK__NAK                    = 0x10;
////        static const    UINT8  MAMSK__ULD                    = 0x20;
////        static const    UINT8  MAMSK__RX_EV                  = 0x40;
////        static const    UINT8  MAMSK__INTR                   = 0x80;
////        /*************/ UINT8  Padding8[3];
////
////        /****/ volatile UINT8  ALTEV;
////        static const    UINT8  ALTEV__MASK                   = 0xFC;
////        static const    UINT8  ALTEV__DMA                    = 0x04;
////        static const    UINT8  ALTEV__EOP                    = 0x08;
////        static const    UINT8  ALTEV__SD3                    = 0x10;
////        static const    UINT8  ALTEV__SD5                    = 0x20;
////        static const    UINT8  ALTEV__RESET                  = 0x40;
////        static const    UINT8  ALTEV__RESUME                 = 0x80;
////        /*************/ UINT8  Padding9[3];
////
////        /****/ volatile UINT8  ALTMSK;
////        static const    UINT8  ALTMSK__MASK                  = 0xFC;
////        static const    UINT8  ALTMSK__DMA                   = 0x04;
////        static const    UINT8  ALTMSK__EOP                   = 0x08;
////        static const    UINT8  ALTMSK__SD3                   = 0x10;
////        static const    UINT8  ALTMSK__SD5                   = 0x20;
////        static const    UINT8  ALTMSK__RESET                 = 0x40;
////        static const    UINT8  ALTMSK__RESUME                = 0x80;
////        /*************/ UINT8  Padding10[3];
////
////        /****/ volatile UINT8  TXEV;
////        static const    UINT8  TXEV__MASK                    = 0xFF;
////        static const    UINT8  TXEV__FIFO_ALL                = 0x0F;
////        static const    UINT8  TXEV__FIFO_EP0                = 0x01;
////        static const    UINT8  TXEV__UNDERRUN_ALL            = 0xF0;
////        /*************/ UINT8  Padding11[3];
////
////        __inline static UINT8  TXEV__FIFO__set    ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  TXEV__UNDERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  TXMSK;
////        static const    UINT8  TXMSK__MASK                   = 0xFF;
////        static const    UINT8  TXMSK__FIFO_ALL               = 0x0F;
////        static const    UINT8  TXMSK__FIFO_EP0               = 0x01;
////        static const    UINT8  TXMSK__UNDERRUN_ALL           = 0xF0;
////        /*************/ UINT8  Padding12[3];
////
////        __inline static UINT8  TXMSK__FIFO__set    ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  TXMSK__UNDERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  RXEV;
////        static const    UINT8  RXEV__MASK                    = 0xFF;
////        static const    UINT8  RXEV__FIFO_ALL                = 0x0F;
////        static const    UINT8  RXEV__FIFO_EP0                = 0x01;
////        static const    UINT8  RXEV__OVERRUN_ALL             = 0xF0;
////        /*************/ UINT8  Padding13[3];
////
////        __inline static UINT8  RXEV__FIFO__set   ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  RXEV__OVERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  RXMSK;
////        static const    UINT8  RXMSK__MASK                   = 0xFF;
////        static const    UINT8  RXMSK__FIFO_ALL               = 0x0F;
////        static const    UINT8  RXMSK__FIFO_EP0               = 0x01;
////        static const    UINT8  RXMSK__OVERRUN_ALL            = 0xF0;
////        /*************/ UINT8  Padding14[3];
////
////        __inline static UINT8  RXMSK__FIFO__set   ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  RXMSK__OVERRUN__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  NAKEV;
////        static const    UINT8  NAKEV__MASK                   = 0xFF;
////        static const    UINT8  NAKEV__IN_ALL                 = 0x0F;
////        static const    UINT8  NAKEV__OUT_ALL                = 0xF0;
////        /*************/ UINT8  Padding15[3];
////
////        __inline static UINT8  NAKEV__IN__set ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  NAKEV__OUT__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  NAKMSK;
////        static const    UINT8  NAKMSK__MASK                  = 0xFF;
////        static const    UINT8  NAKMSK__IN_ALL                = 0x0F;
////        static const    UINT8  NAKMSK__OUT_ALL               = 0xF0;
////        /*************/ UINT8  Padding16[3];
////
////        __inline static UINT8  NAKMSK__IN__set ( UINT32 n ) { return (0x01 << n); }
////        __inline static UINT8  NAKMSK__OUT__set( UINT32 n ) { return (0x10 << n); }
////
////        /****/ volatile UINT8  FWEV;
////        static const    UINT8  FWEV__MASK                    = 0xEE;
////        static const    UINT8  FWEV__TXWARN                  = 0x0E;
////        static const    UINT8  FWEV__RXWARN                  = 0xE0;
////        /*************/ UINT8  Padding17[3];
////
////        /****/ volatile UINT8  FWMSK;
////        static const    UINT8  FWMSK__MASK                   = 0xEE;
////        static const    UINT8  FWMSK__TXWARN                 = 0x0E;
////        static const    UINT8  FWMSK__RXWARN                 = 0xE0;
////        /*************/ UINT8  Padding18[3];
////
////        /****/ volatile UINT8  FNH;
////        static const    UINT8  FNH__MASK                     = 0xE7;
////        static const    UINT8  FNH__FN                       = 0x07;
////        static const    UINT8  FNH__RFC                      = 0x20;
////        static const    UINT8  FNH__UL                       = 0x40;
////        static const    UINT8  FNH__MF                       = 0x80;
////        /*************/ UINT8  Padding19[3];
////
////        /****/ volatile UINT8  FNL;
////        static const    UINT8  FNL__MASK                     = 0xFF;
////        static const    UINT8  FNL__FN                       = 0xFF;
////        /*************/ UINT8  Padding20[3];
////
////        /****/ volatile UINT8  DMACNTRL;
////        static const    UINT8  DMACNTRL__MASK                = 0xFF;
////        static const    UINT8  DMACNTRL__DSRC                = 0x07;
////        static const    UINT8  DMACNTRL__DMOD                = 0x08;
////        static const    UINT8  DMACNTRL__ADMA                = 0x10;
////        static const    UINT8  DMACNTRL__DTGL                = 0x20;
////        static const    UINT8  DMACNTRL__IGNRXTGL            = 0x40;
////        static const    UINT8  DMACNTRL__DEN                 = 0x80;
////        /*************/ UINT8  Padding21[3];
////
////        /****/ volatile UINT8  DMAEV;
////        static const    UINT8  DMAEV__MASK                   = 0x3F;
////        static const    UINT8  DMAEV__DSHLT                  = 0x01;
////        static const    UINT8  DMAEV__DERR                   = 0x02;
////        static const    UINT8  DMAEV__DCNT                   = 0x04;
////        static const    UINT8  DMAEV__DSIZ                   = 0x08;
////        static const    UINT8  DMAEV__NTGL                   = 0x20;
////        static const    UINT8  USB_DMAEV_ARDY                = 0x10;
////        /*************/ UINT8  Padding22[3];
////
////        /****/ volatile UINT8  DMAMSK;
////        static const    UINT8  DMAMSK__MASK                  = 0x2F;
////        static const    UINT8  DMAMSK__DSHLT                 = 0x01;
////        static const    UINT8  DMAMSK__DERR                  = 0x02;
////        static const    UINT8  DMAMSK__DCNT                  = 0x04;
////        static const    UINT8  DMAMSK__DSIZ                  = 0x08;
////        static const    UINT8  DMAMSK__NTGL                  = 0x20;
////        /*************/ UINT8  Padding23[3];
////
////        /****/ volatile UINT8  MIR;
////        static const    UINT8  MIR__MASK                     = 0xFF;
////        static const    UINT8  MIR__STAT                     = 0xFF;
////        /*************/ UINT8  Padding24[3];
////
////        /****/ volatile UINT8  DMACNT;
////        static const    UINT8  DMACNT__MASK                  = 0xFF;
////        static const    UINT8  DMACNT__DCOUNT                = 0xFF;
////        /*************/ UINT8  Padding25[3];
////
////        /****/ volatile UINT8  DMAERR;
////        static const    UINT8  DMAERR__MASK                  = 0xFF;
////        static const    UINT8  DMAERR__DMAERRCNT             = 0x7F;
////        static const    UINT8  DMAERR__AEH                   = 0x80;
////        /*************/ UINT8  Padding26[3];
////
////        /****/ volatile UINT8  WAKEUP;
////        /*************/ UINT8  Padding27[3];
////
////        /*************/ UINT32 Padding28[5];
////
////        /*************/ ENDPNT EP[4];
    }
}