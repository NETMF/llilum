//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

#define BUG_67_ACTIVE

using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Windows.Devices.SerialCommunication
{
    using Windows.Foundation;
    using Windows.Storage.Streams;
    using Windows.Internal;

    internal class SerialInputStream : IInputStream
    {
        private SerialPort m_serialPort;

        public SerialInputStream(SerialPort serialPort)
        {
            m_serialPort = serialPort;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if(disposing)
            {
                m_serialPort?.Dispose();
            }
            m_serialPort = null;
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {
            EnsurePortOpen();

            ByteBuffer memBuffer = (ByteBuffer)buffer;
            if (memBuffer == null || count > memBuffer.Capacity)
            {
                memBuffer = new ByteBuffer(count);
            }

            try
            {
                int readBytes;

#if(BUG_67_ACTIVE)
                readBytes = m_serialPort.Read(memBuffer.Data, 0, (int)count);

                if(options != InputStreamOptions.Partial)
                {
                    while(readBytes < count)
                    {
                        readBytes += m_serialPort.Read(memBuffer.Data, readBytes, ((int)count - readBytes));
                    }
                }
                memBuffer.Length = (uint)readBytes;
                return new SynchronousOperationWithProgress<IBuffer, uint>(memBuffer);
#else
                // BUG: https://github.com/NETMF/llilum/issues/67
                // TODO: This task causes an exception in the Llilum compiler. Need to resolve
                Task<IBuffer> readTask = new Task<IBuffer>(() =>
                {
                    readBytes = m_serialPort.Read(memBuffer.Data, 0, (int)count);

                    if (options != InputStreamOptions.Partial)
                    {
                        while (readBytes < count)
                        {
                            readBytes += m_serialPort.Read(memBuffer.Data, readBytes, ((int)count - readBytes));
                        }
                    }
                    memBuffer.Length = (uint)readBytes;
                    return memBuffer;
                });

                readTask.Start();
                return WindowsRuntimeSystemExtensions.AsAsyncOperationWithProgress<IBuffer, uint>(readTask);
#endif
            }
            catch (Exception ex)
            {
                return new SynchronousOperationWithProgress<IBuffer, uint>(ex);
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
