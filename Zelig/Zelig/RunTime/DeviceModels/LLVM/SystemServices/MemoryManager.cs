//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.LLVMHosted
{
    using System;
    using System.Runtime.InteropServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public sealed class MemoryManager : RT.LinearMemoryManager
    {

        [DllImport( "C" )]
        static unsafe extern byte *malloc( uint size );

        private UIntPtr ManagedStack;
        private UIntPtr ManagedStackEnd;

        private const uint ManagedStackSize = 0x2500; //Value hardcoded to fit in LPC1768 RAM (Device RAM=32KB)

        //Instead of letting Zelig figure out the available RAM / the RAM
        //ocuppied by the runtime, we allocate directly the runtime structures
        //in RAM using LLVM globals and allocate just one big zeroed block of memory 
        //via Calloc 


        public override unsafe void InitializeMemoryManager( )
        {
            base.CallBaseInitializeMemoryManager( );
            byte *mstack=malloc( ManagedStackSize );

            ManagedStack = (UIntPtr) mstack;
            ManagedStackEnd = (UIntPtr) ( ( uint )mstack + ManagedStackSize);
            
            AddLinearSection( ManagedStack, ManagedStackEnd , RT.MemoryAttributes.RAM );
        }

        public override bool RefersToMemory( UIntPtr address )
        {
            return ( uint )ManagedStack >= ( uint )address && ( uint )address < ( uint )ManagedStackEnd;
        }
     
    }
}
