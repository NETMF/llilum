//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexM3OnMBED
{
using System;

    using RT        = Microsoft.Zelig.Runtime;
    using Chipset   = Microsoft.CortexM3OnCMSISCore;
    using MBED      = Microsoft.Zelig.Support.mbed;

    public class MemoryManager : Chipset.MemoryManager
    {
        private UIntPtr ManagedHeap;
        private UIntPtr ManagedHeapEnd;
        
        //--//

        public override unsafe void InitializeMemoryManager( )
        {
            base.InitializeMemoryManager( );

            uint stackSize = Device.Instance.ManagedHeapSize;

            byte *zeligHeap = MBED.Memory.RequestMemoryPool( &stackSize );

            if( zeligHeap == null || stackSize < Device.Instance.ManagedHeapSize / 2 )
            {
                MBED.Memory.FreeMemoryPool( zeligHeap );

                RT.BugCheck.Raise( RT.BugCheck.StopCode.FailedBootstrap );
            }

            ManagedHeap    = (UIntPtr)         zeligHeap              ;
            ManagedHeapEnd = (UIntPtr) ( (uint)zeligHeap + stackSize );

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
