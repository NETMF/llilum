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

    public class PdbSlot
    {
        //
        // State
        //

        public uint   Slot;
        public string Name;
        public ushort Flags;
        public uint   Segment;
        public uint   Address;

        //
        // Constructor Methods
        //

        internal PdbSlot(     ArrayReader bits   ,
                          out uint        typind )
        {
            AttrSlotSym slot;

            slot.index  = bits.ReadUInt32();
            slot.typind = bits.ReadUInt32();
            slot.offCod = bits.ReadUInt32();
            slot.segCod = bits.ReadUInt16();
            slot.flags  = bits.ReadUInt16();
            slot.name   = bits.ReadZeroTerminatedUTF8String();

            this.Slot    = slot.index;
            this.Name    = slot.name;
            this.Flags   = slot.flags;
            this.Segment = slot.segCod;
            this.Address = slot.offCod;

            typind = slot.typind;
        }
    }
}
