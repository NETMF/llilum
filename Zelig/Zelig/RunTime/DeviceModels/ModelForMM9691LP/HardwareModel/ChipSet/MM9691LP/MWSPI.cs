//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x380A0000U,Length=0x00000014U)]
    public class MWSPI
    {
        public enum SPI_CLOCK_RATE
        {
            Freq_13800KHZ = 2,     // STAN
            Freq_06900KHZ = 4,
            Freq_02300KHZ = 12,    // ADC7466
            Freq_01971KHZ = 14,    // AT25HP512 EEPROM
            Freq_00986KHZ = 28,
            Freq_00493KHZ = 56,    // AT25256 EEPROM
            Freq_00400KHZ = 69,
            Freq_00217KHZ = 127,   // slowest possible setting
        }

        public const uint c_MS1LE  = GPIO.c_Pin_07;
        public const uint c_MSK    = GPIO.c_Pin_16;
        public const uint c_MDIDO  = GPIO.c_Pin_17;
        public const uint c_MDODI  = GPIO.c_Pin_18;
        public const uint c_MSC0LE = GPIO.c_Pin_19;

        public const ushort MWnCTL1__MWEN             = 0x0001;
        public const ushort MWnCTL1__MNS_SLAVE        = 0x0000;
        public const ushort MWnCTL1__MNS_MASTER       = 0x0002;
        public const ushort MWnCTL1__MOD_8            = 0x0000;
        public const ushort MWnCTL1__MOD_16           = 0x0004;
        public const ushort MWnCTL1__ECHO             = 0x0008;
        public const ushort MWnCTL1__EIF              = 0x0010;
        public const ushort MWnCTL1__EIR              = 0x0020;
        public const ushort MWnCTL1__EIW              = 0x0040;
        public const ushort MWnCTL1__SCM_NORMAL       = 0x0000;
        public const ushort MWnCTL1__SCM_ALTERNATE    = 0x0080;
        public const ushort MWnCTL1__SCIDL_MSK0       = 0x0000;
        public const ushort MWnCTL1__SCIDL_MSK1       = 0x0100;

        public static ushort MWnCTL1__SCDV__set( uint d ) { return (ushort)((d & 0x7Fu) << 9); }

        public const ushort MWnSTAT__TBF              = 0x0001;
        public const ushort MWnSTAT__RBF              = 0x0002;
        public const ushort MWnSTAT__OVR              = 0x0004;
        public const ushort MWnSTAT__UDR              = 0x0008;
        public const ushort MWnSTAT__BSY              = 0x0010;

        public const ushort MWnCTL2__EDR              = 0x0001;
        public const ushort MWnCTL2__EDW              = 0x0002;
        public const ushort MWnCTL2__LEE0             = 0x0004;
        public const ushort MWnCTL2__LEE1             = 0x0008;
        public const ushort MWnCTL2__LEMD0            = 0x0010;
        public const ushort MWnCTL2__LEMD1            = 0x0020;
        public const ushort MWnCTL2__LEPL0            = 0x0040;
        public const ushort MWnCTL2__LEPL1            = 0x0080;
        public const ushort MWnCTL2__DTMD_FULL_DUPLEX = 0x0000;
        public const ushort MWnCTL2__DTMD_READ_ONLY   = 0x0100;
        public const ushort MWnCTL2__DTMD_WRITE_ONLY  = 0x0200;
        public const ushort MWnCTL2__FNCLE            = 0x0400;

        //--//

        [Register(Offset=0x00000000U)] public ushort MWnDAT;
        [Register(Offset=0x00000004U)] public ushort MWnCTL1;
        [Register(Offset=0x00000008U)] public ushort MWnSTAT;
        [Register(Offset=0x0000000CU)] public ushort MWnCTL2;
        [Register(Offset=0x00000010U)] public ushort MWnTEST;

        [Inline]
        public bool TransmitBufferFull()
        {
            return (MWnSTAT & MWnSTAT__TBF) != 0;
        }


        [Inline]
        public bool ReceiveBufferEmpty()
        {
            return (MWnSTAT & MWnSTAT__RBF) == 0;
        }

        [Inline]
        public bool ShiftBufferEmpty()
        {
            return (MWnSTAT & MWnSTAT__BSY) == 0;
        }

        public static uint ConvertClockRateToDivisor( uint Clock_RateKHz )
        {
            const uint SYSTEM_CLOCK_HZ  = 27600000;
            const uint SYSTEM_CLOCK_KHZ = SYSTEM_CLOCK_HZ / 1000;

            if     (Clock_RateKHz >= SYSTEM_CLOCK_KHZ                                     ) return 0;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_13800KHZ) return (uint)SPI_CLOCK_RATE.Freq_13800KHZ;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_06900KHZ) return (uint)SPI_CLOCK_RATE.Freq_06900KHZ;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_02300KHZ) return (uint)SPI_CLOCK_RATE.Freq_02300KHZ;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_01971KHZ) return (uint)SPI_CLOCK_RATE.Freq_01971KHZ;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_00986KHZ) return (uint)SPI_CLOCK_RATE.Freq_00986KHZ;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_00493KHZ) return (uint)SPI_CLOCK_RATE.Freq_00493KHZ;
            else if(Clock_RateKHz >= SYSTEM_CLOCK_KHZ / (uint)SPI_CLOCK_RATE.Freq_00400KHZ) return (uint)SPI_CLOCK_RATE.Freq_00400KHZ;
            else                                                                            return (uint)SPI_CLOCK_RATE.Freq_00217KHZ;
        }

        //--//

        public class SPI_CONFIGURATION
        {
            public uint DeviceCS;
            public bool CS_Active;             // False = LOW active,      TRUE = HIGH active
            public bool MSK_IDLE;              // False = LOW during idle, TRUE = HIGH during idle
            public bool MSK_SampleEdge;        // False = sample falling edge,  TRUE = samples on rising
            public bool MD_16bits;
            public uint Clock_RateKHz;
            public uint CS_Setup_uSecs;
            public uint CS_Hold_uSecs;
            public uint SPI_mod;
        }

        // only used in 1 place in RTM builds, so inline this for quicker access
        public static ushort ConfigurationToMode( SPI_CONFIGURATION Configuration )
        {
            ushort Mode;

            Mode = MWnCTL1__MWEN | MWnCTL1__MNS_MASTER;

            if(Configuration.MD_16bits)
            {
                Mode |= MWnCTL1__MOD_16;
            }
            else
            {
                Mode |= MWnCTL1__MOD_8;
            }

            if(Configuration.MSK_IDLE)
            {
                Mode |= MWnCTL1__SCIDL_MSK1;
            }
            else
            {
                Mode |= MWnCTL1__SCIDL_MSK0;
            }

            if(Configuration.MSK_SampleEdge)
            {
                Mode |= MWnCTL1__SCM_NORMAL;        // sample on rising edge
            }
            else
            {
                Mode |= MWnCTL1__SCM_ALTERNATE;     // sample on falling edge
            }

            Mode |= MWnCTL1__SCDV__set( ConvertClockRateToDivisor( Configuration.Clock_RateKHz ));

            return Mode;
        }

        //
        // Access Methods
        //

        public static extern MWSPI Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}