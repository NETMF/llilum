// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class: CharEnumerator
**
**
** Purpose: Enumerates the characters on a string.  skips range
**          checks.
**
**
============================================================*/
namespace System
{
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    public sealed class CharEnumerator : IEnumerator, ICloneable, IEnumerator<char>
    {
        private String m_str;
        private int    m_index;
        private char   m_currentElement;

        internal CharEnumerator( String str )
        {
            m_str   = str;
            m_index = -1;
        }

        public Object Clone()
        {
            return MemberwiseClone();
        }

        public bool MoveNext()
        {
            if(m_index < (m_str.Length - 1))
            {
                m_index++;
                m_currentElement = m_str[m_index];
                return true;
            }
            else
            {
                m_index = m_str.Length;
            }

            return false;

        }

        void IDisposable.Dispose()
        {
        }

        /// <internalonly/>
        Object IEnumerator.Current
        {
            get
            {
                if(m_index == -1)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                    throw new InvalidOperationException();
#endif
                }
                if(m_index >= m_str.Length)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                return m_currentElement;
            }
        }

        public char Current
        {
            get
            {
                if(m_index == -1)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumNotStarted" ) );
#else
                    throw new InvalidOperationException();
#endif
                }
                if(m_index >= m_str.Length)
                {
#if EXCEPTION_STRINGS
                    throw new InvalidOperationException( Environment.GetResourceString( "InvalidOperation_EnumEnded" ) );
#else
                    throw new InvalidOperationException();
#endif
                }

                return m_currentElement;
            }
        }

        public void Reset()
        {
            m_currentElement = (char)0;
            m_index          = -1;
        }
    }
}
