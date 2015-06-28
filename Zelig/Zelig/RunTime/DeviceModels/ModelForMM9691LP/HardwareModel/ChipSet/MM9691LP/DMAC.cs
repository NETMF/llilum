//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x30020000U,Length=0x00000114U)]
    public class DMAC
    {
        //
        //    static const UINT32 c_DMAC_ENABLE             0x00000001
        //    static const UINT32 c_DMAC_ERROR              0x00000002         
        //    static const UINT32 c_DMAC_DONE               0x00000004
        //    static const UINT32 c_DMAC_HALFDONE           0x00000008
        //
        //    static const UINT32 c_DMAC_SRC_INCR_UNCH      0x00000000
        //    static const UINT32 c_DMAC_SRC_INCR_1         0x00000010
        //    static const UINT32 c_DMAC_SRC_INCR_2         0x00000020
        //    static const UINT32 c_DMAC_SRC_INCR_4         0x00000030
        //
        //    static const UINT32 c_DMAC_SRC_XFER_SZ_1      0x00000000
        //    static const UINT32 c_DMAC_SRC_XFER_SZ_2      0x00000040
        //    static const UINT32 c_DMAC_SRC_XFER_SZ_4      0x00000080
        //    static const UINT32 c_DMAC_SRC_XFER_SZ_RES    0x000000C0
        //
        //    static const UINT32 c_DMAC_SRC_BURST_SING     0x00000000
        //    static const UINT32 c_DMAC_SRC_BURST_I2       0x00000100
        //    static const UINT32 c_DMAC_SRC_BURST_W4       0x00000200
        //    static const UINT32 c_DMAC_SRC_BURST_I4       0x00000300
        //    static const UINT32 c_DMAC_SRC_BURST_W8       0x00000400
        //    static const UINT32 c_DMAC_SRC_BURST_I8       0x00000500
        //    static const UINT32 c_DMAC_SRC_BURST_W16      0x00000600
        //    static const UINT32 c_DMAC_SRC_BURST_I16      0x00000700
        //
        //    static const UINT32 c_DMAC_DMA_MODE_NORM      0x00000000
        //    static const UINT32 c_DMAC_DMA_MODE_CONT      0x00000800
        //    static const UINT32 c_DMAC_DMA_MODE_LINK      0x00001000
        //    static const UINT32 c_DMAC_DMA_MODE_RES       0x00001800
        //
        //    static const UINT32 c_DMAC_SRC_BPR_1          0x00000000
        //    static const UINT32 c_DMAC_SRC_BPR_2          0x00002000
        //    static const UINT32 c_DMAC_SRC_BPR_4          0x00004000
        //    static const UINT32 c_DMAC_SRC_BPR_8          0x00006000
        //    static const UINT32 c_DMAC_SRC_BPR_16         0x00008000
        //    static const UINT32 c_DMAC_SRC_BPR_32         0x0000A000
        //    static const UINT32 c_DMAC_SRC_BPR_64         0x0000C000
        //    static const UINT32 c_DMAC_SRC_BPR_128        0x0000E000
        //
        //    static const UINT32 c_DMAC_DEST_INC_UNCH      0x00000000
        //    static const UINT32 c_DMAC_DEST_INC_1         0x00010000
        //    static const UINT32 c_DMAC_DEST_INC_2         0x00020000
        //    static const UINT32 c_DMAC_DEST_INC_4         0x00030000
        //
        //    static const UINT32 c_DMAC_DEST_XFER_SZ_1     0x00000000
        //    static const UINT32 c_DMAC_DEST_XFER_SZ_2     0x00040000
        //    static const UINT32 c_DMAC_DEST_XFER_SZ_4     0x00080000
        //    static const UINT32 c_DMAC_DEST_XFER_RES      0x000C0000
        //
        //    static const UINT32 c_DMAC_DEST_BURST_SING    0x00000000
        //    static const UINT32 c_DMAC_DEST_BURST_I2      0x00100000
        //    static const UINT32 c_DMAC_DEST_BURST_W4      0x00200000
        //    static const UINT32 c_DMAC_DEST_BURST_I4      0x00300000
        //    static const UINT32 c_DMAC_DEST_BURST_W8      0x00400000
        //    static const UINT32 c_DMAC_DEST_BURST_I8      0x00500000
        //    static const UINT32 c_DMAC_DEST_BURST_W16     0x00600000
        //    static const UINT32 c_DMAC_DEST_BURST_I16     0x00700000
        //
        //    static const UINT32 c_DMAC_DONE_2_SRC         0x00000000
        //    static const UINT32 c_DMAC_DONE_2_DEST        0x00800000
        //
        //    static const UINT32 c_DMAC_DEV_SEL_MASK       0x07000000
        //    static const UINT32 c_DMAC_DEV_SEL_SW         0x00000000
        //    static const UINT32 c_DMAC_DEV_SEL_USB        0x01000000
        //    static const UINT32 c_DMAC_DEV_SEL_MW_TX      0x02000000
        //    static const UINT32 c_DMAC_DEV_SEL_MW_RX      0x03000000
        //    static const UINT32 c_DMAC_DEV_SEL_SER1_TRX   0x04000000 // CHAN 7-4=RX CHAN 3-0=TX
        //    static const UINT32 c_DMAC_DEV_SEL_SER2_TRX   0x05000000 // CHAN 7-4=RX CHAN 3-0=TX
        //    static const UINT32 c_DMAC_DEV_SEL_VITERBI    0x06000000
        //    static const UINT32 c_DMAC_DEV_SEL_ACRTAN     0x07000000
        //
        //    static const UINT32 c_DMAC_IDL_CYCLE_INDEX    27
        //    static const UINT32 c_DMAC_IDL_CYCLE_MASK     0xf8000000
        //

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0010U)]
        public class CHANNEL
        {
            [Register(Offset=0x00000000U)] public uint SourceStartAddress;
            [Register(Offset=0x00000004U)] public uint DestinationStartAddress;
            [Register(Offset=0x00000008U)] public uint SourceBeat;
            [Register(Offset=0x0000000CU)] public uint ControlWord;
        }

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x0010U)]
        public class CHANNEL_STATE
        {
            [Register(Offset=0x00000000U)] public uint SourceCurrentAddress;       // read only
            [Register(Offset=0x00000004U)] public uint DestinationCurrentAddress;  // read only
            [Register(Offset=0x00000008U)] public uint SourceCurrentBeatCount;     // read only
        }

        //--//

        [Register(Offset=0x00000000U,Size=0x10U,Instances=8)] public CHANNEL[]       Channel;
        [Register(Offset=0x00000080U,Size=0x10U,Instances=8)] public CHANNEL_STATE[] ChannelState;
        [Register(Offset=0x00000100U                       )] public uint            InterruptStatus;
        [Register(Offset=0x00000104U                       )] public uint            InterruptRawStatus;
        [Register(Offset=0x00000108U                       )] public uint            InterruptEnableSet;
        [Register(Offset=0x0000010CU                       )] public uint            InterruptEnableClear;
        [Register(Offset=0x00000110U                       )] public uint            InternalDMACTestMode;
        [Register(Offset=0x00000114U                       )] public uint            InternalTestRequest;

        //
        // Access Methods
        //

        public static extern DMAC Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}