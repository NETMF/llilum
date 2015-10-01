// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: OverflowException
**
**
** Purpose: Exception class for Arthimatic Overflows.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class OverflowException : ArithmeticException
    {
#if EXCEPTION_STRINGS
        public OverflowException() : base( Environment.GetResourceString( "Arg_OverflowException" ) )
#else
        public OverflowException()
#endif
        {
        }

        public OverflowException( String message ) : base( message )
        {
        }

        public OverflowException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected OverflowException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
