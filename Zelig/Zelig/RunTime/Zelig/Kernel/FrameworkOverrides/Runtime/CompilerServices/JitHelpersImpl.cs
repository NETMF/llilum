//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [ExtendClass(typeof(System.Runtime.CompilerServices.JitHelpers), NoConstructors=true)]
    internal static class JitHelpersImpl
    {
        static internal int UnsafeEnumCast<T>( T val ) where T : struct
        {
            return ToInt( val );
        }
        
        static internal long UnsafeEnumCastLong<T>( T val ) where T : struct
        {
            return ToLong( val );
        }
        
        [TS.GenerateUnsafeCast]
        static internal extern int ToInt<T>( T val );
        
        [TS.GenerateUnsafeCast]
        static internal extern long ToLong<T>( T val );
    }
}
