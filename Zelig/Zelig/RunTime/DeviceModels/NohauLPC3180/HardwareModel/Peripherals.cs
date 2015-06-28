//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_STOP


namespace Microsoft.NohauLPC3180
{
    using System;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.LPC3180;


    public sealed unsafe class Peripherals : RT.Peripherals
    {
        //
        // State
        //

        private uint* m_performanceCounter;

        //
        // Helper Methods
        //

        public override void Initialize()
        {
        }

        public override void Activate()
        {
            Drivers.InterruptController.Instance.Initialize();

            Drivers.RealTimeClock.Instance.Initialize();

            RT.SerialPortsManager.Instance.Initialize();
        }

        public override void CauseInterrupt()
        {
            Drivers.InterruptController.Instance.CauseInterrupt();
        }

        public override void ContinueUnderNormalInterrupt( Continuation dlg )
        {
            Drivers.InterruptController.Instance.ContinueUnderNormalInterrupt( dlg );
        }

        [RT.MemoryRequirements( RT.MemoryAttributes.InternalMemory )]
        public override void WaitForInterrupt()
        {
#if ALLOW_STOP
            using(RT.SmartHandles.InterruptState.DisableAll())
            {
                var ctrl = ChipsetModel.SystemControl.Instance;

                if(ctrl.START_SR_INT == 0)
                {
                    ctrl.SwitchToDirectRunMode();

                    ctrl.SwitchToStopModeWithSDRAM();

                    ctrl.Stabilize12vSupply();

                    ctrl.LockHCLKPLL();

                    ctrl.SwitchToRunMode();
                }

                ctrl.START_RSR_INT = ctrl.START_SR_INT;
            }
#else
            RT.TargetPlatform.ARMv4.Coprocessor15.WaitForInterrupt();
#endif
        }

        public override void ProcessInterrupt()
        {
            using(RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = ThreadManager.InstallInterruptThread())
            {
                Drivers.InterruptController.Instance.ProcessInterrupt();
            }
        }

        [RT.MemoryRequirements( RT.MemoryAttributes.RAM )]
        public override void ProcessFastInterrupt()
        {
            using(RT.SmartHandles.SwapCurrentThreadUnderInterrupt hnd = ThreadManager.InstallFastInterruptThread())
            {
                Drivers.InterruptController.Instance.ProcessFastInterrupt();
            }
        }

        public override ulong GetPerformanceCounterFrequency()
        {
            return Processor.Configuration.PeripheralsClockFrequency;
        }

        public override unsafe uint ReadPerformanceCounter()
        {
            uint* ptr = m_performanceCounter;

            if(ptr == null)
            {
                ptr = AllocatePerformanceCounter();
            }

            return *ptr;
        }

        //--//

        private unsafe uint* AllocatePerformanceCounter()
        {
            {
                var val = new ChipsetModel.SystemControl.TIMCLK_CTRL_bitfield();

                val.HSTimer_Enable = true;

                ChipsetModel.SystemControl.Instance.TIMCLK_CTRL = val;
            }

            //--//

            ChipsetModel.HighSpeedTimer timer = ChipsetModel.HighSpeedTimer.Instance;

            timer.HSTIM_COUNTER = 0;

            {
                var val = new ChipsetModel.HighSpeedTimer.HSTIM_MCTRL_bitfield(); // No interrupts for now.

                timer.HSTIM_MCTRL = val;
            }

            {
                var val = new ChipsetModel.HighSpeedTimer.HSTIM_CTRL_bitfield();

                val.COUNT_ENAB = true;
                val.PAUSE_EN   = true; // Allow hardware debugging to stop the counter.

                timer.HSTIM_CTRL = val;
            }

            //--//

            fixed(uint* ptr = &timer.HSTIM_COUNTER)
            {
                m_performanceCounter = ptr;
            }

            return m_performanceCounter;
        }
    }
}
