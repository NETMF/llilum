//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.InteropServices;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public class ReferenceCountingCollector
    {
#if REFCOUNT_STAT
        [TS.WellKnownMethod( "ReferenceCountingCollector_LoadAndAddReference" )]
        [TS.DisableAutomaticReferenceCounting]
        internal static Object LoadAndAddReference( ref Object target )
        {
            Object obj = LoadAndAddReferenceNative( ref target );

            ObjectHeader.RecordAddReference( obj );
            ObjectHeader.s_LoadAndAddRefCalled++;

            return obj;
        }

        [DllImport( "C" )]
        internal static extern Object LoadAndAddReferenceNative( ref Object target );
#else //REFCOUNT_STAT
        [TS.WellKnownMethod( "ReferenceCountingCollector_LoadAndAddReference" )]
        [DllImport( "C" )]
        internal static extern Object LoadAndAddReferenceNative( ref Object target );
#endif //REFCOUNT_STAT

        [TS.WellKnownMethod( "ReferenceCountingCollector_Swap" )]
        [TS.DisableAutomaticReferenceCounting]
        [NoInline]
        internal static void Swap( ref Object target, Object value )
        {
#if REFCOUNT_STAT
            ObjectHeader.RecordAddReference( value );
            ObjectHeader.s_SwapCalled++;
#endif //REFCOUNT_STAT

            Object oldValue = ReferenceCountingExchange( ref target, value );
            ObjectHeader.ReleaseReference( oldValue );
        }

        [TS.WellKnownMethod( "ReferenceCountingCollector_ReferenceCountingExchange" )]
        [DllImport( "C" )]
        private static extern Object ReferenceCountingExchange( ref Object location1,
                                                                    Object value );

        [TS.WellKnownMethod( "ReferenceCountingCollector_ReferenceCountingCompareExchange" )]
        [DllImport( "C" )]
        private static extern Object ReferenceCountingCompareExchange( ref Object location1,
                                                                           Object value,
                                                                           Object comparand );
    }
}
