//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

//#define ENABLE_TRACING
//#define ENABLE_TRACING_VERBOSE
//#define ENABLE_CACHE
//#define ENABLE_MMU
//#define ENABLE_STACKEDMEMORY
//#define ENABLE_ABORTHANDLERS

namespace Microsoft.iMote2Loader
{
    using System;
    using System.Collections.Generic;

    using RT     = Microsoft.Zelig.Runtime;
    using PXA27x = Microsoft.DeviceModels.Chipset.PXA27x;
    using ARMv4  = Microsoft.Zelig.Runtime.TargetPlatform.ARMv4;


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

#if ENABLE_CACHE
            ARMv4.Coprocessor15.InvalidateICache();
    
            ARMv4.Coprocessor15.InvalidateDCache();
    
            //
            // Enable ICache
            //
            ARMv4.Coprocessor15.SetControlRegisterBits( ARMv4.Coprocessor15.c_ControlRegister__ICache );
#endif

            PXA27x.ClockManager.Instance.InitializeClocks();

#if ENABLE_STACKEDMEMORY
            PXA27x.MemoryController.Instance.InitializeStackedSDRAM();
            PXA27x.MemoryController.Instance.InitializeStackedFLASH();
#endif

#if ENABLE_MMU
            ConfigureMMU();
#endif

            RT.Configuration.ExecuteApplication();
        }

#if ENABLE_MMU
        private static void ConfigureMMU()
        {
            ARMv4.MMUv4.ClearTLB();

            const uint baseFlash = 0x80000000u;
            const uint sizeFlash = 0x2000000;

            ARMv4.MMUv4.AddUncacheableSection( 0, 0 + 256 * 1024, 0x5C000000 );
            ARMv4.MMUv4.AddUncacheableSection( 0x5C000000, 0x5C000000 + 256 * 1024, 0x5C000000 );
            ARMv4.MMUv4.AddUncacheableSection( baseFlash, baseFlash + sizeFlash, 0 );

            ARMv4.MMUv4.EnableTLB();
        }
#endif

#if ENABLE_ABORTHANDLERS
        static uint fault_DFSR;
        static uint fault_IFSR;
        static uint fault_FAR;

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.UndefinedInstruction)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void UndefinedInstruction()
        {
            fault_DFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 0 );
            fault_IFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 1 );
            fault_FAR  = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 6, 0, 0 );

            Processor.Instance.Breakpoint();
        }

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.PrefetchAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void PrefetchAbort()
        {
            fault_DFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 0 );
            fault_IFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 1 );
            fault_FAR  = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 6, 0, 0 );

            Processor.Instance.Breakpoint();
        }

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.DataAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void DataAbort()
        {
            fault_DFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 0 );
            fault_IFSR = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 5, 0, 1 );
            fault_FAR  = ARMv4.ProcessorARMv4.MoveFromCoprocessor( 15, 0, 6, 0, 0 );

            Processor.Instance.Breakpoint();
        }
