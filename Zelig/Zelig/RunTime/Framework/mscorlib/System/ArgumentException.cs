// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ArgumentException
**
**
** Purpose: Exception class for invalid arguments to a method.
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

    // The ArgumentException is thrown when an argument does not meet
    // the contract of the method.  Ideally it should give a meaningful error
    // message describing what was wrong and which parameter is incorrect.
    //
    [Serializable]
    public class ArgumentException : SystemException /*, ISerializable*/
    {
        private String m_paramName;

        // Creates a new ArgumentException with its message
        // string set to the empty string.
#if EXCEPTION_STRINGS
        public ArgumentException() : base( Environment.GetResourceString( "Arg_ArgumentException" ) )
#else
        public ArgumentException()
#endif
        {
        }

        // Creates a new ArgumentException with its message
        // string set to message.
        //
        public ArgumentException( String message ) : base( message )
        {
        }

        public ArgumentException( String message, Exception innerException ) : base( message, innerException )
        {
        }

        public ArgumentException( String message, String paramName, Exception innerException ) : base( message, innerException )
        {
            m_paramName = paramName;
        }

        public ArgumentException( String message, String paramName ) : base( message )
        {
            m_paramName = paramName;
        }

////    protected ArgumentException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////        m_paramName = info.GetString( "ParamName" );
////    }

        public override String Message
        {
            get
            {
                return m_message + " " + m_paramName;
////            String s = base.Message;
////
////            if(!((m_paramName == null) || (m_paramName.Length == 0)))
////            {
////                return s + Environment.NewLine + String.Format( CultureInfo.CurrentCulture, Environment.GetResourceString( "Arg_ParamName_Name" ), m_paramName );
////            }
////            else
////            {
////                return s;
////            }
            }
        }

        public virtual String ParamName
        {
            get
            {
                return m_paramName;
            }
        }

////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    public override void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        base.GetObjectData( info, context );
////
////        info.AddValue( "ParamName", m_paramName, typeof( String ) );
////    }
    }
}
