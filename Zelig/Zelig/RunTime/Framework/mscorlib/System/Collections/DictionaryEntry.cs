// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Interface:  DictionaryEntry
**
**
** Purpose: Return Value for IDictionaryEnumerator::GetEntry
**
**
===========================================================*/
namespace System.Collections
{
    using System;

    // A DictionaryEntry holds a key and a value from a dictionary.
    // It is returned by IDictionaryEnumerator::GetEntry().
    [Serializable]
    public struct DictionaryEntry
    {
        private Object m_key;
        private Object m_value;

        // Constructs a new DictionaryEnumerator by setting the Key
        // and Value fields appropriately.
        public DictionaryEntry( Object key, Object value )
        {
            m_key   = key;
            m_value = value;
        }

        public Object Key
        {
            get
            {
                return m_key;
            }

            set
            {
                m_key = value;
            }
        }

        public Object Value
        {
            get
            {
                return m_value;
            }

            set
            {
                m_value = value;
            }
        }
    }
}
