//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// The default GC scheme will be Conservative Mark and Sweep.
// Only one of these should be enabled at a time

#define MarkAndSweepGC

namespace Microsoft.Llilum.LPC1768
{
    using Chipset = Microsoft.CortexM3OnMBED;

#if ReferenceCountingGC

    public sealed class ReferenceCountingCollector : Microsoft.Zelig.Runtime.ReferenceCountingCollector
    {
    }

#endif

#if StrincReferenceCountingGC

    public sealed class StrictReferenceCountingCollector : Microsoft.Zelig.Runtime.StrictReferenceCountingCollector
    {
    }

#endif

#if MarkAndSweepGC

    public sealed class ConservativeMarkAndSweepCollector : Microsoft.Zelig.Runtime.ConservativeMarkAndSweepCollector
    {
    }

#endif
}

