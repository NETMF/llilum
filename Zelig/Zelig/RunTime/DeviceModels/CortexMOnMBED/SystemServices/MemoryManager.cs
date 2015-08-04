//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.CortexMOnMBED
{
    using System;
    using System.Runtime.InteropServices;

    using RT   = Microsoft.Zelig.Runtime;
    using TS   = Microsoft.Zelig.Runtime.TypeSystem;
    using MBED = Microsoft.Zelig.Support.mbed;

    public sealed class MemoryManager : RT.LinearMemoryManager
    {
        private UIntPtr ManagedStack;
        private UIntPtr ManagedStackEnd;

        // Value hardcoded to fit in LPC1768 RAM (Device RAM=32KB)
        // TODO: LT72: make this part of the configuration
        private const uint ManagedStackSize = 0x2500; 
        
        //--//

        public override unsafe void InitializeMemoryManager( )
        {
            base.InitializeMemoryManager( );

            uint stackSize = ManagedStackSize;

            byte *mstack = MBED.Memory.RequestMemoryPool( &stackSize );

            if( mstack == null || stackSize < ManagedStackSize / 2 )
            {
                RT.BugCheck.Raise( RT.BugCheck.StopCode.FailedBootstrap );
            }

            ManagedStack    = (UIntPtr)           mstack;
            ManagedStackEnd = (UIntPtr) ( ( uint )mstack + ManagedStackSize);
            
            AddLinearSection( ManagedStack, ManagedStackEnd , RT.MemoryAttributes.RAM );
        }

        public override bool RefersToMemory( UIntPtr address )
        {
            return ( uint )ManagedStack >= ( uint )address && ( uint )address < ( uint )ManagedStackEnd;
        }     
    }
}
