//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Emulation.ArmProcessor.Chipset
{
    using System;
    using System.Threading;
    using System.Collections.Generic;

    using EncDef = Microsoft.Zelig.TargetModel.ArmProcessor.EncodingDefinition;
    using ElementTypes = Microsoft.Zelig.MetaData.ElementTypes;
    using Cfg = Microsoft.Zelig.Configuration.Environment;

    public static partial class PXA27x
    {
        [Simulator.PeripheralRange( Base = 0x48000000U, Length = 0x00000068U, ReadLatency = 1, WriteLatency = 2 )]
        public class MCU : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x44000000U, Length = 0x04000058U, ReadLatency = 1, WriteLatency = 2 )]
        public class LCD : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x4C000000U, Length = 0x00000070U, ReadLatency = 1, WriteLatency = 2 )]
        public class USBHost : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x50000000U, Length = 0x0000003CU, ReadLatency = 1, WriteLatency = 2 )]
        public class QuickCapture : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x40000000U, Length = 0x0000112CU, ReadLatency = 1, WriteLatency = 2 )]
        public class DMA : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x40100000U, Length = 0x00000030U, ReadLatency = 1, WriteLatency = 2 )]
        public class FFUART : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x40200000U, Length = 0x00000030U, ReadLatency = 1, WriteLatency = 2 )]
        public class BTUART : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x40301680U, Length = 0x00000028U, ReadLatency = 1, WriteLatency = 2 )]
        public class I2C : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40400000U, Length = 0x00000084U, ReadLatency = 1, WriteLatency = 2 )]
        public class I2S : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40500000U, Length = 0x00000600U, ReadLatency = 1, WriteLatency = 2 )]
        public class AC : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40600000U, Length = 0x00000460U, ReadLatency = 1, WriteLatency = 2 )]
        public class USBClient : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40700000U, Length = 0x00000030U, ReadLatency = 1, WriteLatency = 2 )]
        public class UART : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40800000U, Length = 0x00000020U, ReadLatency = 1, WriteLatency = 2 )]
        public class IR : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40900000U, Length = 0x0000003CU, ReadLatency = 1, WriteLatency = 2 )]
        public class RTC : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40A00000U, Length = 0x000000E0U, ReadLatency = 1, WriteLatency = 2 )]
        public class Timers : Simulator.Peripheral
        {
            public class TimerCntReg : Simulator.Peripheral
            {
                [Simulator.LinkToContainer]
                public Timers Parent;

                [Simulator.Register( Offset = 0x00000000U )]
                public virtual uint Counter
                {
                    get
                    {
                        return (uint)(Parent.GetCurrentTime() - m_base);
                    }
                    set
                    {
                        m_base = Parent.GetCurrentTime() - value;
                        Parent.Evaluate();
                    }
                }

                protected ulong m_base;
            }

            public class TimerCnt0Reg : TimerCntReg
            {
                [Simulator.Register( Offset = 0x00000000U )]
                public override uint Counter
                {
                    get
                    {
                        return (uint)( Parent.GetCurrentTicks() - m_base );
                    }
                    set
                    {
                        m_base = Parent.GetCurrentTicks() - value;
                        Parent.Evaluate();
                    }
                }
            }

            public class TimerMchReg : Simulator.Peripheral
            {
                [Simulator.LinkToContainer]
                public Timers Parent;

                [Simulator.Register( Offset = 0x00000000U )]
                public uint Match
                {
                    get
                    {
                        return m_match;
                    }
                    set
                    {
                        m_isMatched = false;
                        m_isEnabled = false;

                        m_match = value;

                        Parent.Evaluate();
                    }
                }

                public void RegisterForCallback( Hosting.DeviceClockTicksTracking svc, long matchValue )
                {
                    m_isEnabled = true;

                    //svc.CancelClockTickCallback( Callback );

                    svc.RequestRelativeClockTickCallback( matchValue, Callback );
                }

                public void CancelCallback( Hosting.DeviceClockTicksTracking svc )
                {
                    m_isEnabled = false;

                    svc.CancelClockTickCallback( Callback );
                }


                private void Callback()
                {
                    m_isMatched = true;
                    Parent.Evaluate();
                }

                public bool IsMatched
                {
                    get { return m_isMatched; }
                    set { m_isMatched = value; }
                }

                public bool IsEnabled
                {
                    get { return m_isEnabled; }
                }

                private bool m_isMatched;
                private bool m_isEnabled;
                private uint m_match;
            }
            public class TimerCtrlReg : Simulator.Peripheral
            {
                [Simulator.LinkToContainer]
                public Timers Parent;

                [Simulator.Register( Offset = 0x00000000U )]
                public uint Control
                {
                    get
                    {
                        return m_control;
                    }
                    set
                    {
                        m_control = value;
                        Parent.Evaluate();
                    }
                }

                private uint m_control;
            }


            int[] m_interruptIndexTable = new int[12];
            uint m_timerIntEnable = 0;
            uint m_status = 0;
            double m_convertFromCpuTicks;
            double m_convertToCpuTicks;

            bool m_fInterruptDisable = false;

            [Simulator.LinkToPeripheral()]
            public INTC InterruptController;

            [Simulator.Register( Offset = 0x00000000U, Size = 0x04U, Instances = 4 )]
            public TimerMchReg[] OSMR0;

            [Simulator.Register( Offset = 0x00000010U, Size = 0x04U, Instances = 1 )]
            public TimerCnt0Reg[] OSCR0; // Main count register (for OSMR0-3)

            [Simulator.Register( Offset = 0x00000014U )]
            public uint OSSR // Status register
            {
                get
                {
                    return m_status;
                }

                set
                {
                    // status register for all counters
                    m_status &= ~value;

                    uint val = value;
                    int idx = -1;

                    while(val != 0)
                    {
                        idx++;
                        val >>= 1;
                    }
                    if(idx < 0 || idx >= OSMR.Length) return;

                    if(idx < 4)
                    {
                        OSMR0[idx].IsMatched = false;
                    }
                    else
                    {
                        OSMR[idx - 4].IsMatched = false;
                    }

                    InterruptController.Reset( m_interruptIndexTable[idx] );

                    Evaluate();
                }
            }

            [Simulator.Register( Offset = 0x00000018U )]
            public uint OWER; // Watchdog enable register

            [Simulator.Register( Offset = 0x0000001CU )]
            public uint OIER // interrupt enable register
            {
                get
                {
                    return m_timerIntEnable;
                }

                set
                {
                    m_timerIntEnable = value;
                    Evaluate();
                }
            }

            [Simulator.Register( Offset = 0x00000020U )]
            public uint OSNR; // snapshot register

            [Simulator.Register( Offset = 0x00000040U, Size = 0x04U, Instances = 8 )]
            public TimerCntReg[] OSCR;

            [Simulator.Register( Offset = 0x00000080U, Size = 0x04U, Instances = 8)]
            public TimerMchReg[] OSMR;

            [Simulator.Register( Offset = 0x000000C0U, Size = 0x04U, Instances = 8 )]
            public TimerCtrlReg[] OMCR;

            public Timers()
            {
                int []timerInts = new int[] 
                { 
                    (int)INTC.c_IRQ_INDEX_OS_TIMER0,
                    (int)INTC.c_IRQ_INDEX_OS_TIMER1,
                    (int)INTC.c_IRQ_INDEX_OS_TIMER2,
                    (int)INTC.c_IRQ_INDEX_OS_TIMER3,
                };

                for(int i = 0; i < m_interruptIndexTable.Length; i++)
                {
                    if(i < timerInts.Length)
                    {
                        m_interruptIndexTable[i] = timerInts[i];
                    }
                    else
                    {
                        m_interruptIndexTable[i] = (int)INTC.c_IRQ_INDEX_OS_TIMER;
                    }
                }
            }

            public override void OnConnected()
            {
                base.OnConnected();

                Cfg.ProcessorCategory proc = m_owner.Product.SearchValue<Cfg.ProcessorCategory>();

                m_convertFromCpuTicks = (double)proc.RealTimeClockFrequency / (double)proc.CoreClockFrequency;
                m_convertToCpuTicks = (double)proc.CoreClockFrequency / (double)proc.RealTimeClockFrequency;
            }

            internal ulong GetCurrentTicks()
            {
                return m_owner.ClockTicks;
            }

            internal ulong GetCurrentTime()
            {
                return ConvertFromCpuTicks( m_owner.ClockTicks );
            }

            internal void Evaluate()
            {
                if(m_fInterruptDisable)
                {
                    this.InterruptController.Reset( 0 );
                }
                else
                {
                    Hosting.DeviceClockTicksTracking svc; m_owner.GetHostingService( out svc );

                    for(int i = 0; i < 12; i++)
                    {
                        TimerMchReg regMatch = i < 4 ? OSMR0[i] : OSMR[i-4];

                        if(0 != ( m_timerIntEnable & ( 1u << i ) ))
                        {
                            TimerCntReg regCount;

                            if(i < 4)
                            {
                                regCount = OSCR0[0];
                            }
                            else if(i < 8)
                            {
                                if(( OMCR[i - 4].Control & ( 1u << 7 ) ) == 0)
                                {
                                    regCount = OSCR[0];
                                }
                                else
                                {
                                    regCount = OSCR[i - 4];
                                }
                            }
                            else
                            {
                                if(( OMCR[i - 4].Control & ( 1u << 7 ) ) == 0)
                                {
                                    regCount = OSCR[8 - 4];
                                }
                                else
                                {
                                    regCount = OSCR[i - 4];
                                }
                            }

                            if(regMatch.IsMatched || !regMatch.IsEnabled)
                            {
                                int diff = (int)( regMatch.Match - regCount.Counter );

                                if(regMatch.IsMatched || diff == 0)
                                {
                                    m_status |= 1u << i;

                                    regMatch.IsMatched = false;

                                    this.InterruptController.Set( m_interruptIndexTable[i] );
                                }

                                if(diff <= 0)
                                {
                                    if(!regMatch.IsEnabled) // overlap 
                                    {
                                        if(i < 4)
                                        {
                                            regMatch.RegisterForCallback( svc, regCount.Counter - regMatch.Match + uint.MaxValue );
                                        }
                                        else
                                        {
                                            regMatch.RegisterForCallback( svc, ConvertToCpuTicks( uint.MaxValue - ( regCount.Counter - regMatch.Match ) ) );
                                        }
                                    }
                                }
                                else if(!regMatch.IsEnabled)
                                {
                                    this.InterruptController.Reset( m_interruptIndexTable[i] );

                                    if(i < 4)
                                    {
                                        regMatch.RegisterForCallback( svc, diff );
                                    }
                                    else
                                    {
                                        regMatch.RegisterForCallback( svc, ConvertToCpuTicks( diff ) );
                                    }
                                }
                            }
                        }
                        else if(regMatch.IsEnabled)
                        {
                            regMatch.CancelCallback( svc );
                        }
                    }
                }
            }

            //--//

            private ulong ConvertFromCpuTicks( ulong ticks )
            {
                return (ulong)( ticks * m_convertFromCpuTicks );
            }

            private long ConvertToCpuTicks( long ticks )
            {
                return (long)( ticks * m_convertToCpuTicks );
            }
        }

        [Simulator.PeripheralRange( Base = 0x40B00000U, Length = 0x00000020U, ReadLatency = 1, WriteLatency = 2 )]
        public class PWM : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40D00000U, Length = 0x000000D0U, ReadLatency = 1, WriteLatency = 2 )]
        public class INTC : Simulator.Peripheral
        {
            public const uint c_IRQ_INDEX_SSP_3 = 0;
            public const uint c_IRQ_INDEX_MSL = 1;
            public const uint c_IRQ_INDEX_USB_HOST_2 = 2;
            public const uint c_IRQ_INDEX_USB_HOST_1 = 3;
            public const uint c_IRQ_INDEX_KEYPAD_CTRL = 4;
            public const uint c_IRQ_INDEX_MEMORY_STICK = 5;
            public const uint c_IRQ_INDEX_PWR_I2C = 6;
            public const uint c_IRQ_INDEX_OS_TIMER = 7;
            public const uint c_IRQ_INDEX_GPIO0 = 8;
            public const uint c_IRQ_INDEX_GPIO1 = 9;
            public const uint c_IRQ_INDEX_GPIOx = 10;
            public const uint c_IRQ_INDEX_USB_CLIENT = 11;
            public const uint c_IRQ_INDEX_PMU = 12;
            public const uint c_IRQ_INDEX_I2S = 13;
            public const uint c_IRQ_INDEX_AC97 = 14;
            public const uint c_IRQ_INDEX_USIM = 15;
            public const uint c_IRQ_INDEX_SSP_2 = 16;
            public const uint c_IRQ_INDEX_LCD = 17;
            public const uint c_IRQ_INDEX_I2C = 18;
            public const uint c_IRQ_INDEX_INFRA_RED_COM = 19;
            public const uint c_IRQ_INDEX_STUART = 20;
            public const uint c_IRQ_INDEX_BTUART = 21;
            public const uint c_IRQ_INDEX_FFUART = 22;
            public const uint c_IRQ_INDEX_FLASH_CARD = 23;
            public const uint c_IRQ_INDEX_SSP_1 = 24;
            public const uint c_IRQ_INDEX_DMA_CTRL = 25;
            public const uint c_IRQ_INDEX_OS_TIMER0 = 26;
            public const uint c_IRQ_INDEX_OS_TIMER1 = 27;
            public const uint c_IRQ_INDEX_OS_TIMER2 = 28;
            public const uint c_IRQ_INDEX_OS_TIMER3 = 29;
            public const uint c_IRQ_INDEX_RTC_1HZ_TIC = 30;
            public const uint c_IRQ_INDEX_RTC_ALARM = 31;
            public const uint c_IRQ_INDEX_TRUSTED_PLFM = 32;
            public const uint c_IRQ_INDEX_QK_CAP = 33;


            private uint m_inputStatus1 = 0;
            private uint m_inputStatus2 = 0;

            private uint m_irqMask1 = 0;
            private uint m_irqMask2 = 0;

            private uint m_levelInt1 = 0;
            private uint m_levelInt2 = 0;

            public class INTCPriReg : Simulator.Peripheral
            {
                [Simulator.LinkToContainer]
                public INTC Parent;

                [Simulator.Register( Offset = 0x00000000U )]
                public uint Priority;
            }

            public void Set( int index )
            {
                if(index > 31)
                {
                    index -= 32;

                    uint oldStatus = m_inputStatus2;

                    m_inputStatus2 |= 1U << index;

                    if(m_inputStatus2 != oldStatus)
                    {
                        Evaluate(m_inputStatus2);
                    }
                }
                else
                {
                    uint oldStatus = m_inputStatus1;

                    m_inputStatus1 |= 1U << index;

                    if(m_inputStatus1 != oldStatus)
                    {
                        Evaluate(m_inputStatus1);
                    }
                }
            }

            public void Reset( int index )
            {
                if(index > 31)
                {
                    index -= 32;

                    uint oldStatus = m_inputStatus2;

                    m_inputStatus2 &= ~(1U << index);

                    if(m_inputStatus2 != oldStatus)
                    {
                        Evaluate( m_inputStatus2 );
                    }
                }
                else
                {
                    uint oldStatus = m_inputStatus1;

                    m_inputStatus1 &= ~( 1U << index );

                    if(m_inputStatus1 != oldStatus)
                    {
                        Evaluate( m_inputStatus1 );
                    }
                }
            }

            public void Evaluate(uint status)
            {
                m_owner.SetIrqStatus( status != 0 );
            }

            [Simulator.Register( Offset = 0x00000000U )]
            public uint ICIP // IRQ Pending register
            {
                get
                {
                    return m_irqMask1 & m_inputStatus1;
                }
            }

            [Simulator.Register( Offset = 0x00000004U )]
            public uint ICMR // Mask register
            {
                get
                {
                    return m_irqMask1;
                }

                set
                {
                    // status register for all counters
                    m_irqMask1 = value;
                    Evaluate( m_irqMask1 & m_inputStatus1 );
                }
            }

            [Simulator.Register( Offset = 0x00000008U )]
            public uint ICLR // level register
            {
                get
                {
                    return m_levelInt1;
                }
                set
                {
                    uint old = m_levelInt1;

                    m_levelInt1 = value;

                    old ^= m_levelInt1;
                    
                    int idx = 0;

                    while(old != 0)
                    {
                        if(0 != ( old & 1 )) Set( idx );

                        idx++;
                        old >>= 1;
                    }
                }
            }

            [Simulator.Register( Offset = 0x0000000CU )]
            public uint ICFP; // FIQ pending register

            [Simulator.Register( Offset = 0x00000010U )]
            public uint ICPR; // IRQ pending register

            [Simulator.Register( Offset = 0x00000014U )]
            public uint ICCR; // control register

            [Simulator.Register( Offset = 0x00000018U )]
            public uint ICHP // Highest Priority register
            {
                get
                {
                    if(( m_irqMask1 & m_inputStatus1 ) != 0)
                    {
                        uint ichp = 0x80000000;
                        int i = 0;

                        while(0 == ( ( 1u << i ) & m_inputStatus1 )) i++;

                        ichp |= (uint)i << 16;

                        m_inputStatus1 &= ~( 1u << i );

                        m_owner.SetIrqStatus( m_inputStatus1 != 0 && m_inputStatus2 != 0 );

                        return ichp;
                    }
                    else if(( m_irqMask2 & m_inputStatus2 ) != 0)
                    {
                        uint ichp = 0x80000000;
                        int i = 0;

                        while(0 == ( ( 1u << i ) & m_inputStatus2 )) i++;

                        ichp |= (uint)( i + 32 ) << 16;

                        m_inputStatus2 &= ~( 1u << i );

                        m_owner.SetIrqStatus( m_inputStatus1 != 0 && m_inputStatus2 != 0 );

                        return ichp;
                    }
                    else
                    {
                        UpdateClocks( 8000, TargetAdapterAbstractionLayer.MemoryAccessType.FETCH );
                    }

                    return 0;
                }
            }

            [Simulator.Register( Offset = 0x0000001CU, Size = 0x04U, Instances = 32 )]
            public INTCPriReg[] IPR; // IRQ pending register

            [Simulator.Register( Offset = 0x0000009CU )]
            public uint ICIP2 // IRQ pending register 2
            {
                get
                {
                    return m_irqMask2 & m_inputStatus2;
                }
            }

            [Simulator.Register( Offset = 0x000000A0U )]
            public uint ICMR2 // Mask register 2
            {
                get
                {
                    return m_irqMask2;
                }

                set
                {
                    // status register for all counters
                    m_irqMask2 = value;
                    Evaluate( m_irqMask2 & m_inputStatus2 );
                }
            }

            [Simulator.Register( Offset = 0x000000A4U )]
            public uint ICLR2 // Level register 2
            {
                get
                {
                    return m_levelInt2;
                }
                set
                {
                    uint old = m_levelInt2;

                    m_levelInt2 = value;

                    old ^= m_levelInt2;

                    int idx = 0;

                    while(old != 0)
                    {
                        if(0 != ( old & 1 )) Set( idx );

                        idx++;
                        old >>= 1;
                    }
                }
            }

            [Simulator.Register( Offset = 0x000000A8U )]
            public uint ICFP2; // FIQ pending register 2

            [Simulator.Register( Offset = 0x000000ACU )]
            public uint ICPR2; // IRQ pending register 2

            [Simulator.Register( Offset = 0x000000B0U, Size = 0x04U, Instances = 8 )]
            public INTCPriReg IPR2; // Priority register 2
        }

        [Simulator.PeripheralRange( Base = 0x40E00000U, Length = 0x0000014CU, ReadLatency = 1, WriteLatency = 2 )]
        public class GPIO : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }
        
        [Simulator.PeripheralRange( Base = 0x40F00000U, Length = 0x00000100U, ReadLatency = 1, WriteLatency = 2 )]
        public class PMR : Simulator.Peripheral
        {
            public override void Write( uint address, uint relativeAddress, uint value, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
            }

            public override uint Read( uint address, uint relativeAddress, TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                return 0;
            }
        }

        [Simulator.PeripheralRange( Base = 0x41300000U, Length = 0x00000010U, ReadLatency = 1, WriteLatency = 2 )]
        public class ClockMgr : Simulator.Peripheral
        {

            [Simulator.Register( Offset = 0x00000000U )]
            public uint CCCR; // Core clock configation register

            [Simulator.Register( Offset = 0x00000004U )]
            public uint CKEN; // Clock enable register

            [Simulator.Register( Offset = 0x00000008U )]
            public uint OSCC; // Oscillator configuration register


            const uint CCSR__CPLCK = 0x20000000; // Core PLL lock
            const uint CCSR__PPLCK = 0x10000000; // Peripheral PLL lock

            [Simulator.Register( Offset = 0x0000000CU )]
            public uint CCSR = CCSR__CPLCK | CCSR__PPLCK; // Clock enable register

            public override void OnConnected()
            {
                base.OnConnected();

                //
                // Map FLASH to 0x00000000. Clock is enabled early so do the remap here
                //
                Simulator.AddressSpaceHandler hnd = m_owner.FindMemoryAtAddress( 0x5c000000u );
                if(hnd != null)
                {
                    hnd.LinkAtAddress( 0 );
                }
            }

        }

        //
        // Internal memory.
        //
        public class RamMemoryHandler : Simulator.MemoryHandler
        {
            //
            // State
            //

            private ulong m_lastWrite = 0;

            //
            // Constructor Methods
            //

            public RamMemoryHandler()
            {
            }

            //--//

            //
            // Helper Methods
            //

            public override void Initialize( Simulator owner,
                                             ulong rangeLength,
                                             uint rangeWidth,
                                             uint readLatency,
                                             uint writeLatency )
            {
                base.Initialize( owner, rangeLength, rangeWidth, readLatency, writeLatency );

                for(int i = 0; i < m_target.Length; i++)
                {
                    m_target[i] = 0xDEADBEEF;
                }
            }

            public override void UpdateClockTicksForLoad( uint address,
                                                          TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                uint latency = m_readLatency; if(m_owner.ClockTicks == m_lastWrite) latency++; // See Ollie Spec on Read-After-Write.

                UpdateClocks( latency, kind );
            }

            public override void UpdateClockTicksForStore( uint address,
                                                           TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                m_lastWrite = m_owner.ClockTicks + 1;

                UpdateClocks( m_writeLatency, kind );
            }
        }

        //
        // FAKE cache memory
        //
        public class CacheMemoryHandler : Simulator.AddressSpaceBusHandler
        {
            const uint CacheableMask = 0x80000000u;
            //
            // Constructor Methods
            //

            public CacheMemoryHandler()
            {
            }

            //--//

            //
            // Helper Methods
            //

            public override bool CanAccess( uint address,
                                            uint relativeAddress,
                                            TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                uint uncachedAddress = GetUncacheableAddress( address );

                return base.CanAccess( uncachedAddress, uncachedAddress, kind );
            }

            public override uint Read( uint address,
                                       uint relativeAddress,
                                       TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                uint uncachedAddress = GetUncacheableAddress( address );

                return base.Read( uncachedAddress, uncachedAddress, kind );
            }

            public override void Write( uint address,
                                        uint relativeAddress,
                                        uint value,
                                        TargetAdapterAbstractionLayer.MemoryAccessType kind )
            {
                uint uncachedAddress = GetUncacheableAddress( address );

                base.Write( uncachedAddress, uncachedAddress, value, kind );
            }

            public override uint GetPhysicalAddress( uint address )
            {
                return GetUncacheableAddress( address );
            }

            public static uint GetUncacheableAddress( uint address )
            {
                return address & ~CacheableMask;
            }

            public static uint GetCacheableAddress( uint address )
            {
                return address | CacheableMask;
            }
        }
    }
}
