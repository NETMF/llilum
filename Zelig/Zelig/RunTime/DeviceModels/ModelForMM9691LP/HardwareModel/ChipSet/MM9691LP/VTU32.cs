//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x38030000U,Length=0x00000050U)]
    public class VTU32
    {
        [MemoryMappedPeripheral(Base=0x0000U,Length=0x004U)]
        public class IO_CONTROL
        {
            public const uint CxyEDG_000   = 0x00000000;
            public const uint CxyEDG_001   = 0x00000001;
            public const uint CxyEDG_010   = 0x00000002;
            public const uint CxyEDG_011   = 0x00000003;
            public const uint CxyEDG_100   = 0x00000004;
            public const uint CxyEDG_101   = 0x00000005;
            public const uint CxyEDG_110   = 0x00000006;
            public const uint CxyEDG_111   = 0x00000007;
            public const uint PxyPOL_RESET = 0x00000000;
            public const uint PxyPOL_SET   = 0x00000008;

            [Register(Offset=0x00000000U)] public uint Value;
               
            // this puts the mode bits M for the chosen timer T (0-1), A/B (0-1) of a pair into the correct nibble/byte
            public static uint Set( uint T ,
                                    uint A ,
                                    uint M )
            {
                return  M << (int)(A*4 + T*8);
            }

            public static uint Get( uint T ,
                                    uint A ,
                                    uint M )
            {
                return (M >> (int)(A*4 + T*8)) & 0x000F;
            }
        }

        [MemoryMappedPeripheral(Base=0x0000U,Length=0x001CU)]
        public class CHANNEL_PAIR
        {
            [MemoryMappedPeripheral(Base=0x0000U,Length=0x000CU)]
            public class CHANNEL
            {
                [Register(Offset=0x00000000U)] public uint Counter;
                [Register(Offset=0x00000004U)] public uint PeriodCapture;
                [Register(Offset=0x00000008U)] public uint DutyCycleCapture;
            };

            [Register(Offset=0x00000000U)]             public uint      ClockPrescalar;
            [Register(Offset=0x00000004U,Instances=2)] public CHANNEL[] Channel;

            //--//

            public uint ClockPrescalar__get( uint T )
            {
                return (ClockPrescalar >> (int)(T*8)) & 0x00FF;
            }

            public void ClockPrescalar__set( uint T ,
                                             uint P )
            {
                uint val = ClockPrescalar;

                val &= ~(     0x000000FFu  << (int)(T*8));
                val |=  ((P & 0x000000FFu) << (int)(T*8));

                ClockPrescalar = val;
            }
        }

        //--//

        public const uint c_TIO1A = GPIO.c_Pin_20;
        public const uint c_TIO2A = GPIO.c_Pin_21;
        public const uint c_TIO3A = GPIO.c_Pin_22;
        public const uint c_TIO4A = GPIO.c_Pin_23;

        public const uint c_TIO1B = GPIO.c_Pin_05;
        public const uint c_TIO2B = GPIO.c_Pin_06;
        public const uint c_TIO3B = GPIO.c_Pin_13;
        public const uint c_TIO4B = GPIO.c_Pin_14;

        //--//

        public const uint ModeControl__TxARUN              = 0x00000001;
        public const uint ModeControl__TxBRUN              = 0x00000002;
        public const uint ModeControl__TMODx_LOW_POWER     = 0x00000000;
        public const uint ModeControl__TMODx_DUAL_PWM16    = 0x00000004;
        public const uint ModeControl__TMODx_PWM32         = 0x00000008;
        public const uint ModeControl__TMODx_CAPTURE       = 0x0000000C;

        public const uint Interrupt__Ix_None               = 0x00000000;
        public const uint Interrupt__Ix_1                  = 0x00000001;
        public const uint Interrupt__Ix_2                  = 0x00000002;
        public const uint Interrupt__Ix_3                  = 0x00000004;
        public const uint Interrupt__Ix_4                  = 0x00000008;
        public const uint Interrupt__Ix_ALL                = (Interrupt__Ix_1 | Interrupt__Ix_2 | Interrupt__Ix_3 | Interrupt__Ix_4);

        public const uint ExternalClockSelectRegister__CK1 = 0x00000001;
        public const uint ExternalClockSelectRegister__CK2 = 0x00000002;
        public const uint ExternalClockSelectRegister__CK3 = 0x00000004;
        public const uint ExternalClockSelectRegister__CK4 = 0x00000008;

        //--//

        [Register(Offset=0x00000000U)]             public uint           ModeControl;
        [Register(Offset=0x00000004U,Instances=2)] public IO_CONTROL[]   IOControl;
        [Register(Offset=0x0000000CU)]             public uint           InterruptControl;
        [Register(Offset=0x00000010U)]             public uint           InterruptPending;
        [Register(Offset=0x00000014U,Instances=2)] public CHANNEL_PAIR[] ChannelPair;
        [Register(Offset=0x0000004CU)]             public uint           ExternalClockSelectRegister;

        //--//

        // this puts the mode bits M for the chosen timer T (0-3) into the correct nibble
        public static uint ModeControl__set( uint T ,
                                             uint M )
        {
            return M << (int)(T*4);
        }

        public static uint ModeControl__get( uint T ,
                                             uint M )
        {
            return (M >> (int)(T*4)) & 0x000F;
        }

        // this puts the INT bits I for the chosen timer T (0-3) into the correct nibble
        public static uint Interrupt__set( uint T ,
                                           uint I )
        {
            return I << (int)(T*4);
        }

        public static uint Interrupt__get( uint T ,
                                           uint I )
        {
            return (I >> (int)(T*4)) & 0x000F;
        }

        //
        // Access Methods
        //

        public static extern VTU32 Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}