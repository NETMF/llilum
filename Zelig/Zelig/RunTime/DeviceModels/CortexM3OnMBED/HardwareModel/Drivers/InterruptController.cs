//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM3OnMBED.Drivers
{
    using System;
    using Chipset = CortexM3OnCMSISCore;
    using TS = Zelig.Runtime.TypeSystem;
    using RT = Zelig.Runtime;

    public abstract class InterruptController : Chipset.Drivers.InterruptController
    {
        //
        // TODO: If GC compaction is ever supported, we will need to
        // come up with way to handle this differently.  Otherwise,
        // the object might move while its address is referenced in 
        // native code.
        //
        [TS.GenerateUnsafeCast()]
        internal extern static Handler CastAsInterruptHandler(UIntPtr ptr);

        [RT.Inline]
        internal static UIntPtr CastInterruptHandlerAsPtr(Handler hnd)
        {
            return ((RT.ObjectImpl)(object)(hnd)).ToPointer();
        }
    }
}
