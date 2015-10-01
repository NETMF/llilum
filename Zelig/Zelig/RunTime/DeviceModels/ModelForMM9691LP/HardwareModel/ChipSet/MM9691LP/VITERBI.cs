//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x30030000U,Length=0x00008008U)]
    public class VITERBI
    {
////        //--//
////
////        CPU_MAC_INT16_ON_32    InputData[c_Input_Array_Elements];            // 0x0000      +0x0A00
////        /*************/ UINT32 Padding1 [                    64];            // 0x0A00      +0x0200
////
////        CPU_MAC_UINT16_ON_32   OutputData[c_Output_Array_Elements];          // 0x0C00      +0x0050
////        /*************/ UINT32 Padding2  [                    118];          // 0x0C50      +0x03B0
////
////        CPU_MAC_UINT16_ON_32   PathMetrics[c_PathMetrics_Array_Elements];    // 0x1000      +0x0800
////        /*************/ UINT32 Padding3   [                         256];    // 0x1800      +0x0800
////
////        CPU_MAC_UINT16_ON_32   TracebackArray[c_Traceback_Array_Elements];   // 0x2000      +0x5000
////        /*************/ UINT32 Padding4      [                      4608];   // 0x7000      +0x1000
////
////        /****/ volatile UINT32 RamControl;                                   // 0x8000      +0x0004
////        static const    UINT32 RamControl__CPU     = 0x00000000;
////        static const    UINT32 RamControl__VITERBI = 0x00000001;
////
////        /****/ volatile UINT32 Control;                                      // 0x8004      +0x0004
////        static const    UINT32 Control__DONE_ACS   = 0x00000001;
////        static const    UINT32 Control__DONE_TRC   = 0x00000002;
////        static const    UINT32 Control__IEN_ACS    = 0x00000004;
////        static const    UINT32 Control__IEN_TRC    = 0x00000008;
////        static const    UINT32 Control__DEN_ACS    = 0x00000010;
////        static const    UINT32 Control__DEN_TRC    = 0x00000020;
////        static const    UINT32 Control__GO_ACS     = 0x00000040;
////        static const    UINT32 Control__GO_TRC     = 0x00000080;
////        static const    UINT32 Control__GO_BOTH    = 0x00000100;
    }
}