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

    public class PdbScope
    {
        //
        // State
        //

        public PdbSlot[]  Slots;
        public PdbScope[] Scopes;

        public uint       Segment;
        public uint       Address;
        public uint       Length;

        //
        // Constructor Methods
        //

        internal PdbScope(     BlockSym32  block  ,
                               ArrayReader bits   ,
                           out uint        typind )
        {
            this.Segment = block.seg;
            this.Address = block.off;
            this.Length  = block.len;

            typind = 0;

            int scopeCount;
            int slotCount;

            PdbFunction.CountScopesAndSlots( bits, block.end, out scopeCount, out slotCount );

            Scopes = new PdbScope[scopeCount];
            Slots  = new PdbSlot [slotCount];

            int scope = 0;
            int slot  = 0;

            while(bits.Position < block.end)
            {
                ushort siz = bits.ReadUInt16();

                ArrayReader subBits = bits.CreateSubsetAndAdvance( siz );

                SYM rec = (SYM)subBits.ReadUInt16();
                switch(rec)
                {
                    case SYM.S_BLOCK32:
                        {
                            BlockSym32 sub = new BlockSym32();

                            sub.parent = subBits.ReadUInt32();
                            sub.end    = subBits.ReadUInt32();
                            sub.len    = subBits.ReadUInt32();
                            sub.off    = subBits.ReadUInt32();
                            sub.seg    = subBits.ReadUInt16();
                            sub.name   = subBits.ReadZeroTerminatedUTF8String();

                            Scopes[scope++] = new PdbScope( sub, bits, out typind );

                            bits.Position = (int)sub.end;
                            break;
                        }

                    case SYM.S_MANSLOT:
                        Slots[slot++] = new PdbSlot( subBits, out typind );
                        break;

                    case SYM.S_END:
                    case SYM.S_UNAMESPACE:
                    case SYM.S_MANCONSTANT:
                        break;

                    default:
                        throw new PdbException( "Unknown SYM in scope {0}", rec );
                    // bits.Position = stop;
                }
            }

            if(bits.Position != block.end)
            {
                throw new PdbException( "Not at S_END" );
            }

            ushort esiz = bits.ReadUInt16();
            ushort erec = bits.ReadUInt16();

            if(erec != (ushort)SYM.S_END)
            {
                throw new PdbException( "Missing S_END" );
            }
        }

        internal void CollectSlots( List<PdbSlot> list )
        {
            foreach(PdbSlot slot in Slots)
            {
                list.Add( slot );
            }

            foreach(PdbScope scope in Scopes)
            {
                scope.CollectSlots( list );
            }
        }
    }
}
