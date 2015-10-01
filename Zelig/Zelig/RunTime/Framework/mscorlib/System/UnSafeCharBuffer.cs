// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  UnSafeBuffer
**
** Purpose: A class to detect incorrect usage of UnSafeBuffer
**
**
===========================================================*/

namespace System
{
    using System.Diagnostics;

    unsafe internal struct UnSafeCharBuffer
    {
        char* m_buffer;
        int   m_totalSize;
        int   m_length;

        public UnSafeCharBuffer( char* buffer, int bufferSize )
        {
            BCLDebug.Assert( buffer != null , "buffer pointer can't be null."  );
            BCLDebug.Assert( bufferSize >= 0, "buffer size can't be negative." );

            m_buffer    = buffer;
            m_totalSize = bufferSize;
            m_length    = 0;
        }

        public void AppendString( string stringToAppend )
        {
            if(String.IsNullOrEmpty( stringToAppend ))
            {
                return;
            }

            if((m_totalSize - m_length) < stringToAppend.Length)
            {
                throw new IndexOutOfRangeException();
            }

            fixed(char* pointerToString = stringToAppend)
            {
                Buffer.InternalMemoryCopy( pointerToString, m_buffer + m_length, stringToAppend.Length );
            }

            m_length += stringToAppend.Length;

            BCLDebug.Assert( m_length <= m_totalSize, "Buffer has been overflowed!" );
        }

        public int Length
        {
            get
            {
                return m_length;
            }
        }
    }
}
