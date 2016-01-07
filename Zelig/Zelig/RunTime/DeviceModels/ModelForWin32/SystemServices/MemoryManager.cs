//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.DeviceModels.Runtime.Win32
{
    using System;
    using RT = Microsoft.Zelig.Runtime;
    using LLOS = Zelig.LlilumOSAbstraction;

    public class MemoryManager : RT.LinearMemoryManager
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
