// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: NotSupportedException
**
**
** Purpose: For methods that should be implemented on subclasses.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NotSupportedException : SystemException
    {
        public NotSupportedException() : base( Environment.GetResourceString( "Arg_NotSupportedException" ) )
        {
        }

        public NotSupportedException( String message ) : base( message )
        {
        }

        public NotSupportedException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected NotSupportedException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
