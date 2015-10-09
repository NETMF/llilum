//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [ExtendClass(typeof(System.Runtime.InteropServices.Marshal), NoConstructors=true)]
    public static class MarshalImpl
    {
        [TS.WellKnownMethod( "MarshalImpl_SizeOf__Object" )]
        public static int SizeOf( Object structure )
        {
            TS.VTable vt = TS.VTable.Get( structure );

            return (int)vt.BaseSize;
        }

        //[Inline]
        [TS.WellKnownMethod( "MarshalImpl_SizeOf__Type" )]
        public static int SizeOf( Type t )
        {
            TS.VTable vt = TS.VTable.GetFromType( t );

            return (int)vt.BaseSize;
        }
    }
}
