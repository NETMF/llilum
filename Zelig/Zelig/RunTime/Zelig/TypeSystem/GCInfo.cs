//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public struct GCInfo
    {
        public enum Kind : short
        {
            Invalid      ,
            NotAPointer  ,
            AggregateType,
            Heap         ,
            Internal     ,
            Potential    , // For IntPtr and UIntPtr, treat them as potential pointers that have been pinned.
        }

        public struct Pointer
        {
            public Kind  Kind;
            public short OffsetInWords;
        }

        //--//

        //
        // State
        //

        public Pointer[] Pointers;

        //--//

        //
        // Access Methods
        //

    }
}
