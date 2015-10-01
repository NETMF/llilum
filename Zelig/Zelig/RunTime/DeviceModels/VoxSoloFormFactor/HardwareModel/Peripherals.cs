//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//#define ALLOW_PAUSE
//#define DCCQUEUE_PROFILE

namespace Microsoft.VoxSoloFormFactor
{
    using System;
    using System.Runtime.CompilerServices;

    using RT           = Microsoft.Zelig.Runtime;
    using TS           = Microsoft.Zelig.Runtime.TypeSystem;
    using ChipsetModel = Microsoft.DeviceModels.Chipset.MM9691LP;


    public sealed unsafe class Peripherals : RT.Peripherals
    {
        class DccQueue
        {
            const int c_size = 8192;

            //
            // State
            //

            private readonly uint[] m_array;

            private          int    m_count;
            private          int    m_writerPos;
            private          int    m_readerPos;

            //
            // Constructor Methods
            //

            internal DccQueue()
            {
                m_array       = new uint[c_size];

                m_count       = 0;
                m_writerPos   = 0;
                m_readerPos   = 0;
            }

            //
            // Helper Methods
            //

            internal bool Enqueue( uint val )
            {
                if(this.IsFull)
                {
                    return false;
                }

                int pos = m_writerPos;

                m_array[pos] = val;

                m_writerPos = NextPosition( pos );
                m_count++;

                return true;
            }

            internal bool Dequeue( out uint val )
            {
                if(this.IsEmpty)
                {
                    val = 0;

                    return false;
                }

                int pos = m_readerPos;

                val = m_array[pos];

                m_readerPos = NextPosition( pos );
                m_count--;

                return true;
            }

            //--//

            [RT.Inline]
            private int NextPosition( int val )
            {
                val = val + 1;

                if(val == c_size)
                {
                    return 0;
                }

                return val;
            }

            [RT.Inline]
            private int PreviousPosition( int val )
            {
                if(val == 0)
                {
                    val = c_size;
                }

                return val - 1;
            }

            //
            // Access Methods
            //

            internal bool IsEmpty
            {
                [RT.Inline]
                get
                {
                    return m_count == 0;
                }
            }

            internal bool IsFull
            {
                [RT.Inline]
                get
                {
                    return m_count == c_size;
                }
            }
        }

        //
        // State
        //

        private uint*    m_performanceCounter;
        private DccQueue m_txQueue;
        private DccQueue m_rxQueue;

#if DCCQUEUE_PROFILE
        private int      m_txCount;
        private int      m_txPump;
        private int      m_rxCount;
        private int      m_rxPump;
#endif

        //
        // Helper Methods
        //

        public override void Initialize()
        {
            AllocatePerformanceCounter();

////        {
////            // enable the arm timer clock
////            ChipsetModel.CMU.Instance.EnableClock( ChipsetModel.CMU.MCLK_EN__ARMTIM );
////
////            var ctrl = new ChipsetModel.ARMTIMERx.ControlBitField();
////
////            ctrl.Prescale = ChipsetModel.ARMTIMERx.Prescale.Div1;
////            ctrl.Periodic = false;
////            ctrl.Enable   = true;
////
////            var timer = ChipsetModel.ARMTIMER0.Instance;
////
////            timer.Control = ctrl;
////            timer.Clear   = 0;
////        }
        }

        public override void Activate()
        {
            m_txQueue = new DccQueue();
            m_rxQueue = new DccQueue();

            Drivers.InterruptController.Instance.Initialize();

            Drivers.RealTimeClock.Instance.Initialize();
        }

        public override void EnableInterrupt( uint index )
        {
        }

        public override void DisableInterrupt( uint index )
        {
        }

        public override void CauseInterrupt()
        {
            Drivers.InterruptController.Instance.CauseInterrupt();
        }

        public override void ContinueUnderNormalInterrupt( Continuation dlg )
        {
            Drivers.InterruptController.Instance.ContinueUnderNormalInterrupt( dlg );
        }

        public override void WaitForInterrupt()
        {
            //
            // Using the Pause register to save power seems to be broken at the hardware level: device locks up!!!!!
            //
#if ALLOW_PAUSE
            ChipsetModel.REMAP_PAUSE.Instance.Pause_AHB = 0;
#else
            ChipsetModel.INTC intc = ChipsetModel.INTC.Instance;

            while(intc.Irq.Status == 0)
            {
                PumpTxDCC();
                PumpRxDCC();
            }
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
            return RT.Configuration.CoreClockFrequency;
        }

        [RT.Inline]
        [RT.DisableNullChecks()]
        public override unsafe uint ReadPerformanceCounter()
        {
            return *m_performanceCounter;
        }

        //--//

        public void PostDCC( uint value )
        {
#if DCCQUEUE_PROFILE
            m_txCount++;
#endif

            if(m_txQueue == null)
            {
#if DCCQUEUE_PROFILE
                m_txPump++;
#endif

                RT.TargetPlatform.ARMv4.Coprocessor14.WriteDCC( value );
            }
            else
            {
                while(true)
                {
                    bool fSent;

                    using(RT.SmartHandles.InterruptState.Disable())
                    {
                        fSent = m_txQueue.Enqueue( value );
                    }

                    if(fSent)
                    {
                        return;
                    }

                    lock(m_txQueue)
                    {
#if DCCQUEUE_PROFILE
                        m_txPump++;
#endif

                        PumpTxDCC();
                    }
                }
            }
        }

