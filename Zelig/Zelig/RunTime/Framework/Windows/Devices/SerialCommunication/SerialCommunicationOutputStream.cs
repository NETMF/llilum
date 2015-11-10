//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Runtime.InteropServices;
using System.IO.Ports;

namespace Windows.Devices.SerialCommunication
{
    using Windows.Foundation;
    using Windows.Storage.Streams;
    using Windows.Internal;
    
    internal class SerialOutputStream : IOutputStream
    {
        private SerialPort m_serialPort;

        public SerialOutputStream(SerialPort serialPort)
        {
            m_serialPort = serialPort;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_serialPort?.Dispose();
            }
            m_serialPort = null;
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            return new SynchronousOperation<bool>(true);
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            EnsurePortOpen();

            ByteBuffer bufferImpl = (ByteBuffer)buffer;
            uint length = bufferImpl.Length;

            try
            {
                m_serialPort.Write(bufferImpl.Data, 0, (int)length);
                return new SynchronousOperationWithProgress<uint, uint>(length);
            }
            catch(Exception ex)
            {
                return new SynchronousOperationWithProgress<uint, uint>(ex);
            }
        }

        private void EnsurePortOpen()
        {
            if (!m_serialPort.IsOpen)
            {
                m_serialPort.Open();
            }
        }
    }
}
