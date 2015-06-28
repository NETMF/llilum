//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class ProductCategory : AbstractCategory
    {
        //
        // Helper Methods
        //

        public MemoryCategory FindMemory( uint address )
        {
            foreach(var mem in this.SearchValues< MemoryCategory >())
            {
                if(mem.InRange( address ))
                {
                    return mem;
                }
            }

            return null;
        }

        public RamMemoryCategory FindAnyBootstrapRAM()
        {
            const Runtime.MemoryAttributes mask = Runtime.MemoryAttributes.ConfiguredAtEntryPoint |
                                                  Runtime.MemoryAttributes.RandomAccessMemory;

            foreach(RamMemoryCategory mem in this.SearchValues< RamMemoryCategory >())
            {
                if((mem.Characteristics & mask) == mask)
                {
                    return mem;
                }

            }

            return null;
        }
    }
}
