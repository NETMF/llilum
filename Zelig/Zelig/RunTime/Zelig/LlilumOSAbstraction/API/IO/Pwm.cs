//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using System.Runtime.InteropServices;


    public enum PwmPolarity
    {
        Normal = 0,
        Inverted,
    };

    public enum PwmPrescaler
    {
        Div1 = 0,
        Div2,
        Div4,
        Div8,
        Div16,
        Div64,
        Div256,
        Div1024
    };

    public static class Pwm
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_Initialize(uint pinName, PwmContext** channel);

        [DllImport( "C" )]
        public static unsafe extern void LLOS_PWM_Uninitialize(PwmContext* channel);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_SetDutyCycle(PwmContext* channel, uint dutyCycleNumerator, uint dutyCycleDenominator);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_SetPeriod(PwmContext* channel, uint periodMicroSeconds);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_SetPulseWidth(PwmContext* channel, uint widthMicroSeconds);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_SetPolarity(PwmContext* channel, PwmPolarity polarity);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_SetPrescaler(PwmContext* channel, PwmPrescaler prescaler);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_Start(PwmContext* channel);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_PWM_Stop(PwmContext* channel);
    }

    public unsafe struct PwmContext
    {
    }
}