#endif
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
        const byte cmd_EndOfStream    = 0xFF;

        const uint FlashSize = 32 * 1024 * 1024;
        const uint FlashMask = ~(FlashSize - 1);

        const uint baseIRAM  = 0x5C000000;
        const uint blockSize = 64 * 1024;

        const uint baseHostToDevice = baseIRAM + blockSize * 1;
        const uint baseDeviceToHost = baseIRAM + blockSize * 2;

        //--//

        static unsafe uint* s_ptrInput;
        static unsafe uint* s_ptrOutput;

        //--//

        [RT.DisableNullChecks()]
        static unsafe void EnterCFI( ushort* baseAddress )
        {
            PXA27x.StackedFlashChip.EnterCFI( baseAddress );
        }

        [RT.DisableNullChecks()]
        static unsafe void ExitCFI( ushort* baseAddress )
        {
            PXA27x.StackedFlashChip.ExitCFI( baseAddress );
        }

        [RT.DisableNullChecks()]
        static unsafe void EraseSector( ushort* sectorStart )
        {
            PXA27x.StackedFlashChip.UnlockFlashSector( sectorStart );
            PXA27x.StackedFlashChip.EraseFlashSector ( sectorStart );
        }

        [RT.DisableNullChecks()]
        static unsafe void WriteWord( ushort* address ,
                                      ushort  value   )
        {
            PXA27x.StackedFlashChip.UnlockFlashSector( address        );
            PXA27x.StackedFlashChip.ProgramFlashWord ( address, value );
        }

        //--//

        static unsafe void RewindAndWait( bool fSetMarker )
        {
            s_ptrInput  = (uint*)baseHostToDevice;
            s_ptrOutput = (uint*)baseDeviceToHost;

            if(fSetMarker)
            {
                *s_ptrOutput = cmd_Signature | cmd_EndOfStream;
            }

            System.Diagnostics.Debugger.Break();
        }

        static unsafe uint ReadInput()
        {
            return *s_ptrInput++;
        }

        static unsafe void WriteOutput( uint val )
        {
            *s_ptrOutput++ = val;
        }

        //--//

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugInitialize()
        {
            PXA27x.UART.Instance.Configure( PXA27x.UART.Id.STUART, 115200 );
        }

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugPrint( string text )
        {
            PXA27x.UART.Instance.Ports[(int)PXA27x.UART.Id.STUART].DEBUG_WriteLine( text );
        }

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugPrint( string text  ,
                                uint   value )
        {
            PXA27x.UART.Instance.Ports[(int)PXA27x.UART.Id.STUART].DEBUG_WriteLine( text, value );
        }

        [System.Diagnostics.Conditional( "ENABLE_TRACING_VERBOSE" )]
        static void DebugPrintVerbose( string text  ,
                                       uint   value )
        {
            DebugPrint( text, value );
        }

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugChar( char c )
        {
            PXA27x.UART.Instance.Ports[(int)PXA27x.UART.Id.STUART].DEBUG_Write( c );
        }

        //--//

        [RT.DisableNullChecks( ApplyRecursively=true )]
        static unsafe void Main()
        {
////        var clockControl = PXA27x.ClockManager.Instance;
////
////        clockControl.CKEN.EnOsTimer = true;
////
////        //--//
////
////        var timer = PXA27x.OSTimers.Instance;
////
////        uint* ptr2 = (uint*)0xA0000000;
////
////        uint last  = timer.ReadCounter( 0 );
////        uint last2 = timer.ReadCounter( 0 );
////
////        for(uint i = 0; i < 1024; i++)
////        {
////            uint sum = 0;
////
////            for(int j = 0; j < 1024 * 16; j++)
////            {
////                sum += last;
////            }
////
////            uint val = timer.ReadCounter( 0 ); 
////            ptr2[i] = val - last2;
////            last2 = val;
////
////            last = sum;
////        }

            DebugInitialize();

            DebugPrint( "Hello World!\r\n" );

            RewindAndWait( true );

            while(true)
            {
                DebugPrint( "Waiting..." );
                uint cmd = ReadInput();
                DebugPrint( "Got data: ", cmd );

                if((cmd & cmd_Mask) == cmd_Signature)
                {
                    DebugPrint( "Got command: ", cmd );

                    switch((byte)cmd)
                    {
                        case cmd_EndOfStream:
                            WriteOutput( cmd );
                            RewindAndWait( false );
                            break;

                        case cmd_Hello:
                            WriteOutput( cmd );
                            break;

                        case cmd_EnterCFI:
                            EnterCFI( (ushort*)ReadInput() );

                            WriteOutput( cmd );
                            break;

                        case cmd_ExitCFI:
                            ExitCFI( (ushort*)ReadInput() );

                            WriteOutput( cmd );
                            break;

                        case cmd_ReadMemory8:
                            {
                                byte* ptr = (byte*)ReadInput();
                                uint  len =        ReadInput();

                                WriteOutput( cmd );

                                while(len > 0)
                                {
                                    WriteOutput( *ptr++ );
                                    len--;
                                }
                            }
                            break;

                        case cmd_ReadMemory16:
                            {
                                ushort* ptr = (ushort*)ReadInput();
                                uint    len =          ReadInput();

                                DebugPrint( "Address: ", (uint)ptr );
                                DebugPrint( "Length:  ",       len );

                                WriteOutput( cmd );

                                while(len > 0)
                                {
                                    DebugPrint( "Data: ", (uint)*ptr );
                                    WriteOutput( *ptr++ );
                                    len--;
                                }

                                DebugPrint( "Done." );
                            }
                            break;

                        case cmd_ReadMemory32:
                            {
                                uint* ptr = (uint*)ReadInput();
                                uint  len =        ReadInput();

                                DebugPrint( "Address: ", (uint)ptr );
                                DebugPrint( "Length:  ",       len );

                                WriteOutput( cmd );

                                while(len > 0)
                                {
                                    DebugPrint( "Data: ", (uint)*ptr );
                                    WriteOutput( *ptr++ );
                                    len--;
                                }

                                DebugPrint( "Done." );
                            }
                            break;

                        case cmd_ChecksumMemory:
                            {
                                uint* ptr = (uint*)ReadInput();
                                uint  len =        ReadInput();
                                uint  sum = 0;
                                uint  and = 0xFFFFFFFF;

                                WriteOutput( cmd );

                                DebugPrint( "Address: ", (uint)ptr );
                                DebugPrint( "Length:  ",       len );

                                while(len > 0)
                                {
                                    uint val = *ptr;

                                    sum = ((sum & 1) << 31) | (sum >> 1);

                                    sum += val;
                                    and &= val;

                                    ptr++;
                                    len--;
                                }

                                DebugPrint( "Sum: ", sum );
                                DebugPrint( "And: ", and );

                                WriteOutput( sum );
                                WriteOutput( and );
                            }
                            break;

                        case cmd_EraseSector:
                            EraseSector( (ushort*)ReadInput() );

                            WriteOutput( cmd );

                            WriteOutput( 0 );
                            break;

                        case cmd_ProgramMemory:
                            {
                                ushort* ptr = (ushort*)ReadInput();
                                uint    len =          ReadInput();

                                WriteOutput( cmd );

                                while(len > 0)
                                {
                                    if(len >= 16)
                                    {
                                        PXA27x.StackedFlashChip.UnlockFlashSector            ( ptr     );
                                        PXA27x.StackedFlashChip.StartBufferedProgramFlashWord( ptr, 32 );

                                        for(int i = 0; i < 16; i++)
                                        {
                                            uint val = ReadInput();

                                            PXA27x.StackedFlashChip.AddBufferedProgramFlashWord( ptr++, (ushort) val        );
                                            PXA27x.StackedFlashChip.AddBufferedProgramFlashWord( ptr++, (ushort)(val >> 16) );

                                            DebugPrintVerbose( "Program Data: ", val );
                                        }

                                        PXA27x.StackedFlashChip.ConfirmProgramFlashWord( ptr - 32 );

                                        len -= 16;
                                    }
                                    else
                                    {
                                        uint val = ReadInput();

                                        WriteWord( ptr++, (ushort) val        );
                                        WriteWord( ptr++, (ushort)(val >> 16) );

                                        len--;
                                    }
                                }

                                WriteOutput( 0 );
                            }
                            break;
                    }
                }
            }
        }
    }
}
