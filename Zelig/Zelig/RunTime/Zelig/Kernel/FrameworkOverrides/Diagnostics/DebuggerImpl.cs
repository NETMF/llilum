//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Diagnostics.Debugger), NoConstructors=true)]
    public class DebuggerImpl
    {
        [Inline]
        public static void Break()
        {
            Processor.Instance.Breakpoint();
        }
    }
}

