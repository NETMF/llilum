//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TargetPlatform.ARMv5_VFP.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal static class Convert
    {
        const float  c_float_32bit  = 65536.0f * 65536.0f;
        const double c_double_32bit = 65536.0  * 65536.0;

        [TS.WellKnownMethod( "SoftVFP_Convert_LongToFloat" )]
        internal static float LongToFloat( long val       ,
                                           bool fOverflow )
        {
            int  valHi =  (int)(val >> 32);
            uint valLo = (uint) val       ;

            float resHi = (float)valHi * c_float_32bit;
            float resLo = (float)valLo;

            return resHi + resLo;
        }

        [TS.WellKnownMethod( "SoftVFP_Convert_UnsignedLongToFloat" )]
        internal static float UnsignedLongToFloat( ulong val       ,
                                                   bool  fOverflow )
        {
            uint valHi = (uint)(val >> 32);
            uint valLo = (uint) val       ;

            float resHi = (float)valHi * c_float_32bit;
            float resLo = (float)valLo;

            return resHi + resLo;
        }

        //--//

        [TS.WellKnownMethod( "SoftVFP_Convert_LongToDouble" )]
        internal static double LongToDouble( long val       ,
                                             bool fOverflow )
        {
            int  valHi =  (int)(val >> 32);
            uint valLo = (uint) val       ;

            double resHi = (double)valHi * c_double_32bit;
            double resLo = (double)valLo;

            return resHi + resLo;
        }

        [TS.WellKnownMethod( "SoftVFP_Convert_UnsignedLongToDouble" )]
        internal static double UnsignedLongToDouble( ulong val       ,
                                                     bool  fOverflow )
        {
            uint valHi = (uint)(val >> 32);
            uint valLo = (uint) val       ;

            double resHi = (double)valHi * c_double_32bit;
            double resLo = (double)valLo;

            return resHi + resLo;
        }

        //--//

        [TS.WellKnownMethod( "SoftVFP_Convert_FloatToLong" )]
        internal static long FloatToLong( float val       ,
                                          bool  fOverflow )
        {
            int resHi = (int)(val         / c_float_32bit);
            int resLo = (int)(val - resHi * c_float_32bit);

            return ((long)resHi << 32) + (long)resLo;
        }

        [TS.WellKnownMethod( "SoftVFP_Convert_FloatToUnsignedLong" )]
        internal static ulong FloatToUnsignedLong( float val       ,
                                                   bool  fOverflow )
        {
            uint resHi = (uint)(val         / c_float_32bit);
            uint resLo = (uint)(val - resHi * c_float_32bit);

            return ((ulong)resHi << 32) + (ulong)resLo;
        }

        [TS.WellKnownMethod( "SoftVFP_Convert_DoubleToLong" )]
        internal static long DoubleToLong( double val       ,
                                           bool   fOverflow )
        {
            int resHi = (int)(val         / c_double_32bit);
            int resLo = (int)(val - resHi * c_double_32bit);

            return ((long)resHi << 32) + (long)resLo;
        }

        [TS.WellKnownMethod( "SoftVFP_Convert_DoubleToUnsignedLong" )]
        internal static ulong DoubleToUnsignedLong( double val       ,
                                                    bool   fOverflow )
        {
            uint resHi = (uint)(val         / c_double_32bit);
            uint resLo = (uint)(val - resHi * c_double_32bit);

            return ((ulong)resHi << 32) + (ulong)resLo;
        }
    }
}
