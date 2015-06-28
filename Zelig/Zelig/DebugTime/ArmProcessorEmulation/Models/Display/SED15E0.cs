//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define PROFILE_FRAMERATE

namespace Microsoft.Zelig.Emulation.ArmProcessor.Display
{
    using System;
    using System.Collections.Generic;

    using EncDef       = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using ElementTypes = Microsoft.Zelig.MetaData.ElementTypes;

    //--//

    [Simulator.PeripheralRange(Base=0x20000000U,Length=0x00020000U,Latency=3)]
    [Simulator.PeripheralRange(Base=0xA0000000U,Length=0x00020000U,Latency=3)]
    public class SED15E0 : Simulator.Peripheral
    {
        public const uint CONTROL_DISPLAY_OFF                     = 0xAE;
        public const uint CONTROL_DISPLAY_ON                      = 0xAF;

        public const uint CONTROL_START_LINE_SET                  = 0x8A;
        // following by control write of start line 0-95 decimal

        public const uint CONTROL_PAGE_ADDRESS_SET                = 0xB0;
        // with page address 0-12 in lower 4 bits

        public const uint CONTROL_COLUMN_ADDRESS_HI_NIBBLE_SET    = 0x10;
        public const uint CONTROL_COLUMN_ADDRESS_LO_NIBBLE_SET    = 0x00;
        // in sequence, high nibble, low nibble

        public const uint CONTROL_ADC_NORMAL                      = 0xA0;
        public const uint CONTROL_ADC_REVERSE                     = 0xA1;

        public const uint CONTROL_COMMON_SCAN_MODE_NORMAL         = 0xC0;
        public const uint CONTROL_COMMON_SCAN_MODE_REVERSE        = 0xC8;

        public const uint CONTROL_DISPLAY_NORMAL                  = 0xA6;
        public const uint CONTROL_DISPLAY_REVERSE                 = 0xA7;

        public const uint CONTROL_DISPLAY_ALL_POINTS_OFF          = 0xA4;
        public const uint CONTROL_DISPLAY_ALL_POINTS_ON           = 0xA5;

        public const uint CONTROL_nLINE_INVERSION_DRIVE           = 0x30;
        // lower 4 bits are No. of inverse lines, 0,8,12,...60,64 for 0-15 value
        public const uint CONTROL_nLINE_INVERSION_CANCEL          = 0xE4;

        public const uint CONTROL_DUTY_RATIO_SET                  = 0x60;
        // lower 5 bits are duty ratio, from 0=1/8 to 0x18=1/100
        // 1/100 is default after reset
////    public const uint DUTY_RATIO(r)                       ((r >> 2) - 1)
        public const uint DUTY_RATIO_008_TO_1                     = 0x01;
        public const uint DUTY_RATIO_012_TO_1                     = 0x02;
        public const uint DUTY_RATIO_016_TO_1                     = 0x03;
        public const uint DUTY_RATIO_020_TO_1                     = 0x04;
        public const uint DUTY_RATIO_024_TO_1                     = 0x05;
        public const uint DUTY_RATIO_028_TO_1                     = 0x06;
        public const uint DUTY_RATIO_032_TO_1                     = 0x07;
        public const uint DUTY_RATIO_036_TO_1                     = 0x08;
        public const uint DUTY_RATIO_040_TO_1                     = 0x09;
        public const uint DUTY_RATIO_044_TO_1                     = 0x0A;
        public const uint DUTY_RATIO_048_TO_1                     = 0x0B;
        public const uint DUTY_RATIO_052_TO_1                     = 0x0C;
        public const uint DUTY_RATIO_056_TO_1                     = 0x0D;
        public const uint DUTY_RATIO_060_TO_1                     = 0x0E;
        public const uint DUTY_RATIO_064_TO_1                     = 0x0F;
        public const uint DUTY_RATIO_068_TO_1                     = 0x10;
        public const uint DUTY_RATIO_072_TO_1                     = 0x11;
        public const uint DUTY_RATIO_076_TO_1                     = 0x12;
        public const uint DUTY_RATIO_080_TO_1                     = 0x13;
        public const uint DUTY_RATIO_084_TO_1                     = 0x14;
        public const uint DUTY_RATIO_088_TO_1                     = 0x15;
        public const uint DUTY_RATIO_092_TO_1                     = 0x16;
        public const uint DUTY_RATIO_096_TO_1                     = 0x17;
        public const uint DUTY_RATIO_100_TO_1                     = 0x18;
        // then followed by Starting Point Block Register Set,
        // lower 5 bits are used only, 0 = COM0 to 3, 0x17=COM92 to 95

        public const uint CONTROL_READ_MODIFY_WRITE               = 0xE0;
        public const uint CONTROL_READ_MODIFY_WRITE_END           = 0xEE;

        public const uint CONTROL_POWER_CONTROL_SET               = 0x20;
        // lower 4 bits are:
        public const uint POWER_BOOSTER_CIRCUIT_DISABLED          = 0x08;
        public const uint POWER_FIRST_BOOSTER_CIRCUIT_ON          = 0x04;
        public const uint POWER_VLCD_REGULATOR_CIRCUIT_ON         = 0x02;
        public const uint POWER_VLCD_OUTPUT_ON                    = 0x01;

        public const uint CONTROL_ELECTRONIC_VOLUME_MODE_SET      = 0x81;
        // next byte is the volume, from 0x00 (small) to 0x7f (large)

