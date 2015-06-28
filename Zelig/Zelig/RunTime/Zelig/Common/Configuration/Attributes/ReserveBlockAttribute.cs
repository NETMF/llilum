//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Field, AllowMultiple=true )]
    public sealed class ReserveBlockAttribute : Attribute
    {

        //
        // State
        //

        public uint   Offset;
        public uint   Size;
        public string Reason;

        //
        // Constructor Methods
        //

        public ReserveBlockAttribute( uint offset ,
                                      uint size   )
        {
            this.Offset = offset;
            this.Size   = size;
        }
    }
}
