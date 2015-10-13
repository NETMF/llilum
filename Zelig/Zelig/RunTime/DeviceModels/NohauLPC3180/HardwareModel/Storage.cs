//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.NohauLPC3180
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class Storage : RT.Storage
    {
        const uint   DRAMSize        = 1 * 8 * 1024 * 1024;
        const uint   DRAMBase        = 0x80000000 + DRAMSize;
        const uint   DRAMEnd         = DRAMBase   + DRAMSize;
        const uint   DRAMMask        = ~(DRAMSize - 1);
        const byte   ErasedValueByte = (byte)0xFFu;
        const ushort ErasedValue     = (ushort)0xFFFFu;
        const uint   ErasedValuePair = 0xFFFFFFFFu;

        //
        // State
        //
        
        //
        // Helper Methods
        //

        public override unsafe void InitializeStorage()
        {
            EraseSectors( new UIntPtr( DRAMBase ), new UIntPtr( DRAMEnd ) );
        }

        //--//

        public override unsafe bool EraseSectors( UIntPtr addressStart ,
                                                  UIntPtr addressEnd   )
        {
            if(ValidateAddress     ( addressStart ) &&
               ValidateAddressPlus1( addressEnd   )  )
            {
                Memory.Fill( addressStart, addressEnd, 0xFF );

                return true;
            }

            return false;
        }

        public override unsafe bool WriteByte( UIntPtr address ,
                                               byte    val     )
        {
            if(ValidateAddress( address ))
            {
                byte* ptr = (byte*)address.ToPointer();

                *ptr = val;

                return true;
            }

            return false;
        }

        public override unsafe bool WriteShort( UIntPtr address ,
                                                ushort  val     )
        {
            if(IsOddAddress( address ))
            {
                return WriteByte(                                        address,      (byte) val       ) &&
                       WriteByte( Microsoft.Zelig.AddressMath.Increment( address, 1 ), (byte)(val >> 8) )  ;
            }
            else
            {
                if(ValidateAddress( address ))
                {
                    ushort* wordAddress = (ushort*)address.ToPointer();

                    *wordAddress = val;

                    return true;
                }

                return false;
            }
        }

        public override bool WriteWord( UIntPtr address ,
                                        uint    val     )
        {
            if(IsOddAddress( address ))
            {
                return WriteByte (                                        address,      (byte  ) val        ) &&
                       WriteShort( Microsoft.Zelig.AddressMath.Increment( address, 1 ), (ushort)(val >>  8) ) &&
                       WriteByte ( Microsoft.Zelig.AddressMath.Increment( address, 3 ), (byte  )(val >> 24) )  ;
            }
            else
            {
                return WriteShort(                                        address,      (ushort) val        ) &&
                       WriteShort( Microsoft.Zelig.AddressMath.Increment( address, 2 ), (ushort)(val >> 16) )  ;
            }
        }

        public override bool Write( UIntPtr address  ,
                                    byte[]  buffer   ,
                                    uint    offset   ,
                                    uint    numBytes )
        {
            if(numBytes > 0)
            {
                if(IsOddAddress( address ))
                {
                    if(WriteByte( address, buffer[offset] ) == false)
                    {
                        return false;
                    }

                    address   = Microsoft.Zelig.AddressMath.Increment( address, 1 );
                    offset   += 1;
                    numBytes -= 1;
                }

                while(numBytes >= 2)
                {
                    uint val;

                    val  = (uint)buffer[ offset++ ];
                    val |= (uint)buffer[ offset++ ] << 8;

                    if(WriteShort( address, (ushort)val ) == false)
                    {
                        return false;
                    }

                    address   = Microsoft.Zelig.AddressMath.Increment( address, 2 );
                    numBytes -= 2;
                }

                if(numBytes != 0)
                {
                    if(WriteByte( address, buffer[offset] ) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //--//

        public override unsafe byte ReadByte( UIntPtr address )
        {
            if(ValidateAddress( address ))
            {
                byte* ptr = (byte*)address.ToPointer();

                return ptr[0];
            }

            return ErasedValueByte;
        }

        public override unsafe ushort ReadShort( UIntPtr address )
        {
            if(ValidateAddress( address ))
            {
                byte* ptr = (byte*)address.ToPointer();

                return (ushort)((uint)ptr[0]      |
                                (uint)ptr[1] << 8 );
            }

            return ErasedValue;
        }

        public override unsafe uint ReadWord( UIntPtr address )
        {
            if(ValidateAddress( address ))
            {
                byte* ptr = (byte*)address.ToPointer();

                return ((uint)ptr[0]       |
                        (uint)ptr[1] << 8  |
                        (uint)ptr[2] << 16 |
                        (uint)ptr[3] << 24 );
            }

            return ErasedValuePair;
        }

        public override void Read( UIntPtr address  ,
                                   byte[]  buffer   ,
                                   uint    offset   ,
                                   uint    numBytes )
        {
            while(numBytes != 0)
            {
                buffer[offset++] = ReadByte( address );

                address = Microsoft.Zelig.AddressMath.Increment( address, 1 );

                numBytes--;
            }
        }

        public override void SubstituteFirmware( UIntPtr addressDestination ,
                                                 UIntPtr addressSource      ,
                                                 uint    numBytes           )
        {
            throw new NotImplementedException();
        }

        public override void RebootDevice()
        {
            throw new System.NotImplementedException();
        }

        //--//

        [RT.Inline]
        static bool ValidateAddress( UIntPtr address )
        {
            if(Zelig.AddressMath.IsLessThan( address, new UIntPtr( DRAMBase ) ))
            {
                return false;
            }

            if(Zelig.AddressMath.IsGreaterThanOrEqual( address, new UIntPtr( DRAMEnd ) ))
            {
                return false;
            }

            return true;
        }

        [RT.Inline]
        static bool ValidateAddressPlus1( UIntPtr address )
        {
            if(Zelig.AddressMath.IsLessThanOrEqual( address, new UIntPtr( DRAMBase ) ))
            {
                return false;
            }

            if(Zelig.AddressMath.IsGreaterThan( address, new UIntPtr( DRAMEnd ) ))
            {
                return false;
            }

            return true;
        }

        [RT.Inline]
        static bool IsOddAddress( UIntPtr address )
        {
            return Zelig.AddressMath.IsAlignedTo16bits( address ) == false;
        }
    }
}
