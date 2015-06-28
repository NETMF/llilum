// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////////
// MulticastNotSupportedException
// This is thrown when you add multiple callbacks to a non-multicast delegate.
////////////////////////////////////////////////////////////////////////////////

namespace System
{
    using System;
////using System.Runtime.Serialization;

    [Serializable]
    public sealed class MulticastNotSupportedException : SystemException
    {
        public MulticastNotSupportedException() : base( Environment.GetResourceString( "Arg_MulticastNotSupportedException" ) )
        {
        }

        public MulticastNotSupportedException( String message ) : base( message )
        {
        }

        public MulticastNotSupportedException( String message, Exception inner ) : base( message, inner )
        {
        }

////    internal MulticastNotSupportedException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