        public const uint CONTROL_DISCHARGE_ON                    = 0xEA;
        public const uint CONTROL_DISCHARGE_OFF                   = 0xEB;

        public const uint CONTROL_POWER_SAVE_ON                   = 0xA9;
        public const uint CONTROL_POWER_SAVE_OFF                  = 0xE1;

        public const uint CONTROL_OSCILLATOR_CIRCUIT_OFF          = 0xAA;
        public const uint CONTROL_OSCILLATOR_CIRCUIT_ON           = 0xAB;

        public const uint CONTROL_OSCILLATOR_FREQUENCY_SET        = 0x50;
        // lower 4 bits are 1 of 16 frequencies from 0=40kHz, to 0x0f=24.5kHz
        public const uint OSCILLATOR_FREQUENCY_400                = 0x00;
        public const uint OSCILLATOR_FREQUENCY_330                = 0x01;
        public const uint OSCILLATOR_FREQUENCY_284                = 0x02;
        public const uint OSCILLATOR_FREQUENCY_245                = 0x03;
        // fOSC/2                                             
        public const uint OSCILLATOR_FREQUENCY_200                = 0x04;
        public const uint OSCILLATOR_FREQUENCY_165                = 0x05;
        public const uint OSCILLATOR_FREQUENCY_142                = 0x06;
        public const uint OSCILLATOR_FREQUENCY_123                = 0x07;
        // fOSC/4                                             
        public const uint OSCILLATOR_FREQUENCY_100                = 0x08;
        public const uint OSCILLATOR_FREQUENCY_082                = 0x09;
        public const uint OSCILLATOR_FREQUENCY_071                = 0x0A;
        public const uint OSCILLATOR_FREQUENCY_061                = 0x0B;
        // fOSC/8                                             
        public const uint OSCILLATOR_FREQUENCY_050                = 0x0C;
        public const uint OSCILLATOR_FREQUENCY_041                = 0x0D;
        public const uint OSCILLATOR_FREQUENCY_036                = 0x0E;
        public const uint OSCILLATOR_FREQUENCY_031                = 0x0F;

        public const uint CONTROL_TEMPERATURE_GRADIENT_SET        = 0x40;
        // lower 3 bits are 1 of 8 gradients from 0=-0.06%/C, to 0x07=-0.18%/C
        public const uint TEMPERATURE_GRADIENT_06                 = 0x00;
        public const uint TEMPERATURE_GRADIENT_08                 = 0x01;
        public const uint TEMPERATURE_GRADIENT_10                 = 0x02;
        public const uint TEMPERATURE_GRADIENT_11                 = 0x03;
        public const uint TEMPERATURE_GRADIENT_13                 = 0x04;
        public const uint TEMPERATURE_GRADIENT_15                 = 0x05;
        public const uint TEMPERATURE_GRADIENT_17                 = 0x06;
        public const uint TEMPERATURE_GRADIENT_18                 = 0x07;

        public const uint CONTROL_RESET                           = 0xE2;
        public const uint CONTROL_NOOP                            = 0xE3;

        public const uint STARTING_BLOCK                          = 0x00;
        public const uint START_LINE                              = 0x00;

        //--//

        const int SCREEN_WIDTH  = 120;
        const int SCREEN_HEIGHT = 96;

        const int c_WidthInWords = (SCREEN_WIDTH + 31) / 32;
        const int c_SizeInWords  = (SCREEN_WIDTH + 31) / 32 * SCREEN_HEIGHT;

        //--//

        [Simulator.Register(Offset=0)] public byte Control
        {
            set
            {
                switch(value & 0xF0u)
                {
                    case CONTROL_PAGE_ADDRESS_SET:
                        m_page = (int)(value & 0x0F);
#if PROFILE_FRAMERATE
                        if(m_page == 0)
                        {
                            Hosting.OutputSink sink;

                            if(m_owner.GetHostingService( out sink ))
                            {
                                sink.OutputLine( "Clocks per iteration: {0}", m_owner.ClockTicks - lastPass );
                            }

                            lastPass = m_owner.ClockTicks;
                        }
#endif
                        break;

                    case CONTROL_COLUMN_ADDRESS_HI_NIBBLE_SET:
                        m_column = (m_column & 0x0F) | (int)((value & 0x0F) << 4);
                        break;

                    case CONTROL_COLUMN_ADDRESS_LO_NIBBLE_SET:
                        m_column = (m_column & 0xF0) | (int)((value & 0x0F) << 0);
                        break;
                }
            }
        }

        [Simulator.Register(Offset=1)] public byte Data
        {
            set
            {
                int  offset  = m_page * c_WidthInWords * 8 + (m_column / 32);
                uint maskBit = 1U << (m_column % 32);

                for(int pos = 0; pos < 8; pos++)
                {
                    if((value & (1 << pos)) != 0)
                    {
                        m_buffer[offset] |= maskBit;
                    }
                    else
                    {
                        m_buffer[offset] &= ~maskBit;
                    }

                    offset += c_WidthInWords;
                }

                if((++m_column % 8) == 0)
                {
                    Hosting.SED15E0Sink sink;

                    if(m_owner.GetHostingService( out sink ))
                    {
                        sink.NewScreenShot( m_buffer, SCREEN_WIDTH, SCREEN_HEIGHT );
                    }
                }
            }
        }

        //--//

        //
        // State
        //

        private int    m_page   = 0;
        private int    m_column = 0;
        private uint[] m_buffer = new uint[c_SizeInWords];

#if PROFILE_FRAMERATE
        static ulong lastPass;
#endif
    }
}
