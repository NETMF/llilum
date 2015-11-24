//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Runtime = Microsoft.Zelig.Runtime;
    using LLIO = Zelig.LlilumOSAbstraction.API.IO;

    public class AdcChannel : Llilum.Devices.Adc.AdcChannel
    {
        private unsafe LLIO.AdcContext* m_adc;
        private int                     m_pinNumber;

        internal AdcChannel(int pinNumber)
        {
            m_pinNumber = pinNumber;
        }

        ~AdcChannel()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (m_adc != null)
            {
                LLIO.Adc.LLOS_ADC_Uninitialize(m_adc);
                m_adc = null;

                if (disposing)
                {
                    Runtime.HardwareProvider.Instance.ReleasePins(m_pinNumber);
                    GC.SuppressFinalize(this);
                }
            }
        }

        public override void InitializePin()
        {
            unsafe
            {
                fixed (LLIO.AdcContext** adc_ptr = &m_adc)
                {
                    LLIO.Adc.LLOS_ADC_Initialize((uint)m_pinNumber, LLIO.AdcDirection.Input, adc_ptr);
                }
            }
        }

        public override uint ReadUnsigned()
        {
            int value = 0;

            unsafe
            {
                LLIO.Adc.LLOS_ADC_ReadRaw(m_adc, &value);
            }

            return (uint)value;
        }

        public override float Read()
        {
            float result = 0f;

            unsafe
            {
                LLIO.Adc.LLOS_ADC_Read(m_adc, &result);
            }

            return result;
        }
    }
}
