//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM0OnMBED.HardwareModel
{
    using System;
    using System.Runtime.InteropServices;
    using Llilum.Devices.I2c;
    using RT   = Microsoft.Zelig.Runtime;
    using LLOS = Zelig.LlilumOSAbstraction;
    using LLIO = Zelig.LlilumOSAbstraction.API.IO;

    public class I2cChannel : Llilum.Devices.I2c.I2cChannel
    {
        private unsafe LLIO.I2CContext* m_i2c;
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
             Dispose(true);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (m_i2c != null)
            {
                LLIO.I2C.LLOS_I2C_Uninitialize( m_i2c );
                m_i2c = null;

                if (disposing)
                {
                    RT.HardwareProvider.Instance.ReleasePins(m_channelInfo.SclPin, m_channelInfo.SdaPin);
                    GC.SuppressFinalize(this);
                }
            }
        }

        public override II2cChannelInfo GetChannelInfo()
        {
            return m_channelInfo;
        }

        public unsafe override void Initialize(II2cChannelInfo channelInfo)
        {
            fixed (LLIO.I2CContext** i2c_ptr = &m_i2c)
            {
                LLIO.I2C.LLOS_I2C_Initialize(channelInfo.SdaPin, channelInfo.SclPin, i2c_ptr);
            }
        }

        public unsafe override void SetFrequency(int hz)
        {
            LLIO.I2C.LLOS_I2C_SetFrequency(m_i2c, (uint)hz);
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
                int bytesRead = transactionLength;
                fixed (byte* pBuffer = &buffer[0])
                {
                    LLOS.LlilumErrors.ThrowOnError( LLIO.I2C.LLOS_I2C_Read( m_i2c, (uint)deviceAddress, pBuffer, transactionStartOffset, &bytesRead, ( sendStop ? 1u : 0u ) ), true );

                    return bytesRead;
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
                int bytesWritten = transactionLength;

                fixed (byte* pBuffer = &buffer[0])
                {
                    LLOS.LlilumErrors.ThrowOnError( LLIO.I2C.LLOS_I2C_Write( m_i2c, (uint)deviceAddress, pBuffer, transactionStartOffset, &bytesWritten, ( sendStop ? 1u : 0u ) ), true );
                    return bytesWritten;
                }
            }
        }
    }
}
