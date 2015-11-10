//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

using System;
using System.Collections.Generic;
using Windows.Storage.Streams;

namespace Windows.Internal
{
    internal sealed class ByteBuffer : IBuffer
    {
        private byte[] m_buffer;

        public ByteBuffer(uint capacity)
        {
            m_buffer = new byte[(int)capacity];
        }

        public uint Capacity
        {
            get
            {
                return (uint)m_buffer.Length;
            }
        }

        public uint Length
        {
            get;
            set;
        }
        

        public byte[] Data
        {
            get
            {
                return m_buffer;
            }
            set
            {
                m_buffer = value;
            }
        }
    }
}
