//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.CodeGeneration.IR
{
    using System;

    public enum BasicBlockEdgeClass
    {
        Unknown    ,
        TreeEdge   ,
        ForwardEdge,
        BackEdge   ,
        CrossEdge  ,
    }
}