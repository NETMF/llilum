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
    public abstract class AbstractMetaDataException : Exception
    {
        //
        // Constructor Methods
        //

        protected AbstractMetaDataException ( String reason ) : base( reason )
        {
        }
    }
}
