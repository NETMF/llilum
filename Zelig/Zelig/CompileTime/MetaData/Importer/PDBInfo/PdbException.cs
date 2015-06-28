//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
//  Originally from the Microsoft Research Singularity code base.
//
namespace Microsoft.Zelig.MetaData.Importer.PdbInfo
{
    using System;

    public class PdbException : Exception
    {
        //
        // Constructor Methods
        //

        public PdbException(        String   format ,
                             params object[] args   ) : base( String.Format( format, args ) )
        {
        }
    }
}
