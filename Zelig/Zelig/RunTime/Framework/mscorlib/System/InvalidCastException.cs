// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: InvalidCastException
**
**
** Purpose: Exception class for bad cast conditions!
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidCastException : SystemException
    {
#if EXCEPTION_STRINGS
        public InvalidCastException() : base( Environment.GetResourceString( "Arg_InvalidCastException" ) )
#else
        public InvalidCastException()
#endif
        {
        }

        public InvalidCastException( String message ) : base( message )
        {
        }

        public InvalidCastException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected InvalidCastException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
////
        public InvalidCastException( String message, int errorCode ) : base( message )
        {
        }
    }
}
