//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
//  Originally from the Microsoft Research Singularity code base.
//
namespace Microsoft.Zelig.MetaData.Importer.PdbInfo
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData.Importer.PdbInfo.CodeView;
    using Microsoft.Zelig.MetaData.Importer.PdbInfo.Features;

    public class PdbLines
    {
        //
        // State
        //

        public PdbSource File;
        public PdbLine[] Lines;

        //
        // Constructor Methods
        //

        public PdbLines( PdbSource file  ,
                         uint      count )
        {
            this.File  = file;
            this.Lines = new PdbLine[count];
        }
    }
}
