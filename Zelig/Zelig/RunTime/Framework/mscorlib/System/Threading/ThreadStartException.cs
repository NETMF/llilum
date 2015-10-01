// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==

namespace System.Threading
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;

    [Serializable]
    public sealed class ThreadStartException : SystemException
    {
        private ThreadStartException() : base( Environment.GetResourceString( "Arg_ThreadStartException" ) )
        {
        }

        private ThreadStartException( Exception reason ) : base( Environment.GetResourceString( "Arg_ThreadStartException" ), reason )
        {
        }

////    internal ThreadStartException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}


