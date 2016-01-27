//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Llilum.LPC1768
{
    using Chipset = Microsoft.CortexM3OnMBED;


    public sealed class ReferenceCountingCollector : Microsoft.Zelig.Runtime.ReferenceCountingCollector
    {
    }

    public sealed class StrictReferenceCountingCollector : Microsoft.Zelig.Runtime.StrictReferenceCountingCollector
    {
    }

    public sealed class ConservativeMarkAndSweepCollector : Microsoft.Zelig.Runtime.ConservativeMarkAndSweepCollector
    {
    }
}

