//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.IO;


    public sealed class BinaryBlob
    {
        //
        // State
        //

        private readonly byte[] m_data;

        //
        // Constructor Methods
        //

        public BinaryBlob( int size )
        {
            m_data = new byte[size];
        }

        public BinaryBlob( byte[] data )
        {
            m_data = data;
        }

        //
        // Helper Methods
        //

        public BinaryBlob Extract( int offset ,
                                   int count  )
        {
            if(offset == 0 && count == this.Size)
            {
                return this;
            }

            var bb = new BinaryBlob( count );
 
            for(int pos = 0; pos < count; pos++)
            {
                bb.WriteUInt8( this.ReadUInt8( pos + offset ), pos );
            }

            return bb;
        }

        public void Insert( BinaryBlob bb     ,
                            int        offset )
        {
            Insert( bb, offset, bb.Size );
        }

        public void Insert( BinaryBlob bb     ,
                            int        offset ,
                            int        count  )
        {
            for(int pos = 0; pos < count; pos++)
            {
                this.WriteUInt8( bb.ReadUInt8( pos ), pos + offset );
            }
        }

        //--//

        public byte ReadUInt8( int offset )
        {
            if(offset >= 0 && offset < m_data.Length)
            {
                return m_data[offset];
            }

            return 0;
        }

        public ushort ReadUInt16( int offset )
        {
            byte partLo = ReadUInt8( offset                );
            byte partHi = ReadUInt8( offset + sizeof(byte) );

            return (ushort)(((uint)partHi << 8) | (uint)partLo);
        }

        public uint ReadUInt32( int offset )
        {
            ushort partLo = ReadUInt16( offset                  );
            ushort partHi = ReadUInt16( offset + sizeof(ushort) );

            return (((uint)partHi << 16) | (uint)partLo);
        }

        public ulong ReadUInt64( int offset )
        {
            uint partLo = ReadUInt32( offset                );
            uint partHi = ReadUInt32( offset + sizeof(uint) );

            return (((ulong)partHi << 32) | (ulong)partLo);
        }

        public byte[] ReadBlock( int offset ,
                                 int count  )
        {
            var res = new byte[count];

            for(int pos = 0; pos < count; pos++)
            {
                res[pos] = this.ReadUInt8( offset + pos ); 
            }

            return res;
        }

        //--//

        public void WriteUInt8( byte val    ,
                                int  offset )
        {
            if(offset >= 0 && offset < m_data.Length)
            {
                m_data[offset] = val;
            }
        }

        public void WriteUInt16( ushort val    ,
                                 int    offset )
        {
            WriteUInt8( (byte) val      , offset                );
            WriteUInt8( (byte)(val >> 8), offset + sizeof(byte) );
        }

        public void WriteUInt32( uint val    ,
                                 int  offset )
        {
            WriteUInt16( (ushort) val       , offset                 );
            WriteUInt16( (ushort)(val >> 16), offset + sizeof(ushort) );
        }

        public void WriteUInt64( ulong val    ,
                                 int   offset )
        {
            WriteUInt32( (uint) val       , offset                );
            WriteUInt32( (uint)(val >> 32), offset + sizeof(uint) );
        }

        //--//

        public void WriteBlock( byte[] buf    ,
                                int    offset )
        {
            foreach(var val in buf)
            {
                WriteUInt8( val, offset++ );
            }
        }

        //--//

        public static BinaryBlob Wrap( byte value )
        {
            var bb = new BinaryBlob( sizeof(byte) );

            bb.WriteUInt8( value, 0 );

            return bb;
        }

        public static BinaryBlob Wrap( ushort value )
        {
            var bb = new BinaryBlob( sizeof(ushort) );

            bb.WriteUInt16( value, 0 );

            return bb;
        }

        public static BinaryBlob Wrap( uint value )
        {
            var bb = new BinaryBlob( sizeof(uint) );

            bb.WriteUInt32( value, 0 );

            return bb;
        }

        public static BinaryBlob Wrap( uint valueLo ,
                                       uint valueHi )
        {
            var bb = new BinaryBlob( sizeof(ulong) );

            bb.WriteUInt32( valueLo, 0                );
            bb.WriteUInt32( valueHi, 0 + sizeof(uint) );

            return bb;
        }

        public static BinaryBlob Wrap( ulong value )
        {
            var bb = new BinaryBlob( sizeof(ulong) );

            bb.WriteUInt64( value, 0 );

            return bb;
        }

        public static BinaryBlob Wrap( object value )
        {
            if(value is bool)
            {
                return Wrap( (byte)((bool)value ? 1 : 0) );
            }

            if(value is byte)
            {
                return Wrap( (byte)value );
            }

            if(value is sbyte)
            {
                return Wrap( (byte)(sbyte)value );
            }

            if(value is char)
            {
                return Wrap( (ushort)(char)value );
            }

            if(value is short)
            {
                return Wrap( (ushort)(short)value );
            }

            if(value is ushort)
            {
                return Wrap( (ushort)value );
            }

            if(value is int)
            {
                return Wrap( (uint)(int)value );
            }

            if(value is uint)
            {
                return Wrap( (uint)value );
            }

            if(value is long)
            {
                return Wrap( (ulong)(long)value );
            }

            if(value is ulong)
            {
                return Wrap( (ulong)value );
            }

            if(value is float)
            {
                return Wrap( DataConversion.GetFloatAsBytes( (float)value ));
            }

            if(value is double)
            {
                return Wrap( DataConversion.GetDoubleAsBytes( (double)value ));
            }

            return null;
        }

        //
        // Access Methods
        //

        public byte[] Payload
        {
            get
            {
                return m_data;
            }
        }

        public int Size
        {
            get
            {
                return m_data.Length;
            }
        }
    }
}
