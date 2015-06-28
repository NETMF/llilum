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

    public struct PdbLine
    {
        public uint   Offset;
        public uint   LineBegin;
        public uint   LineEnd;
        public ushort ColumnBegin;
        public ushort ColumnEnd;

        public PdbLine( uint   offset    ,
                        uint   lineBegin ,
                        uint   lineEnd   ,
                        ushort colBegin  ,
                        ushort colEnd    )
        {
            this.Offset      = offset;
            this.LineBegin   = lineBegin;
            this.LineEnd     = lineEnd;
            this.ColumnBegin = colBegin;
            this.ColumnEnd   = colEnd;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendFormat( "Off:{0} [{1}:{2} - {3}:{4}]", this.Offset, this.LineBegin, this.ColumnBegin, this.LineEnd, this.ColumnEnd );

            return sb.ToString();
        }
    }
}
