//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig
{
    using System;
    using System.Collections;
    using System.Collections.Generic;


    public static class DataConversion
    {
        public static TypeCode GetTypeCode( object val )
        {
            var res = GetTypeCodeNoPointers( val );

            if(res != TypeCode.Empty)
            {
                return res;
            }

            if(val is IntPtr ) return TypeCode.Int32;
            if(val is UIntPtr) return TypeCode.UInt32;

            return TypeCode.Empty;
        }

        public static TypeCode GetTypeCodeNoPointers( object val )
        {
            var valItf = val as IConvertible;

            if(valItf != null)
            {
                return valItf.GetTypeCode();
            }

            return TypeCode.Empty;
        }

        public static unsafe float GetFloatFromBytes( uint val )
        {
            return *((float*)&val);
        }

        public static unsafe double GetDoubleFromBytes( ulong val )
        {
            return *((double*)&val);
        }

        public static unsafe uint GetFloatAsBytes( float val )
        {
            return *((uint*)&val);
        }

        public static unsafe ulong GetDoubleAsBytes( double val )
        {
            return *((ulong*)&val);
        }

        public static unsafe uint GetFragmentOfNumber( object number ,
                                                       int    offset )
        {
            uint* ptr;
            uint  size;

            switch(GetTypeCodeNoPointers( number ))
            {
                case TypeCode.UInt32:
                    {
                        uint val = (uint)number;

                        ptr  = (uint*)&val;
                        size = sizeof(uint);
                    }
                    break;

                case TypeCode.Int32:
                    {
                        int val = (int)number;

                        ptr  = (uint*)&val;
                        size = sizeof(int);
                    }
                    break;

                case TypeCode.UInt64:
                    {
                        ulong val = (ulong)number;

                        ptr  = (uint*)&val;
                        size = sizeof(ulong);
                    }
                    break;

                case TypeCode.Int64:
                    {
                        long val = (long)number;

                        ptr  = (uint*)&val;
                        size = sizeof(long);
                    }
                    break;

                case TypeCode.Single:
                    {
                        float val = (float)number;

                        ptr  = (uint*)&val;
                        size = sizeof(float);
                    }
                    break;

                case TypeCode.Double:
                    {
                        double val = (double)number;

                        ptr  = (uint*)&val;
                        size = sizeof(double);
                    }
                    break;

                default:
                    return 0;
            }

            if(offset * sizeof(uint) < size)
            {
                return ptr[offset];
            }

            return 0;
        }

        public static bool GetAsRawUlong(     object valIn  ,
                                          out ulong  valOut )
        {
            if(valIn is float)
            {
                valOut = (ulong)GetFloatAsBytes( (float)valIn ) ;

                return true;
            }
            
            if(valIn is double)
            {
                valOut = (ulong)GetDoubleAsBytes( (double )valIn ) ;

                return true;
            }

            return GetAsUnsignedInteger( valIn, out valOut );
        }

        public static bool GetAsUnsignedInteger(     object valIn  ,
                                                 out ulong  valOut )
        {
            switch(GetTypeCodeNoPointers( valIn ))
            {
                case TypeCode.Boolean: valOut = (ulong)      ((bool   )valIn ? 1u : 0u); break;
                case TypeCode.Char   : valOut = (ulong)       (char   )valIn           ; break;
                case TypeCode.Byte   : valOut = (ulong)       (byte   )valIn           ; break;
                case TypeCode.UInt16 : valOut = (ulong)       (ushort )valIn           ; break;
                case TypeCode.UInt32 : valOut = (ulong)       (uint   )valIn           ; break;
                case TypeCode.UInt64 : valOut = (ulong)       (ulong  )valIn           ; break;
                case TypeCode.SByte  : valOut = (ulong)(long) (sbyte  )valIn           ; break;
                case TypeCode.Int16  : valOut = (ulong)(long) (short  )valIn           ; break;
                case TypeCode.Int32  : valOut = (ulong)(long) (int    )valIn           ; break;
                case TypeCode.Int64  : valOut = (ulong)       (long   )valIn           ; break;

                default:
                    if(valIn is IntPtr )
                    {
                        valOut = (ulong)(long)((IntPtr )valIn).ToInt32 ();
                        break;
                    }

                    if(valIn is UIntPtr)
                    {
                        valOut = (ulong)      ((UIntPtr)valIn).ToUInt32();
                        break;
                    }

                    valOut = 0;
                    return false;
            }

            return true;
        }

        public static bool GetAsSignedInteger(     object valIn ,
                                               out long   valOut )
        {
            ulong valOutUnsigned;

            if(GetAsUnsignedInteger( valIn, out valOutUnsigned ))
            {
                valOut = (long)valOutUnsigned;

                return true;
            }
            else
            {
                valOut = 0;

                return false;
            }
        }

        public static bool GetFloatingPoint(     object valIn  ,
                                             out double valOut )
        {
            if(valIn is float)
            {
                valOut = (double)(float)valIn;

                return true;
            }
            
            if(valIn is double)
            {
                valOut = (double )valIn;

                return true;
            }

            valOut = 0;

            return false;
        }
    }
}
