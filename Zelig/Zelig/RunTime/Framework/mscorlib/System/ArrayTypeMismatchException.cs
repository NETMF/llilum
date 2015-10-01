// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: ArrayTypeMismatchException
**
**
** Purpose: The arrays are of different primitive types.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;
    // The ArrayMismatchException is thrown when an attempt to store
    // an object of the wrong type within an array occurs.
    //
    [Serializable]
    public class ArrayTypeMismatchException : SystemException
    {
        // Creates a new ArrayMismatchException with its message string set to
        // the empty string, its HRESULT set to COR_E_ARRAYTYPEMISMATCH,
        // and its ExceptionInfo reference set to null.
        public ArrayTypeMismatchException() : base( Environment.GetResourceString( "Arg_ArrayTypeMismatchException" ) )
        {
        }

        // Creates a new ArrayMismatchException with its message string set to
        // message, its HRESULT set to COR_E_ARRAYTYPEMISMATCH,
        // and its ExceptionInfo reference set to null.
        //
        public ArrayTypeMismatchException( String message ) : base( message )
        {
        }

        public ArrayTypeMismatchException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected ArrayTypeMismatchException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
