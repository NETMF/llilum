//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

//#define ENABLE_TRACING
//#define ENABLE_TRACING_VERBOSE
//#define TEST_FP
//#define TEST_VFP
//#define TEST_STRUCT
//#define TEST_ARRAYBOUNDCHECKS
//#define DCC_TEST_HALFDUPLEX
//#define DCC_TEST_FULLDUPLEX


namespace Microsoft.NohauLPC3180Loader
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.LPC3180;

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

            //--//

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
        const byte cmd_GetConfig      = 0x02; //                               => <Page Count> <Page Size> <Usable Page Size> <Block Size>
        const byte cmd_ReadPage       = 0x03; // Arg: Address                  => <Status value> <values>
        const byte cmd_ReadSpare      = 0x04; // Arg: Address                  => <Status value> <values>
        const byte cmd_ChecksumMemory = 0x05; // Arg: Address                  => <Status value> <CRC value>, <AND of all memory>
        const byte cmd_EraseBlock     = 0x06; // Arg: Address                  => <Status value>
        const byte cmd_ProgramPage    = 0x07; // Arg: Address, # pages [bytes] => <Status value page0> .... <Status value pageN>

        //--//

        const int c_PageCount       = 65536;
        const int c_PageSize        = 512;
        const int c_SpareSize       = 16;
        const int c_UsableSpareSize = 6;
        const int c_BlockSize       = 16384;

        static readonly uint  [] s_Page  = new uint  [c_PageSize  / sizeof(uint  )];
        static readonly ushort[] s_Spare = new ushort[c_SpareSize / sizeof(ushort)];

        [RT.DisableNullChecks()]
        static unsafe void Initialize()
        {
            var flashclk = new ChipsetModel.SystemControl.FLASHCLK_CTRL_bitfield();
 
            flashclk.InterruptFromMLC = true;
            flashclk.MLC_ClockEnable  = true;

            ChipsetModel.SystemControl.Instance.FLASHCLK_CTRL = flashclk;

            //--//

            var ctrl = ChipsetModel.MultiLevelNANDController.Instance;

            ctrl.SetTimingRegister( 104, 45, 100, 30, 15, 30, 20, 40 );

            ctrl.MLC_CMD = 0xFF;
    
            while(ctrl.MLC_ISR.NandReady == false)
            {
            }

            {
                var val = new  ChipsetModel.MultiLevelNANDController.MLC_ICR_bitfield();

                val.ChipWordAddress = ChipsetModel.MultiLevelNANDController.MLC_ICR_bitfield.Addressing.ThreeWords;

                ctrl.MLC_LOCK_PR = ChipsetModel.MultiLevelNANDController.MLC_LOCK_PR__UnlockValue;
                ctrl.MLC_ICR     = val;
            }

            //--//

            DebugInitialize();
        }

        //--//

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugInitialize()
        {
            ChipsetModel.StandardUART.Instance.Configure( ChipsetModel.StandardUART.Id.UART5, false, 2 * 115200 );
        }

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugPrint( string text )
        {
            ChipsetModel.StandardUART.Instance.Ports[(int)ChipsetModel.StandardUART.Id.UART5].DEBUG_WriteLine( text );
        }

        [System.Diagnostics.Conditional( "ENABLE_TRACING" )]
        static void DebugPrint( string text  ,
                                uint   value )
        {
            ChipsetModel.StandardUART.Instance.Ports[(int)ChipsetModel.StandardUART.Id.UART5].DEBUG_WriteLine( text, value );
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
            ChipsetModel.StandardUART.Instance.Ports[(int)ChipsetModel.StandardUART.Id.UART5].DEBUG_Write( c );
        }

        //--//

