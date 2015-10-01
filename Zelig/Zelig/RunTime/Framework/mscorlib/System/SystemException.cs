// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SystemException : Exception
    {
#if EXCEPTION_STRINGS
        public SystemException() : base( Environment.GetResourceString( "Arg_SystemException" ) )
#else
        public SystemException()
#endif
        {
        }

        public SystemException( String message ) : base( message )
        {
        }

        public SystemException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected SystemException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
