// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: NotImplementedException
**
**
** Purpose: Exception thrown when a requested method or operation is not
**          implemented.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NotImplementedException : SystemException
    {
#if EXCEPTION_STRINGS
        public NotImplementedException() : base( Environment.GetResourceString( "Arg_NotImplementedException" ) )
#else
        public NotImplementedException()
#endif
        {
        }

        public NotImplementedException( String message ) : base( message )
        {
        }
        public NotImplementedException( String message, Exception inner ) : base( message, inner )
        {
        }
////
////    protected NotImplementedException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
