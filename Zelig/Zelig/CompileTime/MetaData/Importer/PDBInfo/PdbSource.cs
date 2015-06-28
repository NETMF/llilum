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

    public class PdbSource
    {
        //
        // State
        //

        public uint   Index;
        public string Name;

        //
        // Constructor Methods
        //

        public PdbSource( uint   index ,
                          string name  )
        {
            this.Index = index;
            this.Name  = name;
        }
    }
}
