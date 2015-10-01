// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Interface:  KeyValuePair
**
**
** Purpose: Generic key-value pair for dictionary enumerators.
**
**
===========================================================*/
namespace System.Collections.Generic
{
    using System;
////using System.Text;

    // A KeyValuePair holds a key and a value from a dictionary.
    // It is returned by IDictionaryEnumerator::GetEntry().
    [Serializable]
    public struct KeyValuePair< TKey, TValue >
    {
        private TKey   m_key;
        private TValue m_value;

        public KeyValuePair( TKey key, TValue value )
        {
            m_key = key;
            m_value = value;
        }

        public TKey Key
        {
            get
            {
                return m_key;
            }
        }

        public TValue Value
        {
            get
            {
                return m_value;
            }
        }

////    public override string ToString()
////    {
////        StringBuilder s = new StringBuilder();
////
////        s.Append( '[' );
////
////        if(Key != null)
////        {
////            s.Append( Key.ToString() );
////        }
////
////        s.Append( ", " );
////
////        if(Value != null)
////        {
////            s.Append( Value.ToString() );
////        }
////
////        s.Append( ']' );
////
////        return s.ToString();
////    }
    }
}
