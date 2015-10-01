// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////////
// Empty
//  This class represents an empty variant
////////////////////////////////////////////////////////////////////////////////

namespace System
{
    using System;
////using System.Runtime.Remoting;
    using System.Runtime.Serialization;

    [Serializable]
    internal sealed class Empty /*: ISerializable*/
    {
        public static readonly Empty Value = new Empty();

        private Empty()
        {
        }

////    public override String ToString()
////    {
////        return String.Empty;
////    }
////
////    public void GetObjectData( SerializationInfo info, StreamingContext context )
////    {
////        if(info == null)
////        {
////            throw new ArgumentNullException( "info" );
////        }
////        UnitySerializationHolder.GetUnitySerializationInfo( info, UnitySerializationHolder.EmptyUnity, null, null );
////    }
    }
}
