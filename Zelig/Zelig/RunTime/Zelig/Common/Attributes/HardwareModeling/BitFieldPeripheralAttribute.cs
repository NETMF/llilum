//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;


    [TypeSystem.WellKnownType( "Microsoft_Zelig_Runtime_BitFieldPeripheralAttribute" )]
    [AttributeUsage(AttributeTargets.Struct,AllowMultiple=false)]
    public sealed class BitFieldPeripheralAttribute : Attribute
    {
        //
        // State
        //

        public Type PhysicalType;
    }
}
