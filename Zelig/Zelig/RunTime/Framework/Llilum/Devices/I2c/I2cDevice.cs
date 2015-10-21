//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.Devices.I2c
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public class I2cDevice : IDisposable
    {
        private I2cChannel  m_channel;
        private bool        m_open = false;
        private int         m_frequency;

        private const int   defaultFrequency = 100000;

        public I2cDevice(int port)
        {
            I2cChannel newChannel = TryAcquireI2cChannel(port);
            if(newChannel == null)
            {
                throw new ArgumentException();
            }

            Initialize();
            m_channel = newChannel;
        }

        public I2cDevice(II2cChannelInfo channelInfo)
        {
            m_channel = TryAcquireI2cChannel(channelInfo);
            if (m_channel == null)
            {
                throw new ArgumentException();
            }
            Initialize();
        }

        ~I2cDevice()
        {
            Dispose(false);
        }

        private void Initialize()
        {
            m_frequency = defaultFrequency;
        }

        public void Open()
        {
            ThrowIfDisposed();
            
            m_channel.Initialize(m_channel.GetChannelInfo());
            m_channel.SetFrequency(Frequency);
            m_open = true;
        }

        public int Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                ThrowIfDisposed();

                if (value != m_frequency)
                {
                    m_frequency = value;

                    // Cannot make calls to the channel until it is initialized
                    if (m_open)
                    {
                        m_channel.SetFrequency(m_frequency);
                    }
                }
            }
        }

        public int Write(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop)
        {
            ThrowIfDisposed();

            return m_channel.Write(buffer, deviceAddress, transactionStartOffset, transactionLength, sendStop);
        }

        public int Read(byte[] buffer, int deviceAddress, int transactionStartOffset, int transactionLength, bool sendStop)
        {
            ThrowIfDisposed();

            return m_channel.Read(buffer, deviceAddress, transactionStartOffset, transactionLength, sendStop);
        }

        private void ThrowIfDisposed()
        {
            if (m_channel == null)
            {
                throw new ObjectDisposedException(null);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_channel != null)
            {
                if (disposing)
                {
                    m_channel.Dispose();
                    m_channel = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern I2cChannel TryAcquireI2cChannel(int port);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern I2cChannel TryAcquireI2cChannel(II2cChannelInfo channelInfo);
    }
}
