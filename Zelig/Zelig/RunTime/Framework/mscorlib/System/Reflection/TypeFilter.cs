// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// TypeFilter defines a delegate that is as a callback function for filtering
//  a list of Types.
//
// <EMAIL>Author: darylo</EMAIL>
// Date: March 98
//
namespace System.Reflection
{
    // Define the delegate
    [Serializable]
    public delegate bool TypeFilter( Type m, Object filterCriteria );
}
