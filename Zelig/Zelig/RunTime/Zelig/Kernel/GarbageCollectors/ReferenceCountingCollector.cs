//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public class ReferenceCountingCollector
    {
        [TS.WellKnownMethod( "ReferenceCountingCollector_LoadAndAddReference" )]
        [TS.DisableAutomaticReferenceCounting]
        [NoInline]
        internal static object LoadAndAddReference( ref IntPtr target )
        {
            IntPtr valuePtr = InterlockedImpl.InternalCompareExchange( ref target, (IntPtr)0, (IntPtr)0 );
            Object value = IntPtrToObject( valuePtr );
            ObjectHeader.AddReference( value );
            return value;
        }

        [TS.WellKnownMethod( "ReferenceCountingCollector_Swap" )]
        [TS.DisableAutomaticReferenceCounting]
        [NoInline]
        internal static void Swap( ref IntPtr target, object value )
        {
            ObjectHeader.AddReference( value );

            IntPtr oldValuePtr = InterlockedImpl.InternalExchange( ref target, ObjectToIntPtr( value ) );

            ObjectHeader.ReleaseReference( IntPtrToObject( oldValuePtr ) );
        }

        [TS.GenerateUnsafeCast]
        internal extern static IntPtr ObjectToIntPtr( object o );

        [TS.GenerateUnsafeCast]
        internal extern static Object IntPtrToObject( IntPtr ptr ) ;
    }
}
