//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40E00000U,Length=0x0000014CU)]
    public class GPIO
    {
        [MemoryMappedPeripheral(Base=0x0U,Length=0x0000004CU)]
        class Cluster
        {
#pragma warning disable 649
            [Register(Offset=0x00U)] internal uint GPLR; // GPIO<X+31:X> Pin-Level register
            [Register(Offset=0x0CU)] internal uint GPDR; // GPIO<X+31:X> Pin Direction register
            [Register(Offset=0x18U)] internal uint GPSR; // GPIO<X+31:X> Pin Output Set register
            [Register(Offset=0x24U)] internal uint GPCR; // GPIO<X+31:X> Pin Output Clear register
            [Register(Offset=0x30U)] internal uint GRER; // GPIO<X+31:X> Rising-Edge Detect Enable register
            [Register(Offset=0x3CU)] internal uint GFER; // GPIO<X+31:X> Falling-Edge Detect Enable register
            [Register(Offset=0x48U)] internal uint GEDR; // GPIO<X+31:X> Edge Detect Status register
#pragma warning restore 649

            //
            // Helper Methods
            //

            [Inline]
            internal void ConfigureAsInput( int pin )
            {
                uint mask = 1u << (pin & 31);

                this.GPDR &= ~mask;
            }

            [Inline]
            internal void ConfigureAsOutput( int pin )
            {
                uint mask = 1u << (pin & 31);

                this.GPDR |= mask;
            }

            [Inline]
            internal void ConfigureInterrupt( int  pin         ,
                                              bool raisingEdge ,
                                              bool fallingEdge )
            {
                uint mask = 1u << (pin & 31);

                if(raisingEdge)
                {
                    this.GRER |= mask;
                }
                else
                {
                    this.GRER &= ~mask;
                }

                if(fallingEdge)
                {
                    this.GFER |= mask;
                }
                else
                {
                    this.GFER &= ~mask;
                }

                this.GEDR = mask; // Clear any pending interrupt for pin
            }

            [Inline]
            internal bool InterruptPending( int  pin   ,
                                            bool clear )
            {
                uint mask = 1u << (pin & 31);

                var res = this.GEDR & mask;

                if(res == 0)
                {
                    return false;
                }

                if(clear)
                {
                    this.GEDR = res; //write with 1-bits clears associated interrupts
                }

                return true;
            }

            [Inline]
            internal bool GetState( int pin )
            {
                uint mask = 1u << (pin & 31);

                return (this.GPLR & mask) != 0;
            }

            [Inline]
            internal void SetState( int  pin  ,
                                    bool fSet )
            {
                uint mask = 1u << (pin & 31);

                if(fSet)
                {
                    this.GPSR |= mask;
                }
                else
                {
                    this.GPCR |= mask;
                }
            }
        }

        [MemoryMappedPeripheral(Base=0x0U,Length=0x0000004CU)]
        class AltFunctionCluster
        {
            [Register(Offset=0x00U)] internal uint GAFR_L; // GPIO<X+15:X   > Alternate Function register
            [Register(Offset=0x04U)] internal uint GAFR_H; // GPIO<X+31:X+16> Alternate Function register

            //
            // Helper Methods
            //

            internal void Set( int  pin  ,
                               uint mode )
            {
                pin %= 32;

                int shift = (pin % 16) * 2;

                uint mask = 3u   << shift;
                uint val  = mode << shift;

                if(pin < 16)
                {
                    this.GAFR_L = (this.GAFR_L & ~mask) | (val & mask);
                }
                else
                {
                    this.GAFR_H = (this.GAFR_H & ~mask) | (val & mask);
                }
            }
        }

#pragma warning disable 649
        [Register(Offset=0x000U)] private Cluster            m_pin000_031;
        [Register(Offset=0x004U)] private Cluster            m_pin032_063;
        [Register(Offset=0x008U)] private Cluster            m_pin064_095;
        [Register(Offset=0x100U)] private Cluster            m_pin096_120;
        [Register(Offset=0x054U)] private AltFunctionCluster m_altFunc000_031;
        [Register(Offset=0x05CU)] private AltFunctionCluster m_altFunc032_063;
        [Register(Offset=0x064U)] private AltFunctionCluster m_altFunc064_095;
        [Register(Offset=0x06CU)] private AltFunctionCluster m_altFunc096_120;
#pragma warning restore 649

        //--//

        //
        // Helper Methods
        //

        public void EnableAsInputAlternateFunction( int pin  ,
                                                    int mode )
        {
            var altFunc = GetAltFunctionCluster( pin );

            altFunc.Set( pin, (uint)mode );

            //--//

            var cls = GetCluster( pin );

            cls.ConfigureAsInput( pin );
        }

        public void EnableAsOutputAlternateFunction( int  pin  ,
                                                     int  mode ,
                                                     bool fSet )
        {
            var altFunc = GetAltFunctionCluster( pin );

            altFunc.Set( pin, (uint)mode );

            //--//

            var cls = GetCluster( pin );

            cls.SetState         ( pin, fSet );
            cls.ConfigureAsOutput( pin       );
        }

        public void EnableAsInputPin( int pin )
        {
            EnableAsInputAlternateFunction( pin, 0 );
        }

        public void EnableAsOutputPin( int  pin  ,
                                       bool fSet )
        {
            EnableAsOutputAlternateFunction( pin, 0, fSet );
        }

        public void ConfigureInterrupt( int  pin         ,
                                        bool raisingEdge ,
                                        bool fallingEdge )
        {
            var cls = GetCluster(pin);

            cls.ConfigureInterrupt( pin, raisingEdge, fallingEdge );
        }

        public bool InterruptPending( int  pin   ,
                                      bool clear )
        {
            var cls = GetCluster(pin);

            return cls.InterruptPending( pin, clear );
        }

        //--//

        Cluster GetCluster( int pin )
        {
            switch(pin / 32)
            {
                case 0: return this.m_pin000_031;
                case 1: return this.m_pin032_063;
                case 2: return this.m_pin064_095;
                case 3: return this.m_pin096_120;
            }

            BugCheck.Raise( BugCheck.StopCode.IncorrectArgument );

            return null;
        }

        AltFunctionCluster GetAltFunctionCluster( int pin )
        {
            switch(pin / 32)
            {
                case 0: return this.m_altFunc000_031;
                case 1: return this.m_altFunc032_063;
                case 2: return this.m_altFunc064_095;
                case 3: return this.m_altFunc096_120;
            }

            BugCheck.Raise( BugCheck.StopCode.IncorrectArgument );

            return null;
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

        public bool this[int pin]
        {
            get
            {
                var cls = GetCluster( pin );

                BugCheck.Assert(null != cls, BugCheck.StopCode.IncorrectArgument);

                return cls.GetState( pin );
            }

            set
            {
                var cls = GetCluster( pin );

                BugCheck.Assert(null != cls, BugCheck.StopCode.IncorrectArgument);

                cls.SetState( pin, value );
            }
        }
    }
}
