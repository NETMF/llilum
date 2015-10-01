//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x30040000U,Length=0x00008020U)]
    public class FILTERARCTAN
    {
////        static const UINT32 c_Base = 0xB0040000;
////
////        static const UINT32 c_Arctan_Block_Size          = 128;
////        static const UINT32 c_Filter_Coefficient_Size    = 512;
////        static const UINT32 c_Filter_Input_Size          = 2048;
////        static const UINT32 c_Filter_Output_Size         = 1024;
////
////        static const UINT32 c_Arctan_Max                 = c_Arctan_Block_Size;
////        static const UINT32 c_Complex_Max                = c_Filter_Coefficient_Size / 2;
////        static const UINT32 c_Vector_Max                 = c_Filter_Coefficient_Size;
////        static const UINT32 c_Filter_Max                 = c_Filter_Output_Size;
////        static const UINT32 c_Filter_Coefficient_Max     = 31;
////
////        //--//
////
////        CPU_MAC_IQPAIR_ON_64   Arctan_InputData   [c_Arctan_Block_Size      ];
////        CPU_MAC_ANGLEMAG_ON_64 Arctan_OutputData  [c_Arctan_Block_Size      ];
////        CPU_MAC_INT16_ON_32    Filter_Coefficients[c_Filter_Coefficient_Size];
////        CPU_MAC_INT16_ON_32    Filter_InputData   [c_Filter_Input_Size      ];
////        CPU_MAC_INT32_ON_64    Filter_OutputData  [c_Filter_Output_Size     ];
////        /*************/ UINT32 Padding1           [                     5632];
////
////        /****/ volatile UINT32 RPT_CNT;
////        static const    UINT32 RPT_CNT__mask             = 0x0000007F;
////
////        /****/ volatile UINT32 reserved;
////
////        /****/ volatile UINT32 NCOEF;
////        static const    UINT32 NCOEF__mask               = 0x000001FF;
////        static const    UINT32 NCOEF__shift              =          0;
////        static const    UINT32 NCOEF__CMPLX              = 0x00000200;
////        static const    UINT32 NCOEF__LOW_N              = 0x00000400;
////
////        __inline static UINT32 NCOEF__set( UINT32 w ) { return (w << NCOEF__shift) & NCOEF__mask; }
////
////        /****/ volatile UINT32 NOUT;
////        static const    UINT32 NOUT__mask                = 0x000007FF;
////        static const    UINT32 NOUT__shift               =          0;
////
////        __inline static UINT32 NOUT__set( UINT32 w ) { return (w << NOUT__shift) & NOUT__mask; }
////
////        /****/ volatile UINT32 IN_PTR;
////        static const    UINT32 IN_PTR__mask              = 0x000007FF;
////        static const    UINT32 IN_PTR__shift             =          0;
////
////        __inline static UINT32 IN_PTR__set( UINT32 w ) { return (w << IN_PTR__shift) & IN_PTR__mask; }
////
////        /****/ volatile UINT32 OUT_PTR;
////        static const    UINT32 OUT_PTR__mask             = 0x000007FF;
////        static const    UINT32 OUT_PTR__shift            =          0;
////
////        __inline static UINT32 OUT_PTR__set( UINT32 w ) { return (w << OUT_PTR__shift) & OUT_PTR__mask; }
////
////        /****/ volatile UINT32 RAM_CONT;
////        static const    UINT32 RAM_CONT__CPU             = 0x00000000;
////        static const    UINT32 RAM_CONT__MAC             = 0x00000001;
////
////        /****/ volatile UINT32 CONT;
////        static const    UINT32 CONT__DONE_ATN      = 0x00000001;
////        static const    UINT32 CONT__DONE_FILT     = 0x00000002;
////        static const    UINT32 CONT__IEN_ATN       = 0x00000004;
////        static const    UINT32 CONT__IEN_FILT      = 0x00000008;
////        static const    UINT32 CONT__DEN_ATN       = 0x00000010;
////        static const    UINT32 CONT__DEN_FILT      = 0x00000020;
////        static const    UINT32 CONT__GO_ATN        = 0x00000040;
////        static const    UINT32 CONT__GO_FILT       = 0x00000080;
    }
}