// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ArgumentNullException
**
**
** Purpose: Exception class for null arguments to a method.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;
////using System.Runtime.Remoting;
////using System.Security.Permissions;

    // The ArgumentException is thrown when an argument
    // is null when it shouldn't be.
    //
    [Serializable]
    public class ArgumentNullException : ArgumentException
    {
        // Creates a new ArgumentNullException with its message
        // string set to a default message explaining an argument was null.
#if EXCEPTION_STRINGS
        public ArgumentNullException() : base( Environment.GetResourceString( "ArgumentNull_Generic" ) )
#else
        public ArgumentNullException()
#endif
        {
        }

#if EXCEPTION_STRINGS
        public ArgumentNullException( String paramName ) : base( Environment.GetResourceString( "ArgumentNull_Generic" ), paramName )
#else
        public ArgumentNullException( String paramName ) : base( null, paramName )
#endif
        {
        }

        public ArgumentNullException( String message, Exception innerException ) : base( message, innerException )
        {
        }

        public ArgumentNullException( String paramName, String message ) : base( message, paramName )
        {
        }

////    [SecurityPermissionAttribute( SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter )]
////    protected ArgumentNullException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
