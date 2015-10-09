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
        protected static BugCheck.StopCode m_bugCheckCode;

        const int DefaultStackSize = 512 / sizeof(uint);

        [MemoryUsage(MemoryUsage.Stack, ContentsUninitialized=true, AllocateFromHighAddress=true)]
        static readonly uint[] s_bootstrapStack = new uint[DefaultStackSize];
        
        //
        // Helper Methods
        //

        public abstract void PreInitializeProcessorAndMemory();

        public abstract void MoveCodeToProperLocation();

        public virtual void ProcessBugCheck( BugCheck.StopCode code )
        {
            Device.m_bugCheckCode = code;

            while(true);
        }

        public virtual void ProcessLog(string format) { }
        public virtual void ProcessLog(string format, int p1) { }
        public virtual void ProcessLog(string format, int p1, int p2) { }
        public virtual void ProcessLog(string format, int p1, int p2, int p3) { }
        public virtual void ProcessLog(string format, int p1, int p2, int p3, int p4) { }
        public virtual void ProcessLog(string format, int p1, int p2, int p3, int p4, int p5) { }

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
            get
            {
                return s_bootstrapStack;
            }
        }

        public unsafe UIntPtr BootstrapStackPointer
        {
            [NoInline]
            get
            {
                var stack = this.BootstrapStack;

                fixed(uint* ptr = stack)
                {
                    return new UIntPtr( &ptr[stack.Length] );
                }
            }
        }

        public virtual uint ManagedHeapSize
        {
            get
            {
                return 0x4000u; 
            }
        }
    }
}