////    [RT.DisableNullChecks()]
////    static unsafe uint ReadID()
////    {
////        var  ctrl = ChipsetModel.MultiLevelNANDController.Instance;
////        uint id   = 0;
////
////        ctrl.MLC_CMD  = 0x90;
////        ctrl.MLC_ADDR = 0;
////
////        id |= (uint)ctrl.MLC_DATA_8bits[0] << 24;
////        id |= (uint)ctrl.MLC_DATA_8bits[0] << 16;
////        id |= (uint)ctrl.MLC_DATA_8bits[0] <<  8;
////        id |= (uint)ctrl.MLC_DATA_8bits[0] <<  0;
////
////        return id;
////    }

        [RT.NoInline]
        static float ComputeTest1( float a ,
                                   float b )
        {
            float res;

            res  = a * b;
            res += a + b;
            res += a - b;
            res += a / b;

            return res;
        }

        [RT.NoInline]
        static double ComputeTest2( double a ,
                                    double b )
        {
            double res;

            res  = a * b;
            res += a + b;
            res += a - b;
            res += a / b;

            return res;
        }

        [RT.NoInline]
        static int ComputeTest3( int a )
        {
            float tmp = a;

            return (int)(tmp * 3.5);
        }

        [RT.NoInline]
        static long ComputeTest4( long a )
        {
            float tmp = a;

            return (long)(tmp * 3.5);
        }

        [RT.NoInline]
        static int ComputeTest5( int a )
        {
            double tmp = a;

            return (int)(tmp * 3.5);
        }

        [RT.NoInline]
        static long ComputeTest6( long a )
        {
            double tmp = a;

            return (long)(tmp * 3.5);
        }

        [RT.NoInline]
        static float ComputeTest7( int a )
        {
            int tmp;

            ComputeTest8( a, out tmp );

            return (float)tmp;
        }

        [RT.NoInline]
        static void ComputeTest8(     int a ,
                                  out int b )
        {
            b = a;
        }

        [RT.NoInline]
        [RT.SaveFullProcessorContext]
        private static void TestPushContext()
        {
        }

        [RT.DisableNullChecks()]
        static unsafe bool ReadPage( uint address )
        {
            var ctrl = ChipsetModel.MultiLevelNANDController.Instance;

            DebugPrint( "Read Page: ", address );

            uint pageAddress = address / 512;

            ctrl.MLC_CMD              = 0x00; // Select Page A
            ctrl.MLC_ADDR             = 0;
            ctrl.MLC_ADDR             = (pageAddress >>  0) & 0xFF;
            ctrl.MLC_ADDR             = (pageAddress >>  8) & 0xFF;
////        ctrl.MLC_ADDR             = (pageAddress >> 16) & 0xFF; // Used for 4-word address chips.
            ctrl.MLC_ECC_AUTO_DEC_REG = 0;

            //--//

            while(true)
            {
                var val = ctrl.MLC_ISR;

                if(val.ControllerReady == false)
                {
                    continue;
                }

                if(val.DecoderFailure)
                {
                    DebugPrint( "Failed: ", val.SymbolErrors );

                    for(int i = 0; i < s_Page.Length; i++)
                    {
                        s_Page[i] = ctrl.MLC_BUFF_32bits[0];
                    }

                    for(int i = 0; i < s_Spare.Length; i++)
                    {
                        s_Spare[i] = ctrl.MLC_BUFF_16bits[0];
                    }

                    return false;
                }

                break;
            }

            for(int i = 0; i < s_Page.Length; i++)
            {
                s_Page[i] = ctrl.MLC_BUFF_32bits[0];

////            DebugPrint( "Data: ", s_Page[i] );
            }

            for(int i = 0; i < s_Spare.Length; i++)
            {
                s_Spare[i] = ctrl.MLC_BUFF_16bits[0];
            }

            return true;
        }

        [RT.DisableNullChecks()]
        static unsafe bool ProgramPage( uint address )
        {
            var ctrl = ChipsetModel.MultiLevelNANDController.Instance;

            DebugPrint( "Program Page: ", address );

            uint pageAddress = address / 512;

            ctrl.MLC_CMD         = 0x00; // Select Page A
            ctrl.MLC_CMD         = 0x80; // Page Program Setup Code
            ctrl.MLC_ADDR        = 0;
            ctrl.MLC_ADDR        = (pageAddress >>  0) & 0xFF;
            ctrl.MLC_ADDR        = (pageAddress >>  8) & 0xFF;
////        ctrl.MLC_ADDR        = (pageAddress >> 16) & 0xFF; // Used for 4-word address chips.
            ctrl.MLC_ECC_ENC_REG = 0; // Start encoding.

            for(int i = 0; i < s_Page.Length; i++)
            {
                ctrl.MLC_BUFF_32bits[0] = s_Page[i];
            }

            for(int i = 0; i < c_UsableSpareSize / sizeof(ushort); i++)
            {
                ctrl.MLC_BUFF_16bits[0] = s_Spare[i];
            }

            ctrl.MLC_ECC_AUTO_ENC_REG = 0x110; // Auto-Program

            //--//

            while(ctrl.MLC_ISR.ControllerReady == false)
            {
            }

            //--//

            ctrl.MLC_CMD = 0x70; // Read Status Register

            uint status = ctrl.MLC_DATA_8bits[0];

            DebugPrint( "Status: ", status );

            return (status & 1) == 0;
        }

        [RT.DisableNullChecks()]
        static unsafe bool EraseBlock( uint address )
        {
            var ctrl = ChipsetModel.MultiLevelNANDController.Instance;

            DebugPrint( "Erase Block: ", address );

            uint pageAddress = address / 512;

            ctrl.MLC_CMD  = 0x60; // Block Erase Setup Code
            ctrl.MLC_ADDR = (pageAddress >>  0) & 0xFF;
            ctrl.MLC_ADDR = (pageAddress >>  8) & 0xFF;
////        ctrl.MLC_ADDR = (pageAddress >> 16) & 0xFF; // Used for 4-word address chips.
            ctrl.MLC_CMD  = 0xD0; // Confirm Code

            //--//

            while(ctrl.MLC_ISR.ControllerReady == false)
            {
            }

            //--//

            ctrl.MLC_CMD = 0x70; // Read Status Register

            uint status = ctrl.MLC_DATA_8bits[0];

            DebugPrint( "Status: ", status );

            return (status & 1) == 0;
        }

        //--//

