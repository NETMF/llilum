//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x31000000U,Length=0x00000200U)]
    public class GPDMA
    {
        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0020U)]
        public class Channel
        {
            [BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct LLI_bitfield
            {
                [BitFieldRegister(Position=0)] public bool UseM1; // AHB master select for loading the next LLI:
                                                                  //  
                                                                  // 0 - AHB Master 0.
                                                                  // 1 - AHB Master 1.
            }

            //--//

            [BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct Control_bitfield
            {
                public enum Width
                {
                    Byte     = 0, //     000 - Byte (8-bit)
                    Halfword = 1, //     001 - Halfword (16-bit)
                    Word     = 2, //     010 - Word (32-bit)
                                  //     011 to 111 - Reserved
                }

                public enum BurstSize
                {
                    Len1   = 0, //     000 - 1
                    Len4   = 1, //     001 - 4
                    Len8   = 2, //     010 - 8
                    Len16  = 3, //     011 - 16
                    Len32  = 4, //     100 - 32 
                    Len64  = 5, //     101 - 64
                    Len128 = 6, //     110 - 128
                    Len256 = 7, //     111 - 256
                }


                [BitFieldRegister(Position=31       )] public bool      InterruptEnable;      // Terminal count interrupt enable bit.
                                                                                              //  
                                                                                              // 0 - the terminal count interrupt is disabled.
                                                                                              // 1 - the terminal count interrupt is enabled.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=30       )] public bool      Cacheable;            // Indicates that the access is cacheable or not cacheable:
                                                                                              //  
                                                                                              // 0 - access is not cacheable.
                                                                                              // 1 - access is cacheable.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=29       )] public bool      Bufferable;           // Indicates that the access is bufferable or not bufferable:
                                                                                              //  
                                                                                              // 0 - access is not bufferable.
                                                                                              // 1 - access is bufferable.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=28       )] public bool      PrivilegedMode;       // Indicates that the access is in user mode or privileged mode:
                                                                                              //  
                                                                                              // 0 - access is in user mode.
                                                                                              // 1 - access is in privileged mode.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=27       )] public bool      DestinationIncrement; // Destination increment:
                                                                                              //  
                                                                                              // 0 - the destination address is not incremented after each transfer.
                                                                                              // 1 - the destination address is incremented after each transfer.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=26       )] public bool      SourceIncrement;      // Source increment:
                                                                                              //  
                                                                                              // 0 - the source address is not incremented after each transfer.
                                                                                              // 1 - the source address is incremented after each transfer.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=25       )] public bool      DestinationUsesM1;    // Destination AHB master select:
                                                                                              //  
                                                                                              // 0 - AHB Master 0 selected for destination transfer.
                                                                                              // 1 - AHB Master 1 selected for destination transfer.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=24       )] public bool      SourceUsesM1;         // Source AHB master select:
                                                                                              //  
                                                                                              // 0 - AHB Master 0 selected for source transfer.
                                                                                              // 1 - AHB Master 1 selected for source transfer.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=21,Size=3)] public Width     DWidth;               // Destination transfer width.
                                                                                              // 
                                                                                              // Transfers wider than the AHB master bus width are illegal.
                                                                                              // The source and destination widths can be different from each other.
                                                                                              // The hardware automatically packs and unpacks the data as required.
                                                                                              //  
                                                                                              //  
                [BitFieldRegister(Position=18,Size=3)] public Width     SWidth;               // Source transfer width.
                                                                                              // 
                                                                                              // Transfers wider than the AHB master bus width are illegal.
                                                                                              // The source and destination widths can be different from each other.
                                                                                              // The hardware automatically packs and unpacks the data as required.
                                                                                              //  
                                                                                              //  
                [BitFieldRegister(Position=15,Size=3)] public BurstSize DBSize;               // Destination burst size.
                                                                                              // 
                                                                                              // Indicates the number of transfers that make up a destination burst request.
                                                                                              // This value must be set to the burst size of the destination peripheral, or if the destination is memory, to the memory boundary size.
                                                                                              // The burst size is the amount of data that is transferred when the DMACBREQ signal goes active in the destination peripheral.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position=12,Size=3)] public BurstSize SBSize;               // Source burst size.
                                                                                              // 
                                                                                              // Indicates the number of transfers that make up a source burst.
                                                                                              // This value must be set to the burst size of the source peripheral, or if the source is memory, to the memory boundary size.
                                                                                              // The burst size is the amount of data that is transferred when the DMACBREQ signal goes active in the source peripheral.
                                                                                              // 
                                                                                              // 
                [BitFieldRegister(Position= 0,Size=12)] public uint     TransferSize;         // Transfer size. 
                                                                                              // 
                                                                                              // A write to this field sets the size of the transfer when the DMA Controller is the flow controller.
                                                                                              // The transfer size value must be set before the channel is enabled. 
                                                                                              // Transfer size is updated as data transfers are completed. 
                                                                                              // A read from this field indicates the number of transfers completed on the destination bus. 
                                                                                              // Reading the register when the channel is active does not give useful information because by the time
                                                                                              // that the software has processed the value read, the channel might have progressed.
                                                                                              // It is intended to be used only when a channel is enabled and then disabled. 
                                                                                              // The transfer size value is not used if the DMA Controller is not the flow controller. 
                                                                                              //
            }

            //--//

            [BitFieldPeripheral(PhysicalType=typeof(uint))]
            public struct Config_bitfield
            {
                public enum FlowControl
                {
                    M2M_D  = 0, //     000 Memory to memory DMA
                    M2P_D  = 1, //     001 Memory to peripheral DMA
                    P2M_D  = 2, //     010 Peripheral to memory DMA
                    P2P_D  = 3, //     011 Source peripheral to destination peripheral DMA
                    P2P_DP = 4, //     100 Source peripheral to destination peripheral Destination peripheral
                    M2P_P  = 5, //     101 Memory to peripheral Peripheral
                    P2M_P  = 6, //     110 Peripheral to memory Peripheral
                    P2P_SP = 7, //     111 Source peripheral to destination peripheral Source peripheral
                }

                public enum Peripheral
                {
                    SPI1       = 11, // SPI1 receive and transmit
                    Uart7__Rx  = 10, // HS-Uart7 receive
                    Uart7__Tx  =  9, // HS-Uart7 transmit
                    Uart2__Rx  =  8, // HS-Uart2 receive
                    Uart2__Tx  =  7, // HS-Uart2 transmit
                    Uart1__Rx  =  6, // HS-Uart1 receive
                    Uart1__Tx  =  5, // HS-Uart1 transmit
                    SD_Card    =  4, // SD Card interface receive and transmit
                    SPI2       =  3, // SPI2 receive and transmit
                    NAND_Flash =  1, // NAND Flash (same as channel 12)
                }

                [BitFieldRegister(Position=18       )] public bool        H;              // Halt:
                                                                                          //  
                                                                                          //    0 = enable DMA requests.
                                                                                          //    1 = ignore further source DMA requests.
                                                                                          //  
                                                                                          // The contents of the channel FIFO are drained.
                                                                                          // This value can be used with the Active and Channel Enable bits to cleanly disable a DMA channel.
                                                                                          //  
                                                                                          //  
                [BitFieldRegister(Position=17       )] public bool        A;              // Active:
                                                                                          //  
                                                                                          //     0 = there is no data in the FIFO of the channel.
                                                                                          //     1 = the channel FIFO has data.
                                                                                          // 
                                                                                          // This value can be used with the Halt and Channel Enable bits to cleanly disable a DMA channel.
                                                                                          // This is a read-only bit. 
                                                                                          //  
                                                                                          //  
                [BitFieldRegister(Position=16       )] public bool        L;              // Lock.
                                                                                          //  
                                                                                          // When set, this bit enables locked transfers. 
                                                                                          //  
                                                                                          //  
                [BitFieldRegister(Position=15       )] public bool        ITC;            // Terminal count interrupt mask.
                                                                                          //  
                                                                                          // When cleared, this bit masks out the terminal count interrupt of the relevant channel.
                                                                                          //  
                                                                                          //  
                [BitFieldRegister(Position=14       )] public bool        IE;             // Interrupt error mask.
                                                                                          //  
                                                                                          // When cleared, this bit masks out the error interrupt of the relevant channel. 
                                                                                          //  
                                                                                          //  
                [BitFieldRegister(Position=11,Size=3)] public FlowControl FlowCntrl;      // Flow control and transfer type.
                                                                                          // 
                                                                                          // This value indicates the flow controller and transfer type.
                                                                                          // The flow controller can be the DMA Controller, the source peripheral, or the destination peripheral.
                                                                                          // The transfer type can be memory-to-memory, memory-to-peripheral, peripheral-to-memory, or peripheral-to-peripheral.
                                                                                          // 
                                                                                          // 
                [BitFieldRegister(Position= 6,Size=5)] public Peripheral  DestPeripheral; // Source peripheral.
                                                                                          // 
                                                                                          // This value selects the DMA source request peripheral. This field is ignored if the source of the transfer is from memory.
                                                                                          // 
                                                                                          // 
                [BitFieldRegister(Position= 1,Size=5)] public Peripheral  SrcPeripheral;  // Source peripheral.
                                                                                          // 
                                                                                          // This value selects the DMA source request peripheral. This field is ignored if the source of the transfer is from memory.
                                                                                          // 
                                                                                          // 
                [BitFieldRegister(Position= 0       )] public bool        E;              // Channel enable.
                                                                                          //  
                                                                                          // Reading this bit indicates whether a channel is currently enabled or disabled:
                                                                                          //  
                                                                                          //  0 = channel disabled.
                                                                                          //  1 = channel enabled.
                                                                                          //
                                                                                          // The Channel Enable bit status can also be found by reading the DMACEnbldChns Register.
                                                                                          // A channel is enabled by setting this bit.
                                                                                          // A channel can be disabled by clearing the Enable bit.
                                                                                          // This causes the current AHB transfer (if one is in progress) to complete and the channel is then disabled.
                                                                                          // Any data in the FIFO of the relevant channel is lost.
                                                                                          // Restarting the channel by setting the Channel Enable bit has unpredictable effects, the channel must be fully re-initialized.
                                                                                          // The channel is also disabled, and Channel Enable bit cleared, when the last LLI is reached, the DMA transfer is completed, or if a channel error is encountered.
                                                                                          // If a channel must be disabled without losing data in the FIFO, the Halt bit must be set so that further DMA requests are ignored.
                                                                                          // The Active bit must then be polled until it reaches 0, indicating that there is no data left in the FIFO.
                                                                                          // Finally, the Channel Enable bit can be cleared. 
                                                                                          // 
            }

            //--//

            [Register(Offset=0x00U)] public uint             SrcAddr;  // Source Address Register 0 R/W
            [Register(Offset=0x04U)] public uint             DestAddr; // Destination Address Register 0 R/W
            [Register(Offset=0x08U)] public LLI_bitfield     LLI;      // Linked List Item Register 0 R/W
            [Register(Offset=0x0CU)] public Control_bitfield Control;  // Control Register 0 R/W
            [Register(Offset=0x10U)] public Config_bitfield  Config;   // Configuration Register 0[1] R/W

            //
            // Helper Methods
            //

            [Inline]
            public void WaitForCompletion()
            {
                while(this.IsActive)
                {
                }
            }

            [Inline]
            public unsafe void CopyMemory( uint* src            ,
                                           uint* dst            ,
                                           uint  numOfWords     ,
                                           uint  burstSize      ,
                                           bool  fSrcIncrement  ,
                                           bool  fDstIncrement  ,
                                           bool  fUseSameMaster )
            {
#if USE_CPU
                Memory.CopyNonOverlapping( new UIntPtr( src ), new UIntPtr( &src[numOfWords] ), new UIntPtr( dst ) );
#else
                WaitForCompletion();

                //--//

                var ctrl = new Control_bitfield();

                ctrl.Cacheable      = false;
                ctrl.Bufferable     = false;
                ctrl.PrivilegedMode = true;
                ctrl.SWidth         = Control_bitfield.Width.Word;
                ctrl.DWidth         = Control_bitfield.Width.Word;

                if(fUseSameMaster)
                {
                    ctrl.SourceUsesM1      = false;
                    ctrl.DestinationUsesM1 = false;
                }
                else
                {
                    ctrl.SourceUsesM1      = false;
                    ctrl.DestinationUsesM1 = true;
                }

                ctrl.SourceIncrement      = fSrcIncrement;
                ctrl.DestinationIncrement = fDstIncrement;

                switch(burstSize)
                {
                    case 1:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len1;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len1;
                        break;

                    case 4:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len4;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len4;
                        break;

                    case 8:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len8;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len8;
                        break;

                    case 16:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len16;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len16;
                        break;

                    case 32:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len32;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len32;
                        break;

                    case 64:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len64;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len64;
                        break;

                    case 128:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len128;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len128;
                        break;

                    case 256:
                        ctrl.SBSize = Control_bitfield.BurstSize.Len256;
                        ctrl.DBSize = Control_bitfield.BurstSize.Len256;
                        break;
                }

                ctrl.TransferSize = numOfWords;

                //--//

                var config = new Config_bitfield();

                config.L         = true;
                config.FlowCntrl = Config_bitfield.FlowControl.M2M_D;
                config.E         = true;

                //--//

                this.SrcAddr  = (uint)src;
                this.DestAddr = (uint)dst;
                this.LLI      =       new LLI_bitfield();
                this.Control  =       ctrl;
                this.Config   =       config;
#endif
            }

            //
            // Access Methods
            //

            public bool IsActive
            {
                [Inline]
                get
                {
                    return this.Config.E;
                }
            }

            //
            // Debug Methods
            //

        }

        //--//

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct DMACConfig_bitfield
        {
                [BitFieldRegister(Position=2)] public bool M1_BigEndian; // AHB Master 1 endianness configuration:
                                                                         // 
                                                                         // 0 = little-endian mode (default).
                                                                         // 1 = big-endian mode.
                                                                         // 
                                                                         // 
                [BitFieldRegister(Position=1)] public bool M0_BigEndian; // AHB Master 0 endianness configuration:
                                                                         // 
                                                                         // 0 = little-endian mode (default).
                                                                         // 1 = big-endian mode.
                                                                         // 
                                                                         // 
                [BitFieldRegister(Position=0)] public bool E;            // DMA Controller enable:
                                                                         // 
                                                                         // 0 = disabled (default). Disabling the DMA Controller reduces power consumption.
                                                                         // 1 = enabled.
                                                                         // 
        }

        //--//

        [Register(Offset=0x00U)] public uint                DMACIntStat;       // DMA Interrupt Status Register 0 RO
        [Register(Offset=0x04U)] public uint                DMACIntTCStat;     // DMA Interrupt Terminal Count Request Status Register 0 RO
        [Register(Offset=0x08U)] public uint                DMACIntTCClear;    // DMA Interrupt Terminal Count Request Clear Register - WO
        [Register(Offset=0x0CU)] public uint                DMACIntErrStat;    // DMA Interrupt Error Status Register 0 RO
        [Register(Offset=0x10U)] public uint                DMACIntErrClr;     // DMA Interrupt Error Clear Register - WO
        [Register(Offset=0x14U)] public uint                DMACRawIntTCStat;  // DMA Raw Interrupt Terminal Count Status Register 0 RO
        [Register(Offset=0x18U)] public uint                DMACRawIntErrStat; // DMA Raw Error Interrupt Status Register 0 RO
        [Register(Offset=0x1CU)] public uint                DMACEnbldChns;     // DMA Enabled Channel Register 0 RO
                                            
        [Register(Offset=0x20U)] public uint                DMACSoftBReq;      // DMA Software Burst Request Register 0 R/W
        [Register(Offset=0x24U)] public uint                DMACSoftSReq;      // DMA Software Single Request Register 0 R/W
        [Register(Offset=0x28U)] public uint                DMACSoftLBReq;     // DMA Software Last Burst Request Register 0 R/W
        [Register(Offset=0x2CU)] public uint                DMACSoftLSReq;     // DMA Software Last Single Request Register 0 R/W

        [Register(Offset=0x30U)] public DMACConfig_bitfield DMACConfig;        // DMA Configuration Register 0 R/W
        [Register(Offset=0x34U)] public uint                DMACSync;          // DMA Synchronization Register 0 R/W

        [Register(Offset=0x00000100U,Instances=8)] public Channel[] Channels;

        //--//

        //
        // Helper Methods
        //

        [Inline]
        public void Enable()
        {
            SystemControl.Instance.DMACLK_CTRL.Enable = true;

            this.DMACConfig.E = true;
        }

        [Inline]
        public void Disable()
        {
            this.DMACConfig.E = false;

            SystemControl.Instance.DMACLK_CTRL.Enable = false;
        }

        //
        // Access Methods
        //

        public static extern GPDMA Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}