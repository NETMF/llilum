//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.iMote2
{
    using System;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using ARMv4        = Microsoft.Zelig.Runtime.TargetPlatform.ARMv4;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.PXA27x;
    using Microsoft.Zelig.Runtime;


    public sealed class Device : RT.Device
    {
        public override void PreInitializeProcessorAndMemory()
        {
            //
            // Enter System mode, with interrupts disabled.
            //
            Processor.SetStatusRegister( Processor.c_psr_field_c, Processor.c_psr_I | Processor.c_psr_F | Processor.c_psr_mode_SYS );

            Processor.SetRegister( Processor.Context.RegistersOnStack.StackRegister, this.BootstrapStackPointer );

            //--//

            Processor.EnableCaches();

            ChipsetModel.ClockManager.Instance.InitializeClocks();

            //--//

            ChipsetModel.MemoryController.Instance.InitializeStackedSDRAM();
        }

        const int DefaultStackSizeMote = (16 * 1024) / sizeof( uint );

        [MemoryUsage( MemoryUsage.Stack, ContentsUninitialized = true, AllocateFromHighAddress = true )]
        static readonly uint[] s_bootstrapStackMote = new uint[DefaultStackSizeMote];

        public override uint[] BootstrapStack
        {
            get
            {
                return s_bootstrapStackMote;
            }
        }

        public override void MoveCodeToProperLocation()
        {
            Memory.Instance.ExecuteImageRelocation();
        }
    }
}
