//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Object), NoConstructors=true)]
    public class ObjectImpl
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //--//

        //
        // Helper Methods
        //

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

            object obj  = TypeSystemManager.Instance.AllocateObject( vTable );
            int    size =  (int)       vTable.BaseSize;
            uint*  src  =              this .Unpack();
            uint*  dst  = ((ObjectImpl)obj ).Unpack();

            while(size > 0)
            {
                *dst++ = *src++;
                size -= sizeof(uint);
            }

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

        //--//

        //
        // This is used to get the pointer to the data, which is not possible in C#.
        //
        [TS.GenerateUnsafeCast]
        public extern unsafe uint* Unpack();

        [TS.GenerateUnsafeCast]
        public extern static ObjectImpl CastAsObject( UIntPtr ptr );


        [TS.GenerateUnsafeCast]
        public extern UIntPtr CastAsUIntPtr();


        [TS.WellKnownMethod( "Object_NullCheck" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        public extern static void NullCheck( object a );


        //--//

        //
        // Access Methods
        //

    }
}