        public uint ReceiveDCC()
        {
#if DCCQUEUE_PROFILE
            m_rxCount++;
#endif

            if(m_rxQueue == null)
            {
#if DCCQUEUE_PROFILE
                m_rxPump++;
#endif

                return RT.TargetPlatform.ARMv4.Coprocessor14.ReadDCC();
            }
            else
            {
                while(true)
                {
                    bool fGot;
                    uint val;

                    using(RT.SmartHandles.InterruptState.Disable())
                    {
                        fGot = m_rxQueue.Dequeue( out val );
                    }

                    if(fGot)
                    {
                        return val;
                    }

                    lock(m_rxQueue)
                    {
#if DCCQUEUE_PROFILE
                        m_rxPump++;
#endif

                        PumpRxDCC();
                    }

                    if(m_rxQueue.IsEmpty == false)
                    {
                        System.Threading.Thread.Sleep( 1 );
                    }
                }
            }
        }

        private void PumpTxDCC()
        {
            if(m_txQueue.IsEmpty == false && RT.TargetPlatform.ARMv4.Coprocessor14.CanWriteDCC())
            {
                using(RT.SmartHandles.InterruptState.Disable())
                {
                    if(m_txQueue.IsEmpty == false && RT.TargetPlatform.ARMv4.Coprocessor14.CanWriteDCC())
                    {
                        uint val;

                        if(m_txQueue.Dequeue( out val ))
                        {
                            RT.TargetPlatform.ARMv4.Coprocessor14.WriteDebugCommData( val );
                        }
                    }
                }
            }
        }

        private void PumpRxDCC()
        {
            if(RT.TargetPlatform.ARMv4.Coprocessor14.CanReadDCC() && m_rxQueue.IsFull == false)
            {
                using(RT.SmartHandles.InterruptState.Disable())
                {
                    if(RT.TargetPlatform.ARMv4.Coprocessor14.CanReadDCC() && m_rxQueue.IsFull == false)
                    {
                        m_rxQueue.Enqueue( RT.TargetPlatform.ARMv4.Coprocessor14.ReadDebugCommData() );
                    }
                }
            }
        }

        //--//

        private unsafe void AllocatePerformanceCounter()
        {
            const uint VTU_Channel = 0;
            const uint CP          = (VTU_Channel / 2) >> 1; // Channel Pair, Channel pair Half, AorB portion of VTU32 (for IO Control)
            const uint CH          = (VTU_Channel / 2) &  1; // Channel Pair, Channel pair Half, AorB portion of VTU32 (for IO Control)

            //--//

            ChipsetModel.CMU cmu = ChipsetModel.CMU.Instance;

            //
            // Enable VTU clock.
            //
            cmu.EnableClock( ChipsetModel.CMU.MCLK_EN__VTU32 );

            //--//

            ChipsetModel.VTU32 vtu = ChipsetModel.VTU32.Instance;

            // get mode set before any other writes to time subsystem
            {
                uint MC = (vtu.ModeControl & 0xFFFF);

                MC &= ~ChipsetModel.VTU32.ModeControl__get( VTU_Channel / 2, 0x000F                                      );
                MC |=  ChipsetModel.VTU32.ModeControl__set( VTU_Channel / 2, ChipsetModel.VTU32.ModeControl__TMODx_PWM32 );

                vtu.ModeControl = MC;
            }

            // reset 32-bit counter to zero before starting it
            vtu.ChannelPair[CP].Channel[CH].Counter = 0;

            // turn off just our external clock select
            {
                uint extClk = vtu.ExternalClockSelectRegister;

                extClk &= (ChipsetModel.VTU32.ExternalClockSelectRegister__CK1 << (int)(VTU_Channel / 2)) & 0x0000000F;

                vtu.ExternalClockSelectRegister = extClk;
            }

            if(vtu.ChannelPair[CP].ClockPrescalar__get( CH ) != 0)
            {
                // setup current prescalar, assume in 32-bit mode, we can run full tilt with 32-bits
                vtu.ChannelPair[CP].ClockPrescalar__set( CH, 0 );
            }

            // get control word, remove this PWM32's settings (both A and B)
            {
                uint IOC = (vtu.IOControl[CP].Value & 0xFFFF);
                
                IOC &= ~ChipsetModel.VTU32.IO_CONTROL.Set( CH, 0, 0x000F );
                IOC |=  ChipsetModel.VTU32.IO_CONTROL.Set( CH, 1, 0x000F );

                vtu.IOControl[CP].Value = IOC;
            }

            // write in Period, then duty order
            // true period is one more than programmed value
            vtu.ChannelPair[CP].Channel[CH].PeriodCapture    = uint.MaxValue;
            vtu.ChannelPair[CP].Channel[CH].DutyCycleCapture = uint.MaxValue / 2;        // square wave, as if it matters here

            // add GO bit to start PWM counters (always TxARUN for 32-bite mode)
            {
                uint MC = (vtu.ModeControl & 0xFFFF);

                MC &= ~ChipsetModel.VTU32.ModeControl__get( VTU_Channel / 2, 0x000F                                                                               );
                MC |=  ChipsetModel.VTU32.ModeControl__set( VTU_Channel / 2, ChipsetModel.VTU32.ModeControl__TMODx_PWM32 | ChipsetModel.VTU32.ModeControl__TxARUN );

                vtu.ModeControl = MC;
            }

            fixed(uint* ptr = &vtu.ChannelPair[CP].Channel[CH].Counter)
            {
                m_performanceCounter = ptr;
            }
        }
    }
}
