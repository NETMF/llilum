//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;


    public enum CompilationConstraints
    {
        Allocations_ON       ,
        Allocations_OFF      ,

        StackAccess_ON       ,
        StackAccess_OFF      ,

        BoundsChecks_ON      ,
        BoundsChecks_OFF     ,
        BoundsChecks_OFF_DEEP,

        NullChecks_ON        ,
        NullChecks_OFF       ,
        NullChecks_OFF_DEEP  ,
    }
}
