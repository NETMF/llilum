//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal static class Convert
    {
        [TS.WellKnownMethod( "SoftFP_Convert_IntToFloat" )]
        internal static float IntToFloat( int  val       ,
                                          bool fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val, fOverflow );

            return fi.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_LongToFloat" )]
        internal static float LongToFloat( long val       ,
                                           bool fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val, fOverflow );

            return fi.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_UnsignedIntToFloat" )]
        internal static float UnsignedIntToFloat( uint val       ,
                                                  bool fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val, fOverflow );

            return fi.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_UnsignedLongToFloat" )]
        internal static float UnsignedLongToFloat( ulong val       ,
                                                   bool  fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val, fOverflow );

            return fi.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_DoubleToFloat" )]
        internal static float DoubleToFloat( double val       ,
                                             bool   fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val );
            FloatImplementation  fi = new FloatImplementation ( ref di, fOverflow );

            return fi.ToFloat();
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_Convert_IntToDouble" )]
        internal static double IntToDouble( int  val       ,
                                            bool fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val, fOverflow );

            return di.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_LongToDouble" )]
        internal static double LongToDouble( long val       ,
                                             bool fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val, fOverflow );

            return di.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_UnsignedIntToDouble" )]
        internal static double UnsignedIntToDouble( uint val       ,
                                                    bool fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val, fOverflow );

            return di.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_UnsignedLongToDouble" )]
        internal static double UnsignedLongToDouble( ulong val       ,
                                                     bool  fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val, fOverflow );

            return di.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_Convert_FloatToDouble" )]
        internal static double FloatToDouble( float val       ,
                                              bool  fOverflow )
        {
            FloatImplementation  fi = new FloatImplementation ( val );
            DoubleImplementation di = new DoubleImplementation( ref fi, fOverflow );

            return di.ToDouble();
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_Convert_FloatToInt" )]
        internal static int FloatToInt( float val       ,
                                        bool  fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val );

            return fi.ToInt( fOverflow );
        }

        [TS.WellKnownMethod( "SoftFP_Convert_FloatToUnsignedInt" )]
        internal static uint FloatToUnsignedInt( float val       ,
                                                 bool  fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val );

            return fi.ToUnsignedInt( fOverflow );
        }

        [TS.WellKnownMethod( "SoftFP_Convert_DoubleToInt" )]
        internal static int DoubleToInt( double val       ,
                                         bool   fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val );

            return di.ToInt( fOverflow );
        }

        [TS.WellKnownMethod( "SoftFP_Convert_DoubleToUnsignedInt" )]
        internal static uint DoubleToUnsignedInt( double val       ,
                                                  bool   fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val );

            return di.ToUnsignedInt( fOverflow );
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_Convert_FloatToLong" )]
        internal static long FloatToLong( float val       ,
                                          bool  fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val );

            return fi.ToLong( fOverflow );
        }

        [TS.WellKnownMethod( "SoftFP_Convert_FloatToUnsignedLong" )]
        internal static ulong FloatToUnsignedLong( float val       ,
                                                   bool  fOverflow )
        {
            FloatImplementation fi = new FloatImplementation( val );

            return fi.ToUnsignedLong( fOverflow );
        }

        [TS.WellKnownMethod( "SoftFP_Convert_DoubleToLong" )]
        internal static long DoubleToLong( double val       ,
                                           bool   fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val );

            return di.ToLong( fOverflow );
        }

        [TS.WellKnownMethod( "SoftFP_Convert_DoubleToUnsignedLong" )]
        internal static ulong DoubleToUnsignedLong( double val       ,
                                                    bool   fOverflow )
        {
            DoubleImplementation di = new DoubleImplementation( val );

            return di.ToUnsignedLong( fOverflow );
        }
    }
}
