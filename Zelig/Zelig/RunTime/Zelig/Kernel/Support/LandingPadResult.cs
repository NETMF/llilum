//
// Copyright (c) Microsoft Corporation. All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [TS.WellKnownType("Microsoft_Zelig_Runtime_LandingPadResult")]
    internal struct LandingPadResult
    {
        // WARNING: Modify these fields only with extreme caution. LLVM expects landing pad result
        // types to be a struct with two members. No fields may be added or removed, even if unused.
        // The types and meaning of these fields are implementation dependent, and strongly tied to
        // the personality function that creates the result. See LLOS_Personality for more details.
        public IntPtr Exception;
        public int Selector;
    }
}
