//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Class |
                     AttributeTargets.Field , AllowMultiple=true )]
    public sealed class RequiresAttribute : Attribute
    {
        //
        // State
        //

        public string Member;
        public object Value;


        //
        // Constructor Methods
        //

        public RequiresAttribute( string member ,
                                  uint   value  )
        {
            this.Member = member;
            this.Value  = value;
        }

        public RequiresAttribute( string member ,
                                  bool   value  )
        {
            this.Member = member;
            this.Value  = value;
        }

        public RequiresAttribute( string member ,
                                  string value  )
        {
            this.Member = member;
            this.Value  = value;
        }

        public RequiresAttribute( string member ,
                                  Type   value  )
        {
            this.Member = member;
            this.Value  = value;
        }
    }
}
