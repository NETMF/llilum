//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM0OnMBED.Drivers
{
    using System;

    using TS            = Zelig.Runtime.TypeSystem;
    using RT            = Zelig.Runtime;
    using ChipsetModel  = CortexM0OnCMSISCore;

    public abstract class InterruptController : ChipsetModel.Drivers.InterruptController
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
            return ((RT.ObjectImpl)(object)hnd).ToPointer();
        }
    }
}
