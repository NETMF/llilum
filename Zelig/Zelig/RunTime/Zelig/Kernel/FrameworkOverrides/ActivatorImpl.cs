//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Activator), NoConstructors=true)]
    public class ActivatorImpl
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

        [TS.WellKnownMethod( "ActivatorImpl_CreateInstanceInner" )]
        public static extern object CreateInstanceInner( Type t );

        //--//

        //
        // Access Methods
        //
    }
}
