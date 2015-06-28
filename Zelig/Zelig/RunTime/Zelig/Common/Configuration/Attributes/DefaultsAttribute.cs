//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Configuration.Environment
{
    using System;
    using System.Collections.Generic;


    [AttributeUsage( AttributeTargets.Class |
                     AttributeTargets.Field , AllowMultiple=true )]
    public sealed class DefaultsAttribute : AbstractDefaultsAttribute
    {
        //
        // Constructor Methods
        //

        public DefaultsAttribute( string member ,
                                  bool   value  ) : base( member, value )
        {
        }

        public DefaultsAttribute( string member ,
                                  int   value  ) : base( member, value )
        {
        }

        public DefaultsAttribute( string member ,
                                  uint   value  ) : base( member, value )
        {
        }

        public DefaultsAttribute( string member ,
                                  long  value  ) : base( member, value )
        {
        }

        public DefaultsAttribute( string member ,
                                  ulong  value  ) : base( member, value )
        {
        }

        public DefaultsAttribute( string member ,
                                  string value  ) : base( member, value )
        {
        }

        public DefaultsAttribute( string member ,
                                  Type   value  ) : base( member, value )
        {
        }
    }
}
