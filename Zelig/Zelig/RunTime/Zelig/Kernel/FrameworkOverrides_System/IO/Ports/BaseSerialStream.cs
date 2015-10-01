//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class BaseSerialStream : System.IO.Ports.SerialStream
    {
        public struct Configuration
        {
            //
            // State
            //

            public readonly string                    PortName;
            public          int                       BaudRate;
            public          System.IO.Ports.Parity    Parity;
            public          int                       DataBits;
            public          System.IO.Ports.StopBits  StopBits;
            public          int                       ReadBufferSize;
            public          int                       WriteBufferSize;
            public          int                       ReadTimeout;
            public          int                       WriteTimeout;
            public          System.IO.Ports.Handshake Handshake;
            public          bool                      DtrEnable;
            public          bool                      RtsEnable;
            public          bool                      DiscardNull;
            public          byte                      ParityReplace;

            //
            // Constructor Methods
            //

            public Configuration( string portName )
            {
                this.PortName        = portName;
                this.BaudRate        = 9600;
                this.Parity          = System.IO.Ports.Parity.None;
                this.DataBits        = 8;
                this.StopBits        = System.IO.Ports.StopBits.One;
                this.ReadBufferSize  = 1024;
                this.WriteBufferSize = 1024;
                this.ReadTimeout     = Timeout.Infinite;
                this.WriteTimeout    = Timeout.Infinite;
                this.Handshake       = System.IO.Ports.Handshake.None;
                this.DtrEnable       = false;
                this.RtsEnable       = false;
                this.DiscardNull     = false;
                this.ParityReplace   = 0;
            }
        }

        //
        // State
        //

        protected Configuration                m_cfg;
        protected KernelCircularBuffer< byte > m_receiveQueue;
        protected KernelCircularBuffer< byte > m_transmitQueue;

        //
        // Constructor Methods
        //

        protected BaseSerialStream( ref Configuration cfg )
        {
            m_cfg = cfg;

            m_receiveQueue  = new KernelCircularBuffer< byte >( cfg.ReadBufferSize  );
            m_transmitQueue = new KernelCircularBuffer< byte >( cfg.WriteBufferSize );
        }

        //
        // Helper Methods
        //

        public override void DiscardInBuffer()
        {
            using(SmartHandles.InterruptState.Disable())
            {
                m_receiveQueue.Clear();
            }
        }

        public override void DiscardOutBuffer()
        {
            using(SmartHandles.InterruptState.Disable())
            {
                m_transmitQueue.Clear();
            }
        }

        public override int Read( byte[] array   ,
                                  int    offset  ,
                                  int    count   ,
                                  int    timeout )
        {
            int got = 0;

            while(got < count)
            {
                byte val;

                if(m_receiveQueue.DequeueBlocking( timeout, out val ) == false)
                {
                    if(timeout != 0)
                    {
                        throw new TimeoutException();
                    }

                    break;
                }

                array[offset++] = val;
                got++;
            }

            return got;
        }

        public override int ReadByte( int timeout )
        {
            byte val;

            if(m_receiveQueue.DequeueBlocking( timeout, out val ) == false)
            {
                if(timeout != 0)
                {
                    throw new TimeoutException();
                }

                return -1;
            }

            return val;
        }

        public override void Write( byte[] array   ,
                                    int    offset  ,
                                    int    count   ,
                                    int    timeout )
        {
            for(int pos = 0; pos < count; pos++)
            {
                if(m_transmitQueue.EnqueueBlocking( timeout, array[offset++] ) == false)
                {
                    throw new TimeoutException();
                }
            }
        }

        public override void WriteByte( byte value   ,
                                        int  timeout )
        {
            if(m_transmitQueue.EnqueueBlocking( timeout, value ) == false)
            {
                throw new TimeoutException();
            }
        }

        public override void Flush()
        {
            while(m_transmitQueue.IsEmpty == false)
            {
                Thread.Sleep( 0 );
            }
        }

        //
        // Access Methods
        //

        public override int BaudRate
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool BreakState
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override int DataBits
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool DiscardNull
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool DtrEnable
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override System.IO.Ports.Handshake Handshake
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override System.IO.Ports.Parity Parity
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override byte ParityReplace
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return m_cfg.ReadTimeout;
            }
    
            set
            {
                m_cfg.ReadTimeout = value;
            }
        }

        public override bool RtsEnable
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override System.IO.Ports.StopBits StopBits
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return m_cfg.WriteTimeout;
            }
    
            set
            {
                m_cfg.WriteTimeout = value;
            }
        }

        public override bool CDHolding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CtsHolding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool DsrHolding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int BytesToRead
        {
            get
            {
                return m_receiveQueue.Count;
            }
        }

        public override int BytesToWrite
        {
            get
            {
                return m_transmitQueue.Count;
            }
        }
    }
}
