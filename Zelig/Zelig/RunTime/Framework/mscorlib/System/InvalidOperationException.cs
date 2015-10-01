// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: InvalidOperationException
**
**
** Purpose: Exception class for denoting an object was in a state that
** made calling a method illegal.
**
**
=============================================================================*/
namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidOperationException : SystemException
    {
#if EXCEPTION_STRINGS
        public InvalidOperationException() : base( Environment.GetResourceString( "Arg_InvalidOperationException" ) )
#else
        public InvalidOperationException()
#endif
        {
        }

        public InvalidOperationException( String message ) : base( message )
        {
        }

        public InvalidOperationException( String message, Exception innerException ) : base( message, innerException )
        {
        }
////
////    protected InvalidOperationException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}

