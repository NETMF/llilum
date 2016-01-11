//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.STM32F401.HardwareModel.HardwareProviders
{
    using System;
    using Chipset = CortexM4OnMBED;

    public sealed class GpioProvider : Chipset.HardwareModel.GpioProvider
    {
        public override int GetGpioPinIRQNumber(int pinNumber)
        {
            int pinIndex = pinNumber & 0xF;

            switch (pinIndex)
            {
                case 0:
                    return (int)IRQn.EXTI0_IRQn;
                case 1:
                    return (int)IRQn.EXTI1_IRQn;
                case 2:
                    return (int)IRQn.EXTI2_IRQn;
                case 3:
                    return (int)IRQn.EXTI3_IRQn;
                case 4:
                    return (int)IRQn.EXTI4_IRQn;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    return (int)IRQn.EXTI9_5_IRQn;
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    return (int)IRQn.EXTI15_10_IRQn;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
