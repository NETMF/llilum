//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;

    public class I2cChannelMbed : Windows.Devices.I2c.Provider.I2cChannel
    {
        private unsafe I2cObj* m_i2c;
        private bool m_disposed;
        private int m_currentFrequency;

        public override int CurrentFrequency
        {
            get
            {
                return m_currentFrequency;
            }
        }

        ~I2cChannelMbed()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            if (!m_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                m_disposed = true;
            }
        }

        private unsafe void Dispose(bool disposing)
        {
            tmp_i2c_free(m_i2c);
        }

        public unsafe override void Initialize(int sdaPin, int sclPin)
        {
            fixed (I2cObj** i2c_ptr = &m_i2c)
            {
                tmp_i2c_alloc(i2c_ptr);
            }
            tmp_i2c_init(m_i2c, sdaPin, sclPin);
        }
        
        public unsafe override void SetFrequency(int hz)
        {
            m_currentFrequency = hz;
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
        private static unsafe extern void tmp_i2c_alloc(I2cObj** obj);
        [DllImport("C")]
        private static unsafe extern void tmp_i2c_init(I2cObj* obj, int sdaPin, int sclPin);
        [DllImport("C")]
        private static unsafe extern void tmp_i2c_free(I2cObj* obj);
        [DllImport("C")]
        private static unsafe extern void tmp_i2c_frequency(I2cObj* obj, int hz);
        [DllImport("C")]
        private static unsafe extern int tmp_i2c_write(I2cObj* obj, int address, byte* data, int length, int stop);
        [DllImport("C")]
        private static unsafe extern int tmp_i2c_read(I2cObj* obj, int address, byte* data, int length, int stop);
    }

    internal unsafe struct I2cObj
    {
    };
}
