//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.Hosting
{
    using System;
    using System.Collections.Generic;


    public abstract class HalEvents
    {
        public const uint SYSTEM_EVENT_FLAG_COM1_IN             = 0x00000001;
        public const uint SYSTEM_EVENT_FLAG_COM1_OUT            = 0x00000002;
        public const uint SYSTEM_EVENT_FLAG_COM2_IN             = 0x00000004;
        public const uint SYSTEM_EVENT_FLAG_COM2_OUT            = 0x00000008;
        public const uint SYSTEM_EVENT_FLAG_TIMER0              = 0x00000010;
        public const uint SYSTEM_EVENT_FLAG_TIMER1              = 0x00000020;
        public const uint SYSTEM_EVENT_FLAG_TIMER2              = 0x00000040;
        public const uint SYSTEM_EVENT_FLAG_RADIO               = 0x00000080;
        public const uint SYSTEM_EVENT_FLAG_BUTTON              = 0x00000100;
        public const uint SYSTEM_EVENT_FLAG_TONE_COMPLETE       = 0x00000200;
        public const uint SYSTEM_EVENT_FLAG_TONE_BUFFER_EMPTY   = 0x00000400;
        public const uint SYSTEM_EVENT_FLAG_ALARM               = 0x00000800;
        public const uint SYSTEM_EVENT_FLAG_RSA_DECODE_COMPLETE = 0x00001000;
        public const uint SYSTEM_EVENT_FLAG_RSA_ENCODE_COMPLETE = 0x00002000;
        public const uint SYSTEM_EVENT_FLAG_UNUSED_0x00004000   = 0x00004000;
        public const uint SYSTEM_EVENT_FLAG_SPI_COMPLETE        = 0x00008000;
        public const uint SYSTEM_EVENT_FLAG_CHARGER_CHANGE      = 0x00010000;
        public const uint SYSTEM_EVENT_FLAG_APP_DEFINED_1       = 0x00020000;
        public const uint SYSTEM_EVENT_FLAG_APP_DEFINED_2       = 0x00040000;
        public const uint SYSTEM_EVENT_FLAG_APP_DEFINED_3       = 0x00080000;
        public const uint SYSTEM_EVENT_FLAG_APP_DEFINED_4       = 0x00100000;
        public const uint SYSTEM_EVENT_FLAG_VITERBI_DONE        = 0x00200000;
        public const uint SYSTEM_EVENT_FLAG_MAC_DONE            = 0x00400000;
        public const uint SYSTEM_EVENT_HEARTRATE                = 0x00800000;
        public const uint SYSTEM_EVENT_TS_INIT_DONE             = 0x01000000;
        public const uint SYSTEM_EVENT_AIRPRESSURE_DONE         = 0x02000000;
        public const uint SYSTEM_EVENT_FLAG_USB_IN              = 0x04000000;
        public const uint SYSTEM_EVENT_HW_INTERRUPT             = 0x08000000;
        public const uint SYSTEM_EVENT_I2C_XACTION              = 0x10000000;
        public const uint SYSTEM_EVENT_FLAG_UNUSED_0x20000000   = 0x20000000;
        public const uint SYSTEM_EVENT_FLAG_UNUSED_0x40000000   = 0x40000000;
        public const uint SYSTEM_EVENT_FLAG_UNUSED_0x80000000   = 0x80000000;
        public const uint SYSTEM_EVENT_FLAG_ALL                 = 0xFFFFFFFF;

        //--//

        public abstract void Clear( uint mask );

        public abstract void Set( uint mask );

        public abstract uint Get( uint mask );

        public abstract uint MaskedRead( uint mask );
    }
}
