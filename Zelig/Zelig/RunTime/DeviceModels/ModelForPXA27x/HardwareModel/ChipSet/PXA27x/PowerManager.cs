//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40F00000U,Length=0x000001A4U)]
    public class PowerManager
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct PSSR_bitfield
        {
            [BitFieldRegister(Position=6)] public bool OTGPH; // OTG Peripheral Control Hold
                                                              // 
                                                              //   0 = OTG pad is not holding its state.
                                                              //   1 = OTG pad is holding its state.
                                                              //
            [BitFieldRegister(Position=5)] public bool RDH;   // Read Disable Hold
                                                              // 
                                                              //   0 = GPIO pins are configured according to their GPIO configuration (see Chapter 24, “General-Purpose I/O Controller”).
                                                              //   1 = The receivers of all GPIO pins are disabled.
                                                              //
                                                              // If RDH is set as a result of any reset except sleep, resistive pull-downs are enabled until RDH is cleared.
                                                              // RDH must be cleared by software after the peripheral and GPIO interfaces have been configured but before they are actually used.
                                                              // 
            [BitFieldRegister(Position=4)] public bool PH;    // Peripheral Control Hold
                                                              // 
                                                              //   0 = GPIO pins are configured according to their GPIO configuration.
                                                              //   1 = GPIO pins are held in their sleep-mode states.
                                                              // 
                                                              // PH is set upon entry into sleep or standby mode.
                                                              // PH must be cleared by software after the peripherals and GPIO interfaces have been configured but before they are actually used.
                                                              // PH is clear during deep-sleep.
                                                              // 
            [BitFieldRegister(Position=3)] public bool STS;   // Standby Mode Status
                                                              // 
                                                              //   0 = The processor has not been placed in standby mode by configuring the PWRMODE register since STS was cleared by a reset or by software.
                                                              //   1 = The processor was placed in standby mode by configuring the PWRMODE register.
                                                              //
            [BitFieldRegister(Position=2)] public bool VFS;   // VCC Fault Status
                                                              //
                                                              //   0 = nVDD_FAULT has not been asserted since it was last cleared by a reset or by software.
                                                              //   1 = nVDD_FAULT has been asserted and caused the processor to enter deep-sleep mode.
                                                              // 
                                                              // NOTE: This bit is not set by the assertion of nVDD_FAULT while the processor is in deep-sleep mode.
                                                              // 
            [BitFieldRegister(Position=1)] public bool BFS;   // Battery Fault Status
                                                              //
                                                              //   0 = nBATT_FAULT has not been asserted since it was last cleared by a reset or by software.
                                                              //   1 = nBATT_FAULT has been asserted and caused the processor to enter deep-sleep mode.
                                                              //
            [BitFieldRegister(Position=0)] public bool SSS;   // Software Sleep Status
                                                              //
                                                              //   0 = The processor has not been placed in sleep mode by configuring the PWRMODE register since SSS was last cleared by a reset or by the software.
                                                              //   1 = The processor was placed in sleep mode by configuring the PWRMODE register.
        }

        [Register(Offset=0x0000U             )] public uint          PMCR; // Power Manager Control register                                                | 3-67
        [Register(Offset=0x0004U             )] public PSSR_bitfield PSSR; // Power Manager Sleep Status register                                           | 3-69
        [Register(Offset=0x0008U             )] public uint          PSPR; // Power Manager Scratch Pad register                                            | 3-72
        [Register(Offset=0x000CU             )] public uint          PWER; // Power Manager Wake-Up Enable register                                         | 3-73
        [Register(Offset=0x0010U             )] public uint          PRER; // Power Manager Rising-Edge Detect Enable register                              | 3-77
        [Register(Offset=0x0014U             )] public uint          PFER; // Power Manager Falling-Edge Detect Enable register                             | 3-78
        [Register(Offset=0x0018U             )] public uint          PEDR; // Power Manager Edge-Detect Status register                                     | 3-79
        [Register(Offset=0x001CU             )] public uint          PCFR; // Power Manager General Configuration register                                  | 3-80
        [Register(Offset=0x0020U,Instances=32)] public uint[]        PGSR; // Power Manager GPIO Sleep State register for GPIO<120:0>                       | 3-83
        [Register(Offset=0x0030U             )] public uint          RCSR; // Reset Controller Status register                                              | 3-84
        [Register(Offset=0x0034U             )] public uint          PSLR; // Power Manager Sleep Configuration register                                    | 3-85
        [Register(Offset=0x0038U             )] public uint          PSTR; // Power Manager Standby Configuration register                                  | 3-88
        [Register(Offset=0x0040U             )] public uint          PVCR; // Power Manager Voltage Change Control register                                 | 3-89
        [Register(Offset=0x004CU             )] public uint          PUCR; // Power Manager USIM Card Control/Status register                               | 3-90
        [Register(Offset=0x0050U             )] public uint          PKWR; // Power Manager Keyboard Wake-Up Enable register                                | 3-92
        [Register(Offset=0x0054U             )] public uint          PKSR; // Power Manager Keyboard Level-Detect Status register                           | 3-93
        [Register(Offset=0x0080U,Instances=32)] public uint[]        PCMD; // Power Manager I2C Command register File                                       | 3-94

        //--//

        //
        // Helper Methods
        //

        public void ReleaseReadDisableHold()
        {
            this.PSSR = new PSSR_bitfield{ RDH = true };
        }

        //
        // Access Methods
        //

        public static extern PowerManager Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