#if TEST_FP
        static float  t1;
        static double t2;
        static int    t3;
        static long   t4;
        static int    t5;
        static long   t6;
        static float  t7;
#endif

#if TEST_STRUCT
        public struct Holder : IDisposable
        {
            //
            // State
            //

            object   m_thread;
            long     m_timeout;
            ushort[] m_array;

            //
            // Constructor Methods
            //

            public Holder( object   thread  ,
                           long     timeout ,
                           ushort[] array   )
            {
                m_thread  = thread;
                m_timeout = timeout;
                m_array   = array;
            }

            //
            // Helper Methods
            //


            public void Dispose()
            {
                if(m_array != null)
                {
                    m_thread = null;
                }
            }

            //
            // Access Methods
            //

            public bool RequestFulfilled
            {
                [RT.NoInline]
                get
                {
                    return m_array != null;
                }
            }
        }

        [RT.NoInline]
        private static void TestStack()
        {
            using(Holder holder = new Holder( s_Page, 0, s_Spare ))
            {
                if(holder.RequestFulfilled)
                {
                    TestStack_Sub();
                }
            }
        }

        [RT.NoInline]
        private static void TestStack_Sub()
        {
        }
#endif

#if TEST_ARRAYBOUNDCHECKS

        public static uint Sqrt( uint v )
        {
            uint r = 0, s = 0;

            for (int i = 15; i >= 0; i--)
            {
                s = r + (uint)(1 << i * 2);
                r >>= 1;
                if (s <= v)
                {
                    v -= s;
                    r |= (uint)(1 << i * 2);
                }
            }

            return r;
        }

        private static void QuickSort( int[] keys, object[] values, int left, int right )
        {
            do
            {
                int i = left;
                int j = right;

                // pre-sort the low, middle (pivot), and high values in place.
                // this improves performance in the face of already sorted data, or 
                // data that is made up of multiple sorted runs appended together.
                int middle = i + ((j - i) >> 1);

                int x = keys[middle];
                do
                {
                    while(keys[i] < x      ) i++;
                    while(x       < keys[j]) j--;

                    if(i > j) break;
                    if(i < j)
                    {
                        int key = keys[i];
                        keys[i] = keys[j];
                        keys[j] = key;
                        if(values != null)
                        {
                            object value = values[i];
                            values[i] = values[j];
                            values[j] = value;
                        }
                    }
                    i++;
                    j--;
                } while(i <= j);

                if(j - left <= right - i)
                {
                    if(left < j) QuickSort( keys, values, left, j );
                    left = i;
                }
                else
                {
                    if(i < right) QuickSort( keys, values, i, right );
                    right = j;
                }
            } while(left < right);
        }

        static int[] TestArrayBoundChecks( int[] a  ,
                                           int   c1 ,
                                           int   c2 )
        {
////        c1 = c1 / c2;

            //QuickSort( a, null, 0, a.Length - 1 );

            //Array.Sort( a, delegate( int x, int y ) { return x.CompareTo( y ); } );

////        if(a.Length >= 6)
////        {
////            int limit = a.Length - 2;
////            for(int i = 0; i < limit; i++)
                for(int i = 0; i < a.Length; i++)
                {
                    a[i] *= 2;
                }

                for(int i = a.Length; --i >= 0;)
                {
                    a[i] *= 3;
                }
////        }

            return a;
        }
