//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig
{
    using System;

    public interface ITreeEdge<FC>
    {
        ITreeNode<FC> Predecessor { get; }

        ITreeNode<FC> Successor { get; }

        EdgeClass EdgeClass { get; set; }
    }
}