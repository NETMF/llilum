//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    [ExtendClass(typeof(System.Runtime.CompilerServices.RuntimeHelpers), NoConstructors=true)]
    public static class RuntimeHelpersImpl
    {
        [TS.WellKnownMethod( "RuntimeHelpers_InitializeArray" )]
        public static extern void InitializeArray( Array              array     ,
                                                   RuntimeFieldHandle fldHandle );

        [Inline]
        [TS.WellKnownMethod( "RuntimeHelpers_InitializeArray2" )]
        private static void InitializeArray2( Array array ,
                                              Array value )
        {
            Array.Copy( value, array, value.Length );
        }

        //
        // Access Methods
        //

        public static extern int OffsetToStringData
        {
            [TS.WellKnownMethod( "RuntimeHelpers_get_OffsetToStringData")]
            get;
        }
    }
}
