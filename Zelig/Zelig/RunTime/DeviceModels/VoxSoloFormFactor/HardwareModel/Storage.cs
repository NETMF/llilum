//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.VoxSoloFormFactor
{
    using System;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public sealed class Storage : RT.Storage
    {
        const uint   FlashBase       = 0x10000000;
        const uint   FlashSize       = 16 * 1024 * 1024;
        const uint   FlashMask       = ~(FlashSize - 1);
        const ushort ErasedValue     = (ushort)0xFFFFu;
        const uint   ErasedValuePair = 0xFFFFFFFFu;
        const ushort DQ5             = (ushort)(1u << 5);

        class EraseSection
        {
            //
            // State
            //

            internal uint m_baseAddress;
            internal uint m_endAddress;
            internal uint m_numEraseBlocks;
            internal uint m_sizeEraseBlocks;

            //
            // Helper Methods
            //

            [RT.MemoryRequirements( RT.MemoryAttributes.RAM )] // Used during firmware update.
            internal unsafe bool FindBlock(     UIntPtr address    ,
                                            out ushort* blockStart ,
                                            out ushort* blockEnd   )
            {
                if(m_baseAddress <= address.ToUInt32() && address.ToUInt32() < m_endAddress)
                {
                    for(uint i = 0; i < m_numEraseBlocks; i++)
                    {
                        uint block     = m_baseAddress + m_sizeEraseBlocks * i;
                        uint blockNext = block         + m_sizeEraseBlocks;

                        if(block <= address.ToUInt32() && address.ToUInt32() < blockNext)
                        {
                            blockStart = (ushort*)block;
                            blockEnd   = (ushort*)blockNext;
                            return true;
                        }
                    }
                }

                blockStart = null;
                blockEnd   = null;
                return false;
            }
        }

        //
        // State
        //
        
        EraseSection[] m_eraseSections;

        //
        // Helper Methods
        //

        public override unsafe void InitializeStorage()
        {
            ushort[] cfg              = new ushort[128];
            ushort*  flashBaseAddress = GetChipBaseAddress();

            ReadCFI( flashBaseAddress, cfg );

            if(cfg[0x10] == 'Q' &&
               cfg[0x11] == 'R' &&
               cfg[0x12] == 'Y'  )
            {
                uint numEraseBlockRegions = cfg[0x2C];
                uint baseAddress          = (uint)flashBaseAddress;

                m_eraseSections = new EraseSection[numEraseBlockRegions];

                for(uint pos = 0; pos < numEraseBlockRegions; pos++)
                {
                    EraseSection section = new EraseSection();

                    section.m_baseAddress     = baseAddress;
                    section.m_numEraseBlocks  = ((uint)cfg[0x2D + pos * 4] + ((uint)cfg[0x2E + pos * 4] << 8)) + 1u;
                    section.m_sizeEraseBlocks = ((uint)cfg[0x2F + pos * 4] + ((uint)cfg[0x30 + pos * 4] << 8)) * 256;

                    baseAddress += section.m_numEraseBlocks * section.m_sizeEraseBlocks;

                    section.m_endAddress = baseAddress;

                    m_eraseSections[pos] = section;
                }
            }
        }

        //--//

        public override unsafe bool EraseSectors( UIntPtr addressStart ,
                                                  UIntPtr addressEnd   )
        {
            ushort* flashBaseAddress = GetChipBaseAddress();

            foreach(var sec in m_eraseSections)
            {
                ushort* blockStart;
                ushort* blockEnd;

                while(sec.FindBlock( addressStart, out blockStart, out blockEnd ))
                {
                    if(ShouldEraseBlock( blockStart, blockEnd ))
                    {
                        StartBlockErase( flashBaseAddress, blockStart );

                        System.Threading.Thread.Sleep( 500 );

                        if(WaitForCompletion( blockStart ) == false)
                        {
                            return false;
                        }
                    }

                    addressStart = new UIntPtr( blockEnd );

                    if(Microsoft.Zelig.AddressMath.IsLessThan( addressStart, addressEnd ) == false)
                    {
                        break;
                    }
                }
            }

            return true;
        }

        public override unsafe bool WriteByte( UIntPtr address ,
                                               byte    val     )
        {
            byte*  ptr = (byte*)address.ToPointer();
            ushort val2;

            if(IsOddAddress( address ))
            {
                ptr -= 1;

                val2 = (ushort)((uint)val << 8 | (uint)ptr[0]);
            }
            else
            {
                val2 = (ushort)((uint)ptr[1] << 8 | (uint)val);
            }

            return WriteShort( new UIntPtr( ptr ), val2 );
        }

        public override unsafe bool WriteShort( UIntPtr address ,
                                                ushort  val     )
        {
            if(IsOddAddress( address ))
            {
                return WriteByte(                                        address,      (byte) val       ) &&
                       WriteByte( Microsoft.Zelig.AddressMath.Increment( address, 1 ), (byte)(val >> 8) )  ;
            }

            ushort* flashBaseAddress = GetChipBaseAddress();
            ushort* wordAddress      = (ushort*)address.ToPointer();

            StartWordProgramming( flashBaseAddress, wordAddress, val );

            WaitForCompletion( wordAddress );

            return wordAddress[0] == val;
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
            byte* ptr = (byte*)address.ToPointer();

            return ptr[0];
        }

        public override unsafe ushort ReadShort( UIntPtr address )
        {
            byte* ptr = (byte*)address.ToPointer();

            return (ushort)((uint)ptr[0]      |
                            (uint)ptr[1] << 8 );
        }

        public override unsafe uint ReadWord( UIntPtr address )
        {
            byte* ptr = (byte*)address.ToPointer();

            return ((uint)ptr[0]       |
                    (uint)ptr[1] << 8  |
                    (uint)ptr[2] << 16 |
                    (uint)ptr[3] << 24 );
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


        [RT.NoInline]
        [RT.DisableNullChecks()]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        public unsafe override void SubstituteFirmware( UIntPtr addressDestination ,
                                                        UIntPtr addressSource      ,
                                                        uint    numBytes           )
        {
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                ushort* flashBaseAddress = GetChipBaseAddress();

                //
                // Erase old firmware.
                //
                {
                    UIntPtr addressStart =                                        addressDestination;
                    UIntPtr addressEnd   = Microsoft.Zelig.AddressMath.Increment( addressDestination, numBytes ); 

                    foreach(var sec in m_eraseSections)
                    {
                        ushort* blockStart;
                        ushort* blockEnd;

                        while(sec.FindBlock( addressStart, out blockStart, out blockEnd ))
                        {
                            StartBlockErase( flashBaseAddress, blockStart );

                            if(WaitForCompletion( blockStart ) == false)
                            {
                                while(true)
                                {
                                }
                            }

                            addressStart = new UIntPtr( blockEnd );

                            if(Microsoft.Zelig.AddressMath.IsLessThan( addressStart, addressEnd ) == false)
                            {
                                break;
                            }
                        }
                    }
                }

                //
                // Copy firmware.
                //
                {
                    var dst = (ushort*)addressDestination.ToPointer();
                    var src = (ushort*)addressSource     .ToPointer();

                    for(int i = 0; i < numBytes; i += sizeof(ushort))
                    {
                        StartWordProgramming( flashBaseAddress, dst, *src );

                        WaitForCompletion( dst );

                        if(*dst++ != *src++)
                        {
                            while(true)
                            {
                            }
                        }
                    }
                }

                //
                // Erase new firmware.
                //
                {
                    UIntPtr addressStart =                                        addressSource;
                    UIntPtr addressEnd   = Microsoft.Zelig.AddressMath.Increment( addressSource, numBytes ); 

                    foreach(var sec in m_eraseSections)
                    {
                        ushort* blockStart;
                        ushort* blockEnd;

                        while(sec.FindBlock( addressStart, out blockStart, out blockEnd ))
                        {
                            StartBlockErase( flashBaseAddress, blockStart );

                            if(WaitForCompletion( blockStart ) == false)
                            {
                                while(true)
                                {
                                }
                            }

                            addressStart = new UIntPtr( blockEnd );

                            if(Microsoft.Zelig.AddressMath.IsLessThan( addressStart, addressEnd ) == false)
                            {
                                break;
                            }
                        }
                    }
                }

                //
                // Reboot.
                //
                Processor.SetRegister( Processor.Context.RegistersOnStack.ProgramCounterRegister, new UIntPtr( flashBaseAddress ) );
            }
        }

        public unsafe override void RebootDevice()
        {
            //
            // Reboot.
            //
            Processor.SetRegister( Processor.Context.RegistersOnStack.ProgramCounterRegister, new UIntPtr( GetChipBaseAddress() ) );
        }

        //--//

        [RT.Inline]
        static bool IsOddAddress( UIntPtr address )
        {
            return Zelig.AddressMath.IsAlignedTo16bits( address ) == false;
        }

        [RT.DisableNullChecks()]
        static unsafe bool ShouldEraseBlock( ushort* blockStart ,
                                             ushort* blockEnd   )
        {
            uint* ptr = (uint*)blockStart;

            while(ptr < blockEnd)
            {
                if(ptr[0] != ErasedValuePair)
                {
                    return true;
                }

                ptr++;
            }

            return false;
        }

        [RT.NoInline]
        [RT.DisableNullChecks()]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        static unsafe void ReadCFI( ushort*  flashBaseAddress ,
                                    ushort[] buf              )
        {
            EnterCFI( flashBaseAddress );

            for(int i = 0; i < buf.Length; i++)
            {
                buf[i] = flashBaseAddress[i];
            }

            IssueResetCommand( flashBaseAddress );
        }

        [RT.NoInline]
        [RT.DisableNullChecks()]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        static unsafe void StartWordProgramming( ushort* flashBaseAddress ,
                                                 ushort* wordAddress      ,
                                                 ushort  val              )
        {
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                flashBaseAddress[0x555] = 0x00AA;
                flashBaseAddress[0x2AA] = 0x0055;
                flashBaseAddress[0x555] = 0x00A0;

                wordAddress[0] = val;
            }
        }

        [RT.NoInline]
        [RT.DisableNullChecks()]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        static unsafe void StartBlockErase( ushort* flashBaseAddress ,
                                            ushort* blockStart       )
        {
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                flashBaseAddress[0x555] = 0x00AA;
                flashBaseAddress[0x2AA] = 0x0055;
                flashBaseAddress[0x555] = 0x0080;
                flashBaseAddress[0x555] = 0x00AA;
                flashBaseAddress[0x2AA] = 0x0055;

                blockStart[0] = 0x0030;
            }
        }

