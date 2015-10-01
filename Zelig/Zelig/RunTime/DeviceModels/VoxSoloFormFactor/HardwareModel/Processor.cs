//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.VoxSoloFormFactor
{
    using System;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.MM9691LP;


    public sealed class Processor : ChipsetModel.Processor
    {
        //
        // Helper Methods
        //

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.Interrupt)]
        private static void InterruptHandler( ref Context.RegistersOnStack registers )
        {
            Context.InterruptHandlerWithContextSwitch( ref registers );
        }

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.FastInterrupt)]
        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        private static void FastInterruptHandler()
        {
            Context.FastInterruptHandlerWithoutContextSwitch();
        }

        [RT.BottomOfCallStack()]
        [RT.HardwareExceptionHandler(RT.HardwareException.SoftwareInterrupt)]
        private static void SoftwareInterruptHandler( ref Context.RegistersOnStack registers )
        {
            Context.GenericSoftwareInterruptHandler( ref registers );
        }

                //--//

        static Context.RegistersOnStack fault;

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.UndefinedInstruction)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void UndefinedInstruction( ref Context.RegistersOnStack registers )
        {
            fault.Assign( ref registers );

            while(true)
            {
            }
        }

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.PrefetchAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void PrefetchAbort( ref Context.RegistersOnStack registers )
        {
            fault.Assign( ref registers );

            while(true)
            {
            }
        }

        [RT.NoInline]
        [RT.NoReturn()]
        [RT.HardwareExceptionHandler(RT.HardwareException.DataAbort)]
        [RT.MemoryUsage(RT.MemoryUsage.Bootstrap)]
        static void DataAbort( ref Context.RegistersOnStack registers )
        {
            fault.Assign( ref registers );

            while(true)
            {
            }
        }

    }
}
