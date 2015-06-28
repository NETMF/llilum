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
    public class IllegalMetaDataFormatException : AbstractMetaDataException
    {
        //
        // Constructor Methods
        //

        private IllegalMetaDataFormatException( String reason ) : base( reason )
        {
        }

        //
        // Helper Methods
        //

        public static IllegalMetaDataFormatException Create(        string   format ,
                                                             params object[] args   )
        {
            return new IllegalMetaDataFormatException( String.Format( format, args ) );
        }
    }
}
