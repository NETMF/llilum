//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class MemoryCategory : BusAttachedCategory
    {
        //
        // State
        //

        public uint                     BaseAddress;
        public ulong                    SizeInBytes;
        public uint                     WordSize;
        public uint                     WaitStates;

        public Runtime.MemoryAttributes Characteristics;

        //
        // Helper Methods
        //

        public bool InRange( uint address )
        {
            return this.BaseAddress <= address && address < this.EndAddress;
        }

        //
        // Access Methods
        //

        public uint EndAddress
        {
            get
            {
                return (uint)(this.BaseAddress + this.SizeInBytes);
            }
        }

        public bool IsRAM
        {
            get
            {
                return (this.Characteristics & Runtime.MemoryAttributes.RAM) == Runtime.MemoryAttributes.RAM;
            }
        }
    }
}
