// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ThreadStateException
**
**
** Purpose: An exception class to indicate that the Thread class is in an
**          invalid state for the method.
**
**
=============================================================================*/

namespace System.Threading
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ThreadStateException : SystemException
    {
        public ThreadStateException() : base( Environment.GetResourceString( "Arg_ThreadStateException" ) )
        {
        }

        public ThreadStateException( String message ) : base( message )
        {
        }

        public ThreadStateException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected ThreadStateException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
