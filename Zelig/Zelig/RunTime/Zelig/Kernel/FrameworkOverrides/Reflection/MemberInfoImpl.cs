//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;


    [ExtendClass(typeof(System.Reflection.MemberInfo), NoConstructors=true)]
    public abstract class MemberInfoImpl
    {
        //
        // State
        //

        //
        // Constructor Methods
        //

        //
        // Helper Methods
        //

        //
        // Access Methods
        //

        public abstract String Name
        {
            get;
        }
    
        public abstract Type DeclaringType
        {
            get;
        }
    }
}