#endif

#if TEST_VFP
        static float[] s_a = new float[32];
        static float[] s_b = new float[32];

        static unsafe float AccumulateRowByColumn( float[] A       ,
                                                   int     Aoffset ,
                                                   float[] B       ,
                                                   int     Boffset ,
                                                   int     N       )
        {
            fixed(float* Abase = &A[Aoffset])
            {
                fixed(float* Bbase = &B[Boffset])
                {
                    const int VectorStep = 8;

                    int Nrounded = N & ~(VectorStep-1);

                    float res = MultiplyAndAccumulate( Abase, Bbase, Nrounded, VectorStep );

                    if(Nrounded < N)
                    {
                        float* Aptr = &Abase[Nrounded];
                        float* Bptr = &Bbase[Nrounded];

                        while(Nrounded < N)
                        {
                            res += *Aptr++ * *Bptr++;

                            Nrounded++;
                        }
                    }

                    return res;
                }
            }
        }

        [TS.WellKnownMethod( "Solo_DSP_MatrixMultiply__MultiplyAndAccumulate" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static unsafe extern float MultiplyAndAccumulate( float* Abase      ,
                                                                  float* Bbase      ,
                                                                  int    N          ,
                                                                  int    vectorSize );
#endif

        [RT.DisableNullChecks( ApplyRecursively=true )]
        static unsafe void Main()
        {
#if TEST_STRUCT

            TestStack();

#elif TEST_FP

            Processor.EnableRunFastMode();

            TestPushContext();
    
            t1 = ComputeTest1( 1.0f, 2.0f );
            t2 = ComputeTest2( 1.0 , 2.0  );
            t3 = ComputeTest3( 100        );
            t4 = ComputeTest4( 100        );
            t5 = ComputeTest5( 100        );
            t6 = ComputeTest6( 100        );
            t7 = ComputeTest7( 100        );

#elif TEST_VFP

            Processor.EnableRunFastMode();

            for(int i = 0; i < 16; i++)
            {
                s_a[i] = i;
                s_b[i] = i;
            }

            float sum = AccumulateRowByColumn( s_a, 0, s_b, 0, 16 );

#elif TEST_ARRAYBOUNDCHECKS

            Sqrt( 30 );

            int[] a = new int[] { 1, 2, 3, 4, 5 };

            a = TestArrayBoundChecks( a, 2, 3 );

#elif DCC_TEST_HALFDUPLEX

            while(true)
            {
                uint cmd      = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                uint numItems = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
    
                if(cmd == 1)
                {
                    uint Sum = 0;
    
                    for(uint u = 0; u < numItems; u++)
                    {
                        uint data = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
    
                        Sum += data;
                    }
    
                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( Sum );
                }
                else
                {
                    for(uint u = 0; u < numItems; u++)
                    {
                        RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( u );
                    }
                }
            }

#elif DCC_TEST_FULLDUPLEX

            uint counter  = 0;
            uint words    = 0;
            uint lastWord = 0;
    
            while(true)
            {
                if(RT.TargetPlatform.ARMv4.Coprocessor14.CanWriteDCC())
                {
                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDebugCommData( (lastWord << 16) | (words << 8) | counter );
    
                    counter++;
                    counter &= 0xFF;
                }
    
                if(RT.TargetPlatform.ARMv4.Coprocessor14.CanReadDCC())
                {
                    lastWord = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDebugCommData();
                    words++;
                    words &= 0xFF;
    
                    while(RT.TargetPlatform.ARMv4.Coprocessor14.CanWriteDCC() == false)
                    {
                    }
                }
            }

#else

            Initialize();

            DebugPrint( "NOHAU Loader Started" );

////        ReadPage( c_PageSize * 0 );
////        ReadPage( c_PageSize * 1 );
////        ReadPage( c_PageSize * 2 );
////        ReadPage( c_PageSize * 3 );
////    
////        EraseBlock( c_PageSize * 128 );
////        ReadPage  ( c_PageSize * 128 );
////
////        ReadPage   ( c_PageSize * 1   );
////        ProgramPage( c_PageSize * 128 );
////        ReadPage   ( c_PageSize * 128 );

            while(true)
            {
                uint cmd = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                if((cmd & cmd_Mask) == cmd_Signature)
                {
                    DebugPrint( "Got command: ", cmd );

                    switch((byte)cmd)
                    {
                        case cmd_Hello:
                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );
                            break;

                        case cmd_GetConfig:
                            {
                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( c_PageCount       );
                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( c_PageSize        );
                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( c_SpareSize       );
                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( c_UsableSpareSize );
                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( c_BlockSize       );
                            }
                            break;

                        case cmd_ReadPage:
                        case cmd_ReadSpare:
                            {
                                uint address = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                bool fSuccess = ReadPage( address );

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( fSuccess ? 1u : 0u );

                                if((byte)cmd == cmd_ReadPage)
                                {
                                    for(int i = 0; i < s_Page.Length; i++)
                                    {
                                        RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( s_Page[i] );
                                    }
                                }

                                for(int i = 0; i < s_Spare.Length; i += 2)
                                {
                                    uint low  = s_Spare[i];
                                    uint high = s_Spare[i+1];
                                    uint val  = (high << 16) | low;

                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( val );
                                }
                            }
                            break;

                        case cmd_ChecksumMemory:
                            {
                                uint address  = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint len      = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint sum      = 0;
                                uint and      = 0xFFFFFFFF;
                                bool fSuccess = true;

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                DebugPrint( "Address: ", address );
                                DebugPrint( "Length:  ", len     );

                                while(len > 0)
                                {
                                    uint pageAddress = address & ~(uint)(c_PageSize - 1);

                                    fSuccess = ReadPage( pageAddress );
                                    if(fSuccess == false)
                                    {
                                        break;
                                    }

                                    uint offset = (address % c_PageSize);

                                    while(len > 0 && offset < c_PageSize)
                                    {
                                        uint val = s_Page[offset / sizeof(uint)];

                                        sum = ((sum & 1) << 31) | (sum >> 1);

                                        sum += val;

                                        address += sizeof(uint);
                                        offset  += sizeof(uint);
                                        len     -= 1;
                                    }

                                    for(int i = 0; i < s_Page.Length; i++)
                                    {
                                        and &= s_Page[i];
                                    }

                                    for(int i = 0; i < c_UsableSpareSize; i += 2)
                                    {
                                        uint low  = s_Spare[i];
                                        uint high = s_Spare[i+1];
                                        uint val  = (high << 16) | low;

                                        and &= val;
                                    }
                                }

                                if(fSuccess)
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( 1 );

                                    DebugPrint( "Sum: ", sum );
                                    DebugPrint( "And: ", and );

                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( sum );
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( and );
                                }
                                else
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( 0 );
                                }
                            }
                            break;

                        case cmd_EraseBlock:
                            {
                                uint address = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                bool fSuccess = EraseBlock( address );

                                if(fSuccess)
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( 1 );
                                }
                                else
                                {
                                    RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( 0 );
                                }
                            }
                            break;
    
                        case cmd_ProgramPage:
                            {
                                uint address = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
                                uint pages   = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( cmd );

                                bool fSuccess = true;

                                for(int page = 0; page < pages; page++)
                                {
                                    for(int i = 0; i < s_Page.Length; i++)
                                    {
                                        s_Page[i] = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                        DebugPrintVerbose( "Program Data: ", s_Page[i] );
                                    }

                                    for(int i = 0; i < s_Spare.Length; i += 2)
                                    {
                                        uint val = RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();

                                        ushort low  = (ushort)(val      );
                                        ushort high = (ushort)(val >> 16);

                                        s_Spare[i  ] = low;
                                        s_Spare[i+1] = high;

                                        DebugPrintVerbose( "Program Spare: ", s_Spare[i  ] );
                                        DebugPrintVerbose( "Program Spare: ", s_Spare[i+1] );
                                    }

                                    fSuccess &= ProgramPage( address );

                                    address += c_PageSize;
                                }

                                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( fSuccess ? 1u : 0u );
                            }
                            break;
                    }
                }
            }
#endif
        }
    }
}
