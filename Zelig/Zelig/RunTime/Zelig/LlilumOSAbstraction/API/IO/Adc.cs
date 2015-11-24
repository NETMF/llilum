//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API.IO
{
    using System.Runtime.InteropServices;

    public enum AdcDirection
    {
        Input = 0,
        Output,
    };

    public static class Adc
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_ADC_Initialize(uint pinName, AdcDirection direction, AdcContext** channel);

        [DllImport( "C" )]
        public static unsafe extern void LLOS_ADC_Uninitialize(AdcContext* channel);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_ADC_ReadRaw(AdcContext* channel, int* value);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_ADC_WriteRaw(AdcContext* channel, int value);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_ADC_Read(AdcContext* channel, float* value);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_ADC_Write(AdcContext* channel, float value);

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_ADC_GetPrecisionBits(AdcContext* channel, uint* pPrecisionBits);
    }

    public unsafe struct AdcContext
    {
    };
}
