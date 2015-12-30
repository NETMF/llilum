//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

// Enable this macro to shift object pointers from the beginning of the header to the beginning of the payload.
#define CANONICAL_OBJECT_POINTERS

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Object), NoConstructors=true)]
    public class ObjectImpl
    {
        [AliasForBaseMethod( "Finalize" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern virtual void FinalizeImpl();

        [NoInline]
        public new Type GetType()
        {
            return TS.VTable.Get( this ).Type;
        }

        protected unsafe new Object MemberwiseClone()
        {
            TS.VTable vTable = TS.VTable.Get( this );
            object obj = TypeSystemManager.Instance.AllocateObject( vTable );

            byte* src = (byte*)GetFieldPointer();
            byte* dst = (byte*)((ObjectImpl)obj).GetFieldPointer();
            int size  = (int)vTable.BaseSize;
            Buffer.InternalMemoryCopy( src, dst, size );

            return obj;
        }

        public override bool Equals( Object obj )
        {
            return Object.ReferenceEquals( this, obj );
        }

        public override int GetHashCode()
        {
            return SyncBlockTable.GetHashCode( this );
        }

        [Inline]
        public UIntPtr GetFieldPointer()
        {
#if CANONICAL_OBJECT_POINTERS
            return ToPointer();
#else // CANONICAL_OBJECT_POINTERS
            return AddressMath.Increment(ToPointer(), ObjectHeader.HeaderSize);
#endif // CANONICAL_OBJECT_POINTERS
        }

        [Inline]
        public static ObjectImpl FromFieldPointer(UIntPtr fieldPointer)
        {
#if CANONICAL_OBJECT_POINTERS
            return FromPointer(fieldPointer);
#else // CANONICAL_OBJECT_POINTERS
            return FromPointer(AddressMath.Decrement(fieldPointer, ObjectHeader.HeaderSize));
#endif // CANONICAL_OBJECT_POINTERS
        }

        [TS.GenerateUnsafeCast]
        public extern UIntPtr ToPointer();

        [TS.GenerateUnsafeCast]
        public extern static ObjectImpl FromPointer( UIntPtr ptr );

        [TS.WellKnownMethod( "Object_NullCheck" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static void NullCheck( object a );
    }
}

