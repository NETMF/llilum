// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: IndexOutOfRangeException
**
**
** Purpose: Exception class for invalid array indices.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class IndexOutOfRangeException : SystemException
    {
#if EXCEPTION_STRINGS
        public IndexOutOfRangeException() : base( Environment.GetResourceString( "Arg_IndexOutOfRangeException" ) )
#else
        public IndexOutOfRangeException()
#endif
        {
        }

        public IndexOutOfRangeException( String message ) : base( message )
        {
        }

        public IndexOutOfRangeException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    internal IndexOutOfRangeException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
