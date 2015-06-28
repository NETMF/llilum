// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: StackOverflowException
**
**
** Purpose: The exception class for stack overflow.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class StackOverflowException : SystemException
    {
        public StackOverflowException() : base( Environment.GetResourceString( "Arg_StackOverflowException" ) )
        {
        }

        public StackOverflowException( String message ) : base( message )
        {
        }

        public StackOverflowException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    internal StackOverflowException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
