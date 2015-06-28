//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
// Originally based on the Bartok code base.
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Microsoft.Zelig.MetaData.Importer
{
    public class UnresolvedExternalReferenceException : AbstractMetaDataException
    {
        //
        // State
        //

        public readonly object Context;

        //
        // Constructor Methods
        //

        private UnresolvedExternalReferenceException( object context ,
                                                      String reason  ) : base( reason )
        {
            this.Context = context;
        }

        //
        // Helper Methods
        //

        public static UnresolvedExternalReferenceException Create(        object   context ,
                                                                          string   format  ,
                                                                   params object[] args    )
        {
            return new UnresolvedExternalReferenceException( context, String.Format( format, args ) );
        }
    }
}
