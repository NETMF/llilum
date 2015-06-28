// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class:  ObsoleteAttribute
**
**
** Purpose: Attribute for functions, etc that will be removed.
**
**
===========================================================*/
namespace System
{
    using System;
////using System.Runtime.Remoting;
    // This attribute is attached to members that are not to be used any longer.
    // Message is some human readable explanation of what to use
    // Error indicates if the compiler should treat usage of such a method as an
    //   error. (this would be used if the actual implementation of the obsolete
    //   method's implementation had changed).
    //
    [Serializable]
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
        AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate
        , Inherited = false )]
    public sealed class ObsoleteAttribute : Attribute
    {
        private String m_message;
        private bool   m_error;

        public ObsoleteAttribute()
        {
            m_message = null;
            m_error   = false;
        }

        public ObsoleteAttribute( String message )
        {
            m_message = message;
            m_error   = false;
        }

        public ObsoleteAttribute( String message, bool error )
        {
            m_message = message;
            m_error   = error;
        }

        public String Message
        {
            get
            {
                return m_message;
            }
        }

        public bool IsError
        {
            get
            {
                return m_error;
            }
        }
    }
}
