//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED
{
using System;

    using RT      = Microsoft.Zelig.Runtime;
    using Chipset = Microsoft.CortexM3OnCMSISCore;
    using MBED    = Microsoft.Zelig.Support.mbed;
    using LLOS    = Zelig.LlilumOSAbstraction;

    public class MemoryManager : Chipset.MemoryManager
    {
        private UIntPtr ManagedHeap;
        private UIntPtr ManagedHeapEnd;
        
        //--//

        public override unsafe void InitializeMemoryManager( )
        {
            base.InitializeMemoryManager( );

            uint heapSize;
            void *pLlilumHeap;

            LLOS.LlilumErrors.ThrowOnError( LLOS.API.RuntimeMemory.LLOS_MEMORY_GetMaxHeapSize( out heapSize ), false );
            LLOS.LlilumErrors.ThrowOnError( LLOS.API.RuntimeMemory.LLOS_MEMORY_Allocate( heapSize, 0, out pLlilumHeap ), false );

            ManagedHeap    = (UIntPtr)(pLlilumHeap);
            ManagedHeapEnd = (UIntPtr)( (uint)pLlilumHeap + heapSize );

            var attrs = RT.MemoryAttributes.InternalMemory          |
                        RT.MemoryAttributes.RandomAccessMemory      |
                        RT.MemoryAttributes.ConfiguredAtEntryPoint  ;
            
            AddLinearSection( ManagedHeap, ManagedHeapEnd , attrs );
        }

        public override bool RefersToMemory( UIntPtr address )
        {
            return ( uint )ManagedHeap >= ( uint )address && ( uint )address < ( uint )ManagedHeapEnd;
        }     
    }
}
