//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.MM9691LP
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x380C0000U,Length=0x00000020U)]
    public class RTC
    {
        public const uint WDCTL__DIS                        = 0x00000000;
        public const uint WDCTL__EN                         = 0x00000001;
        public const uint WDCTL__LK                         = 0x00000002;
        public const uint WDCTL__IEN                        = 0x00000004;
        public const uint WDCTL__REN                        = 0x00000008;
        public const uint WDCTL__WDOG_PIN_DISABLED          = 0x00000000;
        public const uint WDCTL__WDOG_PIN_ACTIVE            = 0x00000010;
        public const uint WDCTL__WDOG_PIN_OPEN_DRAIN        = 0x00000020;
        public const uint WDCTL__WDOG_PIN_OPEN_DRAIN_PULLUP = 0x00000030;

        public const uint WDDLY__RESET_CLOCK_DELAY          =        512;
        public const uint WDDLY__READ_HREG                  =         12;

        public const uint WDRST__KEY                        = 0x0000005C;

        //--//

        [Register(Offset=0x00000000U)] public ulong HREG;
        [Register(Offset=0x00000008U)] public ulong COMP;
        [Register(Offset=0x00000010U)] public uint  LD_HREG;
        [Register(Offset=0x00000014U)] public uint  WDCTL;
        [Register(Offset=0x00000018U)] public uint  WDDLY;
        [Register(Offset=0x0000001CU)] public uint  WDRST;

        //
        // Access Methods
        //

        public static extern RTC Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}