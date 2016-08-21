//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

//
//  Originally from the Microsoft Research Singularity code base.
//
namespace Microsoft.Zelig.MetaData.Importer.PdbInfo.Features
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Microsoft.Zelig.MetaData.Importer.PdbInfo.CodeView;

    public struct DbiSecCon
    {
        public short section;  // 0..1
        public short pad1;     // 2..3
        public int   offset;   // 4..7
        public int   size;     // 8..11
        public uint  flags;    // 12..15
        public short module;   // 16..17
        public short pad2;     // 18..19
        public uint  dataCrc;  // 20..23
        public uint  relocCrc; // 24..27

        public DbiSecCon( ArrayReader bits )
        {
            section  = bits.ReadInt16();
            pad1     = bits.ReadInt16();
            offset   = bits.ReadInt32();
            size     = bits.ReadInt32();
            flags    = bits.ReadUInt32();
            module   = bits.ReadInt16();
            pad2     = bits.ReadInt16();
            dataCrc  = bits.ReadUInt32();
            relocCrc = bits.ReadUInt32();

            //if(pad1 != 0 || pad2 != 0)
            //{
            //    throw new PdbException( "pad1 or pad2 != 0" );
            //}
        }
    }
}
