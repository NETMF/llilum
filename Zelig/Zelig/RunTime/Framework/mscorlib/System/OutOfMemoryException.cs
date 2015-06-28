// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: OutOfMemoryException
**
**
** Purpose: The exception class for OOM.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class OutOfMemoryException : SystemException
    {
        public OutOfMemoryException() : base( GetMessageFromNativeResources( ExceptionMessageKind.OutOfMemory ) )
        {
        }

        public OutOfMemoryException( String message ) : base( message )
        {
        }

        public OutOfMemoryException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected OutOfMemoryException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