////    public static int fail;
////    public static int fail_loops;
////    public static int words;
////    public static int loops;
////    public static ushort fail_val1;
////    public static ushort fail_val2;
////    public static ushort fail_val3;

        [RT.NoInline]
        [RT.DisableNullChecks()]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        static unsafe bool WaitForCompletion( ushort* wordAddress )
        {
////        int loop = 0;

            while(true)
            {
                uint val1;
                uint val2;
                uint val3;

                using(RT.SmartHandles.InterruptState.DisableAll())
                {
                    val1 = wordAddress[0];
                    val2 = wordAddress[0];
                    val3 = wordAddress[0];
                }

                //
                // Is DQ6 toggling?
                //
                if(((val1 ^ val2) & 0x40) != 0 || ((val2 ^ val3) & 0x40) != 0)
                {
                    //
                    // Continue.
                    //
////                loops++;
                    continue;
                }

                if(val1 == val2 || val2 == val3)
                {
////                words += 1;
////                loops += loop;
                    return true;
                }
                else
                {
////                fail_val1   = (ushort)val1;
////                fail_val2   = (ushort)val2;
////                fail_val2   = (ushort)val3;
////                fail       += 1;
////                fail_loops += loop;
                    IssueResetCommand( wordAddress );
                    return false;
                }
            }
        }

        //--//

        [RT.Inline]
        [RT.DisableNullChecks()]
        static unsafe ushort* GetChipBaseAddress()
        {
            return (ushort*)new UIntPtr( FlashBase ).ToPointer();
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        static unsafe ushort* GetChipBaseAddress( ushort* address )
        {
            return (ushort*)((uint)address & FlashMask);
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        static unsafe void EnterCFI( ushort* baseAddress )
        {
            baseAddress[0x555] = 0x98;
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        static unsafe void IssueResetCommand( ushort* baseAddress )
        {
            baseAddress[0] = 0xF0;
        }
    }
}
