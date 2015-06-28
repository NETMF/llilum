//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Class |
                     AttributeTargets.Field , AllowMultiple=true )]
    public sealed class EnumDefaultsAttribute : AbstractDefaultsAttribute
    {
        //
        // Constructor Methods
        //

        public EnumDefaultsAttribute( string                   member ,
                                      Runtime.MemoryAttributes value  ) : base( member, value )
        {
        }
    }
}
