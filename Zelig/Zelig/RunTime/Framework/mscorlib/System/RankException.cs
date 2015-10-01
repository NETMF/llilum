// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*=============================================================================
**
** Class: RankException
**
**
** Purpose: For methods that are passed arrays with the wrong number of
**          dimensions.
**
**
=============================================================================*/

namespace System
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class RankException : SystemException
    {
        public RankException() : base( Environment.GetResourceString( "Arg_RankException" ) )
        {
        }

        public RankException( String message ) : base( message )
        {
        }

        public RankException( String message, Exception innerException ) : base( message, innerException )
        {
        }

////    protected RankException( SerializationInfo info, StreamingContext context ) : base( info, context )
////    {
////    }
    }
}
