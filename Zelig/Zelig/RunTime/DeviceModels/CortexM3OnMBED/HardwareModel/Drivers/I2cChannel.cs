//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Llilum.Devices.I2c;
    using RT = Microsoft.Zelig.Runtime;

    public class I2cChannel : Llilum.Devices.I2c.I2cChannel
    {
        private unsafe I2cImpl* m_i2c;
        private II2cChannelInfo m_channelInfo;

        public I2cChannel(II2cChannelInfo channelInfo)
        {
            m_channelInfo = channelInfo;
        }

        ~I2cChannel()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            unsafe
            {
                if (m_i2c != null)
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
            }
        }

        private unsafe void Dispose(bool disposing)
        {
            if(disposing)
            {
                RT.HardwareProvider.Instance.ReleasePins(m_channelInfo.SclPin, m_channelInfo.SdaPin);
            }

            tmp_i2c_free(m_i2c);
            m_i2c = null;
        }

        public override II2cChannelInfo GetChannelInfo()
        {
            return m_channelInfo;
        }

        public unsafe override void Initialize(II2cChannelInfo channelInfo)
        {
            fixed (I2cImpl** i2c_ptr = &m_i2c)
            {
                tmp_i2c_alloc(i2c_ptr);
            }
            tmp_i2c_init(m_i2c, channelInfo.SdaPin, channelInfo.SclPin);
        }

        public unsafe override void SetFrequency(int hz)
        {
            tmp_i2c_frequency(m_i2c, hz);
        }

        public override int Read(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop)
        {
            // Ensure buffer isn't null or empty
            if (buffer == null || buffer.Length <= 0)
            {
                throw new ArgumentException();
            }

            // Ensure accurate bounds for the transaction
            if (transactionStartOffset < 0 ||
                buffer.Length < (transactionStartOffset + transactionLength))
            {
                throw new ArgumentException();
            }

            unsafe
            {
                fixed (byte* start = &buffer[transactionStartOffset])
                {
                    return tmp_i2c_read(m_i2c, deviceAddress, start, transactionLength, (sendStop ? 1 : 0));
                }
            }
        }

        public override int Write(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop)
        {
            // Ensure buffer isn't null or empty
            if (buffer == null || buffer.Length <= 0)
            {
                throw new ArgumentException();
            }

            // Ensure accurate bounds for the transaction
            if (transactionStartOffset < 0 ||
                buffer.Length < (transactionStartOffset + transactionLength))
            {
                throw new ArgumentException();
            }

            unsafe
            {
                fixed (byte* start = &buffer[transactionStartOffset])
                {
                    return tmp_i2c_write(m_i2c, deviceAddress, start, transactionLength, (sendStop ? 1 : 0));
                }
            }
        }

        [DllImport("C")]
        private static unsafe extern void tmp_i2c_alloc(I2cImpl** obj);
        [DllImport("C")]
        private static unsafe extern void tmp_i2c_init(I2cImpl* obj, int sdaPin, int sclPin);
        [DllImport("C")]
        private static unsafe extern void tmp_i2c_free(I2cImpl* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_i2c_frequency(I2cImpl* obj, int hz);
        [DllImport("C")]
        private static unsafe extern int tmp_i2c_write(I2cImpl* obj, int address, byte* data, int length, int stop);
        [DllImport("C")]
        private static unsafe extern int tmp_i2c_read(I2cImpl* obj, int address, byte* data, int length, int stop);
    }

    internal unsafe struct I2cImpl
    {
    };
}
