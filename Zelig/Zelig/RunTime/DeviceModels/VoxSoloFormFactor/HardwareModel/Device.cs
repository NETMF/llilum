//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.VoxSoloFormFactor
{
    using System;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.MM9691LP;


    public sealed class Device : RT.Device
    {
        public override void PreInitializeProcessorAndMemory()
        {
            //
            // Enter System mode, with interrupts disabled.
            //
            Processor.SetStatusRegister( Processor.c_psr_field_c, Processor.c_psr_I | Processor.c_psr_F | Processor.c_psr_mode_SYS );

            Processor.SetRegister( Processor.Context.RegistersOnStack.StackRegister, this.BootstrapStackPointer );

            //
            // Disable cache and remove remap of FLASH onto RAM.
            //
            {
                ChipsetModel.REMAP_PAUSE remap = ChipsetModel.REMAP_PAUSE.Instance;
                
                remap.Cache_Enable  = 0;
                remap.ClearResetMap = 0;
            }

            //
            // Configure external FLASH chip with proper wait states.
            //
            {
                ChipsetModel.EBIU.DEVICE flashChip;
                var                      ctrl = new ChipsetModel.EBIU.DEVICE.ControlBitField();

                ctrl.WS   = 2;
                ctrl.RCNT = 0;
                ctrl.SZ   = ChipsetModel.EBIU.DEVICE.Size.SZ16;

                //
                // Low 8mega.
                //
                flashChip = ChipsetModel.EBIU.Instance.Device0;
                flashChip.LowAddress  = 0x10000000;
                flashChip.HighAddress = 0x10000000 + 8 * 1024 * 1024;
                flashChip.Control     = ctrl;

                //
                // High 8mega.
                //
                flashChip = ChipsetModel.EBIU.Instance.Device1;
                flashChip.LowAddress  = 0x10000000 +  8 * 1024 * 1024;
                flashChip.HighAddress = 0x10000000 + 16 * 1024 * 1024;
                flashChip.Control     = ctrl;
            }

            {
                ChipsetModel.CMU cmu = ChipsetModel.CMU.Instance;

                //
                // We should wait 80usec for the switching regulator to warm up.
                //
                Processor.Delay( 80 * 32 / 12 ); // At reset, the clock prescaler is set to 12.

                cmu.CLK_SEL  = ChipsetModel.CMU.CLK_SEL__EXTSLOW |
                               ChipsetModel.CMU.CLK_SEL__NOPCU;
                               // ChipsetModel.CMU.CLK_SEL__CKOUTEN

                cmu.PERF_LVL = (uint)ChipsetModel.CMU.PERF_LEVEL.CLK_SEL__DIV_FAST;

                //
                // Enable GPIO clock.
                //
                cmu.EnableClock( ChipsetModel.CMU.MCLK_EN__GPIO );
            }
        }

        public override void MoveCodeToProperLocation()
        {
            Memory.Instance.ExecuteImageRelocation();
        }
    }
}
