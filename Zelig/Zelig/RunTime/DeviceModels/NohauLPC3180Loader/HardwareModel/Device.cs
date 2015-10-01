//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.NohauLPC3180Loader
{
    using System;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.LPC3180;


    public sealed class Device : RT.Device
    {
        public override void PreInitializeProcessorAndMemory()
        {
            //
            // Enter System mode, with interrupts disabled.
            //
            Processor.SetStatusRegister( Processor.c_psr_field_c, Processor.c_psr_I | Processor.c_psr_F | Processor.c_psr_mode_SYS );

            Processor.SetRegister( Processor.Context.RegistersOnStack.StackRegister, this.BootstrapStackPointer );

            Processor.PreInitializeProcessor( Processor.Configuration.SYSCLK, (uint)Processor.Configuration.CoreClockFrequency, (uint)Processor.Configuration.AHBClockFrequency, (uint)Processor.Configuration.PeripheralsClockFrequency );
        }

        public override void MoveCodeToProperLocation()
        {
            Memory.Instance.ExecuteImageRelocation();
        }
    }
}
