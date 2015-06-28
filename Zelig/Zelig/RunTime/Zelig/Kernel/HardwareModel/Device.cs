//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;


    [ImplicitInstance]
    [ForceDevirtualization]
    public abstract class Device
    {
        private static BugCheck.StopCode bugCheckCode;

        const int DefaultStackSize = 2048 / sizeof(uint);

        [MemoryUsage(MemoryUsage.Stack, ContentsUninitialized=true, AllocateFromHighAddress=true)]
        static readonly uint[] s_bootstrapStack = new uint[DefaultStackSize];

        //
        // Helper Methods
        //

        public abstract void PreInitializeProcessorAndMemory();

        public abstract void MoveCodeToProperLocation();

        public virtual void ProcessBugCheck( BugCheck.StopCode code )
        {
            Device.bugCheckCode = code;

            while(true);
        }

        //
        // Access Methods
        //

        public static extern Device Instance
        {
            [SingletonFactory(ReadOnly=true)]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }

        public virtual uint[] BootstrapStack
        {
            [Inline]
            get
            {
                return s_bootstrapStack;
            }
        }

        public unsafe UIntPtr BootstrapStackPointer
        {
            [Inline]
            get
            {
                var stack = this.BootstrapStack;

                fixed(uint* ptr = stack)
                {
                    return new UIntPtr( &ptr[stack.Length] );
                }
            }
        }
    }
}
