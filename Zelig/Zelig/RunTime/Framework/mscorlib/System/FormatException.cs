// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  FormatException
**
**
** Purpose: Exception to designate an illegal argument to FormatMessage.
**
** 
===========================================================*/
namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class FormatException : SystemException
    {
#if EXCEPTION_STRINGS
        public FormatException() : base( Environment.GetResourceString( "Arg_FormatException" ) )
#else
        public FormatException()
#endif
        {
        }

        public FormatException( String message ) : base( message )
        {
        }

        public FormatException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected FormatException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
