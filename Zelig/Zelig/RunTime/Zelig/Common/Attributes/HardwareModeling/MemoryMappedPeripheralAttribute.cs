//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_MemoryMappedPeripheralAttribute" )]
    [AttributeUsage(AttributeTargets.Class  |
                    AttributeTargets.Struct ,AllowMultiple=false)]
    public sealed class MemoryMappedPeripheralAttribute : Attribute
    {
        //
        // State
        //

        public uint Base;
        public uint Length;
    }
}
