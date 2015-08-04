//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexMOnMBED
{
    using System;
    using System.Runtime.CompilerServices;

    using RT        = Microsoft.Zelig.Runtime;
    using TS        = Microsoft.Zelig.Runtime.TypeSystem;
    using Chipset   = Microsoft.DeviceModels.Chipset.CortexM;

    public sealed unsafe class Peripherals : RT.Peripherals
    {
        //
        // State
        //

        //
        // Helper Methods
        //

        public override void Initialize()
        {
        }

        public override void Activate()
        {
            Drivers.InterruptController.Instance.Initialize();


            Drivers.SystemTimer.Instance.Initialize();

            // These should not be initialized here, otherwise they will always be
            // included in the image
            //Drivers.GPIO.Instance.Initialize();
            //Drivers.I2C.Instance.Initialize();
            //Drivers.SPI.Instance.Initialize();
        }

        public override void CauseInterrupt()
        {
            Drivers.InterruptController.Instance.CauseInterrupt();
        }

        public override void ContinueUnderNormalInterrupt(Continuation dlg)
        {
            Drivers.InterruptController.Instance.ContinueUnderNormalInterrupt(dlg);
        }

        public override void WaitForInterrupt()
        {
            //
            // Using the Pause register to save power seems to be broken at the hardware level: device locks up!!!!!
            //
#if ALLOW_PAUSE
////        ChipsetModel.REMAP_PAUSE.Instance.Pause_AHB = 0;
#else
            while (true)
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.FailedBootstrap );
                // TODO: implement
            }
#endif
        }

        public override void ProcessInterrupt()
        {
            using (RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallInterruptThread())
            {
                Drivers.InterruptController.Instance.ProcessInterrupt();
            }
        }

        [RT.MemoryRequirements(RT.MemoryAttributes.RAM)]
        public override void ProcessFastInterrupt()
        {
            using (RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = RT.ThreadManager.InstallFastInterruptThread())
            {
                Drivers.InterruptController.Instance.ProcessFastInterrupt();
            }
        }

        public override ulong GetPerformanceCounterFrequency()
        {
            return 1000000;
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        public override unsafe uint ReadPerformanceCounter()
        {
            return Drivers.SystemTimer.Instance.CurrentTimeRaw;
        }
    }
}
