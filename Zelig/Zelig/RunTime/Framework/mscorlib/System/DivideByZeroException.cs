// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: DivideByZeroException
**
**
** Purpose: Exception class for bad arithmetic conditions!
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DivideByZeroException : ArithmeticException
    {
#if EXCEPTION_STRINGS
        public DivideByZeroException() : base( Environment.GetResourceString( "Arg_DivideByZero" ) )
#else
        public DivideByZeroException()
#endif
        {
        }

        public DivideByZeroException( String message ) : base( message )
        {
        }

        public DivideByZeroException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected DivideByZeroException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
