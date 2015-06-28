// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ArithmeticException
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

    // The ArithmeticException is thrown when overflow or underflow
    // occurs.
    //
    [Serializable]
    public class ArithmeticException : SystemException
    {
        // Creates a new ArithmeticException with its message string set to
        // the empty string, its HRESULT set to COR_E_ARITHMETIC,
        // and its ExceptionInfo reference set to null.
#if EXCEPTION_STRINGS
        public ArithmeticException() : base( Environment.GetResourceString( "Arg_ArithmeticException" ) )
#else
        public ArithmeticException()
#endif
        {
        }

        // Creates a new ArithmeticException with its message string set to
        // message, its HRESULT set to COR_E_ARITHMETIC,
        // and its ExceptionInfo reference set to null.
        //
        public ArithmeticException( String message ) : base( message )
        {
        }

        public ArithmeticException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected ArithmeticException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
