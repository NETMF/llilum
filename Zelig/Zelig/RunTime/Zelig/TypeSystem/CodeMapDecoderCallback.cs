//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public abstract class CodeMapDecoderCallback
    {
        public enum PointerKind
        {
            Heap     ,
            Internal ,
            Potential,
        }

        public abstract bool RegisterEnter( UIntPtr address, uint num   , PointerKind kind );
        public abstract bool RegisterLeave( UIntPtr address, uint num                      );
        public abstract bool StackEnter   ( UIntPtr address, uint offset, PointerKind kind );
        public abstract bool StackLeave   ( UIntPtr address, uint offset                   );
    }
}
