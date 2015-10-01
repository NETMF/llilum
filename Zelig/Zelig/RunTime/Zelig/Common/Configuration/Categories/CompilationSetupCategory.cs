//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class CompilationSetupCategory : AbstractCategory
    {
        //
        // State
        //

        public Type Platform;
        public Type CallingConvention;
        public Type Product;
        public Type MemoryMap;
    }
}
