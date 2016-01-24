//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F091.HardwareModel.HardwareProviders
{
    using System;
    using Chipset = CortexM0OnMBED;

    public sealed class GpioProvider : Chipset.HardwareModel.GpioProvider
    {
        public override int GetGpioPinIRQNumber(int pinNumber)
        {
            int pinIndex = pinNumber & 0xF;

            switch (pinIndex)
            {
                case 0:
                case 1:
                    return (int)IRQn.EXTI0_1_IRQn;
                case 2:
                case 3:
                    return (int)IRQn.EXTI2_3_IRQn;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    return (int)IRQn.EXTI4_15_IRQn;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
