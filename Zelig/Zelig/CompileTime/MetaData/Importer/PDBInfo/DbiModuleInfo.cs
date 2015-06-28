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

    public class DbiModuleInfo
    {
        //
        // State
        //

        public int       opened;                 //  0..3
        public DbiSecCon section;                //  4..31
        public ushort    flags;                  // 32..33
        public short     stream;                 // 34..35
        public int       cbSyms;                 // 36..39
        public int       cbOldLines;             // 40..43
        public int       cbLines;                // 44..57
        public short     files;                  // 48..49
        public short     pad1;                   // 50..51
        public uint      offsets;
        public int       niSource;
        public int       niCompiler;
        public string    moduleName;
        public string    objectName;

        //
        // Constructor Methods
        //

        public DbiModuleInfo( ArrayReader bits )
        {
            opened     = bits.ReadInt32();
            section    = new DbiSecCon( bits );
            flags      = bits.ReadUInt16();
            stream     = bits.ReadInt16();
            cbSyms     = bits.ReadInt32();
            cbOldLines = bits.ReadInt32();
            cbLines    = bits.ReadInt32();
            files      = bits.ReadInt16();
            pad1       = bits.ReadInt16();
            offsets    = bits.ReadUInt32();
            niSource   = bits.ReadInt32();
            niCompiler = bits.ReadInt32();
            moduleName = bits.ReadZeroTerminatedUTF8String();
            objectName = bits.ReadZeroTerminatedUTF8String();

            bits.AlignAbsolute( 4 );

            if(opened != 0 || pad1 != 0)
            {
                throw new PdbException( "opened is {0}, not 0", opened );
            }
        }
    }
}
