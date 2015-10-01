// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** Class: SerializableAttribute
**
**
** Purpose: Used to mark a class as being serializable
**
**
============================================================*/
namespace System
{

    using System;
    using System.Reflection;

    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false )]
    public sealed class SerializableAttribute : Attribute
    {
////    internal static Attribute GetCustomAttribute( Type type )
////    {
////        return (type.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable ? new SerializableAttribute() : null;
////    }
////    internal static bool IsDefined( Type type )
////    {
////        return type.IsSerializable;
////    }

        public SerializableAttribute()
        {
        }
    }
}
