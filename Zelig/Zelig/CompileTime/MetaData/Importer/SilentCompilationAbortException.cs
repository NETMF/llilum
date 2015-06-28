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
    public class SilentCompilationAbortException : AbstractMetaDataException
    {
        //
        // Constructor Methods
        //

        private SilentCompilationAbortException( String reason ) : base( reason )
        {
        }

        //
        // Helper Methods
        //

        public static SilentCompilationAbortException Create(        string   format ,
                                                              params object[] args   )
        {
            return new SilentCompilationAbortException( String.Format( format, args ) );
        }
    }
}
