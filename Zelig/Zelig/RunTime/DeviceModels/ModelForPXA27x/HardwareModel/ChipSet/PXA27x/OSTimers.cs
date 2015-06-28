//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Chipset.PXA27x
{
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.Zelig.Runtime;


    [MemoryMappedPeripheral(Base=0x40A00000U,Length=0x000000C0U)]
    public class OSTimers
    {
        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct OMCR_bitfield
        {
            public enum Synchronization
            {
                NoSync   = 0, // 0b00 = No external synchronization
                ExySync0 = 1, // 0b01 = Reset OSCRx on the rising edge of EXT_SYNC<0>.
                ExySync1 = 2, // 0b10 = Reset OSCRx on the rising edge of EXT_SYNC<1>.
                              // 0b11 = reserved
            }

            public enum Resolution
            {
                Disable        = 0, // 0b0000 = The counter is disabled.
                Freq32KHz      = 1, // 0b0001 = 1/32768th of a second
                Freq1KHz       = 2, // 0b0010 = 1 millisecond. The interval between clock increments averages one millisecond, but the time between individual clock increments varies because the counter resolution is derived from the 32.768-kHz clock.
                Freq1Hz        = 3, // 0b0011 = 1 second
                Freq1MHz       = 4, // 0b0100 = 1 microsecond
                ExtClock       = 5, // 0b0101 = Externally supplied clock. The counter resolution is the clock period of the externally supplied clock.
                FreqSSP1Detect = 6, // 0b0110 = SSP1 Frame Detect. The counter resolution is the SSP1 frame detect rate.
                FreqSSP2Detect = 7, // 0b0111 = SSP2 Frame Detect. The counter resolution is the SSP2 frame detect rate.
                FreqSSP3Detect = 8, // 0b1000 = SSP3 Frame Detect. The counter resolution is the SSP3 frame detect rate.
                FreqUDCDetect  = 9, // 0b1001 = UDC Frame Detect. The counter resolution is the UDC frame detect rate.
                                    // 0b1010–0b1111 = reserved
            }

            [BitFieldRegister     (Position=9                )] public bool            N;    // Snapshot Mode
                                                                                             //
                                                                                             // Channel 9:
                                                                                             //  0 = Snapshot mode is disabled.
                                                                                             //  1 = Read from OSCR9 copies contents of OSCR8 to OSNR.
                                                                                             // Channel 11:
                                                                                             //  0 = Snapshot mode is disabled.
                                                                                             //  1 = Read from OSCR11 copies contents of OSCR10 to OSNR.
                                                                                             //
            [BitFieldRegister     (Position=7                )] public bool            C;    // Channel X Match Against
                                                                                             //
                                                                                             // X = 4-7
                                                                                             //
                                                                                             //  0 = Channel x is compared to OSCR4 and OSCRx is not incremented.
                                                                                             //  1 = Channel x is compared to OSCRx and a write to OSCRx starts the channel.
                                                                                             //  NOTE: For channel 4, the counter always operates as if this bit is set.
                                                                                             //
                                                                                             // X = 8-11
                                                                                             //
                                                                                             //  0 = Channel x is compared to OSCR8 and OSCRx is not incremented.
                                                                                             //  1 = Channel x is compared to OSCRx and a write to OSCRx starts the channel.
                                                                                             //  NOTE: For channel 8, the counter always operates as if this bit is set.
                                                                                             //
            [BitFieldRegister     (Position=6                )] public bool            P;    // Periodic Timer
                                                                                             //
                                                                                             //  0 = The channel stops incrementing after detecting a match.
                                                                                             //  1 = The channel continues incrementing after detecting a match.
                                                                                             //
            [BitFieldRegister     (Position=4,Size=2         )] public Synchronization S;    // External Synchronization Control
                                                                                             //                                                            
            [BitFieldRegister     (Position=3                )] public bool            R;    // Reset OSCRx on Match
                                                                                             //
                                                                                             //   0 = Do not reset OSCRx on match.
                                                                                             //   1 = Reset OSCRx on match.
                                                                                             //
            [BitFieldSplitRegister(Position=8,Size=1,Offset=3)]                              //
            [BitFieldSplitRegister(Position=0,Size=3,Offset=0)] public Resolution      CRES; // Counter Resolution
                                                                                             // 
                                                                                             // Any channel using a counter that is derived from the 32.768-kHz clock
                                                                                             // continues to operate in standby and sleep mode. This applies to the values of CRES between 0x1 and 0x3.
                                                                                             // Any channel not using a counter that is derived from the 32.768-kHz clock
                                                                                             // stops incrementing in standby, sleep, or deep-sleep mode. This applies to the values of CRES between 0x4 and 0x9
                                                                                             // 
        }

        [BitFieldPeripheral(PhysicalType=typeof(uint))]
        public struct OWER_bitfield
        {
            [BitFieldRegister(Position=0)] public bool WME; // Watchdog Match Enable
                                                            //
                                                            //  0 = OSMR3 match with OSCR0 does not cause a reset of the processor.
                                                            //  1 = OSMR3 match with OSCR0 causes a reset of the processor.
                                                            //
        }

#pragma warning disable 649 // private array fields are never assigned
        [Register(Offset=0x10            )] private uint            OSCR0;    // OS Timer Counter 0 register                                                   | 22-17
        [Register(Offset=0x40,Instances=8)] private uint[]          OSCR4_11; // OS Timer Counter 4-11 register                                                | 22-17
        [Register(Offset=0x14            )] public  uint            OSSR;     // OS Timer Status register (used for all counters)                              | 22-18
        [Register(Offset=0x18            )] public  uint            OWER;     // OS Timer Watchdog Enable register                                             | 22-16
        [Register(Offset=0x1C            )] public  uint            OIER;     // OS Timer Interrupt Enable register (used for all counters)                    | 22-16
        [Register(Offset=0x20            )] public  uint            OSNR;     // OS Timer Snapshot register                                                    | 22-19
        [Register(Offset=0x00,Instances=4)] private uint[]          OSMR0_3;  // OS Timer Match 0-3 register                                                   | 22-15
        [Register(Offset=0x80,Instances=8)] private uint[]          OSMR4_11; // OS Timer Match 4-11 register                                                  | 22-15
        [Register(Offset=0xC0,Instances=8)] private OMCR_bitfield[] OMCR4_11; // OS Match Control 4-11 register                                                | 22-9 through 22-13
#pragma warning restore 649

        //
        // Helper Methods
        //

        [Inline]
        public void SetControl( int           numTimer ,
                                OMCR_bitfield val      )
        {
            if(numTimer < 4)
            {
                // There's no control for these timers.
            }
            else if(numTimer < 12)
            {
                this.OMCR4_11[numTimer - 4] = val;
            }
        }

        [Inline]
        public void SetMatch( int  numTimer ,
                              uint val      )
        {
            if(numTimer < 4)
            {
                this.OSMR0_3[numTimer] = val;
            }
            else if(numTimer < 12)
            {
                this.OSMR4_11[numTimer - 4] = val;
            }
        }

        [Inline]
        public void WriteCounter( int  numTimer ,
                                  uint val      )
        {
            if(numTimer < 4)
            {
                this.OSCR0 = val;
            }
            else if(numTimer < 12)
            {
                this.OSCR4_11[numTimer - 4] = val;
            }
        }
        
        [Inline]
        public uint ReadCounter( int numTimer )
        {
            if(numTimer < 4)
            {
                return this.OSCR0;
            }
            else if(numTimer < 12)
            {
                return this.OSCR4_11[numTimer - 4];
            }
            else
            {
                return 0;
            }
        }
        
        [Inline]
        public bool HasFired( int numTimer )
        {
            return (this.OSSR & (1u << numTimer)) != 0;
        }

        [Inline]
        public void ClearFired( int numTimer )
        {
            this.OSSR = (1u << numTimer);
        }

        [Inline]
        public void EnableInterrupt( int numTimer )
        {
            this.OIER |= (1u << numTimer);
        }

        [Inline]
        public void DisableInterrupt( int numTimer )
        {
            this.OIER &= ~(1u << numTimer);
        }

        //
        // Access Methods
        //

        public static extern OSTimers Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}