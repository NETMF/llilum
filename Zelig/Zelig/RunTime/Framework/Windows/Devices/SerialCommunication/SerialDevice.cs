//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Runtime.InteropServices;
using System.IO.Ports;

namespace Windows.Devices.SerialCommunication
{
    using Windows.Internal;
    using Windows.Foundation;
    using Windows.Storage.Streams;
    using Ports = global::System.IO.Ports;

    public sealed class SerialDevice : IDisposable
    {
        private SerialPort m_serialPort;
        private SerialInputStream m_inputStream;
        private SerialOutputStream m_outputStream;

        internal SerialDevice(SerialPort serialPort)
        {
            m_serialPort = serialPort;
            m_serialPort.ErrorReceived += CatchErrorEvents;
            m_serialPort.PinChanged += CatchPinChangedEvents;

            m_inputStream = new SerialInputStream(m_serialPort);
            m_outputStream = new SerialOutputStream(m_serialPort);
        }

        public event TypedEventHandler<SerialDevice, ErrorReceivedEventArgs> ErrorReceived;

        public event TypedEventHandler<SerialDevice, PinChangedEventArgs> PinChanged;

        public bool IsDataTerminalReadyEnabled
        {
            get
            {
                return m_serialPort.DtrEnable;
            }
            set
            {
                m_serialPort.DtrEnable = value;
            }
        }

        public ushort DataBits
        {
            get
            {
                return (ushort)m_serialPort.DataBits;
            }
            set
            {
                m_serialPort.DataBits = value;
            }
        }

        public SerialHandshake Handshake
        {
            get
            {
                return (SerialHandshake)m_serialPort.Handshake;
            }
            set
            {
                m_serialPort.Handshake = (Handshake)value;
            }
        }

        public bool BreakSignalState
        {
            get
            {
                return m_serialPort.BreakState;
            }
            set
            {
                m_serialPort.BreakState = value;
            }
        }

        public uint BaudRate
        {
            get
            {
                return (uint)m_serialPort.BaudRate;
            }
            set
            {
                m_serialPort.BaudRate = (int)value;
            }
        }

        public TimeSpan WriteTimeout
        {
            get
            {
                return new TimeSpan(m_serialPort.WriteTimeout);
            }
            set
            {
                m_serialPort.WriteTimeout = (int)value.TotalMilliseconds;
            }
        }

        public SerialStopBitCount StopBits
        {
            get
            {
                return (SerialStopBitCount)m_serialPort.StopBits;
            }
            set
            {
                m_serialPort.StopBits = (StopBits)value;
            }
        }

        public TimeSpan ReadTimeout
        {
            get
            {
                return new TimeSpan(m_serialPort.ReadTimeout);
            }
            set
            {
                m_serialPort.ReadTimeout = (int)value.TotalMilliseconds;
            }
        }

        public SerialParity Parity
        {
            get
            {
                return (SerialParity)m_serialPort.Parity;
            }
            set
            {
                m_serialPort.Parity = (Parity)value;
            }
        }

        public bool IsRequestToSendEnabled
        {
            get
            {
                return m_serialPort.RtsEnable;
            }
            set
            {
                m_serialPort.RtsEnable = value;
            }
        }

        public uint BytesReceived => (uint)m_serialPort.BytesToRead;

        public bool CarrierDetectState => m_serialPort.CDHolding;

        public bool ClearToSendState => m_serialPort.CtsHolding;

        public bool DataSetReadyState => m_serialPort.DsrHolding;

        public IInputStream InputStream => m_inputStream;

        public IOutputStream OutputStream => m_outputStream;


        public string PortName => m_serialPort.PortName;

        public ushort UsbProductId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ushort UsbVendorId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static string GetDeviceSelector()
        {
           string[] ports = SerialPort.GetPortNames();

            if(ports == null || ports.Length == 0)
            {
                return null;
            }
            return ports[0];
        }
        
        public static string GetDeviceSelector(string portName)
        {
            return portName;
        }

        public static string GetDeviceSelectorFromUsbVidPid(ushort vendorId, ushort productId)
        {
            throw new NotImplementedException();
        }

        public static IAsyncOperation<SerialDevice> FromIdAsync(string deviceId)
        {
            try
            {
                SerialPort port = new SerialPort(deviceId);
                SerialDevice device = new SerialDevice(port);

                return new SynchronousOperation<SerialDevice>(device);
            }
            catch(Exception ex)
            {
                return new SynchronousOperation<SerialDevice>(ex);
            }
        }

        void Dispose(bool disposing)
        {
            if (m_serialPort != null)
            {
                if (disposing)
                {
                    m_outputStream.Dispose();
                    m_inputStream.Dispose();
                    m_serialPort.Dispose();
                }

                m_serialPort = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void CatchErrorEvents(object src, SerialErrorReceivedEventArgs e)
        {
            TypedEventHandler<SerialDevice, ErrorReceivedEventArgs> eventHandler = ErrorReceived;
            SerialPort serialPort = m_serialPort;

            if ((eventHandler != null) && (serialPort != null))
            {
                lock (serialPort)
                {
                    if (serialPort.IsOpen)
                    {
                        eventHandler(this, GetErrorReceivedEventArgs(e));
                    }
                }
            }
        }

        private void CatchPinChangedEvents(object src, SerialPinChangedEventArgs e)
        {
            TypedEventHandler<SerialDevice, PinChangedEventArgs> eventHandler = PinChanged;
            SerialPort serialPort = m_serialPort;

            if ((eventHandler != null) && (serialPort != null))
            {
                lock (serialPort)
                {
                    if (serialPort.IsOpen)
                    {
                        eventHandler(this, GetPinChangedEventArgs(e));
                    }
                }
            }
        }

        private ErrorReceivedEventArgs GetErrorReceivedEventArgs(SerialErrorReceivedEventArgs e)
        {
            ErrorReceivedEventArgs args;
            
            switch (e.EventType)
            {
                case Ports.SerialError.TXFull:
                    args = new ErrorReceivedEventArgs(SerialError.TransmitFull);
                    break;
                case Ports.SerialError.RXOver:
                    args = new ErrorReceivedEventArgs(SerialError.ReceiveFull);
                    break;
                case Ports.SerialError.RXParity:
                    args = new ErrorReceivedEventArgs(SerialError.ReceiveParity);
                    break;
                case Ports.SerialError.Overrun:
                    args = new ErrorReceivedEventArgs(SerialError.BufferOverrun);
                    break;
                case Ports.SerialError.Frame:
                    args = new ErrorReceivedEventArgs(SerialError.Frame);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return args;
        }

        private PinChangedEventArgs GetPinChangedEventArgs(SerialPinChangedEventArgs e)
        {
            PinChangedEventArgs args;
            switch (e.EventType)
            {
                case Ports.SerialPinChange.Break:
                    args = new PinChangedEventArgs(SerialPinChange.BreakSignal);
                    break;
                case Ports.SerialPinChange.CDChanged:
                    args = new PinChangedEventArgs(SerialPinChange.CarrierDetect);
                    break;
                case Ports.SerialPinChange.CtsChanged:
                    args = new PinChangedEventArgs(SerialPinChange.ClearToSend);
                    break;
                case Ports.SerialPinChange.DsrChanged:
                    args = new PinChangedEventArgs(SerialPinChange.DataSetReady);
                    break;
                case Ports.SerialPinChange.Ring:
                    args = new PinChangedEventArgs(SerialPinChange.RingIndicator);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return args;
        }
    }
}
