//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    //
    // This class is used internally to hold all the storage for static fields and other global entries.
    //
    [WellKnownType( "Microsoft_Zelig_Runtime_TypeSystem_GlobalRoot" )]
    public class GlobalRoot
    {
        //
        // Constructor Methods
        //

        internal GlobalRoot()
        {
        }

        //
        // Access Methods
        //

        public static extern GlobalRoot Instance
        {
            [SingletonFactory()]
            [MethodImpl( MethodImplOptions.InternalCall )]
            get;
        }
    }
}
