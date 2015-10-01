//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    internal static class BinaryOperations
    {
        [TS.WellKnownMethod( "Helpers_BinaryOperations_IntDiv" )]
        internal static int IntDiv( int left  ,
                                    int right )
        {
            bool fReverse;

            if(left < 0)
            {
                left = unchecked(-left);

                fReverse = true;
            }
            else
            {
                fReverse = false;
            }

            if(right < 0)
            {
                right = unchecked(-right);

                fReverse = !fReverse;
            }

            int res = (int)((uint)left / (uint)right);

            if(fReverse)
            {
                res = -res;
            }

            return res;
        }

        [TS.WellKnownMethod( "Helpers_BinaryOperations_IntRem" )]
        internal static int IntRem( int left  ,
                                    int right )
        {
            return left - (left / right) * right;
        }

        //--//

        [TS.WellKnownMethod( "Helpers_BinaryOperations_UintDiv" )]
        internal static uint UintDiv( uint dividend ,
                                      uint divisor  )
        {
            if(divisor == 0)
            {
                throw new DivideByZeroException();
            }

            uint res   = 0;
            int  shift = 0;

            while(dividend > divisor)
            {
                if(MathImpl.IsNegative( divisor ))
                {
                    break;
                }

                divisor *= 2;
                shift++;
            }

            while(shift >= 0)
            {
                res *= 2;

                if(dividend >= divisor)
                {
                    dividend -= divisor;
                    res      += 1;

                    if(dividend == 0)
                    {
                        res <<= shift;
                        break;
                    }
                }

                divisor /= 2;
                shift--;
            }

            return res;
        }

        [TS.WellKnownMethod( "Helpers_BinaryOperations_UintRem" )]
        internal static uint UintRem( uint dividend ,
                                      uint divisor  )
        {
            if(divisor == 0)
            {
                throw new DivideByZeroException();
            }

            int shift = 0;

            while(dividend > divisor)
            {
                if(MathImpl.IsNegative( divisor ))
                {
                    break;
                }

                divisor *= 2;
                shift++;
            }

            while(shift >= 0)
            {
                if(dividend >= divisor)
                {
                    dividend -= divisor;

                    if(dividend == 0)
                    {
                        break;
                    }
                }

                divisor /= 2;
                shift--;
            }

            return dividend;
        }

        //--//

        [TS.WellKnownMethod( "Helpers_BinaryOperations_LongMul" )]
        internal static long LongMul( long left  ,
                                      long right )
        {
            bool fReverse;

            if(left < 0)
            {
                left = unchecked(-left);

                fReverse = true;
            }
            else
            {
                fReverse = false;
            }

            if(right < 0)
            {
                right = unchecked(-right);

                fReverse = !fReverse;
            }

            ulong leftU  = unchecked( (ulong)left  );
            ulong rightU = unchecked( (ulong)right );

            long res = (long)(leftU * rightU);

            if(fReverse)
            {
                res = -res;
            }

            return res;
        }

        [TS.WellKnownMethod( "Helpers_BinaryOperations_LongDiv" )]
        internal static long LongDiv( long left  ,
                                      long right )
        {
            bool fReverse;

            if(left < 0)
            {
                left = unchecked(-left);

                fReverse = true;
            }
            else
            {
                fReverse = false;
            }

            if(right < 0)
            {
                right = unchecked(-right);

                fReverse = !fReverse;
            }

            ulong leftU  = unchecked( (ulong)left  );
            ulong rightU = unchecked( (ulong)right );

            long res = (long)(leftU / rightU);

            if(fReverse)
            {
                res = -res;
            }

            return res;
        }

        [TS.WellKnownMethod( "Helpers_BinaryOperations_LongRem" )]
        internal static long LongRem( long left  ,
                                      long right )
        {
            return left - (left / right) * right;
        }

        [Inline]
        [TS.WellKnownMethod( "Helpers_BinaryOperations_LongShl" )]
        internal static long LongShl( long left  ,
                                      int  shift )
        {
            if(shift == 0)
            {
                return left;
            }
            else
            {
                uint leftHigh = (uint)(left >> 32);
                uint leftLow  = (uint) left       ;

                if(shift >= 32)
                {
                    if(shift >= 64)
                    {
                        return 0;
                    }
                    else
                    {
                        return (long)(((ulong)(leftLow << (shift - 32))) << 32);
                    }
                }
                else
                {
                    uint resHigh = (leftHigh << shift) | (leftLow >> (32 - shift));
                    uint resLow  = (leftLow  << shift);

                    return (long)(((ulong)resHigh << 32) | (ulong)resLow);
                }
            }
        }

        [Inline]
        [TS.WellKnownMethod( "Helpers_BinaryOperations_LongShr" )]
        internal static long LongShr( long left  ,
                                      int  shift )
        {
            if(shift == 0)
            {
                return left;
            }
            else
            {
                int  leftHigh = (int )(left >> 32);
                uint leftLow  = (uint) left       ;

                if(shift >= 32)
                {
                    if(shift >= 64)
                    {
                        //
                        // Just return the sign.
                        //
                        return (long)((int)leftHigh >> 31);
                    }
                    else
                    {
                        return (long)((int)leftHigh >> (shift - 32));
                    }
                }
                else
                {
                    int  resHigh = (leftHigh >> shift);
                    uint resLow  = (leftLow  >> shift) | ((uint)leftHigh << (32 - shift));

                    return (long)(((ulong)resHigh << 32) | (ulong)resLow);
                }
            }
        }

        //--//

        [Inline]
        [TS.WellKnownMethod( "Helpers_BinaryOperations_UlongMul" )]
        internal static ulong UlongMul( ulong left  ,
                                        ulong right )
        {
            uint leftHigh  = (uint)(left  >> 32);
            uint leftLow   = (uint) left        ;
            uint rightHigh = (uint)(right >> 32);
            uint rightLow  = (uint) right       ;

            uint mid1 = (uint)((ulong)leftLow  * (ulong)rightHigh);
            uint mid2 = (uint)((ulong)leftHigh * (ulong)rightLow );

            return ((ulong)leftLow * (ulong)rightLow ) +
                   ((ulong)mid1 << 32                ) +
                   ((ulong)mid2 << 32                ) ;
        }

        [TS.WellKnownMethod( "Helpers_BinaryOperations_UlongDiv" )]
        internal static ulong UlongDiv( ulong dividend ,
                                        ulong divisor  )
        {
            if(divisor == 0)
            {
                throw new DivideByZeroException();
            }

            ulong res   = 0;
            int   shift = 0;

            while(dividend > divisor)
            {
                if(MathImpl.IsNegative( divisor ))
                {
                    break;
                }

                divisor *= 2;
                shift++;
            }

            while(shift >= 0)
            {
                res *= 2;

                if(dividend >= divisor)
                {
                    dividend -= divisor;
                    res      += 1;

                    if(dividend == 0)
                    {
                        res <<= shift;
                        break;
                    }
                }

                divisor /= 2;
                shift--;
            }

            return res;
        }

        [TS.WellKnownMethod( "Helpers_BinaryOperations_UlongRem" )]
        internal static ulong UlongRem( ulong dividend ,
                                        ulong divisor  )
        {
            if(divisor == 0)
            {
                throw new DivideByZeroException();
            }

            int shift = 0;

            while(dividend > divisor)
            {
                if(MathImpl.IsNegative( divisor ))
                {
                    break;
                }

                divisor *= 2;
                shift++;
            }

            while(shift >= 0)
            {
                if(dividend >= divisor)
                {
                    dividend -= divisor;

                    if(dividend == 0)
                    {
                        break;
                    }
                }

                divisor /= 2;
                shift--;
            }

            return dividend;
        }

        [Inline]
        [TS.WellKnownMethod( "Helpers_BinaryOperations_UlongShl" )]
        internal static ulong UlongShl( ulong left  ,
                                        int   shift )
        {
            if(shift == 0)
            {
                return left;
            }
            else
            {
                uint leftHigh = (uint)(left >> 32);
                uint leftLow  = (uint) left       ;

                if(shift >= 32)
                {
                    if(shift >= 64)
                    {
                        return 0;
                    }
                    else
                    {
                        return ((ulong)(leftLow << (shift - 32))) << 32;
                    }
                }
                else
                {
                    uint resHigh = (leftHigh << shift) | (leftLow >> (32 - shift));
                    uint resLow  = (leftLow  << shift);

                    return (((ulong)resHigh << 32) | (ulong)resLow);
                }
            }
        }

        [Inline]
        [TS.WellKnownMethod( "Helpers_BinaryOperations_UlongShr" )]
        internal static ulong UlongShr( ulong left  ,
                                        int   shift )
        {
            if(shift == 0)
            {
                return left;
            }
            else
            {
                uint leftHigh = (uint)(left >> 32);
                uint leftLow  = (uint) left       ;

                if(shift >= 32)
                {
                    if(shift >= 64)
                    {
                        return 0;
                    }
                    else
                    {
                        return (ulong)(leftHigh >> (shift - 32));
                    }
                }
                else
                {
                    uint resHigh = (leftHigh >> shift);
                    uint resLow  = (leftLow  >> shift) | (leftHigh << (32 - shift));

                    return (((ulong)resHigh << 32) | (ulong)resLow);
                }
            }
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_FloatAdd" )]
        internal static float FloatAdd( float left  ,
                                        float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            fiLeft.Add( ref fiRight );

            return fiLeft.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_FloatSub" )]
        internal static float FloatSub( float left  ,
                                        float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            fiLeft.Sub( ref fiRight );

            return fiLeft.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_FloatMul" )]
        internal static float FloatMul( float left  ,
                                        float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            fiLeft.Mul( ref fiRight );

            return fiLeft.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_FloatDiv" )]
        internal static float FloatDiv( float left  ,
                                        float right )
        {
            FloatImplementation fiLeft  = new FloatImplementation( left  );
            FloatImplementation fiRight = new FloatImplementation( right );

            fiLeft.Div( ref fiRight );

            return fiLeft.ToFloat();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_FloatRem" )]
        internal static float FloatRem( float left  ,
                                        float right )
        {
            return left - (int)(left / right) * right;
        }

        //--//

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_DoubleAdd" )]
        internal static double DoubleAdd( double left  ,
                                          double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            diLeft.Add( ref diRight );

            return diLeft.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_DoubleSub" )]
        internal static double DoubleSub( double left  ,
                                          double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            diLeft.Sub( ref diRight );

            return diLeft.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_DoubleMul" )]
        internal static double DoubleMul( double left  ,
                                          double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            diLeft.Mul( ref diRight );

            return diLeft.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_DoubleDiv" )]
        internal static double DoubleDiv( double left  ,
                                          double right )
        {
            DoubleImplementation diLeft  = new DoubleImplementation( left  );
            DoubleImplementation diRight = new DoubleImplementation( right );

            diLeft.Div( ref diRight );

            return diLeft.ToDouble();
        }

        [TS.WellKnownMethod( "SoftFP_BinaryOperations_DoubleRem" )]
        internal static double DoubleRem( double left  ,
                                          double right )
        {
            return left - (long)(left / right) * right;
        }
    }
}

