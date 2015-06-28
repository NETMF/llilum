//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//


namespace Microsoft.VoxSoloFormFactorLoader
{
    using System;
    using System.Collections.Generic;

    using RT = Microsoft.Zelig.Runtime;

    //
    // Override stock bootstrap code, to avoid including most of the code.
    //
    [RT.ExtendClass(typeof(RT.Bootstrap))]
    public static class BootstrapImpl
    {
        [RT.NoInline]
        [RT.NoReturn]
        private static void Initialization()
        {
            Processor.Instance.InitializeProcessor();

            RT.Configuration.ExecuteApplication();
        }
    }

    [RT.ExtendClass(typeof(RT.TypeSystemManager),NoConstructors=true)]
    public class TypeSystemManagerImpl
    {
        public void DeliverException( Exception obj )
        {
        }
    }


    class Loader
    {
        const uint cmd_Signature      = 0xDEADC000;
        const uint cmd_Mask           = 0xFFFFFF00;
                                    
        const byte cmd_Hello          = 0x01;
        const byte cmd_EnterCFI       = 0x02; // Arg: Address
        const byte cmd_ExitCFI        = 0x03; // Arg: Address
        const byte cmd_ReadMemory8    = 0x04; // Arg: Address, Size                 => <Size> values
        const byte cmd_ReadMemory16   = 0x05; // Arg: Address, Size                 => <Size> values
        const byte cmd_ReadMemory32   = 0x06; // Arg: Address, Size                 => <Size> values
        const byte cmd_ChecksumMemory = 0x07; // Arg: Address, Size                 => CRC value, AND of all memory 
        const byte cmd_EraseSector    = 0x08; // Arg: Address                       => Status value
        const byte cmd_ProgramMemory  = 0x09; // Arg: Address, Size, [32bits words] => Status value

        const uint FlashSize = 8 * 1024 * 1024;
        const uint FlashMask = ~(FlashSize - 1);

        //--//

        [RT.DisableNullChecks()]
        static unsafe ushort* GetChipBaseAddress( ushort* address )
        {
            return (ushort*)((uint)address & FlashMask);
        }

        [RT.DisableNullChecks()]
        static unsafe void EnterCFI( ushort* baseAddress )
        {
            baseAddress[0x555] = 0x98;
        }

        [RT.DisableNullChecks()]
        static unsafe void ExitCFI( ushort* baseAddress )
        {
            baseAddress[0] = 0xF0;
        }

        [RT.DisableNullChecks()]
        static unsafe void EraseSector( ushort* sectorStart )
        {
            ushort* baseAddress = GetChipBaseAddress( sectorStart );

            baseAddress[0x555] = 0x00AA;
            baseAddress[0x2AA] = 0x0055;
            baseAddress[0x555] = 0x0080;
            baseAddress[0x555] = 0x00AA;
            baseAddress[0x2AA] = 0x0055;

            sectorStart[0] = 0x0030;

            while(sectorStart[0] != sectorStart[0]);
        }

        [RT.DisableNullChecks()]
        static unsafe void WriteWord( ushort* address ,
                                      ushort  value   )
        {
            ushort* baseAddress = GetChipBaseAddress( address );

            baseAddress[0x555] = 0x00AA;
            baseAddress[0x2AA] = 0x0055;
            baseAddress[0x555] = 0x00A0;

            address[0] = value;

            while(address[0] != address[0]);
        }

        //--//

////    static string m_lock;

        [RT.DisableNullChecks( ApplyRecursively=true )]
        static unsafe void Main()
        {
////        while(System.Threading.Interlocked.CompareExchange( ref m_lock, "Test", null ) != null)
////        {
////        }

            while(true)
            {
                uint cmd = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                if((cmd & cmd_Mask) == cmd_Signature)
                {
                    switch((byte)cmd)
                    {
                        case cmd_Hello:
                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );
                            break;

                        case cmd_EnterCFI:
                            EnterCFI( (ushort*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC() );

                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );
                            break;

                        case cmd_ExitCFI:
                            ExitCFI( (ushort*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC() );

                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );
                            break;

                        case cmd_ReadMemory8:
                            {
                                byte* ptr = (byte*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint  len =        RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                while(len > 0)
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( *ptr++ );
                                    len--;
                                }
                            }
                            break;

                        case cmd_ReadMemory16:
                            {
                                ushort* ptr = (ushort*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint    len =          RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                while(len > 0)
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( *ptr++ );
                                    len--;
                                }
                            }
                            break;

                        case cmd_ReadMemory32:
                            {
                                uint* ptr = (uint*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint  len =        RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                while(len > 0)
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( *ptr++ );
                                    len--;
                                }
                            }
                            break;

                        case cmd_ChecksumMemory:
                            {
                                uint* ptr = (uint*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint  len =        RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint  sum = 0;
                                uint  and = 0xFFFFFFFF;

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                while(len > 0)
                                {
                                    uint val = *ptr;

                                    sum = ((sum & 1) << 31) | (sum >> 1);

                                    sum += val;
                                    and &= val;

                                    ptr++;
                                    len--;
                                }

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( sum );
                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( and );
                            }
                            break;

                        case cmd_EraseSector:
                            EraseSector( (ushort*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC() );

                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( 0 );
                            break;

                        case cmd_ProgramMemory:
                            {
                                ushort* ptr = (ushort*)RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint    len =          RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                while(len > 0)
                                {
                                    uint val = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                    WriteWord( ptr++, (ushort) val        );
                                    WriteWord( ptr++, (ushort)(val >> 16) );

                                    len--;
                                }

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( 0 );
                            }
                            break;
                    }
                }
            }
        }
    }
}
