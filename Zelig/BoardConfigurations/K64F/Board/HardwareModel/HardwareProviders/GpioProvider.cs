//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.K64F.HardwareModel.HardwareProviders
{
    using System;
    using Chipset = CortexM4OnMBED;

    public sealed class GpioProvider : Chipset.HardwareModel.GpioProvider
    {
        //////public override void RemapInterrupts()
        //////{
        //////    Processor.RemapInterrupt( IRQn.PORTA_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.PORTB_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.PORTC_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.PORTD_IRQn  ); 
        //////    Processor.RemapInterrupt( IRQn.PORTE_IRQn  ); 
        //////}

        public override int GetGpioPinIRQNumber(int pinNumber)
        {
            PinName pin = (PinName)pinNumber;

            if (PinName.PTA0 <= pin && pin < PinName.PTB0)
            {
                return (int)IRQn.PORTA_IRQn;
            }
            else if (PinName.PTB0 <= pin && pin < PinName.PTC0)
            {
                return (int)IRQn.PORTB_IRQn;
            }
            else if (PinName.PTC0 <= pin && pin < PinName.PTD0)
            {
                return (int)IRQn.PORTC_IRQn;
            }
            else if (PinName.PTD0 <= pin && pin < PinName.PTE0)
            {
                return (int)IRQn.PORTD_IRQn;
            }
            else if (PinName.PTE0 <= pin && pin <= PinName.PTE31)
            {
                return (int)IRQn.PORTB_IRQn;
            }

            throw new NotSupportedException();
        }
    }
}
