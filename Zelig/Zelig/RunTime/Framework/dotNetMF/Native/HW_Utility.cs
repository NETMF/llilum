////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Microsoft.SPOT.Hardware
{
    public static class Utility
    {
        [Obsolete( "", false )]
        //////[MethodImplAttribute( MethodImplOptions.InternalCall )]
        //////extern static public uint ComputeCRC( byte[] buf, int offset, int length, uint crc );
        static public uint ComputeCRC(byte[] buf, int offset, int length, uint crc)
        {
            // TODO TODO TODO: Implement
            throw new NotImplementedException( ); 
        }

        //--//

        //--//

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public uint ExtractValueFromArray(byte[] data, int pos, int size);
        [Obsolete( "", false )]
        static public unsafe uint ExtractValueFromArray(byte[] data, int pos, int size)
        {
            if(data == null)
            {
                throw new ArgumentException( ); 
            }
            if(size > 4 || pos + size > data.Length)
            {
                throw new ArgumentOutOfRangeException( ); 
            }

            uint val = 0;
            
            fixed(byte* src = &data[pos])
            {
                byte* dst = (byte*)&val;

                while(--size >= 0)
                {
                    dst[ size ] = src[ size ];
                }
            }

            return val;
        }

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public void InsertValueIntoArray(byte[] data, int pos, int size, uint val);
        [Obsolete( "", false )]
        static public unsafe void InsertValueIntoArray(byte[] data, int pos, int size, uint val)
        {
            if(data == null)
            {
                throw new ArgumentException( ); 
            }
            if(size > 4 || pos + size > data.Length)
            {
                throw new ArgumentOutOfRangeException( ); 
            }
            
            fixed(byte* dst = &data[pos])
            {
                byte* src = (byte*)&val;

                while(--size >= 0)
                {
                    dst[ size ] = src[ size ];
                }
            }
        }

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public byte[] ExtractRangeFromArray(byte[] data, int offset, int count);
        [Obsolete( "", false )]
        static public unsafe byte[] ExtractRangeFromArray(byte[] data, int offset, int count)
        {
            if(data == null)
            {
                throw new ArgumentException( ); 
            }
            if(offset + count > data.Length)
            {
                throw new ArgumentOutOfRangeException( ); 
            }

            var res = new byte[count]; 
            
            fixed(byte* src = &data[offset]) fixed(byte* dst = &res[0])
            {
                while(--count >= 0)
                {
                    dst[ count ] = src[ count ];
                }
            }

            return res;
        }
        
        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public byte[] CombineArrays(byte[] src1, byte[] src2);
        [Obsolete( "", false )]
        static public unsafe byte[] CombineArrays(byte[] src1, byte[] src2)
        {
            return CombineArrays( src1, 0, src1.Length, src2, 0, src2.Length );
        }

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public byte[] CombineArrays(byte[] src1, int offset1, int count1, byte[] src2, int offset2, int count2);
        [Obsolete( "", false )]
        static public byte[] CombineArrays(byte[] src1, int offset1, int count1, byte[] src2, int offset2, int count2)
        {
            if(src1 == null || src2 == null)
            {
                throw new ArgumentException( ); 
            }

            var res = new byte[ count1 + count2 ];
            
            Array.Copy( src1, offset1, res, 0     , count1 ); 
            Array.Copy( src2, offset2, res, count1, count2 ); 
            
            return res;
        }

        //--//

        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        extern static public void SetLocalTime( DateTime dt );

        [MethodImplAttribute( MethodImplOptions.InternalCall )]
        extern static public TimeSpan GetMachineTime( );

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public void Piezo(uint frequency, uint duration);
        [Obsolete( "", true )]
        static public void Piezo(uint frequency, uint duration)
        {
            throw new NotSupportedException( ); 
        }

        //////[MethodImplAttribute(MethodImplOptions.InternalCall)]
        //////extern static public void Backlight(bool fStatus);
        [Obsolete( "", true )]
        static public void Backlight(bool fStatus)
        {
            throw new NotSupportedException( ); 
        }
    }
}


