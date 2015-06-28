// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// Interface Map.  This struct returns the mapping of an interface into the actual methods on a class
//  that implement that interface.
//
// <EMAIL>Author: darylo</EMAIL>
// Date: March 2000
//
namespace System.Reflection
{
    using System;

    public struct InterfaceMapping
    {
        public Type         TargetType;         // The type implementing the interface
        public Type         InterfaceType;      // The type representing the interface
        public MethodInfo[] TargetMethods;      // The methods implementing the interface
        public MethodInfo[] InterfaceMethods;   // The methods defined on the interface
    }
}
