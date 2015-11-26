//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.LlilumOSAbstraction.API
{
    using System.Runtime.InteropServices;

    public static class RuntimeMemory
    {
        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MEMORY_GetMaxHeapSize( out uint pMaxHeapSize );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MEMORY_GetDefaultManagedStackSize( );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MEMORY_Allocate( uint size, byte fill, out void* pAllocation );

        [DllImport( "C" )]
        public static unsafe extern uint LLOS_MEMORY_Reallocate( void* allocation, out void* newAllocation );

        [DllImport( "C" )]
        public static unsafe extern void LLOS_MEMORY_Free( void* address );
    }
}
