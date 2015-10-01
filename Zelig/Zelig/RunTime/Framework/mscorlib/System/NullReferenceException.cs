// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: NullReferenceException
**
**
** Purpose: Exception class for dereferencing a null reference.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NullReferenceException : SystemException
    {
#if EXCEPTION_STRINGS
        public NullReferenceException() : base( Environment.GetResourceString( "Arg_NullReferenceException" ) )
#else
        public NullReferenceException()
#endif
        {
        }

        public NullReferenceException( String message ) : base( message )
        {
        }

        public NullReferenceException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected NullReferenceException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
