// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
/*============================================================
**
** File:    AssemblyHashAlgorithm
**
**
** Purpose:
**
**
===========================================================*/
namespace System.Configuration.Assemblies
{
    using System;

    [Serializable]
    public enum AssemblyHashAlgorithm
    {
        None = 0x0000,
        MD5  = 0x8003,
        SHA1 = 0x8004,
    }
}
