// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: KeyNotFoundException
**
**
** Purpose: Exception class for Hashtable and Dictionary.
**
**
=============================================================================*/

namespace System.Collections.Generic
{
    using System;
////using System.Runtime.Remoting;
    using System.Runtime.Serialization;

    [Serializable]
    public class KeyNotFoundException : SystemException /*, ISerializable*/
    {
#if EXCEPTION_STRINGS
        public KeyNotFoundException() : base( Environment.GetResourceString( "Arg_KeyNotFound" ) )
#else
        public KeyNotFoundException()
#endif
        {
        }

        public KeyNotFoundException( String message ) : base( message )
        {
        }

        public KeyNotFoundException( String message, Exception innerException ) : base( message, innerException )
        {
        }


////    protected KeyNotFoundException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
