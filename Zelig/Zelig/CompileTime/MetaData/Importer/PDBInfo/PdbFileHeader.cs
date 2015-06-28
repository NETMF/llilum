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

    public class PdbFileHeader
    {
        //
        // State
        //

        public readonly byte[] magic;
        public readonly int    pageSize;
        public int             freePageMap;
        public int             pagesUsed;
        public int             directorySize;
        public readonly int    zero;
        public int             directoryRoot;

        //
        // Constructor Methods
        //

        public PdbFileHeader( int pageSize )
        {
            this.magic = new byte[32]
            {
                0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, // "Microsof"
                0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20, // "t C/C++ "
                0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30, // "MSF 7.00"
                0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00  // "^^^DS^^^"
            };
    
            this.pageSize = pageSize;
            this.zero     = 0;
        }

        public PdbFileHeader( ArrayReader image )
        {
            image.Rewind();

            this.magic         = image.ReadUInt8Array( 32 ); //   0..31
            this.pageSize      = image.ReadInt32();          //  32..35
            this.freePageMap   = image.ReadInt32();          //  36..39
            this.pagesUsed     = image.ReadInt32();          //  40..43
            this.directorySize = image.ReadInt32();          //  44..47
            this.zero          = image.ReadInt32();          //  48..51
            this.directoryRoot = image.ReadInt32();          //  52..55
        }

        //
        // Helper Methods
        //

        public void Emit( ArrayWriter writer )
        {
            writer.Write( magic         ); //   0..31
            writer.Write( pageSize      ); //  32..35
            writer.Write( freePageMap   ); //  36..39
            writer.Write( pagesUsed     ); //  40..43
            writer.Write( directorySize ); //  44..47
            writer.Write( zero          ); //  48..51
            writer.Write( directoryRoot ); //  52..55
        }
    }
}
