//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Runtime = Microsoft.Zelig.Runtime;

    public class AdcChannel : Llilum.Devices.Adc.AdcChannel
    {
        private unsafe AdcImpl* m_adc;
        private int             m_pinNumber;

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
                tmp_adc_free(m_adc);
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
                fixed (AdcImpl** adc_ptr = &m_adc)
                {
                    tmp_adc_alloc_init(adc_ptr, m_pinNumber);
                }
            }
        }

        public override uint ReadUnsigned()
        {
            unsafe
            {
                return tmp_adc_read_u16(m_adc);
            }
        }

        public override float Read()
        {
            unsafe
            {
                return tmp_adc_read_float(m_adc);
            }
        }

        [DllImport("C")]
        private static unsafe extern void tmp_adc_alloc_init(AdcImpl** obj, int pinNumber);
        [DllImport("C")]
        private static unsafe extern UInt16 tmp_adc_read_u16(AdcImpl* obj);
        [DllImport("C")]
        private static unsafe extern float tmp_adc_read_float(AdcImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_adc_free(AdcImpl* obj);
    }

    internal unsafe struct AdcImpl
    {
    };
}
