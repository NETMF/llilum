//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(Math))]
    internal static class MathImpl
    {
        public static float Abs( float value )
        {
            if(value < 0) return -value;

            return value;
        }

        public static double Abs( double value )
        {
            if(value < 0) return -value;

            return value;
        }

        public static double Exp( double x )
        {
            const double Exp           = Math.E;
            const double ExpHalfFactor = 1.6487212707001281468486507878142; // E^0.5
            const double ExpStep6      = 1.0;
            const double ExpStep5      = ExpStep6 / 2;
            const double ExpStep4      = ExpStep5 / 3;
            const double ExpStep3      = ExpStep4 / 4;
            const double ExpStep2      = ExpStep3 / 5;
            const double ExpStep1      = ExpStep2 / 6;

            int sign;

            // Reduce range to [0.0,1.0] 
            if (x < 0)
            {
                x    = -x;
                sign = -1;
            }
            else
            {
                sign = 1;
            }

            double result = 1.0;

            while(x > 1.0)
            {
                x      -= 1.0;
                result *= Exp;
            }

            // Reduce range to [0.0,0.5] 
            if (x > 0.5)
            {
                x      -= 0.5;
                result *= ExpHalfFactor;
            }

            double temp;

            temp =         ExpStep1  * x;
            temp = (temp + ExpStep2) * x;
            temp = (temp + ExpStep3) * x;
            temp = (temp + ExpStep4) * x;
            temp = (temp + ExpStep5) * x;
            temp = (temp + ExpStep6) * x;

            result *= (temp + 1.0);

            if (sign == -1)
            {
                result = 1 / result;
            }

            return result;
        }

        public static double Log10( double x )
        {
            const double c_Ln10        = 2.3025850929940456840179914546844;
            const double c_OneOverLn10 = 1.0 / c_Ln10;

            return Log( x ) * c_OneOverLn10;
        }

        public static double Log( double x )
        {
            const double LnTwo    =  0.69314718055994530941723212145818;
            const double LogStep1 = -0.0064535442;
            const double LogStep2 =  0.0360884937;
            const double LogStep3 =  0.0953293897;
            const double LogStep4 =  0.1676540711;
            const double LogStep5 =  0.2407338084;
            const double LogStep6 =  0.3317990258;
            const double LogStep7 =  0.4998741238;
            const double LogStep8 =  0.9999964239;

            if(x == 0)
            {
                return Double.MinValue;
            }
            else if (x < 0)
            {
                return 0;
            }

            double result = 0;

            while (x > 2.0)
            {
                result += LnTwo;
                x      /= 2;
            }

            while (x < 1)
            {
                result -= LnTwo;
                x      *= 2;
            }

            x -= 1.0;

            double temp;

            temp =         LogStep1  * x;
            temp = (temp + LogStep2) * x;
            temp = (temp - LogStep3) * x;
            temp = (temp + LogStep4) * x;
            temp = (temp - LogStep5) * x;
            temp = (temp + LogStep6) * x;
            temp = (temp - LogStep7) * x;
            temp = (temp + LogStep8) * x;
            
            result += temp;

            return result;
        }

        public static double Pow( double x ,
                                  double y )
        {
            return Math.Exp( y * Math.Log( x ) );
        }

        public static double Atan( double x )
        {

            bool fNegate = false;

            if(x < 0)
            {
                x = -x;
                fNegate = true;
            }

            x = x % ( Math.PI * 2 );

            if(x > Math.PI)
            {
                x -= Math.PI;
                fNegate = !fNegate;
            }

            if(x > ( Math.PI / 2 ))
            {
                x = Math.PI - x;
            }

            double absX = x;
            if(fNegate) x = -x;

            return ( Math.PI / 4.0 ) * x - x * ( absX - 1 ) * ( 0.2447 + 0.0663 * absX );
        }

        public static double Sin( double x )
        {
            bool fNegate = false;

            if(x < 0)
            {
                x       = -x;
                fNegate = true;
            }

            x = x % (Math.PI * 2);

            if(x > Math.PI)
            {
                x       -= Math.PI;
                fNegate  = !fNegate;
            }

            if(x > (Math.PI/2))
            {
                x = Math.PI - x;
            }

            double y = x * x;
            double res;

            res =         - 0.0000000239;
            res = res * y + 0.0000027526;
            res = res * y - 0.0001984090;
            res = res * y + 0.0083333315;
            res = res * y - 0.1666666664;
            res = res * y + 1;
            res = res * x;

            if(fNegate)
            {
                return -res;
            }
            else
            {
                return res;
            }
        }

        public static double Cos( double x )
        {
            x = Math.Abs( x );

            x = x % (Math.PI * 2);

            bool fNegate = false;

            if(x > Math.PI)
            {
                x       -= Math.PI;
                fNegate  = !fNegate;
            }

            if(x > (Math.PI/2))
            {
                x       = Math.PI - x;
                fNegate = !fNegate;
            }

            double y = x * x;
            double res;

            res =         - 0.0000002605;
            res = res * y + 0.0000247609;
            res = res * y - 0.0013888397;
            res = res * y + 0.0416666418;
            res = res * y - 0.4999999963;
            res = res * y + 1;

            if(fNegate)
            {
                return -res;
            }
            else
            {
                return res;
            }
        }

        public static double Sqrt( double x )
        {
            if(x < 0)
            {
                throw new ArithmeticException();
            }

            if(x == 0)
            {
                return 0;
            }

            double precision = 0.001;
            double oldResult = -1;
            double newResult =  1;

            while(Math.Abs( newResult - oldResult ) > precision)
            {
                oldResult =  newResult;
                newResult = (oldResult * oldResult + x) / (2 * oldResult);
            }

            return newResult;
        }

        public static double Round( double a )
        {
            return (double)(long)a;
        }

        public static double Floor( double a )
        {
            double b = Math.Round( a );

            return (b > a) ? b - 1.0 : b;
        }

        public static double Ceiling( double a )
        {
            double b = Math.Round( a );

            return (b < a) ? b + 1.0 : b;
        }

        [Inline]
        public static uint ExtractHighPart( ulong val )
        {
            return (uint)(val >> 32);
        }

        [Inline]
        public static uint ExtractLowPart( ulong val )
        {
            return (uint)val;
        }

        [Inline]
        public static ulong InsertHighPart( uint val )
        {
            return (ulong)val << 32;
        }

        [Inline]
        public static bool IsPositive( uint val )
        {
            return (int)val >= 0;
        }

        [Inline]
        public static bool IsNegative( uint val )
        {
            return (int)val < 0;
        }

        [Inline]
        public static bool IsPositive( ulong val )
        {
            return (int)ExtractHighPart( val ) >= 0;
        }

        [Inline]
        public static bool IsNegative( ulong val )
        {
            return (int)ExtractHighPart( val ) < 0;
        }
    }
}
