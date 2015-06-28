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

    public enum DbgType
    {
        dbgtypeFPO            = 0,
        dbgtypeException      = 1,
        dbgtypeFixup          = 2,
        dbgtypeOmapToSrc      = 3,
        dbgtypeOmapFromSrc    = 4,
        dbgtypeSectionHdr     = 5,
        dbgtypeTokenRidMap    = 6,
        dbgtypeXdata          = 7,
        dbgtypePdata          = 8,
        dbgtypeNewFPO         = 9,
        dbgtypeSectionHdrOrig = 10,

        dbgtypeFirst          = dbgtypeFPO,
    }

    public struct DbiHeader
    {
        public int    sig;           // 0..3
        public int    ver;           // 4..7
        public int    age;           // 8..11
        public short  gssymStream;   // 12..13
        public ushort vers;          // 14..15
        public short  pssymStream;   // 16..17
        public ushort pdbver;        // 18..19
        public short  symrecStream;  // 20..21
        public ushort pdbver2;       // 22..23
        public int    gpmodiSize;    // 24..27
        public int    secconSize;    // 28..31
        public int    secmapSize;    // 32..35
        public int    filinfSize;    // 36..39
        public int    tsmapSize;     // 40..43
        public int    mfcIndex;      // 44..47
        public int    dbghdrSize;    // 48..51
        public int    ecinfoSize;    // 52..55
        public ushort flags;         // 56..57
        public ushort machine;       // 58..59
        public int    reserved;      // 60..63

        public DbiHeader( ArrayReader bits )
        {
            sig          = bits.ReadInt32();
            ver          = bits.ReadInt32();
            age          = bits.ReadInt32();
            gssymStream  = bits.ReadInt16();
            vers         = bits.ReadUInt16();
            pssymStream  = bits.ReadInt16();
            pdbver       = bits.ReadUInt16();
            symrecStream = bits.ReadInt16();
            pdbver2      = bits.ReadUInt16();
            gpmodiSize   = bits.ReadInt32();
            secconSize   = bits.ReadInt32();
            secmapSize   = bits.ReadInt32();
            filinfSize   = bits.ReadInt32();
            tsmapSize    = bits.ReadInt32();
            mfcIndex     = bits.ReadInt32();
            dbghdrSize   = bits.ReadInt32();
            ecinfoSize   = bits.ReadInt32();
            flags        = bits.ReadUInt16();
            machine      = bits.ReadUInt16();
            reserved     = bits.ReadInt32();
        }
    }
}
