// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ArgumentOutOfRangeException
**
**
** Purpose: Exception class for method arguments outside of the legal range.
**
**
=============================================================================*/

namespace System
{
    using System;
////using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Globalization;
////using System.Security.Permissions;

    // The ArgumentOutOfRangeException is thrown when an argument
    // is outside the legal range for that argument.
    [Serializable]
    public class ArgumentOutOfRangeException : ArgumentException /*, ISerializable*/
    {
        private Object m_actualValue;

        // Creates a new ArgumentOutOfRangeException with its message
        // string set to a default message explaining an argument was out of range.
        public ArgumentOutOfRangeException() : base( RangeMessage )
        {
        }

        public ArgumentOutOfRangeException( String paramName ) : base( RangeMessage, paramName )
        {
        }

        public ArgumentOutOfRangeException( String paramName, String message ) : base( message, paramName )
        {
        }

        public ArgumentOutOfRangeException( String message, Exception innerException ) : base( message, innerException )
        {
        }

        // We will not use this in the classlibs, but we'll provide it for
        // anyone that's really interested so they don't have to stick a bunch
        // of printf's in their code.
        public ArgumentOutOfRangeException( String paramName, Object actualValue, String message ) : base( message, paramName )
        {
            m_actualValue = actualValue;
        }

////    public override String Message
////    {
////        get
////        {
////            String s = base.Message;
////            if(m_actualValue != null)
////            {
////                String valueMessage = String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "ArgumentOutOfRange_ActualValue" ), m_actualValue.ToString() );
////                if(s == null)
////                {
////                    return valueMessage;
////                }
////
////                return s + Environment.NewLine + valueMessage;
////            }
////
////            return s;
////        }
////    }

        // Gets the value of the argument that caused the exception.
        // Note - we don't set this anywhere in the class libraries in
        // version 1, but it might come in handy for other developers who
        // want to avoid sticking printf's in their code.
        public virtual Object ActualValue
        {
            get
            {
                return m_actualValue;
            }
        }

        private static String RangeMessage
        {
            get
            {
#if EXCEPTION_STRINGS
                return Environment.GetResourceString( "Arg_ArgumentOutOfRangeException" );
#else
                return null;
#endif
            }
        }

////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public override void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        base.GetObjectData( info, context );
////
////        info.AddValue( "ActualValue", m_actualValue, typeof( Object ) );
////    }
////
////    protected ArgumentOutOfRangeException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////        m_actualValue = info.GetValue( "ActualValue", typeof( Object ) );
////    }
    }
}
