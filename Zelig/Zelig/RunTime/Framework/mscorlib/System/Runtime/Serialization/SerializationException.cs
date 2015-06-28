// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: SerializationException
**
**
** Purpose: Thrown when something goes wrong during serialization or
**          deserialization.
**
**
=============================================================================*/

namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SerializationException : SystemException
    {
        // Creates a new SerializationException with its message
        // string set to a default message.
        public SerializationException() : base( Environment.GetResourceString( "Arg_SerializationException" ) )
        {
        }

        public SerializationException( String message ) : base( message )
        {
        }

        public SerializationException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected SerializationException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
