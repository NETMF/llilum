//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    public abstract class AbstractDefaultsAttribute : Attribute
    {
        //
        // State
        //

        public    readonly string Member;
        public    readonly object Value;
        protected          bool   m_merge;

        //
        // Constructor Methods
        //

        protected AbstractDefaultsAttribute( string member ,
                                             object value  )
        {
            this.Member = member;
            this.Value  = value;
        }

        //
        // Access Methods
        //

        public bool Merge
        {
            get
            {
                return m_merge;
            }
        }
    }
}
