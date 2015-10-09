//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    public unsafe struct MemoryFreeBlock
    {
        //
        // State
        //

        public MemoryFreeBlock* Next;
        public MemoryFreeBlock* Previous;

        //
        // Helper Methods.
        //

        [Inline]
        public byte[] Pack()
        {
            fixed(MemoryFreeBlock* ptr = &this)
            {
                return CastAsArray( AddressMath.Decrement( new UIntPtr( ptr ), PointerOffset() ) );
            }
        }

        public static MemoryFreeBlock* Unpack( byte[] ptr )
        {
            ArrayImpl array = ArrayImpl.CastAsArray( ptr );

            return (MemoryFreeBlock*)array.GetDataPointer();
        }

        [TS.GenerateUnsafeCast]
        private extern static byte[] CastAsArray( UIntPtr ptr );

        //--//

        public uint Size()
        {
            byte[] array = Pack();
            uint   size  = (uint)array.Length + FixedSize();

            return size;
        }

        public ObjectHeader ToObjectHeader()
        {
            return ObjectHeader.Unpack(Pack());

        }
        public UIntPtr ToObjectHeaderPointer()
        {
            return ToObjectHeader().ToPointer();
        }

        [TS.WellKnownMethod("DebugGC_MemoryFreeBlock_ZeroFreeMemory")]
        public void ZeroFreeMemory()
        {
            byte[] array = Pack();
            byte*  ptr   = (byte*)ArrayImpl.CastAsArray( array ).GetDataPointer();

            UIntPtr start = new UIntPtr( ptr + System.Runtime.InteropServices.Marshal.SizeOf( typeof(MemoryFreeBlock) ) );
            UIntPtr end   = new UIntPtr( ptr + array.Length                                                             );

            Memory.Zero( start, end );
        }

        [TS.WellKnownMethod("DebugGC_MemoryFreeBlock_DirtyFreeMemory")]
        public void DirtyFreeMemory()
        {
            byte[] array = Pack();
            byte*  ptr   = (byte*)ArrayImpl.CastAsArray( array ).GetDataPointer();

            UIntPtr start = new UIntPtr( ptr + System.Runtime.InteropServices.Marshal.SizeOf( typeof(MemoryFreeBlock) ) );
            UIntPtr end   = new UIntPtr( ptr + array.Length                                                             );

            Memory.Dirty( start, end );
        }

        [TS.WellKnownMethod("DebugGC_MemoryFreeBlock_Allocate")]
        public UIntPtr Allocate( ref MemorySegment memorySegment ,
                                     uint          size          )
        {
            ArrayImpl array         = ArrayImpl.CastAsArray( Pack() );
            uint      fixedSize     = FixedSize();
            uint      numElements   = (uint)array.Length;
            uint      availableSize = numElements + fixedSize;

            if(size <= availableSize)
            {
                uint left = availableSize - size;

                numElements -= size;

                if(left <= MinimumSpaceRequired())
                {
                    fixed(MemoryFreeBlock* ptr = &this)
                    {
                        memorySegment.RemoveFreeBlock( ptr );
                    }

                    ObjectHeader oh = ObjectHeader.Unpack( array );

                    if(MemoryManager.Configuration.TrashFreeMemory)
                    {
                        DirtyHeader( oh.ToPointer() );
                    }
                    else
                    {
                        ZeroHeader( oh.ToPointer() );
                    }

                    if(left > 0)
                    {
                        oh.InsertPlug( left );
                    }
                }
                else
                {
                    //
                    // Resize the array, as a way of marking memory allocation.
                    //
                    array.SetLength( numElements );
                }

                return new UIntPtr( (byte*)array.GetDataPointer() + numElements );
            }

            return UIntPtr.Zero;
        }

        [Inline]
        private static uint PointerOffset()
        {
            return (uint)System.Runtime.InteropServices.Marshal.SizeOf( typeof(ArrayImpl) );
        }

        [Inline]
        public static uint FixedSize()
        {
            int size;

            size  = System.Runtime.InteropServices.Marshal.SizeOf( typeof(ObjectHeader) );
            size += System.Runtime.InteropServices.Marshal.SizeOf( typeof(ArrayImpl   ) );

            return (uint)size;
        }

        [Inline]
        public static uint MinimumSpaceRequired()
        {
            int size;

            size  = System.Runtime.InteropServices.Marshal.SizeOf( typeof(ObjectHeader   ) );
            size += System.Runtime.InteropServices.Marshal.SizeOf( typeof(ArrayImpl      ) );
            size += System.Runtime.InteropServices.Marshal.SizeOf( typeof(MemoryFreeBlock) );

            return (uint)size;
        }

        //--//

        [TS.WellKnownMethod("DebugGC_MemoryFreeBlock_InitializeFromRawMemory")]
        public static MemoryFreeBlock* InitializeFromRawMemory( UIntPtr baseAddress ,
                                                                uint    sizeInBytes )
        {
            TS.VTable vTable        = TS.VTable.GetFromType( typeof(byte[]) );
            uint      numOfElements = sizeInBytes - FixedSize();

            byte[] externalRepresentation = (byte[])TypeSystemManager.Instance.InitializeArray( baseAddress, vTable, numOfElements );

            ObjectHeader oh = ObjectHeader.Unpack( externalRepresentation );

            oh.MultiUseWord = (int)(ObjectHeader.GarbageCollectorFlags.FreeBlock | ObjectHeader.GarbageCollectorFlags.Unmarked);

            return Unpack( externalRepresentation );
        }

        public static void ZeroHeader( UIntPtr address )
        {
            Memory.Zero( address, AddressMath.Increment( address, MinimumSpaceRequired() ) );
        }

        public static void DirtyHeader( UIntPtr address )
        {
            Memory.Dirty( address, AddressMath.Increment( address, MinimumSpaceRequired() ) );
        }

        //
        // Access Methods
        //

        public uint AvailableMemory
        {
            get
            {
                byte[] array = Pack();

                return FixedSize() + (uint)array.Length;
            }
        }
    }
}
