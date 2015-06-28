//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

#define LLVM

//--//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [TS.WellKnownType( "Microsoft_Zelig_Runtime_ObjectHeader" )]
    [TS.NoVTable]
    public class ObjectHeader
    {
        const uint GarbageCollectorMask  = 0x000000FF;
        const int  GarbageCollectorShift = 0;

        const uint ExtensionKindMask     = 0x00000300;
        const int  ExtensionKindShift    = 8;

        const uint ExtensionPayloadMask  = 0xFFFFFC00;
        const int  ExtensionPayloadShift = 10;

        [Flags]
        public enum GarbageCollectorFlags : uint
        {
            Unmarked               = 0x00000000,
            Marked                 = 0x00000001,
            MutableMask            = 0x00000001,

            FreeBlock              = 0 << 1, // Free block.
            GapPlug                = 1 << 1, // Used to mark free space that cannot be reclaimed due to fragmentation.
            ReadOnlyObject         = 2 << 1, // This object is stored in read-only memory.
            UnreclaimableObject    = 3 << 1, // This object is stored outside the garbage-collected heap.
            NormalObject           = 4 << 1, // Normal object.
            SpecialHandlerObject   = 5 << 1, // This object has a GC extension handler.
        }
                              
        public enum ExtensionKinds : uint
        {
            Empty     = 0x00000000,
            HashCode  = 0x00000001,
            SyncBlock = 0x00000002,
            Reserved  = 0x00000003,
        }

        //
        // State
        //

        [TS.WellKnownField( "ObjectHeader_MultiUseWord" )] public volatile int       MultiUseWord;
        [TS.WellKnownField( "ObjectHeader_VirtualTable" )] public          TS.VTable VirtualTable;

        //
        // Don't allow object creation, this is not a real class, it's more of a struct.
        //

        private ObjectHeader()
        {
        }

        //
        // Helper Methods
        //      

        [Inline]
        public object Pack()
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf( typeof(ObjectHeader ) );

            return ObjectImpl.CastAsObject( AddressMath.Increment( this.ToPointer(), (uint)size ) );
        }

        [Inline]
        public static ObjectHeader Unpack( object obj )
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf( typeof(ObjectHeader ) );

            return CastAsObjectHeader( AddressMath.Decrement( ((ObjectImpl)obj).CastAsUIntPtr(), (uint)size ) );
        }

        [TS.GenerateUnsafeCast]
        public extern UIntPtr ToPointer();

        [TS.GenerateUnsafeCast]
        public extern static ObjectHeader CastAsObjectHeader( UIntPtr ptr );

        [Inline]
        public unsafe UIntPtr GetNextObjectPointer()
        {
            ObjectImpl obj    = (ObjectImpl)this.Pack();
            TS.VTable  vTable =             this.VirtualTable;
            ArrayImpl  array  = ArrayImpl.CastAsArray( obj );
            uint       size   = vTable.BaseSize + vTable.ElementSize * (uint)array.Length;

            size = AddressMath.AlignToWordBoundary( size );

            return new UIntPtr( (uint)obj.Unpack() + size );
        }

        [TS.WellKnownMethod("DebugGC_ObjectHeader_InsertPlug")]
        public unsafe void InsertPlug( uint size )
        {
            UIntPtr address = this.ToPointer();
            uint*   dst     = (uint*)address.ToPointer();

            while(size >= sizeof(uint))
            {
                *dst++  = (uint)GarbageCollectorFlags.GapPlug;
                size   -= sizeof(uint);
            }
        }

        //--//

        public void UpdateExtension( ExtensionKinds kind    ,
                                     int            payload )
        {
            BugCheck.Assert( this.IsImmutable == false, BugCheck.StopCode.SyncBlockCorruption );

            while(true)
            {
                var  oldValue = this.MultiUseWord;
                uint newValue = ((uint)kind << ExtensionKindShift) | ((uint)payload << ExtensionPayloadShift) | ((uint)oldValue & GarbageCollectorMask);

                // CS0420: a reference to a volatile field will not be treated as volatile
#pragma warning disable 420
                var oldValue2 = System.Threading.Interlocked.CompareExchange( ref this.MultiUseWord, (int)newValue, oldValue );
#pragma warning restore 420

                if(oldValue2 == oldValue)
                {
                    break;
                }
            }
        }

        //
        // Access Methods
        //

        public unsafe GarbageCollectorFlags GarbageCollectorState
        {
            [Inline]
            get
            {
#pragma warning disable 420 // a reference to a volatile field will not be treated as volatile
                fixed(int* ptr = &this.MultiUseWord)
#pragma warning restore 420
                {
                    byte* flags = (byte*)ptr;

                    return (GarbageCollectorFlags)(uint)*flags;
                }
            }

            [Inline]
            set
            {
#pragma warning disable 420 // a reference to a volatile field will not be treated as volatile
                fixed(int* ptr = &this.MultiUseWord)
#pragma warning restore 420
                {
                    byte* flags = (byte*)ptr;

                    *flags = (byte)(uint)value;
                }
            }
        }
        
        public GarbageCollectorFlags GarbageCollectorStateWithoutMutableBits
        {
            [Inline]
            get
            {
                return (this.GarbageCollectorState & ~GarbageCollectorFlags.MutableMask);
            }
        }

        public ExtensionKinds ExtensionKind
        {
            [Inline]
            get
            {
                return (ExtensionKinds)((this.MultiUseWord & ExtensionKindMask) >> ExtensionKindShift);
            }
        }

        public bool IsImmutable
        {
            [Inline]
            get
            {
                return this.GarbageCollectorStateWithoutMutableBits == GarbageCollectorFlags.ReadOnlyObject;
            }
        }

        public int Payload
        {
            [Inline]
            get
            {
                return (int)(((uint)this.MultiUseWord & ExtensionPayloadMask) >> ExtensionPayloadShift);
            }
        }

        //--//


        //
        // Extension Methods
        //

        [ExtendClass(typeof(TS.VTable), NoConstructors=true, ProcessAfter=typeof(TypeImpl))]
        class VTableImpl
        {

            [Inline]
            public static TS.VTable Get( object a )
            {
                ObjectImpl.NullCheck( a );

                ObjectHeader oh = ObjectHeader.Unpack( a );

                return oh.VirtualTable;
            }

            [Inline]
            public static TS.VTable GetFromTypeHandle( RuntimeTypeHandleImpl hnd )
            {
                return hnd.m_value;
            }
        }
    }
}
