//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.LPC3180
{
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40028000U,Length=0x00000034U)]
    public class GPIO
    {
        [Register(Offset=0x00U)] public uint PIO_INP_STATE;     // Input pin state register. Reads the state of input pins. - RO
        [Register(Offset=0x04U)] public uint PIO_OUTP_SET;      // Output pin set register. Allows setting output pin(s). - WO
        [Register(Offset=0x08U)] public uint PIO_OUTP_CLR;      // Output pin clear register. Allows clearing output pin(s). - WO
        [Register(Offset=0x0CU)] public uint PIO_OUTP_STATE;    // Output pin state register. Reads the state of output pins. - RO
        [Register(Offset=0x10U)] public uint PIO_DIR_SET;       // GPIO direction set register. Configures I/O pins as outputs. - WO
        [Register(Offset=0x14U)] public uint PIO_DIR_CLR;       // GPIO direction clear register. Configures I/O pins as inputs. - WO
        [Register(Offset=0x18U)] public uint PIO_DIR_STATE;     // GPIO direction state register. Reads back pin directions. 0 RO
        [Register(Offset=0x1CU)] public uint PIO_SDINP_STATE;   // Input pin state register for SDRAM pins. Reads the state of SDRAM input pins. - RO
        [Register(Offset=0x20U)] public uint PIO_SDOUTP_SET;    // Output pin set register for SDRAM pins. Allows setting SDRAM output pin(s). - WO
        [Register(Offset=0x24U)] public uint PIO_SDOUTP_CLR;    // Output pin clear register for SDRAM pins. Allows clearing SDRAM output pin(s). - WO
        [Register(Offset=0x28U)] public uint PIO_MUX_SET;       // PIO multiplexer control set register. Controls the selection of alternate functions on certain pins. - WO
        [Register(Offset=0x2CU)] public uint PIO_MUX_CLR;       // PIO multiplexer control clear register. Controls the selection of alternate functions on certain pins. - WO
        [Register(Offset=0x30U)] public uint PIO_MUX_STATE;     // PIO multiplexer state register. Reads back the selection of alternate functions on certain pins. 0x00000000 RO

        //--//

        //
        // Helper Methods
        //

        [Inline]
        public void SetGPO( int pin )
        {
            this.PIO_OUTP_SET = 1U << pin;
        }

        [Inline]
        public void ResetGPO( int pin )
        {
            this.PIO_OUTP_CLR = 1U << pin;
        }

        //
        // Access Methods
        //

        public static extern GPIO Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
